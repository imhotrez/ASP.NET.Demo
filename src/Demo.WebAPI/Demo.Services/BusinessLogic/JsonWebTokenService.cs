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

namespace Demo.Services.BusinessLogic {
    public class JsonWebTokenService {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public JsonWebTokenService(IConfiguration configuration, UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager) {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<string> GetToken(AppUser appUser, CancellationToken cancellationToken,
            bool encryptCredentials = false) {
            var utcNow = DateTime.UtcNow;

            #region SigningCredentials

            var keyPath = Path.Combine(
                path1: Directory.GetCurrentDirectory(),
                path2: "Keys",
                path3: _configuration.GetValue<string>("Tokens:SigningPrivateKey"));

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
                path3: _configuration.GetValue<string>("Tokens:EncodingSecretKey"));

            var key = File.ReadAllText(keyPath);
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var encryptingCredentials = new EncryptingCredentials(
                key: secretKey,
                alg: JwtConstants.DirectKeyUseAlg,
                enc: SecurityAlgorithms.Aes256CbcHmacSha512);

            #endregion

            #region Claims

            var userRoles = await _userManager.GetRolesAsync(user: appUser);
            var allRoles = await _roleManager.Roles.ToArrayAsync(cancellationToken);
            var dic = allRoles.ToDictionary(x => x.Name, y => userRoles.Contains(y.Name));
            var claim = new Claim("roles", JsonConvert.SerializeObject(dic));
            // var claims = allRoles.Select(x => new Claim(x.Name, userRoles.Contains(x.Name).ToString())).ToList();

            //var claims = new List<Claim> { new Claim(JwtRegisteredClaimNames.UniqueName, appUser.UserName) };

            //var identityClaims = appUser.Claims?
            //    .Where(c => c?.ClaimType != null && c.ClaimValue != null).ToArray();

            //if (identityClaims?.Any() == true)
            //{
            //    claims.AddRange(identityClaims.Select(c => new Claim(c.ClaimType, c.ClaimValue)));
            //}

            #endregion

            #region Token

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = encryptCredentials
                ? tokenHandler.CreateJwtSecurityToken(
                    issuer: _configuration.GetValue<string>("Tokens:Issuer"),
                    audience: _configuration.GetValue<string>("Tokens:Audience"),
                    subject: new ClaimsIdentity(new List<Claim> {claim}),
                    notBefore: utcNow,
                    expires: utcNow.AddSeconds(_configuration.GetValue<int>("Tokens:AccessTokenLifetime")),
                    issuedAt: utcNow,
                    signingCredentials: signingCredentials,
                    encryptingCredentials: encryptingCredentials
                )
                : tokenHandler.CreateJwtSecurityToken(
                    issuer: _configuration.GetValue<string>("Tokens:Issuer"),
                    audience: _configuration.GetValue<string>("Tokens:Audience"),
                    subject: new ClaimsIdentity(new List<Claim> {claim}),
                    notBefore: utcNow,
                    expires: utcNow.AddSeconds(_configuration.GetValue<int>("Tokens:AccessTokenLifetime")),
                    issuedAt: utcNow,
                    signingCredentials: signingCredentials
                );

            var jwtToken = tokenHandler.WriteToken(token);
            return jwtToken;

            #endregion
        }
    }
}