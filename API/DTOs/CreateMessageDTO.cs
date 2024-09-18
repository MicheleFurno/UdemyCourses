using System;

namespace API.DTOs;

public class CreateMessageDTO
{
    public required string RecipientUserName { get; set; }
    public required string Content { get; set; }
}
