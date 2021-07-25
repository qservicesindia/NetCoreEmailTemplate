namespace EmailTemplate.Services
{
    public interface IEmailService
    {
        void SendWelcomeEmail(string email, string username);
    }
}
