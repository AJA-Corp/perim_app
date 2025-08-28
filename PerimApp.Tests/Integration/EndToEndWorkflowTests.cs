using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using PerimApp.Core.Models;
using PerimApp.Core.Services;
using PerimApp.Core.Converters;
using PerimApp.Core.Utilities;

namespace PerimApp.Tests.Integration
{
    /// <summary>
    /// Tests de fonctionnalités end-to-end simulés pour les workflows complets de l'application
    /// </summary>
    public class EndToEndWorkflowTests
    {
        [Fact]
        public void UserRegistration_CompleteWorkflow_ShouldValidateAllSteps()
        {
            // Arrange - Données d'inscription utilisateur
            var userData = new UserProfileDetails
            {
                FirstName = "Marie",
                LastName = "Martin",
                Email = "marie.martin@example.com",
                Password = "SecurePassword123!",
                HomeCode = 123456
            };

            // Act & Assert - Étape 1: Validation des données
            userData.IsValid().Should().BeTrue("Les données utilisateur doivent être valides");

            // Act & Assert - Étape 2: Validation de l'email
            UserProfileDetails.IsValidEmail(userData.Email).Should().BeTrue("L'email doit être valide");

            // Act & Assert - Étape 3: Validation de la force du mot de passe
            userData.IsPasswordStrong().Should().BeTrue("Le mot de passe doit être fort");
            PasswordHasher.IsPasswordStrong(userData.Password).Should().BeTrue("Le mot de passe doit passer la validation du service");

            // Act & Assert - Étape 4: Hachage du mot de passe
            var hashedPassword = PasswordHasher.HashPassword(userData.Password);
            hashedPassword.Should().NotBeNullOrEmpty("Le mot de passe doit être haché");
            hashedPassword.Should().NotBe(userData.Password, "Le hash doit être différent du mot de passe original");

            // Act & Assert - Étape 5: Vérification du mot de passe
            PasswordHasher.VerifyPassword(userData.Password, hashedPassword).Should().BeTrue("Le mot de passe doit être vérifiable");

            // Act & Assert - Étape 6: Nom complet
            userData.FullName.Should().Be("Marie Martin", "Le nom complet doit être correctement formaté");
        }

        [Fact]
        public void ProductAddition_CompleteWorkflow_ShouldValidateAllSteps()
        {
            // Arrange - Données de produit
            var productData = new ProductInfos
            {
                Barcode = 3245414567890,
                Name = "Yaourt nature Bio",
                Category = "Produits laitiers",
                Conservation = "Réfrigéré",
                UrlImage = "https://example.com/yaourt.jpg",
                Quantity = 4,
                Dlc = DateTime.Today.AddDays(5),
                AddedAt = DateTime.Now
            };

            // Act & Assert - Étape 1: Validation des données de base
            productData.IsValid().Should().BeTrue("Les données du produit doivent être valides");

            // Act & Assert - Étape 2: Validation individuelle des champs
            ValidationUtils.IsValidBarcode(productData.Barcode).Should().BeTrue("Le code-barres doit être valide");
            ValidationUtils.IsValidQuantity(productData.Quantity).Should().BeTrue("La quantité doit être valide");
            ValidationUtils.IsValidDlc(productData.Dlc).Should().BeTrue("La DLC doit être valide");
            ValidationUtils.IsValidImageUrl(productData.UrlImage).Should().BeTrue("L'URL de l'image doit être valide");
            ValidationUtils.IsValidCategory(productData.Category).Should().BeTrue("La catégorie doit être valide");

            // Act & Assert - Étape 3: Nettoyage du nom
            var cleanedName = ValidationUtils.CleanProductName(productData.Name);
            cleanedName.Should().Be(productData.Name, "Le nom du produit ne devrait pas changer s'il est déjà propre");

            // Act & Assert - Étape 4: Calculs de dates
            productData.DaysRemaining.Should().Be(5, "Il devrait rester 5 jours");
            productData.IsExpired.Should().BeFalse("Le produit ne devrait pas être expiré");
            productData.ExpiresNow.Should().BeFalse("Le produit ne devrait pas expirer aujourd'hui");
            productData.ExpiresSoon.Should().BeFalse("Le produit ne devrait pas expirer bientôt (seuil de 3 jours)");

            // Act & Assert - Étape 5: Formatage pour l'affichage
            productData.DaysRemainingTextMainPage.Should().Be("5j", "Le texte pour la page principale doit être correct");
            productData.DaysRemainingTextDetailsPage.Should().Be("5 jours", "Le texte pour la page de détails doit être correct");

            // Act & Assert - Étape 6: Détermination de la couleur
            var color = DlcColorConverter.GetColorFromProduct(productData);
            color.Should().Be(DlcColorConverter.ColorResult.Green, "Un produit qui expire dans 5 jours devrait être vert");
            DlcColorConverter.IsDangerous(color).Should().BeFalse("Un produit qui expire dans 5 jours ne devrait pas être dangereux");
        }

        [Fact]
        public void ProductExpiration_AlertWorkflow_ShouldHandleAllScenarios()
        {
            // Arrange - Créer des produits avec différentes dates d'expiration
            var products = new List<ProductInfos>
            {
                CreateProduct("Lait", DateTime.Today.AddDays(-2)), // Expiré
                CreateProduct("Pain", DateTime.Today), // Expire aujourd'hui
                CreateProduct("Fromage", DateTime.Today.AddDays(1)), // Expire demain
                CreateProduct("Pommes", DateTime.Today.AddDays(3)), // Expire bientôt
                CreateProduct("Conserve", DateTime.Today.AddDays(30)) // Expire plus tard
            };

            // Act & Assert - Classification par urgence
            var expiredProducts = products.Where(p => p.IsExpired).ToList();
            var expiringTodayProducts = products.Where(p => p.ExpiresNow).ToList();
            var expiringSoonProducts = products.Where(p => p.ExpiresSoon).ToList();

            expiredProducts.Should().ContainSingle(p => p.Name == "Lait");
            expiringTodayProducts.Should().ContainSingle(p => p.Name == "Pain");
            expiringSoonProducts.Should().ContainSingle(p => p.Name == "Fromage");

            // Act & Assert - Tri par urgence (les plus urgents en premier)
            var sortedProducts = products.OrderBy(p => p.DaysRemaining).ToList();
            sortedProducts[0].Name.Should().Be("Lait", "Le produit expiré devrait être en premier");
            sortedProducts[1].Name.Should().Be("Pain", "Le produit qui expire aujourd'hui devrait être en deuxième");
            sortedProducts.Last().Name.Should().Be("Conserve", "Le produit qui expire le plus tard devrait être en dernier");

            // Act & Assert - Couleurs d'alerte
            foreach (var product in products)
            {
                var color = DlcColorConverter.GetColorFromProduct(product);
                var isDangerous = DlcColorConverter.IsDangerous(color);
                var urgencyDescription = DlcColorConverter.GetUrgencyDescription(product.DaysRemaining);

                switch (product.Name)
                {
                    case "Lait":
                        color.Should().Be(DlcColorConverter.ColorResult.Red);
                        isDangerous.Should().BeTrue();
                        urgencyDescription.Should().Be("Produit expiré");
                        break;
                    case "Pain":
                        color.Should().Be(DlcColorConverter.ColorResult.Orange);
                        isDangerous.Should().BeTrue();
                        urgencyDescription.Should().Be("Expire aujourd'hui");
                        break;
                    case "Fromage":
                        color.Should().Be(DlcColorConverter.ColorResult.Orange);
                        isDangerous.Should().BeTrue();
                        urgencyDescription.Should().Be("Expire demain");
                        break;
                    case "Pommes":
                        color.Should().Be(DlcColorConverter.ColorResult.Yellow);
                        isDangerous.Should().BeFalse();
                        urgencyDescription.Should().Be("Expire dans 3 jours");
                        break;
                    case "Conserve":
                        color.Should().Be(DlcColorConverter.ColorResult.Green);
                        isDangerous.Should().BeFalse();
                        urgencyDescription.Should().Be("Expire ce mois");
                        break;
                }
            }
        }

        [Fact]
        public void ProductModification_CompleteWorkflow_ShouldMaintainDataIntegrity()
        {
            // Arrange - Produit original
            var originalProduct = CreateProduct("Yaourt", DateTime.Today.AddDays(3));
            originalProduct.Id = 1;
            originalProduct.Quantity = 2;

            // Act - Cloner le produit pour modification
            var modifiedProduct = originalProduct.Clone();

            // Assert - Le clone doit être une copie exacte mais indépendante
            modifiedProduct.Should().NotBeSameAs(originalProduct);
            modifiedProduct.Id.Should().Be(originalProduct.Id);
            modifiedProduct.Name.Should().Be(originalProduct.Name);
            modifiedProduct.Dlc.Should().Be(originalProduct.Dlc);

            // Act - Modifier le clone
            modifiedProduct.Dlc = DateTime.Today.AddDays(5);
            modifiedProduct.Quantity = 1;

            // Assert - Les modifications ne doivent pas affecter l'original
            originalProduct.Dlc.Should().Be(DateTime.Today.AddDays(3));
            originalProduct.Quantity.Should().Be(2);
            modifiedProduct.Dlc.Should().Be(DateTime.Today.AddDays(5));
            modifiedProduct.Quantity.Should().Be(1);

            // Act & Assert - Validation des modifications
            var validationErrors = ValidationUtils.GetProductValidationErrors(modifiedProduct);
            validationErrors.Should().BeEmpty("Le produit modifié doit rester valide");

            // Act & Assert - Recalcul des propriétés dépendantes
            modifiedProduct.DaysRemaining.Should().Be(5);
            modifiedProduct.DaysRemainingTextMainPage.Should().Be("5j");
            var newColor = DlcColorConverter.GetColorFromProduct(modifiedProduct);
            newColor.Should().Be(DlcColorConverter.ColorResult.Green);
        }

        [Fact]
        public void DataValidation_CompleteWorkflow_ShouldCatchAllErrors()
        {
            // Arrange - Produit avec plusieurs erreurs
            var invalidProduct = new ProductInfos
            {
                Barcode = 123, // Trop court
                Name = "", // Vide
                Quantity = 0, // Invalide
                Dlc = DateTime.Today.AddDays(-1), // Passé
                AddedAt = DateTime.Now.AddDays(1), // Futur
                UrlImage = "invalid-url", // URL invalide
                Category = "InvalidCategory" // Catégorie invalide
            };

            // Act
            var errors = ValidationUtils.GetProductValidationErrors(invalidProduct);

            // Assert - Toutes les erreurs doivent être détectées
            errors.Should().NotBeEmpty("Des erreurs de validation doivent être détectées");
            errors.Should().Contain(e => e.Contains("code-barres"));
            errors.Should().Contain(e => e.Contains("nom du produit"));
            errors.Should().Contain(e => e.Contains("quantité"));
            errors.Should().Contain(e => e.Contains("date de péremption"));
            errors.Should().Contain(e => e.Contains("date d'ajout"));
            errors.Should().Contain(e => e.Contains("URL de l'image"));

            // Act & Assert - Le produit ne doit pas être considéré comme valide
            invalidProduct.IsValid().Should().BeFalse("Un produit avec des erreurs ne doit pas être valide");
        }

        [Fact]
        public void DateUtilities_CompleteWorkflow_ShouldHandleAllScenarios()
        {
            // Arrange
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);
            var nextWeek = today.AddDays(7);

            // Act & Assert - Tests de classification temporelle
            DateUtils.IsInPast(yesterday).Should().BeTrue();
            DateUtils.IsInPast(today).Should().BeFalse();
            DateUtils.IsInPast(tomorrow).Should().BeFalse();

            DateUtils.IsToday(today).Should().BeTrue();
            DateUtils.IsToday(yesterday).Should().BeFalse();
            DateUtils.IsToday(tomorrow).Should().BeFalse();

            DateUtils.IsInNearFuture(tomorrow, 7).Should().BeTrue();
            DateUtils.IsInNearFuture(nextWeek, 7).Should().BeTrue();
            DateUtils.IsInNearFuture(today.AddDays(8), 7).Should().BeFalse();

            // Act & Assert - Calculs de différences
            DateUtils.DaysBetween(today, tomorrow).Should().Be(1);
            DateUtils.DaysBetween(tomorrow, today).Should().Be(-1);
            DateUtils.DaysBetween(today, today).Should().Be(0);

            // Act & Assert - Formatage français
            var testDate = new DateTime(2024, 12, 25, 14, 30, 0);
            DateUtils.FormatFrenchDate(testDate).Should().Be("25/12/2024");
            DateUtils.FormatFrenchDateTime(testDate).Should().Be("25/12/2024 14:30");
        }

        [Fact]
        public void PasswordSecurity_CompleteWorkflow_ShouldEnsureSecurity()
        {
            // Arrange - Différents types de mots de passe
            var passwords = new Dictionary<string, bool>
            {
                { "password", false }, // Faible
                { "Password123", false }, // Pas de caractère spécial
                { "Password123!", true }, // Fort
                { "P@ssw0rd!", true }, // Fort
                { "weak", false }, // Trop court
                { "", false }, // Vide
            };

            foreach (var (password, shouldBeStrong) in passwords)
            {
                if (string.IsNullOrEmpty(password))
                {
                    // Act & Assert - Mots de passe vides
                    var action = () => PasswordHasher.HashPassword(password);
                    action.Should().Throw<ArgumentException>();
                    continue;
                }

                // Act & Assert - Validation de la force
                PasswordHasher.IsPasswordStrong(password).Should().Be(shouldBeStrong);

                // Act & Assert - Recommandations
                var recommendations = PasswordHasher.GetPasswordRecommendations(password);
                if (shouldBeStrong)
                {
                    recommendations.Should().BeEmpty("Un mot de passe fort ne devrait pas avoir de recommandations");
                }
                else
                {
                    recommendations.Should().NotBeEmpty("Un mot de passe faible devrait avoir des recommandations");
                }

                // Act & Assert - Hachage et vérification (seulement pour les mots de passe non vides)
                var hash = PasswordHasher.HashPassword(password);
                hash.Should().NotBeNullOrEmpty();
                PasswordHasher.VerifyPassword(password, hash).Should().BeTrue();
                PasswordHasher.VerifyPassword("wrongpassword", hash).Should().BeFalse();
            }
        }

        private static ProductInfos CreateProduct(string name, DateTime dlc)
        {
            return new ProductInfos
            {
                Barcode = 1234567890 + name.GetHashCode() % 1000000, // Génère un code-barres unique
                Name = name,
                Category = "Test",
                Conservation = "Test",
                UrlImage = $"https://example.com/{name.ToLower()}.jpg",
                Quantity = 1,
                Dlc = dlc,
                AddedAt = DateTime.Today.AddDays(-1)
            };
        }
    }
}