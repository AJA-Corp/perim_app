# Spécifications Fonctionnelles - Perim'APP

**Version :** 1.0  
**Date :** Décembre 2024  
**Projet :** Application mobile de gestion des dates de péremption  

---

## 📋 Table des matières

1. [Vue d'ensemble du projet](#vue-densemble-du-projet)
2. [Objectifs et enjeux](#objectifs-et-enjeux)
3. [Personas utilisateurs](#personas-utilisateurs)
4. [Cas d'usage](#cas-dusage)
5. [Exigences fonctionnelles](#exigences-fonctionnelles)
6. [Interface utilisateur](#interface-utilisateur)
7. [Règles métier](#règles-métier)
8. [Roadmap et évolutions futures](#roadmap-et-évolutions-futures)

---

## 1. Vue d'ensemble du projet

### 1.1 Description générale

**Perim'APP** est une application mobile développée en .NET MAUI, conçue pour aider les utilisateurs à suivre et gérer les dates de péremption de leurs produits alimentaires. L'objectif principal est de **réduire le gaspillage alimentaire** en facilitant le suivi, l'organisation et la consommation à temps des produits.

### 1.2 Contexte et problématique

- **Problème identifié :** Le gaspillage alimentaire domestique dû à l'oubli des dates de péremption
- **Solution proposée :** Application mobile intuitive pour tracker et alerter sur les produits périssables
- **Public cible :** Particuliers, familles, personnes soucieuses de réduire leur gaspillage alimentaire

---

## 2. Objectifs et enjeux

### 2.1 Objectifs business

- **Primaire :** Réduire le gaspillage alimentaire domestique
- **Secondaires :**
  - Améliorer la gestion du stock alimentaire personnel
  - Sensibiliser aux dates de péremption
  - Faciliter l'organisation familiale autour de l'alimentation

### 2.2 Indicateurs de succès

- Nombre de produits ajoutés par utilisateur
- Taux de consommation avant expiration
- Engagement utilisateur (fréquence d'utilisation)
- Réduction du gaspillage auto-déclaré

---

## 3. Personas utilisateurs

### 3.1 Persona principal - "Marie, la famille organisée"

- **Profil :** Mère de famille, 35 ans, active
- **Besoins :** Gérer les courses et l'alimentation de sa famille
- **Frustrations :** Oubli des dates de péremption, gaspillage alimentaire
- **Objectifs :** Optimiser ses achats, réduire le gaspillage

### 3.2 Persona secondaire - "Tom, l'étudiant responsable"

- **Profil :** Étudiant, 22 ans, budget limité
- **Besoins :** Maximiser ses achats alimentaires, éviter le gaspillage
- **Frustrations :** Manque d'organisation, produits oubliés qui périment
- **Objectifs :** Économiser de l'argent, être plus responsable

---

## 4. Cas d'usage

### 4.1 Cas d'usage principal : Gestion des produits

#### UC01 - Ajouter un produit
- **Acteur :** Utilisateur
- **Déclencheur :** Achat d'un nouveau produit
- **Préconditions :** Utilisateur connecté
- **Scénario principal :**
  1. L'utilisateur ouvre l'application
  2. Il sélectionne "Ajouter un produit"
  3. Il saisit ou scanne le code-barres
  4. Les informations du produit se remplissent automatiquement
  5. Il saisit la date de péremption
  6. Il confirme l'ajout
- **Postconditions :** Produit ajouté à la liste utilisateur

#### UC02 - Consulter ses produits
- **Acteur :** Utilisateur
- **Déclencheur :** Vouloir voir l'état de ses produits
- **Scénario principal :**
  1. L'utilisateur ouvre l'application
  2. Il accède à la page principale
  3. Il voit la liste de ses produits avec les jours restants
  4. Il peut filtrer par catégorie ou date
- **Postconditions :** Vue d'ensemble des produits

### 4.2 Cas d'usage secondaires

#### UC03 - Recevoir des notifications
- **Acteur :** Système
- **Déclencheur :** Produit proche de la péremption
- **Scénario :** Le système envoie une notification push

#### UC04 - Gérer le profil famille
- **Acteur :** Utilisateur
- **Déclencheur :** Partage familial
- **Scénario :** Configuration du code famille pour partager les produits

---

## 5. Exigences fonctionnelles

### 5.1 Gestion des utilisateurs

| ID | Exigence | Priorité | Statut |
|----|----------|----------|--------|
| EF01 | Inscription avec email/mot de passe | Haute | ✅ Implémenté |
| EF02 | Connexion utilisateur | Haute | ✅ Implémenté |
| EF03 | Gestion du profil utilisateur | Moyenne | ✅ Implémenté |
| EF04 | Mode famille avec code de partage | Moyenne | 🔄 En cours |

### 5.2 Gestion des produits

| ID | Exigence | Priorité | Statut |
|----|----------|----------|--------|
| EF05 | Ajout manuel de produits | Haute | ✅ Implémenté |
| EF06 | Intégration OpenFoodFacts pour les données produits | Haute | ✅ Implémenté |
| EF07 | Calcul automatique des jours restants | Haute | ✅ Implémenté |
| EF08 | Modification des informations produit | Moyenne | ✅ Implémenté |
| EF09 | Suppression de produits | Moyenne | ✅ Implémenté |
| EF10 | Historique des produits supprimés | Basse | ✅ Implémenté |

### 5.3 Fonctionnalités avancées

| ID | Exigence | Priorité | Statut |
|----|----------|----------|--------|
| EF11 | Scanner de code-barres | Haute | 📋 Planifié |
| EF12 | Notifications push pour les produits proches expiration | Haute | 📋 Planifié |
| EF13 | Filtrage par catégorie/date | Moyenne | 🔄 En cours |
| EF14 | Reconnaissance OCR des dates | Basse | 📋 Planifié |
| EF15 | Gestion des quantités | Moyenne | ✅ Implémenté |

---

## 6. Interface utilisateur

### 6.1 Architecture de navigation

```
StartingPage (Accueil)
├── LogInPage (Connexion)
├── SignUpPage (Inscription)
└── MainPage (Page principale)
    ├── AddProductPage (Ajout produit)
    ├── DetailsPage (Détails produit)
    ├── ModifyProductPage (Modification)
    ├── ProfilePage (Profil)
    └── DeletedProductPage (Produits supprimés)
```

### 6.2 Spécifications des écrans

#### Écran principal (MainPage)
- **Objectif :** Vue d'ensemble des produits de l'utilisateur
- **Éléments :**
  - Liste des produits avec image, nom, catégorie
  - Indicateur visuel des jours restants (couleur selon urgence)
  - Bouton d'ajout de produit
  - Options de filtrage
- **Interactions :**
  - Tap sur produit → Détails
  - Bouton + → Ajout produit
  - Filtres → Application des critères

#### Écran d'ajout (AddProductPage)
- **Objectif :** Ajouter un nouveau produit
- **Éléments :**
  - Champ code-barres (avec bouton scan futur)
  - Champs auto-remplis : nom, image, catégorie
  - Sélecteur de date de péremption
  - Champ quantité
  - Bouton validation
- **Interactions :**
  - Saisie code-barres → Récupération données OpenFoodFacts
  - Sélection date → Calcul automatique jours restants
  - Validation → Retour à MainPage

### 6.3 Design et ergonomie

- **Style :** Interface moderne et colorée avec indicateurs visuels clairs
- **Couleurs :** 
  - Vert (#58BF7F) - Couleur principale
  - Rouge - Produits expirés
  - Orange - Produits proches expiration
  - Vert - Produits avec du temps restant
- **Accessibilité :** Tailles de police adaptatives, contrastes suffisants

---

## 7. Règles métier

### 7.1 Gestion des dates

- **Calcul des jours restants :** `(Date de péremption - Date actuelle).jours`
- **Affichage jours restants :**
  - "Exp." pour les produits expirés (< 0 jours)
  - "Auj." pour les produits qui expirent aujourd'hui (0 jour)
  - "1j", "2j", etc. pour les produits avec du temps restant
  - "1 jour", "2 jours", etc. sur la page détails

### 7.2 Notifications

- **Seuils d'alerte :**
  - 3 jours avant expiration : notification d'avertissement
  - 1 jour avant expiration : notification urgente
  - Jour d'expiration : notification critique

### 7.3 Mode famille

- **Code de partage :** Code numérique unique par foyer
- **Synchronisation :** Tous les membres voient les mêmes produits
- **Gestion :** Chaque membre peut ajouter/modifier/supprimer

---

## 8. Roadmap et évolutions futures

### 8.1 Version actuelle (v1.0)
- [x] Affichage de produits (localement)
- [x] Gestion utilisateurs (inscription/connexion)
- [x] Intégration OpenFoodFacts
- [x] Interface MAUI multiplateforme

### 8.2 Prochaines versions

#### Version 1.1 (Q1 2025)
- [ ] Scanner de code-barres intégré
- [ ] Système de notifications push
- [ ] Filtrage avancé

#### Version 1.2 (Q2 2025)
- [ ] Mode famille complet
- [ ] Statistiques de consommation
- [ ] Export des données

#### Version 2.0 (Q3 2025)
- [ ] Reconnaissance OCR des dates
- [ ] Suggestions de recettes
- [ ] Intégration avec les courses en ligne

### 8.3 Améliorations techniques

- Performance et optimisation
- Tests automatisés
- Intégration continue
- Monitoring et analytics

---

## 📞 Contact et validation

**Équipe projet :** AJA-Corp  
**Validation :** Ce document doit être validé par les parties prenantes avant développement  
**Mise à jour :** Document vivant, mis à jour selon l'évolution du projet