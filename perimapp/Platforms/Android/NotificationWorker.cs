using Android.Content;
using AndroidX.Work;
using perimapp.Data;
using perimapp.Models;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Android.App;
using Microsoft.Maui;

namespace perimapp.Platforms.Android;

public class NotificationWorker : Worker
{
    private static readonly List<int> defaultNotificationDays = new List<int> { 1, 3, 7 };

    public NotificationWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
    {
    }

    public override Result DoWork()
    {
        Task.Run(async () =>
        {
            var allProducts = AppData.CurrentProducts.ToList();
            var settingsJson = Preferences.Get("NotificationDays", string.Empty);
            List<int> notificationDays = string.IsNullOrEmpty(settingsJson)
                ? defaultNotificationDays
                : JsonSerializer.Deserialize<List<int>>(settingsJson);

            var dlcTodayProducts = allProducts.Where(p => p.Dlc.Date == DateTime.Today.Date).ToList();
            if (dlcTodayProducts.Any())
            {
                var title = "Alerte de péremption aujourd'hui";
                string description;
                if (dlcTodayProducts.Count > 3)
                {
                    description = $"{dlcTodayProducts.Count} produits périment aujourd'hui.";
                }
                else
                {
                    description = dlcTodayProducts.Count == 1
                        ? $"Le produit '{dlcTodayProducts.First().Name}' périme aujourd'hui."
                        : $"{dlcTodayProducts.Count} produits périment aujourd'hui : {string.Join(", ", dlcTodayProducts.Select(p => p.Name))}.";
                }

                var request = new NotificationRequest
                {
                    NotificationId = 0,
                    Title = title,
                    Description = description,
                    Schedule = new NotificationRequestSchedule { NotifyTime = DateTime.Now }
                };
                await LocalNotificationCenter.Current.Show(request);
            }

            var expiredProducts = allProducts.Where(p => p.Dlc.Date == DateTime.Today.AddDays(-1).Date).ToList();
            if (expiredProducts.Any())
            {
                var title = "Alerte : produit(s) expiré(s)";
                string description;
                if (expiredProducts.Count > 3)
                {
                    description = $"{expiredProducts.Count} produits ont expiré hier.";
                }
                else
                {
                    description = expiredProducts.Count == 1
                        ? $"Le produit '{expiredProducts.First().Name}' a expiré hier."
                        : $"{expiredProducts.Count} produits ont expiré hier : {string.Join(", ", expiredProducts.Select(p => p.Name))}.";
                }
                
                var request = new NotificationRequest
                {
                    NotificationId = -1,
                    Title = title,
                    Description = description,
                    Schedule = new NotificationRequestSchedule { NotifyTime = DateTime.Now }
                };
                await LocalNotificationCenter.Current.Show(request);
            }

            foreach (var days in notificationDays)
        {
            // Trouver les produits qui périment dans le nombre de jours spécifié
            var expiringProducts = allProducts.Where(p =>
                p.Dlc.Date == DateTime.Today.AddDays(days).Date).ToList();
            
            if (expiringProducts.Any())
            {
                var title = string.Empty;
                var description = string.Empty;

                // 5. Applique la logique d'agrégation
                if (expiringProducts.Count == 1)
                {
                    var product = expiringProducts.First();
                    if (days == 1)
                    {
                        title = $"Alerte de péremption dans {days} jour";
                        description = $"Le produit '{product.Name}' périme dans {days} jour.";
                    }
                    else
                    {
                        title = $"Alerte de péremption dans {days} jours";
                        description = $"Le produit '{product.Name}' périme dans {days} jours.";
                    }
                }
                else if (expiringProducts.Count <= 3)
                {
                    if (days == 1)
                    {
                        title = $"Alerte de péremption dans {days} jour";
                        description = $"Les produits suivants vont périmer dans {days} jour : {string.Join(", ", expiringProducts.Select(p => p.Name))}.";
                    }
                    else
                    {
                        title = $"Alerte de péremption dans {days} jours";
                        description = $"Les produits suivants vont périmer dans {days} jour(s) : {string.Join(", ", expiringProducts.Select(p => p.Name))}.";
                    }
                }
                else // Plus de 3 produits
                {
                    if (days == 1)
                    {
                        title = $"Alerte de péremption dans {days} jour";
                        description = $"{expiringProducts.Count} produits vont périmer dans {days} jour.";
                    }
                    else
                    {
                        title = $"Alerte de péremption dans {days} jours";
                        description = $"{expiringProducts.Count} produits vont périmer dans {days} jour(s).";
                    }
                }

                // 6. Crée et programme la notification
                var request = new NotificationRequest
                {
                    // Un ID unique pour chaque jour d'alerte (ex: 1, 3, 7)
                    NotificationId = days, 
                    Title = title,
                    Description = description,
                    Schedule = new NotificationRequestSchedule
                    {
                        // On planifie la notification pour qu'elle s'exécute une fois par jour
                        NotifyTime = DateTime.Now.AddSeconds(10)
                    }
                };

                await LocalNotificationCenter.Current.Show(request);
            }
        }
        }).Wait();
        return Result.InvokeSuccess();
    }
}