using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;
using System.Security;
using BeAllCore.Objects;
using System.Threading.Tasks;
using System.Linq;

namespace BeAllCore
{
    public static class DataBase
    {
        private static string MlabDatabase = "beall";
        //public static string MlabConnection = "mongodb://beall:cswHelvonO0DbSMjE4NexW1oGJeUcf0ubLoojIWT4ncAEBLfzxhngjhC4OOOvvNTooRxelh84YZCL2S2Xcu4SA==@beall.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@beall@";

        //for DEV
        public static string MlabConnection = "mongodb://beall-dev-db:PTUp1rbBPvKUnsxvpxQpO7DRmSlIfWLYrlpMKaROw0TAcjHgM0630e3bWnoXss9lwlSCqAgylH7JACDbynNWZA==@beall-dev-db.mongo.cosmos.azure.com:10255/?ssl=true&retrywrites=false&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@beall-dev-db@";




        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async static Task<List<string>> GetAllGates()
        {
            var client = new MongoClient(MlabConnection);
            var database = client.GetDatabase(MlabDatabase);
            var collection = database.GetCollection<BsonDocument>("Gate");
            var filter = Builders<BsonDocument>.Filter;

            var Cresult = await collection.FindAsync(new BsonDocument());
            List<BsonDocument> doc = await Cresult.ToListAsync();

            var lst = new List<string>();

            if (doc != null)
            {
                for (int i = 0; i < doc.Count; i++)
                {
                    BsonElement tmp;
                    if (doc[i].TryGetElement("GateID", out tmp) == true)
                    {
                        lst.Add(tmp.Value.ToString());
                    }
                }
            }


            return lst;
        }


        public static async Task<string> DeleteOpenEvents()
        {
            DateTime a = DateTime.Now;
            var client = new MongoClient(MlabConnection);
            var database = client.GetDatabase(MlabDatabase);
            var collection = database.GetCollection<BsonDocument>("OpeningEvent");
            var builder = Builders<BsonDocument>.Filter;
            var filter1 = builder.Lte("_created_at", new DateTime(2020, 1, 1));

            var results = await collection.DeleteManyAsync(filter1);


            long c = results.DeletedCount;
            DateTime b = DateTime.Now;

            return "deleted " + c.ToString() + " took: " + b.Subtract(a).TotalSeconds + " seconds";
        }


        #region "Helpers"
        public static string GetStringVal(string Column, BsonDocument document)
        {
            BsonElement _ID;
            if (document.TryGetElement(Column, out _ID))
            {
                return _ID.Value.ToString().Replace("</br>", "\r\n");
            }
            else
            {
                return "";
            }
        }

        #endregion

        #region "BackOffice"

        [SecuritySafeCritical]
        public static List<BsonDocument> CheckUserLogin(string user, string verificationCode)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("username", user) & builder.Eq("verificationCode", Convert.ToInt64(verificationCode));

                return collection.Find(filter).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [SecuritySafeCritical]
        public static BsonDocument GetGateUserJoinTableById(string objID)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");
                var filter = Builders<BsonDocument>.Filter.Eq("_id", objID);
                var Cresult = collection.Find(filter).FirstOrDefault();

                return Cresult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [SecuritySafeCritical]
        public static List<BsonDocument> GetGatesOfUser(string uobjectID)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");
                var filter = Builders<BsonDocument>.Filter.Eq("_p_user", "_User$" + uobjectID);

                return collection.Find(filter).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetGateNameFromGateTable(string GateID)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var filter = Builders<BsonDocument>.Filter.Eq("_id", GateID);

                var Cresult = collection.Find(filter).ToList();

                BsonElement GateName2;
                Cresult[0].TryGetElement("name", out GateName2);
                string GateName = "";
                if (GateName2.Value != null)
                    GateName = GateName2.Value.ToString();
                else
                    GateName = "[No Name]";

                return GateName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static long GetUserCountPerGate(string GateID)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");
                var filter2 = Builders<BsonDocument>.Filter.Eq("_p_gate", "Gate$" + GateID);
                return collection.Count(filter2);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public static void SetNewUserName(string UserID, string NewUserName)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("_id", UserID);
                var update = Builders<BsonDocument>.Update
                .Set("name", NewUserName);

                collection.UpdateOne(filter, update);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region "Core"

        public static async Task<List<UserDataObject>> GetUsersByID(List<string> objId)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("_id", objId[0]);
                try
                {
                    filter |= Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(objId[0]));
                }
                catch { }

                for (int i = 1; i < objId.Count; i++)
                {
                    filter |= builder.Eq("_id", objId[i]);
                    try
                    {
                        filter |= Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(objId[i]));
                    }
                    catch { }
                }

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();


                List<UserDataObject> users = new List<UserDataObject>();
                if (doc != null)
                {
                    for (int i = 0; i < doc.Count; i++)
                    {
                        UserDataObject user = new UserDataObject();
                        BsonElement tmp;
                        if (doc[i].TryGetElement("_id", out tmp) == true)
                        {
                            user.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("username", out tmp) == true)
                        {
                            user.username = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("name", out tmp) == true)
                        {
                            user.name = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("AccessLevel", out tmp) == true)
                        {
                            user.AccessLevel = "";

                            var A = tmp.Value.AsBsonArray;
                            foreach (BsonValue s in A)
                            {
                                user.AccessLevel += s.AsString + ",";
                            }
                            if (user.AccessLevel.Length > 1)
                                user.AccessLevel = user.AccessLevel.Remove(user.AccessLevel.Length - 1, 1);
                        }
                        if (doc[i].TryGetElement("_updated_at", out tmp) == true)
                        {
                            user.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("isNew", out tmp) == true)
                        {
                            user.isNew = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("_created_at", out tmp) == true)
                        {
                            user.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("email", out tmp) == true)
                        {
                            user.email = tmp.Value.ToString();
                        }

                        users.Add(user);
                    }
                }
                else
                {
                    users = null;
                }

                return users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<UserObject> GetUserByUserName(string userName)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("username", userName);


                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                UserObject user = new UserObject();

                if (doc != null)
                {
                    if (doc.Count == 1)
                    {
                        BsonElement tmp;
                        if (doc[0].TryGetElement("_id", out tmp) == true)
                        {
                            user.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("emailVerified", out tmp) == true)
                        {
                            user.emailVerified = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("verificationCode", out tmp) == true)
                        {
                            user.verificationCode = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("username", out tmp) == true)
                        {
                            user.username = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("name", out tmp) == true)
                        {
                            user.name = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("AccessLevel", out tmp) == true)
                        {
                            user.AccessLevel = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("_updated_at", out tmp) == true)
                        {
                            user.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("isNew", out tmp) == true)
                        {
                            user.isNew = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("_created_at", out tmp) == true)
                        {
                            user.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("email", out tmp) == true)
                        {
                            user.email = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("token", out tmp) == true)
                        {
                            user.token = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("lastSmsSent", out tmp) == true)
                        {
                            user.lastSmsSent = tmp.Value.ToLocalTime();
                        }

                        if (doc[0].TryGetElement("userBlockedUntil", out tmp) == true)
                        {
                            user.userBlockedUntil = tmp.Value.ToLocalTime();
                        }
                        else
                        {
                            user.userBlockedUntil = null;
                        }

                        if (doc[0].TryGetElement("smsTimeStamps", out tmp))
                        {
                            if (tmp.Value.IsBsonArray)
                            {
                                user.smsTimeStamps = tmp.Value.AsBsonArray
                                    .Select(v => v.ToUniversalTime())
                                    .ToList();
                            }
                            else
                            {
                                user.smsTimeStamps = null; // If not an array, treat as invalid or missing
                            }
                        }
                        else
                        {
                            user.smsTimeStamps = null;
                        }



                        if (doc[0].TryGetElement("pinCodeValidDueDate", out tmp) == true)
                        {
                            user.pinCodeValidDueDate = tmp.Value.ToLocalTime();
                        }
                        else
                        {
                            user.pinCodeValidDueDate = null;
                        }


                        if (doc[0].TryGetElement("smsRetryCounter", out tmp) == true)
                        {
                            user.smsRetryCounter = tmp.Value.ToInt32();
                        }
                    }
                    else
                    {
                        user = null;
                    }
                }
                else
                {
                    user = null;
                }

                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<List<UserObject>> GetUsersByUserName(List<string> userNames)
        {
            try
            {
                List<UserObject> users = new List<UserObject>();
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.AnyIn("username", userNames);


                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                

                if (doc != null)
                {
                   for(int i = 0;i<doc.Count; i++)
                    {
                        UserObject user = new UserObject();

                        BsonElement tmp;
                        if (doc[i].TryGetElement("_id", out tmp) == true)
                        {
                            user.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("emailVerified", out tmp) == true)
                        {
                            user.emailVerified = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("verificationCode", out tmp) == true)
                        {
                            user.verificationCode = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("username", out tmp) == true)
                        {
                            user.username = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("name", out tmp) == true)
                        {
                            user.name = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("AccessLevel", out tmp) == true)
                        {
                            user.AccessLevel = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("_updated_at", out tmp) == true)
                        {
                            user.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("isNew", out tmp) == true)
                        {
                            user.isNew = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("_created_at", out tmp) == true)
                        {
                            user.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("email", out tmp) == true)
                        {
                            user.email = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("token", out tmp) == true)
                        {
                            user.token = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("lastSmsSent", out tmp) == true)
                        {
                            user.lastSmsSent = tmp.Value.ToLocalTime();
                        }

                        if (doc[0].TryGetElement("userBlockedUntil", out tmp) == true)
                        {
                            user.userBlockedUntil = tmp.Value.ToLocalTime();
                        }
                        else
                        {
                            user.userBlockedUntil = null;
                        }

                        if (doc[0].TryGetElement("smsTimeStamps", out tmp))
                        {
                            if (tmp.Value.IsBsonArray)
                            {
                                user.smsTimeStamps = tmp.Value.AsBsonArray
                                    .Select(v => v.ToUniversalTime())
                                    .ToList();
                            }
                            else
                            {
                                user.smsTimeStamps = null; // If not an array, treat as invalid or missing
                            }
                        }
                        else
                        {
                            user.smsTimeStamps = null;
                        }


                        if (doc[0].TryGetElement("pinCodeValidDueDate", out tmp) == true)
                        {
                            user.pinCodeValidDueDate = tmp.Value.ToLocalTime();
                        }
                        else
                        {
                            user.pinCodeValidDueDate = null;
                        }


                        if (doc[0].TryGetElement("smsRetryCounter", out tmp) == true)
                        {
                            user.smsRetryCounter = tmp.Value.ToInt32();
                        }

                        users.Add(user);
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<UserObject> GetUserByUserObjID(string ObjID)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("_id", ObjID);
                try
                {
                    filter |= Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(ObjID));
                }
                catch { }

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                UserObject user = new UserObject();

                if (doc != null)
                {
                    if (doc.Count == 1)
                    {
                        BsonElement tmp;
                        if (doc[0].TryGetElement("_id", out tmp) == true)
                        {
                            user.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("emailVerified", out tmp) == true)
                        {
                            user.emailVerified = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("verificationCode", out tmp) == true)
                        {
                            user.verificationCode = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("username", out tmp) == true)
                        {
                            user.username = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("name", out tmp) == true)
                        {
                            user.name = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("AccessLevel", out tmp) == true)
                        {
                            user.AccessLevel = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("_updated_at", out tmp) == true)
                        {
                            user.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("isNew", out tmp) == true)
                        {
                            user.isNew = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("_created_at", out tmp) == true)
                        {
                            user.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("email", out tmp) == true)
                        {
                            user.email = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("token", out tmp) == true)
                        {
                            user.token = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("lastSmsSent", out tmp) == true)
                        {
                            user.lastSmsSent = tmp.Value.ToLocalTime();
                        }

                        if (doc[0].TryGetElement("userBlockedUntil", out tmp) == true)
                        {
                            user.userBlockedUntil = tmp.Value.ToLocalTime();
                        }
                        else
                        {
                            user.userBlockedUntil = null;
                        }

                        if (doc[0].TryGetElement("smsTimeStamps", out tmp))
                        {
                            if (tmp.Value.IsBsonArray)
                            {
                                user.smsTimeStamps = tmp.Value.AsBsonArray
                                    .Select(v => v.ToUniversalTime())
                                    .ToList();
                            }
                            else
                            {
                                user.smsTimeStamps = null; // If not an array, treat as invalid or missing
                            }
                        }
                        else
                        {
                            user.smsTimeStamps = null;
                        }


                        if (doc[0].TryGetElement("pinCodeValidDueDate", out tmp) == true)
                        {
                            user.pinCodeValidDueDate = tmp.Value.ToLocalTime();
                        }
                        else
                        {
                            user.pinCodeValidDueDate = null;
                        }


                        if (doc[0].TryGetElement("smsRetryCounter", out tmp) == true)
                        {
                            user.smsRetryCounter = tmp.Value.ToInt32();
                        }
                    }
                    else
                    {
                        user = null;
                    }
                }
                else
                {
                    user = null;
                }

                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<UserObject> GetUserByUserNameAndVerificationCode(string userName, string verificationCode)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("username", userName) & builder.Eq("verificationCode", Convert.ToInt64(verificationCode));

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                UserObject user = new UserObject();

                if (doc != null)
                {
                    if (doc.Count == 1)
                    {
                        BsonElement tmp;
                        if (doc[0].TryGetElement("_id", out tmp) == true)
                        {
                            user.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("emailVerified", out tmp) == true)
                        {
                            user.emailVerified = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("verificationCode", out tmp) == true)
                        {
                            user.verificationCode = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("username", out tmp) == true)
                        {
                            user.username = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("name", out tmp) == true)
                        {
                            user.name = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("AccessLevel", out tmp) == true)
                        {
                            user.AccessLevel = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("_updated_at", out tmp) == true)
                        {
                            user.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("isNew", out tmp) == true)
                        {
                            user.isNew = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("_created_at", out tmp) == true)
                        {
                            user.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("email", out tmp) == true)
                        {
                            user.email = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("token", out tmp) == true)
                        {
                            user.token = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("lastSmsSent", out tmp) == true)
                        {
                            user.lastSmsSent = tmp.Value.ToLocalTime();
                        }
                        
                        if (doc[0].TryGetElement("userBlockedUntil", out tmp) == true)
                        {
                            user.userBlockedUntil = tmp.Value.ToLocalTime();
                        }
                        else
                        {
                            user.userBlockedUntil = null;
                        }

                        if (doc[0].TryGetElement("smsTimeStamps", out tmp))
                        {
                            if (tmp.Value.IsBsonArray)
                            {
                                user.smsTimeStamps = tmp.Value.AsBsonArray
                                    .Select(v => v.ToUniversalTime())
                                    .ToList();
                            }
                            else
                            {
                                user.smsTimeStamps = null; // If not an array, treat as invalid or missing
                            }
                        }
                        else
                        {
                            user.smsTimeStamps = null;
                        }


                        if (doc[0].TryGetElement("pinCodeValidDueDate", out tmp) == true)
                        {
                            user.pinCodeValidDueDate = tmp.Value.ToLocalTime();
                        }
                        else
                        {
                            user.pinCodeValidDueDate = null;
                        }


                        if (doc[0].TryGetElement("smsRetryCounter", out tmp) == true)
                        {
                            user.smsRetryCounter = tmp.Value.ToInt32();
                        }
                    }
                    else
                    {
                        user = null;
                    }
                }
                else
                {
                    user = null;
                }

                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Set User Verification Code and Update Sms Stamp
        /// </summary>
        /// <param name="username">user full number</param>
        /// <param name="VerificationCode">new pin code</param>
        public static async Task<bool> SetUserVerificationCodeUpdateSmsStamp(string username, int VerificationCode)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("username", username);
                var update = Builders<BsonDocument>.Update
                .Set("verificationCode", VerificationCode).Set("lastSmsSent", DateTime.Now).Set("smsRetryCounter", 0);

                await collection.UpdateOneAsync(filter, update);
                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }
        }

        public static async Task<bool> SetUserpinCodeValidDueDate(string username, DateTime? pinCodeValidDueDate)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("username", username);
                var update = Builders<BsonDocument>.Update.Set("pinCodeValidDueDate", pinCodeValidDueDate);

                await collection.UpdateOneAsync(filter, update);
                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }
        }
        


        public static async Task<bool> SetUsersmsCounter(string username, int smsCounter, DateTime newStamp)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("username", username);
                var update = Builders<BsonDocument>.Update.Set("smsRetryCounter", smsCounter).Set("lastSmsSent", newStamp);

                await collection.UpdateOneAsync(filter, update);
                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }
        }


        public static async Task<bool> SetUserBlockUntil(string username, DateTime? newStamp)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("username", username);
                var update = Builders<BsonDocument>.Update.Set("userBlockedUntil", newStamp);

                await collection.UpdateOneAsync(filter, update);
                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }
        }
        


        public static async Task<bool> SetUserToekn(string username, string token)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("username", username);
                var update = Builders<BsonDocument>.Update
                .Set("token", token);

                await collection.UpdateOneAsync(filter, update);
                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns>UserObject only with username (number) and name</returns>
        public static async Task<UserObject> VerifyUserToekn(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            if (token.Length < 3)
            {
                return null;
            }

            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("token", token);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                UserObject user = new UserObject();

                if (doc != null)
                {
                    if (doc.Count == 1)
                    {
                        BsonElement tmp;
                        if (doc[0].TryGetElement("_id", out tmp) == true)
                        {
                            user.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("emailVerified", out tmp) == true)
                        {
                            user.emailVerified = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("verificationCode", out tmp) == true)
                        {
                            user.verificationCode = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("username", out tmp) == true)
                        {
                            user.username = tmp.Value.ToString();
                        }
                        else
                        {
                            user.username = "";
                        }
                        if (doc[0].TryGetElement("name", out tmp) == true)
                        {
                            user.name = tmp.Value.ToString();
                        }
                        else
                        {
                            user.name = "";
                        }

                        if (doc[0].TryGetElement("AccessLevel", out tmp) == true)
                        {
                            user.AccessLevel = tmp.Value.ToString();
                        }
                        else
                        {
                            user.AccessLevel = "";
                        }

                        if (doc[0].TryGetElement("_updated_at", out tmp) == true)
                        {
                            user.updatedAt = tmp.Value.ToUniversalTime();
                        }

                        if (doc[0].TryGetElement("isNew", out tmp) == true)
                        {
                            user.isNew = tmp.Value.ToBoolean();
                        }

                        if (doc[0].TryGetElement("_created_at", out tmp) == true)
                        {
                            user.createdAt = tmp.Value.ToUniversalTime();
                        }

                        if (doc[0].TryGetElement("email", out tmp) == true)
                        {
                            user.email = tmp.Value.ToString();
                        }
                        else
                        {
                            user.email = "";
                        }

                        if (doc[0].TryGetElement("token", out tmp) == true)
                        {
                            user.token = tmp.Value.ToString();
                        }
                        else
                        {
                            user.token = "";
                        }

                        if (doc[0].TryGetElement("lastSmsSent", out tmp) == true)
                        {
                            user.lastSmsSent = tmp.Value.ToLocalTime();
                        }

                        if (doc[0].TryGetElement("FavoriteGatesObjectID", out tmp) == true)
                        {
                            user.FavoriteGatesObjectID = tmp.Value.ToString();
                        }
                        else
                        {
                            user.FavoriteGatesObjectID = "";
                        }


                        if (doc[0].TryGetElement("userBlockedUntil", out tmp) == true)
                        {
                            user.userBlockedUntil = tmp.Value.ToLocalTime();
                        }
                        else
                        {
                            user.userBlockedUntil = null;
                        }

                        if (doc[0].TryGetElement("smsTimeStamps", out tmp))
                        {
                            if (tmp.Value.IsBsonArray)
                            {
                                user.smsTimeStamps = tmp.Value.AsBsonArray
                                    .Select(v => v.ToUniversalTime())
                                    .ToList();
                            }
                            else
                            {
                                user.smsTimeStamps = null; // If not an array, treat as invalid or missing
                            }
                        }
                        else
                        {
                            user.smsTimeStamps = null;
                        }

                        if (doc[0].TryGetElement("pinCodeValidDueDate", out tmp) == true)
                        {
                            user.pinCodeValidDueDate = tmp.Value.ToLocalTime();
                        }
                        else
                        {
                            user.pinCodeValidDueDate = null;
                        }


                        if (doc[0].TryGetElement("smsRetryCounter", out tmp) == true)
                        {
                            user.smsRetryCounter = tmp.Value.ToInt32();
                        }

                        return user;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                //throw ex;
                return null;
            }
        }

        public static async Task<List<OpeningEvent>> GetOpeningEventsPerGate(DateTime start_stamp, DateTime end_stamp, string identifier, UserObject user)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_OpeningEvent");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("gateIdentifier", identifier) & builder.Gte("ForcedDate", start_stamp) & builder.Lte("ForcedDate", end_stamp);

                List<OpeningEvent> events = new List<OpeningEvent>();

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    for (int i = 0; i < doc.Count; i++)
                    {
                        OpeningEvent ev = new OpeningEvent();

                        BsonElement tmp;

                        if (doc[0].TryGetElement("_id", out tmp) == true)
                        {
                            ev.ObjectId = tmp.Value.AsString;
                        }
                        if (doc[0].TryGetElement("ForcedDate", out tmp) == true)
                        {
                            ev.stamp = tmp.Value.AsDateTime;
                        }
                        if (doc[0].TryGetElement("didOpen", out tmp) == true)
                        {
                            ev.didOpen = tmp.Value.AsBoolean;
                        }
                        if (doc[0].TryGetElement("UserNumber", out tmp) == true)
                        {
                            ev.UserNumber = tmp.Value.AsString;
                        }

                        events.Add(ev);
                    }
                }

                return events;
            }
            catch (Exception ex)
            {
                //throw ex;
                return null;
            }
        }

        public static async Task<bool> InserOpeningEvent(DateTime stamp, string identifier, UserObject user)
        {
            try
            {
                var document = new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId()},
                    { "gateIdentifier", identifier},
                    { "didOpen", false},
                    { "username", user.username},
                    { "tries", 0},
                    { "isTest", false},
                    { "isAutoOpen", false},
                    { "success", true},
                    { "islocaltime", true },
                    { "_updated_at", stamp },
                    {"_created_at", stamp },
                    {"ForceDate", stamp }
                };

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("OpeningEvent");

                await collection.InsertOneAsync(document);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<List<GateUserJoinTableObject>> GetUserGatesTable(UserObject userName)
        {
            try
            {
                List<GateUserJoinTableObject> res = new List<GateUserJoinTableObject>();

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");
                var filter = Builders<BsonDocument>.Filter.Eq("_p_user", "_User$" + userName.ObjectId);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    for (int i = 0; i < doc.Count; i++)
                    {
                        GateUserJoinTableObject gujt = new GateUserJoinTableObject();

                        BsonElement tmp;
                        if (doc[i].TryGetElement("_id", out tmp) == true)
                        {
                            gujt.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("_updated_at", out tmp) == true)
                        {
                            gujt.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("_created_at", out tmp) == true)
                        {
                            gujt.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("_p_gate", out tmp) == true)
                        {
                            gujt._p_gate = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("_p_user", out tmp) == true)
                        {
                            gujt.user = tmp.Value.ToString();
                        }
                        //if (doc[i].TryGetElement("_p_location", out tmp) == true)
                        //{
                        //    gujt.location = tmp.Value.ToString();
                        //}
                        gujt.location = "";
                        if (doc[i].TryGetElement("name", out tmp) == true)
                        {
                            gujt.name = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("isAdmin", out tmp) == true)
                        {
                            gujt.isAdmin = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("autoOpen", out tmp) == true)
                        {
                            gujt.autoOpen = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("showNotification", out tmp) == true)
                        {
                            gujt.showNotification = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("DueDate", out tmp) == true)
                        {
                            gujt.DueDate = tmp.Value.ToUniversalTime();
                        }
                        //if (doc[i].TryGetElement("ProductionCode", out tmp) == true)
                        //{
                        //    gujt.ProductionCode = tmp.Value.ToString();
                        //}
                        gujt.ProductionCode = "asdasd";
                        if (doc[i].TryGetElement("GateID", out tmp) == true)
                        {
                            gujt.GateID = tmp.Value.ToString();
                        }

                        res.Add(gujt);
                    }
                }
                else
                {
                    res = null;
                }

                List<GateUserJoinTableObject> res2 = await FillGatesToList(res);


                return res2;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<List<GateUserJoinTableObject>> GetUsersGatesTable(List<UserObject> users)
        {
            try
            {
                var usersIds = new List<string>();
                
                foreach (UserObject u in users)
                {
                    string s = u.ObjectId;
                    if (s.Contains("_User$") == false)
                    {
                        s = "_User$" + s;
                    }

                    usersIds.Add(s);
                }
                
                List<GateUserJoinTableObject> res = new List<GateUserJoinTableObject>();

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");
                var filter = Builders<BsonDocument>.Filter.AnyIn("_p_user", usersIds);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    for (int i = 0; i < doc.Count; i++)
                    {
                        GateUserJoinTableObject gujt = new GateUserJoinTableObject();

                        BsonElement tmp;
                        if (doc[i].TryGetElement("_id", out tmp) == true)
                        {
                            gujt.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("_updated_at", out tmp) == true)
                        {
                            gujt.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("_created_at", out tmp) == true)
                        {
                            gujt.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("_p_gate", out tmp) == true)
                        {
                            gujt._p_gate = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("_p_user", out tmp) == true)
                        {
                            gujt.user = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("_p_location", out tmp) == true)
                        {
                            gujt.location = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("name", out tmp) == true)
                        {
                            gujt.name = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("isAdmin", out tmp) == true)
                        {
                            gujt.isAdmin = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("autoOpen", out tmp) == true)
                        {
                            gujt.autoOpen = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("showNotification", out tmp) == true)
                        {
                            gujt.showNotification = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("DueDate", out tmp) == true)
                        {
                            gujt.DueDate = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("ProductionCode", out tmp) == true)
                        {
                            gujt.ProductionCode = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("GateID", out tmp) == true)
                        {
                            gujt.GateID = tmp.Value.ToString();
                        }

                        res.Add(gujt);
                    }
                }
                else
                {
                    res = null;
                }

                List<GateUserJoinTableObject> res2 = await FillGatesToList(res);
                return res2;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<List<string>> GetUserGatesByGateId(List<string> gatesId)
        {
            try
            {
                var usersgatesIds = new List<string>();
                List<string> new_gatesId = new List<string>();

                List<GateUserJoinTableObject> res = new List<GateUserJoinTableObject>();

                foreach(string s in gatesId)
                {
                    if(s.Contains("Gate$") == false)
                    {
                        new_gatesId.Add("Gate$" + s);
                    }
                    else
                    {
                        new_gatesId.Add(s);
                    }
                }


                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");
                var filter = Builders<BsonDocument>.Filter.In("_p_gate", new_gatesId);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    for (int i = 0; i < doc.Count; i++)
                    {
                        GateUserJoinTableObject gujt = new GateUserJoinTableObject();

                        BsonElement tmp;
                        if (doc[i].TryGetElement("_id", out tmp) == true)
                        {
                            usersgatesIds.Add(tmp.Value.ToString());
                        }
                    }
                }

                return usersgatesIds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<List<GateUserJoinTableObject>> FillGatesToList(List<GateUserJoinTableObject> gateList)
        {
            List<GateUserJoinTableObject> res = new List<GateUserJoinTableObject>();

            if (gateList == null)
            {
                return res;
            }

            if (gateList.Count == 0)
            {
                return res;
            }

            for (int i = 0; i < gateList.Count; i++)
            {
                res.Add(gateList[i]);
            }

            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var filter = Builders<BsonDocument>.Filter.Eq("_id", res[0]._p_gate.Replace("Gate$", ""));

                for (int i = 1; i < res.Count; i++)
                {
                    filter |= Builders<BsonDocument>.Filter.Eq("_id", res[i]._p_gate.Replace("Gate$", ""));
                }

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    for (int i = 0; i < doc.Count; i++)
                    {
                        int g_index = 0;
                        string ObjectId = "";
                        BsonElement tmp;

                        if (doc[i].TryGetElement("_id", out tmp) == true)
                        {
                            ObjectId = tmp.Value.ToString();
                        }

                        if (ObjectId != "")
                        {
                            for (int ii = 0; ii < res.Count; ii++)
                            {
                                if (ObjectId == res[ii]._p_gate.Replace("Gate$", ""))
                                {
                                    g_index = ii;
                                    res[ii].gate = new GateObject();
                                    res[ii].gate.ObjectId = ObjectId;
                                    ii = res.Count;


                                    if (doc[i].TryGetElement("_updated_at", out tmp) == true)
                                    {
                                        res[g_index].gate.updatedAt = tmp.Value.ToUniversalTime();
                                    }
                                    if (doc[i].TryGetElement("_created_at", out tmp) == true)
                                    {
                                        res[g_index].gate.createdAt = tmp.Value.ToUniversalTime();
                                    }
                                    if (doc[i].TryGetElement("identifier", out tmp) == true)
                                    {
                                        res[g_index].gate.identifier = tmp.Value.ToString();
                                    }
                                    if (doc[i].TryGetElement("name", out tmp) == true)
                                    {
                                        res[g_index].gate.name = tmp.Value.ToString();
                                    }
                                    if (doc[i].TryGetElement("latitude", out tmp) == true)
                                    {
                                        res[g_index].gate.latitude = tmp.Value.ToDouble();
                                    }
                                    if (doc[i].TryGetElement("longitude", out tmp) == true)
                                    {
                                        res[g_index].gate.longitude = tmp.Value.ToDouble();
                                    }
                                    if (doc[i].TryGetElement("ProductionCode", out tmp) == true)
                                    {
                                        res[g_index].gate.ProductionCode = tmp.Value.ToString();
                                    }
                                    if (doc[i].TryGetElement("WasFree", out tmp) == true)
                                    {
                                        res[g_index].gate.WasFree = tmp.Value.ToBoolean();
                                    }
                                    if (doc[i].TryGetElement("GlobalUntil", out tmp) == true)
                                    {
                                        res[g_index].gate.GlobalUntil = tmp.Value.ToUniversalTime();
                                    }
                                    if (doc[i].TryGetElement("DueDate", out tmp) == true)
                                    {
                                        res[g_index].gate.DueDate = tmp.Value.ToUniversalTime();
                                    }
                                    if (doc[i].TryGetElement("Subscription", out tmp) == true)
                                    {
                                        res[g_index].gate.Subscription = tmp.Value.ToInt32();
                                    }
                                    if (doc[i].TryGetElement("Subscription", out tmp) == true)
                                    {
                                        res[g_index].gate.Subscription = tmp.Value.ToInt32();
                                    }
                                    if (doc[i].TryGetElement("UsersLimit", out tmp) == true)
                                    {
                                        res[g_index].gate.UsersLimit = tmp.Value.ToInt32();
                                    }
                                    res[g_index].gate.ReSellerID = "";
                                    if (doc[i].TryGetElement("address", out tmp) == true)
                                    {
                                        res[g_index].gate.address = tmp.Value.ToString();
                                    }
                                    if (doc[i].TryGetElement("GateID", out tmp) == true)
                                    {
                                        res[g_index].gate.GateID = tmp.Value.ToString();
                                    }

                                    // split GateID to gate info IL-TLV-Alon-L4-PO-87
                                    try
                                    {
                                        string[] spl = res[g_index].gate.GateID.Split("-");
                                        if (spl.Length == 6)
                                        {
                                            if (spl[0] == "IL")
                                                res[g_index].gate.Loc_Country = "Israel";
                                            else
                                                res[g_index].gate.Loc_Country = spl[0];

                                            if (spl[1] == "TLV")
                                                res[g_index].gate.Loc_City = "Tel Aviv";
                                            else if (spl[1] == "GVT")
                                                res[g_index].gate.Loc_City = "Givataim";
                                            else
                                                res[g_index].gate.Loc_Country = spl[1];

                                            res[g_index].gate.Loc_Building = spl[2];

                                            res[g_index].gate.Loc_Level = spl[3].Replace("L", "");

                                            res[g_index].gate.Loc_Room = spl[5];


                                            short number;
                                            bool result = Int16.TryParse(spl[5], out number);
                                            if (result == true)
                                            {
                                                res[g_index].gate.Loc_Range = (Convert.ToInt16((Convert.ToInt16(spl[5]) / 10)) * 10).ToString() + "-" + ((Convert.ToInt16((Convert.ToInt16(spl[5]) / 10)) * 10) + 9).ToString();
                                            }
                                            else
                                            {
                                                res[g_index].gate.Loc_Range = spl[5];
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        Console.WriteLine("error");
                                    }
                                }
                            }
                        }
                    }


                    for (int g_index = 0; g_index < res.Count; g_index++)
                    {
                        if (res[g_index].gate == null)
                        {
                            Console.WriteLine(res[g_index].ObjectId);
                        }
                    }
                            /*
                            for(int g_index=0; g_index<res.Count; g_index++)
                            {
                                if (res[g_index].gate == null)
                                {
                                    Console.WriteLine("error!");

                                    for (int i = 0; i < doc.Count; i++)
                                    {
                                        bool found = false;
                                        string ObjectId = "";
                                        BsonElement tmp;

                                        found = false;

                                        for (int ii = 0; ii < res.Count; ii++)
                                        {
                                            for (int i = 0; i < doc.Count; i++)
                                            {
                                                g_index = ii;
                                                res[ii].gate = new GateObject();
                                                res[ii].gate.ObjectId = ObjectId;
                                                ii = res.Count;
                                                found = true;
                                            }
                                        }

                                        if (found == false)
                                        {
                                            Console.WriteLine("wqeqwe");
                                        }

                                        if (found == true)
                                        {
                                            if (doc[i].TryGetElement("_updated_at", out tmp) == true)
                                            {
                                                res[g_index].gate.updatedAt = tmp.Value.ToUniversalTime();
                                            }
                                            if (doc[i].TryGetElement("_created_at", out tmp) == true)
                                            {
                                                res[g_index].gate.createdAt = tmp.Value.ToUniversalTime();
                                            }
                                            if (doc[i].TryGetElement("identifier", out tmp) == true)
                                            {
                                                res[g_index].gate.identifier = tmp.Value.ToString();
                                            }
                                            if (doc[i].TryGetElement("name", out tmp) == true)
                                            {
                                                res[g_index].gate.name = tmp.Value.ToString();
                                            }
                                            if (doc[i].TryGetElement("latitude", out tmp) == true)
                                            {
                                                res[g_index].gate.latitude = tmp.Value.ToDouble();
                                            }
                                            if (doc[i].TryGetElement("longitude", out tmp) == true)
                                            {
                                                res[g_index].gate.longitude = tmp.Value.ToDouble();
                                            }
                                            if (doc[i].TryGetElement("ProductionCode", out tmp) == true)
                                            {
                                                res[g_index].gate.ProductionCode = tmp.Value.ToString();
                                            }
                                            if (doc[i].TryGetElement("WasFree", out tmp) == true)
                                            {
                                                res[g_index].gate.WasFree = tmp.Value.ToBoolean();
                                            }
                                            if (doc[i].TryGetElement("GlobalUntil", out tmp) == true)
                                            {
                                                res[g_index].gate.GlobalUntil = tmp.Value.ToUniversalTime();
                                            }
                                            if (doc[i].TryGetElement("DueDate", out tmp) == true)
                                            {
                                                res[g_index].gate.DueDate = tmp.Value.ToUniversalTime();
                                            }
                                            if (doc[i].TryGetElement("Subscription", out tmp) == true)
                                            {
                                                res[g_index].gate.Subscription = tmp.Value.ToInt32();
                                            }
                                            if (doc[i].TryGetElement("Subscription", out tmp) == true)
                                            {
                                                res[g_index].gate.Subscription = tmp.Value.ToInt32();
                                            }
                                            if (doc[i].TryGetElement("UsersLimit", out tmp) == true)
                                            {
                                                res[g_index].gate.UsersLimit = tmp.Value.ToInt32();
                                            }
                                            //if (doc[i].TryGetElement("ReSellerID", out tmp) == true)
                                            //{
                                            //    res[g_index].gate.ReSellerID = tmp.Value.ToString();
                                            //}
                                            res[g_index].gate.ReSellerID = "";
                                            if (doc[i].TryGetElement("address", out tmp) == true)
                                            {
                                                res[g_index].gate.address = tmp.Value.ToString();
                                            }
                                            if (doc[i].TryGetElement("GateID", out tmp) == true)
                                            {
                                                res[g_index].gate.GateID = tmp.Value.ToString();
                                            }

                                            // split GateID to gate info IL-TLV-Alon-L4-PO-87
                                            try
                                            {
                                                string[] spl = res[g_index].gate.GateID.Split("-");
                                                if (spl.Length == 6)
                                                {
                                                    if (spl[0] == "IL")
                                                        res[g_index].gate.Loc_Country = "Israel";
                                                    else
                                                        res[g_index].gate.Loc_Country = spl[0];

                                                    if (spl[1] == "TLV")
                                                        res[g_index].gate.Loc_City = "Tel Aviv";
                                                    else if (spl[1] == "GVT")
                                                        res[g_index].gate.Loc_City = "Givataim";
                                                    else
                                                        res[g_index].gate.Loc_Country = spl[1];

                                                    res[g_index].gate.Loc_Building = spl[2];

                                                    res[g_index].gate.Loc_Level = spl[3].Replace("L", "");

                                                    res[g_index].gate.Loc_Room = spl[5];


                                                    short number;
                                                    bool result = Int16.TryParse(spl[5], out number);
                                                    if (result == true)
                                                    {
                                                        res[g_index].gate.Loc_Range = (Convert.ToInt16((Convert.ToInt16(spl[5]) / 10)) * 10).ToString() + "-" + ((Convert.ToInt16((Convert.ToInt16(spl[5]) / 10)) * 10) + 9).ToString();
                                                    }
                                                    else
                                                    {
                                                        res[g_index].gate.Loc_Range = spl[5];
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                Console.WriteLine("error");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("error");
                                        }
                                    }
                                }
                            }
                            */
                        }

                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<GateObject> GetGateByID(string GateID)
        {
            try
            {
                if (GateID.Contains("Gate$"))
                    GateID = GateID.Replace("Gate$", "");

                GateObject gate = new GateObject();

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var filter = Builders<BsonDocument>.Filter.Eq("_id", GateID);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    if (doc.Count == 1)
                    {
                        BsonElement tmp;
                        if (doc[0].TryGetElement("_id", out tmp) == true)
                        {
                            gate.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("_updated_at", out tmp) == true)
                        {
                            gate.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("_created_at", out tmp) == true)
                        {
                            gate.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("identifier", out tmp) == true)
                        {
                            gate.identifier = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("name", out tmp) == true)
                        {
                            gate.name = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("latitude", out tmp) == true)
                        {
                            gate.latitude = tmp.Value.ToDouble();
                        }
                        if (doc[0].TryGetElement("longitude", out tmp) == true)
                        {
                            gate.longitude = tmp.Value.ToDouble();
                        }
                        if (doc[0].TryGetElement("ProductionCode", out tmp) == true)
                        {
                            gate.ProductionCode = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("WasFree", out tmp) == true)
                        {
                            gate.WasFree = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("GlobalUntil", out tmp) == true)
                        {
                            gate.GlobalUntil = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("DueDate", out tmp) == true)
                        {
                            gate.DueDate = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("Subscription", out tmp) == true)
                        {
                            gate.Subscription = tmp.Value.ToInt32();
                        }
                        if (doc[0].TryGetElement("Subscription", out tmp) == true)
                        {
                            gate.Subscription = tmp.Value.ToInt32();
                        }
                        if (doc[0].TryGetElement("UsersLimit", out tmp) == true)
                        {
                            gate.UsersLimit = tmp.Value.ToInt32();
                        }
                        //if (doc[0].TryGetElement("ReSellerID", out tmp) == true)
                        //{
                        //    gate.ReSellerID = tmp.Value.ToString();
                        //}
                        gate.ReSellerID = "";
                    }
                }

                return gate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<GateObject> GetGateByIdentifier(string GateIdentifier)
        {
            try
            {
                GateObject gate = new GateObject();

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var filter = Builders<BsonDocument>.Filter.Eq("identifier", GateIdentifier);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    if (doc.Count == 1)
                    {
                        BsonElement tmp;
                        if (doc[0].TryGetElement("_id", out tmp) == true)
                        {
                            gate.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("_updated_at", out tmp) == true)
                        {
                            gate.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("_created_at", out tmp) == true)
                        {
                            gate.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("identifier", out tmp) == true)
                        {
                            gate.identifier = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("name", out tmp) == true)
                        {
                            gate.name = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("latitude", out tmp) == true)
                        {
                            gate.latitude = tmp.Value.ToDouble();
                        }
                        if (doc[0].TryGetElement("longitude", out tmp) == true)
                        {
                            gate.longitude = tmp.Value.ToDouble();
                        }
                        if (doc[0].TryGetElement("ProductionCode", out tmp) == true)
                        {
                            gate.ProductionCode = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("WasFree", out tmp) == true)
                        {
                            gate.WasFree = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("GlobalUntil", out tmp) == true)
                        {
                            gate.GlobalUntil = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("DueDate", out tmp) == true)
                        {
                            gate.DueDate = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("Subscription", out tmp) == true)
                        {
                            gate.Subscription = tmp.Value.ToInt32();
                        }
                        if (doc[0].TryGetElement("Subscription", out tmp) == true)
                        {
                            gate.Subscription = tmp.Value.ToInt32();
                        }
                        if (doc[0].TryGetElement("UsersLimit", out tmp) == true)
                        {
                            gate.UsersLimit = tmp.Value.ToInt32();
                        }
                        if (doc[0].TryGetElement("ReSellerID", out tmp) == true)
                        {
                            gate.ReSellerID = tmp.Value.ToString();
                        }

                        return gate;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<List<GateObject2>> GetAllGatesFull()
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");

                var Cresult = await collection.FindAsync(new BsonDocument());
                List<BsonDocument> doc = await Cresult.ToListAsync();


                var results = new List<GateObject2>();

                if (doc != null)
                {
                    foreach (var d in doc)
                    {
                        BsonElement tmp;
                        GateObject2 gate = new GateObject2();

                        if (d.TryGetElement("_created_at", out tmp) == true)
                        {
                            gate.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (d.TryGetElement("identifier", out tmp) == true)
                        {
                            gate.identifier = tmp.Value.ToString();
                        }
                        if (d.TryGetElement("name", out tmp) == true)
                        {
                            gate.name = tmp.Value.ToString();
                        }
                        if (d.TryGetElement("GateID", out tmp) == true)
                        {
                            gate.GateID = tmp.Value.ToString();
                        }
                        if (d.TryGetElement("address", out tmp) == true)
                        {
                            gate.address = tmp.Value.ToString();
                        }
                        if (d.TryGetElement("AccessLevel", out tmp) == true)
                        {
                            gate.AccessLevel = tmp.Value.ToString();
                        }

                        results.Add(gate);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<List<GateObject2>> GetAllGatesWithAl(string accessLevel)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var _accessLevel = new List<string>();
                _accessLevel.Add(accessLevel);
                var filter = Builders<BsonDocument>.Filter.AnyIn("AccessLevel", _accessLevel);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                var results = new List<GateObject2>();

                if (doc != null)
                {
                    foreach (var d in doc)
                    {
                        BsonElement tmp;
                        GateObject2 gate = new GateObject2();

                        if (d.TryGetElement("identifier", out tmp) == true)
                        {
                            gate.identifier = tmp.Value.ToString();
                        }
                        if (d.TryGetElement("name", out tmp) == true)
                        {
                            gate.name = tmp.Value.ToString();
                        }
                        if (d.TryGetElement("GateID", out tmp) == true)
                        {
                            gate.GateID = tmp.Value.ToString();
                        }
                        if (d.TryGetElement("address", out tmp) == true)
                        {
                            gate.address = tmp.Value.ToString();
                        }
                        if (d.TryGetElement("AccessLevel", out tmp) == true)
                        {
                            gate.AccessLevel = tmp.Value.ToString();
                        }

                        results.Add(gate);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<GateObject> GetGateByGateID(string GateID)
        {
            try
            {
                GateObject gate = new GateObject();

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var filter = Builders<BsonDocument>.Filter.Eq("GateID", GateID);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    if (doc.Count == 1)
                    {
                        BsonElement tmp;
                        if (doc[0].TryGetElement("_id", out tmp) == true)
                        {
                            gate.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("GateID", out tmp) == true)
                        {
                            gate.GateID = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("_updated_at", out tmp) == true)
                        {
                            gate.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("_created_at", out tmp) == true)
                        {
                            gate.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("identifier", out tmp) == true)
                        {
                            gate.identifier = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("name", out tmp) == true)
                        {
                            gate.name = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("latitude", out tmp) == true)
                        {
                            gate.latitude = tmp.Value.ToDouble();
                        }
                        if (doc[0].TryGetElement("longitude", out tmp) == true)
                        {
                            gate.longitude = tmp.Value.ToDouble();
                        }
                        if (doc[0].TryGetElement("ProductionCode", out tmp) == true)
                        {
                            gate.ProductionCode = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("WasFree", out tmp) == true)
                        {
                            gate.WasFree = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("GlobalUntil", out tmp) == true)
                        {
                            gate.GlobalUntil = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("DueDate", out tmp) == true)
                        {
                            gate.DueDate = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("Subscription", out tmp) == true)
                        {
                            gate.Subscription = tmp.Value.ToInt32();
                        }
                        if (doc[0].TryGetElement("Subscription", out tmp) == true)
                        {
                            gate.Subscription = tmp.Value.ToInt32();
                        }
                        if (doc[0].TryGetElement("UsersLimit", out tmp) == true)
                        {
                            gate.UsersLimit = tmp.Value.ToInt32();
                        }
                        if (doc[0].TryGetElement("ReSellerID", out tmp) == true)
                        {
                            gate.ReSellerID = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("address", out tmp) == true)
                        {
                            gate.address = tmp.Value.ToString();
                        }

                        return gate;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<List<string>> GetGatesIDByIdentifier(List<String> GateIdentifier)
        {
            try
            {
                List<string> gatesId = new List<string>();

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var filter = Builders<BsonDocument>.Filter.In("identifier", GateIdentifier);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    for (int i = 0; i < doc.Count; i++)
                    {
                        GateObject gate = new GateObject();

                        BsonElement tmp;
                        if (doc[i].TryGetElement("_id", out tmp) == true)
                        {
                            gatesId.Add(tmp.Value.ToString());
                        }
                    }
                }

                return gatesId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<List<string>> GetGatesIDByAddress(String address)
        {
            try
            {
                List<string> gatesId = new List<string>();

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var filter = Builders<BsonDocument>.Filter.AnyIn("address", address);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    if (doc.Count == 1)
                    {
                        BsonElement tmp;
                        if (doc[0].TryGetElement("_id", out tmp) == true)
                        {
                            gatesId.Add(tmp.Value.ToString());
                        }
                    }
                }

                return gatesId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<List<string>> GetUsersByAccessLevel(List<string> AccessLevels)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var filter = Builders<BsonDocument>.Filter.AnyIn("AccessLevel", AccessLevels);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();
                List<string> res = new List<string>();

                if (doc != null)
                {
                    for (int i = 0; i < doc.Count; i++)
                    {
                        BsonElement tmp;
                        if (doc[i].TryGetElement("_id", out tmp) == true)
                        {
                            res.Add(tmp.Value.ToString());
                        }
                    }
                }

                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<List<GateObject>> GetGatesByGateIDs(List<string> GateIDs, List<string> AccessLevels)
        {
            try
            {
                List<GateObject> gates = new List<GateObject>();

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var filter = Builders<BsonDocument>.Filter.AnyIn("GateID", GateIDs) | Builders<BsonDocument>.Filter.AnyIn("AccessLevel", AccessLevels);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    for(int i=0;i< doc.Count; i++)
                    {
                        GateObject gate = new GateObject();

                        BsonElement tmp;
                        if (doc[i].TryGetElement("_id", out tmp) == true)
                        {
                            gate.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("_updated_at", out tmp) == true)
                        {
                            gate.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("_created_at", out tmp) == true)
                        {
                            gate.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("identifier", out tmp) == true)
                        {
                            gate.identifier = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("name", out tmp) == true)
                        {
                            gate.name = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("latitude", out tmp) == true)
                        {
                            gate.latitude = tmp.Value.ToDouble();
                        }
                        if (doc[i].TryGetElement("longitude", out tmp) == true)
                        {
                            gate.longitude = tmp.Value.ToDouble();
                        }
                        if (doc[i].TryGetElement("ProductionCode", out tmp) == true)
                        {
                            gate.ProductionCode = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("WasFree", out tmp) == true)
                        {
                            gate.WasFree = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("GlobalUntil", out tmp) == true)
                        {
                            gate.GlobalUntil = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("DueDate", out tmp) == true)
                        {
                            gate.DueDate = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("Subscription", out tmp) == true)
                        {
                            gate.Subscription = tmp.Value.ToInt32();
                        }
                        if (doc[i].TryGetElement("Subscription", out tmp) == true)
                        {
                            gate.Subscription = tmp.Value.ToInt32();
                        }
                        if (doc[i].TryGetElement("UsersLimit", out tmp) == true)
                        {
                            gate.UsersLimit = tmp.Value.ToInt32();
                        }
                        if (doc[i].TryGetElement("ReSellerID", out tmp) == true)
                        {
                            gate.ReSellerID = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("GateID", out tmp) == true)
                        {
                            gate.GateID = tmp.Value.ToString();
                        }

                        gates.Add(gate);
                    }
                }

                return gates;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<long> deleteAllUserForGate(string GateObjectID)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");
                var filter = Builders<BsonDocument>.Filter.Eq("_p_gate", "Gate$" + GateObjectID);

                var a = await collection.DeleteManyAsync(filter);
              
                return a.DeletedCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<List<UserDataObject>> GetAllUserForGate(string GateObjectID)
        {
            try
            {
                GateObject gate = new GateObject();

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");
                var filter = Builders<BsonDocument>.Filter.Eq("_p_gate", "Gate$" + GateObjectID);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                List<GateUserJoinTableObject> res = new List<GateUserJoinTableObject>();

                List<string> users_id = new List<string>();

                if (doc != null)
                {
                    for (int i = 0; i < doc.Count; i++)
                    {
                        GateUserJoinTableObject gujt = new GateUserJoinTableObject();

                        BsonElement tmp;
                        if (doc[i].TryGetElement("_id", out tmp) == true)
                        {
                            gujt.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("_updated_at", out tmp) == true)
                        {
                            gujt.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("_created_at", out tmp) == true)
                        {
                            gujt.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("_p_gate", out tmp) == true)
                        {
                            gujt._p_gate = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("_p_user", out tmp) == true)
                        {
                            gujt.user = tmp.Value.ToString();
                            users_id.Add(gujt.user.Replace("_User$", ""));
                        }
                        if (doc[i].TryGetElement("_p_location", out tmp) == true)
                        {
                            gujt.location = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("name", out tmp) == true)
                        {
                            gujt.name = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("isAdmin", out tmp) == true)
                        {
                            gujt.isAdmin = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("autoOpen", out tmp) == true)
                        {
                            gujt.autoOpen = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("showNotification", out tmp) == true)
                        {
                            gujt.showNotification = tmp.Value.ToBoolean();
                        }
                        if (doc[i].TryGetElement("DueDate", out tmp) == true)
                        {
                            gujt.DueDate = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("ProductionCode", out tmp) == true)
                        {
                            gujt.ProductionCode = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("GateID", out tmp) == true)
                        {
                            gujt.GateID = tmp.Value.ToString();
                        }

                        res.Add(gujt);
                    }
                }
                else
                {
                    res = null;
                }

                if (users_id.Count > 0)
                {
                    List<UserDataObject> users = await GetUsersByID(users_id);

                    foreach (GateUserJoinTableObject gujt in res)
                    {
                        foreach (UserDataObject user in users)
                        {
                            if (gujt.user.Replace("_User$", "") == user.ObjectId)
                            {
                                user.GateUserObjId = gujt.ObjectId;
                            }
                        }
                    }
                    return users;
                }
                else
                {
                    return new List<UserDataObject>();
                }


                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<bool> SetFavoritesForToken(string FavoriteGatesObjectID, UserObject user)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("_id", user.ObjectId);
                try
                {
                    filter |= Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(user.ObjectId));
                }
                catch { }

                var update = Builders<BsonDocument>.Update
                .Set("FavoriteGatesObjectID", FavoriteGatesObjectID);

                await collection.UpdateOneAsync(filter, update);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<bool> SetSmsTimeStamps(List<DateTime> smsTimeStamps, UserObject user)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");

                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("_id", user.ObjectId);

                try
                {
                    filter |= builder.Eq("_id", new ObjectId(user.ObjectId));
                }
                catch { }

                var bsonTimestamps = new BsonArray(smsTimeStamps.Select(ts => (BsonValue)ts));

                var update = Builders<BsonDocument>.Update
                    .Set("smsTimeStamps", bsonTimestamps);

                await collection.UpdateOneAsync(filter, update);

                return true;
            }
            catch (Exception ex)
            {
                return false; // optionally log instead of rethrowing
            }
        }


        public static async Task<bool> ChangeGateIdentifier(string oldIdentifier, string NewIdentifier)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("identifier", oldIdentifier);

                var update = Builders<BsonDocument>.Update
                .Set("identifier", NewIdentifier);

                await collection.UpdateOneAsync(filter, update);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<GateUserJoinTableObject> IsGateObjIDrelatedToUser(UserObject user, string gateObjID)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");
                var filter = Builders<BsonDocument>.Filter.Eq("_p_user", "_User$" + user.ObjectId);
                filter &= Builders<BsonDocument>.Filter.Eq("_p_gate", "Gate$" + gateObjID);

                var a = await collection.FindAsync(filter);
                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    if (doc.Count != 0)
                    {
                        GateUserJoinTableObject gujt = new GateUserJoinTableObject();

                        BsonElement tmp;
                        if (doc[0].TryGetElement("_id", out tmp) == true)
                        {
                            gujt.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("_updated_at", out tmp) == true)
                        {
                            gujt.updatedAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("_created_at", out tmp) == true)
                        {
                            gujt.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("_p_gate", out tmp) == true)
                        {
                            gujt._p_gate = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("_p_user", out tmp) == true)
                        {
                            gujt.user = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("_p_location", out tmp) == true)
                        {
                            gujt.location = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("name", out tmp) == true)
                        {
                            gujt.name = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("isAdmin", out tmp) == true)
                        {
                            gujt.isAdmin = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("autoOpen", out tmp) == true)
                        {
                            gujt.autoOpen = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("showNotification", out tmp) == true)
                        {
                            gujt.showNotification = tmp.Value.ToBoolean();
                        }
                        if (doc[0].TryGetElement("DueDate", out tmp) == true)
                        {
                            gujt.DueDate = tmp.Value.ToUniversalTime();
                        }
                        if (doc[0].TryGetElement("ProductionCode", out tmp) == true)
                        {
                            gujt.ProductionCode = tmp.Value.ToString();
                        }
                        if (doc[0].TryGetElement("GateID", out tmp) == true)
                        {
                            gujt.GateID = tmp.Value.ToString();
                        }

                        return gujt;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<bool> GiverUserAccessToGate(List<GateUserJoinTableObject> gujtos)
        {
            try
            {
                var documents = new List<BsonDocument>();

                foreach (var gujto in gujtos)
                {
                    if (gujto.user.Contains("_User$") == false)
                    {
                        gujto.user = "_User$" + gujto.user;
                    }
                    if (gujto._p_gate.Contains("Gate$") == false)
                    {
                        gujto._p_gate = "Gate$" + gujto._p_gate;
                    }

                    var document = new BsonDocument
                    {
                        { "_id", gujto.ObjectId},
                        { "_p_gate", gujto._p_gate},
                        { "_p_user",  gujto.user},
                        { "location", ""},
                        { "name", gujto.name},
                        { "isAdmin", false},
                        { "autoOpen", false},
                        { "showNotification", false},
                        { "DueDate", DateTime.Now.AddYears(100) },
                        { "ProductionCode", "12312213" },
                        { "GateID", gujto.GateID },
                        { "updatedAt", DateTime.Now},
                        { "createdAt", DateTime.Now }
                    };

                    documents.Add(document);

                }


                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");

                await collection.InsertManyAsync(documents);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string BeAllInsertNewGate(List<string> _AccessLevel, string identifier, string gate_name, string gateID, string address)
        {
            try
            {
                // AccessLevel is a string array
                BsonArray AccessLevel = new BsonArray();
                string id = DataBase.RandomString(10);


                if (_AccessLevel.Count > 0)
                {
                    foreach (string a in _AccessLevel)
                    {
                        if (a.Length > 0)
                        {
                            AccessLevel.Add(a);
                        }
                    }
                }

                var document = new BsonDocument
                {
                    { "_id", id},
                    { "GlobalUntil", DateTime.Now.AddYears(50)},
                    { "AccessLevel", AccessLevel},
                    { "ProductionCode", "164ba722755bdc8e"},
                    { "identifier", identifier},
                    { "name", gate_name},
                    { "GateID", gateID},
                    { "longitude", 1},
                    { "address", address},
                    { "latitude", 1 },
                    { "WasFree", false },
                    { "useMasterKey", true },
                    { "updatedAt", DateTime.UtcNow},
                    { "createdAt", DateTime.UtcNow },
                    { "_updated_at", DateTime.UtcNow},
                    { "_created_at", DateTime.UtcNow}
                };


                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");

                collection.InsertOne(document);

                return id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<bool> BeAllInsertOpenEvent(string username, bool didOpen, int tries, bool success, bool isTest, bool isAutoOpen,string gateIdentifier, DateTime date)
        {
            try
            {
                var document = new BsonDocument
                {
                    { "_id", RandomString(10)},
                    { "gateIdentifier", gateIdentifier},
                    { "isAutoOpen", isAutoOpen},
                    { "isTest", isTest},
                    { "success", success},
                    { "tries", tries},
                    { "username", username},
                    { "didOpen", didOpen},
                    { "_updated_at", date},
                    { "_created_at", date}
                };


                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("OpeningEvent");

                await collection.InsertOneAsync(document);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<bool> GiverUserAccessToGates(List<GateUserJoinTableObject> gujto)
        {
            if (gujto.Count == 0)
                return true;

            //try
            {
                List<BsonDocument> docs = new List<BsonDocument>();

                for (int i = 0; i < gujto.Count; i++)
                {
                    if (gujto[i].user.Contains("_User$") == false)
                    {
                        gujto[i].user = "_User$" + gujto[i].user;
                    }
                    if (gujto[i]._p_gate.Contains("Gate$") == false)
                    {
                        gujto[i]._p_gate = "Gate$" + gujto[i]._p_gate;
                    }

                    var document = new BsonDocument
                    {
                        { "_id", ObjectId.GenerateNewId()},
                        { "_p_gate", gujto[i]._p_gate},
                        { "_p_user",  gujto[i].user},
                        { "location", ""},
                        { "name", gujto[i].name},
                        { "isAdmin", false},
                        { "autoOpen", false},
                        { "showNotification", false},
                        { "DueDate", DateTime.UtcNow.AddYears(100) },
                        { "ProductionCode", "asdasd" },
                        {"GateID", gujto[i].GateID },
                        { "updatedAt", DateTime.UtcNow},
                        { "createdAt", DateTime.UtcNow },
                        { "_updated_at", DateTime.UtcNow},
                        { "_created_at", DateTime.UtcNow}
                    };

                    docs.Add(document);
                }

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");

                await collection.InsertManyAsync(docs);

                return true;
            }
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        public static async Task<bool> GiverUserAccessToGatesWithAccess(UserDataObject newUser)
        {
            // get all gates with that access level
            var client = new MongoClient(MlabConnection);
            var database = client.GetDatabase(MlabDatabase);
            var collection = database.GetCollection<BsonDocument>("Gate");

            var filter = Builders<BsonDocument>.Filter.AnyIn("AccessLevel", newUser.AccessLevel.Split(","));

            var Cresult = await collection.FindAsync(filter);
            List<BsonDocument> doc = await Cresult.ToListAsync();

            // get all id's
            List<GateUserJoinTableObject> gates = new List<GateUserJoinTableObject>();
            if (doc != null)
            {
                for (int i = 0; i < doc.Count; i++)
                {
                    GateUserJoinTableObject obj = new GateUserJoinTableObject();
                    obj.createdAt = DateTime.UtcNow;
                    obj.updatedAt = DateTime.UtcNow;

                    obj._p_gate = doc[i].GetValue("_id").AsString;
                    obj.name = doc[i].GetValue("name").AsString;
                    obj.GateID = doc[i].GetValue("GateID").AsString;
                    obj.DueDate = DateTime.UtcNow.AddYears(10);
                    obj.isAdmin = false;
                    obj.ProductionCode ="asdasdasd";
                    obj.user = newUser.ObjectId;
                    gates.Add(obj);
                }

                // insert all gates in gates
                bool ans = await GiverUserAccessToGates(gates);
            }

            return true;
        }

        public static async Task<string> removeUsers(List<string> PhoneNumbers)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");

                var filter = Builders<BsonDocument>.Filter.AnyIn("username", PhoneNumbers);

                var results = await collection.DeleteManyAsync(filter);

                if (results.IsAcknowledged == true)
                {
                    return "success, deleted " + results.DeletedCount.ToString() + " users";
                }
                else
                {
                    return "failed to delete";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<long> RemoveUserAccessFromGateByUserGateID(List<string> guids)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");


                var filter = Builders<BsonDocument>.Filter.In("_id", guids);

                var results = await collection.DeleteManyAsync(filter);

                return results.DeletedCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<long> RemoveGatesByGatesID(List<string> GatesID)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");


                var filter = Builders<BsonDocument>.Filter.In("_id", GatesID);

                var results = await collection.DeleteManyAsync(filter);

                return results.DeletedCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<bool> RemoveUserAccessFromGate(GateUserJoinTableObject gujto)
        {
            try
            {
                if (gujto.user.Contains("_User$") == false)
                {
                    gujto.user = "_User$" + gujto.user;
                }
                if (gujto._p_gate.Contains("Gate$") == false)
                {
                    gujto._p_gate = "Gate$" + gujto._p_gate;
                }

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("GateUserJoinTable");


                var filter = Builders<BsonDocument>.Filter.Eq("_id", gujto.ObjectId);
                try
                {
                    filter |= Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(gujto.ObjectId));
                }
                catch { }

                var results = await collection.DeleteManyAsync(filter);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<bool> InsertNewUsers(List<string> usersPhoneNumber, List<string> accessLevels, List<string> objectIds)
        {
            // AccessLevel is a string array
            BsonArray AccessLevel = new BsonArray();

            if (accessLevels.Count > 0)
            {
                foreach (string a in accessLevels)
                {
                    if (a.Length > 0)
                    {
                        AccessLevel.Add(a);
                    }
                }
            }

            List<BsonDocument> allDocs = new List<BsonDocument>();

            try
            {
                for (int i = 0; i < usersPhoneNumber.Count; i++)
                {
                    var document = new BsonDocument
                    {
                        { "_rperm", new BsonArray { "*", objectIds[i] }},
                        { "_wperm", new BsonArray { objectIds[i] }},
                        { "_acl", new BsonDocument {
                                    { "*" , new BsonDocument{ { "r", true } } },
                                    { objectIds[i], new BsonDocument { { "r", true }, { "w", true } }} }
                        },
                        { "_id", objectIds[i]},
                        { "AccessLevel", AccessLevel},
                        { "email",  ""},
                        { "name", ""},
                        { "username", usersPhoneNumber[i]},
                        { "isNew", true},
                        { "FavoriteGatesObjectID", ""},
                        { "_updated_at", DateTime.UtcNow},
                        { "_created_at", DateTime.UtcNow },
                        { "_hashed_password", "$2a$10$gpLOSYZJgIM86rbX07YBJOpmSj1zdMxG2LALQ0SoDBZ9ZIEKzCXuq"},
                        { "token", "" }
                    };
                    allDocs.Add(document);
                }

                if (allDocs.Count > 0)
                {
                    var client = new MongoClient(MlabConnection);
                    var database = client.GetDatabase(MlabDatabase);
                    var collection = database.GetCollection<BsonDocument>("_User");

                    await collection.InsertManyAsync(allDocs);
                }

                return true;
            }
            catch (Exception ex)
            { 
               throw ex;
            }
        }

#warning "to do"
        public static async Task<long> SetNewAccessLevel(List<string> objectID, List<string> accessLevel)
        {
            try
            {
                var array = new BsonArray();
                foreach(var s in accessLevel)
                {
                    array.Add(s);
                }

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.AnyIn("_id", objectID);
                var update = Builders<BsonDocument>.Update
                .Set("AccessLevel", array);

                var a = await collection.UpdateManyAsync(filter, update);

                return a.ModifiedCount;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public static async Task<long> SetNewAccessLevelForGate(string objectID, List<string> accessLevel)
        {
            try
            {
                var array = new BsonArray();
                foreach (var s in accessLevel)
                {
                    array.Add(s);
                }

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("_id", objectID);
                var update = Builders<BsonDocument>.Update
                .Set("AccessLevel", array);

                var a = await collection.UpdateManyAsync(filter, update);

                return a.ModifiedCount;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task SetNewPhoneNumber(string objectID, string NewUserName)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("_id", objectID);
                var update = Builders<BsonDocument>.Update
                .Set("username", NewUserName);

                await collection.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<List<UserObject>> GetAllUsers()
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");
                var sort = Builders<BsonDocument>.Sort.Ascending("_created_at");
                var builder = Builders<BsonDocument>.Filter;

                List<UserObject> users = new List<UserObject>();

                var a = await collection.FindAsync(new BsonDocument(), new FindOptions<BsonDocument, BsonDocument>()
                {
                    Sort = sort
                }); 

                List<BsonDocument> doc = await a.ToListAsync();

                if (doc != null)
                {
                    for (int i=0;i<doc.Count;i++)
                    {
                        UserObject user = new UserObject();
                        BsonElement tmp;
                        if (doc[i].TryGetElement("_id", out tmp) == true)
                        {
                            user.ObjectId = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("username", out tmp) == true)
                        {
                            user.username = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("name", out tmp) == true)
                        {
                            user.name = tmp.Value.ToString();
                        }
                        if (doc[i].TryGetElement("AccessLevel", out tmp) == true)
                        {
                            user.AccessLevel = "";

                            var A = tmp.Value.AsBsonArray;
                            foreach (BsonValue s in A)
                            {
                                user.AccessLevel += s.AsString + ",";
                            }
                            if (user.AccessLevel.Length > 1)
                                user.AccessLevel = user.AccessLevel.Remove(user.AccessLevel.Length - 1, 1);
                        }
                        if (doc[i].TryGetElement("_created_at", out tmp) == true)
                        {
                            user.createdAt = tmp.Value.ToUniversalTime();
                        }
                        if (doc[i].TryGetElement("email", out tmp) == true)
                        {
                            user.email = tmp.Value.ToString();
                        }

                        users.Add(user);
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<bool> RemoveUserAndAllHisGates(UserObject user)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");

                // delete user
                var filter = Builders<BsonDocument>.Filter.Eq("_id", user.ObjectId);
                try
                {
                    filter |= Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(user.ObjectId));
                }
                catch { }

                var results = await collection.DeleteOneAsync(filter);
                //var a = await collection.FindAsync(filter);
                //List<BsonDocument> doc3 = await a.ToListAsync();

                // delete his gates
                var collection2 = database.GetCollection<BsonDocument>("GateUserJoinTable");

                var filter2 = Builders<BsonDocument>.Filter.Eq("_p_user", "_User$" + user.ObjectId);

                var results2 = await collection2.DeleteManyAsync(filter2);
                //var b = await collection2.FindAsync(filter2);
                //List<BsonDocument> doc2 = await b.ToListAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> RemoveUsersAndAllTheirGates(List<UserObject> users)
        {
            try
            {
                List<string> userIds = new List<string>();
                List<string> p_userIds = new List<string>();

                foreach (var user in users)
                {
                    string s = user.ObjectId;
                    userIds.Add(s);

                    if (s.Contains("_User$") == false)
                    {
                        s = "_User$" + s;
                    }
                    p_userIds.Add(s);
                }

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");

                // delete user
                var filter = Builders<BsonDocument>.Filter.AnyIn("_id", userIds);

                var results = collection.DeleteManyAsync(filter);

                // delete his gates
                var collection2 = database.GetCollection<BsonDocument>("GateUserJoinTable");

                var filter2 = Builders<BsonDocument>.Filter.AnyIn("_p_user", p_userIds);

                var results2 = collection2.DeleteManyAsync(filter2);

                await Task.WhenAll(results, results2);

                return "removed " + results.Result.DeletedCount.ToString() + " users, and " + results2.Result.DeletedCount.ToString() + " rooms access";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<bool> RemoveUsersGates(List<UserObject> users)
        {
            List<string> ids = new List<string>();

            foreach (var user in users)
            {
                string s = user.ObjectId;
                if (s.Contains("_User$") == false)
                {
                    s = "_User$" + s;
                }
                ids.Add(s);
            }

            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("_User");

                // delete his gates
                var collection2 = database.GetCollection<BsonDocument>("GateUserJoinTable");

                var filter2 = Builders<BsonDocument>.Filter.AnyIn("_p_user", ids);

                var results2 = await collection2.DeleteManyAsync(filter2);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async void InsertLog(string FreeText, string function_name)
        {
            var client = new MongoClient(MlabConnection);
            var database = client.GetDatabase(MlabDatabase);
            var collection = database.GetCollection<BsonDocument>("Log");
            DateTime t = DateTime.UtcNow;

            string id = RandomString(10);

            var documnt = new BsonDocument
            {
                {"_id",id},
                {"freeText",FreeText},
                {"function_name",function_name},
                {"_updated_at",DateTime.UtcNow}
            };
            await collection.InsertOneAsync(documnt);
        }

        public static long ReplaceAddressInGate(string from_string, string replace_to)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("address", from_string);
                var update = Builders<BsonDocument>.Update.Set("address", replace_to);

                var c = collection.UpdateMany(filter, update);

                return c.ModifiedCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<long> ReplaceGateID(string oldGateID, string newGateID)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("GateID", oldGateID);
                var update = Builders<BsonDocument>.Update
                .Set("GateID", newGateID);

                var ans = await collection.UpdateOneAsync(filter, update);
                return ans.ModifiedCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<long> ReplaceGateName(string gateID, string new_name)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("GateID", gateID);
                var update = Builders<BsonDocument>.Update
                .Set("name", new_name);

                var ans = await collection.UpdateOneAsync(filter, update);
                return ans.ModifiedCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<long> ReplaceGateAddress(string gateID, string new_address)
        {
            try
            {
                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("Gate");
                var builder = Builders<BsonDocument>.Filter;
                var filter = builder.Eq("GateID", gateID);
                var update = Builders<BsonDocument>.Update
                .Set("address", new_address);

                var ans = await collection.UpdateOneAsync(filter, update);
                return ans.ModifiedCount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        #endregion


    }

}