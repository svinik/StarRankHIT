using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using StarRankHIT.Models;

namespace StarRankHIT.Controllers
{
    public class SummaryTableController : Controller
    {

        public class RatioComparer : IComparer
        {
            public int Compare(Object q, Object r)
            {
                bool b = ((BsonValue)q)["ratio_group"].ToDouble() < ((BsonValue)r)["ratio_group"].ToDouble();

                if (b) {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }


        // GET: SummaryTable
        public async System.Threading.Tasks.Task<ActionResult> Index()
        {
            var Client = new MongoClient(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ToString());
            IMongoDatabase DB = Client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["dbName"].ToString());
            IMongoCollection<BsonDocument> resultsCollection = DB.GetCollection<BsonDocument>("results");

            var arrayFilter = Builders<BsonDocument>.Filter.Eq("experiment_completed", true) & Builders<BsonDocument>.Filter.Eq("apk_version", "1.2");

            var docs = await resultsCollection.Find(arrayFilter).ToListAsync();

            double ver = Convert.ToDouble(Request.QueryString["ver"]);
            ver = 1.2;

            string type = Request.QueryString["type"];
            if(type == null)
            {
                type = "participants";
                //type = "decisions";
                //type = "error";
            }

            if (type == "decisions")
            {
                ArrayList decisionsData = GetDecisionRecords(docs);
                return View("DecisionsTable", decisionsData);
            }
            else if(type == "participants")
            {
                ArrayList participantsData = GetParticipantsRecords(docs, ver);
                return View("ParticipantsTable", participantsData);
            }
            else
            {
                return View("InvalidArgument");
            }
        }


        public static ArrayList GetDecisionRecords(List<BsonDocument> decisionsDocs)
        {
            ArrayList DecisionsRecords = new ArrayList();

            foreach (var participantResults in decisionsDocs)
            {
                string PROLOFIC_PID = participantResults["workerId"].ToString();
                var infos = participantResults["pages"]["evaluation_page"]["ratings_arr"];

                var T = 0;
                var F = 0;

                foreach (var info in infos.AsBsonArray)
                {
                    T = info["T"].ToInt32();
                    F = info["F"].ToInt32();

                    DecisionRecord decisionRecord = new DecisionRecord()
                    {
                        PROLOFIC_PID = PROLOFIC_PID,
                        index = info["index"].ToInt32(),
                        T = T,
                        F = F,
                        final_rating = info["rating"].ToString(),
                        changes = info["changes"].ToString(),
                        history_rating = info["history"].ToString(),
                        ratio_real = info["ratio_real"].ToDouble(),
                        ratio_group = info["ratio_group"].ToDouble(),
                        min = Math.Min(T, F),
                        max = Math.Max(T, F),
                        sum = T + F,
                        diff = Math.Max(T, F) - Math.Min(T, F)
                };
                    DecisionsRecords.Add(decisionRecord);
                }
            }

            return DecisionsRecords;
        }

        public static ArrayList GetParticipantsRecords(List<BsonDocument> docs, double ver)
        {
            ArrayList participantsRecords = new ArrayList();

            foreach (var participantResults in docs)
            {
                string PROLOFIC_PID = participantResults["workerId"].ToString();

                ParticipantRecord participantRecord = new ParticipantRecord()
                {
                    PROLOFIC_PID = PROLOFIC_PID,
                    gender = participantResults["pages"]["personal_details_page"]["final_answers"]["gender"].ToString(),
                    age = participantResults["pages"]["personal_details_page"]["final_answers"]["age"].ToString(),
                    country = participantResults["pages"]["personal_details_page"]["final_answers"]["country"].ToString(),

                    mistakes_captcha_page = participantResults["pages"]["personal_details_page"]["mistakes"].ToString(),
                    mistakes_quiz_page = participantResults["pages"]["welcome_page"]["mistakes"].ToString(),
                    unclear = participantResults["pages"]["welcome_page"]["Q_unclear"].ToString(),
                    reasoning = participantResults["pages"]["feedback_page"]["Q-reasoning"].ToString(),
                    bugs = participantResults["pages"]["feedback_page"]["Q-bugs"].ToString(),
                    comments = participantResults["pages"]["feedback_page"]["Q-comments"].ToString(),

                    total_time = participantResults["experiment_total_time"].ToString(),
                    consent_time = participantResults["pages"]["consent_page"]["times"]["page_time"].ToString(),
                    personal_details_time = participantResults["pages"]["personal_details_page"]["times"]["page_time"].ToString(),
                    instructions_time = participantResults["pages"]["welcome_page"]["times"]["instructions_time"].ToString(),
                    example_time = participantResults["pages"]["welcome_page"]["times"]["example_time"].ToString(),
                    quiz_time = participantResults["pages"]["welcome_page"]["times"]["quiz_time"].ToString(),
                    evaluations_time = participantResults["pages"]["evaluation_page"]["times"]["page_time"].ToString(),
                    feedback_time = participantResults["pages"]["feedback_page"]["times"]["page_time"].ToString(),

                    IP = participantResults["ip_address"].ToString(),
                    browser = participantResults["browser_details"]["Type"].ToString(),

                    start_time = participantResults["pages"]["consent_page"]["times"]["start_time_server"].ToString(),
                    //start_time_participant = participantResults["pages"]["consent_page"]["times"]["start_time_participant"].ToString()

                };

                BsonArray rating_arr = participantResults["pages"]["evaluation_page"]["ratings_arr"].AsBsonArray;



                if (ver == 1.1)
                {
                    var sorted_rating_arr = rating_arr.OrderBy(t => t.AsBsonDocument["sum"]); // sort by ratio_group

                    //int currRatio = sorted_rating_arr[0]["ratio_group"];
                    double prevRatioGroup = -1;
                    ArrayList bigArray = new ArrayList();
                    ArrayList array = new ArrayList(); // TODO - one redundant initialization

                    foreach (BsonValue item in sorted_rating_arr)
                    {
                        if (item["sum"].ToDouble() != prevRatioGroup)
                        {
                            if (array.Count != 0)
                            {
                                array.Sort(new RatioComparer());
                                bigArray.Add(array);
                            }
                            array = new ArrayList();
                        }

                        array.Add(item);
                        prevRatioGroup = item["sum"].ToDouble();
                    }
                    if (array.Count > 0) // the last one, if exists
                    {
                        array.Sort(new RatioComparer());
                        bigArray.Add(array);
                    }

                    int softInconsistency = 0;
                    int bigInconsistency = 0;

                    string reasonSoftInconsistency = "";
                    string reasonBigInconsistency = "";

                    for (int i = 0; i < bigArray.Count; i++)
                    {
                        ArrayList innerArr = (ArrayList)bigArray[i];
                        for (int j = 0; j < innerArr.Count; j++)
                        {
                            if (j - 1 >= 0)
                            {
                                BsonValue a = (BsonValue)innerArr[j - 1];
                                var sumA = a["sum"].ToInt32();
                                var ratioA = a["ratio_group"].ToDouble();
                                var ratingA = a["rating"].ToInt32();

                                BsonValue b = (BsonValue)innerArr[j];
                                var sumB = b["sum"].ToInt32();
                                var ratioB = b["ratio_group"].ToDouble();
                                var ratingB = b["rating"].ToInt32();

                                if (ratingB > ratingA)
                                {
                                    if (reasonSoftInconsistency != "")
                                    {
                                        reasonSoftInconsistency += "#";
                                    }
                                    reasonSoftInconsistency += ratingB + " (" + b["T"] + ":" + b["F"] + "," + ratioB + "," + sumB + ")"
                                        + " > "
                                        + ratingA + " (" + a["T"] + ":" + a["F"] + "," + ratioA + "," + sumA + ")";

                                    softInconsistency++;
                                }
                            }
                            if (j - 2 >= 0)
                            {
                                BsonValue a = (BsonValue)innerArr[j - 2];
                                var sumA = a["sum"].ToInt32();
                                var ratioA = a["ratio_group"].ToDouble();
                                var ratingA = a["rating"].ToInt32();

                                BsonValue b = (BsonValue)innerArr[j];
                                var sumB = b["sum"].ToInt32();
                                var ratioB = b["ratio_group"].ToDouble();
                                var ratingB = b["rating"].ToInt32();


                                if (ratingB > ratingA)
                                {
                                    if (reasonBigInconsistency != "")
                                    {
                                        reasonBigInconsistency += "#";
                                    }
                                    reasonBigInconsistency += ratingB + " (" + b["T"] + ":" + b["F"] + "," + ratioB + "," + sumB + ")"
                                        + " > "
                                        + ratingA + " (" + a["T"] + ":" + a["F"] + "," + ratioA + "," + sumA + ")";

                                    bigInconsistency++;
                                }
                            }
                        }
                    }

                    participantRecord.soft_inconsistency_num = softInconsistency;
                    participantRecord.soft_inconsistency_str = reasonSoftInconsistency;

                    participantRecord.big_inconsistency_num = bigInconsistency;
                    participantRecord.big_inconsistency_str = reasonBigInconsistency;
                }
                /////////////////
                ///


                else if (ver == 1.2)
                {
                    int inconsistency = 0;
                    int xSum, ySum, xRating, yRating;
                    double yRatio, xRatio;
                    string reasonInconsistency = "";
                    foreach (BsonValue x in rating_arr)
                    {
                        xSum = x["sum"].ToInt32();
                        xRatio = x["ratio_real"].ToDouble();
                        xRating = x["rating"].ToInt32();

                        foreach (BsonValue y in rating_arr)
                        {
                            ySum = y["sum"].ToInt32();
                            yRatio = y["ratio_real"].ToDouble();
                            yRating = y["rating"].ToInt32();

                            if ((yRating < xRating) && (ySum > xSum) && (yRatio < xRatio))
                            {
                                if (reasonInconsistency != "")
                                {
                                    reasonInconsistency += "#";
                                }
                                reasonInconsistency += xRating + " (" + x["T"] + ":" + x["F"] + "," + xRatio + "," + xSum + ")"
                                    + " > "
                                    + yRating + " (" + y["T"] + ":" + y["F"] + "," + yRatio + "," + ySum + ")";

                                inconsistency++;
                            }

                        }
                    }
                    participantRecord.big_inconsistency_num = inconsistency;
                    participantRecord.big_inconsistency_str = reasonInconsistency;
                }

                participantsRecords.Add(participantRecord);
            }

            return participantsRecords;
        }
    }
}