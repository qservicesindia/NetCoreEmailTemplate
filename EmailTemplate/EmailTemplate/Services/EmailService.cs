using EmailTemplate.ViewModel;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using static EmailTemplate.GlobalConstants;

namespace EmailTemplate.Services
{
    public class EmailService : IEmailService
    {
        #region fields
        readonly EmailSettings _emailSettings;
        readonly IWebHostEnvironment _env;
        readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region constructor
        public EmailService(EmailSettings emailSettings, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _emailSettings = emailSettings;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        #region public methods

        public void SendWelcomeEmail(string email, string username)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("[FROMUSER]", username);

            var messages = new EmailMessage
            {
                Reciever = email,
                Subject = EmailConstants.InviteSubject,
                Content = GetEmailBody(EmailConstants.InviteTemplatePath, parameters)
            };

            SendEmail(messages);
        }

        #endregion

        #region private methods

        private string GetEmailBody(string path, Dictionary<string, string> parameters)
        {
            string messageBody = GetEmailTemplate(path);

            if (parameters != null)
            {
                //Replace the key's in the message body with the corresponding value of the dictionary item
                foreach (KeyValuePair<String, String> par in parameters)
                {
                    if (par.Value == null)
                    {
                        messageBody = messageBody.Replace(par.Key.ToString(), "");
                    }
                    else
                    {
                        messageBody = messageBody.Replace(par.Key.ToString(), par.Value.ToString());

                        //We are also replacing the key by making it in lower cases  as google api change the key sometime to lower case
                        messageBody = messageBody.Replace(par.Key.ToString().ToLower(), par.Value.ToString());
                    }
                }
            }

            messageBody = messageBody.Replace("[APPURL]", $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}");

            return messageBody;
        }

        private string GetEmailTemplate(string path)
        {
            var pathToFile = Path.Combine(_env.WebRootPath, EmailConstants.TempalateDirectory, path);
            var pathToLayout = Path.Combine(_env.WebRootPath, EmailConstants.TempalateDirectory, EmailConstants.LayoutPath);

            return ReadFile(pathToLayout).Replace("[EMAILBODY]", ReadFile(pathToFile));
        }

        private string ReadFile(string path)
        {
            using (StreamReader streamReader = new StreamReader(path))
            {
                return streamReader.ReadToEnd();
            }
        }

        private void SendEmail(EmailMessage message)
        {
            var mimeMessage = CreateMimeMessage(message);
            using (SmtpClient smtpClient = new SmtpClient())
            {
                smtpClient.Connect(_emailSettings.SmtpServer,
                _emailSettings.Port, true);
                smtpClient.Authenticate(_emailSettings.Username,
                _emailSettings.Password);
                smtpClient.Send(mimeMessage);
                smtpClient.Disconnect(true);
            }
        }

        private MimeMessage CreateMimeMessage(EmailMessage message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.Sender));
            mimeMessage.To.Add(new MailboxAddress(message.Reciever));
            mimeMessage.Subject = message.Subject;
            mimeMessage.Body = new TextPart(TextFormat.Html)
            { Text = message.Content };
            return mimeMessage;
        }
        #endregion
    }
}
