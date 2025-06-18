using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeAllCore.Models;
using BeAllCore;
using BeAllCore.Objects;
using System.Threading;
using BeAllCore.Forms;
using System.Text.RegularExpressions;

namespace BeAllCore.Controllers
{
    [Route("/[controller]")]
    public class gatesController : Controller
    {
        // POST /gates/getgatesfortoken
        [HttpPost("/[controller]/getgatesfortoken", Name = nameof(GetGatesForToken))]
        public async Task<IActionResult> GetGatesForToken([FromBody]GetGatesForTokenForm InputForm, CancellationToken ct)
        {
            string token;
            //validate
            if (InputForm.Token == null)
            {
                return ApiResponse.Error(Request, "Toekn missing", "function must input Toekn", ApiError.Err_MissingParameter);
            }
            else
            {
                token = InputForm.Token;
            }

            //0. get the user
            UserObject user = await DataBase.VerifyUserToekn(token);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "No such token in the system", "No such token in the system, logout the user and then login to get a new toekn", ApiError.Err_NoToken);
            }

            //1. get the user gates
            List<GateUserJoinTableObject> GetUserGatesTable = await DataBase.GetUserGatesTable(user);
            GetUserGatesTable = GetUserGatesTable.OrderBy(o => o.GateID).ToList();


            foreach (var g in GetUserGatesTable)
            {
                if (g.gate == null)
                {
                    //DataBase.InsertLog("guj " + g.ObjectId.ToString() + " dont have gate " + g._p_gate.ToString(), "gate null error", "gate null error");
                }
                else
                {
                    if (g._p_gate == null)
                    {
                        //DataBase.InsertLog("guj " + g.ObjectId.ToString() + " have _p_gate null ", "gate null error", "gate null error");
                    }
                    else
                    {
                        if (g._p_gate == "")
                        {
                            //DataBase.InsertLog("guj " + g.ObjectId.ToString() + " have _p_gate empty ", "gate null error", "gate null error");
                        }
                    }
                }
            }

            //2. success
            return ApiResponse.Success(Request, GetUserGatesTable);
        }


        // function to used by the mobile app
        // POST /gates/getgatesfortoken
        [HttpPost("/[controller]/getmgatesfortoken", Name = nameof(GetmGatesForToken))]
        public async Task<IActionResult> GetmGatesForToken([FromBody]GetGatesForTokenForm InputForm, CancellationToken ct)
        {
            string token;
            //validate
            if (InputForm.Token == null)
            {
                return ApiResponse.Error(Request, "Toekn missing", "function must input Toekn", ApiError.Err_MissingParameter);
            }
            else
            {
                token = InputForm.Token;
            }

            //0. get the user
            UserObject user = await DataBase.VerifyUserToekn(token);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "No such token in the system", "No such token in the system, logout the user and then login to get a new toekn", ApiError.Err_NoToken);
            }

            //1. get the user gates
            List<GateUserJoinTableObject> GetUserGatesTable = await DataBase.GetUserGatesTable(user);
            List<mGateUserObject> mGates = new List<mGateUserObject>();
            string favoidents = "";


            GetUserGatesTable = GetUserGatesTable.OrderBy(o => o.GateID).ToList();

            for (int i = 0; i < GetUserGatesTable.Count; i++)
            {
                mGateUserObject gate = new mGateUserObject();
                if (GetUserGatesTable[i].gate != null)
                {
                    gate.objectId = GetUserGatesTable[i].gate.ObjectId;
                    gate.address = GetUserGatesTable[i].gate.address;
                    gate.updatedAt = GetUserGatesTable[i].updatedAt;
                    gate.identifier = GetUserGatesTable[i].gate.identifier;
                    gate.name = GetUserGatesTable[i].name;
                    gate.DueDate = GetUserGatesTable[i].DueDate;
                    gate.ProductionCode = GetUserGatesTable[i].ProductionCode;


                    if (user.FavoriteGatesObjectID.Contains(gate.objectId))
                    {
                        gate.isFavorite = true;
                        favoidents += gate.identifier + ",";
                    }
                    else
                        gate.isFavorite = false;

                    // split GateID to gate info IL-TLV-Alon-L4-PO-87
                    try
                    {
                        string[] spl = GetUserGatesTable[i].gate.GateID.Split("-");
                        if (spl.Length > 5)
                        {
                            if (spl[0] == "IL")
                                gate.Loc_Country = "Israel";
                            else
                                gate.Loc_Country = spl[0];

                            if (spl[1] == "TLV")
                                gate.Loc_City = "Tel Aviv";
                            else if (spl[1] == "GVT")
                                gate.Loc_City = "Givatayim";
                            else
                                gate.Loc_Country = spl[1];

                            gate.Loc_Building = spl[2];

                            switch (spl[2])
                            {
                                case "ShaharTower":
                                    gate.address = "HaShahar Tower";
                                    gate.Loc_Building = "HaShahar";
                                    break;

                                case "Arlozorov":
                                    gate.address = "Arlozorov Tower";
                                    gate.Loc_Building = "Arlozorov";
                                    break;

                                case "Alon":
                                    gate.address = "Alon Tower North";
                                    gate.Loc_Building = "Alon";
                                    break;

                                case "YehudaHalevi":
                                    gate.address = "Yehuda Halevi 48";
                                    gate.Loc_Building = "Yehuda Halevi";
                                    break;

                                case "Alon2":
                                    gate.address = "Alon Tower South";
                                    gate.Loc_Building = "Alon 2";
                                    break;

                                default:
                                    gate.Loc_Building = spl[2];
                                    break;
                            }

                            gate.Loc_Level = spl[3].Replace("L", "");

                            gate.Loc_Room = spl[5];

                            short number;
                            bool result = Int16.TryParse(spl[5], out number);
                            if (result == true)
                            {

                                gate.Loc_Range = (Convert.ToInt16((Convert.ToInt16(spl[5]) / 10)) * 10).ToString() + "-" + ((Convert.ToInt16((Convert.ToInt16(spl[5]) / 10)) * 10) + 9).ToString();
                            }
                            else
                            {
                                gate.Loc_Range = spl[5];
                            }
                        }
                    }
                    catch
                    {

                    }
                    mGates.Add(gate);
                }
            }

            if (favoidents.Length > 1)
            {
                favoidents = favoidents.Remove(favoidents.Length - 1, 1);
            }

            // daniel wanted to get the list from within an object
            mGateUserListObject a = new mGateUserListObject();
            a.ListOfGates = mGates;
            a.UserData = new UserMobileDataObject();
            a.UserData.username = user.username;
            a.UserData.email = user.email;
            a.UserData.name = user.name;
            a.UserData.FavoriteGatesObjectID = user.FavoriteGatesObjectID;
            a.UserData.FavoriteGatesIdentifiers = favoidents;

            //2. success
            return ApiResponse.Success(Request, a);
        }

        // function to used by the mobile app
        // POST /gates/getgatesfortoken
        [HttpPost("/[controller]/getmgatesforuser", Name = nameof(GetmGatesForUser))]
        public async Task<IActionResult> GetmGatesForUser([FromBody]GetGatesForUserForm InputForm, CancellationToken ct)
        {
            string token;
            //validate
            if (InputForm.Token == null)
            {
                return ApiResponse.Error(Request, "Toekn missing", "function must input Toekn", ApiError.Err_MissingParameter);
            }
            else
            {
                token = InputForm.Token;
            }

            //0. get the user
            UserObject user = await DataBase.VerifyUserToekn(token);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "No such token in the system", "No such token in the system, logout the user and then login to get a new toekn", ApiError.Err_NoToken);
            }

            //1. get the requested user 
            UserObject req_user = await DataBase.GetUserByUserObjID(InputForm.UserObjId);

            //return not found if user not in the system
            if (req_user == null)
            {
                return ApiResponse.Error(Request, "No such user in the system", "No such user in the system", ApiError.Err_UserNotFound);
            }

            //1. get the user gates
            List<GateUserJoinTableObject> GetUserGatesTable = await DataBase.GetUserGatesTable(req_user);
            List<mGateUserObject> mGates = new List<mGateUserObject>();
            string favoidents = "";


            GetUserGatesTable = GetUserGatesTable.OrderBy(o => o.GateID).ToList();

            for (int i = 0; i < GetUserGatesTable.Count; i++)
            {
                mGateUserObject gate = new mGateUserObject();
                if (GetUserGatesTable[i].gate != null)
                {
                    gate.objectId = GetUserGatesTable[i].gate.ObjectId;
                    gate.address = GetUserGatesTable[i].gate.address;
                    gate.updatedAt = GetUserGatesTable[i].updatedAt;
                    gate.identifier = GetUserGatesTable[i].gate.identifier;
                    gate.name = GetUserGatesTable[i].name;
                    gate.DueDate = GetUserGatesTable[i].DueDate;
                    gate.ProductionCode = GetUserGatesTable[i].ProductionCode;

                    if (req_user.FavoriteGatesObjectID != null)
                    {
                        if (req_user.FavoriteGatesObjectID.Contains(gate.objectId))
                        {
                            gate.isFavorite = true;
                            favoidents += gate.identifier + ",";
                        }
                        else
                            gate.isFavorite = false;
                    }
                    else
                        gate.isFavorite = false;

                    // split GateID to gate info IL-TLV-Alon-L4-PO-87
                    try
                    {
                        string[] spl = GetUserGatesTable[i].gate.GateID.Split("-");
                        if (spl.Length == 6)
                        {
                            if (spl[0] == "IL")
                                gate.Loc_Country = "Israel";
                            else
                                gate.Loc_Country = spl[0];

                            if (spl[1] == "TLV")
                                gate.Loc_City = "Tel Aviv";
                            else if (spl[1] == "GVT")
                                gate.Loc_City = "Givatayim";
                            else
                                gate.Loc_Country = spl[1];

                            switch (spl[2])
                            {
                                case "ShaharTower":
                                    gate.address = "HaShahar Tower";
                                    gate.Loc_Building = "HaShahar";
                                    break;

                                case "Arlozorov":
                                    gate.address = "Arlozorov Tower";
                                    gate.Loc_Building = "Arlozorov";
                                    break;

                                case "Alon":
                                    gate.address = "Alon Tower North";
                                    gate.Loc_Building = "Alon";
                                    break;

                                case "YehudaHalevi":
                                    gate.address = "Yehuda Halevi 48";
                                    gate.Loc_Building = "Yehuda Halevi";
                                    break;

                                case "Alon2":
                                    gate.address = "Alon Tower South";
                                    gate.Loc_Building = "Alon 2";
                                    break;

                                default:
                                    gate.Loc_Building = spl[2];
                                    break;
                            }

                            gate.Loc_Level = spl[3].Replace("L", "");

                            gate.Loc_Room = spl[5];

                            short number;
                            bool result = Int16.TryParse(spl[5], out number);
                            if (result == true)
                            {
                                gate.Loc_Range = (Convert.ToInt16((Convert.ToInt16(spl[5]) / 10)) * 10).ToString() + "-" + ((Convert.ToInt16((Convert.ToInt16(spl[5]) / 10)) * 10) + 9).ToString();
                            }
                            else
                            {
                                gate.Loc_Range = spl[5];
                            }
                        }
                    }
                    catch
                    {

                    }
                    mGates.Add(gate);
                }
            }

            if (favoidents.Length > 1)
            {
                favoidents = favoidents.Remove(favoidents.Length - 1, 1);
            }

            // daniel wanted to get the list from within an object
            mGateUserListObject a = new mGateUserListObject();
            a.ListOfGates = mGates;
            a.UserData = new UserMobileDataObject();
            a.UserData.username = user.username;
            a.UserData.email = req_user.email;
            a.UserData.name = req_user.name;
            a.UserData.FavoriteGatesObjectID = req_user.FavoriteGatesObjectID;
            a.UserData.FavoriteGatesIdentifiers = favoidents;

            //2. success
            return ApiResponse.Success(Request, a);
        }


        // get all users asign to specific gates
        // POST /gates/getusersforgate
        [HttpPost("/[controller]/GetUsersForGate", Name = nameof(GetUsersForGate))]
        public async Task<IActionResult> GetUsersForGate([FromBody]GetUsersForGateForm InputForm, CancellationToken ct)
        {
            string token;
            //validate
            if (InputForm.Token == null)
            {
                return ApiResponse.Error(Request, "Toekn missing", "function must input Toekn", ApiError.Err_MissingParameter);
            }
            else
            {
                token = InputForm.Token;
            }

            if (InputForm.GateObjectId == null)
            {
                return ApiResponse.Error(Request, "GateObjectId missing", "function must input GateObjectId", ApiError.Err_MissingParameter);
            }

            //0. get the user
            UserObject user = await DataBase.VerifyUserToekn(token);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "No such token in the system", "No such token in the system, logout the user and then login to get a new toekn", ApiError.Err_NoToken);
            }

            //1. get gate ID by it's identifier
            GateObject gate = await DataBase.GetGateByID(InputForm.GateObjectId);

            if (gate == null)
            {
                return ApiResponse.Error(Request, "No identifier", "No such GateObjectId" + InputForm.GateObjectId + " in the system", ApiError.Err_GateIdentifierNotFound);
            }

            //2. get all users_id for that gate
            List<UserDataObject> users = await DataBase.GetAllUserForGate(gate.ObjectId);

            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].ObjectId == user.ObjectId)
                {
                    //3. success
                    return ApiResponse.Success(Request, users);
                }
            }

            // error
            return ApiResponse.Error(Request, "User not allowed", "user dont have access to this gate", ApiError.Err_UserDontHaveAccess);
        }

        /// <summary>
        /// Set the user favorite by using the gate objectID
        /// </summary>
        /// <param name="InputForm"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        // POST /gates/setfavoritesObjIDfortoken
        [HttpPost("/[controller]/setfavoritesObjIDfortoken", Name = nameof(SetFavoritesObjIDForToken))]
        public async Task<IActionResult> SetFavoritesObjIDForToken([FromBody]SetFavoritesObjIDForTokenForm InputForm, CancellationToken ct)
        {
            string token;
            //validate
            if (InputForm.Token == null)
            {
                return ApiResponse.Error(Request, "Toekn missing", "function must input Toekn", ApiError.Err_MissingParameter);
            }
            else
            {
                token = InputForm.Token;
            }

            //0. get the user
            UserObject user = await DataBase.VerifyUserToekn(token);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "No such token in the system", "No such token in the system, logout the user and then login to get a new toekn", ApiError.Err_NoToken);
            }

            //1. save the new favorites value
            bool ans = await DataBase.SetFavoritesForToken(InputForm.Favorites, user);

            //2. success
            return ApiResponse.Success(Request);
        }


        /// <summary>
        /// Set the user favorite by using the gate Identifier
        /// </summary>
        /// <param name="InputForm"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        // POST /gates/setfavoritesIdentfortoken
        [HttpPost("/[controller]/setfavoritesIdentfortoken", Name = nameof(setfavoritesIdentfortoken))]
        public async Task<IActionResult> setfavoritesIdentfortoken([FromBody]SetFavoritesObjIDForTokenForm InputForm, CancellationToken ct)
        {
            string token;
            //validate
            if (InputForm.Token == null)
            {
                return ApiResponse.Error(Request, "Toekn missing", "function must input Toekn", ApiError.Err_MissingParameter);
            }
            else
            {
                token = InputForm.Token;
            }

            if (InputForm.Favorites == null)
            {
                return ApiResponse.Error(Request, "Favorites missing", "function must input Favorites", ApiError.Err_MissingParameter);
            }

            //0. get the user
            UserObject user = await DataBase.VerifyUserToekn(token);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "No such token in the system", "No such token in the system, logout the user and then login to get a new toekn", ApiError.Err_NoToken);
            }


            //1. get all the identifier objID
            string[] favs = InputForm.Favorites.Split(",");
            string Favorites = "";

            if (InputForm.Favorites.Contains(","))
            {
                foreach (string fav in favs)
                {
                    GateObject g = await DataBase.GetGateByIdentifier(fav);
                    if (g != null)
                        Favorites += g.ObjectId + ",";
                }

                // remove last ","
                if (Favorites.Length > 1)
                {
                    Favorites = Favorites.Remove(Favorites.Length - 1, 1);
                }
            }
            else if (InputForm.Favorites != "")
            {
                GateObject g = await DataBase.GetGateByIdentifier(InputForm.Favorites);
                if (g != null)
                    Favorites = g.ObjectId;
            }
            // if Favorites=="", then Favorites will be ""

            //1. save the new favorites value
            bool ans = await DataBase.SetFavoritesForToken(Favorites, user);

            //2. success
            return ApiResponse.Success(Request);
        }


#warning "need to fix Parse compatibility issue"
        // POST /gates/addusertogate
        [HttpPost("/[controller]/AddUserToGate", Name = nameof(AddUserToGate))]
        public IActionResult AddUserToGate([FromBody]AddUserToGateForm InputForm, CancellationToken ct)
        {
            /*
            string token;
            string NumberToAdd;
            string GateObjectID;
            //validate
            if (InputForm.Token == null)
            {
                return ApiResponse.Error(Request, "Toekn missing", "function must input Toekn", ApiError.Err_MissingParameter);
            }
            else
            {
                token = InputForm.Token;
            }
            //validate
            if (InputForm.NumberToAdd == null)
            {
                return ApiResponse.Error(Request, "NumberToAdd missing", "function must input NumberToAdd", ApiError.Err_MissingParameter);
            }
            else
            {
                NumberToAdd = InputForm.NumberToAdd;
            }
            //validate
            if (InputForm.GateObjectID == null)
            {
                return ApiResponse.Error(Request, "GateObjectID missing", "function must input GateObjectID", ApiError.Err_MissingParameter);
            }
            else
            {
                GateObjectID = InputForm.GateObjectID;
            }

            //0. get the user
            UserObject user = await DataBase.VerifyUserToekn(token);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "No such token in the system", "No such token in the system, logout the user and then login to get a new toekn", ApiError.Err_NoToken);
            }

            // 1. get the user to add
            UserObject user2add = await DataBase.GetUserByUserName(NumberToAdd);
            if (user2add == null)
            {
                return ApiResponse.Error(Request, "No such number in the system", "user phone number to add to gate is not in the system", ApiError.Err_UserToAddNotListed);
            }


            //2. make sure user have the specified gate
            GateUserJoinTableObject isUserHaveAcc = await DataBase.IsGateObjIDrelatedToUser(user, GateObjectID);
            if (isUserHaveAcc == null)
            {
                return ApiResponse.Error(Request, "User dont have access", "User dont have access to this gate", ApiError.Err_UserDontHaveAccess);
            }


            //2. make user user2add dont have the specified gate
            GateUserJoinTableObject isuser2addHaveAcc = await DataBase.IsGateObjIDrelatedToUser(user2add, GateObjectID);
            if (isuser2addHaveAcc != null)
            {
                return ApiResponse.Error(Request, "User that you want to add already have access", "User that you want to add already have access to this gate", ApiError.Err_UserAlreadyHaveAccess);
            }

            isUserHaveAcc.user = user2add.ObjectId;

            //3. get the gate to get gate real name
            GateObject gate = await DataBase.GetGateByID(isUserHaveAcc._p_gate);

            if (gate == null)
            {
                return ApiResponse.Error(Request, "gate was not found in the system", "please contact administrator, server issue", ApiError.Err_GateIdentifierNotFound);
            }

            isUserHaveAcc.name = gate.name;

            //4. add this user
            bool res = await DataBase.GiverUserAccessToGate(isUserHaveAcc);
            */
            //5. success
            return ApiResponse.Success(Request, "user was not added successfully because of parse issues");
        }


        // POST /gates/revokeuserfromgate
        [HttpPost("/[controller]/RevokeUserFromGate", Name = nameof(RevokeUserFromGate))]
        public async Task<IActionResult> RevokeUserFromGate([FromBody]RevokeUserFromGateForm InputForm, CancellationToken ct)
        {
            string token;
            string UserObjIDToRemove;
            string GateObjectID;
            //validate
            if (InputForm.Token == null)
            {
                return ApiResponse.Error(Request, "Toekn missing", "function must input Toekn", ApiError.Err_MissingParameter);
            }
            else
            {
                token = InputForm.Token;
            }
            //validate
            if (InputForm.UserObjIDToRemove == null)
            {
                return ApiResponse.Error(Request, "NumberToAdd missing", "function must input NumberToAdd", ApiError.Err_MissingParameter);
            }
            else
            {
                UserObjIDToRemove = InputForm.UserObjIDToRemove;
            }
            //validate
            if (InputForm.GateObjectID == null)
            {
                return ApiResponse.Error(Request, "GateObjectID missing", "function must input GateObjectID", ApiError.Err_MissingParameter);
            }
            else
            {
                GateObjectID = InputForm.GateObjectID;
            }

            //0. get the user
            UserObject user = await DataBase.VerifyUserToekn(token);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "No such token in the system", "No such token in the system, logout the user and then login to get a new toekn", ApiError.Err_NoToken);
            }

            // 1. get the user to revoke
            UserObject user2remove = await DataBase.GetUserByUserObjID(UserObjIDToRemove);
            if (user2remove == null)
            {
                return ApiResponse.Error(Request, "No such number in the system", "user phone number to add to gate is not in the system", ApiError.Err_UserToAddNotListed);
            }


            //2. make sure logged user have the specified gate
            GateUserJoinTableObject isUserHaveAcc = await DataBase.IsGateObjIDrelatedToUser(user, GateObjectID);
            if (isUserHaveAcc == null)
            {
                return ApiResponse.Error(Request, "User dont have access", "User dont have access to this gate", ApiError.Err_UserDontHaveAccess);
            }


            //2. make user user2remove have the specified gate
            GateUserJoinTableObject isuser2removeHaveAcc = await DataBase.IsGateObjIDrelatedToUser(user2remove, GateObjectID);
            if (isuser2removeHaveAcc == null)
            {
                return ApiResponse.Error(Request, "User that you want to remove dont have access", "User that you want to dont dont have access to this gate", ApiError.Err_UserAlreadyHaveAccess);
            }

            //4. add this user
            bool res = await DataBase.RemoveUserAccessFromGate(isuser2removeHaveAcc);

            //5. success
            return ApiResponse.Success(Request, "user revoked access");
        }


        // codes removed for safty.
        // used only for DEV

        // POST /gates/RemoveGates
        [HttpPost("/[controller]/RemoveGates", Name = nameof(RemoveGates))]
        public async Task<IActionResult> RemoveGates([FromBody]RemoveGatesForm InputForm, CancellationToken ct)
        {
            // shahar: ["OC400081","OC400082","OC400083","OC400084","OC400085","OC400086","OC400087","OC400088","OC400089","OC40008A","OC4008B","OC40008C"]
            // Alon ["OC400100","OC400101","OC400102","OC400103","OC400104","OC400105","OC400106","OC400200","OC400201","OC400202","OC400203","OC400204","OC400205","OC400300","OC400301","OC400302","OC400303","OC400304","OC400305","OC400400","OC400401","OC400402","OC400403","OC400404","OC400405","OC400500","OC400501","OC400502","OC400503","OC400504","OC400505","OC400506","OC400600","OC400601","OC400602","OC400603","OC400604","OC400605"]


            var gates = await DataBase.GetGatesIDByIdentifier(InputForm.gatesIdentifiers);
            var user_gates = await DataBase.GetUserGatesByGateId(gates);

            var users_gates_delete_count = await DataBase.RemoveUserAccessFromGateByUserGateID(user_gates);
            var gates_delete_count = await DataBase.RemoveGatesByGatesID(gates);

            //5. success
            return ApiResponse.Success(Request, "requested to delete: " + InputForm.gatesIdentifiers.Count + " And found " + gates.Count + " Gates, " + user_gates.Count + " Had access to this gates, " + users_gates_delete_count.ToString() + "access revoked And " + gates_delete_count.ToString() + " gates deleted");
        }


        //'Shahar Tower - Tel Aviv'
        //'Alon Tower North - Tel Aviv'
        //'Alon Tower South - Tel Aviv'
        //'Arlozerov Tower - Tel Aviv'
        //'Sahar Tower - Tel Aviv'
        // POST /gates/replaceAddress
        [HttpPost("/[controller]/replaceAddress", Name = nameof(replaceAddress))]
        public async Task<IActionResult> replaceAddress([FromBody]replaceAddressForm InputForm, CancellationToken ct)
        {
            //var gates = await DataBase.GetGatesIDByAddress(InputForm.from);
            var v = DataBase.ReplaceAddressInGate(InputForm.from, InputForm.to);

            //5. success
            return ApiResponse.Success(Request, "updated: " + v.ToString() + " gates");
        }

    }

}
