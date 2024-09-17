using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService, IMapper mapper) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO) {
        using var hmac = new HMACSHA512();

        if(await UserExists(registerDTO.UserName)) return BadRequest("UserName is Taken.");

        var user = mapper.Map<AppUser>(registerDTO);

        user.UserName = registerDTO.UserName.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
        user.PasswordSalt = hmac.Key;

        context.Users.Add(user);

        await context.SaveChangesAsync();

        return Ok(new UserDTO {
            UserName = user.UserName,
            Token = tokenService.CreateToken(user),
            KnownAs = user.KnownAs
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO) {

        var user = await context.Users.Include(p => p.Photos).FirstOrDefaultAsync(user => user.UserName == loginDTO.UserName.ToLower());

        if(user == null) return Unauthorized("Invalid UserName");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        for(int i = 0; i < computedHash.Length; i++) {
            if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
        }

        return Ok(new UserDTO {
            UserName = user.UserName,
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
            KnownAs = user.KnownAs
        });
    }

    private async Task<bool> UserExists(string userName) {
        return await context.Users.AnyAsync(user => user.UserName.Trim().ToLower() == userName.Trim().ToLower());
    }

}
