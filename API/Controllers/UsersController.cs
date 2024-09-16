using System;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers() {
        var users = await userRepository.GetMembersAsync();

        return Ok(users);
    }

    [HttpGet("{userName}")]
    public async Task<ActionResult<MemberDTO>> GetUser(string userName) {
        var user = await userRepository.GetMemberByUsernameAsync(userName);

        if(user == null) return NotFound();

        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO updateDTO) {
        var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if(userName == null) return BadRequest("No userName found in token");

        var user = await userRepository.GetUserByUsernameAsync(userName);

        if(user == null) return BadRequest("User not found.");

        mapper.Map(updateDTO, user);

        if(await userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update user.");
    }
}
