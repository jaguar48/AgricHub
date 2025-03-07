using System;
using AgricHub.BLL.Helpers;
using AgricHub.BLL.Interfaces.Email;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace AgricHub.BLL.Implementations.EmailServices;


public class EmailService(IOptions<EmailConfiguration> emailConfig) : IEmail
{
    private readonly EmailConfiguration _emailConfig = emailConfig.Value;

    public void Send(string to, string subject, string html, string? from = null)
    {
        // create message
        var email = new MimeMessage();
        
        email.From.Add(new MailboxAddress("AgricHub Email System", _emailConfig.EmailFrom));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        
        email.Body = new TextPart(TextFormat.Html) { Text = html };

        // send email
        using var smtp = new SmtpClient();
        smtp.Connect(_emailConfig.SmtpHost, _emailConfig.SmtpPort, true);
        smtp.Authenticate(_emailConfig.SmtpUser, _emailConfig.SmtpPass);
        smtp.Send(email);
        smtp.Disconnect(true);
    }
}