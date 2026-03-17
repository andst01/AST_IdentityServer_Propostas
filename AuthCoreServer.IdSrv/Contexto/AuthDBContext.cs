using AuthCoreServer.IdSrv.Entidades;
using AuthCoreServer.IdSrv.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AuthCoreServer.IdSrv.Contexto
{
    public class AuthDBContext : IdentityDbContext<Usuario,
    Funcao,
    int,
    IdentityUserClaim<int>,
    UsuarioFuncao,
    IdentityUserLogin<int>,
    IdentityRoleClaim<int>,
    IdentityUserToken<int>>
    {

        public AuthDBContext(DbContextOptions<AuthDBContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UsuarioMap());
            modelBuilder.ApplyConfiguration(new FuncaoMap());
            modelBuilder.ApplyConfiguration(new UsuarioFuncaoMap());

            

        }
    }
}
