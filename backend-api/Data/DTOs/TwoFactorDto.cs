namespace PentestHub.API.Data.DTOs;

public class TwoFactorDto
{
    public string Code { get; set; } = string.Empty;
    public int? UserId { get; set; } // Used for login step 2
}
