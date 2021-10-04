# Démarrage rapide

> **Prérequis système**
>
> - Le [SDK .NET Core 3.1](https://dotnet.microsoft.com/download)
> - [Node.JS et NPM](https://nodejs.org/en/download/)
> - .NET Core étant orienté multi-plateforme, vous pouvez utiliser le système d'exploitation supporté de votre choix.
> - Un IDE tel que Visual Studio 2019 ou VS Code est recommandé pour le développement, mais ce guide de démarrage rapide indiquera comment lancer l'application depuis l'environnement ligne de commande de son choix.

1. Clonez ce répertoire GitHub, puis rejoignez dans l'environnement ligne de commande de votre choix le dossier du code source `Source\WebApi-Identity-Provider-DotNet` (contenant le fichier .csproj).
2. Exécutez la commande `dotnet restore`, suivie de `dotnet tool restore`, afin de restaurer les dépendances et outils utiles pour ce projet.
3. Mettez en place la configuration de votre choix, en ajustant le fichier appsettings.json
4. Faites de même pour les éléments de configuration secrets, tels que le *ClientSecret* France Connect, à l'aide de la commande `dotnet user-secrets set "FranceConnect:ClientSecret" "VotreSecret"`.
5. Par défaut dans cet environnement de développement, les données sont persistées en mémoire, et donc effacées dès la clôture de l'application. Vous pouvez toutefois configurer la base de données de votre choix.

6. Lancez le projet à l'aide de la commande `dotnet run`.

   Vous pouvez alors naviguer vers le fournisseur d'identité à l'adresse configurée, par défaut <https://localhost:5001>, et observer le document de découverte OpenIDConnect généré à l'adresse <https://localhost:5001/.well-known/openid-configuration> .

Pour aller plus loin, vous pouvez également suivre les documentations complètes, disponibles sous [le dossier /Documentation du répértoire](/Documentation/README.md), comportant notamment ce [guide détaillé](/Documentation/GitHub%20Actions%20%26%20D%C3%A9ploiement%20sur%20Azure.md) pour publier l'application sur un environnement Cloud Azure.
