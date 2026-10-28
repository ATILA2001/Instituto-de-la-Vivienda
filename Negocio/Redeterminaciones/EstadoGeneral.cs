namespace Negocio.Redeterminaciones
{
    /// <summary>
    /// Estado general con el que se muestra una redeterminación en la grilla de Autorizantes.
    /// Son cuatro valores fijos de la aplicación, no filas de ESTADOS_AUTORIZANTES; se exponen con
    /// Id y Nombre para que las grillas y filtros los bindeen igual que a los estados reales.
    /// </summary>
    public readonly struct EstadoGeneral
    {
        public static readonly EstadoGeneral Aprobado = new EstadoGeneral(1, "APROBADO");
        public static readonly EstadoGeneral EnTramite = new EstadoGeneral(2, "EN TRAMITE");
        public static readonly EstadoGeneral Desestimado = new EstadoGeneral(3, "DESESTIMADO");
        public static readonly EstadoGeneral NoIniciado = new EstadoGeneral(4, "NO INICIADO");

        private EstadoGeneral(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }

        public int Id { get; }
        public string Nombre { get; }

        public override string ToString() => Nombre;
    }
}
