using System;
using System.Configuration;

namespace Negocio
{
    /// <summary>
    /// Define el ciclo de formulación vigente: el año base y los 3 años consecutivos
    /// para los que se carga monto estimado de cada obra (base, base+1, base+2).
    ///
    /// El año base se configura en appSettings (clave "FormulacionAnioBase") para poder
    /// "iniciar un nuevo año" sin recompilar. Si la clave falta o es inválida, usa 2027.
    /// Al iniciar un nuevo ciclo se incrementa este valor; las formulaciones de años
    /// anteriores ya no entran en la ventana y pueden eliminarse (no se guarda histórico).
    /// </summary>
    public static class FormulacionCiclo
    {
        private const int AnioBasePorDefecto = 2027;

        public static int AnioBase
        {
            get
            {
                var raw = ConfigurationManager.AppSettings["FormulacionAnioBase"];
                return int.TryParse(raw, out int valor) ? valor : AnioBasePorDefecto;
            }
        }

        /// <summary>Los 3 años del ciclo vigente: [base, base+1, base+2].</summary>
        public static int[] Anios
        {
            get
            {
                int b = AnioBase;
                return new[] { b, b + 1, b + 2 };
            }
        }

        /// <summary>Fecha que se persiste en FECHA_PERIODO para un año del ciclo (1 de enero).</summary>
        public static DateTime FechaPeriodo(int anio) => new DateTime(anio, 1, 1);
    }
}
