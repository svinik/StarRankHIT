using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StarRankHIT.Models
{
    public class ParticipantRecord
    {
        public string PROLOFIC_PID { get; set; }
        public string gender { get; set; }
        public string age { get; set; }
        public string country { get; set; }
        public string mistakes_captcha_page { get; set; } //
        public string mistakes_quiz_page { get; set; } //
        public string unclear { get; set; } //
        public string reasoning { get; set; } //
        public string bugs { get; set; } //
        public string comments { get; set; } //

        public string total_time { get; set; }
        public string consent_time { get; set; }
        public string personal_details_time { get; set; }
        public string instructions_time { get; set; }
        public string example_time { get; set; }
        public string quiz_time { get; set; }
        public string evaluations_time { get; set; }
        public string feedback_time { get; set; }
        public string IP { get; set; }
        public string browser { get; set; }
        public string start_time { get; set; }
        public string start_time_participant { get; set; }

        public int soft_inconsistency_num { get; set; } = -1;

        public string soft_inconsistency_str { get; set; } = "N/A";
        public int big_inconsistency_num { get; set; } = -1;

        public string big_inconsistency_str { get; set; } = "N/A";

    }
}