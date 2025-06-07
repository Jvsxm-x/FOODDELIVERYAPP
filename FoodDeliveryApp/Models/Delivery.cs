using System.ComponentModel.DataAnnotations;
using System;

namespace FoodDeliveryApp.Models
{
    public class Delivery
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public required string Address { get; set; }

        [Required(ErrorMessage = "Estimated delivery time is required.")]
        public DateTime EstimatedDeliveryTime { get; set; }

        public int? DeliveryUserId { get; set; }
        public User? DeliveryUser { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }

}