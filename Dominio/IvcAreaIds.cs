namespace Dominio
{
    /// <summary>
    /// IDs de las áreas del IVC (columna Id de la tabla Areas de la base IVC).
    /// El mapeo hacia Auth.Web se configura en Areas.AuthAreaId.
    /// </summary>
    public static class IvcAreaIds
    {
        /// <summary>
        /// Área de Redeterminaciones (Areas.Id = 16).
        /// Los usuarios de esta área tienen acceso al módulo de redeterminaciones
        /// (Obras y Autorizantes), sin acceso al módulo de planificación/formulación.
        /// </summary>
        public const int Redeterminaciones = 16;

        /// <summary>
        /// Área de Secretaría (Areas.Id = 19).
        /// Los usuarios de esta área tienen acceso limitado: sin botones de acciones
        /// en Obras ni controles de planificación/formulación.
        /// </summary>
        public const int Secretaria = 19;
    }
}
