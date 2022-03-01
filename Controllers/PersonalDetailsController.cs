using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace StarRankHIT.Views.Home
{
    public class PersonalDetailsController : Controller
    {
        // GET: PersonalDetails
        public ActionResult Index()
        {
            Session["personal_start_time_server"] = Constants.getTimeStamp();
            return View("PersonalDetails");
        }

        public async Task PersonalDetailsData(string pageStartTimeClient, string clickTimeClient,
            string ageAll, string genderAll, string countryAll, string capAll, string secondsAll,
            string age, string gender, string country, string cap, string seconds, int mistakes)
        {
            try
            {
                // WRITE DETAILS TO DB.
                var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
                var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
                var collectionResults = DB.GetCollection<BsonDocument>("results");

                // write to the results collection.
                string startTimeServer = Session["personal_start_time_server"].ToString();
                var times = new BsonDocument
                    {
                    {"start_time_server", startTimeServer},
                    {"loading_time", Constants.GetTimeDiff(startTimeServer, pageStartTimeClient) },
                    {"start_time_client", pageStartTimeClient},
                    {"end_time_client", clickTimeClient},
                    {"page_time", Constants.GetTimeDiff(pageStartTimeClient, clickTimeClient) },
                };

                var answersHistory = new BsonDocument
                {
                    {"age", ageAll},
                    {"gender", genderAll},
                    {"country", countryAll},
                    {"captcha", capAll},
                    {"seconds", secondsAll},
                };

                var answersFinal = new BsonDocument
                {
                    {"age", age},
                    {"gender", gender},
                    {"country", country},
                    {"captcha", cap},
                    {"seconds", seconds},
                };

                var personal_details_page = new BsonDocument
                {
                    {"times", times},
                    {"answers_history", answersHistory},
                    {"final_answers", answersFinal},
                    {"mistakes", mistakes}
                };

                var filter = Builders<BsonDocument>.Filter.Eq("PROLIFIC_PID", Session["PROLIFIC_PID"].ToString());
                var update = Builders<BsonDocument>.Update.Set("pages.personal_details_page", personal_details_page);
                await collectionResults.UpdateOneAsync(filter, update);
            }
            catch(Exception e)
            {
                String PROLIFIC_PID = "";
                if (Session["PROLIFIC_PID"] != null)
                {
                    PROLIFIC_PID = Session["PROLIFIC_PID"].ToString();
                }
                Constants.WriteErrorToDB(PROLIFIC_PID, "PersonalDetailsData", e.Message, e.StackTrace);
            }
        }
    }
}