using System.ComponentModel.DataAnnotations;

namespace WhipStat.Models.LegTech
{
    public class Measure
    {
        [Key]
        public int Id { get; set; }
        public short Year { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short? BillNumber { get; set; }
        public string Biennium { get; set; }
    }
}