﻿using IdentityApp_WebApi.DTOs.Account;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace IdentityApp_WebApi.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        async Task<bool> SendEmailAsync(EmailSendDto emailSend)
        {
            MailjetClient client = new MailjetClient(_config["MailJet:ApiKey"], _config["MailJet:SecretKey"]);
            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(_config["Email:From"], _config["Email:ApplicationName"]))
                .WithSubject(emailSend.Subject)
                .WithHtmlPart(emailSend.Body)
                .WithTo(new SendContact(emailSend.To))
                .Build();
            var response = await client.SendTransactionalEmailAsync(email);
            if (response.Messages != null)
            {
                if (response.Messages[0].Status == "Success")
                {
                    return true;
                }
               
            }
            return false;
        }
    }
}