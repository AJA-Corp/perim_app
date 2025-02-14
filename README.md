# Perim'App

Vous en avez marre de gâcher ? Périm'App est faite pour vous.

Scannez votre article à la maison, entrez sa date de péremption, recevez une notification afin de consommer votre produit afin de ne pas le jeter.

## Propriétaires

Alexandre BOBIS
Julien NGUYEN
Aroun GNANAVELAN

## Contributeurs

## Prérequis
- git
- Télécharger et installer un éditeur de code (JetBrains Rider)
- Télécharger [Java SDK 17]()
 
## Installation

```
git clone https://github.com/AJA-Corp/perim_app.git
```

Ouvrez le fichier solution `perimapp.sln` à la racine du projet 

```
git checkout develop
dotnet --list-sdks
dotnet workload list
winget upgrade Microsoft.Dotnet.SDK
dotnet workload install maui
`dotnet build -t:InstallAndroidDependencies -f net9.0-android -p:AndroidSdkDirectory=C:\Users\{USERNAME}\AppData\Local\Android\sdk -p:JavaSdkDirectory="C:\Program Files\Microsoft\{JAVAJDK}" -p:AcceptAndroidSdkLicenses=True`
```

Rendez vous dans `Main Menu > File > Settings > Build, Execution, Deployment > Android`.

`USERNAME` corresponds à votre nom d'utilisateur.

Vérifiez que les 2 paths Android concordent entre eux et que les 2 paths Java concordent entre eux.

Si ce n'est pas le cas, copiez le path dans le menu puis changez la valeur de la variable `AndroidSdkDirectory` et/ou `JavaSdkDirectory` du path de la commande par votre path copié.

Rendez-vous dans `Main Menu > Build > Clean Solution` puis `Main Menu > Build > Rebuild Solution`.

## Link to documents

- [Functional specifications](https://github.com/alexandrebobis/perim_app/blob/master/Documents/Functional_Specifications.md)
- [Technical specifications](https://github.com/alexandrebobis/perim_app/blob/master/Documents/Technical_Specifications.md)
