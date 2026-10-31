namespace Capital_Item_Justification.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(int cijId, string action, int? approvalId, int? clarificationId);
        Task SendUserCreationEmailAsync(string userName, string email);
        Task SendPasswordResetEmailAsync(string email, string resetLink);
    }
}
