# Rapport de Tests - Perim'App

Ce rapport résume l'architecture, la configuration et les résultats de la suite complète de tests unitaires pour l'application **Perim'App**.

---

## 1. Vue d'Ensemble
La suite de tests unitaires a été écrite en utilisant le framework **xUnit** et cible le projet principal `perimapp`. Elle contient **84 tests unitaires** couvrant toutes les fonctionnalités critiques de l'application (modèles, convertisseurs, services de données locaux, services d'API/synchronisation et ViewModels).

Taux de réussite actuel : **100% (84/84 tests réussis)**

---

## 2. Résultats des Tests par Composant

| Composant | Description | Nombre de Tests | Résultat |
| :--- | :--- | :---: | :---: |
| **Modèles** | Calcul de DLC restante, textes d'affichage et taille de police dynamique. | 14 | 🟢 Passé |
| **Convertisseurs** | Convertisseur de couleur dynamique `DlcColorConverter`. | 11 | 🟢 Passé |
| **Services Locaux** | Persistance JSON, gestion des favoris, incrémentation de compteurs. | 11 | 🟢 Passé |
| **Services API & Synchro** | AuthService, recherche de code-barres (OFF & API), SyncService (mode connecté/déconnecté). | 17 | 🟢 Passé |
| **Ordonnanceur de Notifications** | Programmation automatique des alarmes de péremption locales. | 1 | 🟢 Passé |
| **ViewModels** | Logique de présentation complète (Loading, Starting, Login, SignUp, EmailVerification, Main, Details, AddProduct, ModifyProduct, DeletedProduct, Scanner, Profile). | 30 | 🟢 Passé |

---

## 3. Architecture des Tests
Pour éliminer les dépendances fortes aux plateformes mobiles (.NET MAUI) et aux popups graphiques, nous avons mis en place une architecture découplée :
1. **Services IHM abstraits** : L'accès à la navigation Shell, aux popups/alertes et au thread UI a été déplacé derrière des interfaces (`INavigationService`, `IDialogService`, `IDispatcherService`).
2. **Injection de Dépendances (DI)** : Les ViewModels reçoivent leurs services via leurs constructeurs, facilitant l'injection de Mocks au moment du test.
3. **Mocks/Stubs Configurables** : Les tests unitaires utilisent des implémentations de simulation (`MockNavigationService`, `MockDialogService`, etc.) pour vérifier les destinations de navigation, les retours utilisateur et s'assurer qu'aucun appel natif n'est effectué hors du contexte de l'application.
4. **Non-parallélisation xUnit** : Configuration du comportement de collection de tests pour éviter des collisions d'accès concurrent au système de fichiers et à la mémoire partagée (`AppData.CurrentUser`).

---

## 4. Comment exécuter les tests localement
À partir du dossier racine du projet, lancez :
```bash
dotnet test perimapp.Tests/perimapp.Tests.csproj --logger:"console;verbosity=normal"
```
