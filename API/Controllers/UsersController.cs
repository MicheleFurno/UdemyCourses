using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery]UserParams userParams) {

        userParams.CurrentUserName = User.GetUserName();
        var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }

    [HttpGet("{userName}")]
    public async Task<ActionResult<MemberDTO>> GetUser(string userName) {
        var user = await unitOfWork.UserRepository.GetMemberByUsernameAsync(userName);

        if(user == null) return NotFound();

        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO updateDTO) {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());

        if(user == null) return BadRequest("User not found.");

        mapper.Map(updateDTO, user);

        if(await unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to update user.");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file) {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());

        if(user == null) return BadRequest("User not found.");

        var result = await photoService.AddPhotoAsync(file);

        if(result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if(user.Photos.Count == 0) photo.IsMain = true;

        user.Photos.Add(photo);

        if(await unitOfWork.Complete()) 
            return CreatedAtAction(nameof(GetUser), 
                                   new {userName = user.UserName}, 
                                   mapper.Map<PhotoDTO>(photo));

        return BadRequest("Failed to upload photo.");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId) {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());

        if(user == null) return BadRequest("User not found.");

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if(photo == null || photo.IsMain) return BadRequest("This photo cannot be deleted");

        if(photo.PublicId != null) {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if(await unitOfWork.Complete()) return Ok();

        return BadRequest("Problem deleting photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId) {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUserName());

        if(user == null) return BadRequest("User not found.");

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if(photo == null || photo.IsMain) return BadRequest("Cannot use this as main photo");

        var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);

        if(currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

        if(await unitOfWork.Complete()) return NoContent();

        return BadRequest("Problem setting main photo");
    }
}
