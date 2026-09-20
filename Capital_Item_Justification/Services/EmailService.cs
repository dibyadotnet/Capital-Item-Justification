using Capital_Item_Justification.Data;
using Capital_Item_Justification.Repository.Interfaces;
using Capital_Item_Justification.Services.Interfaces;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net.Mail;
using System.Net;
using System;

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
            //string toEmail = "dibya.sutar@gmail.com";
            List<string> approverMails = new List<string>();
            try
            {
                string subject = string.Empty;
                string body = string.Empty;
                //var body = $"""Dear User, CIJ Number: {cijNumber} Action Taken: {action} Comments: {comments} Regards,CIJ System""";
                if (action == "Submitted" || action == "Approve")
                {
                    action = "Pending";
                    subject = $"CIJ {cijNumber} - {action}";
                    body = PendingEmail(cijNumber, action);
                }
                if (action == "Query")
                {
                    action = "Query";
                    subject = $"CIJ {cijNumber} - {action}";
                    body = QueryEmail(cijNumber, action);
                }
                if (action == "Answer")
                {
                    action = "Answered";
                    subject = $"CIJ {cijNumber} - {action}";
                    body = AnsweredEmail(cijNumber, action);
                }
                if (action == "Rejected")
                {
                    subject = $"CIJ {cijNumber} - {action}";
                    body = RejectedEmail(cijNumber, action);
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
                approverMails = await _cijMainRepository.GetApproveEmail(cijId, action);
                using var message = new MailMessage();
                message.From = new MailAddress(config.Username, "CIJ System");
                foreach (var email in approverMails)
                {
                    message.To.Add(email);
                }
                //if (!string.IsNullOrWhiteSpace(ccEmail))
                //{
                //    message.CC.Add(ccEmail);
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
                _logger.LogInformation("CIJ email sent to {Email} for CIJ {CijNumber}", string.Join(",", approverMails), cijNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send CIJ email to {Email}", string.Join(",", approverMails));
            }
        }
        private string PendingEmail(string cijNumber, string approverName)
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

    }
}
