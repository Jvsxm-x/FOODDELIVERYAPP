using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace FoodDeliveryApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public required string Role { get; set; } // Customer, Admin, Delivery

        public bool IsEmailVerified { get; set; }

        public ICollection<Order>? Orders { get; set; }
    }

}