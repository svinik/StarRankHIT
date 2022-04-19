using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Web.Mvc;
using StarRankHIT.Models;
using System.Collections.Generic;
using System;

namespace StarRankHIT.Controllers
{
    public class EvaluationController : Controller
    {
        // GET: Evaluation
        public ActionResult Evaluation()
        {
            DistributionPair[] info = fetchInfo();

            Session["evaluation_time_server"] = Constants.getTimeStamp();
            return View(info);
        }

        public DistributionPair[] fetchInfo()
        {
            var numQuestions = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["numQuestions"].ToString());
            var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
            var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
            var collectionResults = DB.GetCollection<DistributionPair>("distribution-pairs");

            // TODO: add count consideration to sampling
            var sample = new BsonDocument 
            { 
                { 
                    "$sample",
                    new BsonDocument
                    {
                        {"size", numQuestions}
                    }
                } 
            };
            var pipeline = new[] { sample };
            var result = collectionResults.Aggregate<DistributionPair>(pipeline).ToList().ToArray();

            return result;
        }


        public void RandomOrder(ArrayList arrList)
        {
            Random r = new Random();
            for (int cnt = 0; cnt < arrList.Count; cnt++)
            {
                object tmp = arrList[cnt];
                int idx = r.Next(arrList.Count - cnt) + cnt;
                arrList[cnt] = arrList[idx];
                arrList[idx] = tmp;
            }

        }

        public async Task EvaluationData(string startDate, string evaluationStartTime, string evaluationEndTime, string decisions, string decisionsStr)
        {
            try
            {
                // WRITE DETAILS TO DB.
                var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
                var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
                var collectionResults = DB.GetCollection<BsonDocument>("results");

                // write to the results collection.
                string startTimeServer = Session["evaluation_time_server"].ToString();
                var times = new BsonDocument
                {
                    {"start_date", startDate},
                    {"start_time_server", startTimeServer},
                    {"loading_time", Constants.GetTimeDiff(startTimeServer, evaluationStartTime)},
                    {"start_time_client", evaluationStartTime},
                    {"end_time_client", evaluationEndTime},
                    {"page_time", Constants.GetTimeDiff(evaluationStartTime, evaluationEndTime)},
                };

                var serializer = new BsonArraySerializer();

                var jsonReaderDecisions = new JsonReader(decisions);
                var bsonArrayDecisions = serializer.Deserialize(BsonDeserializationContext.CreateRoot(jsonReaderDecisions));

                var evaluation_page = new BsonDocument
                {
                    {"times", times},
                    {"decisions_arr", bsonArrayDecisions},
                    {"decisions_arr_str", decisionsStr}                };

                var filter = Builders<BsonDocument>.Filter.Eq("workerId", Session["workerId"].ToString());
                var update = Builders<BsonDocument>.Update.Set("pages.evaluation_page", evaluation_page);
                await collectionResults.UpdateOneAsync(filter, update);
            }
            catch (Exception e)
            {
                String workerId = "";
                if (Session["workerId"] != null)
                {
                    workerId = Session["workerId"].ToString();
                }
                Constants.WriteErrorToDB(workerId, "EvaluationData", e.Message, e.StackTrace);
            }
        }


        public ActionResult Replay()
        {
            string workerId = Request.QueryString["workerId"].ToString();
            return View(fetchInfo());
        }
    }
}