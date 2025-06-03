using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Formulacion
    {
        public Obra Obra {get; set; }
        public int Id { get; set; }
       public decimal Monto_26 { get; set; }
        public decimal Monto_27 { get; set; }
        public decimal Monto_28 { get; set; }
        public int Ppi { get; set; }
        public UnidadMedida UnidadMedida { get; set; }
        public decimal ValorMedida { get; set; }
        public decimal Plurianual { get; set; }
        public decimal Techos2026 { get; set; }
        public string Observacion { get; set; }
        public Prioridad Prioridad { get; set; }
        public Formulacion() { }
        public DateTime? MesBase { get; set; }
    }
}
