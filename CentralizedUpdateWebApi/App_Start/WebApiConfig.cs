using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using System.Web.Http.Cors;
using CentralizedUpdateWebApi.Utilities;
using System.Web.Http.ExceptionHandling;

namespace CentralizedUpdateWebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //Add our global exception logger
            config.Services.Add(typeof(IExceptionLogger), new GlobalExceptionLogger());

            //Enable CORS support
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            //Enable attribute routing
            config.MapHttpAttributeRoutes();

            //This route enables specifying a specific action
            config.Routes.MapHttpRoute(
                name: "UpdateApi",
                routeTemplate: "api/update/{action}",
                defaults: new { controller = "update" }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
