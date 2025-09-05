using Android.Content;
using AndroidX.Work;
using System;
using System.Collections.Generic;
using Java.Util.Concurrent;
using Microsoft.Maui;

namespace perimapp.Platforms.Android;

public static class NotificationWorkerManager
{
    public static void ScheduleWork()
    {
        WorkManager.GetInstance(MauiApplication.Current.ApplicationContext).CancelAllWork();

        var notificationTimes = new List<TimeSpan>
        {
            new TimeSpan(7, 0, 0), // 7h00
            new TimeSpan(11, 0, 0), // 11h00
            new TimeSpan(18, 0, 0) // 18h00
        };

        foreach (var time in notificationTimes)
        {
            var scheduleTime = DateTime.Today.Add(time);
            if (scheduleTime <= DateTime.Now)
            {
                scheduleTime = scheduleTime.AddDays(1);
            }

            var initialDelay = scheduleTime - DateTime.Now;

            // Définir explicitement le type de la variable
            PeriodicWorkRequest workRequest = new PeriodicWorkRequest.Builder(typeof(NotificationWorker), TimeSpan.FromDays(1))
                .SetInitialDelay((long)initialDelay.TotalMilliseconds, TimeUnit.Milliseconds)
                .AddTag(time.Hours.ToString())
                .Build() as PeriodicWorkRequest;

            WorkManager.GetInstance(MauiApplication.Current.ApplicationContext).EnqueueUniquePeriodicWork(
                $"notification-worker-{time.Hours}",
                ExistingPeriodicWorkPolicy.Update,
                workRequest);
        }
    }
}