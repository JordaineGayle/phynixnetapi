using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace phynixnetapi.Helpers
{
	public static class AuthHelper
	{

        private static bool ValidateToken(string authToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            SecurityToken validatedToken;
            IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
            return true;
        }

        public static bool IsValidToken(this HttpRequest http)
        {
            if(http.Headers == null)
            {
                throw new Exception("Please specify a token for this request");
            }


            if (http.Headers.TryGetValue("Authorization", out StringValues s))
            {

                string to = s[0];

                string newToken = to.Replace("Bearer", "").TrimEnd().TrimStart();

                var valid = ValidateToken(newToken);

                if (valid)
                {
                    return true;
                }
            }

            throw new Exception("Invalid Token");
        }

        //public static bool IsValidToken(this HttpRequestMessage http)
        //{
        //    if (http.Headers == null)
        //    {
        //        throw new Exception("Please specify a token for this request");
        //    }


        //    if (http.Headers.Authorization.G("Authorization", out StringValues s))
        //    {

        //        string to = s[0];

        //        string newToken = to.Replace("Bearer", "").TrimEnd().TrimStart();

        //        var valid = ValidateToken(newToken);

        //        if (valid)
        //        {
        //            return true;
        //        }
        //    }

        //    throw new Exception("Invalid Token");
        //}

        private static TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = false,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidIssuer = "Phynix",
                ValidAudience = "Phynix Inc",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("Secret")))
            };
        }
    }
}
