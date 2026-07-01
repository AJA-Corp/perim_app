# Guide d'implémentation et de mise en place des tests unitaires (Walkthrough)

Ce guide détaille la mise en place de la suite de tests unitaires du projet **Perim'App**, son architecture et la méthodologie à suivre pour maintenir et faire évoluer les tests.

---

## 1. Structure du Projet de Tests
Le projet de tests unitaires est situé dans le répertoire [perimapp.Tests/](file:///c:/Users/compt/Documents/Github/AJA_Corp/perim_app/perimapp.Tests) et utilise le framework de tests **xUnit**.

### Fichiers de Tests :
* **[ConvertersTests.cs](file:///c:/Users/compt/Documents/Github/AJA_Corp/perim_app/perimapp.Tests/ConvertersTests.cs)** : Valide le comportement du convertisseur de couleurs pour les DLC (`DlcColorConverter`).
* **[ProductInfosTests.cs](file:///c:/Users/compt/Documents/Github/AJA_Corp/perim_app/perimapp.Tests/ProductInfosTests.cs)** : Valide le modèle `ProductInfos` (calcul de la DLC restante, texte d'affichage, etc.).
* **[LocalServicesTests.cs](file:///c:/Users/compt/Documents/Github/AJA_Corp/perim_app/perimapp.Tests/LocalServicesTests.cs)** : Valide les services locaux de gestion de fichiers JSON (`LocalProductService`, `LocalUserService`).
* **[ApiServicesTests.cs](file:///c:/Users/compt/Documents/Github/AJA_Corp/perim_app/perimapp.Tests/ApiServicesTests.cs)** : Simule des réponses réseau d'API et valide l'authentification (`AuthService`), la recherche de produits (`ApiProductService`), la récupération de profil (`ApiProfileService`) et la synchronisation (`SyncService`).
* **[NotificationSchedulerTests.cs](file:///c:/Users/compt/Documents/Github/AJA_Corp/perim_app/perimapp.Tests/NotificationSchedulerTests.cs)** : Teste la planification de notifications push locales.
* **[ViewModelsTests.cs](file:///c:/Users/compt/Documents/Github/AJA_Corp/perim_app/perimapp.Tests/ViewModelsTests.cs)** : Valide le comportement et la navigation de tous les ViewModels de l'application en utilisant des simulations de services (Mocks).

---

## 2. Choix d'Architecture (Rendre le code testable)

### A. Découplage de la Plateforme (Abstractions)
Le code original de l'application utilisait des appels statiques liés à la plateforme mobile .NET MAUI (`Shell.Current`, `MainThread.BeginInvokeOnMainThread`, instanciation directe des vues de popup), empêchant l'exécution des tests sur une machine de build ou dans GitHub Actions.

Pour y remédier, nous avons créé et injecté trois services IHM abstraits :
1. **`INavigationService`** (navigation modale et Shell).
2. **`IDialogService`** (popups graphiques Toolkit, alertes natives et menus d'action).
3. **`IDispatcherService`** (exécution de code asynchrone sur le thread principal de l'UI).

### B. Conteneur IoC et Injection de Dépendances (DI)
* Dans le projet principal, les services et ViewModels sont enregistrés dans le conteneur IoC de .NET dans [MauiProgram.cs](file:///c:/Users/compt/Documents/Github/AJA_Corp/perim_app/perimapp/MauiProgram.cs).
* Les constructeurs des Views (ex: [MainView.xaml.cs](file:///c:/Users/compt/Documents/Github/AJA_Corp/perim_app/perimapp/Views/MainView.xaml.cs)) acceptent désormais leurs ViewModels par injection de dépendances.

---

## 3. Techniques clés de simulation pour les Tests unitaires

### A. Utilisation de Mocks pour l'IHM
Dans le projet de tests [ViewModelsTests.cs](file:///c:/Users/compt/Documents/Github/AJA_Corp/perim_app/perimapp.Tests/ViewModelsTests.cs), nous définissons des versions factices de nos services d'interface :
* `MockNavigationService` : Enregistre le nom de la route naviguée dans la propriété `NavigatedTo` pour vérification par assertion.
* `MockDialogService` : Permet d'injecter des choix fictifs de confirmation ou des textes saisis au clavier (prompts) et vérifie si une alerte a été affichée.
* `MockDispatcherService` : Exécute immédiatement les actions sur le thread de test.

### B. Simulation Réseau (HTTP)
Nous utilisons `MockHttpMessageHandler` pour intercepter les requêtes HTTP émises par les services d'API de l'application. Cela nous permet de simuler des scénarios réussis (code de retour `200 OK` avec des payloads JSON factices) et des scénarios d'erreur (code de retour `400 Bad Request` ou `500 Server Error`).

### C. Isolation du Système de Fichiers (Sandboxing)
Les tests pour les services de fichiers locaux génèrent un dossier temporaire unique à chaque instance de test et le suppriment pendant la phase `Dispose()` afin d'éviter tout conflit de lecture/écriture.

### D. Désactivation de la parallélisation
Comme la classe `AppData` conserve des données globales statiques en mémoire (`AppData.CurrentUser`), les tests unitaires ont été configurés avec l'attribut xUnit suivant pour forcer une exécution séquentielle et éviter les collisions :
```csharp
[assembly: CollectionBehavior(DisableTestParallelization = true)]
```

---

## 4. Comment écrire un nouveau test unitaire ?

Pour ajouter un test, créez une méthode avec l'attribut `[Fact]` dans la classe de test appropriée :

```csharp
[Fact]
public async Task MonNouveauTest_DoitFaireQuelqueChose()
{
    // Arrange (Préparation des données et des Mocks)
    var mockNav = new MockNavigationService();
    var mockDialog = new MockDialogService();
    var vm = new StartingViewModel(mockNav, mockDialog);

    // Act (Appel de l'action à tester)
    await vm.LogInCommand.ExecuteAsync(null);

    // Assert (Vérification du résultat attendu)
    Assert.Equal("LogInView", mockNav.NavigatedTo);
}
```
