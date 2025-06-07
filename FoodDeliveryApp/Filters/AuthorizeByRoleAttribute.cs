using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace FoodDeliveryApp.Filters
{
    public class AuthorizeByRoleAttribute : ActionFilterAttribute
    {
        private readonly string _controller; private readonly string _action;

        public AuthorizeByRoleAttribute(string controller, string action)
        {
            _controller = controller;
            _action = action;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetString("UserId");
            var userRole = session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Users", null);
                return;
            }

            var permissions = GetPermissionsForRole(userRole);
            var key = $"{_controller}/{_action}";
            if (!permissions.Contains(key))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            base.OnActionExecuting(context);
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

}