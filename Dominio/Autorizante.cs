﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    [Serializable]
    public class Autorizante
    {
        public int Id { get; set; }
        public Obra Obra { get; set; }
        public Concepto Concepto { get; set; }
        public string CodigoAutorizante { get; set; }
        public string Detalle { get; set; }
        public EstadoAutorizante Estado { get; set; }
        public string Expediente { get; set; }
        public decimal MontoAutorizado { get; set; }
        public bool AutorizacionGG { get; set; }
        public DateTime? Fecha { get; set; }
        public string Empresa { get; set; }
        public DateTime? FechaSade { get; set; }
        public string BuzonSade { get; set; }
        public DateTime? MesBase { get; set; }
        public Autorizante() { }

    }
}
