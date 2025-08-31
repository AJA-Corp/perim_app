# Captures d'écran de l'Application Perim'App

## Interface Utilisateur - Maquettes des Écrans Principaux

### 1. Page de Connexion (LogInPage)
```
┌─────────────────────────────────────┐
│             Perim'App               │
│                                     │
│    🥬                              │
│                                     │
│  ┌─────────────────────────────────┐ │
│  │     Email: exemple@mail.com     │ │
│  └─────────────────────────────────┘ │
│                                     │
│  ┌─────────────────────────────────┐ │
│  │     Mot de passe: ••••••••      │ │
│  └─────────────────────────────────┘ │
│                                     │
│  ┌─────────── CONNEXION ───────────┐ │
│  │          (bouton vert)          │ │
│  └─────────────────────────────────┘ │
│                                     │
│         Pas de compte ?             │
│        Créer un compte              │
│                                     │
└─────────────────────────────────────┘
```

### 2. Page Principale (MainPage) - Liste des Produits
```
┌─────────────────────────────────────┐
│  👤    Vos Produits           +     │ <- Header vert #58BF7F
├─────────────────────────────────────┤
│                                     │
│ ┌─ Produits arrivant à expiration ─┐ │
│ │                                 │ │
│ │ 🥛 Lait demi-écrémé        2j  │ │
│ │    Quantité: 1                  │ │
│ │    Expire: 03/09/2024           │ │
│ │                                 │ │
│ │ 🧀 Fromage râpé           Exp. │ │ <- Rouge si expiré
│ │    Quantité: 1                  │ │
│ │    Expiré: 30/08/2024           │ │
│ │                                 │ │
│ │ 🍞 Pain de mie             5j  │ │
│ │    Quantité: 1                  │ │
│ │    Expire: 05/09/2024           │ │
│ │                                 │ │
│ │ 🥩 Steak haché           Auj.  │ │ <- Orange si aujourd'hui
│ │    Quantité: 500g               │ │
│ │    Expire: 31/08/2024           │ │
│ └─                               ─┘ │
│                                     │
└─────────────────────────────────────┘
```

### 3. Page d'Ajout de Produit (AddProductPage)
```
┌─────────────────────────────────────┐
│  ←        Ajouter Produit           │
├─────────────────────────────────────┤
│                                     │
│  ┌─────────────────────────────────┐ │
│  │   📱 Scanner code-barres        │ │
│  └─────────────────────────────────┘ │
│                                     │
│           OU SAISIR                 │
│                                     │
│  Code-barres:                       │
│  ┌─────────────────────────────────┐ │
│  │   3017620425035                 │ │
│  └─────────────────────────────────┘ │
│                                     │
│  Nom du produit:                    │
│  ┌─────────────────────────────────┐ │
│  │   Nutella 400g                  │ │
│  └─────────────────────────────────┘ │
│                                     │
│  Date de péremption:                │
│  ┌─────────────────────────────────┐ │
│  │   📅 15/12/2024                 │ │
│  └─────────────────────────────────┘ │
│                                     │
│  Quantité:                          │
│  ┌─────────────────────────────────┐ │
│  │   1                             │ │
│  └─────────────────────────────────┘ │
│                                     │
│  ┌─────────── AJOUTER ─────────────┐ │
│  │          (bouton vert)          │ │
│  └─────────────────────────────────┘ │
└─────────────────────────────────────┘
```

### 4. Page de Détails Produit (DetailsPage)
```
┌─────────────────────────────────────┐
│  ←        Nutella 400g         ✏️   │
├─────────────────────────────────────┤
│                                     │
│  ┌─────────────────────────────────┐ │
│  │                                 │ │
│  │       [Image du produit]        │ │
│  │                                 │ │
│  └─────────────────────────────────┘ │
│                                     │
│  📊 Informations                    │
│  ─────────────────────────────────   │
│  Code-barres: 3017620425035         │
│  Catégorie: Pâte à tartiner         │
│  Conservation: Température ambiante │
│                                     │
│  ⏰ Péremption                      │
│  ─────────────────────────────────   │
│  Date limite: 15/12/2024            │
│  Temps restant: 106 jours           │
│  Statut: ✅ Consommable             │
│                                     │
│  📦 Stock                           │
│  ─────────────────────────────────   │
│  Quantité: 1                        │
│  Ajouté le: 31/08/2024              │
│                                     │
│  ┌─────────── MODIFIER ────────────┐ │
│  │          (bouton bleu)          │ │
│  └─────────────────────────────────┘ │
│                                     │
│  ┌─────────── SUPPRIMER ───────────┐ │
│  │          (bouton rouge)         │ │
│  └─────────────────────────────────┘ │
└─────────────────────────────────────┘
```

### 5. Page Profil (ProfilePage)
```
┌─────────────────────────────────────┐
│  ←            Profil                │
├─────────────────────────────────────┤
│                                     │
│           👤                        │
│        Jean DUPONT                  │
│    jean.dupont@email.com            │
│                                     │
│  📊 Statistiques                    │
│  ─────────────────────────────────   │
│  Produits enregistrés: 24           │
│  Produits périmés évités: 18        │
│  Code famille: F2024-789            │
│                                     │
│  ⚙️ Paramètres                      │
│  ─────────────────────────────────   │
│  🔔 Notifications                   │
│  🏠 Gestion familiale               │
│  🔒 Sécurité                        │
│  📱 À propos                        │
│                                     │
│  ┌─────────── DÉCONNEXION ─────────┐ │
│  │          (bouton rouge)         │ │
│  └─────────────────────────────────┘ │
│                                     │
└─────────────────────────────────────┘
```

### 6. Pop-up de Notification
```
┌─────────────────────────────────────┐
│  🔔  Produit proche expiration!     │
├─────────────────────────────────────┤
│                                     │
│  Votre Lait demi-écrémé expire      │
│  dans 2 jours (03/09/2024)          │
│                                     │
│  Pensez à le consommer !            │
│                                     │
│  ┌─── VOIR ───┐  ┌─── IGNORER ───┐  │
│  │   (vert)   │  │    (gris)     │  │
│  └────────────┘  └───────────────┘  │
│                                     │
└─────────────────────────────────────┘
```

## Codes Couleurs de l'Interface

### Palette de Couleurs
- **Primaire** : #58BF7F (Vert) - Headers, boutons principaux
- **Succès** : #4CAF50 (Vert) - Produits OK
- **Attention** : #FF9800 (Orange) - Expire aujourd'hui
- **Danger** : #F44336 (Rouge) - Produits expirés
- **Neutre** : #F5F5F5 (Gris clair) - Arrière-plans
- **Texte** : #212121 (Gris foncé) - Texte principal

### États des Produits
| Statut | Couleur | Affichage | Description |
|---------|---------|-----------|-------------|
| **Expiré** | 🔴 Rouge | "Exp." | Produit périmé |
| **Aujourd'hui** | 🟠 Orange | "Auj." | Expire aujourd'hui |
| **1 jour** | 🟡 Jaune | "1j" | Expire demain |
| **2-7 jours** | 🔵 Bleu | "5j" | Expire bientôt |
| **7+ jours** | 🟢 Vert | "15j" | Expire plus tard |

## Navigation et Flux Utilisateur

### Flux Principal
1. **Connexion** → Page principale
2. **Page principale** → Détails produit (tap sur item)
3. **Page principale** → Ajout produit (bouton +)
4. **Page principale** → Profil (bouton utilisateur)

### Actions Contextuelles
- **Swipe left** sur produit → Options (modifier/supprimer)
- **Pull to refresh** → Actualisation de la liste
- **Long press** → Sélection multiple (futur)

Cette documentation visuelle complète la documentation technique en montrant l'expérience utilisateur prévue pour l'application Perim'App.