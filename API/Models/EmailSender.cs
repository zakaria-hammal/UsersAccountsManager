using System.Net;
using System.Net.Mail;

namespace Projet.Models
{
    public interface ISenderEmail
    {
        Task SendEmailAsync(string ToEmail, string Subject, string Body, bool IsBodyHtml = false);
    }

    public class EmailSender : ISenderEmail
    {
        public Task SendEmailAsync(string ToEmail, string Subject, string Body, bool IsBodyHtml = false)
        {
            string MailServer = "smtp.gmail.com";
            string FromEmail = "hammalzakaria06@gmail.com";
            string Password = "***Google_Key***";
            int Port = 587;

            var client = new SmtpClient(MailServer, Port)
            {
                Credentials = new NetworkCredential(FromEmail, Password),
                EnableSsl = true,
            };

            MailMessage mailMessage = new MailMessage(FromEmail, ToEmail, Subject, Body)
            {
                IsBodyHtml = IsBodyHtml
            };
            
            return client.SendMailAsync(mailMessage);
        }
    }
}
