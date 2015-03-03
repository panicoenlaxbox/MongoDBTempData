using System;
using System.Web;
using MongoDBTempData;

namespace WebApplication1
{
    public static class MongoDBTempDataUtilities
    {
        public static GetUniqueId getUniqueId = controllerContext =>
        {
            var cookie = controllerContext.HttpContext.Request.Cookies["UniqueId"];
            return new Guid(cookie.Value);
        };

        public static void SetUniqueIdCookie(HttpRequest request)
        {
            var cookie = request.Cookies["UniqueId"];
            if (cookie != null) return;
            var uniqueId = Guid.NewGuid();
            cookie = new HttpCookie("UniqueId")
            {
                HttpOnly = true,
                Value = uniqueId.ToString()
            };
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}