using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using perimapp.Models;
using perimapp.Services;

namespace perimapp.ViewModels
{
    public partial class NotificationsViewModel : ObservableObject
    {
        private readonly LocalProductService _localProductService;
        private readonly LocalUserService _localUserService;

        public ObservableCollection<HouseholdNotificationItem> Notifications { get; } = new();
        public ObservableCollection<HouseholdNotificationItem> FilteredNotifications { get; } = new();

        [ObservableProperty]
        private string _foyerCode = "---";

        [ObservableProperty]
        private string _selectedFilter = "Tous";

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private int _totalCount;

        public NotificationsViewModel(LocalProductService localProductService, LocalUserService localUserService)
        {
            _localProductService = localProductService;
            _localUserService = localUserService;
        }

        [RelayCommand]
        public async Task LoadNotificationsAsync()
        {
            try
            {
                var user = await _localUserService.LoadUserAsync();
                if (user != null && !string.IsNullOrEmpty(user.HomeCode))
                {
                    FoyerCode = user.HomeCode;
                }

                var allProducts = await _localProductService.LoadProductsAsync();

                var items = new System.Collections.Generic.List<HouseholdNotificationItem>();

                // Événements d'ajout
                foreach (var product in allProducts.Where(p => p.State == "Active"))
                {
                    items.Add(new HouseholdNotificationItem
                    {
                        Title = "Produit ajouté",
                        Description = product.DisplayName,
                        Detail = $"Quantité: {product.Quantity} • DLC: {product.Dlc:dd/MM/yyyy}",
                        Timestamp = product.AddedAt != default ? product.AddedAt : DateTime.UtcNow.AddHours(-2),
                        Type = HouseholdNotificationType.ProductAdded,
                        BadgeColor = "#58BF7F",
                        TypeLabel = "Ajout"
                    });

                    // Événements de modification si date de modification > date d'ajout
                    if (product.LastModified > product.AddedAt.AddMinutes(1))
                    {
                        items.Add(new HouseholdNotificationItem
                        {
                            Title = "Produit modifié",
                            Description = product.DisplayName,
                            Detail = $"Mis à jour • DLC: {product.Dlc:dd/MM/yyyy}",
                            Timestamp = product.LastModified,
                            Type = HouseholdNotificationType.ProductModified,
                            BadgeColor = "#F8961E",
                            TypeLabel = "Modif"
                        });
                    }
                }

                // Événements de suppression
                foreach (var product in allProducts.Where(p => p.State != "Active"))
                {
                    items.Add(new HouseholdNotificationItem
                    {
                        Title = "Produit retiré",
                        Description = product.DisplayName,
                        Detail = "Retiré ou consommé",
                        Timestamp = product.LastModified != default ? product.LastModified : DateTime.UtcNow.AddDays(-1),
                        Type = HouseholdNotificationType.ProductDeleted,
                        BadgeColor = "#F94144",
                        TypeLabel = "Suppr"
                    });
                }

                // Événement membre du foyer
                if (user != null)
                {
                    items.Add(new HouseholdNotificationItem
                    {
                        Title = "Membre du foyer",
                        Description = $"{user.FirstName} {user.LastName}".Trim(),
                        Detail = $"Compte associé au foyer {user.HomeCode}",
                        Timestamp = DateTime.UtcNow.AddDays(-3),
                        Type = HouseholdNotificationType.MemberJoined,
                        BadgeColor = "#4A90E2",
                        TypeLabel = "Membre"
                    });
                }

                var sorted = items.OrderByDescending(i => i.Timestamp).ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Notifications.Clear();
                    foreach (var itm in sorted)
                    {
                        Notifications.Add(itm);
                    }
                    TotalCount = Notifications.Count;
                    ApplyFilter(SelectedFilter);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationsViewModel] Erreur chargement : {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadNotificationsAsync();
        }

        [RelayCommand]
        private void SetFilter(string filter)
        {
            SelectedFilter = filter;
            ApplyFilter(filter);
        }

        private void ApplyFilter(string filter)
        {
            FilteredNotifications.Clear();

            var query = filter switch
            {
                "Ajouts" => Notifications.Where(n => n.Type == HouseholdNotificationType.ProductAdded),
                "Modifs" => Notifications.Where(n => n.Type == HouseholdNotificationType.ProductModified),
                "Suppressions" => Notifications.Where(n => n.Type == HouseholdNotificationType.ProductDeleted),
                "Membres" => Notifications.Where(n => n.Type == HouseholdNotificationType.MemberJoined),
                _ => Notifications
            };

            foreach (var item in query)
            {
                FilteredNotifications.Add(item);
            }
        }
    }
}
