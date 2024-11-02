using System;

namespace YourNamespace.Classes
{
    public class CacheItem
    {
        public string Data { get; set; }  // Les données JSON du stock
        public DateTime Expiration { get; set; }  // Date d'expiration des données

        public CacheItem(string data, DateTime expiration)
        {
            Data = data;
            Expiration = expiration;
        }

        // Méthode pour vérifier si l'entrée est expirée
        public bool IsExpired() => DateTime.UtcNow > Expiration;
    }
}
