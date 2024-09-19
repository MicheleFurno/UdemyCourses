using System;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles() {
        var users = await userManager.Users.OrderBy(u => u.UserName).Select(u => new {
            u.Id,
            u.UserName,
            Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
        }).ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy ="RequireAdminRole")]
    [HttpPost("edit-roles/{userName}")]
    public async Task<ActionResult<IEnumerable<string>>> EditRoles(string userName, string roles) {
        if(string.IsNullOrEmpty(roles)) BadRequest("Specify at least a role");

        var selectedRoles = roles.Split(",").ToArray();

        var user = await userManager.FindByNameAsync(userName);

        if(user == null) return BadRequest("User not found");

        var userRoles = await userManager.GetRolesAsync(user!);

        var result = await userManager.AddToRolesAsync(user!, selectedRoles.Except(userRoles));

        if(!result.Succeeded) return BadRequest("Failed to upodate roles.");

        result = await userManager.RemoveFromRolesAsync(user!, userRoles.Except(selectedRoles));

        if(!result.Succeeded) return BadRequest("Failed to upodate roles.");

        return Ok(await userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet]
    public ActionResult GetPhotosForModeration() {
        return Ok("Admin or Mods.");
    }

}
