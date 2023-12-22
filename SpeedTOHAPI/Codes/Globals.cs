using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using SpeedTOHAPI.Models;

namespace SpeedTOHAPI.Codes
{
    public class Globals
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static void WriteLog(string LogType, string LogText)
        {
            try
            {
                string LogPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Logs/" + LogType);
                if (!Directory.Exists(LogPath))
                    Directory.CreateDirectory(LogPath);
                StreamWriter sw = new StreamWriter(LogPath + "/" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + (new Random()).Next(1, 10000) + ".log");
                sw.Write(LogText);
                sw.Close();
            }
            catch (Exception ex)
            {

            }
        }

        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }


        public static DataTable LoopPriority(DataTable dtTaxConfig, int Data, int Priority, int index)
        {
            string ApplyTop = Convert.ToString(Data, 2).PadLeft(6, '0');
            int priTemp = int.Parse(dtTaxConfig.Rows[index]["Priority"].ToString());
            dtTaxConfig.Rows[index]["Priority"] = priTemp + Priority + 1;
            if (ApplyTop[4] == '1' && index != 0)
                return LoopPriority(dtTaxConfig, int.Parse(dtTaxConfig.Rows[0]["Data"].ToString()), int.Parse(dtTaxConfig.Rows[0]["Priority"].ToString()), 0);
            if (ApplyTop[3] == '1' && index != 1)
                return LoopPriority(dtTaxConfig, int.Parse(dtTaxConfig.Rows[1]["Data"].ToString()), int.Parse(dtTaxConfig.Rows[1]["Priority"].ToString()), 1);
            if (ApplyTop[2] == '1' && index != 2)
                return LoopPriority(dtTaxConfig, int.Parse(dtTaxConfig.Rows[2]["Data"].ToString()), int.Parse(dtTaxConfig.Rows[2]["Priority"].ToString()), 2);
            if (ApplyTop[1] == '1' && index != 3)
                return LoopPriority(dtTaxConfig, int.Parse(dtTaxConfig.Rows[3]["Data"].ToString()), int.Parse(dtTaxConfig.Rows[3]["Priority"].ToString()), 3);
            if (ApplyTop[0] == '1' && index != 4)
                return LoopPriority(dtTaxConfig, int.Parse(dtTaxConfig.Rows[4]["Data"].ToString()), int.Parse(dtTaxConfig.Rows[4]["Priority"].ToString()), 4);
            return dtTaxConfig;
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string RSAEncryption(string strText)
        {
            var publicKey = "<RSAKeyValue><Modulus>cqndQeH+clUoOn+cKRB3K/sRtX6TOfqu2vjeLSPSc+SzpI52yOA4BedE7dp2tlA9A46pi0WP18HOCAoZXy4qHA2ri7DOnsKX8Mg1Vr2KPAMl3YFWqhk/S99+4a/dpKDwrnRNi5kv0i2mllN5x5ZcZ9E7Y1e8nm9FGdIJCxA+XiM=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            var testData = Encoding.UTF8.GetBytes(strText);
            using (var rsa = new RSACryptoServiceProvider(1024))
            {
                try
                {
                    rsa.FromXmlString(publicKey.ToString());
                    var encryptedData = rsa.Encrypt(testData, true);
                    var base64Encrypted = Convert.ToBase64String(encryptedData);
                    return base64Encrypted;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public static string RSADecryption(string strText)
        {
            var privateKey = "<RSAKeyValue><Modulus>cqndQeH+clUoOn+cKRB3K/sRtX6TOfqu2vjeLSPSc+SzpI52yOA4BedE7dp2tlA9A46pi0WP18HOCAoZXy4qHA2ri7DOnsKX8Mg1Vr2KPAMl3YFWqhk/S99+4a/dpKDwrnRNi5kv0i2mllN5x5ZcZ9E7Y1e8nm9FGdIJCxA+XiM=</Modulus><Exponent>AQAB</Exponent><P>zuYlR922YFu/MakBNDKCHn83FkYMQCaFvcDoUX4TZ4R2Qjg+acUjXzScV41Ul/mWedBwlXcGQ/epoB4OsOQkxQ==</P><Q>jeAVdokpxC+pKhKTAGFEXq7Z4Sji6UUrhf3ARcfa4v7hQEMqTlcui7jp9/kCz25feCpmzCPjg1E26mkWRLU1xw==</Q><DP>YHvO8t6fx/vBA4WOvCq5p0MoC0kLOXc9cyncrPQgVGvfQi48XNLEFgfQyLttsZmA5LmhZvIkh9mczsB1lWQvCQ==</DP><DQ>RP81cPBD36VOH6fo1cZ3+ZQPYfEAaXG6OO+vEkCfssVBxn7jlDXR7SGAp5fyRe7nfwkf9Sd+/d4BVv7EVaXLAQ==</DQ><InverseQ>grNU3qASSC4QYF7X6BB+lxIP3rHbaN0zSeTJtt0jJMNHA48PDv6FrGMj6KPWK0pDDPxKrTdEXD5JixSc8iR+gg==</InverseQ><D>B6P4AV7cxKOWBafhMP9O4ZheSri/eLqSkjbJHzrm2CAiNFHl6ma+dO4/MpY/GNDp7+W+uHAPMLJSV0jM/gGmfpbRAP7WGOaRMToBNwxHV/dwVqnNzjAS6pd8TJGt8lF6AbQla3uSABbyG/YXb59BXKEivPDOuCoFbY+tQTb/Tek=</D></RSAKeyValue>";
            var testData = Encoding.UTF8.GetBytes(strText);
            using (var rsa = new RSACryptoServiceProvider(1024))
            {
                try
                {
                    var base64Encrypted = strText;
                    rsa.FromXmlString(privateKey);
                    var resultBytes = Convert.FromBase64String(base64Encrypted);
                    var decryptedBytes = rsa.Decrypt(resultBytes, true);
                    var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                    return decryptedData.ToString();
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public static bool VerifyMessage(string originalMessage, string signedMessage)
        {
            bool verified;
            try
            {
                var publicKey = "<RSAKeyValue><Modulus>cqndQeH+clUoOn+cKRB3K/sRtX6TOfqu2vjeLSPSc+SzpI52yOA4BedE7dp2tlA9A46pi0WP18HOCAoZXy4qHA2ri7DOnsKX8Mg1Vr2KPAMl3YFWqhk/S99+4a/dpKDwrnRNi5kv0i2mllN5x5ZcZ9E7Y1e8nm9FGdIJCxA+XiM=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
                rsa.FromXmlString(publicKey);
                verified = rsa.VerifyData(Encoding.UTF8.GetBytes(originalMessage), CryptoConfig.MapNameToOID("SHA1"), Convert.FromBase64String(signedMessage));
            }
            catch (Exception)
            {
                verified = false;
            }

            return verified;
        }

        public static string ObjectToJsonString(object obj)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(obj);
            return JSONString;
        }

        public static string SignMessage(string message)
        {
            string signedMessage;
            try
            {
                var privateKey = "<RSAKeyValue><Modulus>cqndQeH+clUoOn+cKRB3K/sRtX6TOfqu2vjeLSPSc+SzpI52yOA4BedE7dp2tlA9A46pi0WP18HOCAoZXy4qHA2ri7DOnsKX8Mg1Vr2KPAMl3YFWqhk/S99+4a/dpKDwrnRNi5kv0i2mllN5x5ZcZ9E7Y1e8nm9FGdIJCxA+XiM=</Modulus><Exponent>AQAB</Exponent><P>zuYlR922YFu/MakBNDKCHn83FkYMQCaFvcDoUX4TZ4R2Qjg+acUjXzScV41Ul/mWedBwlXcGQ/epoB4OsOQkxQ==</P><Q>jeAVdokpxC+pKhKTAGFEXq7Z4Sji6UUrhf3ARcfa4v7hQEMqTlcui7jp9/kCz25feCpmzCPjg1E26mkWRLU1xw==</Q><DP>YHvO8t6fx/vBA4WOvCq5p0MoC0kLOXc9cyncrPQgVGvfQi48XNLEFgfQyLttsZmA5LmhZvIkh9mczsB1lWQvCQ==</DP><DQ>RP81cPBD36VOH6fo1cZ3+ZQPYfEAaXG6OO+vEkCfssVBxn7jlDXR7SGAp5fyRe7nfwkf9Sd+/d4BVv7EVaXLAQ==</DQ><InverseQ>grNU3qASSC4QYF7X6BB+lxIP3rHbaN0zSeTJtt0jJMNHA48PDv6FrGMj6KPWK0pDDPxKrTdEXD5JixSc8iR+gg==</InverseQ><D>B6P4AV7cxKOWBafhMP9O4ZheSri/eLqSkjbJHzrm2CAiNFHl6ma+dO4/MpY/GNDp7+W+uHAPMLJSV0jM/gGmfpbRAP7WGOaRMToBNwxHV/dwVqnNzjAS6pd8TJGt8lF6AbQla3uSABbyG/YXb59BXKEivPDOuCoFbY+tQTb/Tek=</D></RSAKeyValue>";
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
                rsa.FromXmlString(privateKey);
                signedMessage = Convert.ToBase64String(rsa.SignData(Encoding.UTF8.GetBytes(message), CryptoConfig.MapNameToOID("SHA1")));
            }
            catch (Exception)
            {
                signedMessage = string.Empty;
            }
            return signedMessage;

        }

        public static string HMACSHA256(string text, string key)
        {
            Encoding encoding = Encoding.UTF8;
            Byte[] textBytes = encoding.GetBytes(text);
            Byte[] keyBytes = encoding.GetBytes(key);
            Byte[] hashBytes;
            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }


        public static string ConvertToUnsign(string str)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = str.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty)
                        .Replace('\u0111', 'd').Replace('\u0110', 'D').Replace("'", "''");
        }

        public static string CUTSTRING(string strText, int MaxLength)
        {
            return (strText.Length <= MaxLength ? strText : strText.Substring(0, MaxLength));
        }

        public static string VARCHARNULL(string strText)
        {
            return strText != "" ? "'" + strText + "'" : "NULL";
        }

        public static object ByteToObject(byte[] value)
        {
            MemoryStream ms1 = new MemoryStream(value);
            BinaryFormatter bf1 = new BinaryFormatter();
            ms1.Position = 0;
            return bf1.Deserialize(ms1);
        }

        public static byte[] ObjectToByte(Object obj)
        {
            MemoryStream ms1 = new MemoryStream();
            BinaryFormatter bf1 = new BinaryFormatter();
            bf1.Serialize(ms1, obj);
            Console.WriteLine(ms1.ToArray().Length.ToString());
            return ms1.ToArray();
        }

        public static List<MessageModel> GetStatusCode()
        {
            List<MessageModel> rs = new List<MessageModel>();

            //Any
            rs.Add(new MessageModel { Status = 200, Function = "Any", Message = "OK", Description = "Xử lí thành công" });
            rs.Add(new MessageModel { Status = 201, Function = "Any", Message = "There is no access to this feature", Description = "Không có quyền truy cập vào tính năng này (Gửi yêu cầu quyền)" });
            rs.Add(new MessageModel { Status = 202, Function = "Any", Message = "Invalid token code", Description = "Mã xác thực không hợp lệ (Kiểm tra Token ở Header)" });
            rs.Add(new MessageModel { Status = 203, Function = "Any", Message = "Token code not found", Description = "Không tìm thấy mã xác thực (Kiểm tra Token ở Header)" });
            rs.Add(new MessageModel { Status = 204, Function = "Any", Message = "The input parameter is incomplete", Description = "Tham số đầu vào không đầy đủ (Kiểm tra các tham số truyền lên)" });
            rs.Add(new MessageModel { Status = 205, Function = "Any", Message = "An exception error occurred", Description = "Xảy ra lỗi ngoại lệ (Kiểm tra Exception trả về)" });
            rs.Add(new MessageModel { Status = 206, Function = "Any", Message = "PartnerKey not found", Description = "Không tìm thấy mã đối tác (Kiểm tra PartnerKey ở Header)" });

            return rs;
        }
    }
}