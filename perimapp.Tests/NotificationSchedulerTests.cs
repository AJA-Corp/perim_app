using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Plugin.LocalNotification;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using Xunit;

namespace perimapp.Tests
{
    public class NotificationSchedulerTests
    {
        [Fact]
        public void NotificationScheduler_UpdateSchedules_ShouldScheduleNotifications_ForExpiringProducts()
        {
            // Arrange
            var shownRequests = new List<NotificationRequest>();
            bool cancelAllCalled = false;

            NotificationScheduler.CancelAllOverride = () => cancelAllCalled = true;
            NotificationScheduler.ShowOverride = req =>
            {
                shownRequests.Add(req);
                return Task.FromResult(true);
            };
            NotificationScheduler.PreferenceGetter = (key, defValue) => "[1,3]";

            AppData.Clear();
            var today = DateOnly.FromDateTime(DateTime.Today);

            var p1 = new ProductInfos
            {
                Name = "Expiring Today",
                Dlc = today,
                State = "Active"
            };

            var p2 = new ProductInfos
            {
                Name = "Expiring in 3 days",
                Dlc = today.AddDays(3),
                State = "Active"
            };

            AppData.CurrentProducts.Add(p1);
            AppData.CurrentProducts.Add(p2);

            // Act
            NotificationScheduler.UpdateSchedules();

            // Assert
            Assert.True(cancelAllCalled);
            Assert.NotEmpty(shownRequests);

            // Check if "aujourd'hui" is in at least one notification description
            var todayNotif = shownRequests.FirstOrDefault(r => r.Title.Contains("aujourd'hui") || r.Description.Contains("aujourd'hui"));
            Assert.NotNull(todayNotif);

            // Check if "surveiller" is in at least one notification description (upcoming)
            var upcomingNotif = shownRequests.FirstOrDefault(r => r.Title.Contains("surveiller") || r.Description.Contains("Dans 3 jours"));
            Assert.NotNull(upcomingNotif);

            // Clean up overrides
            NotificationScheduler.CancelAllOverride = null;
            NotificationScheduler.ShowOverride = null;
            NotificationScheduler.PreferenceGetter = null;
        }
    }
}
