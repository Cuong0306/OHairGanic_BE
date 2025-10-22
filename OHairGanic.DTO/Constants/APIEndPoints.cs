namespace OHairGanic.DTO.Constants
{
    public static class ApiRoutes
    {
        public const string Base = "api";

        public static class Auth
        {
            public const string Login = $"{Base}/auth/login";
            public const string Register = $"{Base}/auth/register";
        }

        public static class Product
        {
            public const string GetAll = $"{Base}/products";
            public const string GetById = $"{Base}/products/{{id}}";
            public const string GetByName = $"{Base}/products/name/{{name}}";
            public const string Create = $"{Base}/products";
            public const string Update = $"{Base}/products/{{id}}";
            public const string Delete = $"{Base}/products/{{id}}";
            public const string Upload = $"{Base}/products/upload";
        }
        public static class Capture
        {
            public const string GetAll = $"{Base}/captures";
            public const string GetById = $"{Base}/captures/{{id}}";
            public const string Create = $"{Base}/captures";
            public const string Update = $"{Base}/captures/{{id}}";
            public const string Delete = $"{Base}/captures/{{id}}";
        }
        public static class Analyze
        {
            public const string Create = $"{Base}/analyzes";
            public const string GetById = $"{Base}/analyzes/{{id}}";
        }
        public static class Order
        {
            public const string Create = $"{Base}/orders";
            public const string GetById = $"{Base}/orders/{{id}}";
            public const string GetAll = $"{Base}/orders";
            public const string AdminUpdateStatus = $"{Base}/orders/admin/update-status";
        }
        public static class Payment
        {
            public const string Create = $"{Base}/payments/create/{{orderId}}";   // Tạo QR PayOS
            public const string Complete = $"{Base}/payments/{{id}}/complete";    // Hoàn tất thanh toán
            public const string GetById = $"{Base}/payments/{{id}}";              // Lấy chi tiết giao dịch
            public const string Webhook = $"{Base}/payments/webhook";             // Nhận callback từ PayOS
        }
        public static class Users
        {
            public const string BaseUser = $"{Base}/user";
            public const string Create = $"{Base}/create";
            public const string GetAll = $"{BaseUser}/all";
            public const string GetById = $"{BaseUser}/{{id}}";
            public const string Update = $"{BaseUser}/update";
            public const string Delete = $"{BaseUser}/delete-soft/{{id}}";
            public const string HardDelete = $"{BaseUser}/delete-hard/{{userId}}";
        }

    }
}
