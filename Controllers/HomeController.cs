using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace StarRankHIT.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            try
            {
                Session["assignmentId"] = Request.QueryString["assignmentId"];
                Session["hitId"] = Request.QueryString["hitId"];
                Session["workerId"] = Request.QueryString["workerId"];

                // manage debugging situations.
                if (Session["workerId"] == null)
                {
                    Session["workerId"] = Constants.EMPTY_WORKER_ID;
                }
                if (Session["hitId"] == null)
                {
                    Session["hitId"] = Constants.EMPTY_HIT_ID;
                }
                if (Session["assignmentId"] == null)
                {
                    Session["assignmentId"] = Constants.EMPTY_ASSIGNMENT_ID;
                }
                
                String userAgent = "";
                if(Request.UserAgent != null)
                {
                    userAgent = Request.UserAgent;
                }
                // prevent mobile devices.
                if (Constants.IsMobileUser(userAgent.Trim().ToLower())) {
                    await WriteRestrictedAcceptToDB("Mobile", Session["workerId"].ToString(), Session["hitId"].ToString(), Session["assignmentId"].ToString(), Constants.APK_VER);
                    return View("Mobile");
                }

                // prevent Internet Explorer browsers. 
                if (IsExplorerUser())
                {
                    // relevant in debug when no assigmentId was passed. Not necessary for Preview mode (there ASSIGNMENT_ID_NOT_AVAILABLE is passed).
                    await WriteRestrictedAcceptToDB("Explorer", Session["workerId"].ToString(), Session["hitId"].ToString(), Session["assignmentId"].ToString(), Constants.APK_VER);
                    return View("Explorer");
                };

                Session["start_time_server"] = Constants.getTimeStamp();

                // prevent multiple participation (except for in debug mode)
                if (Session["workerId"].ToString() != null)
                {
                    if (Session["workerId"].ToString() != Constants.EMPTY_WORKER_ID)
                    {
                        // ignore past workers from playing again.
                        if (!IsNewWorker(Session["workerId"].ToString()))
                        {
                            await WriteAlreadyParticipatedToDB(Session["workerId"].ToString(), Session["hitId"].ToString(), Session["assignmentId"].ToString(), Constants.APK_VER);
                            return View("AlreadyParticipated");
                        }
                        // VALID WORKER! can start working.
                        else
                        {

                        }
                    }
                }
                await WriteDetailsToDB(Session["workerId"].ToString(), Session["hitId"].ToString(), Session["assignmentId"].ToString(),
                         Session["start_time_server"].ToString(), Constants.APK_VER);
            }
            catch (Exception e)
            {
                String workerId = "";
                if (Session["workerId"] != null)
                {
                    workerId = Session["workerId"].ToString();
                }
                Constants.WriteErrorToDB(workerId, "HomeIndex", e.Message, e.StackTrace);
            }

            // debug or real participant - start the experiment!
            return View("Introduction");
        }

        public async Task ConsentData(String pageStartTimeClient, Boolean agreed, String clickTimeClient)
        {
            /*
            if (Constants.EMPTY_ASSIGNMENT_ID.Equals(Session["assignmentId"].ToString()))
            {
                return Json(true);
            }
            */

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
                {"workerId", Session["workerId"].ToString()},
                {"hitId", Session["hitId"].ToString()},
                {"assignmentId", Session["assignmentId"].ToString()},
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
                        {"workerId", Session["workerId"].ToString()},
                        {"hitId", Session["hitId"].ToString() },
                        {"assignmentId", Session["assignmentId"].ToString()},
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
                String workerId = "";
                if (Session["workerId"] != null)
                {
                    workerId = Session["workerId"].ToString();
                }
                Constants.WriteErrorToDB(workerId, "ConsentData", e.Message, e.StackTrace);
            }
          
            //return Json(false);
        }

        public ActionResult Error()
        {
            //String lastScreen = Request.QueryString["lastScreen"];

            return View();
        }

        public async Task WriteDetailsToDB(String workerId, String hitId, String assignmentId,
            String accept_hit_time, String apk_version)
        {
            // WRITE DETAILS TO DB.
            var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
            var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
            //var collection = DB.GetCollection<BsonDocument>("results");

            IMongoCollection<BsonDocument> workersCollection = DB.GetCollection<BsonDocument>("participants-all"); ;
            
            var workerDoc = new BsonDocument
            {
                {"workerId", workerId},
                {"hitId", hitId},
                {"assignmentId", assignmentId},
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

        

        static async Task WriteAlreadyParticipatedToDB(String workerId, String hitId, String assignmentId, String apk_version)
        {
            // WRITE DETAILS TO DB.
            var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
            var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
            var collection = DB.GetCollection<BsonDocument>("already-participated");

            String time = Constants.getTimeStamp();

            var documnt = new BsonDocument
            {
                {"current_apk_version", apk_version},
                {"workerId", workerId},
                {"hitId", hitId},
                {"assignmentId", assignmentId},
                {"start_time_server", time},
            };

            await collection.InsertOneAsync(documnt);
        }

        public Boolean IsNewWorker(String workerId)
        {
            var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
            var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
            var workersCollection = DB.GetCollection<BsonDocument>("participants-agreed");
            var filter = Builders<BsonDocument>.Filter.Eq("workerId", workerId);
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

        public ActionResult Preview()
        {
            return View();
        }

        public ActionResult QuizFailed()
        {
            return View();
        }

        static async Task WriteRestrictedAcceptToDB(String reason, String workerId, String hitId, String assignmentId, String apk_version)
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
                {"workerId", workerId},
                {"hitId", hitId},
                {"assignmentId", assignmentId},
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