using AuthCoreServer.IdSrv.Entidades;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace AuthCoreServer.IdSrv
{
    //public static class SeedRoles
    //{
    //    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    //    {
    //        string[] roles = { "Admin", "Colaborador", "Leitor" };

    //        foreach (var role in roles)
    //        {
    //            if (!await roleManager.RoleExistsAsync(role))
    //            {
    //                await roleManager.CreateAsync(new IdentityRole(role));
    //            }
    //        }
    //    }
    //}

    public static class SeedRoles
    {
        public static async Task SeedAsync(RoleManager<Funcao> roleManager)
        {
            string[] roles = { "Admin", "Colaborador", "Leitor" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new Funcao
                    {
                        Name = role,
                        NormalizedName = role.ToUpper()
                    });
                }
            }
        }
    }

}
