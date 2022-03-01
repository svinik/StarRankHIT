using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StarRankHIT.Models
{
    public class DecisionRecord
    {
        public string PROLOFIC_PID { get; set; }
        public int index { get; set; }
        public double ratio_group { get; set; }
        public double ratio_real { get; set; }
        public int sum { get; set; }

        public int T { get; set; }
        public int F { get; set; }
        public string final_rating { get; set; }
        public string history_rating { get; set; }

        public string changes { get; set; }

        public int min { get; set; }
        public int max { get; set; }
        public int diff { get; set; }
    }
}