using Dominio;
using System.Linq;

namespace Negocio
{
    public class ABMPlaniNegocio
    {
        public static bool GetIsPlanningOpen()
        {
            using (var db = new IVCdbContext())
            {
                var plani = db.ABMPlani.FirstOrDefault();
                return plani != null && plani.IsPlanningOpen;
            }
        }

        public static void SetIsPlanningOpen(bool isOpen)
        {
            using (var db = new IVCdbContext())
            {
                var plani = db.ABMPlani.FirstOrDefault();
                if (plani != null)
                {
                    plani.IsPlanningOpen = isOpen;
                }
                else
                {
                    plani = new ABMPlaniEF { IsPlanningOpen = isOpen };
                    db.ABMPlani.Add(plani);
                }
                db.SaveChanges();
            }
        }
    }
}
