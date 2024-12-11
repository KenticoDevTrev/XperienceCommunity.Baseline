using Microsoft.AspNetCore.Builder;
using Kentico.Web.Mvc;

namespace MVC
{
    public class RouteConfig
    {
        /// <summary>
        /// BASELINE CUSTOMIZATION: Starting Site - Adjust Routes below if you want custom routes
        /// </summary>
        /// <param name="app"></param>
        public static void RegisterRoutes(WebApplication app)
        {
            // Adds system routes such as HTTP handlers and feature-specific routes
            app.Kentico().MapRoutes();

            app.UseRouting();
           
             app.MapControllerRoute(
                name: "error",
                pattern: "error/{code}",
                   defaults: new { controller = "HttpErrors", action = "Error" }
            );

            app.MapControllerRoute(
                   name: "MySiteMap",
                   pattern: "sitemap.xml",
                   defaults: new { controller = "Sitemap", action = "Index" }
               );

            app.MapControllerRoute(
                name: "MySiteMap_Google",
                pattern: "googlesitemap.xml",
                defaults: new { controller = "Sitemap", action = "Index" }
            );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Only enable until home page is set
            // app.MapGet("/", () => "The MVC site has not been configured yet.");
            
        }
    }
}
