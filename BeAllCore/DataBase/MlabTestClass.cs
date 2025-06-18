using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;
using System.Security;
using BeAllCore.Objects;
using System.Threading.Tasks;

namespace BeAllCore
{
    public static class DataBaseTest
    {
        // ====== connection for DEV ONLY
        // to manage via parse please use:
        //https://devomgate.azurewebsites.net/parse-dashboard
        //77bb4b6b-36c5-4b8e-8f6f-8c011d03a46c
        //95e7cb74-406f-4684-9c16-ee8000d19ea4

        public static string MlabDatabase = "omgate";
        public static string MlabConnection = "mongodb://apicore:apicore@ds046217-a0.mlab.com:46217,ds046217-a1.mlab.com:46212/omgate?replicaSet=rs-ds046217";

        public static async Task<bool> InsertNewCentralLog(string identifiers, string status, string token)
        {
            BsonArray identifiers_arr = new BsonArray();
            BsonArray status_arr = new BsonArray();
            string new_AccessLevel = "";

            if (identifiers.Length > 0)
            {
                foreach (string a in identifiers.Split(","))
                {
                    if (a.Length > 0)
                    {
                        identifiers_arr.Add(a);
                    }
                }
            }

            if (status.Length > 0)
            {
                foreach (string a in status.Split(","))
                {
                    if (a.Length > 0)
                    {
                        status_arr.Add(a);
                    }
                }
            }

            //try
            {
                var document = new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId()},
                    { "identifiers", identifiers_arr},
                    { "status",  status_arr},
                    { "token", token},
                    { "_created_at", DateTime.Now }
                };

                var client = new MongoClient(MlabConnection);
                var database = client.GetDatabase(MlabDatabase);
                var collection = database.GetCollection<BsonDocument>("CentralLogs");

                await collection.InsertOneAsync(document);

                return true;
            }
            //catch (Exception ex)
            // {
            //    throw ex;
            // }
        }

    }

}