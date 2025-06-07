using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryApp.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Payment method is required.")]
        public required string PaymentMethod { get; set; } // Card, Cash, Paypal

        public bool IsPaid { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }

}