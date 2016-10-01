using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class AccessToken
    {
        [Key]
        public AccessTokenType Type { get; set; }

        [StringLength(255)]
        [Required]
        public string Value { get; set; }

        public DateTime? Expire { get; set; }
    }
}
