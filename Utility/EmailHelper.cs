using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;

namespace Utility
{
    public static class EmailHelper
    {
        /// <summary>
        /// Send Email
        /// </summary>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <param name="to">To address</param>
        /// <param name="cc">CC address</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="isBodyHtml">Body HTML or not</param>
        public static void SendEmail(string subject, string body, string to = "", string cc = "", string attachmentFilePath = "", bool isBodyHtml = true)
        {
            string from = ConfigurationManager.AppSettings[ConstStrings.FromEmailKey];
            if (string.IsNullOrWhiteSpace(to))
            {
                to = ConfigurationManager.AppSettings[ConstStrings.AdminEmailKey];
            }

            to = String.Join(",", to.Split(new char[] { ',', ';' }).Select(s => s.EndsWith("@163.com", StringComparison.InvariantCultureIgnoreCase) ? s : s + "@163.com"));
            MailMessage message = new MailMessage(from, to, subject, body);

            FileStream attachmentStream = null;
            if (!string.IsNullOrEmpty(attachmentFilePath) && File.Exists(attachmentFilePath))
            {
                Log.CloseLog();

                attachmentStream = new FileStream(attachmentFilePath, FileMode.Open, FileAccess.Read);
                var attachmentNameArray = attachmentFilePath.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                var attachmentName = attachmentNameArray[attachmentNameArray.Length - 1];
                message.Attachments.Add(new Attachment(attachmentStream, attachmentName, MediaTypeNames.Text.Plain));
            }

            message.IsBodyHtml = isBodyHtml;
            if (!string.IsNullOrWhiteSpace(cc))
            {
                var ccList = cc.Split(new char[] { ',', ';' }).Select(s => s.EndsWith("@163.com", StringComparison.InvariantCultureIgnoreCase) ? s : s + "@163.com");
                foreach (var ccItem in ccList)
                {
                    message.CC.Add(new MailAddress(ccItem));
                }
            }

            SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings[ConstStrings.SmtpHost]);
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential(from, ConfigurationManager.AppSettings[ConstStrings.FromPassword]);

            int retryCount = 0;
            const int maxRetryCount = 3;
            bool sendSuccessful = false;
            string errorMsg = string.Empty;
            while (!sendSuccessful && retryCount < maxRetryCount)
            {
                try
                {
                    client.Send(message);
                    sendSuccessful = true;
                }
                catch (SmtpException exp)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    errorMsg = exp.ToString();
                    retryCount++;
                }
            }

            if (!string.IsNullOrEmpty(attachmentFilePath) && File.Exists(attachmentFilePath))
            {
                attachmentStream.Close();
                Log.ReopenLog();
            }

            if (!sendSuccessful)
            {
                Log.WriteErrorLog("Send mail failed: {0}", errorMsg);
            }
        }
    }
}
