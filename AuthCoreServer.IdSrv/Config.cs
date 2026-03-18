// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using IdentityServer4.Models;

namespace AuthCoreServer.IdSrv
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
                   new IdentityResource[]
                   {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource("roles", new [] { "role" })
                   };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("scope1"),
                new ApiScope("scope2"),
                new ApiScope("api1", "Minha API")
    
               // new ApiScope("openid"),
               // new ApiScope("profile"),
               //  new ApiScope("roles")
            };



        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // m2m client credentials flow client
                new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "scope1" }
                },

                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:44300/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "scope2" }
                },
                new Client()
                {
                    ClientId = "mvc",
                    ClientName = "MVC Web App",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris = { "https://localhost:7022/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:7022/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        "openid",
                        "profile",
                        "roles"
                    },

                    AllowOfflineAccess = true,

                    // MUITO IMPORTANTE
                    AllowedCorsOrigins =
                    {
                        "https://localhost:7022"
                    }
                },

                new Client
                {
                    ClientId = "react",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris = { "https://localhost:5173/callback" },
                    PostLogoutRedirectUris = { "https://localhost:5173" },

                    AllowedCorsOrigins = { "https://localhost:5173" },

                    AllowedScopes = { "openid", "profile", "roles", "api1" },

                    AllowAccessTokensViaBrowser = true
                }
            };
    }
}