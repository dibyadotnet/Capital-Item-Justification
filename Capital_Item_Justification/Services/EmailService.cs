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
using Capital_Item_Justification.Models;

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

        public async Task SendEmailAsync(int cijId, string action,string targetRoleId,int? clarificationId)
        {
            CIJMainViewModel? cijRequest = new();
            try
            {
                cijRequest = await _cijMainRepository.GetCIJById(cijId);

                if (cijRequest == null)
                {
                    _logger.LogError("CIJ request not found for CIJ ID {CijId}", cijId);
                    return;
                }

                string emailAction;
                string subject;
                string body = string.Empty;
                List<EmailViewModel>? emails = new();
                switch (action.Trim())
                {
                    case "Submitted":
                    case "Approve":
                        emailAction = "Pending";
                        subject = $"CIJ {cijRequest.CIJRequest.CIJSNumber} - {emailAction}";
                        emails = await _cijMainRepository.GetApproveEmail(cijId, emailAction);
                        break;

                    case "Query":
                        emailAction = "Query";
                        subject = $"CIJ {cijRequest.CIJRequest.CIJSNumber} - {emailAction}";
                        emails = await _cijMainRepository.GetQueryEmail(cijId, targetRoleId);
                        break;

                    case "Answer":
                        emailAction = "Answered";
                        subject = $"CIJ {cijRequest.CIJRequest.CIJSNumber} - {emailAction}";
                        emails = await _cijMainRepository.GetAnsweredEmail(cijId, clarificationId??0);
                        break;

                    case "Rejected":
                        emailAction = "Rejected";
                        subject = $"CIJ {cijRequest.CIJRequest.CIJSNumber} - {emailAction}";
                        emails = await _cijMainRepository.GetRejectedEmail(cijId);
                        break;

                    default:
                        _logger.LogWarning("Unsupported email action {Action} for CIJ {CijNumber}", action, cijRequest.CIJRequest.CIJSNumber);
                        return;
                }
             
                if (emails == null || emails.Count == 0)
                {
                    _logger.LogWarning("No email recipients found for CIJ {CijNumber}, action {Action}", cijRequest.CIJRequest.CIJSNumber, emailAction);
                    return;
                }

                // Build the email body using the recipient's first name.
                var config = await _cijMainRepository.GetEmailConfig();

                if (config == null)
                {
                    _logger.LogError("SMTP email configuration was not found.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(config.Username) ||
                    string.IsNullOrWhiteSpace(config.Password) ||
                    string.IsNullOrWhiteSpace(config.SmtpServer) ||
                    config.SmtpPort <= 0)
                {
                    _logger.LogError("SMTP configuration is incomplete.");
                    return;
                }

                using var message = new MailMessage();

                message.From = new MailAddress(config.Username, "CIJ System");

                // Add all valid recipient email addresses.
                foreach (var recipient in emails)
                {
                    if (string.IsNullOrWhiteSpace(recipient.approverEmail))
                    {
                        _logger.LogWarning("Recipient email is empty for CIJ {CijNumber}", cijRequest.CIJRequest.CIJSNumber);
                        continue;
                    }

                    string approverName = recipient.approverName?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";

                    // Generate the correct email body for each recipient.
                    string recipientBody = emailAction switch
                    {
                        "Pending" => PendingEmail(cijRequest.CIJRequest.CIJSNumber, approverName, cijRequest.CIJRequest.RequestDate),

                        "Query" => QueryEmail(cijRequest.CIJRequest.CIJSNumber, approverName, cijRequest.CIJRequest.RequestDate),

                        "Answered" => AnsweredEmail(cijRequest.CIJRequest.CIJSNumber, approverName, cijRequest.CIJRequest.RequestDate),

                        "Rejected" => RejectedEmail(cijRequest.CIJRequest.CIJSNumber, approverName, cijRequest.CIJRequest.RequestDate),

                        _ => string.Empty
                    };

                    // Use BCC so recipients do not see each other's addresses.
                    message.Bcc.Add(recipient.approverEmail.Trim());

                    // All recipients receive the same body. If your email
                    // templates contain personalized greetings, send separate
                    // messages per recipient instead of using BCC.
                    if (string.IsNullOrEmpty(body))
                    {
                        body = recipientBody;
                    }
                }

                if (message.Bcc.Count == 0)
                {
                    _logger.LogWarning("No valid email addresses found for CIJ {CijNumber}", cijRequest.CIJRequest.CIJSNumber);
                    return;
                }

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(config.SmtpServer, config.SmtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(config.Username, config.Password)
                };

                _logger.LogInformation(
                    "Sending CIJ email. SMTP={SmtpServer}:{SmtpPort}, " +
                    "CIJ={CijNumber}, Action={Action}, RecipientCount={RecipientCount}",
                    config.SmtpServer,
                    config.SmtpPort,
                    cijRequest.CIJRequest.CIJSNumber,
                    emailAction,
                    message.Bcc.Count);

                await smtpClient.SendMailAsync(message);

                _logger.LogInformation(
                    "CIJ email sent successfully for CIJ {CijNumber}, action {Action}",
                    cijRequest.CIJRequest.CIJSNumber,
                    emailAction);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send CIJ email. CIJ ID={CijId}, " +
                    "CIJ Number={CijNumber}, Action={Action}",
                cijId,
                    cijRequest.CIJRequest.CIJSNumber,
                    action);
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
        private string QueryEmail(string cijNumber, string approverName, DateOnly requestDate)
        {
            string url = "http://190.92.175.61/";

            return $"""
    <html>
    <body style="font-family: Arial, sans-serif; color: #333;">
    <p>Dear {approverName},</p>

    <p>
        A query has been raised for the following CIJ request.
        Please review the request and provide the required clarification.
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
                {requestDate:dd-MMM-yyyy}
            </td>
            <td style="border: 1px solid #ddd; padding: 8px; color: #fd7e14; font-weight: bold;">
                Query Raised
            </td>
        </tr>
    </table>

    <p>
        Please log in to the CIJ system to review the query details
        and take the necessary action.
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
        private string AnsweredEmail(string cijNumber, string approverName, DateOnly requestDate)
        {
            string url = "http://190.92.175.61/";

            return $"""
    <html>
    <body style="font-family: Arial, sans-serif; color: #333;">
    <p>Dear {approverName},</p>

    <p>
        The query raised for the following CIJ request has been answered.
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
                {requestDate:dd-MMM-yyyy}
            </td>
            <td style="border: 1px solid #ddd; padding: 8px; color: #198754; font-weight: bold;">
                Answered
            </td>
        </tr>
    </table>

    <p>
        Please log in to the CIJ system to review the response
        and continue the approval process.
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
        private string RejectedEmail(string cijNumber, string approverName, DateOnly requestDate)
        {
            string url = "http://190.92.175.61/";

            return $"""
    <html>
    <body style="font-family: Arial, sans-serif; color: #333;">
    <p>Dear {approverName},</p>

    <p>
        The following CIJ request has been rejected.
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
                {requestDate:dd-MMM-yyyy}
            </td>
            <td style="border: 1px solid #ddd; padding: 8px; color: #dc3545; font-weight: bold;">
                Rejected
            </td>
        </tr>
    </table>

    <p>
        Please log in to the CIJ system to review the request details
        and rejection information.
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

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            try
            {
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

                var subject = "CIJ Application - Reset Your Password";

                var body = $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; color: #333;">

                <p>Dear User,</p>

                <p>
                    We received a request to reset your password for the
                    <strong>CIJ Application</strong>.
                </p>

                <p>
                    Please click the button below to reset your password:
                </p>

                <p>
                    <a href="{WebUtility.HtmlEncode(resetLink)}"
                       style="
                           display:inline-block;
                           padding:10px 20px;
                           background-color:#0d6efd;
                           color:#ffffff;
                           text-decoration:none;
                           border-radius:5px;">
                        Reset Password
                    </a>
                </p>

                <p>
                    If you did not request a password reset, you can safely
                    ignore this email.
                </p>

                <p>
                    For security reasons, this link will expire according to
                    the application's password reset token settings.
                </p>

                <p>
                    Regards,<br />
                    <strong>CIJ Administration Team</strong>
                </p>

            </body>
            </html>
            """;

                using var message = new MailMessage();

                message.From = new MailAddress(
                    config.Username,
                    "CIJ System");

                message.To.Add(email);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var smtpClient =
                    new SmtpClient(config.SmtpServer, config.SmtpPort);

                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials =
                    new NetworkCredential(
                        config.Username,
                        config.Password);

                await smtpClient.SendMailAsync(message);

                _logger.LogInformation(
                    "Password reset email sent successfully to {Email}",
                    email);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send password reset email to {Email}",
                    email);
            }
        }
    }
}
