using System;
using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserName(this ClaimsPrincipal user) {
        
        var userName = user.FindFirstValue(ClaimTypes.Name) ?? throw new Exception("Cannot find UserName from token");
        
        return userName;
    }  

    public static int GetUserId(this ClaimsPrincipal user) {
        
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Cannot find UserName from token"));
        
        return userId;
    }      
}
