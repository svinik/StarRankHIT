using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StarRankHIT.Models
{
    public class DistributionPair
    {
        public ObjectId _id;
        public Distribution first;
        public Distribution second;
        public string category;
        public string site;
        public string type;
    }


    public class Distribution
    {
        public int star1;
        public int star2;
        public int star3;
        public int star4;
        public int star5;
        public string name;
        public string url;
    }
}