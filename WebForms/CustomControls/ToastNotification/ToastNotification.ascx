<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ToastNotification.ascx.cs" Inherits="WebForms.CustomControls.ToastNotification" %>

<%-- 
  CONTROL DE NOTIFICACIONES TOAST REUTILIZABLE
    
    FUNCIONALIDAD:
- Muestra notificaciones estilo Bootstrap Toast
    - Soporte para múltiples tipos: success, error, warning, info
    - Auto-dismiss configurable
    - Posición fija en la esquina superior derecha
    
    USO:
 1. Registrar: <%@ Register Src="~/CustomControls/ToastNotification/ToastNotification.ascx" TagPrefix="CustomControls" TagName="ToastNotification" %>
    2. Agregar: <CustomControls:ToastNotification ID="toastNotification" runat="server" />
    3. En code-behind: toastNotification.ShowToast("Mensaje", "success", 3000);
--%>

<!-- Container para los toasts (posición fija en la parte inferior derecha) -->
<div class="toast-container position-fixed start-50 translate-middle-x p-3" style="top: 80px; z-index: 9999;">
    <div id="<%= ClientID %>_toast" class="toast align-items-center border-0" role="alert" aria-live="assertive" aria-atomic="true"
         style="width: fit-content; max-width: 100%; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
        <div class="d-flex">
            <div class="toast-body fs-5 py-3 text-truncate" id="<%= ClientID %>_toastBody"
                 style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
                <!-- El mensaje se inyectará aquí -->
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    </div>
</div>