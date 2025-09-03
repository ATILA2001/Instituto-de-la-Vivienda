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

        public static bool GetIsFormulationOpen()
        {
            using (var db = new IVCdbContext())
            {
                var plani = db.ABMPlani.FirstOrDefault();
                return plani != null && plani.IsFormulationOpen;
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

        public static void SetIsFormulationOpen(bool isOpen)
        {
            using (var db = new IVCdbContext())
            {
                var plani = db.ABMPlani.FirstOrDefault();
                if (plani != null)
                {
                    plani.IsFormulationOpen = isOpen;
                }
                else
                {
                    plani = new ABMPlaniEF { IsFormulationOpen = isOpen };
                    db.ABMPlani.Add(plani);
                }
                db.SaveChanges();
            }
        }

    }
}
