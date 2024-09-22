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
            //var key = GetHmacKey();
            //user.Password = HashPassword(user.Password, key);//Not working during login
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
            //var user = await _context.Users
            //    .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Username == usernameOrEmail || u.Email == usernameOrEmail) && u.Password==password);

            //if (user == null || !VerifyPassword(password, user.Password))
            //{
            //    return null; // Invalid username/email or password
            //}

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

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value));
            var signingCredentails = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            List<Claim> claims = new List<Claim>();
            Claim claim = new Claim(JwtRegisteredClaimNames.Sub, user.Username);
            Claim claim2 = new Claim(JwtRegisteredClaimNames.Email, user.Email);
            Claim claim3 = new Claim("id", user.Id.ToString());
            claims.Add(claim);
            claims.Add(claim2);
            claims.Add(claim3);
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
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:DurationInMinutes"])),
                signingCredentials: signingCredentails
                );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private byte[] GetHmacKey()
        {
            return Encoding.UTF8.GetBytes(_configuration["Security:HmacKey"]);
        }
    }
}
