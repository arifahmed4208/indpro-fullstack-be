using IndProBackend.Context;
using IndProBackend.Entities;
using IndProBackend.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IndProBackend.Services
{
    public class UserService : IUserService
    {
        private readonly MyContext _context;
        private readonly IConfiguration _configuration;

        public UserService(MyContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username || u.Email == user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this username or email already exists.");
            }
            var key = GetHmacKey();
            user.Password = HashPassword(user.Password, key);//Not working during login
            user.Created_At = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(e => e.Id == id);
        }

        private static string HashPassword(string password, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hashBytes = new byte[key.Length + hash.Length];
            Array.Copy(key, 0, hashBytes, 0, key.Length);
            Array.Copy(hash, 0, hashBytes, key.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);

        }

        public async Task<string?> AuthenticateUserAsync(string usernameOrEmail, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);

            var key = GetHmacKey();
            var newHashedPassword = HashPassword(password, key);//Not working during login

            if (user !=null && newHashedPassword != user.Password)
            {
                return null; // Invalid username/email or password
            }

            if (user == null)
            {
                return null; // Invalid username/email or password
            }

            return GenerateJwtToken(user);
        }

        private bool VerifyPassword(string inputPassword, string storedHashedPassword)
        {
            var hashBytes = Convert.FromBase64String(storedHashedPassword);
            var key = new byte[32];
            Array.Copy(hashBytes, 0, key, 0, key.Length);
            using var hmac = new HMACSHA256(key);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            return hashBytes.Skip(key.Length).SequenceEqual(computedHash);
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
            var symmetricSecurityKey = new SymmetricSecurityKey(key);
            var signingCredentails = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("id", user.Id.ToString())
            };
            if (user.Username == "admin")
            {
                Claim claim4 = new Claim("role", "admin");
                claims.Add(claim4);
            }
            else
            {
                Claim claim4 = new Claim("role", "user");
                claims.Add(claim4);
            }
            //var token = new JwtSecurityToken(
            //    issuer: _configuration["Jwt:Issuer"],
            //    audience: _configuration["Jwt:Audience"],
            //    claims: claims,
            //    expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:DurationInMinutes"])),
            //    signingCredentials: signingCredentails
            //    );
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescription = new SecurityTokenDescriptor { 
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = signingCredentails
            };
            var token = tokenHandler.CreateToken(tokenDescription);
            return tokenHandler.WriteToken(token);

        }

        private byte[] GetHmacKey()
        {
            var key = _configuration.GetSection("Security:HmacKey").Value!;
            return Encoding.UTF8.GetBytes(key);
        }
    }
}
