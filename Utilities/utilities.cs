using System;
using System.IO;

// Last Updated by: Harrison
namespace HL.Library
{
	// Utlities.cs: common functions for routine tasks
	public class utilities
	{
        public static void WriteToDebugLog(string message)
        {
            try
            {
                // Perform the write in this format:
                //		TimeStamp, User, Message + exception message
                // ============================================
                string entry = DateTime.Now.ToLongTimeString() + ", " + message;

                WriteToTextFile(
                    utilities.GetApplicationDirectory() + "\\App_Data\\logs\\Debug_" + utilities.GetYyyymmdd() + ".txt", entry);
            }
            catch
            {
                // Do Nothing
            }
        }
         
        public static string GetApplicationDirectory()
        {
            return System.Web.HttpContext.Current.Server.MapPath("~");
        }
         
        public static string GetHttpHeader(string headerName)
        {
            return System.Web.HttpContext.Current.Request.Headers[headerName];
        }
         
		public static string GetYyyymmdd()
		{
			string returnString = DateTime.Now.Year.ToString();
			if (DateTime.Now.Month.ToString().Length == 1)
			{
				returnString += "0" + DateTime.Now.Month.ToString();
			}
			else
			{
				returnString += DateTime.Now.Month.ToString();
			}
			if (DateTime.Now.Day.ToString().Length == 1)
			{
				returnString += "0" + DateTime.Now.Day.ToString();
			}
			else
			{
				returnString += DateTime.Now.Day.ToString();
			}
			return returnString;
		}

		public static bool WriteToTextFile(string file, string message)
		{
			try
			{
				StreamWriter w = File.AppendText(file);
				w.WriteLine(message);
				w.Close();
				return true;
			}
			catch (Exception err)
			{
                HL.Library.Emails.SendMailAlert("Error writing to Logging File", err);
				return false;
			}
		}

		public static string ReadFromTextFile(string path) 
		{
			string returnString = "";

			using (StreamReader sr = new StreamReader(path)) 
			{
				String line;
				// Read and display lines from the file until the end of 
				// the file is reached.
				while ((line = sr.ReadLine()) != null) 
				{
					returnString += line;
				}
			}
			return returnString;
		}

        public static string EncodeHTML(string stringToEncode)
        {
            return System.Web.HttpUtility.HtmlEncode(stringToEncode);
        }

       

        public static int ConvertQueryStringToIntOrZero(string key)
        {
            return ConvertQueryStringToIntOrZero(key, false);
        }

        public static int ConvertQueryStringToIntOrZero(string key, bool decrypt)
        {
            return ConvertStringToIntOrZero(System.Web.HttpContext.Current.Request.QueryString[key], 0, decrypt);
        }
        
        public static int ConvertStringToIntOrZero(string input)
        {
            return ConvertStringToIntOrZero(input, 0);
        }

        public static int ConvertStringToIntOrZero(string input, bool decrypt)
        {
            return ConvertStringToIntOrZero(input, 0, decrypt);
        }

        public static int ConvertStringToIntOrZero(string input, int length, bool decrypt)
        {
            if (decrypt)
            {
                try
                {
                    input = HL.Library.Security.Decrypt(input);
                }
                catch
                {
                    return 0;
                }
            }
            return ConvertStringToIntOrZero(input, length);
        }
        
        public static int ConvertStringToIntOrZero(string input, int length)
        {
            int returnInt = 0;

            if (string.IsNullOrEmpty(input))
            {
                return returnInt;
            }

            if (length > 0)
            {
                AppSec.Validate.CheckLength(input, length);
            }

            try
            {
                returnInt = int.Parse(input);
            }
            catch
            {
                returnInt = 0;
            }
            
            return returnInt;
        }

        public static bool FindValueInCsv(string csv, string value, bool ignoreCase)
        {
            string[] csvArray = csv.Split(',');

            foreach (string csvValue in csvArray)
            {
                if (ignoreCase)
                {
                    if (csvValue.Trim().ToLower() == value.ToLower())
                    {
                        return true;
                    }
                }
                else
                {
                    if (csvValue.Trim() == value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool DoAgeCheck(DateTime startDate, int numDaysOld)
        {
            TimeSpan currentTimeSpan = DateTime.Now.Subtract(startDate);

            if (TimeSpan.Compare(currentTimeSpan, new TimeSpan(numDaysOld, 0, 0, 0)) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string RemoveHtmlTags(string html)
        {
            //  Remove the HTML tags
            System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.IgnoreCase;
            System.Text.RegularExpressions.MatchCollection m = System.Text.RegularExpressions.Regex.Matches(html, "<.*?>", options);

            for (int i = m.Count - 1; i >= 0; i--)
            {
                string tag = html.Substring(m[i].Index + 1, m[i].Length - 1).Trim().ToLower();
                html = html.Remove(m[i].Index, m[i].Length);
            }
            return html;
        }
	}
}
