using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Time { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string Exception { get; set; }
    }
}
