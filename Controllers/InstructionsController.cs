using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace StarRankHIT.Controllers
{
    public class InstructionsController : Controller
    {
        // GET: Instructions
        public ActionResult Index()
        {
            Session["instructions_time_server"] = Constants.getTimeStamp();
            return View("Instructions");
        }

        public async Task InstructionsData(string instructionsStartTime, string pageEndTime, string selectedOption)
        {
            try
            {
                // WRITE DETAILS TO DB.
                var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
                var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
                var collectionResults = DB.GetCollection<BsonDocument>("results");

                // write to the results collection.
                string startTimeServer = Session["instructions_time_server"].ToString();
                var times = new BsonDocument
                {
                    {"start_time_server", startTimeServer},
                    {"loading_time", Constants.GetTimeDiff(startTimeServer, instructionsStartTime)},
                    {"start_time_client", instructionsStartTime},
                    {"end_time_client", pageEndTime},
                    {"entire_page_time", Constants.GetTimeDiff(instructionsStartTime, pageEndTime)},
                };
                var welcome_page = new BsonDocument
                {
                    {"times", times},
                    {"selected_option", selectedOption},
                };

                var filter = Builders<BsonDocument>.Filter.Eq("PROLIFIC_PID", Session["PROLIFIC_PID"].ToString());
                var update = Builders<BsonDocument>.Update.Set("pages.welcome_page", welcome_page);
                await collectionResults.UpdateOneAsync(filter, update);
            }
            catch (Exception e)
            {
                //return e.ToString();
                var g = 4;
            }
        }
    }
}