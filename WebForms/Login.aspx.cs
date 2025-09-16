using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnIniciar_Click(object sender, EventArgs e)
        {
            if (!FormatValidation(txtEmail.Text.Trim()))
            {
                // Guardar el mensaje en sesión antes de redirigir a la página de error
                Session["error"] = "Formato de CUIL o Email incorrecto.";
                Response.Redirect("Error.aspx", false);
                return;
            }

            var input = txtEmail.Text.Trim();
            var password = txtPass.Text ?? string.Empty;

            UsuarioEF usuario = null;
            try
            {
                using (var ctx = new IVCdbContext())
                {
                    var auth = new AuthenticationManager(ctx);

                    var sw = Stopwatch.StartNew();
                    var startUtc = DateTime.UtcNow;
                    Debug.WriteLine($"[{startUtc:O}] Authentication started for input='{input}'");

                    usuario = auth.Authenticate(input, password);

                    sw.Stop();
                    var endUtc = DateTime.UtcNow;
                    var resultInfo = usuario != null ? $"success.UserId={usuario.Id}" : "failure.no-user";
                    Debug.WriteLine($"[{endUtc:O}] Authentication finished for input='{input}' result={resultInfo} elapsed={sw.ElapsedMilliseconds}ms");
                }
            }
            catch (Exception)
            {
                // Guardar el mensaje de error en sesión (se puede ajustar para no exponer detalles en producción)
                Session["error"] = "Error al intentar autenticar";
                Response.Redirect("Error.aspx", false);
                return;
            }

            if (usuario != null)
            {
                if (ValidarEstado(usuario))
                {
                    Session.Add("Usuario", usuario);
                    RedirigirSegunArea(usuario);
                    return;
                }
                // Usuario encontrado pero inactivo
                Session["error"] = "Usuario inactivo. Contacte al administrador.";
                Response.Redirect("Error.aspx", false);
                return;
            }

            // Credenciales inválidas
            Session["error"] = "Usuario o contraseña inválidos.";
            Response.Redirect("Error.aspx", false);
        }


        private bool ValidarEstado(UsuarioEF usuario)
        {
            return usuario != null && usuario.Estado;
        }

        private void RedirigirSegunArea(UsuarioEF usuario)
        {
            if (usuario != null && usuario.Area != null && usuario.Area.Id == 16)
            {
                Response.Redirect("RedeterminacionesEF.aspx", false);
            }
            else
            {
                Response.Redirect("CertificadosEF.aspx", false);
            }
        }

        private bool FormatValidation(string input)
        {
            input = (input ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input)) return false;

            // Si parece un correo, validar con System.Net.Mail.MailAddress (más robusto que un regex simple)
            if (input.Contains("@"))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(input);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            // Sino, validar CUIT/CUIL usando el algoritmo módulo 11 (verifica dígito verificador)
            return ValidateCuitCuil(input);
        }

        // Valida un CUIT/CUIL (acepta dígitos y guiones). Algoritmo: multiplicadores 5,4,3,2,7,6,5,4,3,2 y comprobación módulo 11.
        private bool ValidateCuitCuil(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            var digits = new string(input.Where(char.IsDigit).ToArray());
            if (digits.Length != 11) return false;

            int[] weights = new int[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += (digits[i] - '0') * weights[i];
            }

            int mod = sum % 11;
            int dv = 11 - mod;
            if (dv == 11) dv = 0;
            // Si dv == 10 el número es inválido según la regla estándar
            if (dv == 10) return false;

            int provided = digits[10] - '0';
            return dv == provided;
        }

    }
}