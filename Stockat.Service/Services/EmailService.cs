using Microsoft.Extensions.Configuration;
using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.Services;

internal sealed class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpClient = new SmtpClient
        {
            Host = _config["Smtp:Host"],
            Port = int.Parse(_config["Smtp:Port"]),
            EnableSsl = true,
            Credentials = new NetworkCredential(
                _config["Smtp:Username"],
                _config["Smtp:Password"])
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_config["Smtp:Username"]),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }

}