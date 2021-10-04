// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE-IdentityServer4-Templates.md in the project root for license information.

// This file was changed for the needs of the project. These modifications are subject to 
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebApi_Identity_Provider_DotNet.Configuration
{
    public class ConsentOptions
    {
        public static bool EnableOfflineAccess = true;
        public static string OfflineAccessDisplayName = "Accès hors connexion";
        public static string OfflineAccessDescription = "Accès à vos données, même lorsque vous êtes déconnecté";

        public static readonly string MustChooseOneErrorMessage = "Vous devez choisir au moins une permission";
        public static readonly string InvalidSelectionErrorMessage = "Selection invalide";
    }
}
