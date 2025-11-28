using Identity;
using IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services;

public class JWTService:IJWTService
{
	private readonly IConfiguration _configuration;
	private readonly UserManager<ApplicationUser> _userManager;

	public JWTService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
	{
		_configuration = configuration;
		_userManager = userManager;
	}
	public async Task<object> CreateJwtToken(ApplicationUser user)
	{
		var roles = await _userManager.GetRolesAsync(user);

		var claims = new List<Claim>();
		claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
		claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
		claims.Add(new Claim(ClaimTypes.Name, user.PersonName));
		claims.Add(new Claim(ClaimTypes.Email, user.Email));
		claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

		foreach (var role in roles)
		{
			claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
		}

		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecritKey"]));
		var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

		var tokenGenerator = new JwtSecurityToken(
			issuer: _configuration["JWT:Issuer"],
			audience: _configuration["JWT:Audience"],
			expires: DateTime.Now.AddHours(1),
			claims: claims,
			signingCredentials: signingCredentials
		);

		var tokenHandler = new JwtSecurityTokenHandler();
		var token = tokenHandler.WriteToken(tokenGenerator);

		var respone = new
		{
			Token = token,
			Email = user.Email,
			PersonName = user.PersonName,
			Expiration = DateTime.Now.AddHours(1)
		};

		return respone;
	}
}
