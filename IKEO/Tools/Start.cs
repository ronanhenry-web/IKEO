using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IKEO.Models;
using IKEO.Controllers;

namespace IKEO.Tools
{
    public class Start
    {
        IkeoDBEntities db = new IkeoDBEntities();

        List<String> listeRoles = new List<string>()
        {
            "Utilisateur",
            "Employe",
            "Administrateur"
        };

        List<String> listeCategorie = new List<string>()
        {
            "Tabouret",
            "Chaise",
            "Fauteil et canapé",
            "Lit",
            "Table",
            "Buffet",
            "Armoire",
            "Autre"
        };

        List<string[]> listeCouleurs = new List<string[]>()
        {
            new string[] { "Rouge", "FF0000" },
            new string[] { "Orange", "FF8000" },
            new string[] { "Jaune", "FFFB00" },
            new string[] { "Vert", "00FF00" },
            new string[] { "Bleu", "0000FF" },
            new string[] { "Violet", "4200FF" },
            new string[] { "Rose", "E400FF" },
            new string[] { "Noir", "000000" },
            new string[] { "Blanc", "FFFFFF" },
            new string[] { "Marron", "603900" },
            new string[] { "Gris", "8A8A8A" },
            new string[] { "Azur", "F0FFFF" },

        };

        public void AjouterRoles()
        {

            var dbListeRole = db.Roles.Select(x => x.designation).ToList();
            foreach(var roleNonInscrit in listeRoles.Where(x => !dbListeRole.Contains(x)))
            {
                Roles newRole = new Roles()
                {
                    designation = roleNonInscrit
                };
                db.Roles.Add(newRole);
            }

            db.SaveChanges();
        }

        public void AjouterCouleurs()
        {

            var dbListeCouleur = db.Couleur.Select(x => x.couleur1).ToList();
            foreach (var couleurNonInscrit in listeCouleurs.Where(x => !dbListeCouleur.Contains(x[0])))
            {
                Couleur newCouleur = new Couleur()
                {
                    couleur1 = couleurNonInscrit[0],
                    hexadecimal = couleurNonInscrit[1]
                };
                db.Couleur.Add(newCouleur);
            }

            db.SaveChanges();
        }

        public void AjouterCategories()
        {

            var dbListeCategorie = db.Categorie.Select(x => x.nom).ToList();
            foreach (var categorieNonInscrit in listeCategorie.Where(x => !dbListeCategorie.Contains(x)))
            {
                Categorie newCategorie = new Categorie()
                {
                    nom = categorieNonInscrit
                };
                db.Categorie.Add(newCategorie);
            }

            db.SaveChanges();
        }

        public void CreerCompteAdminTest(bool active)
        {
            if (!db.Utilisateur.Any(x => x.nom == "Admin" && x.prenom == "Test" && x.email == "test@admin.site" && x.Roles.designation == "Administrateur"))
            {
                if (active) 
                { 
                Panier newPanier = new Panier();

                Utilisateur nouvelUtilisateur = new Utilisateur()
                {
                    email = "test@admin.site",
                    mdp = hashPassword("123456789"),
                    nom = "Admin",
                    prenom = "Test",
                    confirm = true,
                    id_cartebancaire = null,
                    Panier = newPanier,
                    Roles = db.Roles.FirstOrDefault(x => x.designation == "Administrateur")
                };

                db.Panier.Add(newPanier);
                db.Utilisateur.Add(nouvelUtilisateur);
                db.SaveChanges();
            
                }
            }
            else
            {
                if (!active)
                {
                    db.Utilisateur.Remove(db.Utilisateur.FirstOrDefault(x => x.nom == "Admin" && x.prenom == "Test" && x.email == "test@admin.site" && x.Roles.designation == "Administrateur"));
                }
            }
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

    }
}