using Dominio;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    public class UsuariosVinculadosNegocio
    {
        public List<UsuarioVinculadoEF> Listar()
        {
            using (var db = new IVCdbContext())
            {
                return db.UsuariosVinculados
                    .AsNoTracking()
                    .OrderBy(u => u.Nombre)
                    .ToList();
            }
        }

        public void SetPlanningOverride(string authUserId, bool value)
        {
            using (var db = new IVCdbContext())
            {
                var user = db.UsuariosVinculados.FirstOrDefault(u => u.AuthUserId == authUserId);
                if (user != null)
                {
                    user.IsPlanningOpenOverride = value;
                    db.SaveChanges();
                }
            }
        }
    }
}
