using AuthCoreServer.IdSrv.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthCoreServer.IdSrv.Mapping
{
    public class FuncaoMap : IEntityTypeConfiguration<Funcao>
    {
        public void Configure(EntityTypeBuilder<Funcao> builder)
        {
            builder.ToTable("Funcao");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ConcurrencyStamp);

            builder.Property(x => x.Name);

            builder.Property(x => x.NormalizedName);

            builder.HasMany(x => x.UsuariosFuncoes)
                   .WithOne(r => r.Role)
                   .HasForeignKey(x => x.RoleId);

            builder.Ignore(x => x.UsuariosFuncoes);
        }
    }
}
