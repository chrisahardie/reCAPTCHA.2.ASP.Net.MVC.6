using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace reCAPTCHA
{
    public class CryptUtils
    {        
        public static byte[] GetKey(string siteSecret)
        {
            byte[] key = Encoding.UTF8.GetBytes(siteSecret);
            return SHA1.Create().ComputeHash(key).Take(16).ToArray();
        }

        public static string CreateToken(string siteSecret)
        {
            if (String.IsNullOrWhiteSpace(siteSecret)){
                throw new ArgumentNullException(nameof(siteSecret));
            }
            var sessionId = Guid.NewGuid().ToString();
            var token = CreateJsonToken(sessionId);
            return EncryptText(token, siteSecret);
        }

        private static string CreateJsonToken(string sessionId)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = DateTime.UtcNow - epoch;
            var diffInMs = Convert.ToInt64(diff.TotalMilliseconds);

            dynamic token = new ExpandoObject();
            token.session_id = sessionId;
            token.ts_ms = diffInMs;

            return JsonConvert.SerializeObject(token);
        }

        public static string EncryptText(string plainText, string siteSecret)
        {
            char[] padding = { '=' };
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = GetKey(siteSecret);

                ICryptoTransform encrypto = aes.CreateEncryptor();

                byte[] plainTextByte = ASCIIEncoding.UTF8.GetBytes(plainText);
                byte[] CipherText = encrypto.TransformFinalBlock(plainTextByte, 0, plainTextByte.Length);
                return Convert.ToBase64String(CipherText).TrimEnd(padding).Replace('+', '-').Replace('/', '_');
            }
        }

        public static string DecryptText(string input, string siteSecret)
        {
            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = GetKey(siteSecret);
                var decryptor = aes.CreateDecryptor();

                string originalBase64 = input.Replace('_', '/').Replace('-', '+');
                switch (input.Length % 4)
                {
                    case 2: originalBase64 += "=="; break;
                    case 3: originalBase64 += "="; break;
                }
                byte[] bytes = Convert.FromBase64String(originalBase64);

                var text = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                return ASCIIEncoding.UTF8.GetString(text);
            }
        }
    }
}
