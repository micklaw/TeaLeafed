using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TeaLeafed.Domain.Importer.Imports;
using TeaLeafed.Domain.Importer.Migrations;
using Umbraco.Web;

namespace TeaLeafed
{
    public class MvcApplication : UmbracoApplication
    {
        protected override void OnApplicationStarting(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ConverterResolver.Init();
            ImporterResolver.Init();

            base.OnApplicationStarting(sender, e);
        }
    }
}
