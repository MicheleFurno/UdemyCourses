using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO) {
        if(await UserExists(registerDTO.UserName)) return BadRequest("UserName is Taken.");

        var user = mapper.Map<AppUser>(registerDTO);

        user.UserName = registerDTO.UserName.ToLower();

        var result = await userManager.CreateAsync(user, registerDTO.Password);

        if(!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new UserDTO {
            UserName = user.UserName,
            Token = await tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO) {

        var user = await userManager.Users
                    .Include(p => p.Photos)
                    .FirstOrDefaultAsync(user => user.NormalizedUserName == loginDTO.UserName.ToUpper());

        if(user == null || user.UserName == null) return Unauthorized("Invalid UserName");

        var result = await userManager.CheckPasswordAsync(user, loginDTO.Password);

        if(!result) return Unauthorized();

        return Ok(new UserDTO {
            UserName = user.UserName,
            Token = await tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        });
    }

    private async Task<bool> UserExists(string userName) {
        return await userManager.Users.AnyAsync(user => user.NormalizedUserName == userName.Trim().ToUpper());
    }

}
