﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhipStat.Models.LegTech
{
    public class Testimony
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organization { get; set; }
        public int? OrgId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Biennium { get; set; }
        public string Agency { get; set; }
        public string CommitteeName { get; set; }
        public DateTime MeetingDate { get; set; }
        public string BillId { get; set; }
        public short? BillNumber { get; set; }
        public string AbbrTitle { get; set; }
        public string Position { get; set; }
        public bool Testify { get; set; }
        public bool OutOfTown { get; set; }
        public bool CalledUp { get; set; }
        public bool NoShow { get; set; }

        public override string ToString() => $"{LastName}, {FirstName}";
    }
}
