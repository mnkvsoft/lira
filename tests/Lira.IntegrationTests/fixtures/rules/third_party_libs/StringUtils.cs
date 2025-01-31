using System;
using System.Text;
using System.Collections.Generic;

// namespace from third party lib
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace utils;

public static class stringUtils
{
    public static string digits(int length)
    {
        var claims = new List<Claim>
        {
            new("phone", "9161112233"),
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("key"));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: cred);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }
}