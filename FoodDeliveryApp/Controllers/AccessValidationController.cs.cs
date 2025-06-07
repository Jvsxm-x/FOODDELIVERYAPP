using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodDeliveryApp.Controllers
{
    public class AccessValidationController : Controller
    {
        private readonly ILogger _logger;

        public AccessValidationController(ILogger<AccessValidationController> logger)
        {
            _logger = logger;
        }

        // GET: AccessValidation/CheckRole
        [HttpGet]
        public IActionResult CheckRole(string role)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to CheckRole without authentication.");
                return Unauthorized(new { Message = "User is not authenticated." });
            }

            bool hasRole = userRole == role;
            return Ok(new { HasRole = hasRole, UserRole = userRole });
        }

        // POST: AccessValidation/CheckPermission
        [HttpPost]
        public IActionResult CheckPermission([FromBody] PermissionRequest request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to CheckPermission without authentication.");
                return Unauthorized(new { Message = "User is not authenticated." });
            }

            bool isAuthorized = IsActionAllowed(userRole, request.Controller, request.Action);
            if (!isAuthorized)
            {
                _logger.LogWarning("Access denied for user with role {UserRole} to {Controller}/{Action}.", userRole, request.Controller, request.Action);
            }

            return Ok(new { IsAuthorized = isAuthorized, UserRole = userRole });
        }

        // GET: AccessValidation/GetAllowedActions
        [HttpGet]
        public IActionResult GetAllowedActions()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to GetAllowedActions without authentication.");
                return Unauthorized(new { Message = "User is not authenticated." });
            }

            var allowedActions = GetPermissionsForRole(userRole);
            return Ok(new { UserRole = userRole, AllowedActions = allowedActions });
        }

        private bool IsActionAllowed(string role, string controller, string action)
        {
            var permissions = GetPermissionsForRole(role);
            var key = $"{controller}/{action}";
            return permissions.Contains(key);
        }

        private List<string> GetPermissionsForRole(string role)
        {
            switch (role)
            {
                case "Admin":
                    return new List<string>
                {
                    "Users/Register",
                    "Users/Login",
                    "Users/Logout",
                    "Restaurants/Create",
                    "Restaurants/Edit",
                    "Restaurants/Delete",
                    "Categories/Create",
                    "Categories/Edit",
                    "Categories/Delete",
                    "FoodItems/Create",
                    "FoodItems/Edit",
                    "FoodItems/Delete",
                    "Orders/Create",
                    "Orders/Edit",
                    "Orders/Delete",
                    "Orders/TrackOrder",
                    "Orders/UpdateStatus",
                    "Deliveries/Create",
                    "Deliveries/Edit",
                    "Deliveries/Delete",
                    "Deliveries/TrackDelivery",
                    "Deliveries/AssignDelivery",
                    "Payments/Create",
                    "Payments/ConfirmPayment",
                    "Home/Profile",
                    "Home/Restaurants",
                    "Home/Categories"
                };
                case "Customer":
                    return new List<string>
                {
                    "Users/Register",
                    "Users/Login",
                    "Users/Logout",
                    "Orders/Create",
                    "Orders/TrackOrder",
                    "Payments/Create",
                    "Home/Profile",
                    "Home/Restaurants",
                    "Home/Categories",
                    "FoodItems/BrowseByCategory",
                    "FoodItems/BrowseByRestaurant",
                    "OrderItems/AddToOrder"
                };
                case "Delivery":
                    return new List<string>
                {
                    "Users/Login",
                    "Users/Logout",
                    "Deliveries/TrackDelivery",
                    "Home/Profile"
                };
                default:
                    return new List<string>();
            }
        }
    }

    public class PermissionRequest
    {
        public string Controller { get; set; }
        public string Action { get; set; }
    }

}