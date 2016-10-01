using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class ChatterInfo
    {
        [Key]
        public int ChatterId { get; set; }

        [ForeignKey("ChatterId")]
        public virtual Chatter Chatter { get; set; }

        public long? SteamId { get; set; }

        [DefaultValue(typeof(long), "0")]
        public long Seconds { get; set; }

        [DefaultValue(typeof(int), "0")]
        public int QuizScore { get; set; }
    }
}
