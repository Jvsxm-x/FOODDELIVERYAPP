using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace FoodDeliveryApp.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public required string Status { get; set; } // Pending, Preparing, Delivered

        public int UserId { get; set; }
        public User? User { get; set; }

        public Delivery? Delivery { get; set; }
        public Payment? Payment { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }

}