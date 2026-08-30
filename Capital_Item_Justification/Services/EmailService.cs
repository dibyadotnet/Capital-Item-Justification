using Capital_Item_Justification.Data;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net.Mail;
using System.Net;

namespace Capital_Item_Justification.Services
{
    public class EmailService : IEmailService
    {
        private readonly ICIJMainRepository _cijMainRepository;
        private readonly ILogger<EmailService> _logger;
        public EmailService(ICIJMainRepository cijMainRepository, ILogger<EmailService> logger)
        {
            _cijMainRepository = cijMainRepository;
            _logger = logger;
        }
        public async Task SendEmailAsync(string cijNumber, string action, string comments)
        {
            string toEmail = "dibya.sutar@gmail.com";
            try
            {
                var subject = $"CIJ {cijNumber} - {action}";

                //var body = $"""Dear User, CIJ Number: {cijNumber} Action Taken: {action} Comments: {comments} Regards,CIJ System""";
                var body = PendingApprovalEmail(cijNumber, action);
                CijEmailConfiguration email = await _cijMainRepository.GetEmailConfig();
                var config = await _cijMainRepository.GetEmailConfig();
                using var message = new MailMessage();
                string UserName = string.Empty;
                message.From = new MailAddress(config.SenderEmail, UserName ?? "CIJ System");

                message.To.Add(toEmail);

                //if (!string.IsNullOrWhiteSpace(ccEmail))
                //{
                //    message.CC.Add(ccEmail);
                //}

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(config.SmtpServer, config.SmtpPort);

                smtpClient.EnableSsl = true;

                if (!string.IsNullOrWhiteSpace(config.Username))
                {
                    smtpClient.Credentials =
                        new NetworkCredential(
                            config.Username,
                            config.Password);
                }
                else
                {
                    smtpClient.UseDefaultCredentials = false;
                }

                await smtpClient.SendMailAsync(message);
                _logger.LogInformation("CIJ email sent to {Email} for CIJ {CijNumber}", toEmail, cijNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send CIJ email to {Email}", toEmail);
            }
        }
        private string PendingApprovalEmail(string cijNumber, string approverName)
        {
            approverName = "Dibya";
            return $"""
            <html>
            <body>

            <p>Dear {approverName},</p>

            <p>
                A CIJ request is pending for your approval.
            </p>

            <table border="1" cellpadding="6">
                <tr>
                    <td><b>CIJ Number</b></td>
                    <td>{cijNumber}</td>
                </tr>

                <tr>
                    <td><b>Status</b></td>
                    <td>Pending Approval</td>
                </tr>
            </table>

            <p>
                Please log in to the CIJ system and take the required action.
            </p>

            <p>
                Regards,<br/>
                CIJ System
            </p>

            </body>
            </html>
            """;
        }
    }
}
