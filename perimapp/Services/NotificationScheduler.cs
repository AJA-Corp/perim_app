using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Maui.Storage;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using perimapp.Data;

namespace perimapp.Services
{
    public static class NotificationScheduler
    {
        private static readonly List<int> defaultNotificationDays = new List<int> { 1, 3, 7 };
        private static readonly List<int> notificationHours = new List<int> { 7, 11, 18 };

        public static void UpdateSchedules()
        {
            LocalNotificationCenter.Current.CancelAll();

            var allProducts = AppData.CurrentProducts.ToList();
            if (!allProducts.Any()) return;

            var settingsJson = Preferences.Get("NotificationDays", string.Empty);
            var notificationDays = string.IsNullOrEmpty(settingsJson)
                ? defaultNotificationDays
                : JsonSerializer.Deserialize<List<int>>(settingsJson) ?? defaultNotificationDays;

            int notificationId = 1000;

            // Generate schedules for the next 30 days
            var startDay = DateTime.Today;
            var endDay = DateTime.Today.AddDays(30);

            for (var date = startDay; date <= endDay; date = date.AddDays(1))
            {
                // Vérifier les produits qui périment aujourd'hui
                var dlcTodayProducts = allProducts.Where(p => p.Dlc.Date == date.Date).ToList();
                if (dlcTodayProducts.Any())
                {
                    string title = "Alerte de péremption aujourd'hui";
                    string description = dlcTodayProducts.Count > 3
                        ? $"{dlcTodayProducts.Count} produits périment aujourd'hui."
                        : (dlcTodayProducts.Count == 1
                            ? $"Le produit '{dlcTodayProducts.First().Name}' périme aujourd'hui."
                            : $"{dlcTodayProducts.Count} produits périment aujourd'hui : {string.Join(", ", dlcTodayProducts.Select(p => p.Name))}.");

                    ScheduleForDate(date, title, description, ref notificationId);
                }

                // Pour chaque jour de notification configuré
                foreach (var days in notificationDays)
                {
                    var expiringProducts = allProducts.Where(p => p.Dlc.Date == date.AddDays(days).Date).ToList();
                    if (expiringProducts.Any())
                    {
                        string title;
                        string description;

                        if (expiringProducts.Count == 1)
                        {
                            var product = expiringProducts.First();
                            title = $"Alerte de péremption dans {days} jour" + (days > 1 ? "s" : "");
                            description = $"Le produit '{product.Name}' périme dans {days} jour" + (days > 1 ? "s" : "") + ".";
                        }
                        else if (expiringProducts.Count <= 3)
                        {
                            title = $"Alerte de péremption dans {days} jour" + (days > 1 ? "s" : "");
                            description = $"Les produits suivants vont périmer dans {days} jour(s) : {string.Join(", ", expiringProducts.Select(p => p.Name))}.";
                        }
                        else
                        {
                            title = $"Alerte de péremption dans {days} jour" + (days > 1 ? "s" : "");
                            description = $"{expiringProducts.Count} produits vont périmer dans {days} jour(s).";
                        }

                        ScheduleForDate(date, title, description, ref notificationId);
                    }
                }
            }
        }

        private static void ScheduleForDate(DateTime actionDate, string title, string description, ref int notificationId)
        {
            foreach (var hour in notificationHours)
            {
                var notifyTime = new DateTime(actionDate.Year, actionDate.Month, actionDate.Day, hour, 0, 0);
                if (notifyTime <= DateTime.Now) continue;

                var request = new NotificationRequest
                {
                    NotificationId = notificationId++,
                    Title = title,
                    Description = description,
                    Android = new AndroidOptions
                    {
                        IconSmallName = new AndroidIcon("perimapplogotransparent"),
                        VisibilityType = AndroidVisibilityType.Public
                    },
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = notifyTime
                    }
                };
                LocalNotificationCenter.Current.Show(request);
            }
        }
    }
}
