using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    // DbContext principal para Entity Framework
    public class IVCdbContext : DbContext
    {
        // Constructor que usa la cadena de conexión "IVCDbContext" del Web.config
        public IVCdbContext() : base("name=IVC_TEST")
        {
            // Opcional: Deshabilita la creación automática de la base de datos
            Database.SetInitializer<IVCdbContext>(null);
        }

        // DbSet para cada entidad con sufijo EF
        public DbSet<ObraEF> Obras { get; set; } // Obras principales
        public DbSet<ProyectoEF> Proyectos { get; set; } // Proyectos asociados a Obra
        public DbSet<FormulacionEF> Formulaciones { get; set; } // Formulación asociada a Obra
        public DbSet<AutorizanteEF> Autorizantes { get; set; } // Autorizantes de cada Obra
        public DbSet<RedeterminacionEF> Redeterminaciones { get; set; } // Redeterminaciones por Autorizante
        public DbSet<CertificadoEF> Certificados { get; set; } // Certificados por Autorizante
        public DbSet<MovimientoEF> Movimientos { get; set; } // Movimientos asociados a Obra
        public DbSet<LegitimoEF> Legitimos { get; set; } // Legítimos asociados a Obra
        public DbSet<DevengadoEF> Devengados { get; set; } // Devengados asociados a Obra
        public DbSet<ExpedienteReliqEF> ExpedientesReliq { get; set; } // Expedientes de Reliquidación

        // Catálogos y entidades de referencia con sufijo EF
        public DbSet<EmpresaEF> Empresas { get; set; }
        public DbSet<ContrataEF> Contratas { get; set; }
        public DbSet<BarrioEF> Barrios { get; set; }
        public DbSet<AreaEF> Areas { get; set; }
        public DbSet<LineaGestionEF> LineasGestion { get; set; }
        public DbSet<LineaGestionFFEF> LineasGestionFF { get; set; }
        public DbSet<ConceptoEF> Conceptos { get; set; }
        public DbSet<EstadoAutorizanteEF> EstadosAutorizante { get; set; }
        public DbSet<EstadoRedetEF> EstadosRedet { get; set; }        public DbSet<TipoPagoEF> TiposPago { get; set; }
        
        // Nuevas tablas para consultas EF
        public DbSet<PaseSadeEF> PasesSade { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Ejemplo: configuración de relaciones o nombres de tablas
            // modelBuilder.Entity<Autorizante>().ToTable("Autorizantes");
            // modelBuilder.Entity<Obra>().HasRequired(o => o.Empresa).WithMany().HasForeignKey(o => o.EmpresaId);


            // Relación Obra - BdProyecto (0..1 a 1)
            modelBuilder.Entity<ObraEF>()
                .HasOptional(o => o.Proyecto) // Obra puede tener o no un BdProyecto
                .WithRequired(bp => bp.ObraEF);   // BdProyecto requiere una Obra

            // Relación Obra - Formulacion (0..1 a 1)
            modelBuilder.Entity<ObraEF>()
                .HasOptional(o => o.Formulacion) // Obra puede tener o no una Formulacion
                .WithRequired(f => f.ObraEF);      // Formulacion requiere una Obra

            // Relación Obra - Autorizante (1 a muchos)
            modelBuilder.Entity<ObraEF>()
                .HasMany(o => o.Autorizantes)
                .WithRequired(a => a.Obra)
                .HasForeignKey(a => a.ObraId);

            // Relación Obra - Movimiento (1 a muchos)
            modelBuilder.Entity<ObraEF>()
                .HasMany(o => o.Movimientos)
                .WithRequired(m => m.ObraEF)
                .HasForeignKey(m => m.ObraId);

            // Relación BdProyecto - LineaGestion (muchos a uno)
            modelBuilder.Entity<ProyectoEF>()
                .HasRequired(bp => bp.LineaGestionEF)
                .WithMany(lg => lg.BdProyectos)
                .HasForeignKey(bp => bp.LineaGestionEFId);

            // Relación Autorizante - Concepto (muchos a uno)
            modelBuilder.Entity<AutorizanteEF>()
                .HasRequired(a => a.Concepto)
                .WithMany(c => c.Autorizantes)
                .HasForeignKey(a => a.ConceptoId);

            // Relación Autorizante - EstadoAutorizante (muchos a uno)
            modelBuilder.Entity<AutorizanteEF>()
                .HasRequired(a => a.Estado)
                .WithMany(ea => ea.Autorizantes)
                .HasForeignKey(a => a.EstadoId);

            // REVISAR: La relación entre Autorizante y Certificado se basa en Nro_Autorizante, que es una clave única pero no la clave primaria.
            // EF6 no soporta relaciones de clave externa a claves únicas que no son primarias (Unique Key Foreign Keys).
            // Por lo tanto, esta relación no se puede mapear directamente con Fluent API.
            // La carga de certificados para un autorizante deberá hacerse manualmente en la capa de negocio.
            /*
            modelBuilder.Entity<AutorizanteEF>()
                .HasMany(a => a.Certificados)
                .WithRequired(c => c.Autorizante)
                .HasForeignKey(c => c.AutorizanteId);
            */

            // Relación Redeterminacion - EstadoRedet (muchos a uno)
            modelBuilder.Entity<RedeterminacionEF>()
                .HasRequired(r => r.Etapa) // Corregido: Se usa la propiedad de navegación 'Etapa'
                .WithMany(er => er.Redeterminaciones)
                .HasForeignKey(r => r.EstadoRedetEFId);            // Relación Certificado - TipoPago (muchos a uno)
            modelBuilder.Entity<CertificadoEF>()
                .HasRequired(c => c.TipoPago) // Corregido de c.Tipo a c.TipoPago
                .WithMany(tp => tp.Certificados)
                .HasForeignKey(c => c.TipoPagoId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
