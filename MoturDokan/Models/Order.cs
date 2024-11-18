using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MoturDokan.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        [Required]
        public string CustomerName { get; set; } = "";

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }
    }
}
