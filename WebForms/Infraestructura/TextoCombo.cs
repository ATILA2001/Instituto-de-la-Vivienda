namespace WebForms.Infraestructura
{
    /// <summary>
    /// Textos para las opciones de los combos del formulario. Un select nativo arma su lista desplegable
    /// con el ancho de la opción más larga; cuando una descripción de obra supera el ancho de la pantalla
    /// el navegador corre la lista hacia la izquierda y el principio de todas las opciones queda fuera de
    /// vista. Se acorta la parte larga (la descripción) y se deja entero el sufijo que distingue (barrio,
    /// empresa); el texto completo va en el atributo title de la opción.
    /// </summary>
    public static class TextoCombo
    {
        /// <summary>Largo máximo de la descripción dentro de una opción.</summary>
        public const int LargoDescripcion = 80;

        /// <summary>Devuelve el texto recortado a <paramref name="largo"/> caracteres con puntos suspensivos, o entero si entra.</summary>
        public static string Acortar(string texto, int largo = LargoDescripcion)
        {
            if (string.IsNullOrEmpty(texto) || texto.Length <= largo)
                return texto;

            return texto.Substring(0, largo).TrimEnd() + "…";
        }
    }
}
