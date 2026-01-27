using System;
using System.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;
using Owin;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(WebForms.Startup))]

namespace WebForms
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var sharedCookieName = ConfigurationManager.AppSettings["SharedCookieName"] ?? ".Auth.Shared";
            var sharedAppName = ConfigurationManager.AppSettings["SharedCookieAppName"] ?? "Auth.SharedCookie";

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
                TicketDataFormat = ticketDataFormat,
                CookieSecure = CookieSecureOption.Always,
                CookiePath = ConfigurationManager.AppSettings["SharedCookiePath"] ?? "/",
                // Allow cross-site usage (Required when sharing cookie between different hostnames)
                CookieSameSite = Microsoft.Owin.SameSiteMode.None,
            });
        }
    }
}