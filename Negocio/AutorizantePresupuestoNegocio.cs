using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Negocio
{
    /// <summary>
    /// Lógica de negocio para gestionar los presupuestos de los autorizantes.
    /// </summary>
    public class AutorizantePresupuestoNegocio
    {
        private readonly DbContext _context;

        /// <summary>
  /// Constructor que recibe un DbContext
   /// </summary>
        public AutorizantePresupuestoNegocio(DbContext context)
      {
    _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtiene todos los presupuestos de autorizantes.
        /// </summary>
        public IEnumerable<AutorizantePresupuestoEF> GetAll()
        {
      return _context.Set<AutorizantePresupuestoEF>()
      .OrderByDescending(p => p.FechaNorma)
         .ToList();
    }

        /// <summary>
   /// Obtiene un presupuesto por su ID.
        /// </summary>
        public AutorizantePresupuestoEF GetById(int id)
    {
            if (id <= 0) throw new ArgumentException("Id debe ser mayor que cero.", nameof(id));
     
      return _context.Set<AutorizantePresupuestoEF>()
 .FirstOrDefault(p => p.Id == id);
    }

        /// <summary>
        /// Obtiene el presupuesto asociado a un Autorizante por el ID del autorizante.
        /// </summary>
        public AutorizantePresupuestoEF GetByAutorizanteId(int autorizanteId)
 {
 if (autorizanteId <= 0) throw new ArgumentException("AutorizanteId debe ser mayor que cero.", nameof(autorizanteId));
 
       return _context.Set<AutorizantePresupuestoEF>()
    .FirstOrDefault(p => p.AutorizanteId == autorizanteId);
     }

        /// <summary>
        /// Obtiene todos los presupuestos para un rango de fechas de norma.
        /// </summary>
        public IEnumerable<AutorizantePresupuestoEF> GetByFechaRange(DateTime fechaDesde, DateTime fechaHasta)
        {
            return _context.Set<AutorizantePresupuestoEF>()
      .Where(p => p.FechaNorma >= fechaDesde && p.FechaNorma <= fechaHasta)
.OrderBy(p => p.FechaNorma)
        .ToList();
        }

     /// <summary>
   /// Obtiene presupuestos cuyo importe sea mayor o igual al especificado.
        /// </summary>
        public IEnumerable<AutorizantePresupuestoEF> GetByImporteMinimo(decimal importeMinimo)
        {
            return _context.Set<AutorizantePresupuestoEF>()
      .Where(p => p.Importe >= importeMinimo)
     .OrderByDescending(p => p.Importe)
      .ToList();
        }

        /// <summary>
        /// Valida los campos del presupuesto y devuelve lista de errores (vacía si es válido).
   /// </summary>
        public IList<string> Validate(AutorizantePresupuestoEF presupuesto)
        {
  if (presupuesto == null) throw new ArgumentNullException(nameof(presupuesto));
            
        var errors = new List<string>();

   if (presupuesto.AutorizanteId <= 0)
                errors.Add("AutorizanteId es requerido y debe ser mayor que cero.");

            if (presupuesto.Importe <= 0)
       errors.Add("Importe debe ser mayor que cero.");

if (string.IsNullOrWhiteSpace(presupuesto.Norma))
      errors.Add("Norma es requerida.");
            else if (presupuesto.Norma.Length > 255)
           errors.Add("Norma no puede exceder 255 caracteres.");

      if (presupuesto.FechaNorma == default(DateTime))
         errors.Add("FechaNorma es requerida.");

      if (presupuesto.FechaNorma > DateTime.Now)
         errors.Add("FechaNorma no puede ser una fecha futura.");

     return errors;
        }

      /// <summary>
        /// Valida que no exista otro presupuesto para el mismo autorizante.
        /// </summary>
        private bool ExistsForAutorizante(int autorizanteId, int? excludeId = null)
        {
            var query = _context.Set<AutorizantePresupuestoEF>()
   .Where(p => p.AutorizanteId == autorizanteId);

            if (excludeId.HasValue)
      {
       query = query.Where(p => p.Id != excludeId.Value);
         }

    return query.Any();
        }

        /// <summary>
        /// Agrega un nuevo presupuesto, guarda cambios y devuelve la entidad persistida (con Id).
        /// </summary>
        public AutorizantePresupuestoEF Add(AutorizantePresupuestoEF presupuesto)
        {
    if (presupuesto == null) throw new ArgumentNullException(nameof(presupuesto));

 var errors = Validate(presupuesto);
            if (errors.Count > 0)
              throw new InvalidOperationException("Validación fallida: " + string.Join("; ", errors));

         // Validar que el autorizante exista
            var autorizanteExists = _context.Set<AutorizanteEF>().Any(a => a.Id == presupuesto.AutorizanteId);
            if (!autorizanteExists)
  throw new InvalidOperationException($"El Autorizante con Id {presupuesto.AutorizanteId} no existe.");

   // Validar que no exista otro presupuesto para el mismo autorizante
            if (ExistsForAutorizante(presupuesto.AutorizanteId))
                throw new InvalidOperationException($"Ya existe un presupuesto para el Autorizante con Id {presupuesto.AutorizanteId}.");

 var set = _context.Set<AutorizantePresupuestoEF>();
    set.Add(presupuesto);
            _context.SaveChanges();
  
        return presupuesto;
      }

        /// <summary>
        /// Actualiza un presupuesto existente; devuelve la entidad actualizada.
        /// </summary>
        public AutorizantePresupuestoEF Update(AutorizantePresupuestoEF presupuesto)
        {
  if (presupuesto == null) throw new ArgumentNullException(nameof(presupuesto));
            if (presupuesto.Id <= 0) throw new ArgumentException("Id inválido para actualización.", nameof(presupuesto));

   var errors = Validate(presupuesto);
  if (errors.Count > 0)
          throw new InvalidOperationException("Validación fallida: " + string.Join("; ", errors));

            var set = _context.Set<AutorizantePresupuestoEF>();
         var existing = set.Find(presupuesto.Id);
      
  if (existing == null)
  throw new InvalidOperationException($"Presupuesto con Id {presupuesto.Id} no existe.");

     // Validar que no exista otro presupuesto para el mismo autorizante (excluyendo el actual)
            if (existing.AutorizanteId != presupuesto.AutorizanteId && ExistsForAutorizante(presupuesto.AutorizanteId, presupuesto.Id))
 throw new InvalidOperationException($"Ya existe un presupuesto para el Autorizante con Id {presupuesto.AutorizanteId}.");

            // Copiar campos editables
  existing.AutorizanteId = presupuesto.AutorizanteId;
 existing.Importe = presupuesto.Importe;
     existing.Norma = presupuesto.Norma;
 existing.FechaNorma = presupuesto.FechaNorma;

            _context.Entry(existing).State = EntityState.Modified;
  _context.SaveChanges();

         return existing;
        }

      /// <summary>
        /// Elimina un presupuesto por id; devuelve true si se eliminó.
        /// </summary>
  public bool Delete(int id)
        {
        if (id <= 0) throw new ArgumentException("Id debe ser mayor que cero.", nameof(id));

            var set = _context.Set<AutorizantePresupuestoEF>();
        var existing = set.Find(id);
            
    if (existing == null) return false;

  set.Remove(existing);
     _context.SaveChanges();
            
        return true;
     }

    /// <summary>
    /// Elimina el presupuesto asociado a un autorizante específico.
        /// </summary>
        public bool DeleteByAutorizanteId(int autorizanteId)
  {
            if (autorizanteId <= 0) throw new ArgumentException("AutorizanteId debe ser mayor que cero.", nameof(autorizanteId));

          var presupuesto = GetByAutorizanteId(autorizanteId);
            
       if (presupuesto == null) return false;

            return Delete(presupuesto.Id);
   }

        /// <summary>
        /// Obtiene el importe total de todos los presupuestos.
        /// </summary>
        public decimal GetTotalImporte()
 {
     return _context.Set<AutorizantePresupuestoEF>()
    .Sum(p => (decimal?)p.Importe) ?? 0;
   }

        /// <summary>
        /// Obtiene estadísticas de presupuestos: Total, Promedio, Mínimo, Máximo.
        /// </summary>
 public Dictionary<string, decimal> GetEstadisticas()
 {
         var presupuestos = _context.Set<AutorizantePresupuestoEF>().ToList();

            if (!presupuestos.Any())
  {
           return new Dictionary<string, decimal>
     {
        { "Total", 0 },
            { "Promedio", 0 },
   { "Minimo", 0 },
    { "Maximo", 0 },
          { "Cantidad", 0 }
          };
        }

            return new Dictionary<string, decimal>
            {
     { "Total", presupuestos.Sum(p => p.Importe) },
   { "Promedio", presupuestos.Average(p => p.Importe) },
  { "Minimo", presupuestos.Min(p => p.Importe) },
          { "Maximo", presupuestos.Max(p => p.Importe) },
    { "Cantidad", presupuestos.Count }
            };
  }

 /// <summary>
        /// Carga el presupuesto para un autorizante (helper para cargar la propiedad NotMapped).
     /// </summary>
        public void CargarPresupuestoParaAutorizante(AutorizanteEF autorizante)
      {
      if (autorizante == null) throw new ArgumentNullException(nameof(autorizante));
            
       autorizante.Presupuesto = GetByAutorizanteId(autorizante.Id);
        }

        /// <summary>
        /// Carga los presupuestos para una colección de autorizantes.
        /// </summary>
   public void CargarPresupuestosParaAutorizantes(IEnumerable<AutorizanteEF> autorizantes)
        {
if (autorizantes == null) throw new ArgumentNullException(nameof(autorizantes));

     var autorizanteIds = autorizantes.Select(a => a.Id).ToList();
     
            var presupuestos = _context.Set<AutorizantePresupuestoEF>()
       .Where(p => autorizanteIds.Contains(p.AutorizanteId))
        .ToList()
  .ToDictionary(p => p.AutorizanteId);

     foreach (var autorizante in autorizantes)
            {
       if (presupuestos.TryGetValue(autorizante.Id, out var presupuesto))
     {
        autorizante.Presupuesto = presupuesto;
   }
            }
        }
    }
}
