﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class TipoPago
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public TipoPago() { }
        public override string ToString()
        {
            return Nombre;
        }
    }
}
