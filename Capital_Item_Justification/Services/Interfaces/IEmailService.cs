namespace Capital_Item_Justification.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string cijNumber,string action,string comments);
    }
}
