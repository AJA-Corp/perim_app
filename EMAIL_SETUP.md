# Configuration Email pour Perim'App

## Mode Développement (Par défaut)

Par défaut, l'application utilise un **mode développement** qui simule l'envoi d'emails en affichant le code de vérification dans la console. Cela permet de tester la fonctionnalité sans configuration email.

## Configuration pour Production

Pour activer l'envoi d'emails réels, vous devez configurer un compte email SMTP.

### Option 1: Gmail (Recommandé pour les tests)

1. Créez un compte Gmail dédié à l'application
2. Activez l'authentification à 2 facteurs
3. Générez un "Mot de passe d'application" :
   - Allez dans Paramètres Google > Sécurité > Authentification à 2 facteurs
   - Sélectionnez "Mots de passe d'applications"
   - Créez un nouveau mot de passe pour "Autre (nom personnalisé)"
   - Nommez-le "PerimApp"

4. Modifiez le fichier `EmailConfig.cs` ou utilisez la méthode `ConfigureEmail()` :

```csharp
var emailService = new EmailService();
emailService.ConfigureEmail(
    "votre-email@gmail.com", 
    "votre-mot-de-passe-application", 
    EmailProvider.Gmail
);
```

### Option 2: Outlook/Hotmail

```csharp
var emailService = new EmailService();
emailService.ConfigureEmail(
    "votre-email@outlook.com", 
    "votre-mot-de-passe", 
    EmailProvider.Outlook
);
```

### Option 3: Serveur SMTP personnalisé

```csharp
var emailService = new EmailService();
// Configurez d'abord les paramètres SMTP personnalisés
var config = new EmailConfig
{
    SmtpServer = "votre-serveur-smtp.com",
    SmtpPort = 587,
    Provider = EmailProvider.Custom
};
emailService.ConfigureEmail("email@votredomaine.com", "votre-mot-de-passe", EmailProvider.Custom);
```

## Test de la Configuration

Pour tester la configuration email :

1. Lancez l'application en mode debug
2. Tentez une connexion
3. Vérifiez la console pour voir si l'email est envoyé ou simulé
4. Si configuré correctement, l'email devrait être reçu dans la boîte de réception

## Dépannage

- **Erreur "Authentication Required"** : Vérifiez que vous utilisez un mot de passe d'application pour Gmail
- **Erreur de connexion SSL** : Vérifiez les paramètres de port et SSL
- **Email non reçu** : Vérifiez les dossiers spam/courrier indésirable

## Sécurité

⚠️ **Important** : Ne jamais commiter les vraies informations d'identification dans le code source. Utilisez des variables d'environnement ou un système de configuration sécurisé en production.