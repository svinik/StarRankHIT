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
using Weighted_Randomizer;

namespace StarRankHIT.Controllers
{
    public class EvaluationController : Controller
    {
        // GET: Evaluation
        public ActionResult Evaluation()
        {
            DistributionPair[] info = FetchInfo();

            Session["evaluation_time_server"] = Constants.getTimeStamp();
            return View(info);
        }

        class DistributionPairRandComparer: IComparer<DistributionPair> {
            // each comparator has a different seed
            private readonly int seed = new Random().Next();

            public int Compare(DistributionPair x, DistributionPair y)
            {
                int result = x.count - y.count;
                if (result == 0)
                {
                    // every time we compare a particular pair, the same random number will be produced
                    int xValue = new Random(seed * x.GetHashCode()).Next();
                    int yValue = new Random(seed * y.GetHashCode()).Next();
                    return xValue - yValue;
                }
                return result;
            }
        }

        public DistributionPair[] FetchInfo()
        {
            var numQuestions = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["numQuestions"].ToString());
            var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
            var DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
            var collectionResults = DB.GetCollection<DistributionPair>("distribution-pairs");

            List<DistributionPair> allPairs = collectionResults.Find(_ => true).ToList();
            IComparer<DistributionPair> comparer = new DistributionPairRandComparer();
            allPairs.Sort(comparer);

            return allPairs.GetRange(0, numQuestions).ToArray();
        }

        public async Task EvaluationData(string startDate, string evaluationStartTime, string evaluationEndTime, string decisions, bool passedValidation)
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
                    {"passed_validation", passedValidation }
                };

                var filter = Builders<BsonDocument>.Filter.Eq("workerId", Session["workerId"].ToString());
                var update = Builders<BsonDocument>.Update.Set("pages.evaluation_page", evaluation_page);
                await collectionResults.UpdateOneAsync(filter, update);

                // update decisions counts
                var pairsDB = DB.GetCollection<DistributionPair>("distribution-pairs");
                var decisionsArr = bsonArrayDecisions.ToArray();
                for (int i = 0; i < decisionsArr.Length; i++)
                {
                    var filterPairs = Builders<DistributionPair>.Filter.Eq("_id", new ObjectId(bsonArrayDecisions[i]["id"].AsString));
                    var updatePair = Builders<DistributionPair>.Update.Inc("count", 1);

                    await pairsDB.UpdateOneAsync(filterPairs, updatePair);
                }
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
            return View(FetchInfo());
        }
    }
}