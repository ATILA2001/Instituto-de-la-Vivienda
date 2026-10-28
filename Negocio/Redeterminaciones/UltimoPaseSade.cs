using System;

namespace Negocio.Redeterminaciones
{
    /// <summary>Último pase SADE de un expediente. Buzon null significa que el expediente no tiene pases.</summary>
    public sealed class UltimoPaseSade
    {
        public static readonly UltimoPaseSade SinDatos = new UltimoPaseSade(null, null);

        public UltimoPaseSade(string buzon, DateTime? fecha)
        {
            Buzon = buzon;
            Fecha = fecha;
        }

        public string Buzon { get; }
        public DateTime? Fecha { get; }

        public bool TieneDatos => Buzon != null;
    }
}
