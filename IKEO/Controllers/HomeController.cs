
using System.Web.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using IKEO.Models;
using System.Linq;
using System;
using IKEO.Controllers;

namespace IKEO.Controllers
{
    public class HomeController : Controller
    {
        IkeoDBEntities db = new IkeoDBEntities();

        public JsonResult NumberInPanier()
        {
            var id = Convert.ToInt32(Session["UserID"].ToString());
            var user = db.Utilisateur.Include("Panier").Include("Panier.ArticlePanier").FirstOrDefault(x => x.id == id);
            if (user == null)
                return Json(new { type = "ok", message = 0 }, JsonRequestBehavior.AllowGet);

            int nmbrDansPanier = 0;
            foreach(var a in user.Panier.ArticlePanier.ToList())
            {
                nmbrDansPanier += (int)a.nombre_article;
            }

            return Json(new { type = "ok", message = nmbrDansPanier }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            var listeArticleDb = db.Article.Include("Notation").Include("Categorie").Include("ArticleCouleurs").Include("ImagesArticle").Include("ArticleCouleurs.Couleur").Where(x => x.disponibilite == true).ToList();
            Utilisateur currentUser = null;
            if (Session["UserID"] != null)
            {
                int idUser = Convert.ToInt32(Session["UserID"]);
                currentUser = db.Utilisateur.FirstOrDefault(x => x.id == idUser);
            }
            List<ArticleView> listeArticle = new List<ArticleView>();
            foreach(var art in listeArticleDb)
            {
                List<CouleurView> listeCouleurDispo = new List<CouleurView>();
                foreach (var AC in art.ArticleCouleurs.ToList())
                {
                    listeCouleurDispo.Add(
                        new CouleurView()
                        {
                            Id = AC.Couleur.id,
                            Nom = AC.Couleur.couleur1,
                            Hexa = AC.Couleur.hexadecimal
                        }
                    );
                }

                double notation = 0;
                if (art.Notation != null && art.Notation.Count > 0)
                {
                    var nombreTotalNotation = art.Notation.Count();
                    double calculTotalNotation = 0;
                    foreach (var note in art.Notation.ToList())
                    {
                        calculTotalNotation += note.note;
                    }

                    notation = calculTotalNotation / nombreTotalNotation;
                }

                string imagePresentation = (art.ImagesArticle.Count() == 0) ? "" : art.ImagesArticle.OrderBy(x => x.id).FirstOrDefault().nom_image;
                


                ArticleView a = new ArticleView()
                {

                    Id = art.id,
                    Nom = art.nom,
                    Description = art.description,
                    Prix = (double)((art.prix == null) ? 0 : art.prix),
                    Stocks = (int)art.nbrStock,
                    Categorie = new CategorieView() { Id = art.Categorie.id, Nom = art.Categorie.nom },
                    Couleurs = listeCouleurDispo,
                    Notation = notation,
                    Visible = art.disponibilite,
                    ImagePresentation = imagePresentation
                };
                listeArticle.Add(a);
            }
            ViewBag.CurrentUser = currentUser;
            ViewBag.Article = listeArticle;
            ViewBag.Couleur = db.Couleur.ToList();
            ViewBag.Categorie = db.Categorie.ToList();

            return View();
        }

        public ActionResult FiltrerArticles(string tri, int? couleur, int? categorie)
        {
            var articles = db.Article.Include("ArticleCouleurs").Include("ArticleCouleurs.Couleur").ToList();

            // Tri en fonction de la sélection de l'utilisateur
            if (!string.IsNullOrEmpty(tri))
            {
                if (tri == "croissant")
                {
                    articles = articles.OrderBy(a => a.prix).ToList();
                }
                else if (tri == "decroissant")
                {
                    articles = articles.OrderByDescending(a => a.prix).ToList();
                }
            }

            if (couleur.HasValue && couleur != 0)
            {
               
            }

            if (categorie.HasValue && categorie != 0)
            {
                articles = articles.Where(a => a.id_categorie == categorie).ToList();
            }


            // Vérifier si aucun article n'est disponible lors de la sélection d'un filtre
            if (articles == null)
            {
                ViewBag.Message = "Aucun article disponible.";
            }

            ViewBag.Article = articles.ToList();
            ViewBag.Couleur = db.Couleur.ToList();
            ViewBag.Categorie = db.Categorie.ToList();


            return View("Index");
        }
        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult MentionsLégales()
        {
            return View();
        }

        public ActionResult ConditionGeneralVente()
        {
            return View();
        }

        public ActionResult RGPD()
        {
            return View();
        }
    }
}