using System.ComponentModel.DataAnnotations;

namespace MoturDokan.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; } = "";

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal Stock { get; set; }
    }
}
