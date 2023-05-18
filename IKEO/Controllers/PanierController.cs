using IKEO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IKEO.Controllers
{
    public class PanierController : Controller
    {

        IkeoDBEntities db = new IkeoDBEntities();

        public JsonResult GetCarteBancaire()
        {
            var idUtilisateur = int.Parse(Session["UserID"].ToString());
            var currentUser = db.Utilisateur.Include("CarteBancaire").FirstOrDefault(x => x.id == idUtilisateur);
            if (currentUser.CarteBancaire != null)
                return Json(new { type = "ok", message = currentUser.CarteBancaire }, JsonRequestBehavior.AllowGet);

            else
                return Json(new { type = "ok", message = "Aucune carte bancaire enregistrée" }, JsonRequestBehavior.AllowGet);

        }

        // GET: Panier
        [HttpGet]
        public ActionResult Index(string nomEtape, int? idAdresse, int? idCb)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Index", "Home");


            var idUtilisateur = int.Parse(Session["UserID"].ToString());
            var currentUser = db.Utilisateur.Include("Adresse").Include("Panier").Include("Panier.ArticlePanier").Include("Panier.ArticlePanier.Article").Include("Panier.ArticlePanier.Article.ImagesArticle").FirstOrDefault(x => x.id == idUtilisateur);

            switch (nomEtape)
            {
                case "EtapeDeux":
                    ViewBag.nomEtape = nomEtape;
                    ViewBag.numEtape = 2;
                    ViewBag.NombreAdresseEnregistre = currentUser.Adresse.Count();
                    if(currentUser.Adresse.Count() > 0)
                    {
                        ViewBag.Adresse = currentUser.Adresse.ToList();
                    }
                    break;
                case "EtapeTrois":
                    ViewBag.nomEtape = nomEtape;
                    ViewBag.idAdresse = idAdresse;
                    ViewBag.numEtape = 3;
                    break;
                case "EtapeQuatre":
                    ViewBag.nomEtape = nomEtape;
                    ViewBag.idAdresse = idAdresse;
                    ViewBag.idCb = idAdresse;
                    ViewBag.numEtape = 4;
                    break;
                case "EtapeCinq":
                    ViewBag.nomEtape = nomEtape;
                    ViewBag.numEtape = 5;
                    break;
                default:
                    ViewBag.nomEtape = "EtapeUn";
                    ViewBag.numEtape = 1;
                    break;
            }


            

            ViewBag.Panier = currentUser.Panier;

            return View();
        }

        public ActionResult EtapeDeux()
        {

            return PartialView("EtapeDeux");
        }

        public JsonResult AjouterArticleDansPanier(int id)
        {
            if (Session["UserID"] == null )
                return Json(new { type = "error", message = "Non connecté !" });

            var article = db.Article.FirstOrDefault(x => x.id == id && x.disponibilite == true && x.nbrStock > 0);
            if(article == null)
                return Json(new { type = "error", message = "Article inexistant ou indisponibler !" });


            var idUtilisateur = int.Parse(Session["UserID"].ToString());
            var currentUser = db.Utilisateur.Include("Panier").Include("Panier.ArticlePanier").FirstOrDefault(x => x.id == idUtilisateur);
            if(currentUser == null)
                return Json(new { type = "error", message = "Utilisateur inexistant !" });

            if (currentUser.Panier.ArticlePanier.Any(x => x.Article.id == id))
                currentUser.Panier.ArticlePanier.FirstOrDefault(x => x.Article.id == id).nombre_article += 1;
            else
                db.ArticlePanier.Add(
                    new ArticlePanier(){
                        id_panier = currentUser.Panier.id,
                        id_article = id,
                        nombre_article = 1
                    }
                );


            db.SaveChanges();

            return Json(new { type = "ok", message = "Article ajouté !" });
        }


        public JsonResult SuprimerArticleDansPanier(int id)
        {
            if (Session["UserID"] == null)
                return Json(new { type = "error", message = "Non connecté !" });

      
            var idUtilisateur = int.Parse(Session["UserID"].ToString());
            var currentUser = db.Utilisateur.Include("Panier").Include("Panier.ArticlePanier").FirstOrDefault(x => x.id == idUtilisateur);
            if (currentUser == null)
                return Json(new { type = "error", message = "Utilisateur inexistant !" });


            

            var articlePanier = currentUser.Panier.ArticlePanier.FirstOrDefault(x => x.id == id);
            if (articlePanier == null)
                return Json(new { type = "error", message = "Article inexistant !" });

           
           db.ArticlePanier.Remove(articlePanier);


            db.SaveChanges();

            return Json(new { type = "ok", message = "Article retiré !" });
        }


        public JsonResult ChangerNombreDansPanier(int id, int valeur)
            {
            if (Session["UserID"] == null)
                return Json(new { type = "error", message = "Non connecté !" });


            var idUtilisateur = int.Parse(Session["UserID"].ToString());
            var currentUser = db.Utilisateur.Include("Panier").Include("Panier.ArticlePanier").FirstOrDefault(x => x.id == idUtilisateur);
            if (currentUser == null)
                return Json(new { type = "error", message = "Utilisateur inexistant !" });




            var articlePanier = currentUser.Panier.ArticlePanier.FirstOrDefault(x => x.id == id);
            if (articlePanier == null)
                return Json(new { type = "error", message = "Article inexistant !" });

            articlePanier.nombre_article = valeur;
            if (valeur == 0)
                db.ArticlePanier.Remove(articlePanier);

            db.SaveChanges();

            return Json(new { type = "ok", message = "Article retiré !" });
        }

        public JsonResult EnregistrerCB(string numeroCb, string titulaireCb, string dateExp, string codeCb)
        {


            if (Session["UserID"] == null)
                return Json(new { type = "error", message = "Erreur au niveau des informations, veuillez vérifier celles-ci !" }, JsonRequestBehavior.AllowGet);

            if (numeroCb.Replace(" ", "") != "" && titulaireCb.Replace(" ", "") != "" && dateExp.Replace(" ", "") != "" && codeCb.Replace(" ", "") != "")
            {

                var idUser = int.Parse(Session["UserID"].ToString());

                Utilisateur us = db.Utilisateur.Include("CarteBancaire").FirstOrDefault(x => x.id == idUser);
                CarteBancaire cb = (us.CarteBancaire == null) ? null : us.CarteBancaire;
                if (cb == null) {

                    cb = new CarteBancaire()
                    {
                        numero = numeroCb,
                        nom = titulaireCb,
                        dateExpiration = dateExp
                    };
                }
                db.CarteBancaire.Add(cb);

                db.SaveChanges();
                return Json(new { type = "ok", message = "Adresse créé", idCb = cb.id }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { type = "error", message = "Erreur au niveau des informations, veuillez vérifier celles-ci !" }, JsonRequestBehavior.AllowGet);
        }
    }
}