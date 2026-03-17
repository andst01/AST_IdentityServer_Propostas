using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthCoreServer.IdSrv.Entidades
{
    public class Usuario : IdentityUser<int>
    {
        public override int Id { get; set; }
        public string Password { get; set; }
        //public DateTime DataNascimento { get; set; }
        public string Nome { get; set; }
        public virtual ICollection<UsuarioFuncao> UsuariosFuncoes { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAlteracao { get; set; }
    }
}
