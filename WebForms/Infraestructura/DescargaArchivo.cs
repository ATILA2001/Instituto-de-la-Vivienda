using System.Web;

namespace WebForms.Infraestructura
{
    /// <summary>Envía al navegador un archivo generado en memoria y termina la respuesta.</summary>
    public static class DescargaArchivo
    {
        private const string ContentTypeXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>Descarga un libro Excel (.xlsx). El nombre se envía sin extensión.</summary>
        public static void Excel(HttpResponse response, byte[] contenido, string nombreSinExtension)
        {
            Enviar(response, contenido, nombreSinExtension + ".xlsx", ContentTypeXlsx);
        }

        public static void Enviar(HttpResponse response, byte[] contenido, string nombreArchivo, string contentType)
        {
            response.Clear();
            response.ContentType = contentType;
            response.AddHeader("content-disposition", "attachment;  filename=" + nombreArchivo);
            response.BinaryWrite(contenido);
            response.End();
        }
    }
}
