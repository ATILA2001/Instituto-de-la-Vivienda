using Dominio;
using System;
using System.Data.Entity;
using System.Linq;

namespace Negocio
{
    /// <summary>
    /// Clase de negocio para manejar la entidad ExpedienteReliqEF utilizando Entity Framework.
    /// </summary>
    public class ExpedienteReliqNegocioEF
    {
        /// <summary>
        /// Recreación del método GuardarOActualizar utilizando EF.
        /// Busca un registro existente por CodigoRedet y Mes/Año. Si lo encuentra, lo actualiza.
        /// Si no, crea un nuevo registro.
        /// </summary>
        public bool GuardarOActualizar(string codigoRedet, DateTime mesAprobacion, string expediente)
        {
            using (var context = new IVCdbContext())
            {
                try
                {
                    // 1. Buscar si ya existe un registro para ese código y mes/año.
                    // Se compara por Año y Mes para replicar la lógica original de SQL.
                    var registroExistente = context.ExpedientesReliq
                        .FirstOrDefault(e => e.CodigoRedet == codigoRedet &&
                                             e.MesAprobacion.Year == mesAprobacion.Year &&
                                             e.MesAprobacion.Month == mesAprobacion.Month);

                    if (registroExistente != null)
                    {
                        // 2. Si existe, se actualiza el expediente.
                        registroExistente.Expediente = expediente;
                    }
                    else
                    {
                        // 3. Si no existe, se crea una nueva entidad y se añade.
                        var nuevoRegistro = new ExpedienteReliqEF
                        {
                            CodigoRedet = codigoRedet,
                            MesAprobacion = mesAprobacion,
                            Expediente = expediente
                        };
                        context.ExpedientesReliq.Add(nuevoRegistro);
                    }

                    // 4. Se guardan los cambios (sea un UPDATE o un INSERT).
                    context.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    // Manejo de errores (opcionalmente, registrar el error).
                    System.Diagnostics.Debug.WriteLine($"Error en GuardarOActualizar ExpedienteReliq: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
