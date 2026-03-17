using AuthCoreServer.IdSrv.Entidades;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

public static class SeedUsers
{
    public static async Task SeedAsync(UserManager<Usuario> userManager)
    {

        for (int i = 0; i <= 1; i++)
        {
            string userName = "";
            string email = "";
            string userNameLeitor = "";
            string emailLeitor = "";

            if (i == 0)
            {
                userName = "Alice";
                email = "alice@teste.com.br";

                userNameLeitor = "Paulo";
                emailLeitor = "paulo@teste.com.br";

            }
            else
            {
                userName = "Marta";
                email = "marta@teste.com.br";

                userNameLeitor = "Pedro";
                emailLeitor = "pedro@teste.com.br";
            }

            await CreateUserAdmin(userManager, userName, email);
            await CreateUserLeitos(userManager, userNameLeitor, emailLeitor);
        }

       
    }

    private static async Task CreateUserAdmin(UserManager<Usuario> userManager,
                                              string userName,
                                              string email)
    {

        try
        {


            var admin = new Usuario
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                Password = "Colab@123"
            };

            await userManager.CreateAsync(admin, "Colab@123");

            var resultRoles = await userManager.AddToRoleAsync(admin, "Colaborador");

            if (!resultRoles.Succeeded)
            {
                foreach (var error in resultRoles.Errors)
                    Console.WriteLine(error.Description);
            }
        }
        catch (Exception ex) 
        { 
            throw new Exception($"Erro ao criar usuário admin: {ex.Message}", ex);
        }
    }

    private static async Task CreateUserLeitos(UserManager<Usuario> userManager,
                                                string userName,
                                                string email)
    {
        var admin = new Usuario
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            Password = "Leitor@123"
        };

        await userManager.CreateAsync(admin, "Leitor@123");

        var resultRoles = await userManager.AddToRoleAsync(admin, "Leitor");

        if (!resultRoles.Succeeded)
        {
            foreach (var error in resultRoles.Errors)
                Console.WriteLine(error.Description);
        }
    }
}
