using System.Web.Mvc;
using IKEO.Models;
using System.Linq;
using System.Collections.Generic;

namespace IKEO.Controllers
{
    public class ArticleController : Controller
    {
        IkeoDBEntities db = new IkeoDBEntities();
        public ActionResult Index(int id)
        {
            var article = db.Article
                .Include("ArticleCouleurs")
                .Include("ArticleCouleurs.Couleur")
                .Include("Categorie")
                .Include("Notation")
                .FirstOrDefault(x => x.id == id);

            if (article == null)
                return Json(new { type = "error", message = "L'article n'existe pas." });

            List<CouleurView> listeCouleur = new List<CouleurView>();
            foreach (var c in article.ArticleCouleurs)
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

            string imagePresentation = "";

            if (article.ImagesArticle.Count() != 0)
            {
                foreach (var image in article.ImagesArticle.ToList())
                {
                    listeImage.Add(new ImageView()
                    {
                        Id = (int)image.id,
                        NomImage = image.nom_image
                    });
                }
                imagePresentation = article.ImagesArticle.OrderBy(x => x.id).FirstOrDefault().nom_image;
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
                Images = listeImage,
                ImagePresentation = imagePresentation
            };


            ViewBag.Article = av;

            return View("Article");
        }


        
        public JsonResult AjouterArticle(string nom, string description, int stock, float prix,string[] couleur, string categorie)
        {
            List<string> couleurs = new List<string>(couleur);
            
            
            Article newArticle = db.Article.Include("Notation").Include("Categorie").Include("ArticleCouleurs").Include("ImagesArticle").Include("ArticleCouleurs.Couleur").FirstOrDefault(x => x.nom == nom && x.description == description && x.Categorie.nom == categorie);

            if (newArticle == null)
            {
                newArticle = new Article()
                {
                    nom = nom,
                    description = description,
                    nbrStock = stock,
                    prix = prix,
                    Categorie = db.Categorie.FirstOrDefault(x => x.nom == categorie),
                    disponibilite = true
                };
                db.Article.Add(newArticle);
            }
            else
            {
                newArticle.nom = nom;
                newArticle.description = description;
                newArticle.nbrStock = stock;
                newArticle.prix = prix;
                newArticle.Categorie = db.Categorie.FirstOrDefault(x => x.nom == categorie);
            }
            

            foreach(var c in newArticle.ArticleCouleurs.Select(x => x.Couleur.couleur1).Where(x => couleur.Contains(x)).ToList())
            {
                ArticleCouleurs newAC = new ArticleCouleurs()
                {
                    id_article = newArticle.id,
                    id_couleur = db.Couleur.FirstOrDefault(x => x.couleur1 == c).id
                };
                db.ArticleCouleurs.Add(newAC);
            }

                db.SaveChanges();
                return Json(new { type = "ok", message = "L'article a été crée avec succès." });

        }
    }
}
