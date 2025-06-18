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
using System.Security.Cryptography;


namespace BeAllCore.Controllers
{
    [Route("/[controller]")]

    public class SalesForceController : Controller
    {

        private (bool IsValid, IActionResult? ErrorResult, string? Token) ValidateTokenFromHeader()
        {
            if (!Request.Headers.TryGetValue("Token", out var authHeader))
            {
                return (false, ApiResponse.Error(Request, "token missing", "Authorization header is required", ApiError.Err_MissingParameter), null);
            }

            var token = authHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();

            if (string.IsNullOrEmpty(token))
            {
                return (false, ApiResponse.Error(Request, "token missing", "Authorization header is empty", ApiError.Err_MissingParameter), null);
            }

            if (token != "w54ERY^w54dtyj4W%$FQ#454q%^5h67gE$QTC") // Replace this with your actual validation logic
            {
                return (false, ApiResponse.Error(Request, "token wrong", "", ApiError.Err_MissingParameter), token);
            }

            return (true, null, token);
        }



        // get all gates related to specific user
        // POST /SalesForce/GetGatesForUser
        [HttpPost("/[controller]/sendSms", Name = nameof(sendSms))]
        public async Task<IActionResult> sendSms([FromBody] SendSMSForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            string str = SmsClass.send_upSendSmsFrom(InputForm.message, InputForm.phone, InputForm.from);
            return ApiResponse.Success(Request, str);
        }

        // get all gates related to specific user
        // POST /SalesForce/GetGatesForUser
        [HttpPost("/[controller]/GetGatesForUser", Name = nameof(GetGatesForUser))]
        public async Task<IActionResult> GetGatesForUser([FromBody] GetUserPinForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.user_phone_number == null)
            {
                return ApiResponse.Error(Request, "user_phone_number missing", "function must input user_phone_number", ApiError.Err_MissingParameter);
            }

            //1. get gate ID by it's identifier
            var user =await DataBase.GetUserByUserName(InputForm.user_phone_number);
            if (user == null)
            {
                return ApiResponse.Error(Request, "no such user in the system", "no such user in the system", ApiError.Err_UserNotFound);
            }


            List<GateUserJoinTableObject> GetUserGatesTable = await DataBase.GetUserGatesTable(user);

            List<string> gates_id = new List<string>();
            foreach (var g in GetUserGatesTable)
            {
                if (g.gate != null)
                { 
                    gates_id.Add(g.gate.GateID);
                }
            }
            var nas = new
            {
                gates = gates_id,
                total = gates_id.Count
            };

            // error
            return ApiResponse.Success(Request, nas);
        }


        // delete old open events
        // POST /SalesForce/DeleteOpenEvents
        [HttpPost("/[controller]/DeleteOpenEvents", Name = nameof(DeleteOpenEvents))]
        public async Task<IActionResult> DeleteOpenEvents()
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            var a = await DataBase.DeleteOpenEvents();
            return ApiResponse.Success(Request, a);
        }

        // get all users asign to specific gate
        // POST /SalesForce/GetUsersForGateID
        [HttpPost("/[controller]/GetUsersForGateID", Name = nameof(GetUsersForGateID))]
        public async Task<IActionResult> GetUsersForGateID([FromBody] SalesGetUsersForGateForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.gate_id == null)
            {
                return ApiResponse.Error(Request, "gate_id missing", "function must input gate_id", ApiError.Err_MissingParameter);
            }

            //1. get gate ID by it's identifier
            GateObject gate = await DataBase.GetGateByGateID(InputForm.gate_id);

            if (gate == null)
            {
                return ApiResponse.Error(Request, "No gate_id", "No such gate with gate_id" + InputForm.gate_id + " in the system", ApiError.Err_GateIdentifierNotFound);
            }

            //2. get all users_id for that gate
            List<UserDataObject> users = await DataBase.GetAllUserForGate(gate.ObjectId);

            var res = new List<string>();

            for (int i = 0; i < users.Count; i++)
            {
                res.Add(users[i].username);
            }


            var ans = new
            {
                users = res,
                total = res.Count
            };

            // error
            return ApiResponse.Success(Request, ans);
        }

        // get PIN code for specific phone number
        // POST /SalesForce/GetUserPIN
        [HttpPost("/[controller]/GetUserPIN", Name = nameof(GetUserPIN))]
        public async Task<IActionResult> GetUserPIN([FromBody] GetUserPinForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.user_phone_number == null)
            {
                return ApiResponse.Error(Request, "GateObjectId missing", "function must input GateObjectId", ApiError.Err_MissingParameter);
            }

            var user = await DataBase.GetUserByUserName(InputForm.user_phone_number);

            if (user == null)
                return ApiResponse.Error(Request, "no such phone number", "no such phone number", ApiError.Err_UserNotFound);

            if (user.verificationCode == null)
            {
                // Generate 6-digit secure random number
                byte[] bytes = new byte[4];
                RandomNumberGenerator.Fill(bytes);

                // Convert to positive integer
                int value = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;

                // Generate number between 100000 and 999999
                int rInt = 100000 + (value % 900000);
                user.verificationCode = rInt.ToString();

                await DataBase.SetUserVerificationCodeUpdateSmsStamp(user.username, rInt);
            }


            var nas = new
            {
                user_pin_code = user.verificationCode,
            };

            return ApiResponse.Success(Request, nas);
        }

        // POST /gates/GetAllGates
        [HttpPost("/[controller]/GetAllGates", Name = nameof(GetAllGates))]
        public async Task<IActionResult> GetAllGates()
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            //1. get gate ID by it's identifier
            var c = await DataBase.GetAllGatesFull();

            var ans = new
            {
                gates = c,
                total = c.Count
            };


            return ApiResponse.Success(Request, ans);
        }

        // POST /SalesForce/postLog
        [HttpPost("/[controller]/revokeUsers", Name = nameof(revokeUsers))]
        public async Task<IActionResult> revokeUsers([FromBody]SalesForceRevokeUsersForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.usersPhoneNumbers == null)
            {
                return ApiResponse.Error(Request, "usersPhoneNumbers array parameter missing", "usersPhoneNumbers array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.usersPhoneNumbers.Count == 0)
            {
                return ApiResponse.Error(Request, "usersPhoneNumbers array cant be empty", "usersPhoneNumbers array cant be empty", ApiError.Err_MissingParameter);
            }

            var existsusers = await DataBase.GetUsersByUserName(InputForm.usersPhoneNumbers);

            string res = await DataBase.RemoveUsersAndAllTheirGates(existsusers);

            return ApiResponse.Success(Request, res);
        }

        // POST /SalesForce/addNewUsers
        [HttpPost("/[controller]/addNewUsers", Name = nameof(addNewUsers))]
        public async Task<IActionResult> addNewUsers([FromBody]AddNewUsersForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.usersPhoneNumbers == null)
            {
                return ApiResponse.Error(Request, "usersPhoneNumbers array parameter missing", "usersPhoneNumbers array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.accessLevel == null)
            {
                return ApiResponse.Error(Request, "accessLevel array parameter missing", "accessLevel array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.roomsIds == null)
            {
                return ApiResponse.Error(Request, "roomsIds array parameter missing", "roomsIds array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.usersPhoneNumbers.Count == 0)
            {
                return ApiResponse.Error(Request, "usersPhoneNumbers array cant be empty", "usersPhoneNumbers array cant be empty", ApiError.Err_MissingParameter);
            }

            var t_Existsusers = DataBase.GetUsersByUserName(InputForm.usersPhoneNumbers);

            // get all needed gates
            var t_gates = DataBase.GetGatesByGateIDs(InputForm.roomsIds, InputForm.accessLevel);

            await Task.WhenAll(t_Existsusers, t_gates);

            var Existsusers = t_Existsusers.Result;
            var gates = t_gates.Result;

            if ((InputForm.accessLevel.Count == 0) && (InputForm.roomsIds.Count > 0))
            {
                if (gates.Count < InputForm.roomsIds.Count)
                {
                    return ApiResponse.Error(Request, "not all roomsID exists", "not all roomsID exists", ApiError.Err_Unknown);
                }
            }

            // check if any of the users exists
            if (Existsusers.Count > 0)
            {
                string err = "exists users: ";
                for (int i = 0; i < Existsusers.Count; i++)
                {
                    err += Existsusers[i].username + ", ";
                }
                err = err.Remove(err.Length - 2, 2);
                return ApiResponse.Error(Request, "user already exists", err, ApiError.Err_Unknown);
            }

            // build the list of objectId's for the users list
            List<string> usersObjectsIds = new List<string>();

            for (int i = 0; i < InputForm.usersPhoneNumbers.Count; i++)
            {
                usersObjectsIds.Add(DataBase.RandomString(10));
            }

            // build the user join table
            List<GateUserJoinTableObject> gujtos = new List<GateUserJoinTableObject>();
            for (int i = 0; i < usersObjectsIds.Count; i++)
            {
                foreach (var gate in gates)
                {
                    GateUserJoinTableObject guj = new GateUserJoinTableObject();
                    guj.gate = gate;
                    guj.GateID = gate.GateID;
                    guj.isAdmin = false;
                    guj.ProductionCode = gate.ProductionCode;
                    guj.DueDate = DateTime.Now.AddYears(50);
                    guj.ObjectId = DataBase.RandomString(10);
                    guj._p_gate = gate.ObjectId;
                    guj.user = usersObjectsIds[i];
                    guj.name = gate.name;

                    gujtos.Add(guj);
                }
            }

            // insert the user join
            Task a = DataBase.GiverUserAccessToGate(gujtos);

            // insert the users
            Task b = DataBase.InsertNewUsers(InputForm.usersPhoneNumbers, InputForm.accessLevel, usersObjectsIds);

            await Task.WhenAll(a, b);

            string results = "added " + InputForm.usersPhoneNumbers.Count.ToString() + " new users, and gave " + gujtos.Count.ToString() + " gate access to those users";

            //6. success
            return ApiResponse.Success(Request, results);
        }

        // POST /SalesForce/refreshRooms
        [HttpPost("/[controller]/refreshRooms", Name = nameof(refreshRooms))]
        public async Task<IActionResult> refreshRooms([FromBody]RefreshRoomsForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            var start_stamp = DateTime.Now;

            if (InputForm.usersPhoneNumbers == null)
            {
                return ApiResponse.Error(Request, "usersPhoneNumbers array parameter missing", "usersPhoneNumbers array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.accessLevel == null)
            {
                return ApiResponse.Error(Request, "accessLevel array parameter missing", "accessLevel array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.roomsIds == null)
            {
                return ApiResponse.Error(Request, "roomsIds array parameter missing", "roomsIds array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.usersPhoneNumbers.Count == 0)
            {
                return ApiResponse.Error(Request, "usersPhoneNumbers array cant be empty", "usersPhoneNumbers array cant be empty", ApiError.Err_MissingParameter);
            }


            // if there are only phone numbers without any gates/access level, delete the users
            if (InputForm.usersPhoneNumbers.Count > 0)
            {
                if (InputForm.accessLevel.Count == 0)
                {
                    if (InputForm.roomsIds.Count == 0)
                    {
                        var existsusers = await DataBase.GetUsersByUserName(InputForm.usersPhoneNumbers);

                        string res = await DataBase.RemoveUsersAndAllTheirGates(existsusers);

                        return ApiResponse.Success(Request, res);
                    }
                }
            }

            // get all users in the list
            var t_users = DataBase.GetUsersByUserName(InputForm.usersPhoneNumbers);

            // get all needed gates
            var t_neededgates = DataBase.GetGatesByGateIDs(InputForm.roomsIds, InputForm.accessLevel);

            await Task.WhenAll(t_users, t_neededgates);

            var users = t_users.Result;
            var needGates = t_neededgates.Result;
            
            // rooms validation
            // check that all wanted gates exists
            // only if there is no access level
            if ((InputForm.accessLevel.Count == 0) && (InputForm.roomsIds.Count > 0))
            {
                if (needGates.Count < InputForm.roomsIds.Count)
                {
                    string unFoundRooms = "";
                    for(int i = 0; i < InputForm.roomsIds.Count; i++)
                    {
                        bool found = false;
                        for (int ii = 0; (ii < needGates.Count) && (found==false); ii++)
                        {
                            if (InputForm.roomsIds[i].ToString() == needGates[ii].GateID)
                            {
                                found = true;
                            }
                        }

                        if (found == false)
                        {
                            unFoundRooms += InputForm.roomsIds[i].ToString() + ",";
                        }
                        
                    }


                    return ApiResponse.Error(Request, "not all roomsID exists: " + unFoundRooms, "not all roomsID exists: " + unFoundRooms, ApiError.Err_Unknown);
                }
            }

            // if there is any access level change to any of the users, change the access level parameter in the user table
            var usersIDsList = new List<string>();
            foreach (var user in users)
            {
                usersIDsList.Add(user.ObjectId);
            }

            var newUsers = new List<string>();
            var newUsersIds = new List<string>();

            // check if there are new users need to add
            if (users.Count < InputForm.usersPhoneNumbers.Count)
            {
                foreach (var number in InputForm.usersPhoneNumbers)
                {
                    Boolean found = false;
                    foreach (var reg_user in users)
                    {
                        if (reg_user.username == number)
                        {
                            found = true;
                        }
                    }

                    // number listed in the inputform is not registered at the server
                    if (found == false)
                    {
                        newUsers.Add(number);
                        newUsersIds.Add(DataBase.RandomString(10));
                    }
                }

                if (newUsers.Count == 0)
                {
                    return ApiResponse.Error(Request, "newUsers error", "server newUsers error", ApiError.Err_Unknown);
                }

                // register all users in newUsers
                await DataBase.InsertNewUsers(newUsers, InputForm.accessLevel, newUsersIds);
            }

            var t_changedAccessLevel = DataBase.SetNewAccessLevel(usersIDsList, InputForm.accessLevel);

            //NO NEED
            //var t_userGates = DataBase.GetUsersGatesTable(users);

            // delete all gatesToUsers
            var t_remove = DataBase.RemoveUsersGates(users);


            //NO NEED
            //Task.WaitAll(t_changedAccessLevel, t_userGates, t_remove);
            Task.WaitAll(t_changedAccessLevel, t_remove);

            var changedAccessLevel = t_changedAccessLevel.Result;

            //NO NEED
            //var userGates = t_userGates.Result;

            // build the user join table for the registered users
            List<GateUserJoinTableObject> gujtos = new List<GateUserJoinTableObject>();
            for (int i = 0; i < users.Count; i++)
            {
                foreach (var gate in needGates)
                {
                    GateUserJoinTableObject guj = new GateUserJoinTableObject();
                    guj.gate = gate;
                    guj.GateID = gate.GateID;
                    guj.isAdmin = false;
                    guj.ProductionCode = gate.ProductionCode;
                    guj.DueDate = DateTime.Now.AddYears(50);
                    guj.ObjectId = DataBase.RandomString(10);
                    guj._p_gate = gate.ObjectId;
                    guj.user = users[i].ObjectId;
                    guj.name = gate.name;

                    gujtos.Add(guj);
                }
            }

            // build the user join table for the new users
            for (int i = 0; i < newUsersIds.Count; i++)
            {
                foreach (var gate in needGates)
                {
                    GateUserJoinTableObject guj = new GateUserJoinTableObject();
                    guj.gate = gate;
                    guj.GateID = gate.GateID;
                    guj.isAdmin = false;
                    guj.ProductionCode = gate.ProductionCode;
                    guj.DueDate = DateTime.Now.AddYears(50);
                    guj.ObjectId = DataBase.RandomString(10);
                    guj._p_gate = gate.ObjectId;
                    guj.user = newUsersIds[i];
                    guj.name = gate.name;

                    gujtos.Add(guj);
                }
            }

            // insert the user join
            if (gujtos.Count > 0)
            {
                Task a = DataBase.GiverUserAccessToGate(gujtos);
                await Task.WhenAll(a);
            }

            string newUsersResults = "";
            if (newUsersIds.Count > 0)
            {
                newUsersResults = ", and added " + newUsersIds.Count.ToString() + " new users";
            }

            string delta = DateTime.Now.Subtract(start_stamp).TotalSeconds.ToString();



            string results = "time to do: " + delta + "seconds refresh rooms list for: " + InputForm.usersPhoneNumbers.Count.ToString() + " users, and gave " + gujtos.Count.ToString() + " gate access to those users" + newUsersResults + ", and " + changedAccessLevel.ToString() + " users changed their access level";

            //6. success
            return ApiResponse.Success(Request, results);
        }

        // POST /SalesForce/changePhoneNumber
        [HttpPost("/[controller]/changePhoneNumber", Name = nameof(changePhoneNumber))]
        public async Task<IActionResult> changePhoneNumber([FromBody]ChangePhoneNumberForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.newPhoneNumber == null)
            {
                return ApiResponse.Error(Request, "newPhoneNumber parameter missing", "newPhoneNumber parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.oldPhoneNumber == null)
            {
                return ApiResponse.Error(Request, "oldPhoneNumber parameter missing", "oldPhoneNumber parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.newPhoneNumber == "")
            {
                return ApiResponse.Error(Request, "newPhoneNumber parameter empty", "newPhoneNumber parameter empty", ApiError.Err_MissingParameter);
            }
            if (InputForm.oldPhoneNumber == "")
            {
                return ApiResponse.Error(Request, "oldPhoneNumber parameter empty", "oldPhoneNumber parameter empty", ApiError.Err_MissingParameter);
            }

            List<string> u = new List<string>();
            u.Add(InputForm.oldPhoneNumber);

            var users = await DataBase.GetUsersByUserName(u);

            List<string> uNew = new List<string>();
            uNew.Add(InputForm.newPhoneNumber);

            var usersNew = await DataBase.GetUsersByUserName(uNew);

            if (users.Count > 1)
            {
                return ApiResponse.Error(Request, "more then one user with this number", "please contact omgate admin to fix this issue", ApiError.Err_Unknown);
            }

            if (users.Count == 0)
            {
                return ApiResponse.Error(Request, "user does not exist in the system", "user number " + InputForm.oldPhoneNumber + " does not exist in the system", ApiError.Err_Unknown);
            }

            if (usersNew.Count > 0)
            {
                return ApiResponse.Error(Request, "new phone number already exist in the system", "user number " + InputForm.newPhoneNumber + "  exist in the system, you cant have two users with the same number", ApiError.Err_Unknown);
            }

            await DataBase.SetNewPhoneNumber(users[0].ObjectId, InputForm.newPhoneNumber);

            //6. success
            return ApiResponse.Success(Request);
        }


        // GET /SalesForce/GateIDNameReplace
        [HttpPost("/[controller]/GateIDNameReplace", Name = nameof(GateIDNameReplace))]
        public IActionResult GateIDNameReplace()
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            //6. success
            return ApiResponse.Success(Request);
        }

        // POST /SalesForce/insertNewGate
        [HttpPost("/[controller]/insertNewGate", Name = nameof(insertNewGate))]
        public async Task<IActionResult> insertNewGate([FromBody]InsertNewGateForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.identifier == null)
            {
                return ApiResponse.Error(Request, "identifier parameter missing", "usersPhoneNumbers array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.name == null)
            {
                return ApiResponse.Error(Request, "name parameter missing", "accessLevel array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.gateID == null)
            {
                return ApiResponse.Error(Request, "gateID parameter missing", "gateID array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.address == null)
            {
                return ApiResponse.Error(Request, "address parameter missing", "roomsIds array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.accessLevel == null)
            {
                return ApiResponse.Error(Request, "accessLevel parameter missing", "accessLevel array parameter missing", ApiError.Err_MissingParameter);
            }

            {
                var gate = await DataBase.GetGateByIdentifier(InputForm.identifier);
                if (gate != null)
                {
                    return ApiResponse.Error(Request, "identifier already in system", "identifier already in system", ApiError.Err_MissingParameter);
                }
            }

            {
                var gate = await DataBase.GetGateByGateID(InputForm.gateID);
                if (gate != null)
                {
                    return ApiResponse.Error(Request, "gateID already in system", "gateID already in system", ApiError.Err_MissingParameter);
                }
            }

            string id = DataBase.BeAllInsertNewGate(InputForm.accessLevel, InputForm.identifier, InputForm.name, InputForm.gateID, InputForm.address);

            var users_to_add = await DataBase.GetUsersByAccessLevel(InputForm.accessLevel);
            
            // build the user join table for the registered users
            List<GateUserJoinTableObject> gujtos = new List<GateUserJoinTableObject>();

            foreach (var user in users_to_add)
            {
                GateUserJoinTableObject guj = new GateUserJoinTableObject();
                guj.GateID = InputForm.gateID;
                guj.isAdmin = false;
                guj.ProductionCode = "164ba722755bdc8e";
                guj.DueDate = DateTime.Now.AddYears(50);
                guj.ObjectId = DataBase.RandomString(10);
                guj._p_gate = id;
                guj.user = user;
                guj.name = InputForm.name;
                guj.createdAt = DateTime.UtcNow;
                guj.updatedAt = DateTime.UtcNow;

                gujtos.Add(guj);
            }

            // insert the user join
            if (gujtos.Count > 0)
            {
                Task a = DataBase.GiverUserAccessToGate(gujtos);
                await Task.WhenAll(a);
            }

            //6. success
            return ApiResponse.Success(Request, string.Format("inserted 1 gate, id={0} and total {1} users added", id,users_to_add.Count));
        }

        // get all users asign to specific gates
        // POST /gates/GetAllGatesByAL
        [HttpPost("/[controller]/GetAllGatesByAL", Name = nameof(GetAllGatesByAL))]
        public async Task<IActionResult> GetAllGatesByAL([FromBody] GetAllGatesByALForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            //1. get gate ID by it's identifier
            var c = await DataBase.GetAllGatesWithAl(InputForm.access_level);

            var ans = new
            {
                gates = c,
                total = c.Count
            };


            return ApiResponse.Success(Request, ans);
        }


        // POST /SalesForce/makeRoomPrivate
        [HttpPost("/[controller]/makeRoomPrivate", Name = nameof(makeRoomPrivate))]
        public async Task<IActionResult> makeRoomPrivate([FromBody] makeRoomPrivateForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.roomId == null)
            {
                return ApiResponse.Error(Request, "roomId string missing", "roomId parameter missing", ApiError.Err_MissingParameter);
            }
            
            if (InputForm.roomId == "")
            {
                return ApiResponse.Error(Request, "roomId cant be empty", "roomId cant be empty", ApiError.Err_MissingParameter);
            }

           
            // get all needed gates
            var t_neededgates = await DataBase.GetGateByGateID(InputForm.roomId);

            if (t_neededgates == null)
            {
                return ApiResponse.Error(Request, "cant find this room ID " + InputForm.roomId, "cant find this room ID " + InputForm.roomId, ApiError.Err_MissingParameter);
            }

            if (t_neededgates.GateID == "")
            {
                return ApiResponse.Error(Request, "cant find this room ID " + InputForm.roomId, "cant find this room ID " + InputForm.roomId, ApiError.Err_MissingParameter);
            }

            var count = await DataBase.deleteAllUserForGate(t_neededgates.ObjectId);

            string results = "removed " + count.ToString() + " users from " + t_neededgates.GateID + " " + t_neededgates.identifier + " " + t_neededgates.name;


            var v = new List<string>();
            var res2 = await DataBase.SetNewAccessLevelForGate(t_neededgates.ObjectId, v);

            results += " - and set new access level to " + res2.ToString() + " gates";

            //6. success
            return ApiResponse.Success(Request, results);
        }

        // POST /SalesForce/resetGateAL
        [HttpPost("/[controller]/resetGateAL", Name = nameof(resetGateAL))]
        public async Task<IActionResult> resetGateAL([FromBody] resetGateALForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.accessLevel == null)
            {
                return ApiResponse.Error(Request, "accessLevel parameter missing", "accessLevel array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.gateID == null)
            {
                return ApiResponse.Error(Request, "roomId parameter missing", "roomId array parameter missing", ApiError.Err_MissingParameter);
            }
           

            var gate = await DataBase.GetGateByGateID(InputForm.gateID);
            if (gate == null)
            {
                return ApiResponse.Error(Request, "roomId not in the system", "roomId not in the system", ApiError.Err_MissingParameter);
            }
 
            var users_to_add = await DataBase.GetUsersByAccessLevel(InputForm.accessLevel);

            var delete_count = await DataBase.deleteAllUserForGate(gate.ObjectId);

            // build the user join table for the registered users
            List<GateUserJoinTableObject> gujtos = new List<GateUserJoinTableObject>();

            foreach (var user in users_to_add)
            {
                GateUserJoinTableObject guj = new GateUserJoinTableObject();
                guj.GateID = gate.GateID;
                guj.isAdmin = false;
                guj.ProductionCode = "164ba722755bdc8e";
                guj.DueDate = DateTime.Now.AddYears(50);
                guj.ObjectId = DataBase.RandomString(10);
                guj._p_gate = gate.ObjectId;
                guj.user = user;
                guj.name = gate.name;

                gujtos.Add(guj);
            }

            // insert the user join
            if (gujtos.Count > 0)
            {
                Task a = DataBase.GiverUserAccessToGate(gujtos);
                await Task.WhenAll(a);
            }


            var r_d = await (DataBase.SetNewAccessLevelForGate(gate.ObjectId, InputForm.accessLevel));

            //6. success
            return ApiResponse.Success(Request, string.Format("updated 1 gate, id={0}, removed all access to this gate: " + delete_count.ToString() + " and total {1} users added " + "set AL to {2} gates,", gate.GateID, users_to_add.Count, r_d.ToString()));
        }

        // POST /SalesForce/replaceGateID
        [HttpPost("/[controller]/replaceGateID", Name = nameof(replaceGateID))]
        public async Task<IActionResult> replaceGateID([FromBody] changeGateIDForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.oldgateID == null)
            {
                return ApiResponse.Error(Request, "oldgateID parameter missing", "oldgateID array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.newgateID == null)
            {
                return ApiResponse.Error(Request, "newgateID parameter missing", "newgateID array parameter missing", ApiError.Err_MissingParameter);
            }

            var oldgate = await DataBase.GetGateByGateID(InputForm.oldgateID);
            if (oldgate == null)
            {
                return ApiResponse.Error(Request, "oldgate " + InputForm.oldgateID + " not in the system", "oldgate " + InputForm.oldgateID + " not in the system", ApiError.Err_MissingParameter);
            }

            var newgate = await DataBase.GetGateByGateID(InputForm.newgateID);
            if (newgate != null)
            {
                return ApiResponse.Error(Request, "newgateID " + InputForm.newgateID + " in the system already", "newgateID " + InputForm.newgateID + " in the system already", ApiError.Err_MissingParameter);
            }

            var r_d = await (DataBase.ReplaceGateID(InputForm.oldgateID, InputForm.newgateID));

            //6. success
            return ApiResponse.Success(Request, string.Format("changed gateID from {0} to {1} to {2} total gates", InputForm.oldgateID, InputForm.newgateID, r_d.ToString()));
        }


        // POST /SalesForce/refreshRooms
        [HttpPost("/[controller]/addroomtouser", Name = nameof(addroomtouser))]
        public async Task<IActionResult> addroomtouser([FromBody] addRoomToUserForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.userPhoneNumber == null)
            {
                return ApiResponse.Error(Request, "userPhoneNumber array parameter missing", "userPhoneNumber array parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.roomId == null)
            {
                return ApiResponse.Error(Request, "roomId array parameter missing", "roomId array parameter missing", ApiError.Err_MissingParameter);
            }
            

            var t_users = await DataBase.GetUserByUserName(InputForm.userPhoneNumber);

            // get all needed gates
            var t_neededgates = await DataBase.GetGateByGateID(InputForm.roomId);


            if (t_neededgates == null)
            {
                return ApiResponse.Error(Request, "roomId no in the system", "roomId no in the system", ApiError.Err_MissingParameter);
            }

            if (t_users == null)
            {
                return ApiResponse.Error(Request, "userPhoneNumber no in the system", "userPhoneNumber no in the system", ApiError.Err_MissingParameter);
            }


            var t_userGates = await DataBase.GetUserGatesTable(t_users);

            foreach (var a in t_userGates)
            {
                if (a.GateID == InputForm.roomId)
                {
                    return ApiResponse.Error(Request, "user already has access to this roomId", "user already has access to this roomId", ApiError.Err_MissingParameter);
                }
            }


            // build the user join table for the registered users
            List<GateUserJoinTableObject> gujtos = new List<GateUserJoinTableObject>();

            GateUserJoinTableObject guj = new GateUserJoinTableObject();
            guj.gate = t_neededgates;
            guj.GateID = t_neededgates.GateID;
            guj.isAdmin = false;
            guj.ProductionCode = t_neededgates.ProductionCode;
            guj.DueDate = DateTime.Now.AddYears(50);
            guj.ObjectId = DataBase.RandomString(10);
            guj._p_gate = t_neededgates.ObjectId;
            guj.user = t_users.ObjectId;
            guj.name = t_neededgates.name;

            gujtos.Add(guj);

            await DataBase.GiverUserAccessToGate(gujtos);

            string results = "added 1 access for " + t_users.username;

            //6. success
            return ApiResponse.Success(Request, results);
        }


        // POST /SalesForce/replaceGateNameAddress
        [HttpPost("/[controller]/replaceGateNameAddress", Name = nameof(replaceGateNameAddress))]
        public async Task<IActionResult> replaceGateNameAddress([FromBody] replaceGateNameAddressForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.gateID == null)
            {
                return ApiResponse.Error(Request, "gateID parameter missing", "gateID parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.new_name == null)
            {
                return ApiResponse.Error(Request, "new_name parameter missing", "new_name parameter missing", ApiError.Err_MissingParameter);
            }
            if (InputForm.new_address == null)
            {
                return ApiResponse.Error(Request, "new_address parameter missing", "new_address parameter missing", ApiError.Err_MissingParameter);
            }

            var oldgate = await DataBase.GetGateByGateID(InputForm.gateID);
            if (oldgate == null)
            {
                return ApiResponse.Error(Request, "gateID " + InputForm.gateID + " not in the system", "gateID " + InputForm.gateID + " not in the system", ApiError.Err_MissingParameter);
            }

            if (InputForm.new_address != "")
            {
                var r_d = await (DataBase.ReplaceGateAddress(InputForm.gateID, InputForm.new_address));
            }

            if (InputForm.new_name != "")
            {
                var r_d = await (DataBase.ReplaceGateName(InputForm.gateID, InputForm.new_name));
            }

            oldgate = await DataBase.GetGateByGateID(InputForm.gateID);

            //6. success
            return ApiResponse.Success(Request, string.Format("changed gateID {0} name to {1} and address to {2}", oldgate.GateID, oldgate.name, oldgate.address));
        }


        // POST /SalesForce/replaceGateIdentifier
        [HttpPost("/[controller]/replaceGateIdentifier", Name = nameof(replaceGateIdentifier))]
        public async Task<IActionResult> replaceGateIdentifier([FromBody] changeGateIdentifierForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            if (InputForm.oldIdentifier == null)
            {
                return ApiResponse.Error(Request, "oldIdentifier parameter missing", "oldIdentifier parameter missing", ApiError.Err_MissingParameter);
            }
            else
            {
                if (InputForm.oldIdentifier == "")
                {
                    return ApiResponse.Error(Request, "oldIdentifier parameter missing", "oldIdentifier parameter missing", ApiError.Err_MissingParameter);
                }
            }

            if (InputForm.newIdentifier == null)
            {
                return ApiResponse.Error(Request, "newIdentifier parameter missing", "newIdentifier parameter missing", ApiError.Err_MissingParameter);
            }
            else
            {
                if (InputForm.newIdentifier == "")
                {
                    return ApiResponse.Error(Request, "newIdentifier parameter missing", "newIdentifier parameter missing", ApiError.Err_MissingParameter);
                }
            }


            var oldgate = await DataBase.GetGateByIdentifier(InputForm.oldIdentifier);
            if (oldgate == null)
            {
                return ApiResponse.Error(Request, "oldIdentifier " + InputForm.oldIdentifier + " not in the system", "oldIdentifier " + InputForm.oldIdentifier + " not in the system", ApiError.Err_MissingParameter);
            }

            var newgate = await DataBase.GetGateByIdentifier(InputForm.newIdentifier);
            if (newgate != null)
            {
                return ApiResponse.Error(Request, "newIdentifier " + InputForm.newIdentifier + " in the system already", "newIdentifier " + InputForm.newIdentifier + " in the system already", ApiError.Err_MissingParameter);
            }

            var r_d = await (DataBase.ChangeGateIdentifier(InputForm.oldIdentifier, InputForm.newIdentifier));

            //6. success
            if (r_d == true)
            {
                return ApiResponse.Success(Request, string.Format("changed identifier from {0} to {1} SUCCESS", InputForm.oldIdentifier, InputForm.newIdentifier));
            }
            else
            {
                return ApiResponse.Error(Request, string.Format("changed identifier from {0} to {1} FAIL", InputForm.oldIdentifier, InputForm.newIdentifier), "",0);
            }
        }

        // POST /gates/GetAllGatesId
        [HttpPost("/[controller]/GetAllGatesId", Name = nameof(GetAllGatesId))]
        public async Task<IActionResult> GetAllGatesId([FromBody] GetAllGatesIdForm InputForm, CancellationToken ct)
        {
            var (isValid, errorResult, token) = ValidateTokenFromHeader();
            if (!isValid)
                return errorResult!;

            //1. get gate ID by it's identifier
            var c = await DataBase.GetAllGatesFull();

            int count = 0;
            string ans = "";

            if (count > (c.Count - 30))
            {
                return ApiResponse.Error(Request, "count too high", "count too high",0);
            }

            for(count = 20;count<(InputForm.count+20);count++)
            {
                ans += "\"" + c[count].GateID + "\",";
            }

            ans = ans.Remove(ans.Length - 1, 1);

            return ApiResponse.Success(Request, ans);
        }

    }

}
