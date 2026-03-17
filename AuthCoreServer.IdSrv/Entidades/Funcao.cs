using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace AuthCoreServer.IdSrv.Entidades
{
    public class Funcao : IdentityRole<int>
    {
        public override int Id { get => base.Id; set => base.Id = value; }

        public virtual ICollection<UsuarioFuncao> UsuariosFuncoes { get; set; }
    }
}
