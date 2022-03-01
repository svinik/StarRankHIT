using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace StarRankHIT.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            try
            {
                Session["PROLIFIC_PID"] = Request.QueryString["PROLIFIC_PID"];
                Session["STUDY_ID"] = Request.QueryString["STUDY_ID"];
                Session["SESSION_ID"] = Request.QueryString["SESSION_ID"];

                // manage debugging situations.
                if (Session["PROLIFIC_PID"] == null)
                {
                    Session["PROLIFIC_PID"] = Constants.EMPTY_PROLIFIC_PID_STR;
                }
                if (Session["STUDY_ID"] == null)
                {
                    Session["STUDY_ID"] = Constants.EMPTY_STUDY_ID_STR;
                }
                if (Session["SESSION_ID"] == null)
                {
                    Session["SESSION_ID"] = Constants.EMPTY_SESSION_ID_STR;
                }

                // prevent mobile devices.
                if (Constants.IsMobileUser(Request.UserAgent.Trim().ToLower())) {
                    await WriteRestrictedAcceptToDB("Mobile", Session["PROLIFIC_PID"].ToString(), Session["STUDY_ID"].ToString(), Session["SESSION_ID"].ToString(), Constants.APK_VER);
                    return View("Mobile");
                }

                // prevent Internet Explorer browsers. 
                if (IsExplorerUser())
                {
                    // relevant in debug when no assigmentId was passed. Not necessary for Preview mode (there ASSIGNMENT_ID_NOT_AVAILABLE is passed).
                    string assignmentId;
                    if (Session["assignment_id"] == null)
                    {
                        assignmentId = "ASSIGNMENT_ID";
                    }
                    else
                    {
                        assignmentId = Session["assignment_id"].ToString();
                    }
                    await WriteRestrictedAcceptToDB("Explorer", Session["PROLIFIC_PID"].ToString(), Session["STUDY_ID"].ToString(), Session["SESSION_ID"].ToString(), Constants.APK_VER);
                    return View("Explorer");
                };

                Session["start_time_server"] = Constants.getTimeStamp();

                // prevent multiple participation (except for in debug mode)
                if (Session["PROLIFIC_PID"].ToString() != null)
                {
                    if (Session["PROLIFIC_PID"].ToString() != Constants.EMPTY_PROLIFIC_PID_STR)
                    {
                        // ignore past workers from playing again.
                        if (!IsNewWorker(Session["PROLIFIC_PID"].ToString()))
                        {
                            await WriteAlreadyParticipatedToDB(Session["PROLIFIC_PID"].ToString(), Session["STUDY_ID"].ToString(), Session["SESSION_ID"].ToString(), Constants.APK_VER);
                            return View("AlreadyParticipated");
                        }
                        // VALID WORKER! can start working.
                        else
                        {

                        }
                    }
                }
                await WriteDetailsToDB(Session["PROLIFIC_PID"].ToString(), Session["STUDY_ID"].ToString(), Session["SESSION_ID"].ToString(),
                         Session["start_time_server"].ToString(), Constants.APK_VER);
            }
            catch (Exception e)
            {
                String PROLIFIC_PID = "";
                if (Session["PROLIFIC_PID"] != null)
                {
                    PROLIFIC_PID = Session["PROLIFIC_PID"].ToString();
                }
                Constants.WriteErrorToDB(PROLIFIC_PID, "HomeIndex", e.Message, e.StackTrace);
            }

            // debug or real participant - start the experiment!
            return View("Introduction");
        }

        public async Task ConsentData(String pageStartTimeClient, Boolean agreed, String clickTimeClient)
        {
            try
            {
                // WRITE DETAILS TO DB.
                var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
                var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
                var collectionResults = DB.GetCollection<BsonDocument>("results");
                IMongoCollection<BsonDocument> workersCollection;

                if (agreed)
                {
                    workersCollection = DB.GetCollection<BsonDocument>("participants-agreed");
                }
                else
                {
                    workersCollection = DB.GetCollection<BsonDocument>("participants-disagreed");
                }

                var workerDoc = new BsonDocument
            {
                {"PROLIFIC_PID", Session["PROLIFIC_PID"].ToString()},
                {"STUDY_ID", Session["STUDY_ID"].ToString()},
                {"SESSION_ID", Session["SESSION_ID"].ToString()},
                {"game_variant", Constants.GAME_VARIANT},
                {"apk_version", Constants.APK_VER}
            };
                await workersCollection.InsertOneAsync(workerDoc);

                ///////////////////////

                if (agreed)
                {
                    // write to the results collection.
                    BsonDocument browser_details = GetBrowserDetails();
                    string startTimeServer = Session["start_time_server"].ToString();
                    var times = new BsonDocument
                    {
                        {"start_time_server", startTimeServer},
                        {"loading_time", Constants.GetTimeDiff(startTimeServer, pageStartTimeClient) },
                        {"start_time_client", pageStartTimeClient},
                        {"end_time_client", clickTimeClient},
                        {"page_time", Constants.GetTimeDiff(pageStartTimeClient, clickTimeClient) },
                    };
                    var consent_page = new BsonDocument
                    {
                        {"times", times},
                    };

                    var pages = new BsonDocument
                    {
                        {"consent_page", consent_page},
                    };

                    var documnt = new BsonDocument
                    {
                        {"PROLIFIC_PID", Session["PROLIFIC_PID"].ToString()},
                        {"STUDY_ID", Session["STUDY_ID"].ToString() },
                        {"SESSION_ID", Session["SESSION_ID"].ToString()},
                        {"game_variant", Constants.GAME_VARIANT},
                        {"apk_version", Constants.APK_VER},
                        {"pages", pages},
                        {"experiment_completed", false},
                        {"browser_details", browser_details},
                        {"ip_address", GetIPAddress()},
                        {"validity", "1" } 
                    };

                    await collectionResults.InsertOneAsync(documnt);
                }
            }
            
            catch(Exception e)
            {
                String PROLIFIC_PID = "";
                if (Session["PROLIFIC_PID"] != null)
                {
                    PROLIFIC_PID = Session["PROLIFIC_PID"].ToString();
                }
                Constants.WriteErrorToDB(PROLIFIC_PID, "ConsentData", e.Message, e.StackTrace);
            }
        }

        public ActionResult Error()
        {
            //String lastScreen = Request.QueryString["lastScreen"];

            return View();
        }

        public async Task WriteDetailsToDB(String PROLIFIC_PID, String STUDY_ID, String SESSION_ID,
            String accept_hit_time, String apk_version)
        {
            // WRITE DETAILS TO DB.
            var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
            var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
            //var collection = DB.GetCollection<BsonDocument>("results");

            IMongoCollection<BsonDocument> workersCollection = DB.GetCollection<BsonDocument>("participants-all"); ;
            
            var workerDoc = new BsonDocument
            {
                {"PROLIFIC_PID", PROLIFIC_PID},
                {"STUDY_ID", STUDY_ID},
                {"SESSION_ID", SESSION_ID},
                {"game_variant", Constants.GAME_VARIANT},
                {"apk_version", apk_version},
                {"start_time_server", accept_hit_time},
            };
            await workersCollection.InsertOneAsync(workerDoc);

        }

        protected string GetIPAddress()
        {
            return Request.UserHostAddress;

            /*
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
            */
            
            //HttpContext.Current.Request.UserHostAddress;
        }

        

        static async Task WriteAlreadyParticipatedToDB(String PROLIFIC_PID, String STUDY_ID, String SESSION_ID, String apk_version)
        {
            // WRITE DETAILS TO DB.
            var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
            var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
            var collection = DB.GetCollection<BsonDocument>("already-participated");

            String time = Constants.getTimeStamp();

            var documnt = new BsonDocument
            {
                {"current_apk_version", apk_version},
                {"PROLIFIC_PID", PROLIFIC_PID},
                {"STUDY_ID", STUDY_ID},
                {"SESSION_ID", SESSION_ID},
                {"start_time_server", time},
            };

            await collection.InsertOneAsync(documnt);
        }

        public Boolean IsNewWorker(String PROLIFIC_PID)
        {
            var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
            var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
            var workersCollection = DB.GetCollection<BsonDocument>("participants-agreed");
            var filter = Builders<BsonDocument>.Filter.Eq("PROLIFIC_PID", PROLIFIC_PID);
            var results = workersCollection.Find(filter).ToList();
            if (results.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public ActionResult PersonalDetails()
        {
            return View();
        }

        public ActionResult Disagree()
        {
            return View();
        }

        public ActionResult QuizFailed()
        {
            return View();
        }

        static async Task WriteRestrictedAcceptToDB(String reason, String PROLIFIC_PID, String STUDY_ID, String SESSION_ID, String apk_version)
        {
            // WRITE DETAILS TO DB.
            var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
            var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
            var collection = DB.GetCollection<BsonDocument>("restricted-access");

            String time = Constants.getTimeStamp();

            var documnt = new BsonDocument
            {
                {"reason", reason},
                {"apk_version", apk_version},
                {"PROLIFIC_PID", PROLIFIC_PID},
                {"STUDY_ID", STUDY_ID},
                {"SESSION_ID", SESSION_ID},
                {"start_time_server", time},
                {"user_interface", "Web"}
            };

            await collection.InsertOneAsync(documnt);
        }

        public BsonDocument GetBrowserDetails()
        {
            System.Web.HttpBrowserCapabilitiesBase browser = Request.Browser;
            var browser_details = new BsonDocument
            {
                {"Type", browser.Type},
                {"Name", browser.Browser},
                {"Version", browser.Version},
                {"Major Version", browser.MajorVersion},
                {"Minor Version", browser.MinorVersion},
                {"Platform", browser.Platform},
                {"Is Beta", browser.Beta},
                {"Is Crawler", browser.Crawler},
                {"Is AOL", browser.AOL},
                {"Supports Frames", browser.Frames},
                {"Supports Tables", browser.Tables},
                {"Supports Cookies", browser.Cookies},
                {"Supports VBScript", browser.VBScript},
                {"Supports JavaScript", browser.EcmaScriptVersion.ToString()},
                {"Supports Java Applets", browser.JavaApplets},
                {"Supports ActiveX Controls", browser.ActiveXControls},
            };
            return browser_details;
        }

        public Boolean IsExplorerUser()
        {
            if ((HttpContext.Request.Browser.Browser == "IE") || (HttpContext.Request.Browser.Browser == "InternetExplorer"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}