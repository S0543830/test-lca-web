using System.Web.Mvc;
using System.Web.Routing;

namespace LCA_WEB
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "edit",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Edit", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "addRowIndi",
                url: "{controller}/{action}/{atRow}",
                defaults: new { controller = "Home", action = "AddNewRowIndikator", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Delete",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Delete", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "addProdukt",
                url: "{controller}/{action}/{typ}/{roh}/{indi}",
                defaults: new { controller = "Home", action = "Create", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "deleteRoh",
                url: "{controller}/{action}/{name}/{roh}",
                defaults: new { controller = "Home", action = "DeleteRohstoff", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AddRohstoff",
                url: "{controller}/{action}/{_rohstoffView}",
                defaults: new { controller = "Home", action = "AddRohstoff", id = UrlParameter.Optional }
            );

           

            routes.MapRoute(
                name: "AddIndi",
                url: "{controller}/{action}/{name}/{_indiDetails}",
                defaults: new { controller = "Home", action = "AddIndikator", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "indikator",
                url: "{controller}/{action}/{name}/{indi}",
                defaults: new { controller = "Home", action = "DeleteIndikator", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AddProduktTyp",
                url: "{controller}/{action}/{_typ}",
                defaults: new { controller = "Home", action = "AddProduktTyp", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Modi",
                url: "{controller}/{action}/{_name}/{_whichView}",
                defaults: new { controller = "Home", action = "DeleteProduktTyp", id = UrlParameter.Optional }
            );

            
           
        }
    }
}
