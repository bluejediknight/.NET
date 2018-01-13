using System;
using System.Configuration;
using System.Net.Mail;
using System.Web;
using System.Text;

namespace HL.Library
{
    public class Emails
    {
        // Add these as defaults
        private static string gDefaultFromEmailAddress = "webmaster@site.com";
	private static string gDefaultSMTPServer = "site.com";
	private static string gDefaultSubject = "site.com Application Alert";
        private static string gDefaultRecipients = "webmaster@site.com";
        
        /// <summary>
        /// <para>Library.Emails: methods for sending email alerts
        /// but can also be used for generic email handling.</para>
        /// <para>Defaults can be set in the AppSettings.config file located in the path specified 
        /// in the AppSettingsPath element of the web.config.</para>
        ///<para>Class-level default recipient: webmaster@site.com</para>
        ///<para>Should have plenty of flexibility with the parameters.</para>
        /// </summary>
        public static void SendMailAlert(string emailBodyText, Exception ex, string subject, bool highPriority, string recipients)
		{
            try
            {
                #region Email sending turned off / recipients  and delayWanted settings
                if (HL.Library.Settings.GetAppSetting("sendErrorEmailAlert") != "1")
                {
                    return;
                }

                // Check the recipients
                if (recipients == "")
                {
                    recipients = gDefaultRecipients;
                }

                // deal with the delay
                int delayWanted = 0;
                try
                {
                    delayWanted = System.Convert.ToInt32(HL.Library.Settings.GetAppSetting("alertDelayInMin"));
                }
                catch
                {
                    delayWanted = 0;
                }
                #endregion

                #region Bail out if under the timeout
                if (delayWanted > 0)
                {
                    DateTime timeRightNow = DateTime.Now;
                    DateTime timeLastAlert = timeRightNow;

                    // Do we send the alert?
                    if (System.Web.HttpContext.Current.Cache["timeOfLastAlert"] == null)
                    {
                        // Add timeOfLastLowPriorityAlert to application cache with an expiration
                        System.Web.HttpContext.Current.Cache.Insert("timeOfLastAlert", timeRightNow,
                            null, timeRightNow.AddMinutes(delayWanted), TimeSpan.Zero);
                    }
                    else
                    {
                        timeLastAlert = (DateTime)System.Web.HttpContext.Current.Cache["timeOfLastAlert"];
                    }

                    // Determine if time since last alert > delayWanted
                    TimeSpan ts = timeRightNow.Subtract(timeLastAlert);

                    if (ts.TotalMinutes == 0.0 || ts.TotalMinutes > delayWanted)
                    {
                        // send it
                    }
                    else
                    {
                        // bail out
                        return;
                    }
                }
                #endregion

                MailAddress from = new MailAddress(gDefaultFromEmailAddress);

                MailMessage message = new MailMessage();
                message.From = from;
                message.To.Add(recipients.Replace(";", ","));

                // Add the message body
                StringBuilder text = new StringBuilder(emailBodyText.Trim() + "\r\n\r\n");

                try
                {
                    text.Append("URL: " + System.Web.HttpContext.Current.Request.Url.ToString() + "\r\n");
                    text.Append("Platform: " + System.Web.HttpContext.Current.Request.Browser.Platform + "\r\n");
                    text.Append("Browser: " + System.Web.HttpContext.Current.Request.Browser.Browser + "\r\n");
                    text.Append("Version: " + System.Web.HttpContext.Current.Request.Browser.Type + "\r\n");
                    text.Append("Client IP: http://whatismyipaddress.com/ip/" + System.Web.HttpContext.Current.Request.UserHostAddress.ToString() + "\r\n");
                    text.Append("Server Time: " + DateTime.Now.ToString() + "\r\n");
                    System.Collections.Specialized.NameValueCollection headers = System.Web.HttpContext.Current.Request.Headers;
                    text.Append("Headers:\r\n");
                    foreach (string key in System.Web.HttpContext.Current.Request.Headers.Keys)
                    {
                        text.Append("\t" + key + ": " + headers.Get(key) + "\r\n");
                    }
                    headers = null;

                    if (ex != null)
                    {
                        try
                        {
                            text.Append("\r\nException Message: " + ex.Message);
                            // text.Append("\r\n\r\nException Stacktrace: " + ex.StackTrace.Replace(" at ", "\nat "));
                            Exception exx = ex.InnerException;
                            while (exx != null)
                            {
                                text.Append("\r\n\r\nInner Exception Message: " + exx.Message);
                                // text.Append("\r\n\r\nInner Exception Stacktrace: " + exx.StackTrace.Replace(" at ", "\n\tat "));
                                exx = exx.InnerException;
                            }
                        }
                        catch
                        {
                            // no worries
                        }
                    }
                }
                catch
                {
                    // no worries
                    text.Append("There was an error, but then another getting all the information.");
                }

                #region Subject
                if (!string.IsNullOrEmpty(subject))
                {
                    message.Subject = subject;
                }
                else
                {
                    message.Subject = gDefaultSubject;
                }

                // Add environment to SUBJECT
                string environment = "UNKNOWN";
                try
                {
                    environment = HL.Library.Settings.GetSettingFromWebConfig("Environment");
                }
                catch
                {
                    environment = "UNKNOWN";
                }
                message.Subject += " - " + environment.ToUpper();
                #endregion

                // Assign the message
                message.Body = text.ToString();
                text = null;


                // Priority
                if (highPriority)
                {
                    message.Priority = MailPriority.High;
                }

                SmtpClient client = new SmtpClient(gDefaultSMTPServer);
                client.Send(message);

                // Clean up
                client.Dispose();
                message.Dispose();
            }
            catch
            {
                // Do nothing
            }
		}

        #region
        public static void SendMailAlert(string emailBodyText, Exception ex)
        {
            SendMailAlert(emailBodyText, ex, string.Empty, false, "");
        }
        public static void SendMailAlert(string emailBodyText, Exception ex, string subject)
        {
            SendMailAlert(emailBodyText, ex, subject, false, "");
        }
        public static void SendMailAlert(string emailBodyText, Exception ex, string subject, bool highPriority)
        {
            SendMailAlert(emailBodyText, ex, subject, highPriority, "");
        }
        #endregion
    }
}
