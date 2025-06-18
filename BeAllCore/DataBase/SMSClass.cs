using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

/// <summary>
/// Summary description for OmGate_SMS
/// </summary>
public static class SmsClass
{
    public static bool SendSMS(string Phone, string Content)
    {

        send_upSendSms(Content, Phone);


        return true;

        /*
        var credentials = Credentials.FromApiKeyAndSecret(
            "069fa0ad",
            "9cd9e7c697be7a80"
        );

        var VonageClient = new VonageClient(credentials);

        var response = VonageClient.SmsClient.SendAnSms(new Vonage.Messaging.SendSmsRequest()
        {
            To = Phone,
            From = "BeAll",
            Text = Content
        });
        return true;

        */
    }


    public static string send_upSendSms(string message, string number)
    {
        //set password, user name, message text, semder name and number 
        string userName = "UPSEND13699";
        string apiToken = "fdab1bdf-6e9e-479d-b82a-f880113ef111";
        string messageText = System.Security.SecurityElement.Escape(message);
        string sender = "BeAll";
        //set phone numbers 
        string phonesList = number;
        //set additional parameters 
        string timeToSend = ""; //"21/12/2017 15:30";
        // create XML 
        StringBuilder sbXml = new StringBuilder();
        sbXml.Append("<Inforu>");
        sbXml.Append("<User>");
        sbXml.Append("<Username>" + userName + "</Username>");
        sbXml.Append("<ApiToken>" + apiToken + "</ApiToken>");
        sbXml.Append("</User>");
        sbXml.Append("<Content Type=\"sms\">");
        sbXml.Append("<Message>" + messageText + "</Message>");
        sbXml.Append("</Content>");
        sbXml.Append("<Recipients>");
        sbXml.Append("<PhoneNumber>" + phonesList + "</PhoneNumber>");
        sbXml.Append("</Recipients>");
        sbXml.Append("<Settings>");
        sbXml.Append("<Sender>" + sender + "</Sender>");
        sbXml.Append("<MessageInterval>" + "0" + "</MessageInterval>");
        sbXml.Append("<TimeToSend>" + timeToSend + "</TimeToSend>");
        sbXml.Append("</Settings>");
        sbXml.Append("</Inforu >");
        string strXML = HttpUtility.UrlEncode(sbXml.ToString(), System.Text.Encoding.UTF8);
        string result = PostDataToURL("https://api.upsend.co.il/SendMessageXml.ashx", "InforuXML=" +
        strXML);

        return result;
    }



    public static string send_upSendSmsFrom(string message, string number, string from)
    {
        //set password, user name, message text, semder name and number 
        string userName = "UPSEND13699";
        string apiToken = "fdab1bdf-6e9e-479d-b82a-f880113ef111";
        string messageText = System.Security.SecurityElement.Escape(message);
        string sender = from;
        //set phone numbers 
        string phonesList = number;
        //set additional parameters 
        string timeToSend = ""; //"21/12/2017 15:30";
        // create XML 
        StringBuilder sbXml = new StringBuilder();
        sbXml.Append("<Inforu>");
        sbXml.Append("<User>");
        sbXml.Append("<Username>" + userName + "</Username>");
        sbXml.Append("<ApiToken>" + apiToken + "</ApiToken>");
        sbXml.Append("</User>");
        sbXml.Append("<Content Type=\"sms\">");
        sbXml.Append("<Message>" + messageText + "</Message>");
        sbXml.Append("</Content>");
        sbXml.Append("<Recipients>");
        sbXml.Append("<PhoneNumber>" + phonesList + "</PhoneNumber>");
        sbXml.Append("</Recipients>");
        sbXml.Append("<Settings>");
        sbXml.Append("<Sender>" + sender + "</Sender>");
        sbXml.Append("<MessageInterval>" + "0" + "</MessageInterval>");
        sbXml.Append("<TimeToSend>" + timeToSend + "</TimeToSend>");
        sbXml.Append("</Settings>");
        sbXml.Append("</Inforu >");
        string strXML = HttpUtility.UrlEncode(sbXml.ToString(), System.Text.Encoding.UTF8);
        string result = PostDataToURL("https://api.upsend.co.il/SendMessageXml.ashx", "InforuXML=" +
        strXML);

        return result;
    }


    public static string PostDataToURL(string szUrl, string szData)
    { //Setup the web request 
        string szResult = string.Empty;
        WebRequest Request = WebRequest.Create(szUrl);
        Request.Timeout = 25000;
        Request.Method = "POST";
        Request.ContentType = "application/x-www-form-urlencoded";
        //Set the POST data in a buffer 
        byte[] PostBuffer;
        try
        {
            // replacing " " with "+" according to Http post RPC 
            szData = szData.Replace(" ", "+");
            //Specify the length of the buffer 
            PostBuffer = Encoding.UTF8.GetBytes(szData);
            Request.ContentLength = PostBuffer.Length;
            //Open up a request stream 
            Stream RequestStream = Request.GetRequestStream();
            //Write the POST data 
            RequestStream.Write(PostBuffer, 0, PostBuffer.Length);
            //Close the stream 
            RequestStream.Close();
            //Create the Response object 
            WebResponse Response;
            Response = Request.GetResponse();
            //Create the reader for the response 
            StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.UTF8);
            //Read the response 
            szResult = sr.ReadToEnd();
            //Close the reader, and response 
            sr.Close();
            Response.Close();

            return szResult;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

}