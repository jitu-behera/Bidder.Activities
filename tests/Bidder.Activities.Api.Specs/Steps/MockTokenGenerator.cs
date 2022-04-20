using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Bidder.Activities.Specs.Steps;

public class MockTokenGenerator
{
    public static string GenerateTokenWith(List<Claim> enumerable, DateTime expirationDate)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("super secure un-guessable secret key");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(enumerable),
            //TODO:: get from AppConfig
            Expires = expirationDate,
            Issuer = "BA",
            Audience = "Shared-Services",
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var stringToken = tokenHandler.WriteToken(token);
        return stringToken;
    }
}