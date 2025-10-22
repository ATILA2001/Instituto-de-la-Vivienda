using System;
using System.Web;
using System.Web.UI;

namespace WebForms.CustomControls
{
 /// <summary>
 /// Static toast notification utility for Bootstrap toasts.
 /// Usage: ToastService.Show(Page, "message", ToastType.Success);
 /// </summary>
 public static class ToastService
 {
 public enum ToastType
 {
 Info,
 Success,
 Warning,
 Error
 }

 /// <summary>
 /// Shows a Bootstrap toast on the given page. Duration is fixed to5 seconds.
 /// </summary>
 public static void Show(Page page, string message, ToastType type = ToastType.Info)
 {
 if (page == null)
 {
 page = HttpContext.Current?.CurrentHandler as Page;
 if (page == null) return;
 }

 string bgClass = GetBackgroundClass(type);
 string icon = GetIcon(type);
 string safeMessage = EscapeJavaScript(message ?? string.Empty);

 // Ensure a container exists, create toast element, show and remove after hidden
 string script = $@"
(function() {{
 var containerId = 'global-toast-container';
 var container = document.getElementById(containerId);
 if (!container) {{
 container = document.createElement('div');
 container.id = containerId;
 container.className = 'toast-container position-fixed start-50 translate-middle-x p-3';
 container.style.top = '80px';
 container.style.zIndex =9999;
 document.body.appendChild(container);
 }}

 var toastEl = document.createElement('div');
 toastEl.className = 'toast align-items-center border-0 text-white {bgClass}';
 toastEl.setAttribute('role', 'alert');
 toastEl.setAttribute('aria-live', 'assertive');
 toastEl.setAttribute('aria-atomic', 'true');
toastEl.style.width = 'fit-content';
    toastEl.style.maxWidth = '100%';
    toastEl.style.whiteSpace = 'nowrap';
    toastEl.style.overflow = 'hidden';
    toastEl.style.textOverflow = 'ellipsis';

 var inner = document.createElement('div');
 inner.className = 'd-flex';

 var body = document.createElement('div');
 body.className = 'toast-body fs-5 py-3 text-truncate' ;
body.style.whiteSpace = 'nowrap';
    body.style.overflow = 'hidden';
    body.style.textOverflow = 'ellipsis';
 body.innerHTML = '<i class=\'{icon} me-2\'></i>' + '{safeMessage}';

 var btn = document.createElement('button');
 btn.type = 'button';
 btn.className = 'btn-close btn-close-white me-2 m-auto';
 btn.setAttribute('data-bs-dismiss', 'toast');
 btn.setAttribute('aria-label', 'Close');

 inner.appendChild(body);
 inner.appendChild(btn);
 toastEl.appendChild(inner);
 container.appendChild(toastEl);

 var toast = new bootstrap.Toast(toastEl, {{ autohide: true, delay:5000 }});
 toast.show();

 toastEl.addEventListener('hidden.bs.toast', function() {{
 toastEl.parentNode && toastEl.parentNode.removeChild(toastEl);
 }});
}})();";

 ScriptManager.RegisterStartupScript(page, typeof(Page), "Toast_" + Guid.NewGuid(), script, true);
 }

 private static string GetBackgroundClass(ToastType type)
 {
 switch (type)
 {
 case ToastType.Success: return "bg-success";
 case ToastType.Error: return "bg-danger";
 case ToastType.Warning: return "bg-warning";
 case ToastType.Info:
 default: return "bg-info";
 }
 }

 private static string GetIcon(ToastType type)
 {
 switch (type)
 {
 case ToastType.Success: return "bi bi-check-circle-fill";
 case ToastType.Error: return "bi bi-x-circle-fill";
 case ToastType.Warning: return "bi bi-exclamation-triangle-fill";
 case ToastType.Info:
 default: return "bi bi-info-circle-fill";
 }
 }

 private static string EscapeJavaScript(string text)
 {
 return text
 .Replace("\\", "\\\\")
 .Replace("'", "\\'")
 .Replace("\"", "\\\"")
 .Replace("\n", "\\n")
 .Replace("\r", "\\r");
 }
 }
}
