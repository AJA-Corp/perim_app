# Rapport de Tests - Perim'App

Ce rapport résume l'architecture, la configuration et les résultats de la suite complète de tests pour l'application **Perim'App**.

---

## 1. Vue d'Ensemble
La suite de tests a été écrite en utilisant le framework **xUnit** et cible le projet `perimapp.Tests`. Elle contient **86 tests** (84 tests unitaires + 2 tests d'intégration) couvrant toutes les fonctionnalités critiques de l'application.

Taux de réussite actuel : **100% (86/86 tests réussis)**

---

## 2. Résultats des Tests par Composant

| Composant | Description | Nombre de Tests | Résultat |
| :--- | :--- | :---: | :---: |
| **Modèles** | Calcul de DLC restante, textes d'affichage et taille de police dynamique. | 14 | [x] Passé |
| **Convertisseurs** | Convertisseur de couleur dynamique `DlcColorConverter`. | 11 | [x] Passé |
| **Services Locaux** | Persistance JSON, gestion des favoris, incrémentation de compteurs. | 11 | [x] Passé |
| **Services API & Synchro** | AuthService, recherche de code-barres (OFF & API), SyncService. | 17 | [x] Passé |
| **Ordonnanceur de Notifications** | Programmation automatique des alarmes de péremption locales. | 1 | [x] Passé |
| **ViewModels** | Logique de présentation complète (avec injection de dépendances). | 30 | [x] Passé |
| **Intégration** | Flux de synchronisation mocké + intégration de bout en bout réelle avec les serveurs Render et Neon (avec nettoyage automatique). | 2 | [x] Passé |

---

## 3. Architecture des Tests
Pour éliminer les dépendances fortes aux plateformes mobiles (.NET MAUI) et aux popups graphiques, nous avons mis en place une architecture découplée :
1. **Services IHM abstraits** : L'accès à la navigation Shell, aux popups/alertes et au thread UI a été déplacé derrière des interfaces (`INavigationService`, `IDialogService`, `IDispatcherService`).
2. **Injection de Dépendances (DI)** : Les ViewModels reçoivent leurs services via leurs constructeurs, facilitant l'injection de Mocks au moment du test.
3. **Mocks/Stubs Configurables** : Les tests unitaires utilisent des implémentations de simulation (`MockNavigationService`, `MockDialogService`, etc.) pour vérifier les destinations de navigation, les retours utilisateur et s'assurer qu'aucun appel natif n'est effectué hors du contexte de l'application.
4. **Non-parallélisation xUnit** : Configuration du comportement de collection de tests pour éviter des collisions d'accès concurrent au système de fichiers et à la mémoire partagée (`AppData.CurrentUser`).

---

## 4. Tests d'Intégration
Deux tests d'intégration majeurs ont été introduits dans [IntegrationTests.cs](IntegrationTests.cs) :
* **Test d'Intégration Local (Mocké)** : Valide le cycle de transition offline/online et s'assure que les produits en attente de synchronisation (`PendingCreate`) passent correctement au statut synchronisé (`Synced`) dès que le réseau redevient disponible.
* **Test d'Intégration Réel (Bout en bout)** : Effectue une phase de réveil automatique (warm-up) du serveur Render (free tier), inscrit un utilisateur factice temporaire, valide la création automatique d'un code foyer, synchronise un produit local avec la base de données distante Neon, valide sa présence sur l'inventaire en ligne et supprime le compte pour préserver la propreté de la base de données.

---

## 5. Comment exécuter les tests localement
À partir du dossier racine du projet, lancez :
```bash
dotnet test perimapp.Tests/perimapp.Tests.csproj --logger:"console;verbosity=normal"
```
