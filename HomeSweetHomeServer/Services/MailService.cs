using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeSweetHomeServer.Models;
using MailKit;
using MimeKit;
using HomeSweetHomeServer.Repositories;
using Microsoft.Extensions.Configuration;
using MimeKit.Text;
using MailKit.Net.Smtp;

namespace HomeSweetHomeServer.Services
{
    public class MailService : IMailService
    {
        IConfiguration _config;

        public MailService(IConfiguration config)
        {
            _config = config;
        }
        
        //Sends given mail
        public async Task SendMailAsync(EMailModel mail)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mail.FromName, mail.FromAddress));
            message.To.Add(new MailboxAddress(mail.ToName, mail.ToAddress));
            message.Subject = mail.Subject;

            message.Body = new TextPart(TextFormat.Html)
            {
                Text = mail.Content
            };

            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect(_config["EMailConfiguration:SmtpServer"], Convert.ToInt32(_config["EMailConfiguration:SmtpPort"]), true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(_config["EMailConfiguration:SmtpUsername"], _config["EMailConfiguration:SmtpPassword"]);
                await emailClient.SendAsync(message);
                emailClient.Disconnect(true);
            }
        }
    }
}
