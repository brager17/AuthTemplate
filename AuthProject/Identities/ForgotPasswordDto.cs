namespace AuthProject.Identities
{
    public class ForgotPasswordDto
    {
        public ForgotPasswordDto(string email)
        {
            Email = email;
        }
        public string Email { get; set; }
    }
}