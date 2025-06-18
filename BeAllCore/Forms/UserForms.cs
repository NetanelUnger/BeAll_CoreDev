using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeAllCore.Forms
{
    #region "gates forms"
    public class GetGatesForTokenForm
    {
        public String Token { get; set; }
    }

    public class GetGatesForUserForm
    {
        public String Token { get; set; }
        public String UserObjId { get; set; }
    }

    public class AddUserToGateForm
    {
        public String Token { get; set; }
        public String NumberToAdd { get; set; }
        public String GateObjectID { get; set; }
    }

    public class RevokeUserFromGateForm
    {
        public String Token { get; set; }
        public String UserObjIDToRemove{ get; set; }
        public String GateObjectID { get; set; }
    }

    public class RemoveGatesForm
    {
        public List<String> gatesIdentifiers { get; set; }
    }

    public class replaceAddressForm
    {
        public String from { get; set; }
        public String to { get; set; }
    }

    public class SetFavoritesObjIDForTokenForm
    {
        public String Token { get; set; }
        public String Favorites { get; set; }
    }

    public class GetUsersForGateForm
    {
        public String Token { get; set; }
        public String GateObjectId { get; set; }
    }
    #endregion

    #region "user forms"
    public class UserFirstLogInForm
    {
        public string Token { get; set; }
        public String UserPhoneNumber { get; set; }
    }

    public class LogInForm
    {
        public String UserPhoneNumber { get; set; }
        public String UserVerificationCode { get; set; }
    }

    public class UserFirstLogInResponseForm
    {
        public String UserPhoneNumber { get; set; }
    }

    public class ToeknCheckForm
    {
        public String token { get; set; }
    }

    public class RegisterNewUserForm
    {
        public string token { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string AccessLevel { get; set; }
        public string email { get; set; }
    }

    public class GetAllUsersForm
    {
        public string token { get; set; }
    }

    
    public class DeleteUserForm
    {
        public string token { get; set; }
        public string deleteObjId { get; set; }
    }

    public class UserVerifyCodeForm
    {
        public String UserPhoneNumber { get; set; }
        public String UserToken { get; set; }
        public String VerificationCode { get; set; }
    }

    public class UserVerifyCodeResponse
    {
        public String UserPhoneNumber { get; set; }
        public String UserName { get; set; }
        public String VerificationCode { get; set; }
    }

    #endregion

    #region "log forms"

    public class GetAllGatesIdForm
    {
        public int count { get; set; }
    }

    public class changeGateIdentifierForm
    {
        public String oldIdentifier { get; set; }
        public String newIdentifier { get; set; }
    }

    public class changeGateIDForm
    {
        public String oldgateID { get; set; }
        public String newgateID { get; set; }
    }


    public class resetGateALForm
    {
        public String gateID { get; set; }
        public List<String> accessLevel { get; set; }
    }


    public class InsertNewGateForm
    {
        public String identifier { get; set; }
        public String name { get; set; }
        public String gateID { get; set; }
        public String address { get; set; }
        public List<String> accessLevel { get; set; }
    }

    public class replaceGateNameAddressForm
    {
        public String gateID { get; set; }
        public String new_name { get; set; }
        public String new_address { get; set; }
    }

    public class SalesForceRevokeUsersForm
    {
        public List<String> usersPhoneNumbers { get; set; }
    }

    public class AddNewUsersForm
    {
        public List<String> usersPhoneNumbers { get; set; }
        public List<String> accessLevel { get; set; }
        public List<String> roomsIds { get; set; }
    }

    public class addRoomToUserForm
    {
        public String userPhoneNumber { get; set; }
        public String roomId { get; set; }
    }

    public class makeRoomPrivateForm
    {
        public String roomId { get; set; }
    }

    public class RefreshRoomsForm
    {
        public List<String> usersPhoneNumbers { get; set; }
        public List<String> roomsIds { get; set; }
        public List<String> accessLevel { get; set; }
    }

    public class ChangePhoneNumberForm
    {
        public String oldPhoneNumber { get; set; }
        public String newPhoneNumber { get; set; }
    }


    public class InsertOpeningEventForTokenForm
    {
        public String Token { get; set; }
        /*
        public int? stamp_day { get; set; }
        public int? stamp_month { get; set; }
        public int? stamp_year { get; set; }
        public int? stamp_hour { get; set; }
        public int? stamp_minute { get; set; }
        public int? stamp_second { get; set; }
        */
        public String identifier { get; set; }
        public bool didOpen { get; set; }
        public int tries { get; set; }
        public bool success { get; set; }
        public string log { get; set; }
    }

    public class GetOpeningEventForGateForm
    {
        public String Token { get; set; }
        public String identifier { get; set; }

        public int start_stamp_day { get; set; }
        public int start_stamp_month { get; set; }
        public int start_stamp_year { get; set; }

        public int end_stamp_day { get; set; }
        public int end_stamp_month { get; set; }
        public int end_stamp_year { get; set; }
    }

    public class openTheGateObject
    {
        public int scramberbyte0 { get; set; }
        public int scramberbyte1 { get; set; }
        public string token { get; set; }
        public string identifier { get; set; }
    }

    #endregion

    #region "centeral"
    public class PostLogForm
    {
        public String token { get; set; }
        public String identifiers { get; set; }
        public String status { get; set; }
    }
    #endregion

    #region "salesForce"

    public class gateIDAndMore
    {
        public String gate_id { get; set; }
    }

    public class SalesGetUsersForGateForm
    {
        public String gate_id { get; set; }
    }

    public class GetUserPinForm
    {
        public String user_phone_number { get; set; }
    }

    public class SendSMSForm
    {
        public String phone { get; set; }
        public String message { get; set; }
        public String from { get; set; }
    }


    public class GetAllGatesByALForm
    {
        public String access_level { get; set; }
    }
    #endregion
}
