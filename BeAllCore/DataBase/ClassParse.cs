using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Bson;
using System.Web;
using System.Security;
using System.Net;
using System.IO;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Summary description for ClassMlabDB
/// </summary>
//[SecuritySafeCritical]
public static class ClassParse
{
    public static string GATE_SUBCRIPTION_NOT_DEFINE = "0";
    public static string GATE_SUBCRIPTION_GLOBAL = "1";
    public static string GATE_SUBCRIPTION_PER_USER = "2";
    public static string GATE_SUBCRIPTION_FREE = "3";
    public static string GATE_SUBCRIPTION_GLOBAL_FREE_TRAIL_48H = "4"; // free for specific amount of time
    public static string GATE_SUBCRIPTION_GLOBAL_FREE_TRAIL_3M = "5"; // free for specific amount of time


    private static string ParseServer = "https://beallaccesscontroller.azurewebsites.net/parse/functions/";

    // atlas server
    //private static string ParseServer = "https://beallparseatlas.azurewebsites.net/parse/functions/";
    private static string ParseApplicationId = "f3c3e1a2-8b6b-4780-b97c-2312ef178614";
    private static string ParseMasterKey = "5625dc0d-10e7-4328-b473-2555ac78e9e1";


    public static bool SendParseCommand(string function, string data)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new System.Uri(ParseServer + function));

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("X-Parse-Application-Id", ParseApplicationId);
            request.Headers.Add("X-Parse-Master-Key", ParseMasterKey);

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(data);

            request.ContentLength = bytes.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public static string SendParseCommandReturnObjectID(string function, string data)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new System.Uri(ParseServer + function));

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("X-Parse-Application-Id", ParseApplicationId);
            request.Headers.Add("X-Parse-Master-Key", ParseMasterKey);

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(data);

            request.ContentLength = bytes.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();
            //"objectId\":\"SSwALA4YIB\",

            try
            {
                responseFromServer = responseFromServer.Substring(responseFromServer.IndexOf("\"objectId\":\"") + "\"objectId\":\"".Length, "SSwALA4YIB".Length);
            }
            catch
            {
                responseFromServer = "";
            }

            return responseFromServer;
        }
        catch (Exception ex)
        {
            return "";
        }
    }

    public static string SendParseCommandReturnResponse(string function, string data)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new System.Uri(ParseServer + function));

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("X-Parse-Application-Id", ParseApplicationId);
            request.Headers.Add("X-Parse-Master-Key", ParseMasterKey);

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(data);

            request.ContentLength = bytes.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }
        catch (WebException ex)
        {
            WebResponse wr = ex.Response;
            Stream dataStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            return responseFromServer;
        }
    }

    public static async Task<string> SendParseCommandReturnResponseAsync(string function, string data)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new System.Uri(ParseServer + function));

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("X-Parse-Application-Id", ParseApplicationId);
            request.Headers.Add("X-Parse-Master-Key", ParseMasterKey);

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(data);

            request.ContentLength = bytes.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();

            WebResponse response = await request.GetResponseAsync();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }
        catch (WebException ex)
        {
            WebResponse wr = ex.Response;
            Stream dataStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            return responseFromServer;
        }
    }

    public static bool CheckIfGateIdentifierSigned(string GateIdentifier)
    {
        string param = "{" + InsertParameterString("identifier", GateIdentifier) + "}";

        string res = SendParseCommandReturnResponse("is_gate_identifier_signed", param);

        if (IsResponseSuccess(res))
        {
            if (GetResponseDetailValue(res, "") == "no gate")
                return false;
            else if (GetResponseDetailValue(res, "") == "there is gate")
                return true;
            else
                throw new Exception("CheckIfGateIdentifierSigned not expected this value: " + res);
        }
        else
        {
            throw new Exception("error at CheckIfGateIdentifierSigned: " + res);
        }
    }

    public static bool CheckIfUserNumberisOK(string UserID, string UserNumber)
    {
        string param = "{" + InsertParameterString("UserID", UserID) + ", " + InsertParameterString("UserNumber", UserNumber) + "}";

        string res = SendParseCommandReturnResponse("is_userdid_and_number_signed", param);

        if (IsResponseSuccess(res))
        {
            if (GetResponseDetailValue(res, "") == "user found")
                return true;
            else if (GetResponseDetailValue(res, "") == "there is no such user")
                return false;
            else
                throw new Exception("CheckIfGateIdentifierSigned not expected this value: " + res);
        }
        else
        {
            throw new Exception("error at CheckIfUserNumberisOK: " + res);
        }
    }

    #region "BackOffice"

    

    public static bool ChangeUserEmail(string userObID, string new_email)
    {
        string param = "{" + InsertParameterString("userId", userObID) + "," + InsertParameterString("new_email", new_email) + "}";

        string res = SendParseCommandReturnResponse("BackOffice_ChangeUserEmail", param);

        if (IsResponseSuccess(res))
        {
            return true;
        }
        else
        {
            throw new Exception("error at ChangeUserEmail: " + res);
        }
    }

    public static bool insert_OpeningEvent(DateTime ForcedDate1, DateTime ForcedDate2, string username, string gateIdentifier, string ForcedNote, DateTime DelFrom, DateTime DelTo)
    {
        string param = "{" + InsertParameterString("DelFrom", DelFrom.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")) + "," + InsertParameterString("DelTo", DelTo.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")) + "," + InsertParameterString("ForcedDate2", ForcedDate2.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")) + "," + InsertParameterString("ForcedDate1", ForcedDate1.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))  + "," +  InsertParameterString("username", username) + "," +  InsertParameterString("gateIdentifier", gateIdentifier) + "," + InsertParameterString("ForcedNote", ForcedNote) + "}";

        string res = SendParseCommandReturnResponse("insert_OpeningEvent", param);

        if (IsResponseSuccess(res))
        {
            return true;
        }
        else
        {
            throw new Exception("error at insert_OpeningEvent: " + res);
        }
    }

    public static async Task<bool> insert_RealOpeningEvent(string identifier, bool success, string userPhoneNumber, bool didOpen, int tries)
    {
        /*
        var identifier = request.params.identifier;
        var isAutoOpen = request.params.isAutoOpen;
        var isTest = request.params.isTest;
        var success = request.params.success;
        var tries = request.params.tries;
        var userPhoneNumber = request.params.userPhoneNumber;
        var didOpen = request.params.didOpen;
        */

        //isAutoOpen == true and isTest == true, indicate that it's the new android app log

        string param = "{" + InsertParameterString("identifier", identifier) +  "," +
            InsertParameterBool("isAutoOpen", true) + "," +
            InsertParameterBool("isTest", true) + "," +
            InsertParameterBool("success", success) + "," +
            InsertParameterValue("tries", tries.ToString()) + ", " +
            InsertParameterString("userPhoneNumber", userPhoneNumber) + "," +
            InsertParameterBool("didOpen", didOpen) + "}";

        string res = await SendParseCommandReturnResponseAsync("add_OpeningEvent", param);

        // res include the new object only
        return true;
    }

    public static string DeleteGate(string GateIdentifier, string password)
    {
        string param = "{" + InsertParameterString("GateIdentifier", GateIdentifier) + ","  + InsertParameterString("password", password) + "}";

        string res = SendParseCommandReturnResponse("Delete_Gate_Site", param);
        return res;
    }

    public static string CanUserLogInToBackOffice(string UserNumber, string verificationCode, string backDoor)
    {
        string param = "";
        if (verificationCode == "")
         param = "{" + InsertParameterString("userNumber", UserNumber) +"}";
        else
            param = "{" + InsertParameterString("userNumber", UserNumber) + ", " + InsertParameterValue("verificationCode", verificationCode) + ", " + InsertParameterString("backDoor", backDoor) + "}";


        string res = SendParseCommandReturnResponse("BackOffice_CanUserLogIn", param);
        return res;
    }

    public static string InsertNewBackOfficePaymentRequest(string UserNumber, string NeedToPay)
    {
        string param = "{" + InsertParameterString("userNumber", UserNumber) + ", " + InsertParameterValue("NeedToPay", NeedToPay) + "}";

        string res = SendParseCommandReturnResponse("BackOffice_InsertNewPaymentRequest", param);
        return res;
    }

    public static string IsUserRealAndHaveGates(string UserNumber)
    {
        UserNumber = UserNumber.Replace("\"", "").Replace("\r", "").Replace("\n", "").Replace("{", "").Replace("}", "").Replace("'", "");
        string param = "{" + InsertParameterString("userNumber", UserNumber) + "}";
        string res = SendParseCommandReturnResponse("how_many_gates_for_user", param);

        if (IsResponseSuccess(res))
        {
            return (GetResponseDetailValue(res, "GatesCount"));
        }
        else
        {
            throw new Exception("error at IsUserRealAndHaveGates: " + res);
        }
    }

        #endregion

        #region "Reseller"

    public static string InsertReseller(string Name, string PhoneNumber, string Location, string DiscountPrecent, string IncomePrecent, string ContactInfo, string Notes, string Master, string password, string ResellerCode)
    {
        Name = FixString(Name);
        PhoneNumber = FixString(PhoneNumber);
        Location = FixString(Location);
        DiscountPrecent = FixString(DiscountPrecent);
        IncomePrecent = FixString(IncomePrecent);
        ContactInfo = FixString(ContactInfo);
        Notes = FixString(Notes);
        Master = FixString(Master);
        password = FixString(password);
        ResellerCode = FixString(ResellerCode);

        string param = "{ \"Name\": \"" + Name + "\", \"PhoneNumber\": \"" + PhoneNumber + "\", \"Location\": \"" + Location + "\", \"DiscountPrecent\": " + DiscountPrecent + ", \"IncomePrecent\": " + IncomePrecent + ", \"ContactInfo\": \"" + ContactInfo + "\", \"Notes\": \"" + Notes + "\" , \"Master\": \"" + Master + "\" , \"Password\": \"" + password + "\", \"ResellerCode\": \""+ ResellerCode + "\"}";

        return SendParseCommandReturnObjectID("add_reseller", param);
    }

    public static bool EditReseller(string objectID, string Name, string PhoneNumber, string Location, string DiscountPrecent, string IncomePrecent, string ContactInfo, string Notes, string ResellerCode, string pass)
    {
        Name = FixString(Name);
        PhoneNumber = FixString(PhoneNumber);
        Location = FixString(Location);
        DiscountPrecent = FixString(DiscountPrecent);
        IncomePrecent = FixString(IncomePrecent);
        ContactInfo = FixString(ContactInfo);
        Notes = FixString(Notes);
        ResellerCode = FixString(ResellerCode);
        pass = FixString(pass);

        string param = "{ \"objectID\": \"" + objectID + "\", \"Name\": \"" + Name + "\", \"PhoneNumber\": \"" + PhoneNumber + "\", \"Location\": \"" + Location + "\", \"DiscountPrecent\": " + DiscountPrecent + ", \"IncomePrecent\": " + IncomePrecent + ", \"ContactInfo\": \"" + ContactInfo + "\", \"Notes\": \"" + Notes + "\", \"ResellerCode\": \"" + ResellerCode + "\", \"pass\": \"" + pass + "\"}";

        if (SendParseCommand("edit_reseller", param))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public static string Reseller_LogIn(string userNumber, string userPassword)
    {
        string param = "{" + InsertParameterString("userNumber", userNumber) + ", " + InsertParameterString("userPassword", userPassword) + "}";
        string res = SendParseCommandReturnResponse("Resellers_LogIn", param);
        return res;
    }

    public static string Resellers_GetAllGates(string resellerId)
    {
        string param = "{" + InsertParameterString("resellerId", resellerId) + "}";
        string res = SendParseCommandReturnResponse("Resellers_GetAllGates", param);
        return res;
    }

    public static string Resellers_GetAllGatesWithPayments(string resellerId)
    {
        string param = "{" + InsertParameterString("resellerId", resellerId) + "}";
        string res = SendParseCommandReturnResponse("Resellers_GetAllGatesWithPayments", param);
        return res;
    }

    public static string Resellers_GetAllPaymentsFromAllResellerGates()
    {
        string param = "{}";
        string res = SendParseCommandReturnResponse("Resellers_GetAllPaymentsFromAllResellerGates", param);
        return res;
    }

    public static string Resellers_GetAllPaymentsFromReseller(string resellerId)
    {
        string param = "{" + InsertParameterString("resellerId", resellerId) + "}";
        string res = SendParseCommandReturnResponse("Resellers_GetAllGatesWithPayments", param);
        return res;
    }

    public static string Resellers_GetAllReseller()
    {
        string param = "{}";
        string res = SendParseCommandReturnResponse("Resellers_GetAllReseller", param);
        return res;
    }
    

    #endregion

    #region "Coupons"

    public static string InsertCoupon(bool ForGate, string Used, string UseLimit, string FreeDays, string Notes)
    {
        Used = FixString(Used);
        UseLimit = FixString(UseLimit);
        FreeDays = FixString(FreeDays);
        Notes = FixString(Notes);

        string param = "{ \"ForGate\": ";
        if (ForGate == true)
        {
            param = "{ \"ForGate\": true, \"ForUser\": false, ";
        }
        else
        {
            param = "{ \"ForGate\": false, \"ForUser\": true, ";
        }


        param += "\"Used\": " + Used + ", \"UseLimit\": " + UseLimit + ", \"FreeDays\": " + FreeDays + ", \"Notes\": \"" + Notes + "\"}";

        string res = SendParseCommandReturnObjectID("add_coupon", param);
        return res;
    }

    public static bool EditCoupon(string objectId, bool ForGate, string Used, string UseLimit, string FreeDays, string Notes)
    {
        Used = FixString(Used);
        UseLimit = FixString(UseLimit);
        FreeDays = FixString(FreeDays);
        Notes = FixString(Notes);

        string param = "{ \"ForGate\": ";
        if (ForGate == true)
        {
            param = "{ \"ForGate\": true, \"ForUser\": false, ";
        }
        else
        {
            param = "{ \"ForGate\": false, \"ForUser\": true, ";
        }


        param += "\"Used\": " + Used + ", \"UseLimit\": " + UseLimit + ", \"FreeDays\": " + FreeDays + ", \"Notes\": \"" + Notes + "\", \"objectId\": \"" + objectId + "\"}";

        if (SendParseCommand("edit_coupon", param))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region "Helpers"

    static public string GetStringBetween(string str, string id)
    {
        try
        {
            int start = str.IndexOf("<" + id + ">") + ("<" + id + ">").Length;
            int end = str.IndexOf("</" + id + ">");

            return str.Substring(start, end - start);
        }
        catch {
            return "";
        }
    }

    public static string FixString(string str)
    {
        return str.Replace("\"", "").Replace("\r", "</br>").Replace("\n", "").Replace("{","").Replace("}","").Replace("'","");
    }

    public static string FixUserInputString(string str)
    {
        return str.Replace("\"", "").Replace("\r", "").Replace("\n", "").Replace("{", "").Replace("}", "").Replace("'", "");
    }

    public static string InsertParameterString(string parm, string value)
    {
        value = value.Replace("{", "").Replace("}", "").Replace("\r", "").Replace("\n", "");
        return "\"" + parm + "\": \"" + value + "\"";
    }

    public static string InsertParameterValue(string parm, string value)
    {
        value = value.Replace("{", "").Replace("}", "").Replace("\r", "").Replace("\n", "");
        return "\"" + parm + "\": " + value ;
    }

    public static string InsertParameterBool(string parm, bool value)
    {
        if (value == true)
            return "\"" + parm + "\": " + "true";
        else
            return "\"" + parm + "\": " + "false";
    }

    public static bool IsResponseSuccess(string ParseResponse)
    {
        if (ParseResponse.Contains("<result>error</result>"))
            return false;
        else if (ParseResponse.Contains("<result>success</result>"))
            return true;
        else
            throw new Exception("response does not include results :" + ParseResponse);
    }

    public static string GetResponseDetailValue(string ParseResponse, string valueId)
    {
        if (ParseResponse.Contains("<details>") && ParseResponse.Contains("</details>"))
        {
            if (valueId != "")
            {
                if (ParseResponse.Contains("<" + valueId + ">") && ParseResponse.Contains("</" + valueId + ">"))
                {
                    return GetStringBetween(ParseResponse, valueId);
                }
                else
                {
                    throw new Exception("response does not include details :" + ParseResponse + "value of: " + valueId);
                }
            }
            else
            {
                return GetStringBetween(ParseResponse, "details");
            }
        }
        else
            throw new Exception("response does not include details :" + ParseResponse + "value of: " + valueId);
    }


    public static Boolean CanGetResponseDetailValue(string ParseResponse, string valueId)
    {
        if (ParseResponse.Contains("<details>") && ParseResponse.Contains("</details>"))
        {
            if (valueId != "")
            {
                if (ParseResponse.Contains("<" + valueId + ">") && ParseResponse.Contains("</" + valueId + ">"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
        else
            return false;
    }

    #endregion

    #region "Ox cloud functions"
public static string InsertOXGate(string address,
                                    string isAdmin,
                                    string longitude,
                                    string latitude,
                                    string gateName,
                                    string ResellerCode,
                                    string identifier,
                                    string Subscription,
                                    bool AllowSubUsers,
                                    string UsersLimit,
                                    string UserID,
                                    string productionCode)
{
    string param = "{" + InsertParameterString("address", address) + ", " + 
                        InsertParameterString("UserID", UserID) + ", " +
                        InsertParameterValue("isAdmin", isAdmin) + ", " +
                        InsertParameterValue("longitude", longitude) + ", " +
                        InsertParameterValue("latitude", latitude) + ", " +
                        InsertParameterString("gateName", gateName) + ", " +
                        InsertParameterString("ResellerCode", ResellerCode) + ", " +
                        InsertParameterValue("Subscription", Subscription) + ", " +
                        InsertParameterBool("AllowSubUsers", AllowSubUsers) + ", " +
                        InsertParameterValue("UsersLimit", UsersLimit) + ", " +
                        InsertParameterString("identifier", identifier) + ", " +
                        InsertParameterString("productionCode", productionCode) + "}";

    string res = SendParseCommandReturnResponse("add_ox_gate", param);
    return res;
}

public static string CouponForOXGate(string UserID,
                                string GateID,
                                string CouponID)
{
    string param = "{" + InsertParameterString("UserID", UserID) + ", " +
                        InsertParameterString("GateID", GateID) + ", " +
                        InsertParameterString("CouponID", CouponID) + "}";

    string res = SendParseCommandReturnResponse("coupon_ox_gate", param);
    return res;
}

public static string CanOpenOXGate(string UserID, string GateID)
{
    string param = "{" + InsertParameterString("UserID", UserID) + ", " +
                        InsertParameterString("GateID", GateID) + "}";

    string res = SendParseCommandReturnResponse("can_open_ox_gate", param);
    return res;
}


public static string IsFreeGate(string gateIdentifier)
{
    string param = "{" + InsertParameterString("gateIdentifier", gateIdentifier) + "}";

    string res = SendParseCommandReturnResponse("Production_GetIsFreeGate", param);
    return res;
}

public static string GetOXUserGateInfo(string UserID,
                            string GateID)
{
    string param = "{" + InsertParameterString("UserID", UserID) + ", " +
                        InsertParameterString("GateID", GateID) + "}";

    string res = SendParseCommandReturnResponse("PaymentSystem_GetUserGateInfo", param);
    return res;
}

public static string ChangeOXGateSubscription(string UserID,
                        string GateID, string NewSubscription)
{
    string param = "{" + InsertParameterString("UserID", UserID) + ", " +
                        InsertParameterString("GateID", GateID) + ", " +
                        InsertParameterValue("NewSubscription", NewSubscription) + "}";

    string res = SendParseCommandReturnResponse("ChangeOXGateSubscription", param);
    return res;
}

public static string Ox_InsertNewPaymentRequest(string UserID,
                    string GateID, string NeedToPay, string PaidFor)
{
    string param = "{" + InsertParameterString("UserID", UserID) + ", " +
                        InsertParameterString("GateID", GateID) + ", " +
                        InsertParameterValue("NeedToPay", NeedToPay) + ", " +
                        InsertParameterValue("PaidFor", PaidFor) + "}";

    string res = SendParseCommandReturnResponse("Ox_InsertNewPaymentRequest", param);
    return res;
}

public static string Log_FreeText(string userPhone,
                string freeText, string userName)
{
    string param = "{" + InsertParameterString("userPhone", userPhone) + ", " +
                        InsertParameterString("freeText", freeText) + ", " +
                        InsertParameterString("userName", userName) + "}";

    string res = SendParseCommandReturnResponse("Log_FreeText", param);
    return res;
}

public static string oX_InsertNewPaymentApprove(string paymentId,
            string paymentAmount)
{
    string param = "{" + InsertParameterString("paymentId", paymentId) + ", " +
                        InsertParameterValue("paymentAmount", paymentAmount) + "}";

    string res = SendParseCommandReturnResponse("oX_InsertNewPaymentApprove", param);
    return res;
}


    #endregion


    #region "Payments"
    public static string EditPaymentPaidTo(string paymentId, string AlsoPaidToId)
    {
        paymentId = FixString(paymentId);
        AlsoPaidToId = FixString(AlsoPaidToId);

        string param = "{ \"paymentId\": \"" + paymentId + "\", \"AlsoPaidToId\": \"" + AlsoPaidToId + "\"}";

        return SendParseCommandReturnObjectID("Paymentes_EditPaymentPaidTo", param);
    }
    #endregion

}



public class Seller
{
    public string id;
    public string Name;
    public string Location;
    public string PhoneNumber;
    public double DiscountPrecent;
    public double IncomePrecent;
    public string Master;
    public string ResellerCode;

    public double total_paid;
    public double total_left_pay;

    public string html;
}

public class Payment
{
    public double paid;
    public string RessellersGotPaid;
    public string gateid;
    public string userid;
    public string createdat;
    public string id;

    public string html_row;
}

public class Resseller
{
    public string id;
    public string Name;
    public string Location;
    public string PhoneNumber;
    public double DiscountPrecent;
    public double IncomePrecent;
    public string Master;
    public string ResellerCode;

    public ArrayList gates;
    public ArrayList resellers;

    public double total_paid;
    public double total_left_pay;

    public string html_row;
    public string html_resellers;
    public string html_gates;
}

public class RessellerSimple
{
    public string id;
    public string Name;
    public string Location;
    public string PhoneNumber;
    public string ResellerCode;
}


public class Gate
{
    public string id;
    public string identifier;
    public string name;
    public string address;
    public string subscription;
    public string UsersLimit;
    public string createdat;
    public string resellerID;
    public string resellerName;
    public double total_income;

    public ArrayList payments;

    public string payments_html_table;
    public string html;
}

public class ResellerData
{
    //get all sellers [id, Resseller]
    public Dictionary<string, Resseller> ResellersAllData = new Dictionary<string, Resseller>();

    //get all gates [id, Gate]
    public Dictionary<string, Gate> GatesAllData = new Dictionary<string, Gate>();

    //get all payments [id, Payment]
    public Dictionary<string, Payment> PaymentAllData = new Dictionary<string, Payment>();


    public string ResellersTable;

    public ArrayList SimpleResellerList = new ArrayList();
}
 