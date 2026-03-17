using AuthCoreServer.IdSrv.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthCoreServer.IdSrv.Mapping
{
    public class UsuarioMap : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuario");

            //builder.Property(x => x.Id);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ConcurrencyStamp);

            builder.Property(x => x.AccessFailedCount);

            builder.Property(x => x.DataAlteracao).IsRequired(false);

            builder.Property(x => x.DataCriacao);

            //builder.Property(x => x.DataNascimento).HasColumnName("USUA_DATA_NASCIMENTO");

            builder.Property(x => x.Email);

            builder.Property(x => x.EmailConfirmed);

            // builder.Property(x => x.Genero).HasColumnName("USUA_GENERO");

            builder.Property(x => x.LockoutEnabled);

            builder.Property(x => x.LockoutEnd);

            builder.Property(x => x.Nome);

            builder.Property(x => x.NormalizedEmail);

            builder.Property(x => x.NormalizedUserName);

            builder.Property(x => x.PasswordHash);

            builder.Property(x => x.PhoneNumber);

            builder.Property(x => x.PhoneNumberConfirmed);

            builder.Property(x => x.SecurityStamp);

            builder.Property(x => x.TwoFactorEnabled);

            builder.Property(x => x.UserName);

           
            builder.HasMany(x => x.UsuariosFuncoes)
                    .WithOne(r => r.User)
                    .HasForeignKey(x => x.UserId);


           // builder.Ignore(x => x.Password);
            builder.Ignore(x => x.UsuariosFuncoes);
        }
    }
}
