﻿using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryApp.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative.")]
        public decimal UnitPrice { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int FoodItemId { get; set; }
        public FoodItem? FoodItem { get; set; }
    }

}