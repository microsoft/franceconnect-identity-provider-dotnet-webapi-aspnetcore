// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace WebApi_Identity_Provider_DotNet.Configuration
{
    public class ConsentOptions
    {
        public static bool EnableOfflineAccess = true;
        public static string OfflineAccessDisplayName = "Acc�s hors connexion";
        public static string OfflineAccessDescription = "Acc�s � vos donn�es, m�me lorsque vous �tes d�connect�";

        public static readonly string MustChooseOneErrorMessage = "Vous devez choisir au moins une permission";
        public static readonly string InvalidSelectionErrorMessage = "Selection invalide";
    }
}