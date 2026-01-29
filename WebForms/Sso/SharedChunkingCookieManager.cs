using System;
using System.Text;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;

namespace WebForms.Sso
{
    public sealed class SharedChunkingCookieManager : ICookieManager
    {
        private readonly ICookieManager _inner = new Microsoft.Owin.Host.SystemWeb.SystemWebChunkingCookieManager
        {
            ThrowForPartialCookies = false
        };

        public string GetRequestCookie(IOwinContext context, string key)
        {
            var value = _inner.GetRequestCookie(context, key);
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (!value.StartsWith("chunks-", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            var countText = value.Substring("chunks-".Length);
            if (!int.TryParse(countText, out var count) || count <= 0)
            {
                return value;
            }

            var builder = new StringBuilder();
            for (var i = 1; i <= count; i++)
            {
                var part = context.Request.Cookies[key + "C" + i];
                if (string.IsNullOrWhiteSpace(part))
                {
                    return value;
                }
                builder.Append(part);
            }

            return builder.ToString();
        }

        public void AppendResponseCookie(IOwinContext context, string key, string value, CookieOptions options)
        {
            _inner.AppendResponseCookie(context, key, value, options);
        }

        public void DeleteCookie(IOwinContext context, string key, CookieOptions options)
        {
            _inner.DeleteCookie(context, key, options);
        }
    }
}
