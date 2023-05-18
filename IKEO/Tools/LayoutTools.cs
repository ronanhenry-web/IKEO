using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using IKEO.Models;

namespace IKEO.Tools
{
    public class LayoutTools
    {
        static IkeoDBEntities db = new IkeoDBEntities();

        static public bool GotAdminRights()
        {
            if (HttpContext.Current.Session["UserID"] != null)
            {
                var id = Convert.ToInt32(HttpContext.Current.Session["UserID"].ToString());
                return db.Utilisateur.Any(x => x.id == id && x.Roles.designation != "Utilisateur");
            }
            return false;
        }

        static public string GetPresentationImage(int id)
        {
            return db.ImagesArticle.FirstOrDefault(x => x.Article.id == id).nom_image;
        }

    }
}