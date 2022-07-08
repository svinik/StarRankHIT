using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StarRankHIT.Models
{
    public class DistributionPair : IComparable<DistributionPair>
    {
        public ObjectId _id;
        public Distribution first;
        public Distribution second;
        public string category;
        public string site;
        public string type;
        public int count;

        public int CompareTo(DistributionPair other)
        {
            return this._id.CompareTo(other._id);
        }
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