﻿namespace EmailTemplate.ViewModel
{
    public class EmailSettings
    {
        public string Sender { get; set; }
        public string SenderName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
    }
}
