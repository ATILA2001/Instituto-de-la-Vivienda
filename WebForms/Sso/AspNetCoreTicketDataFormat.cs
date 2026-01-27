using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using OwinAuth = Microsoft.Owin.Security;

namespace WebForms.Sso
{
    public sealed class SharedCookieTicketDataFormat : OwinAuth.ISecureDataFormat<OwinAuth.AuthenticationTicket>
    {
        private readonly IDataProtector _protector;
        private static readonly DataContractSerializer Serializer = new DataContractSerializer(typeof(SharedCookieTicketDto));

        public SharedCookieTicketDataFormat(IDataProtector protector)
        {
            _protector = protector ?? throw new ArgumentNullException(nameof(protector));
        }

        public string Protect(OwinAuth.AuthenticationTicket data)
        {
            if (data == null)
            {
                return null;
            }

            var dto = SharedCookieTicketDto.FromOwin(data);
            var serialized = Serialize(dto);
            var protectedBytes = _protector.Protect(serialized);
            return Base64UrlEncode(protectedBytes);
        }

        public string Protect(OwinAuth.AuthenticationTicket data, string purpose)
        {
            return Protect(data);
        }

        public OwinAuth.AuthenticationTicket Unprotect(string protectedText)
        {
            if (string.IsNullOrWhiteSpace(protectedText))
            {
                return null;
            }

            try
            {
                var protectedBytes = Base64UrlDecode(protectedText);
                var unprotectedBytes = _protector.Unprotect(protectedBytes);
                var dto = Deserialize(unprotectedBytes);
                return dto != null ? dto.ToOwin() : null;
            }
            catch
            {
                return null;
            }
        }

        public OwinAuth.AuthenticationTicket Unprotect(string protectedText, string purpose)
        {
            return Unprotect(protectedText);
        }

        internal static OwinAuth.AuthenticationTicket TryUnprotectDetailed(IDataProtector protector, string protectedText, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(protectedText))
            {
                error = "Empty cookie value";
                return null;
            }

            byte[] protectedBytes;
            try
            {
                protectedBytes = Base64UrlDecode(protectedText);
            }
            catch (Exception ex)
            {
                error = "Base64 decode failed: " + ex.GetType().Name + " - " + ex.Message;
                return null;
            }

            byte[] unprotectedBytes;
            try
            {
                unprotectedBytes = protector.Unprotect(protectedBytes);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? " | Inner: " + ex.InnerException.GetType().Name + " - " + ex.InnerException.Message : string.Empty;
                error = "Protector.Unprotect failed: " + ex.GetType().Name + " - " + ex.Message + inner;
                return null;
            }

            try
            {
                var dto = Deserialize(unprotectedBytes);
                if (dto == null)
                {
                    error = "Deserialize returned null";
                    return null;
                }
                return dto.ToOwin();
            }
            catch (Exception ex)
            {
                error = "Deserialize failed: " + ex.GetType().Name + " - " + ex.Message;
                return null;
            }
        }

        private static byte[] Serialize(SharedCookieTicketDto dto)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.WriteObject(ms, dto);
                return ms.ToArray();
            }
        }

        private static SharedCookieTicketDto Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                return Serializer.ReadObject(ms) as SharedCookieTicketDto;
            }
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var base64 = Convert.ToBase64String(input);
            return base64.TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SharedCookie")]
    internal sealed class SharedCookieTicketDto
    {
        [DataMember(Order = 1)]
        public List<ClaimDto> Claims { get; set; }

        [DataMember(Order = 2)]
        public string AuthenticationType { get; set; }

        [DataMember(Order = 3)]
        public string NameClaimType { get; set; }

        [DataMember(Order = 4)]
        public string RoleClaimType { get; set; }

        [DataMember(Order = 5)]
        public Dictionary<string, string> Properties { get; set; }

        [DataMember(Order = 6)]
        public DateTimeOffset? IssuedUtc { get; set; }

        [DataMember(Order = 7)]
        public DateTimeOffset? ExpiresUtc { get; set; }

        [DataMember(Order = 8)]
        public bool IsPersistent { get; set; }

        [DataMember(Order = 9)]
        public bool? AllowRefresh { get; set; }

        [DataMember(Order = 10)]
        public string RedirectUri { get; set; }

        public static SharedCookieTicketDto FromOwin(OwinAuth.AuthenticationTicket ticket)
        {
            var identity = ticket.Identity;
            return new SharedCookieTicketDto
            {
                AuthenticationType = identity.AuthenticationType,
                NameClaimType = identity.NameClaimType,
                RoleClaimType = identity.RoleClaimType,
                Claims = identity.Claims.Select(ClaimDto.FromClaim).ToList(),
                Properties = new Dictionary<string, string>(ticket.Properties.Dictionary ?? new Dictionary<string, string>()),
                IssuedUtc = ticket.Properties.IssuedUtc,
                ExpiresUtc = ticket.Properties.ExpiresUtc,
                IsPersistent = ticket.Properties.IsPersistent,
                AllowRefresh = ticket.Properties.AllowRefresh,
                RedirectUri = ticket.Properties.RedirectUri
            };
        }

        public OwinAuth.AuthenticationTicket ToOwin()
        {
            var identity = new ClaimsIdentity(
                Claims != null ? Claims.Select(c => c.ToClaim()) : Enumerable.Empty<Claim>(),
                AuthenticationType,
                NameClaimType ?? ClaimTypes.Name,
                RoleClaimType ?? ClaimTypes.Role);

            var props = new OwinAuth.AuthenticationProperties(Properties ?? new Dictionary<string, string>());
            props.IssuedUtc = IssuedUtc;
            props.ExpiresUtc = ExpiresUtc;
            props.IsPersistent = IsPersistent;
            props.AllowRefresh = AllowRefresh;
            props.RedirectUri = RedirectUri;

            return new OwinAuth.AuthenticationTicket(identity, props);
        }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SharedCookie")]
    internal sealed class ClaimDto
    {
        [DataMember(Order = 1)]
        public string Type { get; set; }

        [DataMember(Order = 2)]
        public string Value { get; set; }

        [DataMember(Order = 3)]
        public string ValueType { get; set; }

        [DataMember(Order = 4)]
        public string Issuer { get; set; }

        [DataMember(Order = 5)]
        public string OriginalIssuer { get; set; }

        public static ClaimDto FromClaim(Claim claim)
        {
            return new ClaimDto
            {
                Type = claim.Type,
                Value = claim.Value,
                ValueType = claim.ValueType,
                Issuer = claim.Issuer,
                OriginalIssuer = claim.OriginalIssuer
            };
        }

        public Claim ToClaim()
        {
            return new Claim(Type, Value, ValueType ?? ClaimValueTypes.String, Issuer ?? ClaimsIdentity.DefaultIssuer, OriginalIssuer ?? ClaimsIdentity.DefaultIssuer);
        }
    }
}
