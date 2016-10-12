using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class ChatterInfo
    {
        [Key, Column(Order = 0)]
        [StringLength(255)]
        public string Name { get; set; }

        [Key, Column(Order = 1)]
        [StringLength(255)]
        public string ChatName { get; set; }

        public long? SteamId { get; set; }

        [DefaultValue(typeof(long), "0")]
        public long Seconds { get; set; }

        [DefaultValue(typeof(int), "0")]
        public int QuizScore { get; set; }

        public long? RouletteId { get; set; }
    }
}
