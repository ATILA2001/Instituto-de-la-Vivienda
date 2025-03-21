﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Movimiento
    {
        public int Id { get; set; }
        public DateTime? Fecha { get; set; }
        public decimal Monto { get; set; }
        public Obra Obra { get; set; }
    }
}
