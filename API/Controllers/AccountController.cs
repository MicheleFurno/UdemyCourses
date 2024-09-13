using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO) {
        using var hmac = new HMACSHA512();

        if(await UserExists(registerDTO.UserName)) return BadRequest("UserName is Taken.");

        return Ok();
        /*
        var user = new AppUser {
            UserName = registerDTO.UserName.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        return Ok(new UserDTO {
            UserName = user.UserName,
            Token = tokenService.CreateToken(user)
        });
        */
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO) {

        var user = await context.Users.FirstOrDefaultAsync(user => user.UserName == loginDTO.UserName.ToLower());

        if(user == null) return Unauthorized("Invalid UserName");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        for(int i = 0; i < computedHash.Length; i++) {
            if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
        }

        return Ok(new UserDTO {
            UserName = user.UserName,
            Token = tokenService.CreateToken(user)
        });
    }

    private async Task<bool> UserExists(string userName) {
        return await context.Users.AnyAsync(user => user.UserName.Trim().ToLower() == userName.Trim().ToLower());
    }

}
