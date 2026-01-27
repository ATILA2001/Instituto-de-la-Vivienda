using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace Dominio
{
    /// <summary>
    /// Repositorio XML para almacenar y recuperar claves de protección de datos utilizando Entity Framework 6.
    /// Implementa IXmlRepository para la funcionalidad de Data Protection en ASP.NET Core.
    /// </summary>
    public class DataProtectionXmlRepository : IXmlRepository
    {
        private readonly Func<DataProtectionKeysDbContext> _contextFactory;

        public DataProtectionXmlRepository(Func<DataProtectionKeysDbContext> contextFactory) => _contextFactory = contextFactory;

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            using (var ctx = _contextFactory())
            {
                // Materialize the XML strings from the database first to avoid EF trying to translate
                // XElement.Parse into SQL (which causes NotSupportedException).
                var xmlValues = ctx.DataProtectionKeys
                                   .AsNoTracking()
                                   .Where(k => k.Xml != null)
                                   .Select(k => k.Xml)
                                   .ToList();

                return xmlValues
                       .Where(x => !string.IsNullOrWhiteSpace(x))
                       .Select(x => XElement.Parse(x))
                       .ToList();
            }
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            using (var ctx = _contextFactory())
            {
                var kp = new DataProtectionKeyEF { FriendlyName = friendlyName, Xml = element.ToString(SaveOptions.DisableFormatting) };
                ctx.DataProtectionKeys.Add(kp);
                ctx.SaveChanges();
            }
        }
    }
}