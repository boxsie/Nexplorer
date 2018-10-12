using System.Collections.Generic;

namespace Nexplorer.Web.Services.Email
{
    public class EmailMessage
    {
        public List<EmailAddress> ToAddresses { get; set; }
        public List<EmailAddress> FromAddresses { get; set; }

        public string Subject { get; set; }
        public string Content { get; set; }

        public EmailMessage(EmailAddress toAddress, EmailAddress fromAddress = null)
        {
            ToAddresses = new List<EmailAddress>();
            FromAddresses = new List<EmailAddress>();

            if (toAddress != null)
                ToAddresses.Add(toAddress);

            FromAddresses.Add(fromAddress 
                ?? new EmailAddress { Address = "system@nexplorer.io", Name = "Nexplorer" });
        }

        public EmailMessage(List<EmailAddress> toAddresses, List<EmailAddress> fromAddresses = null)
        {
            ToAddresses = new List<EmailAddress>();
            FromAddresses = new List<EmailAddress>();

            if (toAddresses != null)
                ToAddresses = toAddresses;

            FromAddresses = fromAddresses 
                ?? new List<EmailAddress> { new EmailAddress { Address = "system@nexplorer.io", Name = "Nexplorer" } };
        }
    }
}