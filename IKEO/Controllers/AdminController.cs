using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IKEO.Models;
using Newtonsoft.Json;

namespace IKEO.Controllers
{
    public class ImageView
    {
        public int Id { get; set; }
        public string NomImage { get; set; }

    }
    public class UtilisateurView
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

    }

    public class CouleurView
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Hexa { get; set; }
    }

    public class CategorieView
    {
        public int Id { get; set; }
        public string Nom { get; set; }
    }

    public class ArticleView
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public double Prix { get; set; }
        public int Stocks { get; set; }
        public CategorieView Categorie { get; set; }

        public string ImagePresentation { get; set; }
        public List<ImageView> Images { get; set; }
        public List<CouleurView> Couleurs { get; set; }
        public double Notation { get; set; }
        public bool? Visible { get; set; }
    }
    public class AdminController : Controller
    {
        IkeoDBEntities db = new IkeoDBEntities();

            
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Utilisateurs()
        {
            return View();
        }
        public ActionResult Articles()
        {
            return View();
        }

        public JsonResult GetArticles()
        {
            var listeArticles = db.Article
                .Include("ArticleCouleurs")
                .Include("ArticleCouleurs.Couleur")
                .Include("Categorie")
                .Include("Notation")
                .Include("ImagesArticle")
                .ToList();

            List<ArticleView> listeArticle = new List<ArticleView>();

            foreach (var art in listeArticles.OrderByDescending(x => x.disponibilite).ToList())
            {

                List<CouleurView> listeCouleurDispo = new List<CouleurView>(); 
                foreach(var AC in art.ArticleCouleurs.ToList())
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

                List<ImageView> listeImage = new List<ImageView>();
                if(art.ImagesArticle.Count() != 0)
                    foreach(var image in art.ImagesArticle.ToList())
                    {
                        listeImage.Add(new ImageView()
                        {
                            Id = (int)image.id,
                            NomImage = image.nom_image
                        });
                    }


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
                    Images = listeImage
                };
                listeArticle.Add(a);
            }
            return Json(new { type = "ok", message = listeArticle }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUtilisateurs()
        {
            var listeUtilisateurs = db.Utilisateur.Include("Roles").ToList();
            List<UtilisateurView> listeUser = new List<UtilisateurView>();

            foreach(var user in listeUtilisateurs)
            {
                UtilisateurView u = new UtilisateurView()
                {
                    Id = user.id,
                    Nom = user.nom,
                    Prenom = user.prenom,
                    Email = user.email,
                    Role = (user.Roles.designation == null) ? "Utilisateur" : user.Roles.designation
                };
                listeUser.Add(u);
            }
            return Json(new { type = "ok", message = listeUser }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCategorieAndColor()
        {
            var listeCouleur = db.Couleur.ToList();
            List<CouleurView> listeCouleurDispo = new List<CouleurView>();
            foreach (var color in listeCouleur)
            {
                listeCouleurDispo.Add(new CouleurView()
                {
                    Id = color.id,
                    Nom = color.couleur1,
                    Hexa = color.hexadecimal
                });
            }


            var listeCategorie = db.Categorie.ToList();
            List<CategorieView> listeCategorieDispo = new List<CategorieView>();
            foreach (var cat in listeCategorie)
            {
                listeCategorieDispo.Add(new CategorieView()
                {
                    Id = cat.id,
                    Nom = cat.nom
                });
            }

            return Json(new { type = "ok", lCo = listeCouleurDispo, lCa = listeCategorieDispo }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AjouterArticle(string artName, string artDesc, string artPrix, int artStoc, string artColo, int artCate, int? id = null)
        {
            if (!db.Categorie.Any(x => x.id == artCate))
                return Json(new { type = "error", message = "Catégorie inexistante." });

            if (artColo == null)
                return Json(new { type = "error", message = "Probleme avec les couleurs demandées." });




            Article article = null;
            if (id != null)
            {
                article = db.Article.FirstOrDefault(x => x.id == id);
                if (article == null)
                {
                    return Json(new { type = "error", message = "L'article n'existe pas, aucunes modifications n'est possible." });
                }
                else
                {
                    article.nom = artName;
                    article.description = artDesc;
                    article.nbrStock = artStoc;
                    article.prix = double.Parse(artPrix, CultureInfo.InvariantCulture.NumberFormat);
                    article.Categorie = db.Categorie.FirstOrDefault(x => x.id == artCate);
                }
            }
            else
            {
                
                 article = new Article()
                {
                    nom = artName,
                    description = artDesc,
                    prix = double.Parse(artPrix, CultureInfo.InvariantCulture.NumberFormat),
                    nbrStock = artStoc,
                    id_categorie = artCate,
                    disponibilite = (artStoc > 0) ? true : false
                };
                db.Article.Add(article);
            }

            List<int> listeCouleur = new List<int>(JsonConvert.DeserializeObject<List<int>>(artColo)); 
            
            if (!db.Couleur.Any(x => listeCouleur.Contains(x.id)))
                return Json(new { type = "error", message = "Couleur inexistante." });

            if (artStoc < 0)
                return Json(new { type = "error", message = "Nombre dans le stock impossible." });




            var listeDbCouleur = db.Couleur.ToList();
            if(article.ArticleCouleurs.Count() > 0)
            {
                var listeArticleCouleur = article.ArticleCouleurs.Select(x => x.Couleur.id).ToList();


                List<int> listAParcourir = new List<int>();

                foreach (var dif in listeArticleCouleur.Except(listeCouleur).ToList())
                    listAParcourir.Add(dif);
                foreach (var dif in listeCouleur.Except(listeArticleCouleur).ToList())
                    listAParcourir.Add(dif);



                foreach (var co in listAParcourir)
                {
                    if (listeArticleCouleur.Any(x => x == co))
                        db.ArticleCouleurs.Remove(article.ArticleCouleurs.FirstOrDefault(x => x.Couleur.id == co));
                    else
                    {
                        ArticleCouleurs artC = new ArticleCouleurs()
                        {
                            id_article = article.id,
                            id_couleur = listeDbCouleur.FirstOrDefault(x => x.id == co).id
                        };
                        db.ArticleCouleurs.Add(artC);
                    }
                }

            }
            else
            { 
                foreach (var couleur in listeCouleur)
                {
                    ArticleCouleurs artC = new ArticleCouleurs()
                    {
                        id_article = article.id,
                        id_couleur = listeDbCouleur.FirstOrDefault(x => x.id == couleur).id
                    };
                    db.ArticleCouleurs.Add(artC);
                }

            }
            db.SaveChanges();

            var idArticle = db.Article.FirstOrDefault(x => x.id == article.id).id;
            HttpFileCollectionBase files = Request.Files;
            for (int i = 0; i < files.Count; i++)
            {
                HttpPostedFileBase file = files[i];
                string fname;
                if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                {
                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1].Trim().Replace(" ", "");
                }
                else
                {
                    fname = file.FileName.Trim().Replace(" ", "");
                }

                if(!Directory.Exists(Server.MapPath("~/Content/images/Articles/" + idArticle)))
                    Directory.CreateDirectory(Server.MapPath("~/Content/images/Articles/" + idArticle));

                 
                file.SaveAs(Server.MapPath("~/Content/images/Articles/" + idArticle + "/" + fname));

                ImagesArticle img = new ImagesArticle()
                {
                    id_article = idArticle,
                    nom_image = fname.Trim().Replace(" ", "").Replace("\"", "'")
                };
                db.ImagesArticle.Add(img);
            }
            db.SaveChanges();
            return Json(new { type = "ok", message = "L'article a été ajouté." });
        }





        public JsonResult recupererInfoArticle(int id)
        {
            var article = db.Article
                .Include("ArticleCouleurs")
                .Include("ArticleCouleurs.Couleur")
                .Include("Categorie")
                .Include("Notation")
                .FirstOrDefault(x => x.id == id);

            if(article == null)
                return Json(new { type = "error", message = "L'article n'existe pas." });

            List<CouleurView> listeCouleur = new List<CouleurView>();
            foreach(var c in article.ArticleCouleurs)
            {
                listeCouleur.Add(new CouleurView()
                {
                    Id = c.Couleur.id,
                    Nom = c.Couleur.couleur1,
                    Hexa = c.Couleur.hexadecimal
                });
            }

            double notation = 0;
            if (article.Notation != null && article.Notation.Count > 0)
            {
                var nombreTotalNotation = article.Notation.Count();
                double calculTotalNotation = 0;
                foreach (var note in article.Notation.ToList())
                {
                    calculTotalNotation += note.note;
                }

                notation = calculTotalNotation / nombreTotalNotation;
            }

            List<ImageView> listeImage = new List<ImageView>();
            if (article.ImagesArticle.Count() != 0)
                foreach (var image in article.ImagesArticle.ToList())
                {
                    listeImage.Add(new ImageView()
                    {
                        Id = (int)image.id,
                        NomImage = image.nom_image
                    });
                }

            ArticleView av = new ArticleView()
            {
                Id = article.id,
                Nom = article.nom,
                Description = article.description,
                Prix = (double)article.prix,
                Stocks = (int)article.nbrStock,
                Categorie = new CategorieView() { Id = article.Categorie.id, Nom = article.Categorie.nom },
                Couleurs = listeCouleur,
                Notation = notation,
                Images = listeImage
            };

            return Json(new { type = "ok", message = av });
        }
        public JsonResult ModifierVisibilite(int id, bool visibilite)
        {
            var newArticle = db.Article.FirstOrDefault(x => x.id == id);
            if (newArticle == null)
                return Json(new { type = "error", message = "L'article n'existe pas." });

            newArticle.disponibilite = visibilite;
            db.SaveChanges();
            return Json(new { type = "ok", message = "Opération effectuée avec succès." });
        }
    }
}