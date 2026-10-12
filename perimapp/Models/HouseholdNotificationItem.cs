using System;

namespace perimapp.Models
{
    public enum HouseholdNotificationType
    {
        ProductAdded,
        ProductModified,
        ProductDeleted,
        MemberJoined,
        Alert
    }

    public class HouseholdNotificationItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public HouseholdNotificationType Type { get; set; }
        public string BadgeColor { get; set; } = "#58BF7F";
        public string TypeLabel { get; set; } = "Info";

        public string FormattedTimestamp
        {
            get
            {
                var diff = DateTime.UtcNow - Timestamp;
                if (diff.TotalMinutes < 60)
                {
                    int mins = Math.Max(1, (int)diff.TotalMinutes);
                    return $"Il y a {mins} min";
                }
                if (diff.TotalHours < 24)
                {
                    int hours = (int)diff.TotalHours;
                    return $"Il y a {hours} h";
                }
                if (diff.TotalDays < 7)
                {
                    int days = (int)diff.TotalDays;
                    return $"Il y a {days} j";
                }
                return Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            }
        }
    }
}
