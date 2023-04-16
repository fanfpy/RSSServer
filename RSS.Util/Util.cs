using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace RSS.Web.Util
{
    public class Util
    {
        static readonly HttpClient client = new HttpClient();
        

        public string GetTime(DateTime dateTime)
        {

            
            try {
                var time = DateTime.Now - dateTime;

                if (time.TotalHours > 24)
                {
                    return Math.Floor(time.TotalDays) + "天前";
                }
                if (time.TotalHours > 1)
                {
                    return Math.Floor(time.TotalHours) + "小时前";
                }
                if (time.TotalMinutes > 1)
                {
                    return Math.Floor(time.TotalMinutes) + "分钟前";
                }
                return Math.Floor(time.TotalSeconds) + "秒前";
            } catch (Exception) { return "0秒前"; }
            
         }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static bool SendPushplus(string title, string content) 
        {

           var json = new
            {
                msgtype = "text",
                text = new { content = title + "\n" + content }
            };

            string jsonString = JsonSerializer.Serialize(json);

            client.PostAsync("https://qyapi.weixin.qq.com/cgi-bin/webhook/send?key=769d6571-30c5-4506-9b56-1ef3156da86f", new StringContent(jsonString, Encoding.UTF8, "application/json"));


            return true;
        }

        private static string GetXmlEncoding(string xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString)) throw new ArgumentException("The provided string value is null or empty.");

            using (var stringReader = new StringReader(xmlString))
            {
                var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };

                using (var xmlReader = XmlReader.Create(stringReader, settings))
                {
                    if (!xmlReader.Read())
                        throw new ArgumentException("The provided XML string does not contain enough data to be valid XML (see https://msdn.microsoft.com/en-us/library/system.xml.xmlreader.read)");

                    var result = xmlReader.GetAttribute("encoding");
                    return result;
                }
            }
        }
    }
}
