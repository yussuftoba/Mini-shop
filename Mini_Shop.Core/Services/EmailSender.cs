using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Services;
public class EmailSender
{
	private string hostEmail;
	private string fromEmail;
	private string password;
	private int port;
	public EmailSender(IConfiguration configuration)
	{
		fromEmail = configuration["EmailSender:FromEmail"]!;
		hostEmail = configuration["EmailSender:HostEmail"]!;
		port = int.Parse(configuration["EmailSender:Port"]!);
		password = configuration["EmailSender:Password"]!;

	}
	public void SendEmail(string subject, string toEmail, string message)
	{
		using (var client = new SmtpClient(hostEmail, port))
		{
			client.EnableSsl = true;
			client.Credentials = new NetworkCredential(fromEmail, password);

			// 1. Create the MailMessage object
			var mail = new MailMessage(fromEmail, toEmail);
			mail.Subject = subject;
			mail.Body = message;

			// 2. THIS is the line that makes your HTML code work
			mail.IsBodyHtml = true;

			// 3. Send the object, not the strings
			client.Send(mail);
		}
	}
}
