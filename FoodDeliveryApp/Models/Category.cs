using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FoodDeliveryApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public required string Name { get; set; }

        public ICollection<FoodItem>? FoodItems { get; set; }
    }

}