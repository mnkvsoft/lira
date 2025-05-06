using System;
using System.Text;
using System.Collections.Generic;

using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace utils;

public static class jwtUtils
{
    public static string getJwt(string phone)
    {
        var claims = new List<Claim>
        {
            new("phone", phone),
        };

        var secretPhrase = "super secret phrase";
        using var sha512 = SHA512.Create();
        byte[] keyBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(secretPhrase));

        var key = new SymmetricSecurityKey(keyBytes);
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.SpecifyKind(new DateTime(2025, 03, 06), DateTimeKind.Utc),
            signingCredentials: cred);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }
}