using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using perimapp.Data;
using perimapp.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui;

namespace perimapp.Platforms.Android
{
    [Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync)]
    public class NotificationForegroundService : Service
    {
        private static readonly List<int> defaultNotificationDays = new List<int> { 1, 3, 7 };
        private static readonly List<int> notificationHours = new List<int> { 7, 11, 18 };
        private Timer _timer;
        private const int SERVICE_NOTIFICATION_ID = 10001;
        private const string LastNotificationSentHourKey = "LastNotificationSentHour";

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            StartForeground(SERVICE_NOTIFICATION_ID, CreateNotification().Build());
            
            // On déclenche la méthode de vérification toutes les 60 secondes pour une grande fiabilité
            _timer = new Timer(async (e) => await CheckAndSendNotifications(), null, 0, 60000);

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _timer?.Dispose();
        }

        private async Task CheckAndSendNotifications()
        {
            var currentHour = DateTime.Now.Hour;
            var lastNotificationSentHour = Preferences.Get(LastNotificationSentHourKey, -1);

            if (!notificationHours.Contains(currentHour) || lastNotificationSentHour == currentHour)
            {
                return;
            }

            LocalNotificationCenter.Current.CancelAll();
            
            var allProducts = AppData.CurrentProducts.ToList();
            var settingsJson = Preferences.Get("NotificationDays", string.Empty);
            List<int> notificationDays = string.IsNullOrEmpty(settingsJson)
                ? defaultNotificationDays
                : JsonSerializer.Deserialize<List<int>>(settingsJson);

            // DLC today products
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
                    Android = new AndroidOptions
                    {
                        IconSmallName = new AndroidIcon("perimapplogotransparent"),
                        VisibilityType = AndroidVisibilityType.Public
                    },
                    Schedule = new NotificationRequestSchedule { NotifyTime = DateTime.Now }
                };
                await LocalNotificationCenter.Current.Show(request);
            }

            // Expired products
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
                    Android = new AndroidOptions
                    {
                        IconSmallName = new AndroidIcon("perimapplogotransparent"),
                        VisibilityType = AndroidVisibilityType.Public
                    },
                    Schedule = new NotificationRequestSchedule { NotifyTime = DateTime.Now }
                };
                await LocalNotificationCenter.Current.Show(request);
            }

            // Upcoming expiring products
            foreach (var days in notificationDays)
            {
                var expiringProducts = allProducts.Where(p => p.Dlc.Date == DateTime.Today.AddDays(days).Date).ToList();
                if (expiringProducts.Any())
                {
                    var title = string.Empty;
                    var description = string.Empty;

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
                    
                    var request = new NotificationRequest
                    {
                        NotificationId = days,
                        Title = title,
                        Description = description,
                        Android = new AndroidOptions
                        {
                            IconSmallName = new AndroidIcon("perimapplogotransparent"),
                            VisibilityType = AndroidVisibilityType.Public
                        },
                        Schedule = new NotificationRequestSchedule { NotifyTime = DateTime.Now }
                    };
                    await LocalNotificationCenter.Current.Show(request);
                    await Task.Delay(1500); 
                }
            }

            Preferences.Set(LastNotificationSentHourKey, currentHour);
        }
        
        private NotificationCompat.Builder CreateNotification()
        {
            var notificationTitle = "Perim'App est en cours d'exécution";
            var notificationContent = "Perim'App vérifie vos produits pour vous notifier des dates de péremption.";
            
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            var notificationBuilder = new NotificationCompat.Builder(this, "perimapp_channel")
                .SetContentTitle(notificationTitle)
                .SetContentText(notificationContent)
                .SetSmallIcon(Resource.Drawable.perimapplogotransparent)
                .SetCategory(NotificationCompat.CategoryService)
                .SetOngoing(true);
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel("perimapp_channel", "Perim'App Service", NotificationImportance.Min);
                notificationManager.CreateNotificationChannel(channel);
            }
            
            return notificationBuilder;
        }
    }
}