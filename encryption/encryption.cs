using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace Site.Library
{
	/// <summary>
	/// Summary description for encdec.
	/// </summary>
    public class Security
	{
        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        private static string password = "If a quiz is quizzical, whats a test?";

        public static string Encrypt(string plaintext)
        {
            // password
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password); //read with UTF8 encoding the password.
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes); //hash the psw

            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(plaintext); //read bytes to encrypt them 

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            return Convert.ToBase64String(bytesEncrypted);
            //return Encoding.UTF8.GetString(bytesEncrypted);
        }

        public static string Decrypt(string ciphertext)
        {
            // password
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password); //read with UTF8 encoding the password.
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes); //hash the psw

            byte[] bytesToBeDecrypted = Convert.FromBase64String(ciphertext);            
            //byte[] bytesToBeDecrypted = Encoding.UTF8.GetBytes(ciphertext);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);
            return Encoding.UTF8.GetString(bytesDecrypted);
        }
        
        public static string EncryptAndUrlEncode(string plaintext)
        {
            return System.Web.HttpUtility.UrlEncode(Encrypt(plaintext));
        }
        public static string DecryptAndUrlDecode(string ciphertext)
        {
            return Decrypt(System.Web.HttpUtility.UrlDecode(ciphertext));
        }

    }

}
