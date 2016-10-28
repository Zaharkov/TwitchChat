using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class RouletteInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [DefaultValue(typeof(int), "0")]
        public int CurrentTry { get; set; }

        [DefaultValue(typeof(int), "0")]
        public int Streak { get; set; }

        [DefaultValue(typeof(int), "0")]
        public int MaxStreak { get; set; }

        [DefaultValue(typeof(int), "0")]
        public int TryCount { get; set; }

        [DefaultValue(typeof(int), "0")]
        public int DeathCount { get; set; }

        [DefaultValue(typeof(double), "1")]
        public double Percent { get; set; }

        [DefaultValue(typeof(double), "1")]
        public double MaxPercent { get; set; }

        [StringLength(255)]
        public string DuelName { get; set; }

        [DefaultValue(typeof(int), "0")]
        public int DuelScore { get; set; }
    }
}
