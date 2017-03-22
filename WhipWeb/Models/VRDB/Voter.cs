using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.VRDB
{
    public class Voter
    {
        [Key]
        public string StateVoterID { get; set; }
        public string CountyVoterID { get; set; }
        public string FName { get; set; }
        public string MName { get; set; }
        public string LName { get; set; }
        public string NameSuffix { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Gender { get; set; }
        public string RegStNum { get; set; }
        public string RegStFrac { get; set; }
        public string RegStName { get; set; }
        public string RegStType { get; set; }
        public string RegUnitType { get; set; }
        public string RegStPreDirection { get; set; }
        public string RegStPostDirection { get; set; }
        public string RegUnitNum { get; set; }
        public string RegCity { get; set; }
        public string RegState { get; set; }
        public string RegZipCode { get; set; }
        public string CountyCode { get; set; }
        public int PrecinctCode { get; set; }
        public int PrecinctPart { get; set; }
        public int LegislativeDistrict { get; set; }
        public int CongressionalDistrict { get; set; }
        public string Mail1 { get; set; }
        public string Mail2 { get; set; }
        public string Mail3 { get; set; }
        public string Mail4 { get; set; }
        public string MailCity { get; set; }
        public string MailZip { get; set; }
        public string MailState { get; set; }
        public string MailCountry { get; set; }
        public DateTime? Registrationdate { get; set; }
        public string AbsenteeType { get; set; }
        public DateTime? LastVoted { get; set; }
        public string StatusCode { get; set; }
    }
}
