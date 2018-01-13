using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Web.Caching;

namespace HL.Library
{
    /// <summary>
    /// Library.Settings:
    /// 
    /// Contains methods for retreiving Site Specific settings.  Settings are stored 
    /// in AppSettings.config, located in the path specified in the AppSettingsPath
    /// element of the web.config of the portal application
    /// </summary>
    public class Settings
    {
        private static string uniqueSettingPrefix = "vanhalen";

        /// <summary>
        ///  <para>Used to get an AppSetting from the appSettings.config file specified</para>
        ///  <para>in AppSettingsPath in the web.config file.</para>
        ///  <para>Option for encrypted settings</para>
        /// </summary>
        public static string GetAppSetting(string settingName)
        {
            return GetAppSetting(settingName, false);
        }

        /// <summary>
        ///  <para>Used to get an AppSetting from the appSettings.config file specified</para>
        ///  <para>in AppSettingsPath in the web.config file.</para>
        ///  <para>Option for encrypted settings</para>
        /// </summary>
		public static string GetAppSetting(string settingName, bool encrypted)
		{
			string appSetting = null;
            string appSettingsPath = System.Web.HttpContext.Current.Server.MapPath("/App_Data/appSettings.config");
            

			try
			{
                // try cache first
                appSetting = (string)System.Web.HttpContext.Current.Cache[uniqueSettingPrefix + settingName];

                // If null, get it
				if (appSetting == null)
				{
					XmlDocument xDoc = new XmlDocument();
					xDoc.Load(appSettingsPath);
					XmlNodeList xNodeList = xDoc.SelectNodes("SiteSettings/child::node()");
					foreach (XmlNode xNode in xNodeList)
					{
						if (xNode.NodeType != XmlNodeType.Comment)
						{
							if (settingName == xNode.Name)
							{
								appSetting = xNode.InnerText;
							}
							// Save setting in application cache with a dependency
                            System.Web.HttpContext.Current.Cache.Insert(uniqueSettingPrefix + xNode.Name, xNode.InnerText, new CacheDependency(appSettingsPath));
						}
					}
				}
			}
			catch (Exception ex)
			{
                throw new ApplicationException("Error getting appSetting " + settingName + " from " + appSettingsPath + ": " + ex.Message);
			}

            if (encrypted)
            {
                return DecryptString(appSetting);
            }
            else
            {
                return appSetting;
            }
		}    

        /// <summary>
        ///  Used to get settings specified in the appSetting section of the Web Config
        ///  like AppSettingsPath or Environment
        /// </summary>
        public static string GetSettingFromWebConfig(string setting)
        {
            return ConfigurationManager.AppSettings[setting];
        }

        public static string EncryptString(string cleartext)
        {
            return Library.Security.Encrypt(cleartext);
        }

        public static string DecryptString(string encryptedtext)
        {
            return Library.Security.Decrypt(encryptedtext);
        }
    }
}
