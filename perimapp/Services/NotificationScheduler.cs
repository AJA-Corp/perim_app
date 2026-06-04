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
        private static readonly List<int> _defaultNotificationDays = new List<int> { 1, 3, 7 };
        private static readonly List<TimeSpan> _notificationTimes = new List<TimeSpan> 
        { 
            new TimeSpan(7, 0, 0),
            new TimeSpan(11, 0, 0),
            new TimeSpan(18, 00, 0)
        };

        public static void UpdateSchedules()
        {
            LocalNotificationCenter.Current.CancelAll();

            var allProducts = AppData.CurrentProducts.ToList();
            if (!allProducts.Any()) return;

            var settingsJson = Preferences.Get("NotificationDays", string.Empty);
            List<int> notificationDays;
            if (string.IsNullOrEmpty(settingsJson))
            {
                notificationDays = _defaultNotificationDays;
            }
            else
            {
                try
                {
                    notificationDays = JsonSerializer.Deserialize<List<int>>(settingsJson) ?? _defaultNotificationDays;
                }
                catch (JsonException)
                {
                    notificationDays = _defaultNotificationDays;
                }
            }

            int notificationId = 1000;

            var startDay = DateTime.Today;
            var endDay = DateTime.Today.AddDays(15);

            for (var date = startDay; date < endDay; date = date.AddDays(1))
            {
                var dlcTodayProducts = allProducts.Where(p => p.Dlc.Date == date.Date).ToList();
                if (dlcTodayProducts.Any())
                {
                    string title = "Péremption aujourd'hui ⚠️";
                    string description = dlcTodayProducts.Count == 1 
                        ? $"Le produit '{dlcTodayProducts.First().Name}' périme aujourd'hui !"
                        : $"{dlcTodayProducts.Count} produits périment aujourd'hui !";

                    ScheduleNotification(title, description, date, _notificationTimes, ref notificationId);
                }

                var upcomingMessages = new List<string>();

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

                    ScheduleNotification(title, description, date, _notificationTimes, ref notificationId);
                }
            }
        }

        private static void ScheduleNotification(string title, string description, DateTime actionDate, List<TimeSpan> times, ref int notificationId)
        {
            foreach (var notifyTime in times
                 .Select(time => actionDate.Date.Add(time))
                 .Where(notifyTime => notifyTime > DateTime.Now))
            {
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
                LocalNotificationCenter.Current.Show(request).ContinueWith(
                    t => System.Diagnostics.Debug.WriteLine(t.Exception?.ToString()),
                    System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }
}
