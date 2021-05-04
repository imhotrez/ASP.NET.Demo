using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.Models.Domain.Auth;
using Demo.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EmailService = Demo.WebAPI.Services.BusinessLogic.EmailService;
using JsonWebTokenService = Demo.WebAPI.Services.BusinessLogic.JsonWebTokenService;
using PassGenService = Demo.WebAPI.Services.BusinessLogic.PassGenService;
using RefreshSessionService = Demo.WebAPI.Services.DataAccess.RefreshSessionService;

namespace Demo.WebAPI.Controllers {
    [Produces("application/json")]
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class AuthController : Controller {
        private readonly IConfiguration _configuration;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly JsonWebTokenService _jsonWebTokenService;
        private readonly RefreshSessionService _refreshSessionService;
        private readonly EmailService _emailService;
        private readonly PassGenService _passGenService;

        public AuthController(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            JsonWebTokenService jsonWebTokenService,
            RefreshSessionService refreshSessionService,
            IConfiguration configuration,
            EmailService emailService,
            PassGenService passGenService) {
            _signInManager = signInManager;
            _userManager = userManager;
            _jsonWebTokenService = jsonWebTokenService;
            _refreshSessionService = refreshSessionService;
            _configuration = configuration;
            _emailService = emailService;
            _passGenService = passGenService;
        }

        private async Task<Response<AuthResponse>> BuildAuthResponse(
            AppUser appUser,
            string fingerprint,
            int expiresIn,
            CancellationToken cancellationToken,
            bool rememberMe = true) {
            var authResponse = new AuthResponse {
                AccessToken = await _jsonWebTokenService.GetToken(appUser, cancellationToken)
            };

            if (!rememberMe) return new Response<AuthResponse>(authResponse);

            var refreshSession = new RefreshSession {
                UserId = appUser.Id,
                Fingerprint = fingerprint,
                RefreshToken = Guid.NewGuid(),
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString(),
                ExpiresIn = DateTime.UtcNow.AddSeconds(expiresIn),
            };

            refreshSession = await _refreshSessionService.Save(refreshSession, cancellationToken);

            var cookieOptions = new CookieOptions {
                HttpOnly = true,
                Domain = $"{_configuration.GetValue<string>("DomainName")}",
                Path = "/api/auth",
                SameSite = SameSiteMode.Strict,
                Expires = refreshSession.ExpiresIn
            };

            HttpContext.Response.Cookies.Append(
                key: "refreshToken",
                value: refreshSession.RefreshToken.ToString(),
                options: cookieOptions);

            return new Response<AuthResponse>(authResponse);
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<ActionResult<Response<AuthResponse>>> Login(LoginRequest loginRequest,
            CancellationToken cancellationToken) {
            var authResponse = await LoginProcess(loginRequest, cancellationToken);
            HttpContext.Response.Cookies.Delete(".AspNetCore.Identity.Application");
            return authResponse;
        }

        private async Task<ActionResult<Response<AuthResponse>>> LoginProcess(LoginRequest loginRequest,
            CancellationToken cancellationToken) {
            if (!ModelState.IsValid) throw new Exception("Отправленные данные не валидны");

            var loginResult = await _signInManager.PasswordSignInAsync(
                userName: loginRequest.Email,
                password: loginRequest.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (!loginResult.Succeeded) throw new Exception("Неверное имя пользователя или пароль");

            var user = await _userManager.FindByEmailAsync(loginRequest.Email);

            var refreshTokenLifetime = _configuration.GetValue<string>("Tokens:RefreshTokenLifetime");

            if (!int.TryParse(refreshTokenLifetime, out var expiresIn)) expiresIn = 86400 * 30;

            var authResponse = await BuildAuthResponse(
                appUser: user,
                fingerprint: loginRequest.FingerPrint,
                expiresIn: expiresIn,
                cancellationToken: cancellationToken,
                rememberMe: loginRequest.RememberMe);

            return Ok(authResponse);
        }

        [Authorize]
        [HttpPost]
        [Route("refresh")]
        public async Task<ActionResult<Response<AuthResponse>>> RefreshToken(RefreshTokenRequest refreshTokenRequest,
            CancellationToken cancellationToken) {
            if (!HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken) ||
                string.IsNullOrEmpty(refreshToken)) {
                throw new Exception("Требуется повторная аутентификация");
            }

            var session = await _refreshSessionService.GetSessionByGuid(refreshToken, cancellationToken);

            if (session == null) {
                var cookieOptions = new CookieOptions {
                    HttpOnly = true,
                    Domain = $"{_configuration.GetValue<string>("DomainName")}",
                    Path = "/api/auth",
                    SameSite = SameSiteMode.Strict,
                    //Expires = refreshSession.ExpiresIn
                };

                HttpContext.Response.Cookies.Append(
                    key: "refreshToken",
                    value: "",
                    options: cookieOptions);

                throw new Exception("Требуется повторная аутентификация");
            }

            await _refreshSessionService.Remove(session, cancellationToken);

            if (session.ExpiresIn < DateTime.UtcNow) {
                //throw new Exception("Время жизни refresh-токена истекло");
                throw new Exception("Требуется повторная аутентификация");
            }

            if (session.Fingerprint != refreshTokenRequest.FingerPrint) {
                //throw new Exception("Отпечаток не распознан");
                throw new Exception("Требуется повторная аутентификация");
            }

            //TODO Обработать айпишник
            //TODO Обработать User-Agent

            var refreshTokenLifetime = _configuration.GetValue<string>("Tokens:RefreshTokenLifetime");
            if (!int.TryParse(refreshTokenLifetime, out var expiresIn)) expiresIn = 86400 * 30;

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == session.UserId, cancellationToken);

            if (user == null) throw new Exception("Пользователь не найден");

            var authResponse = await BuildAuthResponse(
                appUser: user,
                fingerprint: refreshTokenRequest.FingerPrint,
                expiresIn: expiresIn,
                cancellationToken: cancellationToken);

            return Ok(authResponse);

            //var result = new AuthResponse
            //{
            //    AccessToken = _jsonWebTokenService.GetToken(user),
            //    RefreshToken = refreshSession.RefreshToken.ToString()
            //};

            //var username = User.Identity.Name ??
            //               User.Claims
            //                   .Where(c => c.Properties.ContainsKey("unique_name"))
            //                   .Select(c => c.Value)
            //                   .FirstOrDefault();

            //if (!string.IsNullOrWhiteSpace(username))
            //{
            //    var user = await _userManager.FindByNameAsync(username);
            //    return Ok(_jsonWebTokenService.GetToken(user));
            //}

            //ModelState.AddModelError("Authentication", "Authentication failed!");
            //return BadRequest(ModelState);
        }

        [Authorize]
        [Route("logout")]
        [HttpGet]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken) {
            if (!HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken) ||
                string.IsNullOrEmpty(refreshToken)) {
                throw new Exception("Требуется повторная аутентификация");
            }

            var session = await _refreshSessionService.GetSessionByGuid(refreshToken, cancellationToken);
            if (session == null) {
                throw new Exception("Требуется повторная аутентификация");
            }

            await _refreshSessionService.Remove(session, cancellationToken);
            return Ok(ModelState);
        }

        /// <summary>
        /// Выйти со всех устройств кроме текущего
        /// </summary>
        /// <param name="cancellationToken">Токен для отмены</param>
        /// <returns></returns>
        [Route("totalLogout")]
        public async Task<IActionResult> TotalLogout(CancellationToken cancellationToken) {
            if (!HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken) ||
                string.IsNullOrEmpty(refreshToken)) {
                ModelState.AddModelError("Authentication", "Требуется повторная аутентификация");
                return BadRequest(ModelState);
            }

            await _refreshSessionService.TotalLogout(refreshToken, cancellationToken);
            return Ok(ModelState);
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<ActionResult<Response<AuthResponse>>> Register([FromBody] RegisterRequest registerRequest,
            CancellationToken cancellationToken) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = new AppUser() {
                // TODO: Use Automapper instaed of manual binding
                UserName = registerRequest.Email,
                Email = registerRequest.Email
            };

            var identityResult = await _userManager.CreateAsync(user, registerRequest.Password);
            if (!identityResult.Succeeded)
                return BadRequest(new Response<AuthResponse>(null,
                    identityResult.Errors.Select(x => new WebError {ErrorMessage = x.Description})
                        .ToArray()));

            var loginModel = new LoginRequest {
                Email = registerRequest.Email,
                Password = registerRequest.Password,
                FingerPrint = registerRequest.FingerPrint,
                RememberMe = true
            };

            var authResponse = await LoginProcess(loginModel, cancellationToken);
            HttpContext.Response.Cookies.Delete(".AspNetCore.Identity.Application");
            return Ok(authResponse);
        }

        [HttpPost]
        [Route("restorepassword")]
        [AllowAnonymous]
        public async Task<ActionResult> RestorePassword([FromBody] RestorePasswordRequest restorePasswordRequest,
            CancellationToken cancellationToken) {
            var user = await _userManager.FindByEmailAsync(restorePasswordRequest.Email);

            //TODO залоггировать попытку ввода не существующего логина
            //|| !await _userManager.IsEmailConfirmedAsync(user)
            if (user == null) return Ok();
            //    throw new Exception($"Пользователь с адресом {restorePasswordRequest.Email} не зарегистрирован");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Action(
                action: "ResetPassword",
                controller: "Auth",
                values: new {userId = user.Id, code},
                protocol: HttpContext.Request.Scheme);

            await _emailService.SendEmailAsync(
                email: restorePasswordRequest.Email,
                subject: "Сброс пароля",
                message: $"Для сброса пароля пройдите по ссылке: <a href='{callbackUrl}'>link</a>",
                cancellationToken: cancellationToken);

            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(int userId, string code, CancellationToken cancellationToken) {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) {
                return View(new ResetPasswordViewModel {Message = "Пользователь не найден"});
            }

            var newPassword = _passGenService.Generate();

            var result = await _userManager.ResetPasswordAsync(user, code, newPassword);

            if (result.Succeeded) {
                await _emailService.SendEmailAsync(
                    email: user.Email,
                    subject: "Новый пароль",
                    message: $"Ваш новый пароль для входа в систему: {newPassword}",
                    cancellationToken: cancellationToken);

                return View(new ResetPasswordViewModel {Message = "Новый пароль был отправлен вам на E-Mail"});
            }

            var exceptionDescription = string.Empty;
            foreach (var error in result.Errors) {
                exceptionDescription += $"{error.Description}\n";
            }

            return View(new ResetPasswordViewModel {Message = exceptionDescription});
        }
    }
}