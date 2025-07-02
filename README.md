<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=58BF7F&height=180&section=header&text=Perim'APP&fontSize=40&fontColor=ffffff" />
</p>

<p align="center">
  <strong>Une application .NET MAUI pour lutter contre le gaspillage alimentaire.</strong><br>
  <em>Gérez vos produits périssables, recevez des rappels, consommez à temps.</em>
</p>

<p align="center">
  <!-- Badges -->
  <img src="https://img.shields.io/badge/platforms-Android%20%7C%20Windows%20%7C%20iOS-blue?style=flat-square" />
  <img src="https://img.shields.io/badge/.NET-MAUI-58BF7F?style=flat-square&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/status-en%20développement-orange?style=flat-square" />
  <img src="https://img.shields.io/github/languages/top/AJA-Corp/perim_app?style=flat-square" />
  <img src="https://img.shields.io/github/last-commit/AJA-Corp/perim_app?style=flat-square" />
  <img src="https://img.shields.io/github/license/AJA-Corp/perim_app?style=flat-square" />
  <img src="https://img.shields.io/github/stars/AJA-Corp/perim_app?style=flat-square" />
  <img src="https://img.shields.io/github/issues/AJA-Corp/perim_app?style=flat-square" />
</p>

---

## 📋 Sommaire

- [📋 Sommaire](#-sommaire)
- [📱 À propos](#-à-propos)
- [✨ Fonctionnalités](#-fonctionnalités)
- [🚀 Installation](#-installation)
- [🖼️ Captures d’écran](#️-captures-décran)
- [🛠️ Technologies utilisées](#️-technologies-utilisées)
- [🗺️ Roadmap](#️-roadmap)
- [🤝 Contribuer](#-contribuer)
- [📄 Licence](#-licence)

---

## 📱 À propos

**Perim'APP** est une application mobile développée en **.NET MAUI**, conçue pour aider les utilisateurs à suivre la date de péremption de leurs produits alimentaires.

🎯 Objectif : **réduire le gaspillage alimentaire** en facilitant le suivi, l’organisation et la consommation à temps des produits.

---

## ✨ Fonctionnalités

- 📦 Ajout rapide de produits avec nom, date de péremption, catégorie
- 🕒 Notifications automatiques pour les produits à consommer bientôt
- 🔍 Filtrage par date, type, catégorie
- 👨‍👩‍👧‍👦 Mode famille, pour accéder aux aliments du foyer

---

## 🚀 Installation

> Prérequis : .NET 9 SDK + Visual Studio 2022 (avec MAUI) ou JetBrains Rider

```bash
# Clonez le projet
git clone https://github.com/AJA-Corp/perim_app.git

# Ouvrez le projet dans Visual Studio
# Lancez l’application sur un émulateur Android ou iOS
```

💡 Vous pouvez également utiliser `dotnet build` ou `dotnet maui run` pour lancer manuellement l’application.

---

## 🖼️ Captures d’écran

| Logo | Écran d’accueil | Ajout de produit | Alertes de péremption |
|------|-----------------|------------------|------------------------|
| ![logo](docs/screens/logo.png) | ![home](docs/screens/home.png) | ![add](docs/screens/add.png) | ![alerts](docs/screens/alerts.png) |

---

## 🛠️ Technologies utilisées

- [✅ .NET MAUI](https://learn.microsoft.com/dotnet/maui/) – développement multiplateforme
- [📦 SQLite](https://www.sqlite.org/index.html) – stockage local
- [🔔 Notifications locales MAUI](https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/notifications)
- [📱 MVVM Community Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) – architecture claire
- [🖌️ XAML] – interface utilisateur

---

## 🗺️ Roadmap

- [x] Affichage de produit (localement)
- [ ] Saisie manuelle des produits
- [ ] Système de rappels/notifications
- [ ] Scanner de code-barres
- [ ] Reconnaissance d’image (OCR des dates ?)
- [ ] Mode "famille"

---

## 🤝 Contribuer

Les contributions sont les bienvenues ! Voici comment faire :

1. Fork le repo
2. Crée ta branche (`git checkout -b feature/ma-feature`)
3. Commit tes changements (`git commit -m 'feat: nouvelle fonctionnalité'`)
4. Push ta branche (`git push origin feature/ma-feature`)
5. Ouvre une **pull request**

---

## 📄 Licence

Ce projet est sous licence **MIT**. Voir le fichier [LICENSE](LICENSE) pour plus d’informations.

---

<p align="center">
  <img src="https://capsule-render.vercel.app/api?type=waving&color=58BF7F&height=100&section=footer" />
</p>
