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
        private static readonly List<TimeSpan> notificationTimes = new List<TimeSpan> 
        { 
            new TimeSpan(7, 0, 0),   // 7h00
            new TimeSpan(11, 0, 0),  // 11h00
            new TimeSpan(18, 00, 0)  // 18h00
        };

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

            // Generate schedules for the next 15 days (pour éviter la limite des 64 notifications d'iOS et la limite Android)
            var startDay = DateTime.Today;
            var endDay = DateTime.Today.AddDays(15);

            for (var date = startDay; date <= endDay; date = date.AddDays(1))
            {
                // -- 1ère NOTIFICATION : Les produits qui périment AUJOURD'HUI --
                var dlcTodayProducts = allProducts.Where(p => p.Dlc.Date == date.Date).ToList();
                if (dlcTodayProducts.Any())
                {
                    string title = "Péremption aujourd'hui ⚠️";
                    string description = dlcTodayProducts.Count == 1 
                        ? $"Le produit '{dlcTodayProducts.First().Name}' périme aujourd'hui !"
                        : $"{dlcTodayProducts.Count} produits périment aujourd'hui !";

                    ScheduleNotification(title, description, date, notificationTimes, ref notificationId);
                }

                // -- 2ème NOTIFICATION : Les autres jours (à venir) --
                var upcomingMessages = new List<string>();

                // On trie les jours pour les afficher dans l'ordre (ex: 1 jour, 2 jours...)
                foreach (var days in notificationDays.OrderBy(d => d))
                {
                    var expiringProducts = allProducts.Where(p => p.Dlc.Date == date.AddDays(days).Date).ToList();
                    if (expiringProducts.Any())
                    {
                        string dayText = days > 1 ? "jours" : "jour";
                        upcomingMessages.Add(expiringProducts.Count == 1
                            ? $"Dans {days} {dayText} : 1 produit ({expiringProducts.First().Name})"
                            : $"Dans {days} {dayText} : {expiringProducts.Count} produits");
                    }
                }

                if (upcomingMessages.Any())
                {
                    string title = "Péremptions à surveiller 🗓️";
                    string description = "• " + string.Join("\n• ", upcomingMessages);

                    ScheduleNotification(title, description, date, notificationTimes, ref notificationId);
                }
            }
        }

        private static void ScheduleNotification(string title, string description, DateTime actionDate, List<TimeSpan> times, ref int notificationId)
        {
            foreach (var time in times)
            {
                var notifyTime = actionDate.Date.Add(time);
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
