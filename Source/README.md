# Démarrage rapide

> **Prérequis système**
>
> - Le [SDK .NET 6](https://dotnet.microsoft.com/download)
> - .NET Core étant orienté multi-plateforme, vous pouvez utiliser le système d'exploitation supporté de votre choix.
> - Un IDE tel que Visual Studio 2022 ou VS Code est recommandé pour le développement, mais ce guide de démarrage rapide indiquera comment lancer l'application depuis l'environnement ligne de commande de son choix.

1. Clonez ce répertoire GitHub, puis rejoignez dans l'environnement ligne de commande de votre choix le dossier du code source `Source\WebApi-Identity-Provider-DotNet` (contenant le fichier .csproj).
2. Exécutez la commande `dotnet restore`, suivie de `dotnet tool restore`, afin de restaurer les dépendances et outils utiles pour ce projet.
3. Mettez en place la configuration de votre choix, en ajustant le fichier appsettings.json
4. Faites de même pour les éléments de configuration secrets, tels que le *ClientSecret* France Connect, à l'aide de la commande `dotnet user-secrets set "FranceConnect:ClientSecret" "VotreSecret"`.
5. Par défaut dans cet environnement de développement, les données sont persistées en mémoire, et donc effacées dès la clôture de l'application. Vous pouvez toutefois configurer la base de données de votre choix.

6. Lancez le projet à l'aide de la commande `dotnet run`.

   Vous pouvez alors naviguer vers le fournisseur d'identité à l'adresse configurée, par défaut <https://localhost:7042>, et observer le document de découverte OpenIDConnect généré à l'adresse <https://localhost:7042/.well-known/openid-configuration> .

La documentation pour recréer ce projet depuis un projet vide n'est pas encore rédigée. L'historique des commits indique cependant précisément la nature et le détail de chaque changements.