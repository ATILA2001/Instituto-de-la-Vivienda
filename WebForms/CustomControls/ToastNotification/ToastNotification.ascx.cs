using System;
using System.Web.UI;

namespace WebForms.CustomControls
{
    /// <summary>
    /// Control reutilizable para mostrar notificaciones Toast de Bootstrap
    /// </summary>
    public partial class ToastNotification : UserControl
    {
        /// <summary>
        /// Muestra un toast con el mensaje y tipo especificados.
        /// DEPRECATED: use ToastService.Show(Page, message, ToastService.ToastType.X) en su lugar.
        /// Mantiene compatibilidad llamando al nuevo servicio con duración fija de5s.
        /// </summary>
        public void ShowToast(string message, string type = "info", int duration = 3000)
        {
            // Map string type to enum and delegate to ToastService
            ToastService.ToastType mapped = MapType(type);
            ToastService.Show(Page, message, mapped);
        }

        private ToastService.ToastType MapType(string type)
        {
            switch ((type ?? string.Empty).ToLower())
            {
                case "success": return ToastService.ToastType.Success;
                case "error":
                case "danger": return ToastService.ToastType.Error;
                case "warning": return ToastService.ToastType.Warning;
                case "info":
                default: return ToastService.ToastType.Info;
            }
        }
    }
}
