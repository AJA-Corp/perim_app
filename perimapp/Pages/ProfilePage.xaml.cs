using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Text.Json;
using perimapp.PopUp;  // Pour NotificationPopUp
using perimapp.Models; // Pour UserProfile
using perimapp.Data;   // Pour AppData
using System.Linq;     // Pour les méthodes .Any() et .First()
using System;          // Pour Console.WriteLine() et Exception
using System.IO;       // Pour Stream et StreamReader
using System.Threading.Tasks; // Pour Task
using System.ComponentModel; // NOUVEAU : Ajouté pour INotifyPropertyChanged
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Views; // NOUVEAU : Ajouté pour [CallerMemberName]

namespace perimapp.Pages
{
    // NOUVEAU : Implémente l'interface INotifyPropertyChanged
    public partial class ProfilePage : ContentPage, INotifyPropertyChanged
    {
        // Champs privés de "backing" pour stocker les valeurs des propriétés
        private string _userName;
        private int _registeredProductsCount;
        private int _lostProductsCount;
        private string _familyCode;

        // Propriétés publiques avec notification de changement
        public string UserName
        {
            get => _userName;
            set
            {
                // Seulement mettre à jour et notifier si la valeur a réellement changé
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(); // Notifie l'UI que la propriété a changé
                }
            }
        }

        public int RegisteredProductsCount
        {
            get => _registeredProductsCount;
            set
            {
                if (_registeredProductsCount != value)
                {
                    _registeredProductsCount = value;
                    OnPropertyChanged(); // Notifie l'UI
                }
            }
        }

        public int LostProductsCount
        {
            get => _lostProductsCount;
            set
            {
                if (_lostProductsCount != value)
                {
                    _lostProductsCount = value;
                    OnPropertyChanged(); // Notifie l'UI
                }
            }
        }

        public string FamilyCode
        {
            get => _familyCode;
            set
            {
                if (_familyCode != value)
                {
                    _familyCode = value;
                    OnPropertyChanged(); // Notifie l'UI
                }
            }
        }

        // NOUVEAU : Implémentation de l'événement PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // NOUVEAU : Méthode helper pour déclencher l'événement PropertyChanged
        // [CallerMemberName] remplit automatiquement le nom de la propriété appelante
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ProfilePage()
        {
            InitializeComponent();

            // Initialiser les propriétés avec des valeurs par défaut pour l'affichage initial
            // Elles seront mises à jour une fois les données chargées.
            UserName = "Chargement...";
            FamilyCode = "Chargement...";
            LostProductsCount = 0;
            // RegisteredProductsCount peut être initialisé ici ou dans LoadProfileDataAsync/finally
            // Nous le mettrons à jour à la fin de LoadProfileDataAsync pour être sûr.
            RegisteredProductsCount = 0; // Valeur temporaire

            // IMPORTANT : Définir le BindingContext AVANT de lancer l'opération asynchrone
            // Les propriétés initiales seront affichées, puis mises à jour.
            BindingContext = this;

            // Lancer le chargement des données de profil de manière asynchrone ("fire and forget")
            _ = LoadProfileDataAsync();
        }

        private async Task LoadProfileDataAsync()
        {
            string jsonContent = string.Empty; // Variable pour stocker le contenu du JSON lu
            try
            {
                // Lecture du fichier responseProfilePage.json depuis les ressources de l'application
                using Stream fileStream = await FileSystem.OpenAppPackageFileAsync("responseProfilePage.json");
                using StreamReader reader = new StreamReader(fileStream);
                jsonContent = await reader.ReadToEndAsync(); // Lit tout le contenu du fichier

                // Désérialisation du JSON en une liste d'objets UserProfile
                var userProfiles = JsonSerializer.Deserialize<List<UserProfile>>(jsonContent);

                if (userProfiles != null && userProfiles.Any())
                {
                    var user = userProfiles.First(); // Prenez le premier profil du tableau
                    // Assignez les valeurs aux propriétés, ce qui déclenchera OnPropertyChanged
                    UserName = $"{user.FirstName} {user.LastName}";
                    FamilyCode = user.HomeCode;

                    // Convertir la chaîne "lost_products" en entier
                    if (int.TryParse(user.LostProducts, out int lostCount))
                    {
                        LostProductsCount = lostCount;
                    }
                    else
                    {
                        LostProductsCount = 0;
                    }
                    Console.WriteLine($"ProfilePage: Profil utilisateur chargé depuis fichier - Nom: {UserName}, Foyer: {FamilyCode}, Produits perdus: {LostProductsCount}");
                }
                else
                {
                    // Si le JSON est vide ou n'a pas de profil, utilisez des valeurs par défaut
                    SetDefaultProfileValues();
                    Console.WriteLine("ProfilePage: Fichier JSON de profil vide ou invalide, valeurs par défaut appliquées.");
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("ProfilePage [ERREUR] : Fichier 'responseProfilePage.json' introuvable dans les ressources brutes. Vérifiez son emplacement.");
                SetDefaultProfileValues();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"ProfilePage [ERREUR JSON] : Erreur lors de la désérialisation du JSON du profil. Vérifiez la structure du JSON. Message : {ex.Message}");
                Console.WriteLine($"Contenu JSON tenté de désérialiser : {jsonContent}");
                SetDefaultProfileValues();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProfilePage [ERREUR] : Une erreur inattendue est survenue lors du chargement du profil : {ex.Message}");
                SetDefaultProfileValues();
            }
            finally
            {
                // NOUVEAU : Assurez-vous de mettre à jour RegisteredProductsCount après les opérations asynchrones.
                // Cela est important car MainPage pourrait charger ses produits après le constructeur de ProfilePage.
                RegisteredProductsCount = AppData.CurrentProducts.Count;
                Console.WriteLine($"ProfilePage: Nombre de produits enregistrés mis à jour après chargement du profil : {RegisteredProductsCount}");
            }
        }

        private void SetDefaultProfileValues()
        {
            UserName = "Utilisateur Inconnu";
            FamilyCode = "N/A";
            LostProductsCount = 0;
            // Met à jour RegisteredProductsCount avec la valeur actuelle d'AppData même en cas d'erreur
            RegisteredProductsCount = AppData.CurrentProducts.Count;
        }

        private void OnActivateNotificationsClicked(object sender, EventArgs e)
        {
            var popup = new NotificationPopUp();
            this.ShowPopup(popup);
        }
    }
}