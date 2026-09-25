using Capital_Item_Justification.Data;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net.Mail;
using System.Net;
using System;
using Capital_Item_Justification.Repository;
using System.Numerics;
using System.Security.Policy;
using Capital_Item_Justification.ViewModels;

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
        public async Task SendEmailAsync(int cijId, string cijNumber, string action, string comments)
        {
            EmailViewModel email = new EmailViewModel();
            try
            {
                string subject = string.Empty;
                string body = string.Empty;
                var cijRequest = await _cijMainRepository.GetCIJById(cijId);
                if (cijRequest == null)
                {
                    _logger.LogError("CIJ request not found for CIJ ID {CijId}", cijId);
                    return;
                }
                string emailAction = action;

                switch (action)
                {
                    case "Submitted":
                    case "Approve":
                        emailAction = "Pending";

                        email = await _cijMainRepository.GetApproveEmail(cijId, emailAction);

                        if (email == null || string.IsNullOrWhiteSpace(email.approverEmail))
                        {
                            _logger.LogWarning(
                                "No approver email found for CIJ {CijNumber} and action {Action}",
                                cijNumber,
                                emailAction);

                            return;
                        }

                        string approverName = email.approverName?
                            .Trim()
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .FirstOrDefault() ?? "";

                        subject = $"CIJ {cijNumber} - {emailAction}";
                        body = PendingEmail(
                            cijNumber,
                            approverName,
                            cijRequest.CIJRequest.RequestDate);

                        break;

                    case "Query":
                        emailAction = "Query";

                        email = await _cijMainRepository.GetApproveEmail(cijId, emailAction);

                        if (email == null || string.IsNullOrWhiteSpace(email.approverEmail))
                        {
                            _logger.LogWarning(
                                "No recipient email found for CIJ {CijNumber} and action {Action}",
                                cijNumber,
                                emailAction);

                            return;
                        }

                        subject = $"CIJ {cijNumber} - {emailAction}";
                        body = QueryEmail(cijNumber, emailAction);

                        break;

                    case "Answer":
                        emailAction = "Answered";

                        email = await _cijMainRepository.GetApproveEmail(cijId, emailAction);

                        if (email == null || string.IsNullOrWhiteSpace(email.approverEmail))
                        {
                            _logger.LogWarning(
                                "No recipient email found for CIJ {CijNumber} and action {Action}",
                                cijNumber,
                                emailAction);

                            return;
                        }

                        subject = $"CIJ {cijNumber} - {emailAction}";
                        body = AnsweredEmail(cijNumber, emailAction);

                        break;

                    case "Rejected":
                        emailAction = "Rejected";

                        email = await _cijMainRepository.GetApproveEmail(cijId, emailAction);

                        if (email == null || string.IsNullOrWhiteSpace(email.approverEmail))
                        {
                            _logger.LogWarning(
                                "No recipient email found for CIJ {CijNumber} and action {Action}",
                                cijNumber,
                                emailAction);

                            return;
                        }

                        subject = $"CIJ {cijNumber} - {emailAction}";
                        body = RejectedEmail(cijNumber, emailAction);

                        break;

                    default:
                        _logger.LogWarning(
                            "Unsupported email action {Action} for CIJ {CijNumber}",
                            action,
                            cijNumber);

                        return;
                }

                var config = await _cijMainRepository.GetEmailConfig();
                if (config == null)
                {
                    _logger.LogError("SMTP email configuration was not found.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(config.Username))
                {
                    _logger.LogError("SMTP username is not configured.");
                    return;
                }
                using var message = new MailMessage();
                message.From = new MailAddress(config.Username, "CIJ System");
                string toEmail = email?.approverEmail;//"dibya.sutar@gmail.com";
                message.To.Add(toEmail);
                //foreach (var email in approverMails)
                //{
                //    message.To.Add(email);
                //}
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(config.SmtpServer, config.SmtpPort);

                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
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
                _logger.LogInformation(
                    "Sending CIJ email. SMTP={SmtpServer}:{SmtpPort}, From={From}, To={To}, Subject={Subject}",
                    config.SmtpServer,
                    config.SmtpPort,
                    message.From?.Address,
                    string.Join(",", message.To.Select(x => x.Address)),
                    message.Subject
                );
                await smtpClient.SendMailAsync(message);
                //_logger.LogInformation("CIJ email sent to {Email} for CIJ {CijNumber}", string.Join(",", approverMails), cijNumber);
                _logger.LogInformation("CIJ email sent to {Email} for CIJ {CijNumber}", email?.approverEmail, cijNumber);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Failed to send CIJ email to {Email}", string.Join(",", approverMails));
                _logger.LogError(ex, "Failed to send CIJ email to {Email}", email?.approverEmail);
            }
        }
        private string PendingEmail(string cijNumber, string approverName, DateOnly requestDate)
        {
            //approverName = "Dibya";
            string url = "http://190.92.175.61/";
            return $"""
            <html>
            <body style="font-family: Arial, sans-serif; color: #333;">
            <p>Dear {approverName},</p>
            <p>
                A CIJ request is pending for your approval.
            </p>

            <table style="border-collapse: collapse; width: 100%; max-width: 700px;">
                <tr style="background-color: #f2f2f2;">
                    <th style="border: 1px solid #ddd; padding: 8px; text-align: left;">
                        CIJ Number
                    </th>
                    <th style="border: 1px solid #ddd; padding: 8px; text-align: left;">
                        Request Date
                    </th>
                    <th style="border: 1px solid #ddd; padding: 8px; text-align: left;">
                        Status
                    </th>
                </tr>

                <tr>
                    <td style="border: 1px solid #ddd; padding: 8px;">
                        {cijNumber}
                    </td>
                    <td style="border: 1px solid #ddd; padding: 8px;">
                        {requestDate}
                    </td>
                    <td style="border: 1px solid #ddd; padding: 8px;">
                        Pending Approval
                    </td>
                </tr>
            </table>

            <p>
                Please log in to the CIJ system and take the required action.
            </p>
            <p>
                <b>CIJ System:</b>
                <a href="{url}">{url}</a>
            </p>
            <p style="font-size: 12px; color: #6c757d;">
                This is an automated notification. Please do not reply to this email.
            </p>
            <p>
                Regards,<br/>
                CIJ System
            </p>

            </body>
            </html>
            """;
        }
        private string QueryEmail(string cijNumber, string approverName)
        {
            return $"""
    <html>
    <body>

    <p>Dear {approverName},</p>

    <p>
        A query has been raised on your CIJ request.
    </p>

    <table border="1" cellpadding="6" cellspacing="0">
        <tr>
            <td><b>CIJ Number</b></td>
            <td>{cijNumber}</td>
        </tr>

        <tr>
            <td><b>Status</b></td>
            <td>Query Raised</td>
        </tr>
    </table>

    <p>
        Please log in to the CIJ system and provide the required clarification.
    </p>

    <p>
        Regards,<br/>
        CIJ System
    </p>

    </body>
    </html>
    """;
        }
        private string AnsweredEmail(string cijNumber, string approverName)
        {
            return $"""
    <html>
    <body>

    <p>Dear {approverName},</p>

    <p>
        The query raised on the CIJ request has been answered.
    </p>

    <table border="1" cellpadding="6" cellspacing="0">
        <tr>
            <td><b>CIJ Number</b></td>
            <td>{cijNumber}</td>
        </tr>

        <tr>
            <td><b>Status</b></td>
            <td>Query Answered</td>
        </tr>
    </table>

    <p>
        Please log in to the CIJ system and review the clarification provided.
    </p>

    <p>
        Regards,<br/>
        CIJ System
    </p>

    </body>
    </html>
    """;
        }

        private string RejectedEmail(string cijNumber, string approverName)
        {
            return $"""
    <html>
    <body>

    <p>Dear {approverName},</p>

    <p>
        Your CIJ request has been rejected.
    </p>

    <table border="1" cellpadding="6" cellspacing="0">
        <tr>
            <td><b>CIJ Number</b></td>
            <td>{cijNumber}</td>
        </tr>

        <tr>
            <td><b>Status</b></td>
            <td>Rejected</td>
        </tr>
    </table>

    <p>
        Please log in to the CIJ system to view the rejection details.
    </p>

    <p>
        Regards,<br/>
        CIJ System
    </p>

    </body>
    </html>
    """;
        }

        public async Task SendUserCreationEmailAsync(string userName, string email)
        {
            try
            {
                string defaultPassword = "Welcome@123";
                var subject = "CIJ Application – User Account Created";

                var body = $@"
                    <!DOCTYPE html>
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        
                        <p>Dear {WebUtility.HtmlEncode(userName)},</p>

                        <p>
                            Your user account has been successfully created
                            in the <strong>CIJ Application</strong> by the Administrator.
                        </p>

                        <p>Please use the following credentials to log in:</p>

                        <table style='border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd;'>
                                    <strong>Username</strong>
                                </td>
                                <td style='padding: 8px; border: 1px solid #ddd;'>
                                    {WebUtility.HtmlEncode(email)}
                                </td>
                            </tr>

                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd;'>
                                    <strong>Default Password</strong>
                                </td>
                                <td style='padding: 8px; border: 1px solid #ddd;'>
                                    {WebUtility.HtmlEncode(defaultPassword)}
                                </td>
                            </tr>
                        </table>

                        <br />

                        <p>
                            <strong>Application URL:</strong><br />
                            <a href='http://190.92.175.61/Account/Login'> http://190.92.175.61/Account/Login </a>
                        </p>

                        <p>
                            For security reasons, please change your password
                            after your first login.
                        </p>

                        <p>
                            If you have any issues accessing the application,
                            please contact the Administrator.
                        </p>

                        <p>
                            Regards,<br />
                            <strong>CIJ Administration Team</strong>
                        </p>

                    </body>
                    </html>";
                var config = await _cijMainRepository.GetEmailConfig();
                if (config == null)
                {
                    _logger.LogError("SMTP email configuration was not found.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(config.Username))
                {
                    _logger.LogError("SMTP username is not configured.");
                    return;
                }
                using var message = new MailMessage();
                message.From = new MailAddress(config.Username, "CIJ System");
                message.To.Add(email);

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(config.SmtpServer, config.SmtpPort);

                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
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
                _logger.LogInformation(
                    "Sending CIJ email. SMTP={SmtpServer}:{SmtpPort}, From={From}, To={To}, Subject={Subject}",
                    config.SmtpServer,
                    config.SmtpPort,
                    message.From?.Address,
                    string.Join(",", message.To.Select(x => x.Address)),
                    message.Subject
                );
                await smtpClient.SendMailAsync(message);
                _logger.LogInformation("CIJ user creation email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(
            ex,
            "Failed to send CIJ user creation email to {Email}",
            email);
            }
        }
    }
}
