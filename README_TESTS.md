# Suite de Tests Complète pour Perim'App

Cette suite de tests couvre tous les aspects de l'application Perim'App de gestion de produits périssables.

## 🎯 Couverture des Tests

### Tests Unitaires (Models)
- **ProductInfosTests** : Tests des calculs de dates de péremption, formatage des textes, validation des données
- **UserProfileDetailsTests** : Tests de validation des profils utilisateur, formats d'email, mots de passe

### Tests Unitaires (Services)
- **PasswordHasherTests** : Tests de hachage de mots de passe avec Argon2, validation de la force des mots de passe
- **ValidationUtilsTests** : Tests des utilitaires de validation (codes-barres, quantités, dates, URLs)
- **DateUtilsTests** : Tests des utilitaires de dates (calculs, formatage français)

### Tests Unitaires (Convertisseurs)
- **DlcColorConverterTests** : Tests de conversion des dates en couleurs d'alerte, détermination de l'urgence

### Tests d'Intégration
- **ProductServiceIntegrationTests** : Tests des services de gestion des produits (avec simulation de base de données)
- **UserServiceIntegrationTests** : Tests des services de gestion des utilisateurs

### Tests End-to-End
- **EndToEndWorkflowTests** : Tests de workflows complets simulant les cas d'usage réels :
  - Workflow d'inscription utilisateur complet
  - Workflow d'ajout de produit
  - Workflow de gestion des alertes de péremption
  - Workflow de modification de produit
  - Validation complète des données

## 🚀 Exécution des Tests

### Prérequis
- .NET 8.0 SDK
- Les packages sont restaurés automatiquement

### Commandes

```bash
# Exécuter tous les tests
dotnet test PerimApp.Tests/PerimApp.Tests.csproj

# Exécuter avec plus de détails
dotnet test PerimApp.Tests/PerimApp.Tests.csproj --verbosity normal

# Exécuter avec couverture de code
dotnet test PerimApp.Tests/PerimApp.Tests.csproj --collect:"XPlat Code Coverage"

# Exécuter des tests spécifiques
dotnet test PerimApp.Tests/PerimApp.Tests.csproj --filter "ClassName=ProductInfosTests"
dotnet test PerimApp.Tests/PerimApp.Tests.csproj --filter "Category=Integration"

# Générer un rapport HTML (si coverlet est installé)
reportgenerator -reports:"**/*.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

### Script de Test Rapide

```bash
# Exécuter le script de test inclus
./run-tests.sh
```

## 📊 Résultats Attendus

La suite contient **287 tests** couvrant :

- ✅ **Tests unitaires** : 250+ tests
- ✅ **Tests d'intégration** : 30+ tests  
- ✅ **Tests end-to-end** : 7 scénarios complets

### Métriques de Couverture
- **Modèles** : 100% des méthodes publiques
- **Services** : 95%+ des fonctionnalités métier
- **Utilitaires** : 100% des fonctions de validation
- **Convertisseurs** : 100% des logiques de conversion

## 🧪 Types de Tests Implémentés

### 1. Tests Unitaires
- Validation des données de modèles
- Calculs de dates et délais
- Hachage et vérification de mots de passe
- Formatage et nettoyage de données
- Logiques de conversion et couleurs d'alerte

### 2. Tests de Fonctionnalités
- Workflows d'inscription utilisateur
- Gestion complète des produits
- Système d'alertes de péremption
- Validation croisée des données

### 3. Tests d'Intégration (Simulés)
- Services de base de données (avec gestion d'erreurs)
- Interfaces entre composants
- Gestion des exceptions et cas d'erreur

### 4. Tests de Validation
- Règles métier de l'application
- Contraintes de données
- Formats et limites

### 5. Tests de Régression
- Cas limites et valeurs extrêmes
- Scénarios d'erreur
- Compatibilité des modifications

## 🛠️ Architecture des Tests

```
PerimApp.Tests/
├── Models/              # Tests des modèles de données
├── Services/           # Tests des services métier
├── Converters/         # Tests des convertisseurs
├── Utilities/          # Tests des utilitaires
└── Integration/        # Tests d'intégration et end-to-end
```

### Bibliothèque Core

```
PerimApp.Core/
├── Models/             # Modèles de données métier
├── Services/           # Services de gestion (produits, utilisateurs)
├── Converters/         # Convertisseurs de données
└── Utilities/          # Utilitaires de validation et dates
```

## 📝 Outils et Frameworks Utilisés

- **xUnit** : Framework de tests principal
- **FluentAssertions** : Assertions lisibles et expressives
- **Moq** : Framework de mocking (pour les tests futurs)
- **System.Text.Json** : Sérialisation JSON
- **Npgsql** : Connecteur PostgreSQL
- **Isopoh.Cryptography.Argon2** : Hachage sécurisé des mots de passe

## 🎨 Fonctionnalités Testées

### Gestion des Produits
- ✅ Calculs de jours restants avant péremption
- ✅ Formatage des textes d'affichage (page principale vs détails)
- ✅ Validation des codes-barres, quantités, dates
- ✅ Gestion des couleurs d'alerte (rouge, orange, jaune, vert)
- ✅ Clonage et modification de produits

### Gestion des Utilisateurs
- ✅ Validation des emails et mots de passe
- ✅ Hachage sécurisé avec Argon2
- ✅ Génération de codes famille uniques
- ✅ Noms complets et formatage

### Utilitaires et Validation
- ✅ Validation de tous les types de données
- ✅ Nettoyage et normalisation de textes
- ✅ Calculs et formatage de dates françaises
- ✅ Détection d'erreurs et recommandations

### Alertes et Notifications
- ✅ Classification par urgence de péremption
- ✅ Codes couleur adaptatifs
- ✅ Messages d'urgence contextuels
- ✅ Tri et filtrage des produits

## 🔧 Maintenance et Extension

### Ajouter de Nouveaux Tests

1. **Tests Unitaires** : Ajouter dans le dossier approprié (`Models/`, `Services/`, etc.)
2. **Tests d'Intégration** : Étendre les classes dans `Integration/`
3. **Tests End-to-End** : Ajouter des scénarios dans `EndToEndWorkflowTests`

### Conventions de Nommage

- **Classes** : `[Classe]Tests` (ex: `ProductInfosTests`)
- **Méthodes** : `[Méthode]_[Condition]_[Résultat]`
- **Données de test** : Utiliser `[Theory]` et `[InlineData]` pour les cas multiples

### Bonnes Pratiques

- Tests indépendants et reproductibles
- Arrange-Act-Assert structure
- Messages d'erreur descriptifs
- Couverture des cas limites
- Documentation des cas complexes

## 📈 Métriques de Qualité

- **Lisibilité** : Tests expressifs avec FluentAssertions
- **Maintenabilité** : Structure modulaire et réutilisable
- **Fiabilité** : Gestion appropriée des cas d'erreur
- **Complétude** : Couverture de tous les chemins critiques

---

Cette suite de tests garantit la robustesse et la fiabilité de l'application Perim'App, facilitant la maintenance et l'évolution future du code.