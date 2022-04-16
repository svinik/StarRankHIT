using System;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace StarRankHIT
{
    public static class Constants
    {
        public static String EMPTY_WORKER_ID = "EMPTY_WORKER_ID";
        public static String EMPTY_HIT_ID = "EMPTY_HIT_ID";
        public static String EMPTY_ASSIGNMENT_ID = "AVAILABLE_NOT_ID_ASSIGNMENT";

        public static String MTURK_ASSIGNMENT_NOT_AVAILABLE = "ASSIGNMENT_ID_NOT_AVAILABLE";

        public static String APK_VER = "1.0.0";

        public static String GAME_VARIANT = "StarRankHIT";

        public static String PHASE = "Initial";

        public static String ERROR_MESSAGE = "Something went wrong. Please contact the HIT requester.";

        public static String TIME_STAMP_FORMAT = "dd.MM.yyyy HH:mm:ss:fff";

        public static String getTimeStamp()
        {
            TimeZoneInfo israelTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            DateTime utc = DateTime.UtcNow;
            DateTime dateTimeEnd = TimeZoneInfo.ConvertTimeFromUtc(utc, israelTimeZone);
            return dateTimeEnd.ToString(TIME_STAMP_FORMAT);
        }

        public static String GetTimeDiff(String time_start, String time_end)
        {
            try
            {
                DateTime dateTimeStart = DateTime.ParseExact(time_start, TIME_STAMP_FORMAT, System.Globalization.CultureInfo.InvariantCulture);
                DateTime dateTimeEnd = DateTime.ParseExact(time_end, TIME_STAMP_FORMAT, System.Globalization.CultureInfo.InvariantCulture);
                TimeSpan span = dateTimeEnd.Subtract(dateTimeStart);
                String hours = timeToStr(span.Hours);
                String minutes = timeToStr(span.Minutes);
                String seconds = timeToStr(span.Seconds);

                String diff = hours + ":" + minutes + ":" + seconds;
                return diff;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public static String timeToStr(long timeUnit)
        {
            String str = timeUnit + "";
            if (str.Length == 1)
            {
                str = "0" + str;
            }
            return str;
        }

        public static void WriteErrorToDB(String workerId, String function, String exceptionMessage, String stackTrace)
        {
            // WRITE DETAILS TO DB.
            var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
            var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
            var collection = DB.GetCollection<BsonDocument>("exceptions");
            String timeStamp = Constants.getTimeStamp();

            var documnt = new BsonDocument
            {
                {"workerId", workerId},
                {"function", function},
                {"exception_message", exceptionMessage},
                {"stack_trace", stackTrace},
                {"phase", PHASE },
                {"time_stamp_server", timeStamp  }
            };

            collection.InsertOneAsync(documnt);
        }

        /*
        * A Mongodb Docuemnt is not a valid Json document.
        * It has some values that is not gonna parsed with json.net.
        * Here we should remove them. 
        */
        public static String GetValidJson(string doc)
        {
            var result = Regex.Match(doc, @"ObjectId\(([^\)]*)\)").Value;
            var id = result.Replace("ObjectId(", string.Empty).Replace(")", String.Empty);
            var validJson = doc.Replace(result, id);
            return validJson;
        }

        public static string GetIPAddress()
        {
            //If I use the Binding Address Localhost:5000 then the IP is returned as "::1" (Localhost IPv6 address). If I bind my Webapi on the IP Address and try to reach it from another client computer, I get Client's IP Address in API Response.
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }
            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static Boolean IsMobileUser(string sUA)
        {
            string[] options = { "ipod", "iphone", "android", "opera mobi", "fennec" };
            foreach (string option in options)
            {
                if (sUA.Contains(option))
                {
                    return true;
                }
            }

            if (sUA.Contains("windows phone os") && sUA.Contains("iemobile"))
            {
                return true;
            }

            // no evidence that the decive is mobile.
            return false;
        }
    }
}