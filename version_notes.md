# Notes de version

## Version 1.2.0 - 01/07/2026

- **Refactoring d'Architecture MVVM Propre (IoC & DI)** :
  - Création des abstractions et implémentations pour la navigation (`INavigationService`), les dialogues/popups (`IDialogService`) et le dispatcher de thread UI (`IDispatcherService`).
  - Nettoyage des ViewModels par suppression de toutes les méthodes d'aide virtuelles redondantes au profit de l'injection.
  - Enregistrement de tous les services, ViewModels et Views dans le conteneur IoC (`MauiProgram.cs`).
  - Injection des ViewModels directement dans le constructeur (Code-Behind) de chaque View.
- **Mise en place de la Suite de Tests Unitaires (84 tests xUnit)** :
  - Création et configuration du projet `perimapp.Tests`.
  - Écriture de tests unitaires complets couvrant Modèles, Convertisseurs, Services locaux, API/Synchro, NotificationScheduler, et tous les ViewModels (y compris `ProfileViewModel`).
  - Remplacement de la méthode *Extract and Override* par des versions mockées des services DI, rendant les tests plus robustes et maintenables.
- **Intégration Continue (CI/CD)** :
  - Modification du workflow GitHub Actions (`cicd.yml`) pour exécuter automatiquement les tests unitaires avant la génération de l'APK de production et le déploiement sur Firebase.
