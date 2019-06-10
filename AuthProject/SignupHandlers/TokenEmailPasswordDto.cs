namespace AuthProject.Services
{
#nullable enable
    public class TokenEmailPasswordDto : TokenEmailDto
    {
        public string? NewPassword { get; set; }
    }
}