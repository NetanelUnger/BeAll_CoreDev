using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;


namespace BeAllCore.Objects
{
    
    public class UserObject: Resource
    {
        [BsonIgnoreIfNull] public string ObjectId { get; set;} 
        [BsonIgnoreIfNull] public bool emailVerified { get; set; }
        [BsonIgnoreIfNull] public string verificationCode { get; set; }
        [BsonIgnoreIfNull] public string username { get; set; }
        [BsonIgnoreIfNull] public string name { get; set; }
        [BsonIgnoreIfNull] public string AccessLevel { get; set; }
        [BsonIgnoreIfNull] public DateTime updatedAt { get; set; }
        [BsonIgnoreIfNull] public bool isNew { get; set; }
        [BsonIgnoreIfNull] public DateTime createdAt { get; set; }
        [BsonIgnoreIfNull] public string email { get; set; }
        [BsonIgnoreIfNull] public string token { get; set; }

        [BsonIgnoreIfNull] public DateTime? lastSmsSent { get; set; }
        [BsonIgnoreIfNull] public int smsSentCounter { get; set; }
        [BsonIgnoreIfNull] public int smsRetryCounter { get; set; }
        [BsonIgnoreIfNull] public DateTime? userBlockedUntil { get; set; }

        [BsonIgnoreIfNull] public List<DateTime>? smsTimeStamps { get; set; }
        [BsonIgnoreIfNull] public DateTime? pinCodeValidDueDate { get; set; }

        [BsonIgnoreIfNull] public string FavoriteGatesObjectID { get; set; }
    }

    public class UserDataObject : Resource
    {
        [BsonIgnoreIfNull] public string GateUserObjId { get; set; } //if it's gate per user, here we will find the GateUserJoinTable  objectID
        [BsonIgnoreIfNull] public string ObjectId { get; set; }
        [BsonIgnoreIfNull] public string username { get; set; }
        [BsonIgnoreIfNull] public string name { get; set; }
        [BsonIgnoreIfNull] public string AccessLevel { get; set; }
        [BsonIgnoreIfNull] public DateTime updatedAt { get; set; }
        [BsonIgnoreIfNull] public bool isNew { get; set; }
        [BsonIgnoreIfNull] public DateTime createdAt { get; set; }
        [BsonIgnoreIfNull] public string email { get; set; }
        [BsonIgnoreIfNull] public string FavoriteGatesObjectID { get; set; } //return gate objectId that are favorites, splited by ","
    }

    public class UserMobileDataObject : Resource
    {
        [BsonIgnoreIfNull] public string username{ get; set; }
        [BsonIgnoreIfNull] public string name { get; set; }
        [BsonIgnoreIfNull] public string email { get; set; }
        [BsonIgnoreIfNull] public string FavoriteGatesObjectID { get; set; } //return gate objectId that are favorites, splited by ","
        [BsonIgnoreIfNull] public string FavoriteGatesIdentifiers { get; set; } //return gate objectId that are favorites, splited by ","
    }

   public class GateUserJoinTableObject : Resource
    {
        [BsonIgnoreIfNull] public string ObjectId { get; set; }
        [BsonIgnoreIfNull] public DateTime updatedAt { get; set; }
        [BsonIgnoreIfNull] public DateTime createdAt { get; set; }

        [BsonIgnoreIfNull] public GateObject gate { get; set; }
        [BsonIgnoreIfNull] public string _p_gate { get; set; }
        [BsonIgnoreIfNull] public string user { get; set; }
        [BsonIgnoreIfNull] public string location { get; set; }

        [BsonIgnoreIfNull] public string name { get; set; }

        [BsonIgnoreIfNull] public bool isAdmin { get; set; }
        [BsonIgnoreIfNull] public bool autoOpen { get; set; }
        [BsonIgnoreIfNull] public bool showNotification { get; set; }
        [BsonIgnoreIfNull] public DateTime DueDate { get; set; }
        [BsonIgnoreIfNull] public string ProductionCode { get; set; }

        [BsonIgnoreIfNull] public string GateID { get; set; }
    }

    public class GateObject : Resource
    {
        [BsonIgnoreIfNull] public string ObjectId { get; set; }
        [BsonIgnoreIfNull] public DateTime updatedAt { get; set; }
        [BsonIgnoreIfNull] public DateTime createdAt { get; set; }

        [BsonIgnoreIfNull] public string identifier { get; set; }
        [BsonIgnoreIfNull] public string name { get; set; }

        [BsonIgnoreIfNull] public double latitude { get; set; }
        [BsonIgnoreIfNull] public double longitude { get; set; }

        [BsonIgnoreIfNull] public string ProductionCode { get; set; }

        [BsonIgnoreIfNull] public string address { get; set; }
        [BsonIgnoreIfNull] public bool WasFree { get; set; }
        [BsonIgnoreIfNull] public DateTime GlobalUntil { get; set; }
        [BsonIgnoreIfNull] public int Subscription { get; set; }
        [BsonIgnoreIfNull] public double UsersLimit { get; set; }
        [BsonIgnoreIfNull] public string ReSellerID { get; set; }
        [BsonIgnoreIfNull] public DateTime DueDate { get; set; }

        [BsonIgnoreIfNull] public string GateID { get; set; }

        [BsonIgnoreIfNull] public string Loc_Country { get; set; }
        [BsonIgnoreIfNull] public string Loc_City { get; set; }
        [BsonIgnoreIfNull] public string Loc_Building { get; set; }
        [BsonIgnoreIfNull] public string Loc_Level { get; set; }
        [BsonIgnoreIfNull] public string Loc_Room { get; set; }
        [BsonIgnoreIfNull] public string Loc_Range { get; set; }
    }


    public class GateObject2 : Resource
    {
        [BsonIgnoreIfNull] public DateTime createdAt { get; set; }
        [BsonIgnoreIfNull] public string identifier { get; set; }
        [BsonIgnoreIfNull] public string name { get; set; }
        [BsonIgnoreIfNull] public string address { get; set; }
        [BsonIgnoreIfNull] public string GateID { get; set; }
        [BsonIgnoreIfNull] public string AccessLevel { get; set; }
    }


    public class mGateUserListObject : Resource
    {
        [BsonIgnoreIfNull] public UserMobileDataObject UserData { get; set; }
        [BsonIgnoreIfNull] public List<mGateUserObject> ListOfGates{ get; set; }
    }


    // gates for mobile app
    public class mGateUserObject : Resource
    {
        [BsonIgnoreIfNull] public DateTime updatedAt { get; set; }
    
        [BsonIgnoreIfNull] public string objectId { get; set; }

        [BsonIgnoreIfNull] public string identifier { get; set; }
        [BsonIgnoreIfNull] public string name { get; set; }
        [BsonIgnoreIfNull] public string address { get; set; }
        [BsonIgnoreIfNull] public DateTime DueDate { get; set; }
        [BsonIgnoreIfNull] public string ProductionCode { get; set; }


        [BsonIgnoreIfNull] public string Loc_Country { get; set; }
        [BsonIgnoreIfNull] public string Loc_City { get; set; }
        [BsonIgnoreIfNull] public string Loc_Building { get; set; }
        [BsonIgnoreIfNull] public string Loc_Level { get; set; }
        [BsonIgnoreIfNull] public string Loc_Room { get; set; }
        [BsonIgnoreIfNull] public string Loc_Range { get; set; }

        [BsonIgnoreIfNull] public bool isFavorite { get; set; }
    }


    public class OpeningEvent
    {
        [BsonIgnoreIfNull] public string ObjectId { get; set; }
        [BsonIgnoreIfNull] public DateTime stamp { get; set; }
        [BsonIgnoreIfNull] public bool didOpen { get; set; }
        [BsonIgnoreIfNull] public string UserNumber { get; set; }
    }


}
