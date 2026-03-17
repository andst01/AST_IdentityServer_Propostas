using AuthCoreServer.IdSrv.Entidades;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthCoreServer.IdSrv
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<Usuario> _userManager;

        public ProfileService(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var userId = context.Subject.FindFirst("sub")?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>();

            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            // 🔥 FILTRO IMPORTANTE
            claims = claims
                .Where(c => context.RequestedClaimTypes.Contains(c.Type))
                .ToList();

            context.IssuedClaims.AddRange(claims);

            Console.WriteLine("ProfileService executado");
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var userId = context.Subject.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);

            context.IsActive = user != null;
        }
    }
}
