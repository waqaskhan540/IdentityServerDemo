using AspIdentityClient.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspIdentityClient.Middleware
{
    public class BasicAuthenticationMiddleware
    {
        public RequestDelegate _next;

        private readonly IdentityServerConfig _identityServerConfig;

        public BasicAuthenticationMiddleware(RequestDelegate next, IOptions<IdentityServerConfig> config)
        {
            _next = next;
            _identityServerConfig = config.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue)
            {
                if (context.Request.Path.Value.Contains("/api/"))
                {
                    var authHeader = context.Request.Headers["Authorization"];
                    if (authHeader.Count < 1)
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }

                    if (!authHeader[0].StartsWith("Basic"))
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }

                    var credString = authHeader[0].Substring(6);

                    if (string.IsNullOrWhiteSpace(credString))
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }

                    var base64bytes = Convert.FromBase64String(credString);
                    var decodedString = Encoding.UTF8.GetString(base64bytes);

                    credString = decodedString;

                    var credentials = credString.Split(':');

                    if (credentials == null || credentials.Count() < 2)
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }

                    var username = credentials[0].Trim();
                    var password = credentials[1].Trim();

                    var config_username = _identityServerConfig.ISUsername;
                    var config_password = _identityServerConfig.ISPassword;
                    if (username.Equals(config_username) && password.Equals(config_password))
                    {
                        await _next.Invoke(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 401;
                        return;
                    }

                    return;
                }
            }

            await _next.Invoke(context);
        }
    }

    public static class BasicAuthenticationExtensions
    {
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthenticationMiddleware>();
        }
    }
}