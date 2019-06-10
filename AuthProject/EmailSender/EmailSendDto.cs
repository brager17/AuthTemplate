using AuthProject.ValueTypes;

namespace AuthProject.EmailSender
{
    public class EmailSendDto
    {
        public EmailSendDto(Email sendeeEmail, string text, string subject)
        {
            SendeeEmail = sendeeEmail;
            Text = text;
            Subject = subject;
        }

        public string Subject { get; set; }
        public Email SendeeEmail { get; set; }
        public string Text { get; set; }
    }
}