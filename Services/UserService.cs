using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Todo2Api.V1.Entities;
using Todo2Api.Helpers;
using Todo2Api.Models;

namespace Todo2Api.Services
{
    public interface IUserService
    {
        Users Authenticate(string username, string password);
        IEnumerable<Users> GetAll();
    }

    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;
        private readonly TodoContext _context ;

        public UserService(IOptions<AppSettings> appSettings,TodoContext context)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        public Users Authenticate(string username, string password)
        {
            Users Users = _context.Users.Where (x => x.Username == username && x.Password == password).FirstOrDefault();

            if (Users == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.Name, Users.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token Süresi
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            Users.Token = tokenHandler.WriteToken(token);

            // remove password before returning
            Users.Password = null;

            return Users;
        }

        public IEnumerable<Users> GetAll()
        {
            return _context.Users.Where(x=>x.Password == null || x.Password == "").ToList();
        }
    }
}