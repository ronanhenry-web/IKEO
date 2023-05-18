using System.Web.Mvc;
using IKEO.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Globalization;

namespace IKEO.Controllers
{
    public class CompteController : Controller
    {
        IkeoDBEntities db = new IkeoDBEntities();

        public ActionResult Index()
        {
            var idUtilisateurCourant = Convert.ToInt32(Session["UserID"]);
            var utilisateur = db.Utilisateur.FirstOrDefault(x => x.id == idUtilisateurCourant);
            ViewBag.infoUtilisateur = utilisateur;
            return View();
        }

        public ActionResult Connexion()
        {
            return View();
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult RedirigerCreerCompte()
        {
            return View("CreerCompte");
        }

        public ActionResult InformationsPaiement()
        {
            var idUtilisateurCourant = Convert.ToInt32(Session["UserID"]);
            var utilisateur = db.Utilisateur.FirstOrDefault(x => x.id == idUtilisateurCourant);
            
            if (utilisateur.id_cartebancaire != null)
            {
                var cbUtilisateur = db.CarteBancaire.FirstOrDefault(x => x.id == utilisateur.id_cartebancaire);
                ViewBag.cbUtilisateur = cbUtilisateur;
            }
            else
            {
                ViewBag.cbUtilisateur = null;
            }

            ViewBag.utilisateur = utilisateur;
            
     
            return View();
        }
        public ActionResult DownloadPDF()
        {
            // Logique pour générer le fichier PDF à partir de l'identifiant "id"
            // Assurez-vous que le chemin d'accès au fichier PDF est correctement défini

            string filePath = "C:\\SupDeVinci\\Gantt.pdf";
            string fileName = "Gantt.pdf";

            // Téléchargement du fichier
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }

        public ActionResult MotDePasse()
        {
            return View();
        }
        public ActionResult Commande()
        {
            return View();
        }
        public ActionResult CommandeDetails()
        {
            return View();
        }
        public ActionResult Information()
        {
            var idUtilisateurCourant = Convert.ToInt32(Session["UserID"]);
            var utilisateur = db.Utilisateur.FirstOrDefault(x => x.id == idUtilisateurCourant);
            ViewBag.infoUtilisateur = utilisateur;
            return View();
        }
        public ActionResult Deconnecter()
        {
            Session["UserID"] = null;
            Session["UserNom"] = null;
            Session["UserPrenom"] = null;
            return RedirectToAction("Index", "Home");
        }

        public JsonResult Connecter(string email, string password)
        {
            //Faire algorithme de hashage pour password
            string passwordHash = hashPassword(password);
            var utilisateur = db.Utilisateur.FirstOrDefault(x => (x.email == email) && (x.mdp == passwordHash));
            if(utilisateur != null)
            {
                //if(utilisateur.confirm != null || utilisateur.confirm != false) return Json(new { type = "error", message = "Utilisateur non-confirmé" }, JsonRequestBehavior.AllowGet);
                Session["UserID"] = utilisateur.id;
                Session["UserNom"] = utilisateur.nom;
                Session["UserPrenom"] = utilisateur.prenom;
                return Json(new { type="ok", message="Utilisateur connecté"}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { type = "error", message = "E-mail ou mot de passe incorrect !" }, JsonRequestBehavior.AllowGet);
            }
        }
        private bool CheckInfo(string info)
        {
            if (info.Replace(" ", "") != "") return true;
            else return false;
        }
        
        

        private string hashPassword(string text, string salt = "52aZe")
        {
            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                // Convert the string to a byte array first, to be processed
                byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text + salt);
                byte[] hashBytes = sha.ComputeHash(textBytes);

                // Convert back to a string, removing the '-' that BitConverter adds
                string hash = BitConverter
                    .ToString(hashBytes)
                    .Replace("-", String.Empty);

                return hash;
            }
        }

        public JsonResult CreerCompte(string civilite, string prenom, string nom, string email, string password, string passwordCheck)
        {
            if(CheckInfo(civilite) && CheckInfo(prenom) && CheckInfo(nom) && CheckInfo(email) && CheckInfo(password) && CheckInfo(passwordCheck))
            {
           

                string passwordHash = hashPassword(password);
                string passwordCheckHash = hashPassword(passwordCheck);

                if (passwordHash != passwordCheckHash) return Json(new { type = "error", message = "Les mots de passe ne correspondent pas !" }, JsonRequestBehavior.AllowGet);

                if (db.Utilisateur.Any(x => x.email == email)) return Json(new { type = "error", message = "Ce compte existe déjà !" }, JsonRequestBehavior.AllowGet);

                Panier newPanier = new Panier();

                Utilisateur nouvelUtilisateur = new Utilisateur()
                {
                    email = email,
                    mdp = passwordHash,
                    nom = nom,
                    prenom = prenom,
                    confirm = false,
                    id_cartebancaire = null,
                    Panier = newPanier,
                    Roles = db.Roles.FirstOrDefault(x => x.designation == "Utilisateur"),
                    civilite = civilite
                };

                db.Panier.Add(newPanier);
                db.Utilisateur.Add(nouvelUtilisateur);
                db.SaveChanges();

                return Json(new { type = "ok", message = "Compte créé" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { type = "error", message = "Erreur au niveau des informations, veuillez vérifier celles-ci !" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AjouterAdresse(string nom, string prenom, string adresse, string ville, string code)
        {
            if (Session["UserID"] == null)
                return Json(new { type = "error", message = "Erreur au niveau des informations, veuillez vérifier celles-ci !" }, JsonRequestBehavior.AllowGet);

            if (nom.Replace(" ", "") != "" && prenom.Replace(" ", "") != "" && adresse.Replace(" ", "") != "" && ville.Replace(" ", "") != "" && code.Replace(" ", "") != "")
            {
                var codePo = decimal.Parse(code, CultureInfo.InvariantCulture);

                if (db.Adresse.Any(x => x.adresse1 == adresse && x.Utilisateur.prenom == prenom && x.Utilisateur.nom == nom && x.codepostal == codePo))
                    return Json(new { type = "error", message = "Adresse déja existante !" }, JsonRequestBehavior.AllowGet);

                var adr = new Adresse()
                {
                    codepostal = codePo,
                    adresse1 = adresse,
                    id_utilisateur = int.Parse(Session["UserID"].ToString())
                };
                db.Adresse.Add(adr);

                db.SaveChanges();
                return Json(new { type = "ok", message = "Adresse créé", idAdresse = adr.id }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { type = "error", message = "Erreur au niveau des informations, veuillez vérifier celles-ci !" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VerifierAdresse(int idAd)
        {
            if (Session["UserID"] == null)
                return Json(new { type = "error", message = "Erreur au niveau des informations, veuillez vérifier celles-ci !" }, JsonRequestBehavior.AllowGet);

            var idUs = int.Parse(Session["UserID"].ToString());
            if (db.Utilisateur.Any(x => x.id == idUs && x.Adresse.Any(y => y.id == idAd))){
                return Json(new { type = "ok", message = "Adresse retrouvée", idAdresse = idAd }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { type = "error", message = "Erreur au niveau des informations, veuillez vérifier celles-ci !" }, JsonRequestBehavior.AllowGet);
        }
    }
}
