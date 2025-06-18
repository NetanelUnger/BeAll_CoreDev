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
using Newtonsoft.Json.Linq;


namespace BeAllCore.Controllers
{
    [Route("/[controller]")]
    public class usersController : Controller
    {
        // POST /Users/FirstlogIn6Digits
        [HttpPost("/[controller]/firstlogIn6Digits", Name = nameof(FirstlogIn6Digits))]
        public async Task<IActionResult> FirstlogIn6Digits([FromBody] UserFirstLogInForm InputForm, CancellationToken ct)
        {
            if (InputForm.Token != null)
            {
                if (InputForm.Token != "")
                {
                    var ans = BeAllCore.Security.RecaptchaVerifier.VerifyToken("omgate-beall", "6LfkIEkrAAAAAPy3J4s8dCxRN2lfFA9Bq77WVPx7", InputForm.Token);
                    var ans2 = BeAllCore.Security.RecaptchaVerifier.VerifyToken("omgate-beall", "6Le5B0orAAAAACrLRXrb1eOABzyAlWjyd_O2ktNu", InputForm.Token);
                    await Task.WhenAll(ans, ans2);

                    if ( (ans.Result != true) && (ans2.Result != true) )
                    {
                        return ApiResponse.Error(Request, "Captcha token error", "Captcha token error", ApiError.Err_NoToken);
                    }
                }
                else
                {
                    return ApiResponse.Error(Request, "no Captcha token error", "no Captcha token error", ApiError.Err_NoToken);
                }
            }
            else
            {
                return ApiResponse.Error(Request, "no Captcha token error", "no Captcha token error", ApiError.Err_NoToken);
            }

            string UserPhoneNumber;
            //validate
            if (InputForm.UserPhoneNumber == null)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }
            else
            {
                UserPhoneNumber = InputForm.UserPhoneNumber;
            }

            if ((UserPhoneNumber.StartsWith("+") == false) || (Regex.IsMatch(UserPhoneNumber.Replace("+", ""), @"^\d+$") == false))
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }

            //0. get the user
            UserObject user = await DataBase.GetUserByUserName(UserPhoneNumber);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }

            //1. check user last sms sent time stamp and limit it
            //if ((UserPhoneNumber.Contains("547755955") == false) && (UserPhoneNumber.Contains("547679977") == false))
            if (user.lastSmsSent != null)
            {
                TimeSpan t_lim = new TimeSpan(0, 0, 30);
                if (DateTime.Now.Subtract(user.lastSmsSent.Value) < t_lim)
                {
                    return ApiResponse.Error(Request, "SMS only once in 30 seconds", "SMS only once in 30 seconds", ApiError.Err_UserNotFound);
                }
            }


            var newList = new List<DateTime>();
            if (user.smsTimeStamps != null)
            {
                foreach (var v in user.smsTimeStamps)
                {
                    if (v.AddMinutes(30) > DateTime.UtcNow)
                    {
                        // forget about it
                    }
                    else
                    {
                        // keep it in the list
                        newList.Add(v);
                    }
                }
            }


            // Reassign the filtered list back to the user if needed
            user.smsTimeStamps = newList;

            if (newList.Count > 5)
            {
                // If more than 5 SMS were sent in the last 30 minutes, block the user
                double delta = newList[0].AddMinutes(30).Subtract(DateTime.UtcNow).TotalSeconds;
                int secondsToWait = (int)Math.Ceiling(delta);
                TimeSpan waitTime = TimeSpan.FromSeconds(secondsToWait);
                string waitMessage = $"{waitTime.Minutes} minutes and {waitTime.Seconds} seconds";

                return ApiResponse.Error(
                    Request,
                    "User error, try again later",
                    $"Sent {newList.Count} SMS in the last 30 minutes. Please wait at least {waitMessage} before trying again.",
                    ApiError.Err_UserNotFound
                );
            }


            //1.1 verify user blocked until time stamp and limit it
            if (user.userBlockedUntil != null)
                if (user.userBlockedUntil != null)
                {
                    double t = Math.Round(user.userBlockedUntil.Value.Subtract(DateTime.UtcNow).TotalSeconds, 0);
                    if (user.userBlockedUntil.Value.Subtract(DateTime.UtcNow).TotalMilliseconds > 0)
                    {
                        string t1 = "User blocked for " + t.ToString() + " seconds";
                        return ApiResponse.Error(Request, t1, t1, ApiError.Err_UserNotFound);
                    }
                }

            //2. generate new pin code
            // Generate 6-digit secure random number
            byte[] bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);

            // Convert to positive integer
            int value = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;

            // Generate number between 100000 and 999999
            int rInt = 100000 + (value % 900000);


            //4. send the user sms with the new code
            if (SmsClass.SendSMS(UserPhoneNumber, "PIN for BeAll app: " + rInt.ToString()) == false)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }
            else
            {
                await DataBase.SetUserpinCodeValidDueDate(UserPhoneNumber, DateTime.UtcNow.AddMinutes(2)); //validate for 2 minutes
                newList.Add(DateTime.UtcNow);
                await DataBase.SetSmsTimeStamps(newList, user);
                await DataBase.SetUserVerificationCodeUpdateSmsStamp(UserPhoneNumber, rInt);
                await DataBase.SetUserToekn(UserPhoneNumber, "0"); //5. clear the user token to 0
            }

            //6. success
            return ApiResponse.Success(Request, "sms sent");
        }


        // POST /Users/FirstLogIn
        [HttpPost("/[controller]/firstlogIn", Name = nameof(FirstLogIn))]
        public async Task<IActionResult> FirstLogIn([FromBody]UserFirstLogInForm InputForm, CancellationToken ct)
        {
            if (InputForm.Token != null)
            {
                if (InputForm.Token != "")
                {
                    var ans = BeAllCore.Security.RecaptchaVerifier.VerifyToken("omgate-beall", "6LfkIEkrAAAAAPy3J4s8dCxRN2lfFA9Bq77WVPx7", InputForm.Token);
                    var ans2 = BeAllCore.Security.RecaptchaVerifier.VerifyToken("omgate-beall", "6Le5B0orAAAAACrLRXrb1eOABzyAlWjyd_O2ktNu", InputForm.Token);
                    await Task.WhenAll(ans, ans2);

                    if ((ans.Result != true) && (ans2.Result != true))
                    {
                        return ApiResponse.Error(Request, "Captcha token error", "Captcha token error", ApiError.Err_NoToken);
                    }
                }
                else
                {
                    return ApiResponse.Error(Request, "no Captcha token error", "no Captcha token error", ApiError.Err_NoToken);
                }
            }
            else
            {
                return ApiResponse.Error(Request, "no Captcha token error", "no Captcha token error", ApiError.Err_NoToken);
            }

            string UserPhoneNumber;
            //validate
            if (InputForm.UserPhoneNumber == null)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }
            else
            {
                UserPhoneNumber = InputForm.UserPhoneNumber;
            }

            if ((UserPhoneNumber.StartsWith("+") == false) || (Regex.IsMatch(UserPhoneNumber.Replace("+", ""), @"^\d+$") == false))
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }
                
            //0. get the user
            UserObject user = await DataBase.GetUserByUserName(UserPhoneNumber);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }

            //1. check user last sms sent time stamp and limit it
            //if ((UserPhoneNumber.Contains("547755955") == false) && (UserPhoneNumber.Contains("547679977") == false))
            if (user.lastSmsSent != null)
            {
                TimeSpan t_lim = new TimeSpan(0, 0, 30);
                if (DateTime.Now.Subtract(user.lastSmsSent.Value) < t_lim)
                {
                    return ApiResponse.Error(Request, "SMS only once in 30 seconds", "SMS only once in 30 seconds", ApiError.Err_UserNotFound);
                }
            }


            var newList = new List<DateTime>();
            if (user.smsTimeStamps != null)
            {
                foreach (var v in user.smsTimeStamps)
                {
                    if (v.AddMinutes(30) > DateTime.UtcNow)
                    {
                        // forget about it
                    }
                    else
                    {
                        // keep it in the list
                        newList.Add(v);
                    }
                }
            }


            // Reassign the filtered list back to the user if needed
            user.smsTimeStamps = newList;

            if (newList.Count > 5)
            {
                // If more than 5 SMS were sent in the last 30 minutes, block the user
                double delta = newList[0].AddMinutes(30).Subtract(DateTime.UtcNow).TotalSeconds;
                int secondsToWait = (int)Math.Ceiling(delta);
                TimeSpan waitTime = TimeSpan.FromSeconds(secondsToWait);
                string waitMessage = $"{waitTime.Minutes} minutes and {waitTime.Seconds} seconds";

                return ApiResponse.Error(
                    Request,
                    "User error, try again later",
                    $"Sent {newList.Count} SMS in the last 30 minutes. Please wait at least {waitMessage} before trying again.",
                    ApiError.Err_UserNotFound
                );
            }


            //1.1 verify user blocked until time stamp and limit it
            if (user.userBlockedUntil != null)
                if (user.userBlockedUntil != null)
            {
                double t = Math.Round(user.userBlockedUntil.Value.Subtract(DateTime.UtcNow).TotalSeconds, 0);
                if (user.userBlockedUntil.Value.Subtract(DateTime.UtcNow).TotalMilliseconds > 0)
                {
                    string t1 = "User blocked for " + t.ToString() + " seconds";
                    return ApiResponse.Error(Request, t1, t1, ApiError.Err_UserNotFound);
                }
            }

            //2. generate new pin code
            // Generate 4 digit secure random number
            byte[] bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);

            // Convert to positive integer
            int value = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;

            // Generate number between 1000 and 9999
            int rInt = 1000 + (value % 9000);


            //4. send the user sms with the new code
            if (SmsClass.SendSMS(UserPhoneNumber, "PIN for BeAll app: " + rInt.ToString()) == false)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }
            else
            {
                await DataBase.SetUserpinCodeValidDueDate(UserPhoneNumber, DateTime.UtcNow.AddMinutes(2)); //validate for 2 minutes
                newList.Add(DateTime.UtcNow);
                await DataBase.SetSmsTimeStamps(newList, user);
                await DataBase.SetUserVerificationCodeUpdateSmsStamp(UserPhoneNumber, rInt);
                await DataBase.SetUserToekn(UserPhoneNumber, "0"); //5. clear the user token to 0
            }

            //6. success
            return ApiResponse.Success(Request, "sms sent");
        }


        /// <summary>
        /// get the verification coe and the phone number
        /// after verifying, will return the token
        /// </summary>
        /// <param name="InputForm"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        // POST /Users/login
        [HttpPost("/[controller]/login", Name = nameof(login))]
        public async Task<IActionResult> login([FromBody]LogInForm InputForm, CancellationToken ct)
        {
            string UserPhoneNumber;
            string UserVerificationCode;

            //validate
            if (InputForm.UserPhoneNumber == null)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }
            else
            {
                UserPhoneNumber = InputForm.UserPhoneNumber;
            }

            if (InputForm.UserVerificationCode == null)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }
            else
            {
                UserVerificationCode = InputForm.UserVerificationCode;
            }

            //1. get the user
            UserObject user = await DataBase.GetUserByUserName(UserPhoneNumber);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }

            if (user.userBlockedUntil != null)
            {
                double t = Math.Round(user.userBlockedUntil.Value.Subtract(DateTime.UtcNow).TotalSeconds, 0);
                if (user.userBlockedUntil.Value.Subtract(DateTime.UtcNow).TotalMilliseconds > 0)
                {
                    string t1 = "User blocked for " + t.ToString() + " seconds";
                    return ApiResponse.Error(Request, t1, t1, ApiError.Err_UserNotFound);
                }
            }

            if (user.pinCodeValidDueDate != null)
            {
                double t = Math.Round(user.pinCodeValidDueDate.Value.Subtract(DateTime.UtcNow).TotalSeconds, 0);
                if (user.pinCodeValidDueDate.Value.Subtract(DateTime.UtcNow).TotalMilliseconds > 0)
                {
                    string t1 = "Pin code not valid, 2 minutes passed";
                    return ApiResponse.Error(Request, t1, t1, ApiError.Err_UserNotFound);
                }
            }

            if (user.smsRetryCounter > 5)
            {
                await DataBase.SetUserpinCodeValidDueDate(UserPhoneNumber, null);
                await DataBase.SetUserBlockUntil(user.username, DateTime.UtcNow.AddMinutes(30));
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }

            if (user.verificationCode !=  UserVerificationCode)
            {
                user.smsRetryCounter += 1;
                await DataBase.SetUsersmsCounter(UserPhoneNumber, user.smsRetryCounter, DateTime.UtcNow);

                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }    

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }

            //2. update the token
            string new_token = (DateTime.Now.Ticks + user.createdAt.Ticks).ToString() + user.ObjectId;

            if (await DataBase.SetUserToekn(user.username, new_token) == false)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }

            await DataBase.SetUserpinCodeValidDueDate(UserPhoneNumber, null);
            await DataBase.SetUserBlockUntil(user.username, null);

            var result = new
            {
                token = new_token
            };

            //3. success
            return ApiResponse.Success(Request, result);
        }


        /// <summary>
        /// check user number and verification code, without updating the Token.
        /// </summary>
        /// <param name="InputForm"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        // POST /Users/login
        [HttpPost("/[controller]/login_NoTokenUpdate", Name = nameof(login_NoTokenUpdate))]
        public async Task<IActionResult> login_NoTokenUpdate([FromBody]LogInForm InputForm, CancellationToken ct)
        {
            string UserPhoneNumber;
            string UserVerificationCode;

            //validate
            if (InputForm.UserPhoneNumber == null)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }
            else
            {
                UserPhoneNumber = InputForm.UserPhoneNumber;
            }

            if (InputForm.UserVerificationCode == null)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }
            else
            {
                UserVerificationCode = InputForm.UserVerificationCode;
            }

            //1. get the user
            UserObject user = await DataBase.GetUserByUserName(UserPhoneNumber);

            //return not found if user not in the system
            if (user == null)
            {
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }

            if (user.userBlockedUntil != null)
            {
                double t = Math.Round(user.userBlockedUntil.Value.Subtract(DateTime.UtcNow).TotalSeconds, 0);
                if (user.userBlockedUntil.Value.Subtract(DateTime.UtcNow).TotalMilliseconds > 0)
                {
                    string t1 = "User blocked for " + t.ToString() + " seconds";
                    return ApiResponse.Error(Request, t1, t1, ApiError.Err_UserNotFound);
                }
            }

            if (user.smsRetryCounter > 5)
            {
                await DataBase.SetUserpinCodeValidDueDate(UserPhoneNumber, null);
                await DataBase.SetUserBlockUntil(user.username, DateTime.UtcNow.AddMinutes(30));
                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }

            if (user.verificationCode != UserVerificationCode)
            {
                user.smsRetryCounter += 1;
                await DataBase.SetUsersmsCounter(UserPhoneNumber, user.smsRetryCounter, DateTime.UtcNow);

                return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
            }

            if (user.token == null)
            {
                //2. update the token
                string new_token = (DateTime.Now.Ticks + user.createdAt.Ticks).ToString() + user.ObjectId;

                if (await DataBase.SetUserToekn(user.username, new_token) == false)
                {
                    return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
                }

                user.token = new_token;
            }

            if (user.token == "")
            {
                //2. update the token
                string new_token = (DateTime.Now.Ticks + user.createdAt.Ticks).ToString() + user.ObjectId;

                if (await DataBase.SetUserToekn(user.username, new_token) == false)
                {
                    return ApiResponse.Error(Request, "User error, try again later", "User error, try again later", ApiError.Err_UserNotFound);
                }

                user.token = new_token;
            }

            await DataBase.SetUserpinCodeValidDueDate(UserPhoneNumber, null);
            await DataBase.SetUserBlockUntil(user.username, null);

            var result = new
            {
                token = user.token
            };

            //3. success
            return ApiResponse.Success(Request, result);
        }

        /// <summary>
        /// get the token, return "token listed, OK" with status 200,
        /// return "token fail" with status 500.
        /// </summary>
        /// <param name="InputForm"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        // POST /Users/login
        [HttpPost("/[controller]/tokencheck", Name = nameof(tokencheck))]
        public async Task<IActionResult> tokencheck([FromBody]ToeknCheckForm InputForm, CancellationToken ct)
        {
            string token;

            //1. validate
            if (InputForm.token == null)
            {
                return ApiResponse.Error(Request, "token error, try again later", "token error, try again later", ApiError.Err_UserNotFound);
            }
            else
            {
                token = InputForm.token;
            }

            if (token == "0")
            {
                return ApiResponse.Error(Request, "token error, try again later", "token error, try again later", ApiError.Err_UserNotFound);
            }

            //2. check the token
            UserObject user = await DataBase.VerifyUserToekn(token);

            if (user == null)
            {
                return ApiResponse.Error(Request, "token error, try again later", "token error, try again later", ApiError.Err_UserNotFound);
            }

            //3. success
            return ApiResponse.Success(Request, "token listed, OK");
        }


#warning "need to fix Parse compatibility issue"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="InputForm"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        // POST /Users/login
        [HttpPost("/[controller]/registernewuser", Name = nameof(RegisterNewUser))]
        public IActionResult RegisterNewUser([FromBody]RegisterNewUserForm InputForm, CancellationToken ct)
        {
            /*
            string token;

            //1. validate
            if (InputForm.token == null)
            {
                return ApiResponse.Error(Request, "token missing", "function must input token", ApiError.Err_MissingParameter);
            }
            else
            {
                token = InputForm.token;
            }

            if (InputForm.AccessLevel == null)
            {
                return ApiResponse.Error(Request, "AccessLevel missing", "function must input AccessLevel", ApiError.Err_MissingParameter);
            }
            if (InputForm.email == null)
            {
                return ApiResponse.Error(Request, "email missing", "function must input email", ApiError.Err_MissingParameter);
            }
            if (InputForm.name == null)
            {
                return ApiResponse.Error(Request, "name missing", "function must input name", ApiError.Err_MissingParameter);
            }
            if (InputForm.username == null)
            {
                return ApiResponse.Error(Request, "username missing", "function must input username", ApiError.Err_MissingParameter);
            }


            if (InputForm.username.Length< 5)
            {
                return ApiResponse.Error(Request, "username is not good phone number", "username is not good phone number", ApiError.Err_MissingParameter);
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

            UserDataObject new_user = new UserDataObject();
            new_user.name = InputForm.name;
            new_user.username = InputForm.username;
            new_user.email = InputForm.email;
            new_user.AccessLevel = InputForm.AccessLevel;

            // check if user is not already registered
            UserObject j = await DataBase.GetUserByUserName(new_user.username);

            if (j != null)
            {
                return ApiResponse.Error(Request, "User already signed", "user phone number already in the system", ApiError.Err_UserAlreadySign);
            }

            await DataBase.InsertNewUser(new_user);
            */

            //3. success
            return ApiResponse.Error(Request, "token error, try again later", "token error, try again later", ApiError.Err_UserNotFound);
        }


        [HttpPost("/[controller]/getallusers", Name = nameof(GetAllUsers))]
        public async Task<IActionResult> GetAllUsers([FromBody]GetAllUsersForm InputForm, CancellationToken ct)
        {
            /*
            string token;

            //1. validate
            if (InputForm.token == null)
            {
                return ApiResponse.Error(Request, "token missing", "function must input token", ApiError.Err_MissingParameter);
            }
            else
            {
                token = InputForm.token;
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

            // check if user is not already registered
            List<UserObject> allusers = await DataBase.GetAllUsers();

            if ((allusers == null) || (allusers.Count == 0))
            {
                return ApiResponse.Error(Request, "no users error", "no users error", ApiError.Err_Unknown);
            }

            //3. success
            return ApiResponse.Success(Request, allusers);
            */
            return ApiResponse.Error(Request, "token error, try again later", "token error, try again later", ApiError.Err_UserNotFound);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="InputForm"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        // POST /Users/login
        [HttpPost("/[controller]/deleteUser", Name = nameof(DeleteUser))]
        public async Task<IActionResult> DeleteUser([FromBody]DeleteUserForm InputForm, CancellationToken ct)
        {
            /*
            string token;

            //1. validate
            if (InputForm.token == null)
            {
                return ApiResponse.Error(Request, "token missing", "function must input token", ApiError.Err_MissingParameter);
            }
            else
            {
                token = InputForm.token;
            }

            if (InputForm.deleteObjId == null)
            {
                return ApiResponse.Error(Request, "deleteObjId missing", "function must input deleteObjId", ApiError.Err_MissingParameter);
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

            UserObject del_user = await DataBase.GetUserByUserObjID(InputForm.deleteObjId);

            if (del_user == null)
            {
                return ApiResponse.Error(Request, "user to delete was not found", "there is no such user in the system", ApiError.Err_NoToken);
            }

            //TODO:FUNCTION TO DELETE THE USER
            bool res = await DataBase.RemoveUserAndAllHisGates(del_user);
            */
            //3. success
            return ApiResponse.Error(Request, "token error, try again later", "token error, try again later", ApiError.Err_UserNotFound);
        }
    }
}
