using System;
using System.Web.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security;
using Owin;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(WebForms.Startup))]

namespace WebForms
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var sharedCookieName = Environment.GetEnvironmentVariable("SharedCookie__Name")
                ?? WebConfigurationManager.AppSettings["SharedCookieName"];
            if (string.IsNullOrEmpty(sharedCookieName))
                throw new InvalidOperationException(
                    "La cookie compartida no está configurada. " +
                    "Defina la variable de entorno 'SharedCookie__Name' o el appSetting 'SharedCookieName'.");

            var sharedAppName = Environment.GetEnvironmentVariable("SharedCookie__ApplicationName")
                ?? WebConfigurationManager.AppSettings["SharedCookieAppName"];
            if (string.IsNullOrEmpty(sharedAppName))
                throw new InvalidOperationException(
                    "El nombre de aplicación compartido no está configurado. " +
                    "Defina la variable de entorno 'SharedCookie__ApplicationName' o el appSetting 'SharedCookieAppName'.");

            app.SetDefaultSignInAsAuthenticationType("Identity.Application");

            // Build a transient DI container to create DataProtection provider with our EF6-backed IXmlRepository
            var services = new ServiceCollection();

            // Register the EF6 IXmlRepository (uses DataProtectionKeysDbContext)
            var xmlRepo = new Dominio.DataProtectionXmlRepository(() => new Dominio.DataProtectionKeysDbContext());
            services.AddSingleton<IXmlRepository>(sp => xmlRepo);

            // Configure DataProtection and set application name
            services.AddDataProtection()
                .SetApplicationName(sharedAppName);

            // Ensure key manager uses our EF6-backed IXmlRepository
            services.Configure<KeyManagementOptions>(opts =>
            {
                opts.XmlRepository = xmlRepo;
                // Prevent WebForms from generating new keys in the shared key ring
                opts.AutoGenerateKeys = false;
            });

            // Build final service provider and obtain the IDataProtectionProvider
            var serviceProvider = services.BuildServiceProvider();
            var dpProvider = serviceProvider.GetService(typeof(IDataProtectionProvider)) as IDataProtectionProvider;

            // Create protector compatible with ASP.NET Core cookie middleware
            var protector = dpProvider.CreateProtector(
                "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                "Identity.Application",
                "v2");

            var ticketDataFormat = new WebForms.Sso.SharedCookieTicketDataFormat(protector);

            // Configure OWIN cookie authentication to validate the shared cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                CookieName = sharedCookieName,
                AuthenticationType = "Identity.Application",
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                TicketDataFormat = ticketDataFormat,
                CookieManager = new WebForms.Sso.SharedChunkingCookieManager(),
                CookieSecure = CookieSecureOption.Always,
                CookiePath = Environment.GetEnvironmentVariable("SharedCookie__Path")
                    ?? WebConfigurationManager.AppSettings["SharedCookiePath"] ?? "/",
                // Allow cross-site usage (Required when sharing cookie between different hostnames)
                CookieSameSite = Microsoft.Owin.SameSiteMode.None,
            });
        }
    }
}