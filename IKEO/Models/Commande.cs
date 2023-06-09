//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré à partir d'un modèle.
//
//     Des modifications manuelles apportées à ce fichier peuvent conduire à un comportement inattendu de votre application.
//     Les modifications manuelles apportées à ce fichier sont remplacées si le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IKEO.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Commande
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Commande()
        {
            this.ArticleCommande = new HashSet<ArticleCommande>();
        }
    
        public int id { get; set; }
        public string numero_commande { get; set; }
        public double prix_commande { get; set; }
        public double etat_livraison { get; set; }
        public bool etat_paiement { get; set; }
        public long id_panier { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ArticleCommande> ArticleCommande { get; set; }
        public virtual Panier Panier { get; set; }
    }
}
