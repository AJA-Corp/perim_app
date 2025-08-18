using SQLite;
using perimapp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace perimapp.Services
{
    public class ProductDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public ProductDatabase()
        {
            // Création du chemin du fichier SQLite (local storage)
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "products.db");

            // Connexion à la base
            _database = new SQLiteAsyncConnection(dbPath);

            // Création de la table si elle n’existe pas
            _database.CreateTableAsync<ProductInfos>().Wait();
        }

        //Récupérer tous les produits
        public Task<List<ProductInfos>> GetProductsAsync()
        {
            return _database.Table<ProductInfos>().ToListAsync();
        }

        //Récupérer un produit par ID
        public Task<ProductInfos> GetProductByIdAsync(int id)
        {
            return _database.Table<ProductInfos>()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        // Ajouter ou modifier un produit
        public Task<int> SaveProductAsync(ProductInfos product)
        {
            if (product.Id != 0)
            {
                return _database.UpdateAsync(product);
            }
            else
            {
                return _database.InsertAsync(product);
            }
        }

        //Supprimer un produit
        public Task<int> DeleteProductAsync(ProductInfos product)
        {
            return _database.DeleteAsync(product);
        }
    }
}