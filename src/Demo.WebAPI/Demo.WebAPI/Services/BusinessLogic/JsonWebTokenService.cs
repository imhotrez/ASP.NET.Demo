using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Demo.Helpers;
using Demo.Models.Domain.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using JwtConstants = Microsoft.IdentityModel.JsonWebTokens.JwtConstants;

namespace Demo.WebAPI.Services.BusinessLogic {
    public class JsonWebTokenService {
        private readonly IConfiguration configuration;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<AppRole> roleManager;

        public JsonWebTokenService(IConfiguration configuration, UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager) {
            this.configuration = configuration;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public string GetClaimValue(string jwt, string claimKey) {
            var signingPublicKeyPath = Path.Combine(
                path1: Directory.GetCurrentDirectory(),
                path2: "Keys",
                path3: configuration["Tokens:SigningPublicKey"]);

            var publicRsa = RSA.Create(2048);

            publicRsa.FromXmlFile(signingPublicKeyPath);

            var signingPublicKey = new RsaSecurityKey(publicRsa);
            
            var tokenValidationParameters = new TokenValidationParameters {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingPublicKey,
                ValidateIssuer = true,
                ValidIssuer = configuration["Tokens:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Tokens:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(5)
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(jwt, tokenValidationParameters, out _);
            var result = principal.Claims.FirstOrDefault(c => c.Type == claimKey)?.Value; 
            return result;
        }

        public async Task<string> GetToken(AppUser appUser, CancellationToken cancellationToken,
            bool encryptCredentials = false) {
            var utcNow = DateTime.UtcNow;

            #region SigningCredentials

            var keyPath = Path.Combine(
                path1: Directory.GetCurrentDirectory(),
                path2: "Keys",
                path3: configuration.GetValue<string>("Tokens:SigningPrivateKey"));

            var privateRsa = RSA.Create(2048);

            privateRsa.FromXmlFile(keyPath);

            var privateKey = new RsaSecurityKey(privateRsa);

            var signingCredentials = new SigningCredentials(
                key: privateKey,
                algorithm: SecurityAlgorithms.RsaSha256);

            #endregion

            #region EncryptingCredentials

            keyPath = Path.Combine(
                path1: Directory.GetCurrentDirectory(),
                path2: "Keys",
                path3: configuration.GetValue<string>("Tokens:EncodingSecretKey"));

            var key = await File.ReadAllTextAsync(keyPath, cancellationToken);
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var encryptingCredentials = new EncryptingCredentials(
                key: secretKey,
                alg: JwtConstants.DirectKeyUseAlg,
                enc: SecurityAlgorithms.Aes256CbcHmacSha512);

            #endregion

            #region Claims

            var userRoles = await userManager.GetRolesAsync(user: appUser);
            var allRoles = await roleManager.Roles.ToArrayAsync(cancellationToken);
            var dic = allRoles.ToDictionary(x => x.Name, y => userRoles.Contains(y.Name));

            var claims = new List<Claim> {
                new("UserId", appUser.Id.ToString()),
                new("Email", appUser.Email),
                new("roles", JsonConvert.SerializeObject(dic))
            };

            if (!string.IsNullOrEmpty(appUser.LastName)) {
                claims.Add(new Claim("LastName", appUser.LastName));  
            }
            
            if (!string.IsNullOrEmpty(appUser.FirstName)) {
                claims.Add(new Claim("FirstName", appUser.FirstName));  
            }
            
            if (!string.IsNullOrEmpty(appUser.MiddleName)) {
                claims.Add(new Claim("MiddleName", appUser.MiddleName));  
            }

            var identityClaims = appUser.Claims?.Where(c => c?.ClaimType != null && c.ClaimValue != null).ToArray();

            if (identityClaims?.Any() == true) {
                claims.AddRange(identityClaims.Select(c => new Claim(c.ClaimType, c.ClaimValue)));
            }

            #endregion

            #region Token

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = encryptCredentials
                ? tokenHandler.CreateJwtSecurityToken(
                    issuer: configuration.GetValue<string>("Tokens:Issuer"),
                    audience: configuration.GetValue<string>("Tokens:Audience"),
                    subject: new ClaimsIdentity(claims),
                    notBefore: utcNow,
                    expires: utcNow.AddSeconds(configuration.GetValue<int>("Tokens:AccessTokenLifetime")),
                    issuedAt: utcNow,
                    signingCredentials: signingCredentials,
                    encryptingCredentials: encryptingCredentials
                )
                : tokenHandler.CreateJwtSecurityToken(
                    issuer: configuration.GetValue<string>("Tokens:Issuer"),
                    audience: configuration.GetValue<string>("Tokens:Audience"),
                    subject: new ClaimsIdentity(claims),
                    notBefore: utcNow,
                    expires: utcNow.AddSeconds(configuration.GetValue<int>("Tokens:AccessTokenLifetime")),
                    issuedAt: utcNow,
                    signingCredentials: signingCredentials
                );

            var jwtToken = tokenHandler.WriteToken(token);
            return jwtToken;

            #endregion
        }
    }
}