using Microsoft.AspNetCore.Identity;
using System;

namespace AuthCoreServer.IdSrv.Entidades
{
    public class UsuarioFuncao : IdentityUserRole<int>
    {
        public int Id { get; set; }

        public override int UserId { get => base.UserId; set => base.UserId = value; }

        public override int RoleId { get => base.RoleId; set => base.RoleId = value; }

        public virtual Funcao Role { get; set; }

        public virtual Usuario User { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }
    }
}
