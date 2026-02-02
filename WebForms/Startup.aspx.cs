using System;
using System.Linq;
using System.Web.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Web;
using System.Web.Script.Serialization;

namespace WebForms
{
    public partial class StartupPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var user = Context.GetOwinContext().Authentication.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                if (Context?.IsDebuggingEnabled != true)
                {
                    Response.Redirect(BuildAuthLoginUrl(Context), true);
                    return;
                }

                var diagHtml = new System.Text.StringBuilder();
                diagHtml.AppendLine("<h3>No autenticado. Inicie sesión desde el portal de autenticación.</h3>");
                diagHtml.AppendLine("<h4>Diagnóstico</h4><ul>");
                diagHtml.AppendLine($"<li><strong>Servidor</strong>: {HttpUtility.HtmlEncode(Environment.MachineName)}</li>");
                diagHtml.AppendLine($"<li><strong>Fecha/Hora (UTC)</strong>: {HttpUtility.HtmlEncode(DateTime.UtcNow.ToString("O"))}</li>");
                diagHtml.AppendLine($"<li><strong>Request URL</strong>: {HttpUtility.HtmlEncode(Request?.Url?.ToString())}</li>");
                diagHtml.AppendLine($"<li><strong>Auth.Type</strong>: {HttpUtility.HtmlEncode(user?.Identity?.AuthenticationType)}</li>");
                diagHtml.AppendLine($"<li><strong>IsAuthenticated</strong>: {HttpUtility.HtmlEncode((user?.Identity?.IsAuthenticated ?? false).ToString())}</li>");
                diagHtml.AppendLine($"<li><strong>Identity.Name</strong>: {HttpUtility.HtmlEncode(user?.Identity?.Name)}</li>");
                diagHtml.AppendLine($"<li><strong>Claims.Count</strong>: {HttpUtility.HtmlEncode((user?.Claims?.Count() ?? 0).ToString())}</li>");

                var authManager = Context.GetOwinContext()?.Authentication;
                var authTypes = authManager?.GetAuthenticationTypes()?.Select(t => t.AuthenticationType).ToArray() ?? Array.Empty<string>();
                diagHtml.AppendLine($"<li><strong>Auth Schemes</strong>: {HttpUtility.HtmlEncode(string.Join(", ", authTypes))}</li>");

                if (authManager != null)
                {
                    try
                    {
                        var ticket = authManager.AuthenticateAsync("Identity.Application").GetAwaiter().GetResult();
                        var ticketAuth = ticket?.Identity?.IsAuthenticated ?? false;
                        var ticketName = ticket?.Identity?.Name;
                        var ticketClaims = ticket?.Identity?.Claims?.Count() ?? 0;
                        diagHtml.AppendLine($"<li><strong>AuthTicket</strong>: isAuthenticated={ticketAuth}, name={HttpUtility.HtmlEncode(ticketName)}, claims={ticketClaims}</li>");

                        // AuthTicket2 removed (AuthenticateAsync overload not available in this OWIN version)
                    }
                    catch (Exception ex)
                    {
                        diagHtml.AppendLine($"<li><strong>AuthTicket</strong>: error - {HttpUtility.HtmlEncode(ex.GetType().Name + ": " + ex.Message)}</li>");
                    }
                }

                var cookieNames = Request?.Cookies?.AllKeys ?? Array.Empty<string>();
                diagHtml.AppendLine($"<li><strong>Cookies</strong>: {HttpUtility.HtmlEncode(string.Join(", ", cookieNames))}</li>");

                try
                {
                    var sharedCookieName = WebConfigurationManager.AppSettings["SharedCookieName"] ?? ".Auth.Shared";
                    var owinCtx = Context.GetOwinContext();
                    var cookieManager = new ChunkingCookieManager { ThrowForPartialCookies = false };
                    var cookieValue = cookieManager.GetRequestCookie(owinCtx, sharedCookieName);
                    var manualCookieValue = cookieValue;

                    if (string.IsNullOrWhiteSpace(cookieValue))
                    {
                        diagHtml.AppendLine($"<li><strong>SharedCookie</strong>: {HttpUtility.HtmlEncode(sharedCookieName)} (no encontrada)</li>");
                    }
                    else
                    {
                        diagHtml.AppendLine($"<li><strong>SharedCookie</strong>: {HttpUtility.HtmlEncode(sharedCookieName)} (len={cookieValue.Length})</li>");
                        if (cookieValue.StartsWith("chunks-", StringComparison.OrdinalIgnoreCase))
                        {
                            var c1 = Request.Cookies[sharedCookieName + "C1"]?.Value;
                            var c2 = Request.Cookies[sharedCookieName + "C2"]?.Value;
                            diagHtml.AppendLine($"<li><strong>SharedCookieChunks</strong>: C1={(c1 == null ? "(null)" : c1.Length.ToString())}, C2={(c2 == null ? "(null)" : c2.Length.ToString())}</li>");
                            if (int.TryParse(cookieValue.Substring("chunks-".Length), out var chunkCount) && chunkCount > 0)
                            {
                                var chunks = new System.Text.StringBuilder();
                                var missing = new System.Collections.Generic.List<int>();
                                for (var i = 1; i <= chunkCount; i++)
                                {
                                    var chunk = Request.Cookies[sharedCookieName + "C" + i]?.Value;
                                    if (string.IsNullOrWhiteSpace(chunk))
                                    {
                                        missing.Add(i);
                                    }
                                    else
                                    {
                                        chunks.Append(chunk);
                                    }
                                }

                                if (missing.Count > 0)
                                {
                                    diagHtml.AppendLine($"<li><strong>SharedCookieChunks</strong>: missing={string.Join(",", missing)}</li>");
                                }
                                else
                                {
                                    manualCookieValue = chunks.ToString();
                                    diagHtml.AppendLine($"<li><strong>SharedCookieChunks</strong>: manual.len={manualCookieValue.Length}</li>");
                                }
                            }
                        }

                        var sharedAppName = WebConfigurationManager.AppSettings["SharedCookieAppName"] ?? "Auth.SharedCookie";
                        var services = new ServiceCollection();
                        var xmlRepo = new Dominio.DataProtectionXmlRepository(() => new Dominio.DataProtectionKeysDbContext());
                        services.AddSingleton<IXmlRepository>(sp => xmlRepo);
                        services.AddDataProtection().SetApplicationName(sharedAppName);
                        services.Configure<KeyManagementOptions>(opts =>
                        {
                            opts.XmlRepository = xmlRepo;
                            opts.AutoGenerateKeys = false;
                        });

                        var serviceProvider = services.BuildServiceProvider();
                        var dpProvider = serviceProvider.GetService(typeof(IDataProtectionProvider)) as IDataProtectionProvider;
                        if (dpProvider == null)
                        {
                            diagHtml.AppendLine("<li><strong>SharedCookie</strong>: DataProtectionProvider no disponible</li>");
                        }
                        else
                        {
                            var protector = dpProvider.CreateProtector(
                                "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                                "Identity.Application",
                                "v2");

                            var ticket = WebForms.Sso.SharedCookieTicketDataFormat.TryUnprotectDetailed(protector, manualCookieValue, out var error);
                            if (ticket == null)
                            {
                                diagHtml.AppendLine($"<li><strong>SharedCookie</strong>: Unprotect FAILED - {HttpUtility.HtmlEncode(error ?? "(sin detalle)")}</li>");
                            }
                            else
                            {
                                var count = ticket.Identity?.Claims?.Count() ?? 0;
                                var issued = ticket.Properties?.IssuedUtc?.ToString("O") ?? "(null)";
                                var expires = ticket.Properties?.ExpiresUtc?.ToString("O") ?? "(null)";
                                var authType = ticket.Identity?.AuthenticationType ?? "(null)";
                                diagHtml.AppendLine($"<li><strong>SharedCookie</strong>: Unprotect OK (claims={count}, authType={HttpUtility.HtmlEncode(authType)}, issued={HttpUtility.HtmlEncode(issued)}, expires={HttpUtility.HtmlEncode(expires)})</li>");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    diagHtml.AppendLine($"<li><strong>SharedCookie</strong>: Error diagnóstico - {HttpUtility.HtmlEncode(ex.GetType().Name + ": " + ex.Message)}</li>");
                }

                diagHtml.AppendLine("</ul>");
                LitMessage.Text = diagHtml.ToString();
                return;
            }


            var returnUrl = Request.QueryString["returnUrl"];

            var permsJsonTmp = user.Claims.FirstOrDefault(c => c.Type == "perms_json")?.Value;
            var firstPage = TryGetFirstPageFromPermissions(permsJsonTmp);

            if (!string.IsNullOrWhiteSpace(firstPage) && Context?.IsDebuggingEnabled != true)
            {
                var targetUrl = NormalizeRedirectUrl(firstPage);
                Response.Redirect(targetUrl, true);
                return;
            }

            var claimsHtml = new System.Text.StringBuilder();
            claimsHtml.AppendLine("<h3>Detalle de claims del usuario</h3>");

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                claimsHtml.AppendLine($"<p><strong>returnUrl</strong>: {HttpUtility.HtmlEncode(returnUrl)}</p>");
            }

            if (!string.IsNullOrWhiteSpace(firstPage))
            {
                claimsHtml.AppendLine($"<p><strong>first_page</strong>: {HttpUtility.HtmlEncode(firstPage)}</p>");
            }

            var expectedClaimTypes = new[]
            {
                ClaimTypes.NameIdentifier,
                ClaimTypes.Name,
                ClaimTypes.Email,
                ClaimTypes.Role,
                "area",
                "app",
                "perms_json"
            };

            var groupedClaims = user.Claims
                .GroupBy(c => c.Type, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(c => c.Value).ToList(), StringComparer.OrdinalIgnoreCase);

            claimsHtml.AppendLine("<h4>Todos los claims (incluye vacíos)</h4><ul>");

            foreach (var claimType in expectedClaimTypes)
            {
                if (groupedClaims.TryGetValue(claimType, out var values) && values.Count > 0)
                {
                    foreach (var value in values)
                    {
                        claimsHtml.AppendLine($"<li><strong>{HttpUtility.HtmlEncode(claimType)}</strong>: {HttpUtility.HtmlEncode(value)}</li>");
                    }
                }
                else
                {
                    claimsHtml.AppendLine($"<li><strong>{HttpUtility.HtmlEncode(claimType)}</strong>: <em>(vacío)</em></li>");
                }
            }

            foreach (var claim in user.Claims)
            {
                if (expectedClaimTypes.Contains(claim.Type, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                claimsHtml.AppendLine($"<li><strong>{HttpUtility.HtmlEncode(claim.Type)}</strong>: {HttpUtility.HtmlEncode(claim.Value)}</li>");
            }

            claimsHtml.AppendLine("</ul>");

            LitMessage.Text = claimsHtml.ToString();
        }

        private static string TryGetFirstPageFromPermissions(string permsJson)
        {
            if (string.IsNullOrWhiteSpace(permsJson))
            {
                return null;
            }

            try
            {
                var serializer = new JavaScriptSerializer();
                var payload = serializer.Deserialize<PermissionsPayload>(permsJson);
                return payload?.pages?.FirstOrDefault()?.url;
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeRedirectUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "/";
            }

            var trimmed = url.Trim();
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absolute))
            {
                return absolute.ToString();
            }

            if (trimmed.StartsWith("~/", StringComparison.Ordinal))
            {
                return VirtualPathUtility.ToAbsolute(trimmed);
            }

            return trimmed.StartsWith("/", StringComparison.Ordinal) ? trimmed : "/" + trimmed;
        }

        private static string BuildAuthLoginUrl(HttpContext context)
        {
            var baseUrl = WebConfigurationManager.AppSettings["AuthWebBaseUrl"]
                ?? WebConfigurationManager.AppSettings["AuthWebUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return "/Account/Login";
            }

            baseUrl = baseUrl.Trim().TrimEnd('/');
            return baseUrl + "/Account/Login";
        }


        private sealed class PermissionsPayload
        {
            public PagePermission[] pages { get; set; }
        }

        private sealed class PagePermission
        {
            public string url { get; set; }
            public string[] actions { get; set; }
        }
    }
}