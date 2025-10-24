// OHairGanic.DTO/Constants/ApiRoutes.cs
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
            public const string GetByInitial = $"{Base}/products/initial/{{initial?}}";
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
            public const string Base = $"{ApiRoutes.Base}/analyzes";

            // Phân tích ảnh từ URL
            public const string AnalyzeByUrl = $"{Base}/by-url";

            // Admin
            public const string GetAll = Base;
            public const string GetById = $"{Base}/{{id}}";
            public const string GetByUserId = $"{Base}/by-user/{{userId}}"; // giữ nếu cần

            // Cũ (giữ nếu muốn tương thích)
            public const string GetDaily = $"{Base}/daily";
            public const string Filter = $"{Base}/filter";

            // ✅ Mới: chỉ lấy của chính mình
            public const string GetMineDaily = $"{Base}/mine/daily";

            // ✅ Mới: filter theo ngày, của chính mình
            // GET /api/analyzes/mine/filter?date=yyyy-MM-dd
            // hoặc /api/analyzes/mine/filter?from=yyyy-MM-dd&to=yyyy-MM-dd
            public const string FilterMine = $"{Base}/mine/filter";
        }

        public static class Order
        {
            public const string Create = $"{Base}/orders";
            public const string GetById = $"{Base}/orders/{{id}}";
            public const string GetAll = $"{Base}/orders";
            public const string AdminUpdateStatus = $"{Base}/orders/admin/update-status";
            public const string GetByUserId = $"{Base}/users/{{userId}}/orders";
            public const string GetMine = $"{Base}/orders/mine";
            public const string GetMyPaid = $"{Base}/orders/mine/paid";
            public const string GetMyUnpaid = $"{Base}/orders/mine/unpaid";
            public const string CancelMine = $"{Base}/orders/{{id}}/cancel";
            public const string GetMyCancelled = $"{Base}/orders/mine/cancelled";
        }

        public static class Payment
        {
            public const string Create = $"{Base}/payments/create/{{orderId}}";
            public const string Complete = $"{Base}/payments/{{id}}/complete";
            public const string GetById = $"{Base}/payments/{{id}}";
            public const string Webhook = $"{Base}/payments/webhook";
        }

        public static class Users
        {
            public const string BaseUser = $"{Base}/user";
            public const string GetMe = $"{BaseUser}/me";
            public const string UpdateMe = $"{BaseUser}/me";

            public const string Create = $"{Base}/create";
            public const string GetAll = $"{BaseUser}/all";
            public const string GetById = $"{BaseUser}/{{id}}";
            public const string Update = $"{BaseUser}/update";
            public const string Delete = $"{BaseUser}/delete-soft/{{id}}";
            public const string HardDelete = $"{BaseUser}/delete-hard/{{userId}}";
        }
    }
}
