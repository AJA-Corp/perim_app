# Configuration Email pour Perim'App

## 📧 Configuration Simple

Pour configurer l'envoi d'emails, vous devez modifier le fichier `EmailConfig.cs` dans le dossier `Models`.

### Étapes de Configuration

1. **Ouvrez le fichier** `perimapp/Models/EmailConfig.cs`

2. **Remplacez les valeurs par défaut** :
   ```csharp
   public string SenderEmail { get; set; } = "votre-email@gmail.com"; // ← Votre adresse Gmail
   public string SenderPassword { get; set; } = "votre-mot-de-passe-application"; // ← Votre mot de passe d'application
   ```

3. **Pour Gmail** (recommandé) :
   - Utilisez votre adresse Gmail complète
   - Créez un "Mot de passe d'application" (pas votre mot de passe Gmail normal)

### 🔑 Comment créer un Mot de Passe d'Application Gmail

1. Allez dans **Paramètres Google** → **Sécurité**
2. Activez **Authentification à 2 facteurs** (obligatoire)
3. Dans **Authentification à 2 facteurs**, cliquez sur **Mots de passe d'applications**
4. Sélectionnez **Autre (nom personnalisé)** et tapez "PerimApp"
5. Copiez le mot de passe généré (16 caractères sans espaces)
6. Utilisez ce mot de passe dans `EmailConfig.cs`

### 📝 Exemple de Configuration

```csharp
public string SenderEmail { get; set; } = "monapp@gmail.com";
public string SenderPassword { get; set; } = "abcdéfghijklmnop"; // Mot de passe d'application Gmail
```

### ✅ Test de la Configuration

1. Lancez l'application
2. Tentez de vous connecter
3. Si configuré correctement, vous recevrez un email avec le code de vérification
4. Sinon, vérifiez la console pour les messages d'erreur

### 🔧 Dépannage

- **"Configuration email manquante"** : Vous n'avez pas remplacé les valeurs par défaut
- **"Authentication Required"** : Vérifiez que vous utilisez un mot de passe d'application, pas votre mot de passe Gmail
- **"SMTP Error"** : Vérifiez votre connexion internet et les informations d'identification

### 🛡️ Sécurité

⚠️ **Important** : Ne partagez jamais votre mot de passe d'application. Si compromis, révoque-le dans les paramètres Google et créez-en un nouveau.