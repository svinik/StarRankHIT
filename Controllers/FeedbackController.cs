using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net.Http;
using System.Collections.Generic;

namespace StarRankHIT.Controllers
{
    public class FeedbackController : Controller
    {

        private static readonly HttpClient client = new HttpClient();

        // GET: Feedback
        public ActionResult Index()
        {
            Session["feedback_time_server"] = Constants.getTimeStamp();
            string submitUrl = SubmitUrl();

            return View("Feedback", null, submitUrl);
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

                var filter = Builders<BsonDocument>.Filter.Eq("workerId", Session["workerId"].ToString());
                var update = Builders<BsonDocument>.Update.Set("pages.feedback_page", feedback_page).Set("experiment_total_time", Constants.GetTimeDiff(Session["start_time_server"].ToString(), feedbackEndTime)).Set("experiment_completed", true);
                await collectionResults.UpdateOneAsync(filter, update);
            }
            catch (Exception e)
            {
                String workerId = "";
                if (Session["workerId"] != null)
                {
                    workerId = Session["workerId"].ToString();
                }
                Constants.WriteErrorToDB(workerId, "FeedbackData", e.Message, e.StackTrace);
            }
        }

        private string SubmitUrl()
        {
            string mturkUrl = System.Configuration.ConfigurationManager.AppSettings["mturkUrl"].ToString();

            return mturkUrl + "?assignmentId=" + Session["assignmentId"].ToString()
                + "&foo=bar";
        }
    }
}