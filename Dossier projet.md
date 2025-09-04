# Dossier projet

## Version: 1.0

## Date: Septembre 2025

## Projet: Perim’App

[**Introduction	2**](#introduction)

[Version Française	2](#version-française)

[English Version	3](#english-version)

[**Liste des compétences du référentiel	4**](#liste-des-compétences-du-référentiel)

[1\. RNCP37873BC01 \- Développer une application sécurisée	4](#1.-rncp37873bc01---développer-une-application-sécurisée)

[2\. RNCP37873BC02 \- Concevoir et développer une application sécurisée organisée en couches	4](#2.-rncp37873bc02---concevoir-et-développer-une-application-sécurisée-organisée-en-couches)

[3\. RNCP37873BC03 \- Préparer le déploiement d’une application sécurisée	4](#3.-rncp37873bc03---préparer-le-déploiement-d’une-application-sécurisée)

[**Gestion projet	5**](#gestion-projet)

[**Spécifications Fonctionnelles	7**](#spécifications-fonctionnelles)

[**Specifications Techniques	10**](#specifications-techniques)

[**Maquettes	11**](#maquettes)

[**MCD \- MLD	12**](#mcd---mld)

[Modèle Conceptuel de Données	12](#modèle-conceptuel-de-données)

[Modèle Logique de Données	12](#modèle-logique-de-données)

[**Code	13**](#code)

[Architecture	13](#architecture)

[Front-end	13](#front-end)

[Back-end	13](#back-end)

[Tests	13](#tests)

[**Sécurité	14**](#sécurité)

[**CI/CD	15**](#ci/cd)

[**Evolutions	16**](#evolutions)

[**Conclusions	17**](#conclusions)

# 

# Introduction {#introduction}

## Version Française {#version-française}

*Vous en avez marre de jeter vos aliments périmés ?*  
*Perim'App est faite pour vous.*  
*Scannez vos articles à la maison, entrez leur date de péremption et recevez une notification afin de consommer votre produit avant qu’il ne soit périmé.*

Arrivés en octobre 2023, nous avons réalisé notre formation chez **ADA TECH SCHOOL** pendant une durée de 9 mois avant de démarrer nos alternances en septembre et novembre 2024\. Mais c'est en mai 2024 qu'émerge l'idée d'une application qui permettrait de réduire le gaspillage. C'est là qu'est né **PerimApp**.

Perim’App est une application mobile qui permet d’enregistrer ses produits périssables industriels et d’alerter à l’approche de la date de péremption.  
L’application s’adresse à l'entièreté des habitants d’un foyer afin d'inciter chacun à avoir une consommation responsable.

D’après le ministère de l’agriculture et de la souveraineté alimentaire, cette année-là (2020) en France, le gaspillage alimentaire s’élevait à plus de **8,7 millions de tonnes**. En effet, en octobre 2023, le ministère a publié les chiffres de l’année 2021\. Résultat ? Le gaspillage alimentaire est resté au même niveau qu’en 2020\.  
**8,8 millions de tonnes de déchets alimentaires** ont été produits en France, soit **129 kg par personne**, 47% des déchets proviennent des ménages. Le gaspillage alimentaire représente près de **4,3 millions de tonnes** de déchets issus des parties comestibles des aliments (aliments non consommés encore emballés, restes de repas, etc.).

Nos objectifs:   
\- Alerter nos utilisateurs lorsque le produit arrive à date de péremption  
\- Réduire le gaspillage alimentaire  
\- Encourager une consommation \+ responsable

## 

## English Version {#english-version}

*Are you tired of throwing away expired food?*  
*Perim'App is for you.*  
*Scan your items at home, enter their expiration date, and receive a notification so you can use them before they expire.*

Arriving in October 2023, we completed our training at **ADA TECH SCHOOL** for a period of 9 months before starting our work-study programs in September and November 2024\. However, it was in May 2024 that the idea for an application that would reduce waste emerged. This is when **PerimApp** was born.

Perim’App is a mobile application that allows you to record your perishable industrial products and alert you when their expiration date is approaching.

The application is aimed at all household members to encourage everyone to consume responsibly.

According to the Ministry of Agriculture and Food Sovereignty, that year (2020) in France, food waste amounted to more than **8.7 million tons**. Indeed, in October 2023, the Ministry published the figures for 2021\. The result? Food waste remained at the same level as in 2020\.  
**8.8 million tons of food waste** were produced in France, or **129 kg per person**; 47% of this waste comes from households. Food waste represents nearly **4.3 million tons** of waste from the edible parts of food (unconsumed food still packaged, leftovers, etc.).

Our objectives:  
\- Alert our users when a product is approaching its expiration date  
\- Reduce food waste  
\- Encourage more responsible consumption

# 

# Liste des compétences du référentiel {#liste-des-compétences-du-référentiel}

## 1\. RNCP37873BC01 \- Développer une application sécurisée {#1.-rncp37873bc01---développer-une-application-sécurisée}

- Installer et configurer son environnement de travail en fonction du projet  
- Développer des interfaces utilisateurs  
- Développer des composants métier  
- Contribuer à la gestion d’un projet informatique

## 2\. RNCP37873BC02 \- Concevoir et développer une application sécurisée organisée en couches {#2.-rncp37873bc02---concevoir-et-développer-une-application-sécurisée-organisée-en-couches}

- Analyser les besoins et maquetter une application  
- Définir l’architecture logicielle d’une application  
- Concevoir et mettre en place une base de données relationnelle  
- Développer des composants d’accès aux données SQL et NoSQL

## 3\. RNCP37873BC03 \- Préparer le déploiement d’une application sécurisée {#3.-rncp37873bc03---préparer-le-déploiement-d’une-application-sécurisée}

- Préparer et exécuter les plans de test d’une application  
- Préparer et documenter le déploiement d’une application  
- Contribuer à la mise en production dans une démarche DevOps

# 

# Gestion projet {#gestion-projet}

La gestion de projet repose sur l’utilisation d’outils collaboratifs qui permettent d’organiser, de planifier et de suivre l’évolution du développement de l’application.

Jira :  
Jira est notre plateforme principale de gestion de projet. Elle permet de découper le travail en plusieurs niveaux :

- Les epics regroupent les grandes fonctionnalités de l’application.

- Chaque epic est découpé en stories ou tickets, qui représentent des tâches plus petites et concrètes.  
- Grâce à ce système, chaque membre de l’équipe peut se voir attribuer des tâches précises et suivre leur avancement. Jira nous permet aussi :  
- De générer automatiquement les commandes Git nécessaires pour créer des branches correspondant aux tickets (par exemple feature/DEV-65 ou fixes/BUG-7).  
- De répertorier et suivre les bugs identifiés pendant le développement.  
- De visualiser l’avancement global grâce à des tableaux Kanban ou Scrum.

Figma :  
Figma est notre outil de maquettage et de prototypage. Il nous permet de concevoir l’interface utilisateur avant même de commencer le développement. L’outil offre un espace collaboratif où chaque membre peut visualiser et commenter les maquettes en temps réel. Cela facilite la communication entre les développeurs et les designers et assure une cohérence visuelle dans l’application.

JetBrains Rider :  
Rider est l’environnement de développement intégré (IDE) que nous utilisons. Il est spécialement adapté aux projets .NET et MAUI, et dispose d’outils puissants pour :

- La complétion automatique du code.  
- L’intégration avec Git.  
- Le débogage avancé.  
- La gestion des dépendances.

Sa configuration personnalisée selon notre projet permet d’optimiser la productivité des développeurs et de réduire les erreurs.

NeonDB :  
Neon est notre plateforme d’hébergement de base de données PostgreSQL. En plus de stocker les données de manière sécurisée et scalable, elle offre un éditeur SQL intégré pour exécuter des requêtes directement en ligne, sans nécessiter d’outils supplémentaires. Cela simplifie grandement la gestion et l’analyse des données lors du développement et des tests.

Git et GitHub :  
Pour la gestion du code source, nous utilisons Git associé à GitHub :

- Git est notre système de contrôle de version. Il permet de travailler à plusieurs sur le même projet sans écraser le travail des autres, grâce à la création de branches.

- GitHub est notre plateforme de collaboration. Elle nous permet d’héberger le code, de créer des pull requests pour valider les modifications, d’intégrer des actions GitHub (CI/CD) pour automatiser les tests et les déploiements, et de centraliser toute la documentation du projet.

L’intégration de Git/GitHub avec Jira renforce encore cette organisation, puisque chaque commit et chaque branche peuvent être associés directement à un ticket ou une story Jira.

# Spécifications Fonctionnelles {#spécifications-fonctionnelles}

## Expression des besoins

### Contexte

D'après le ministère de l'agriculture et de la souveraineté alimentaire, en France, le gaspillage alimentaire s'élevait en 2021 à **8,8 millions de tonnes**, soit **129 kg par personne**. 47% de ces déchets proviennent des ménages, représentant près de **4,3 millions de tonnes** de produits encore comestibles.

### Persona principal

**Marie, 32 ans, mère de famille active**
- Vit en famille avec 2 enfants
- Travaille à temps plein
- Fait ses courses 1-2 fois par semaine
- Préoccupée par le gaspillage alimentaire
- Utilise son smartphone quotidiennement
- Besoin de rapidité et simplicité dans ses outils

*"J'aimerais un moyen simple de suivre les dates de péremption de nos produits pour éviter de jeter de la nourriture. Il me faut quelque chose de rapide à utiliser entre mes courses et mon travail."*

### Objectifs de l'application

1. **Alerter** les utilisateurs lorsque le produit arrive à date de péremption
2. **Réduire** le gaspillage alimentaire  
3. **Encourager** une consommation plus responsable

### Exigences fonctionnelles

#### F1 - Gestion des utilisateurs
- **F1.1** : L'utilisateur peut créer un compte avec email/mot de passe
- **F1.2** : L'utilisateur peut se connecter/déconnecter
- **F1.3** : L'utilisateur peut modifier ses informations de profil

#### F2 - Gestion des produits
- **F2.1** : L'utilisateur peut ajouter un produit manuellement
- **F2.2** : L'utilisateur peut scanner un code-barres pour ajouter un produit
- **F2.3** : L'utilisateur peut modifier les informations d'un produit
- **F2.4** : L'utilisateur peut supprimer un produit
- **F2.5** : L'utilisateur peut visualiser la liste de ses produits
- **F2.6** : L'utilisateur peut trier/filtrer ses produits par date de péremption

#### F3 - Notifications et alertes
- **F3.1** : L'application alerte l'utilisateur des produits proches de l'expiration
- **F3.2** : L'utilisateur peut configurer ses préférences de notification
- **F3.3** : L'application affiche visuellement l'urgence (codes couleur)

#### F4 - Base de données produits
- **F4.1** : L'application récupère automatiquement les informations produit via code-barres
- **F4.2** : L'application stocke les données utilisateur de manière sécurisée

### Exigences non-fonctionnelles

#### Performance
- Temps de réponse < 2 secondes pour l'affichage de la liste
- Synchronisation en arrière-plan

#### Compatibilité
- Compatible Android (API 21+) et iOS (13+)
- Interface adaptée aux écrans mobiles
- Support hors ligne pour la consultation

#### Sécurité
- Chiffrement des mots de passe (Argon2)
- Communication sécurisée HTTPS
- Stockage local sécurisé

#### Ergonomie
- Interface intuitive et accessible
- Design cohérent suivant les guidelines Material Design / Human Interface
- Support multi-langues (français principalement)

## Cas d'utilisation

![Diagramme des cas d'utilisation](Images/Use%20Cases.png)

### UC1 - Ajouter un produit
**Acteur** : Utilisateur  
**Précondition** : Utilisateur connecté  
**Déclencheur** : L'utilisateur souhaite ajouter un nouveau produit  
**Scenario nominal** :
1. L'utilisateur accède à l'écran d'ajout
2. L'utilisateur saisit ou scanne le code-barres
3. L'application récupère les informations produit
4. L'utilisateur confirme et ajoute la date de péremption
5. Le produit est ajouté à la liste

### UC2 - Consulter les alertes
**Acteur** : Utilisateur  
**Précondition** : Utilisateur connecté, produits en base  
**Déclencheur** : L'utilisateur consulte sa liste de produits  
**Scenario nominal** :
1. L'utilisateur ouvre l'application
2. La liste des produits s'affiche avec indicateurs visuels
3. Les produits proches de l'expiration sont mis en évidence
4. L'utilisateur peut agir sur les produits alertés

## Modélisation UML

![Diagramme d'activité](Images/Activity%20Diagram.png)

Le diagramme d'activité présente le processus principal de l'application, de la connexion à la gestion des produits.

![Diagramme de classes](Images/Core%20Class%20Diagram.png)

Le diagramme de classes illustre la structure des données principales : User, Product, et leurs relations.

## Risques et difficultés identifiés

### Risques techniques
- **Tests non intégrés** : Risque de régressions non détectées
- **Manque de temps** : Planning serré avec contraintes d'alternance
- **Synchronisation données** : Gestion de la cohérence entre local/distant

### Difficultés organisationnelles
- **Rythme d'alternance** : Alternance 4j entreprise / 1j école complexifie la continuité
- **Équipe distribuée** : Alternances différées (septembre/novembre) 
- **Coordination** : Synchronisation entre les différents rythmes de travail

### Mesures d'atténuation
- Planning adaptatif avec jalons flexibles
- Documentation technique complète
- Outils de collaboration (GitHub, Jira)
- Communication régulière via Discord/Teams

# Specifications Techniques {#specifications-techniques}

## Architecture globale

![Diagramme d'architecture](Images/Architecture%20Diagram.png)

L'application Perim'App suit une architecture moderne basée sur .NET MAUI pour le développement multiplateforme.

### Stack technologique

#### Frontend - Application mobile
- **Framework** : .NET MAUI 9.0
- **Langage** : C# 12
- **UI** : XAML avec binding MVVM
- **Patterns** : MVVM Community Toolkit
- **Plateformes cibles** : Android (API 21+), iOS (13+), Windows

#### Backend - Base de données et services
- **Base de données** : PostgreSQL (hébergée sur NeonDB)
- **ORM** : Npgsql pour l'accès direct PostgreSQL
- **API externe** : OpenFoodFacts pour les informations produits
- **Authentification** : Locale avec hachage Argon2

#### Packages NuGet utilisés
```xml
<PackageReference Include="CommunityToolkit.Maui" Version="11.2.0" />
<PackageReference Include="Isopoh.Cryptography.Argon2" Version="2.0.0" />
<PackageReference Include="Microsoft.Maui.Controls" Version="9.0.50" />
<PackageReference Include="Npgsql" Version="9.0.3" />
```

### Modèle de données

![Modèle Conceptuel de Données](Images/MCD_PerimAPP.png)

#### Tables principales

**Users**
- user_id (PK)
- email
- password_hash
- first_name
- last_name
- created_at

**Products**
- product_id (PK)
- user_id (FK)
- product_name
- product_barcode
- product_category
- product_quantity
- product_dlc (date limite consommation)
- url_image
- added_at

### Services et couches

#### Couche Données (Data Layer)
```csharp
// AppData.cs - Gestionnaire de données global
public static class AppData
{
    public static ObservableCollection<ProductInfos> CurrentProducts { get; set; }
    public static UserProfile? CurrentUser { get; set; }
}
```

#### Couche Services (Service Layer)

**NeonProductService** : Gestion des produits avec PostgreSQL
```csharp
public class NeonProductService
{
    public async Task<List<ProductInfos>> GetProductsAsync(int userId)
    public async Task AddProductAsync(ProductInfos product)
    public async Task UpdateProductAsync(ProductInfos product)
    public async Task DeleteProductAsync(int productId)
}
```

**NeonUserService** : Gestion des utilisateurs
```csharp
public class NeonUserService
{
    public async Task<bool> RegisterUserAsync(UserProfile user)
    public async Task<UserProfile?> AuthenticateUserAsync(string email, string password)
}
```

**OpenFoodFactsService** : Intégration API externe
```csharp
public class OpenFoodFactsService
{
    public async Task<ProductInfos?> GetProductByBarcodeAsync(string barcode)
}
```

**PasswordHasher** : Sécurisation des mots de passe
```csharp
public static class PasswordHasher
{
    public static string HashPassword(string password)
    public static bool VerifyPassword(string password, string hash)
}
```

#### Couche Présentation (UI Layer)

Pages principales :
- **StartingPage** : Écran d'accueil
- **LogInPage** : Authentification
- **SignUpPage** : Inscription
- **MainPage** : Liste des produits
- **AddProductPage** : Ajout de produit
- **DetailsPage** : Détails d'un produit
- **ModifyProductPage** : Modification
- **ProfilePage** : Profil utilisateur

### Configuration technique

#### Plateformes supportées
```xml
<TargetFrameworks>net9.0-android</TargetFrameworks>
<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">
  $(TargetFrameworks);net9.0-windows10.0.19041.0
</TargetFrameworks>
```

#### Versions minimales
- **Android** : API 21 (Android 5.0)
- **Windows** : 10.0.17763.0
- **iOS** : Compatible avec les versions supportées par .NET MAUI

### Connexion base de données

Configuration PostgreSQL via NeonDB :
```csharp
private const string ConnectionString = "Host=ep-restless-field-a8gc8nce.eastus2.aws.neon.tech;" +
                                       "Database=perimapp;" +
                                       "Username=perimapp_owner;" +
                                       "Password=[PROTECTED];" +
                                       "SSL Mode=Require;";
```

### Performance et optimisation

#### Stratégies de performance
- **Lazy Loading** : Chargement des données à la demande
- **Caching local** : Stockage temporaire pour réduire les appels réseau
- **Async/Await** : Opérations asynchrones pour la fluidité UI
- **ObservableCollection** : Binding automatique pour les mises à jour UI

#### Gestion hors ligne
- Stockage local des données critiques
- Synchronisation différée lors de la reconnexion
- Gestion des conflits de données

# Maquettes {#maquettes}

L’entièreté du maquettage à été réalisé avec Figma  
La couleur principale de l’application est le vert (\#58BF7F):  
![\#58BF7F][image1]

## Écrans de l'application

### Écran d'accueil et d'authentification

![Écran d'accueil](Images/Maquettes/Accueil.png)

L'écran d'accueil présente l'application avec un design épuré utilisant la charte graphique verte.

![Page de connexion](Images/Maquettes/LogIn.png)

La page de connexion permet aux utilisateurs existants de s'authentifier avec leur email et mot de passe.

![Page d'inscription](Images/Maquettes/SignUp.png)

La page d'inscription permet aux nouveaux utilisateurs de créer un compte avec validation des informations.

### Interface principale

![Page principale](Images/Maquettes/Main.png)

La page principale affiche la liste des produits avec leur date d'expiration, permettant un tri par date de péremption.

![Page principale après ajout](Images/Maquettes/MainAfterAdd.png)

Vue de la page principale après l'ajout d'un nouveau produit, montrant la mise à jour en temps réel de la liste.

![Page principale après modification](Images/Maquettes/MainAfterModify.png)

Vue de la page principale après modification d'un produit existant.

### Gestion des produits

![Ajout de produit](Images/Maquettes/AddProduct.png)

Interface d'ajout d'un nouveau produit avec saisie du nom, code-barres et date de péremption.

![Détails du produit](Images/Maquettes/DetailsProduct.png)

Page de détails d'un produit affichant toutes les informations disponibles (nom, catégorie, date de péremption, etc.).

![Détails après modification](Images/Maquettes/DetailsAfterModify.png)

Vue des détails du produit après modification des informations.

![Modification de produit](Images/Maquettes/ModifyProduct.png)

Interface de modification permettant d'éditer les informations d'un produit existant.

### Fonctionnalités avancées

![Notifications](Images/Maquettes/Notifications.png)

Système de notifications pour alerter les utilisateurs des produits arrivant à expiration.

![Profil utilisateur](Images/Maquettes/Profile.png)

Page de profil utilisateur permettant de gérer les informations personnelles et les préférences.

![Interface PomPotes](Images/Maquettes/PomPotes.png)

Fonctionnalité collaborative permettant le partage de produits entre utilisateurs (fonctionnalité future).

## Cohérence du design

Toutes les maquettes respectent une charte graphique cohérente avec :
- Utilisation du vert \#58BF7F comme couleur principale
- Typographie lisible et moderne
- Interface intuitive avec navigation claire
- Design responsive adapté aux écrans mobiles
- Iconographie cohérente pour les actions (ajout, modification, suppression)

# MCD \- MLD {#mcd---mld}

## Modèle Conceptuel de Données {#modèle-conceptuel-de-données}

## 

## 

## Modèle Logique de Données {#modèle-logique-de-données}

## ![][image2]

# Code {#code}

# Sécurité {#sécurité}

## Authentification et autorisation

### Hachage des mots de passe
L'application utilise **Argon2id** pour le hachage sécurisé des mots de passe, considéré comme l'état de l'art en matière de protection des mots de passe.

```csharp
public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        var config = new Argon2Config
        {
            Type = Argon2Type.Argon2id,    // Résistant aux attaques GPU et side-channel
            Version = Argon2Version.Nineteen,
            TimeCost = 10,                  // 10 itérations
            MemoryCost = 32768,            // 32 MB de mémoire
            Lanes = 4,                     // 4 threads parallèles
            Threads = Environment.ProcessorCount,
            Password = Encoding.UTF8.GetBytes(password),
            Salt = GenerateSalt(),         // Salt aléatoire unique
            HashLength = 20                // Hash de 20 bytes
        };
        
        var argon2A = new Argon2(config);
        using (argon2A)
        {
            return argon2A.Hash().Encoded;
        }
    }
}
```

### Génération de sel cryptographique
Chaque mot de passe utilise un sel unique généré de manière cryptographiquement sécurisée :

```csharp
private static byte[] GenerateSalt()
{
    var buffer = new byte[16];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(buffer);
    }
    return buffer;
}
```

## Protection des données

### Chiffrement des communications
- **HTTPS obligatoire** : Toutes les communications entre l'application et la base de données PostgreSQL utilisent SSL/TLS
- **Certificats valides** : Vérification des certificats SSL pour éviter les attaques man-in-the-middle

```csharp
private const string ConnectionString = 
    "Host=ep-restless-field-a8gc8nce.eastus2.aws.neon.tech;" +
    "Database=perimapp;" +
    "Username=perimapp_owner;" +
    "Password=[PROTECTED];" +
    "SSL Mode=Require;";  // SSL obligatoire
```

### Conformité RGPD
Conformément au RGPD, l'application met en place plusieurs mesures :

**Minimisation des données** :
- Collecte uniquement l'email et le mot de passe hashé
- Les produits stockés ne contiennent pas de données personnelles sensibles
- Pas de tracking ou de cookies non essentiels

**Droits des utilisateurs** :
- Droit d'accès : Consultation des données via l'interface utilisateur
- Droit de rectification : Modification des informations produits
- Droit à l'effacement : Suppression de compte et données associées

**Sécurité des données** :
- Mots de passe jamais stockés en clair
- Base de données hébergée en Europe (AWS eu-east-1)
- Chiffrement en transit et au repos

## Sécurité applicative

### Validation des entrées
```csharp
// Validation des emails
public static bool IsValidEmail(string email)
{
    var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    return emailRegex.IsMatch(email);
}

// Protection contre les injections SQL avec paramètres
using var command = new NpgsqlCommand(query, connection);
command.Parameters.AddWithValue("@userId", userId);
command.Parameters.AddWithValue("@productName", productName);
```

### Gestion des erreurs sécurisée
- Messages d'erreur génériques pour éviter la fuite d'informations
- Logs détaillés côté serveur, messages simples côté client
- Pas d'exposition de la stack trace en production

### Protection contre les attaques communes

**Injection SQL** :
- Utilisation exclusive de requêtes paramétrées
- Aucune concaténation de chaînes dans les requêtes SQL

**Cross-Site Scripting (XSS)** :
- Encodage automatique des données en XAML
- Validation des entrées utilisateur

**Attaques par force brute** :
- Complexité minimale des mots de passe (implémentée côté client)
- Possibilité d'ajouter un système de limitation de tentatives

## Sécurité mobile

### Stockage local sécurisé
```csharp
// Utilisation du stockage sécurisé MAUI pour les tokens sensibles
await SecureStorage.SetAsync("auth_token", authToken);
var token = await SecureStorage.GetAsync("auth_token");
```

### Permissions minimales
L'application demande uniquement les permissions nécessaires :
- **Accès réseau** : Pour la synchronisation des données
- **Stockage local** : Pour le cache des données utilisateur
- **Appareil photo** (futur) : Pour le scan de codes-barres

### Obfuscation du code
En production, le code peut être obfusqué pour compliquer la rétro-ingénierie :
```xml
<!-- Configuration de protection du code -->
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <DebugType>none</DebugType>
    <DebugSymbols>false</DebugSymbols>
</PropertyGroup>
```

## Audit et monitoring

### Journalisation sécurisée
```csharp
// Logs d'audit sans données sensibles
_logger.LogInformation("User {UserId} attempted login at {Timestamp}", 
    userId, DateTime.UtcNow);

// Pas de log des mots de passe ou données personnelles
_logger.LogWarning("Failed login attempt for user {UserId}", userId);
```

### Détection d'anomalies
- Monitoring des tentatives de connexion échouées
- Alertes en cas d'activité suspecte
- Logs centralisés pour analyse forensique

### Tests de sécurité
**Tests automatisés** :
- Validation des hashes de mots de passe
- Tests d'injection SQL
- Vérification des permissions

**Tests manuels périodiques** :
- Audit de sécurité du code
- Tests de pénétration des API
- Revue des configurations de sécurité

# CI/CD {#ci/cd}

## Stratégie DevOps

L'intégration continue et le déploiement continu (CI/CD) sont mis en place pour automatiser les processus de développement, test et déploiement de l'application Perim'App.

## Gestion du code source

### Git et GitHub
```bash
# Structure des branches
main/                 # Branche principale (production)
├── develop/         # Branche de développement
├── feature/DEV-*    # Branches de fonctionnalités (intégration Jira)
└── hotfix/BUG-*     # Branches de correction urgente
```

### Convention de nommage des commits
```
feat: ajout de la fonctionnalité de scan de code-barres
fix: correction du calcul des jours restants
docs: mise à jour de la documentation technique
style: amélioration de l'interface utilisateur
refactor: restructuration du service de données
test: ajout de tests unitaires pour ProductService
```

### Intégration Jira-GitHub
- **Branches automatiques** : Création automatique depuis les tickets Jira
- **Liaison commits-tickets** : Référencement automatique des issues
- **Suivi de progression** : Mise à jour du statut des tickets via les commits

## Pipeline CI/CD

### Workflow GitHub Actions

**.github/workflows/ci.yml**
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build application
      run: dotnet build --no-restore --configuration Release
    
    - name: Run unit tests
      run: dotnet test --no-build --configuration Release --logger trx
    
    - name: Publish test results
      uses: dorny/test-reporter@v1
      if: success() || failure()
      with:
        name: Test Results
        path: '**/*.trx'
        reporter: dotnet-trx

  security-scan:
    runs-on: ubuntu-latest
    needs: build
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Run security scan
      uses: securecodewarrior/github-action-add-sarif@v1
      with:
        sarif-file: 'security-results.sarif'
    
    - name: Upload results to GitHub Security
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: 'security-results.sarif'

  android-build:
    runs-on: ubuntu-latest
    needs: [build, security-scan]
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET MAUI
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Install MAUI workload
      run: dotnet workload install maui
    
    - name: Build Android APK
      run: |
        dotnet publish -f net9.0-android \
          -c Release \
          -p:AndroidSdkDirectory=$ANDROID_SDK_ROOT \
          -o ./artifacts/android
    
    - name: Upload Android artifact
      uses: actions/upload-artifact@v3
      with:
        name: android-apk
        path: ./artifacts/android/*.apk

  deploy-staging:
    runs-on: ubuntu-latest
    needs: [android-build]
    if: github.ref == 'refs/heads/develop'
    environment: staging
    
    steps:
    - name: Deploy to staging
      run: |
        echo "Deploying to staging environment"
        # Commands de déploiement staging
    
    - name: Run integration tests
      run: |
        echo "Running integration tests"
        # Tests d'intégration automatisés

  deploy-production:
    runs-on: ubuntu-latest
    needs: [android-build]
    if: github.ref == 'refs/heads/main'
    environment: production
    
    steps:
    - name: Deploy to production
      run: |
        echo "Deploying to production"
        # Commands de déploiement production
```

## Environnements

### Développement
- **Local** : Environnement de développement individuel
- **Base de données** : PostgreSQL locale ou conteneur Docker
- **Configuration** : `appsettings.Development.json`

### Staging (Pré-production)
- **Environnement** : Serveur de test dédié
- **Base de données** : Instance PostgreSQL de test sur NeonDB
- **Tests automatisés** : Exécution de la suite de tests d'intégration
- **Validation** : Tests utilisateurs et validation métier

### Production
- **Environnement** : Serveur de production sécurisé
- **Base de données** : PostgreSQL production sur NeonDB avec sauvegardes
- **Monitoring** : Surveillance en temps réel des performances
- **Rollback** : Capacité de retour à la version précédente

## Tests automatisés

### Tests unitaires
```csharp
[TestClass]
public class ProductServiceTests
{
    [TestMethod]
    public async Task GetProductsAsync_ShouldReturnUserProducts()
    {
        // Arrange
        var service = new NeonProductService();
        var userId = 1;
        
        // Act
        var result = await service.GetProductsAsync(userId);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.All(p => p.UserId == userId));
    }
}
```

### Tests d'intégration
```csharp
[TestClass]
public class DatabaseIntegrationTests
{
    [TestMethod]
    public async Task DatabaseConnection_ShouldSucceed()
    {
        // Test de connexion à la base de données
        using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        
        Assert.AreEqual(ConnectionState.Open, connection.State);
    }
}
```

### Tests end-to-end
```csharp
[TestClass]
public class UserJourneyTests
{
    [TestMethod]
    public async Task CompleteUserJourney_ShouldSucceed()
    {
        // Test du parcours utilisateur complet :
        // 1. Inscription
        // 2. Connexion
        // 3. Ajout de produit
        // 4. Consultation de la liste
        // 5. Modification du produit
        // 6. Suppression
    }
}
```

## Qualité du code

### Analyse statique
```yaml
# Configuration SonarQube
sonar:
  projectKey: "perim-app"
  sources: "."
  exclusions: "**/bin/**,**/obj/**"
  coverage.exclusions: "**/Models/**,**/Data/**"
  cs.coverage.reportPaths: "coverage.xml"
```

### Métriques de qualité
- **Couverture de tests** : > 80%
- **Complexité cyclomatique** : < 10 par méthode
- **Duplication de code** : < 3%
- **Vulnérabilités** : 0 critique, 0 haute

### Code review obligatoire
```yaml
# Configuration des pull requests
protection_rules:
  required_reviews: 2
  dismiss_stale_reviews: true
  require_code_owner_reviews: true
  required_status_checks:
    - "build"
    - "security-scan"
    - "unit-tests"
```

## Déploiement

### Stratégie de déploiement
1. **Blue-Green Deployment** : Basculement sans interruption
2. **Rolling Updates** : Mise à jour progressive
3. **Rollback automatique** : En cas d'échec de déploiement

### Configuration des secrets
```bash
# Variables d'environnement sécurisées
DATABASE_CONNECTION_STRING=${{ secrets.DB_CONNECTION }}
API_KEY=${{ secrets.OPENFOODFACTS_KEY }}
SIGNING_KEY=${{ secrets.ANDROID_SIGNING_KEY }}
```

### Monitoring post-déploiement
- **Health checks** : Vérification automatique du bon fonctionnement
- **Alertes** : Notification en cas de problème
- **Métriques** : Suivi des performances et de l'utilisation

## Outils utilisés

### Développement
- **IDE** : JetBrains Rider
- **Contrôle de version** : Git + GitHub
- **Gestion de projet** : Jira

### CI/CD
- **Pipeline** : GitHub Actions
- **Analyse de code** : SonarQube
- **Tests** : MSTest / NUnit
- **Sécurité** : CodeQL, Dependabot

### Monitoring
- **Application** : Application Insights
- **Infrastructure** : Azure Monitor
- **Logs** : Centralisés avec Serilog

# Evolutions {#evolutions}

# Déploiement {#déploiement}

## Architecture de déploiement

### Infrastructure cloud
L'application Perim'App est conçue pour un déploiement cloud moderne avec une approche scalable et sécurisée.

**Base de données** :
- **Provider** : NeonDB (PostgreSQL managé)
- **Région** : Europe (GDPR compliance)
- **Haute disponibilité** : Réplication automatique
- **Sauvegardes** : Quotidiennes avec rétention 30 jours

**API et services** :
- **Hébergement** : Potentiel déploiement sur Azure App Service ou AWS Lambda
- **Monitoring** : Application Insights / CloudWatch
- **Scaling** : Auto-scaling basé sur la charge

### Déploiement mobile

#### Android
```yaml
# Configuration du build de production
Production:
  BuildConfiguration: Release
  TargetFramework: net9.0-android
  MinimumSdkVersion: 21  # Android 5.0+
  TargetSdkVersion: 34   # Android 14
  
  Optimizations:
    EnableProguard: true      # Obfuscation du code
    EnableR8: true           # Optimisation du bytecode
    AndroidLinkMode: Full    # Liaison complète des assemblies
    
  Security:
    UseHttpsOnly: true
    EnableNetworkSecurityConfig: true
    RequireAppSigning: true
```

**Google Play Store** :
```bash
# Build pour production
dotnet publish -f net9.0-android \
  -c Release \
  -p:AndroidKeyStore=true \
  -p:AndroidSigningKeyStore=release.keystore \
  -p:AndroidSigningKeyAlias=perimapp \
  -p:AndroidSigningKeyPass=$KEYSTORE_PASSWORD \
  -p:AndroidSigningStorePass=$STORE_PASSWORD
```

#### iOS (Prévu)
```yaml
# Configuration iOS future
iOS:
  MinimumVersion: "13.0"
  TargetVersion: "17.0"
  Distribution: App Store
  
  Certificates:
    Development: iOS_Development.p12
    Distribution: iOS_Distribution.p12
    
  Provisioning:
    Development: Dev_Provisioning_Profile.mobileprovision
    AdHoc: AdHoc_Provisioning_Profile.mobileprovision
    AppStore: AppStore_Provisioning_Profile.mobileprovision
```

## Processus de déploiement

### Environnements de déploiement

#### 1. Développement local
```bash
# Configuration locale
DATABASE_URL=postgresql://localhost:5432/perimapp_dev
API_BASE_URL=http://localhost:5000
LOG_LEVEL=Debug
ENVIRONMENT=Development
```

#### 2. Staging
```bash
# Configuration de test
DATABASE_URL=postgresql://staging-ep-xxx.neon.tech/perimapp_staging
API_BASE_URL=https://staging-api.perimapp.com
LOG_LEVEL=Information
ENVIRONMENT=Staging
```

#### 3. Production
```bash
# Configuration production
DATABASE_URL=postgresql://ep-restless-field-a8gc8nce.eastus2.aws.neon.tech/perimapp
API_BASE_URL=https://api.perimapp.com
LOG_LEVEL=Warning
ENVIRONMENT=Production
```

### Pipeline de déploiement automatisé

#### Étape 1 : Validation
```yaml
validation:
  - code_quality_check
  - security_scan
  - unit_tests
  - integration_tests
  - performance_tests
```

#### Étape 2 : Build
```yaml
build:
  android:
    - restore_dependencies
    - compile_application
    - generate_apk
    - sign_apk
    - upload_to_artifacts
  
  ios:  # Future
    - compile_application
    - generate_ipa
    - sign_ipa
    - upload_to_artifacts
```

#### Étape 3 : Déploiement staging
```yaml
staging_deployment:
  - download_artifacts
  - deploy_to_test_environment
  - run_smoke_tests
  - notify_qa_team
```

#### Étape 4 : Validation QA
```yaml
qa_validation:
  - manual_testing
  - user_acceptance_testing
  - performance_validation
  - security_testing
```

#### Étape 5 : Déploiement production
```yaml
production_deployment:
  - create_release_tag
  - deploy_to_play_store
  - deploy_to_app_store  # Future
  - update_documentation
  - notify_stakeholders
```

## Configuration des stores

### Google Play Store

#### Configuration du listing
```yaml
play_store:
  app_id: com.ajacorp.perimapp
  title: "Perim'App - Anti-Gaspillage Alimentaire"
  short_description: "Gérez vos dates de péremption et réduisez le gaspillage"
  full_description: |
    Perim'App vous aide à suivre les dates de péremption de vos produits 
    alimentaires pour réduire le gaspillage. Ajoutez vos produits, 
    recevez des alertes et consommez responsable.
  
  category: Food & Drink
  content_rating: Everyone
  price: Free
  
  screenshots:
    - main_screen.png
    - add_product.png
    - notifications.png
    - product_details.png
  
  keywords:
    - gaspillage alimentaire
    - dates de péremption
    - écologie
    - alimentation
    - anti-gaspi
```

#### Déploiement en phases
```yaml
rollout_strategy:
  internal_testing:
    - team_members: 5
    - duration: 1 week
    
  closed_testing:
    - beta_users: 50
    - duration: 2 weeks
    
  open_testing:
    - public_users: 500
    - duration: 3 weeks
    
  production:
    - rollout_percentage: 100%
    - monitoring: enabled
```

### Apple App Store (Futur)

#### Configuration iOS
```yaml
app_store:
  bundle_id: com.ajacorp.perimapp
  app_name: "Perim'App"
  version: "1.0.0"
  
  categories:
    primary: Food & Drink
    secondary: Utilities
  
  app_store_connect:
    team_id: XXXXXXXXXX
    app_id: YYYYYYYYYY
  
  review_guidelines:
    - no_private_apis: true
    - content_appropriate: true
    - functionality_complete: true
```

## Monitoring et maintenance

### Métriques de performance
```yaml
monitoring:
  application:
    - response_time: < 2s
    - error_rate: < 1%
    - crash_rate: < 0.1%
    - user_satisfaction: > 4.5/5
  
  infrastructure:
    - database_response: < 100ms
    - api_availability: > 99.9%
    - ssl_certificate: valid
    - security_scan: weekly
```

### Alertes automatiques
```yaml
alerts:
  critical:
    - database_down
    - api_unreachable
    - high_error_rate
    
  warning:
    - slow_response_time
    - high_memory_usage
    - ssl_expiration_soon
    
  info:
    - deployment_complete
    - backup_successful
    - security_scan_complete
```

### Plan de maintenance
```yaml
maintenance_schedule:
  daily:
    - backup_verification
    - log_analysis
    - performance_check
    
  weekly:
    - security_updates
    - dependency_updates
    - performance_optimization
    
  monthly:
    - full_security_audit
    - capacity_planning
    - disaster_recovery_test
```

## Stratégie de mise à jour

### Versioning sémantique
```
Version Format: MAJOR.MINOR.PATCH
- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes

Examples:
- 1.0.0: Initial release
- 1.1.0: Add barcode scanning
- 1.1.1: Fix date calculation bug
- 2.0.0: Complete UI redesign
```

### Distribution des mises à jour
```yaml
update_strategy:
  android:
    - auto_update: enabled
    - staged_rollout: 10% -> 50% -> 100%
    - rollback_capability: enabled
    
  ios:  # Future
    - app_store_review: required
    - phased_release: enabled
    - emergency_fixes: expedited_review
```

### Communication utilisateurs
```yaml
release_communication:
  in_app:
    - update_notifications
    - changelog_display
    - feature_highlights
    
  external:
    - blog_posts
    - social_media
    - email_newsletters
```

## Sécurité du déploiement

### Chiffrement et signatures
```yaml
security:
  code_signing:
    - android_keystore: encrypted
    - certificate_validation: enabled
    - signature_verification: required
  
  secrets_management:
    - environment_variables: encrypted
    - api_keys: rotated_regularly
    - certificates: auto_renewal
```

### Conformité et certifications
```yaml
compliance:
  gdpr:
    - data_minimization: implemented
    - user_consent: explicit
    - data_portability: available
    - right_to_erasure: implemented
  
  security_standards:
    - owasp_compliance: checked
    - penetration_testing: quarterly
    - vulnerability_scanning: automated
```

# Conclusions {#conclusions}

[image1]: <data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAAWUlEQVR4Xu3PMRHAIADAQGTiptKQBnsVBO6HX7JlzPXtF4x/uJWRGiM1RmqM1BipMVJjpMZIjZEaIzVGaozUGKkxUmOkxkiNkRojNUZqjNQYqTFSY6TGSM0B8mY4jKFvpg0AAAAASUVORK5CYII=>

[image2]: <data:image/png;base64,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>

[image3]: <data:image/png;base64,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>