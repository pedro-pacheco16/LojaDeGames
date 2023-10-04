﻿using LojaGame.Model;
using LojaGame.Service;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LojaGame.Security.Implements
{
    
        public class AuthService : IAuthService
        {
            private readonly IUserService _userService;
            public AuthService(IUserService userService)
            {
                _userService = userService;
            }

            public async Task<UserLogin?> Autenticar(UserLogin userLogin)
            {

                if (userLogin is null || string.IsNullOrEmpty(userLogin.Usuario) || string.IsNullOrEmpty(userLogin.Senha))
                    return null;

                var BuscaUsuario = await _userService.GetByUsuario(userLogin.Usuario);

                if (BuscaUsuario is null)
                    return null;

                if (!BCrypt.Net.BCrypt.Verify(userLogin.Senha, BuscaUsuario.Senha))
                    return null;

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.UTF8.GetBytes(Settings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, userLogin.Usuario)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                userLogin.Id = BuscaUsuario.Id;
                userLogin.Nome = BuscaUsuario.Nome;
                userLogin.Token = "Bearer " + tokenHandler.WriteToken(token).ToString();
                userLogin.Senha = "";

                return userLogin;
            }
        }
}
