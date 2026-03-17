using AuthCoreServer.IdSrv.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthCoreServer.IdSrv.Mapping
{
    public class UsuarioFuncaoMap : IEntityTypeConfiguration<UsuarioFuncao>
    {
        public void Configure(EntityTypeBuilder<UsuarioFuncao> builder)
        {
            builder.ToTable("UsuarioFuncao");

            builder.Property(x => x.RoleId);

            builder.Property(x => x.UserId);

            //builder.HasNoKey();

           // builder.HasKey(x => new { x.RoleId, x.UserId }).HasName("PK_FNUS");

            builder.Property(x => x.DataInicio);

            builder.Property(x => x.DataFim).IsRequired(false);

            builder.Ignore(x => x.Id);
            builder.Ignore(x => x.Role);
            builder.Ignore(x => x.User);
        }
    }
}
