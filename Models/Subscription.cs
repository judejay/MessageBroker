using System.ComponentModel.DataAnnotations;

namespace MesssageBroker.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

[Required]
        public int TpoicId { get; set; }
    }
}