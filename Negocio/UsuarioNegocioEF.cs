using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class UsuarioNegocioEF
    {
        private readonly DbContext _context;

        // Constructor que recibe un DbContext
        public UsuarioNegocioEF(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Devuelve todos los usuarios (incluye Area)
        public IEnumerable<UsuarioEF> GetAll()
        {
            return _context.Set<UsuarioEF>()
                           .Include(u => u.Area)
                           .ToList();
        }

        // Devuelve un usuario por id (incluye Area) o null si no existe
        public UsuarioEF GetById(int id)
        {
            if (id <= 0) throw new ArgumentException("Id debe ser mayor que cero.", nameof(id));
            return _context.Set<UsuarioEF>()
                           .Include(u => u.Area)
                           .FirstOrDefault(u => u.Id == id);
        }

        public UsuarioEF GetByEmailOrCUIL(string emailOrCUIL)
        {
            if (string.IsNullOrWhiteSpace(emailOrCUIL))
                throw new ArgumentException("Debe proporcionar correo o CUIL.");
            return _context.Set<UsuarioEF>()
                           .Include(u => u.Area)
                           .FirstOrDefault(u => u.Correo == emailOrCUIL || u.Cuil == emailOrCUIL);
        }

        // Devuelve usuarios por AreaId
        public IEnumerable<UsuarioEF> GetByArea(int areaId)
        {
            return _context.Set<UsuarioEF>()
                           .Where(u => u.AreaId == areaId)
                           .Include(u => u.Area)
                           .ToList();
        }

        // Devuelve solo administradores (Tipo == true)
        public IEnumerable<UsuarioEF> GetAdministrators()
        {
            return _context.Set<UsuarioEF>()
                           .Where(u => u.Tipo)
                           .Include(u => u.Area)
                           .ToList();
        }

        // Valida campos básicos del usuario y devuelve lista de errores (vacía si válido)
        public IList<string> Validate(UsuarioEF usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                errors.Add("Nombre es requerido.");

            if (string.IsNullOrWhiteSpace(usuario.Correo))
                errors.Add("Correo es requerido.");
            else if (!usuario.Correo.Contains("@"))
                errors.Add("Correo no tiene un formato válido.");

            if (string.IsNullOrWhiteSpace(usuario.Cuil))
                errors.Add("Cuil es requerido.");

            return errors;
        }

        // Agrega un usuario, guarda cambios y devuelve la entidad persistida (con Id)
        public UsuarioEF Add(UsuarioEF usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            var errors = Validate(usuario);
            if (errors.Count > 0)
                throw new InvalidOperationException("Validación fallida: " + string.Join("; ", errors));

            var set = _context.Set<UsuarioEF>();
            set.Add(usuario);
            _context.SaveChanges();
            return usuario;
        }

        // Actualiza un usuario existente; devuelve la entidad actualizada
        public UsuarioEF Update(UsuarioEF usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));
            if (usuario.Id <= 0) throw new ArgumentException("Id inválido para actualización.", nameof(usuario));

            var errors = Validate(usuario);
            if (errors.Count > 0)
                throw new InvalidOperationException("Validación fallida: " + string.Join("; ", errors));

            var set = _context.Set<UsuarioEF>();
            var existing = set.Find(usuario.Id) ?? throw new InvalidOperationException($"Usuario con Id {usuario.Id} no existe.");

            // Copiar campos editables
            existing.Nombre = usuario.Nombre;
            existing.Correo = usuario.Correo;
            existing.Tipo = usuario.Tipo;
            existing.Estado = usuario.Estado;
            existing.Cuil = usuario.Cuil;
            existing.AreaId = usuario.AreaId;

            _context.Entry(existing).State = EntityState.Modified;
            _context.SaveChanges();

            // Si se necesita, recargar navegación Area
            _context.Entry(existing).Reference(u => u.Area).Load();

            return existing;
        }

        // Elimina un usuario por id; devuelve true si se eliminó
        public bool Delete(int id)
        {
            if (id <= 0) throw new ArgumentException("Id debe ser mayor que cero.", nameof(id));

            var set = _context.Set<UsuarioEF>();
            var existing = set.Find(id);
            if (existing == null) return false;

            set.Remove(existing);
            _context.SaveChanges();
            return true;
        }

        // Invierte el Estado de un usuario y retorna el nuevo valor; lanza si no existe
        public bool ToggleEstado(int id)
        {
            if (id <= 0) throw new ArgumentException("Id debe ser mayor que cero.", nameof(id));

            var set = _context.Set<UsuarioEF>();
            var existing = set.Find(id);
            if (existing == null)
                throw new InvalidOperationException($"Usuario con Id {id} no existe.");

            existing.Estado = !existing.Estado;
            _context.Entry(existing).State = EntityState.Modified;
            _context.SaveChanges();
            return existing.Estado;
        }

        // Agregado a la clase UsuarioNegocioEF
        public List<UsuarioEF> ListarDdlRedet()
        {
            // Area fijo 16 (ID BD) y solo usuarios activos (Estado == true)
            return _context.Set<UsuarioEF>()
                           .Where(u => u.Estado && u.AreaId.HasValue && u.AreaId.Value == 16)
                           .OrderBy(u => u.Nombre)
                           .ToList();
        }

    }
}
