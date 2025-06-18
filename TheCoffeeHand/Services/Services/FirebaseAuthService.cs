using Domain.Base;
using FirebaseAdmin.Auth;
using Interfracture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Services.ServiceInterfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Services
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public FirebaseAuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
        }

        public async Task<string> SignInWithFirebaseAsync(string idToken, string? fcmToken)
        {
            try
            {
                // ✅ Verify Firebase Token
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string firebaseUserId = decodedToken.Uid;
                string? email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString() : null;
                string? firstName = decodedToken.Claims.ContainsKey("given_name") ? decodedToken.Claims["given_name"].ToString() : "";
                string? lastName = decodedToken.Claims.ContainsKey("family_name") ? decodedToken.Claims["family_name"].ToString() : "";
                string? username = decodedToken.Claims.ContainsKey("nickname") ? decodedToken.Claims["nickname"].ToString() : email?.Split('@')[0];

                if (string.IsNullOrEmpty(email))
                {
                    throw new Exception("Firebase token does not contain an email.");
                }

                // ✅ Check if the user exists
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // ✅ Create a new user
                    user = new ApplicationUser
                    {
                        UserName = username,
                        Email = email,
                        FirstName = firstName ?? "",
                        LastName = lastName ?? "",
                        FcmToken = fcmToken // ✅ Store FCM Token
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    // ✅ Assign default role if needed
                    await _userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    // ✅ Update FCM token if user already exists
                    if (!string.IsNullOrEmpty(fcmToken))
                    {
                        user.FcmToken = fcmToken;
                        await _userManager.UpdateAsync(user);
                    }
                }

                // ✅ Generate JWT Token with roles
                return await GenerateJwtToken(user, _userManager);
            }
            catch (FirebaseAuthException)
            {
                throw new Exception("Invalid Firebase token.");
            }
        }

        public async Task<string> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                throw new BaseException.UnauthorizedException("invalid_credentials", "Invalid email or password.");
            }

            return await GenerateJwtToken(user, _userManager);
        }

        public async Task<string> SignInWithEmailAndPasswordFirebaseAsync(string email, string password)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            var signInUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";

            var payload = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var httpClient = new HttpClient();
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(signInUrl, content);

            if (!response.IsSuccessStatusCode)
                throw new BaseException.UnauthorizedException("invalid_credentials", "Invalid email or password.");

            var responseData = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var idToken = responseData.RootElement.GetProperty("idToken").GetString();

            // ✅ Verify email
            var isVerified = await IsEmailVerifiedAsync(idToken);
            if (!isVerified)
                throw new BaseException.UnauthorizedException("email_not_verified", "Email is not verified. Please verify your email.");

            // ✅ Find user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new BaseException.NotFoundException("user_not_found", "User not found.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
                throw new BaseException.UnauthorizedException("invalid_credentials", "Invalid email or password.");

            return await GenerateJwtToken(user, _userManager);
        }

        private async Task<bool> IsEmailVerifiedAsync(string idToken)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            var accountInfoUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={apiKey}";

            var payload = new { idToken };
            var httpClient = new HttpClient();
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(accountInfoUrl, content);

            if (!response.IsSuccessStatusCode)
                throw new BaseException.CoreException("firebase_error", "Failed to retrieve account information.");

            var responseData = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var users = responseData.RootElement.GetProperty("users");

            if (users.GetArrayLength() == 0)
                return false;

            return users[0].GetProperty("emailVerified").GetBoolean();
        }


        public async Task RegisterWithEmailPasswordFireBaseAsync(string email, string password)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            var signUpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";

            var payload = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var httpClient = new HttpClient();
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(signUpUrl, content);

            if (!response.IsSuccessStatusCode)
                throw new BaseException.BadRequestException("registration_failed", "Failed to register user. Please try again.");

            var responseData = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var idToken = responseData.RootElement.GetProperty("idToken").GetString();

            // ✅ Send email verification
            await SendEmailVerificationAsync(idToken);

            // ✅ Create user in Identity
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                throw new BaseException.BadRequestException("user_exists", "User already exists.");

            var newUser = new ApplicationUser
            {
                UserName = email.Split('@')[0],
                Email = email,
                FirstName = "",
                LastName = ""
            };

            var result = await _userManager.CreateAsync(newUser, password);
            if (!result.Succeeded)
            {
                throw new BaseException.BadRequestException("create_user_failed",
                    $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await _userManager.AddToRoleAsync(newUser, "User");
        }

        private async Task SendEmailVerificationAsync(string idToken)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            var verificationUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}";

            var payload = new
            {
                requestType = "VERIFY_EMAIL",
                idToken = idToken
            };

            var httpClient = new HttpClient();
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(verificationUrl, content);

            if (!response.IsSuccessStatusCode)
                throw new BaseException.CoreException("email_verification_failed", "Failed to send verification email.");
        }

        public async Task DisableUserAsync(string email)
        {
            // ✅ Find user in Identity
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new BaseException.NotFoundException("user_not_found", "User not found.");

            // ✅ Disable Firebase user
            try
            {
                var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
                await FirebaseAuth.DefaultInstance.UpdateUserAsync(new UserRecordArgs
                {
                    Uid = firebaseUser.Uid,
                    Disabled = true
                });
            }
            catch (FirebaseAuthException)
            {
                throw new BaseException.CoreException("firebase_error", "Failed to disable Firebase user.");
            }

            // ✅ Disable user in database
            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new BaseException.CoreException("db_update_failed", "Failed to disable user in database.");
            }
        }

        public async Task EnableUserAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new BaseException.NotFoundException("user_not_found", "User not found.");

            try
            {
                var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
                await FirebaseAuth.DefaultInstance.UpdateUserAsync(new UserRecordArgs
                {
                    Uid = firebaseUser.Uid,
                    Disabled = false
                });
            }
            catch (FirebaseAuthException)
            {
                throw new BaseException.CoreException("firebase_error", "Failed to enable Firebase user.");
            }

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new BaseException.CoreException("db_update_failed", "Failed to enable user in database.");
            }
        }




        private async Task<string> GenerateJwtToken(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new BaseException.NotFoundException("not_found", "Jwt key not found")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email ?? "")
            };

            // ✅ Fetch roles from Identity and add them as claims
            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
