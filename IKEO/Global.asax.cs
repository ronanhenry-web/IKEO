using IKEO.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace IKEO
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            
            
            Start config = new Start();
            config.AjouterRoles();
            config.AjouterCouleurs();
            config.AjouterCategories();
            config.CreerCompteAdminTest(Properties.Settings.Default.AdminTest); //Voir properties
        
        }
    }
}
