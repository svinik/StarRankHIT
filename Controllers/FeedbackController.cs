using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace StarRankHIT.Controllers
{
    public class FeedbackController : Controller
    {
        // GET: Feedback
        public ActionResult Index()
        {
            Session["feedback_time_server"] = Constants.getTimeStamp();
            return View("Feedback");
        }

        public async Task FeedbackData(String feedbackStartTime, String feedbackEndTime, string reasoning, string affect, string importance, string otherInfo, string issues, int warnings)
        {
            try
            {
                // WRITE DETAILS TO DB.
                var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
                var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
                var collectionResults = DB.GetCollection<BsonDocument>("results");

                // write to the results collection.
                string startTimeServer = Session["feedback_time_server"].ToString();
                var times = new BsonDocument
                    {
                    {"start_time_server", startTimeServer},
                    {"loading_time", Constants.GetTimeDiff(startTimeServer, feedbackStartTime) },
                    {"start_time_client", feedbackStartTime},
                    {"end_time_client", feedbackEndTime},
                    {"page_time", Constants.GetTimeDiff(feedbackStartTime, feedbackEndTime) },
                };
                var feedback_page = new BsonDocument
                {
                    {"times", times},
                    {"Q-reasoning", reasoning},
                    {"Q-affect", affect},
                    {"Q-importance", importance},
                    {"Q-otherInfo", otherInfo},
                    {"Q-issues", issues},
                    {"warnings", warnings}
                };

                var filter = Builders<BsonDocument>.Filter.Eq("PROLIFIC_PID", Session["PROLIFIC_PID"].ToString());
                var update = Builders<BsonDocument>.Update.Set("pages.feedback_page", feedback_page).Set("experiment_total_time", Constants.GetTimeDiff(Session["start_time_server"].ToString(), feedbackEndTime)).Set("experiment_completed", true);
                await collectionResults.UpdateOneAsync(filter, update);
            }
            catch (Exception e)
            {
                String PROLIFIC_PID = "";
                if (Session["PROLIFIC_PID"] != null)
                {
                    PROLIFIC_PID = Session["PROLIFIC_PID"].ToString();
                }
                Constants.WriteErrorToDB(PROLIFIC_PID, "FeedbackData", e.Message, e.StackTrace);
            }
        }
    }
}