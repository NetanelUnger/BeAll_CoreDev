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
    public class openingeventsController : Controller
    {
        // POST /openingevents/InsertOpeningEventForToken
        [HttpPost("/[controller]/InsertOpeningEventForToken", Name = nameof(InsertOpeningEventForToken))]
        public async Task<IActionResult> InsertOpeningEventForToken([FromBody] InsertOpeningEventForTokenForm InputForm, CancellationToken ct)
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

            if (
            //(InputForm.stamp_day == null) ||
            //        (InputForm.stamp_month == null) ||
            //        (InputForm.stamp_year == null) ||
            //        (InputForm.stamp_hour == null) ||
            //        (InputForm.stamp_minute == null) ||
            //        (InputForm.stamp_second == null) ||
                    (InputForm.identifier == null)
            //        (InputForm.didOpen == null)
                   )
            {
                return ApiResponse.Error(Request, "parameter missing", "function must input all parameters", ApiError.Err_MissingParameter);
            }

            //0. get the user
            UserObject user = await DataBase.VerifyUserToekn(token);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "No such token in the system", "No such token in the system, logout the user and then login to get a new toekn", ApiError.Err_NoToken);
            }

            // not yet compatible with PARSE for IOS
            //1. build the time stamp
            //DateTime stamp = new DateTime(Convert.ToInt32(InputForm.stamp_year), Convert.ToInt32(InputForm.stamp_month), Convert.ToInt32(InputForm.stamp_day), Convert.ToInt32(InputForm.stamp_hour), Convert.ToInt32(InputForm.stamp_minute), Convert.ToInt32(InputForm.stamp_second));
            //await DataBase.InserOpeningEvent(stamp, InputForm.identifier, user);

            DateTime date;
            //if (InputForm.stamp_year.HasValue && InputForm.stamp_month.HasValue && InputForm.stamp_day.HasValue && InputForm.stamp_hour.HasValue && InputForm.stamp_minute.HasValue && InputForm.stamp_second.HasValue)
            //{
            //    try
            //    {
            //        date = new DateTime(InputForm.stamp_year.Value, InputForm.stamp_month.Value, InputForm.stamp_day.Value, InputForm.stamp_hour.Value, InputForm.stamp_minute.Value, InputForm.stamp_second.Value);
            //    }
            //    catch
            //    {
            //        date = DateTime.UtcNow;
            //    }
            //}
            //else
            {
                date = DateTime.UtcNow;
            }

            //2. insert to DB
            await DataBase.BeAllInsertOpenEvent(user.username, InputForm.didOpen, InputForm.tries, InputForm.success, true, true, InputForm.identifier, date);
            //await ClassParse.insert_RealOpeningEvent(InputForm.identifier, InputForm.success, user.username, InputForm.didOpen, InputForm.tries);

            //3. success
            return ApiResponse.Success(Request);
        }

        // POST /openingevents/GetOpeningEventForGate
        [HttpPost("/[controller]/GetOpeningEventForGate", Name = nameof(GetOpeningEventForGate))]
        public async Task<IActionResult> GetOpeningEventForGate([FromBody]GetOpeningEventForGateForm InputForm, CancellationToken ct)
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

            if ((InputForm.start_stamp_day == null) ||
                    (InputForm.start_stamp_month == null) ||
                    (InputForm.start_stamp_year == null) ||
                    (InputForm.end_stamp_day == null) ||
                    (InputForm.end_stamp_month == null) ||
                    (InputForm.end_stamp_year == null) ||
                    (InputForm.identifier == null))
            {
                return ApiResponse.Error(Request, "parameter missing", "function must input all parameters", ApiError.Err_MissingParameter);
            }

            //0. get the user
            UserObject user = await DataBase.VerifyUserToekn(token);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "No such token in the system", "No such token in the system, logout the user and then login to get a new toekn", ApiError.Err_NoToken);
            }

            //1. build the time stamp
            DateTime start_stamp = new DateTime(Convert.ToInt32(InputForm.start_stamp_year), Convert.ToInt32(InputForm.start_stamp_month), Convert.ToInt32(InputForm.start_stamp_day), 0,0,0);
            DateTime end_stamp = new DateTime(Convert.ToInt32(InputForm.end_stamp_year), Convert.ToInt32(InputForm.end_stamp_month), Convert.ToInt32(InputForm.end_stamp_day), 23, 59, 59);


            //2. request from DB
           List<OpeningEvent> res = await DataBase.GetOpeningEventsPerGate(start_stamp, end_stamp, InputForm.identifier, user);

            //2. success
            return ApiResponse.Success(Request, res);
        }


        [HttpPost("/[controller]/open/", Name = nameof(openTheGate))]
        public async Task<IActionResult> openTheGate([FromBody] openTheGateObject inputForm, CancellationToken ct)
        {
            try
            {
                string token;

                //1. validate
                if (inputForm.token == null)
                {
                    return ApiResponse.Error(Request, "token missing2", "function must input token2", ApiError.Err_MissingParameter);
                }
                else
                {
                    token = inputForm.token;
                }


                if (token == "")
                {
                    return ApiResponse.Error(Request, "token cant be 0", "token cant be 0", ApiError.Err_MissingParameter);
                }

                //2. check the token
                UserObject user = await DataBase.VerifyUserToekn(token);

                if (user == null)
                {
                    return ApiResponse.Error(Request, "token not listed", "there is no such token in the system", ApiError.Err_NoToken);
                }


                var gate = await DataBase.GetGateByIdentifier(inputForm.identifier);

                if (gate == null)
                {
                    return ApiResponse.Error(Request, "no such gate", "there is no such gate in the system", ApiError.Err_GateIdentifierNotFound);
                }

                var gid = await DataBase.IsGateObjIDrelatedToUser(user, gate.ObjectId);

                if (gid == null)
                {
                    return ApiResponse.Error(Request, "user dont have access", "user dont have access to this gate", ApiError.Err_GateIdentifierNotFound);
                }

                //(OMGATE X1, X2, X3, X4, X5, X6)
                //X1, X2 = ownAddress[2]
                //X3, X4 = ownAddress[1]
                //X5, X6 = ownAddress[0]  
                //(OMGATEB1DEBF)

                var name = gate.identifier.Substring(gate.identifier.Length - 4);//001A
                var b = int.Parse(name, System.Globalization.NumberStyles.HexNumber); // 26
                                                                                      //console.log("identifier last 4: " + b);


                var X3 = (b & 0x000f);               //10
                var X4 = (b & 0x00f0) >> 4;          //1
                var X5 = (b & 0x0f00) >> 8;          //0
                var X6 = (b & 0xf000) >> 12;         //0

                byte[] tempByteArray = new byte[2] { Convert.ToByte(inputForm.scramberbyte1), Convert.ToByte(inputForm.scramberbyte0) };
                UInt16 val = BitConverter.ToUInt16(tempByteArray, 0);

                var S1 = (val & 0x000f);               //2
                var S2 = (val & 0x00f0) >> 4;          //1
                var S3 = (val & 0x0f00) >> 8;          //4
                var S4 = (val & 0xf000) >> 12;         //3

                // (S1, S3, S4, S2) 
                var S = (S1) | (S3 << 4) | (S4 << 8) | (S2 << 12); //0x5BF5


                // (X6, X3, X5, X4) 
                var X = (X3) | (X6 << 4) | (X4 << 8) | (X5 << 12);

                // (S1, S3, S4, S2)Xor(X5, X4, X6, X3)
                var ans = S ^ X; //12334


                var res = new
                {
                    codebyte0 = ((ans & 0xff00) >> 8),
                    codebyte1 = ans & 0x00ff
                };

                return ApiResponse.Success(Request, res);
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(Request, ex.Message, "ex + stack", ApiError.Err_Unknown);
            }
        }

    }

}
