using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.Linq;
using System.Web.Mvc;
using StarRankHIT.Models;

namespace StarRankHIT.Controllers
{
    public class ReplayController : Controller
    {
        // GET: Replay
        public ActionResult Index()
        {
            Session["PROLIFIC_PID"] = Request.QueryString["PROLIFIC_PID"];

            //Session["PROLIFIC_PID"] = "EMPTY_PROLIFIC_PID";

            if (Session["PROLIFIC_PID"] == null)
            {
                return View("NoID");
            }
            else
            {
                var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
                var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
                var resultsCollection = DB.GetCollection<BsonDocument>("results");
                var filter = Builders<BsonDocument>.Filter.Eq("PROLIFIC_PID", Session["PROLIFIC_PID"].ToString());
                var results = resultsCollection.Find(filter).ToList();
                if (results.Count == 0)
                {
                    return View("invalidID");
                }
                else
                {
                    ArrayList infos = new ArrayList(); // for data (cards) presentations in html file.
                    BsonDocument resultDoc = results[0];
                    var decisions_info_server = resultDoc["decisions_info_server"];

                    foreach (var decision in decisions_info_server.AsBsonArray)
                    {
                        int T = decision["T"].ToInt32();
                        int F = decision["F"].ToInt32();
                        int Index = decision["index"].ToInt32();

                        infos.Add(new Info() { T = T, F = F, index = Index });
                    }

                    Session["decisions_arr_str"] = resultDoc["pages"]["evaluation_page"]["decisions_arr_str"];
                    Session["start_time_ratings"] = resultDoc["pages"]["evaluation_page"]["times"]["start_date"].ToString();

                    return View("Replay", infos);
                }
            }


            return View();
        }
    }
}