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
            int maxLength = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxLengthAPI"]);
            //Any
            rs.Add(new MessageModel { Status = 200, Function = "Any", Message = "Processed successfully", Description = "Xử lí thành công" });
            rs.Add(new MessageModel { Status = 201, Function = "Any", Message = "There is no access to this feature", Description = "Không có quyền truy cập vào tính năng này (Gửi yêu cầu quyền)" });
            rs.Add(new MessageModel { Status = 202, Function = "Any", Message = "Invalid token code", Description = "Mã xác thực không hợp lệ (Kiểm tra Token ở Header)" });
            rs.Add(new MessageModel { Status = 203, Function = "Any", Message = "Token code not found", Description = "Không tìm thấy mã xác thực (Kiểm tra Token ở Header)" });
            rs.Add(new MessageModel { Status = 204, Function = "Any", Message = "The input parameter is incomplete", Description = "Tham số đầu vào không đầy đủ (Kiểm tra các tham số truyền lên)" });
            rs.Add(new MessageModel { Status = 205, Function = "Any", Message = "An exception error occurred", Description = "Xảy ra lỗi ngoại lệ (Kiểm tra Exception trả về)" });
            rs.Add(new MessageModel { Status = 206, Function = "Any", Message = "PartnerKey not found", Description = "Không tìm thấy mã đối tác (Kiểm tra PartnerKey ở Header)" });
            rs.Add(new MessageModel { Status = 207, Function = "Any", Message = "PartnerKey does not exist", Description = "Mã đối tác không tồn tại (Kiểm tra PartnerKey ở Header)" });
            rs.Add(new MessageModel { Status = 208, Function = "Any", Message = "The maximum number of elements is " + maxLength + " items", Description = "Số lượng phần tử tối đa là "+ maxLength +" phần tử" });
            rs.Add(new MessageModel { Status = 0, Function = "Any", Message = "Exception erorr", Description = "Lỗi ngoại lệ" });

            //Patient
            rs.Add(new MessageModel { Status = 301, Function = "Patient", Message = "Data list patient can not be empty", Description = "Danh sách bệnh nhân không được để trống" });
            rs.Add(new MessageModel { Status = 302, Function = "Patient", Message = "VisitCode can not be empty", Description = "VisitCode không được để trống" });
            rs.Add(new MessageModel { Status = 303, Function = "Patient", Message = "HN can not be empty", Description = "HN không được để trống" });
            rs.Add(new MessageModel { Status = 304, Function = "Patient", Message = "VisitCode & HN already exist", Description = "VisitCode & HN đã tồn tại" });
            rs.Add(new MessageModel { Status = 305, Function = "Patient", Message = "BedCode can not be empty", Description = "BedCode không được để trống" });
            rs.Add(new MessageModel { Status = 306, Function = "Patient", Message = "Ward can not be empty", Description = "Phường không được để trống" });
            rs.Add(new MessageModel { Status = 307, Function = "Patient", Message = "Patient Full Name can not be empty", Description = "Họ và tên không được để trống" });
            rs.Add(new MessageModel { Status = 308, Function = "Patient", Message = "DoB can not be empty", Description = "Ngày sinh không được để trống" });
            rs.Add(new MessageModel { Status = 309, Function = "Patient", Message = "Nationality can not be empty", Description = "Quốc tịch không được để trống" });
            rs.Add(new MessageModel { Status = 310, Function = "Patient", Message = "Primary Doctor can not be empty", Description = "Bác sĩ chính không được để trống" });
            rs.Add(new MessageModel { Status = 311, Function = "Patient", Message = "Fasting From can not be empty", Description = "Ngày bắt đầu ăn kiêng không được để trống" });
            rs.Add(new MessageModel { Status = 312, Function = "Patient", Message = "VisitCode & HN does not exist", Description = "VisitCode & HN không tồn tại" });
            rs.Add(new MessageModel { Status = 313, Function = "Patient", Message = "BedCode does not exist", Description = "BedCode không tồn tại" });
            rs.Add(new MessageModel { Status = 314, Function = "Patient", Message = "Ward does not exist", Description = "Ward không tồn tại" });

            //DietaryPropertie
            rs.Add(new MessageModel { Status = 501, Function = "DietaryPropertie", Message = "Data list Dietary Properties can not be empty", Description = "Danh sách đặc tính dinh dưỡng không được để trống" });
            rs.Add(new MessageModel { Status = 502, Function = "DietaryPropertie", Message = "VisitCode can not be empty", Description = "VisitCode không được để trống" });
            rs.Add(new MessageModel { Status = 503, Function = "DietaryPropertie", Message = "HN can not be empty", Description = "HN không được để trống" });
            rs.Add(new MessageModel { Status = 504, Function = "DietaryPropertie", Message = "VisitCode & HN does not exist", Description = "VisitCode & HN không tồn tại" });
            rs.Add(new MessageModel { Status = 505, Function = "DietaryPropertie", Message = "ValidFrom can not be empty", Description = "Thời gian bắt đầu hiệu lực không được để trống" });
            rs.Add(new MessageModel { Status = 506, Function = "DietaryPropertie", Message = "ValidTo can not be empty", Description = "Thời gian hết hiệu lực không được để trống" });
            rs.Add(new MessageModel { Status = 507, Function = "DietaryPropertie", Message = "DietCode can not be empty", Description = "Mã chế độ ăn không được để trống" });
            rs.Add(new MessageModel { Status = 508, Function = "DietaryPropertie", Message = "DietCode does not exist", Description = "Mã chế độ ăn không tồn tại" });
            rs.Add(new MessageModel { Status = 509, Function = "DietaryPropertie", Message = "FoodCode does not exist", Description = "Mã thực phẩm không tồn tại" });
            rs.Add(new MessageModel { Status = 510, Function = "DietaryPropertie", Message = "VisitCode & HN already exist", Description = "VisitCode & HN đã tồn tại" });
            rs.Add(new MessageModel { Status = 511, Function = "DietaryPropertie", Message = "KitchenCode does not exist", Description = "Kitchen Code không tồn tại" });
            rs.Add(new MessageModel { Status = 512, Function = "DietaryPropertie", Message = "PantryCode does not exist", Description = "Pantry Code không tồn tại" });
            rs.Add(new MessageModel { Status = 513, Function = "DietaryPropertie", Message = "SnackCode does not exist", Description = "Snack Code không tồn tại" });

            //SYNC POS
            rs.Add(new MessageModel { Status = 601, Function = "SyncPOSProduct", Message = "Data list POS Product empty!", Description = "Danh sách product trống" });
            rs.Add(new MessageModel { Status = 602, Function = "SyncPOSProduct", Message = "ProductNum can not be empty!", Description = "ProductNum không được trống" });
            rs.Add(new MessageModel { Status = 603, Function = "SyncPOSProductCombo", Message = "Data list POS Product Combo empty!", Description = "Danh sách product combo trống" });
            rs.Add(new MessageModel { Status = 604, Function = "SyncPOSProductCombo", Message = "ProductComboID can not be empty!", Description = "ProductComboID không được trống" });
            rs.Add(new MessageModel { Status = 605, Function = "SyncPOSForcedChoice", Message = "Data list ForcedChoice empty!", Description = "Danh sách ForcedChoice trống" });
            rs.Add(new MessageModel { Status = 606, Function = "SyncPOSForcedChoice", Message = "UniqueID can not be empty!", Description = "UniqueID không được trống" });
            rs.Add(new MessageModel { Status = 607, Function = "SyncPOSQuestion", Message = "Data list ForcedChoice empty!", Description = "Danh sách ForcedChoice trống" });
            rs.Add(new MessageModel { Status = 608, Function = "SyncPOSQuestion", Message = "OptionIndex can not be empty!", Description = "OptionIndex không được trống" });
            rs.Add(new MessageModel { Status = 609, Function = "SyncPOSReportCat", Message = "Data list ReportCat empty!", Description = "Danh sách ReportCat trống" });
            rs.Add(new MessageModel { Status = 610, Function = "SyncPOSReportCat", Message = "ReportNo can not be empty!", Description = "ReportNo không được trống" });

            rs.Add(new MessageModel { Status = 611, Function = "POSProduct", Message = "ProductNum does not exists!", Description = "ProductNum không tồn tại!" });
            rs.Add(new MessageModel { Status = 612, Function = "POSProduct", Message = "Translation does not exists!", Description = "Translation không tồn tại!" });
            rs.Add(new MessageModel { Status = 613, Function = "POSProduct", Message = "TranslationType can not be empty!", Description = "TranslationType không được bỏ trống!" });
            rs.Add(new MessageModel { Status = 614, Function = "POSProduct", Message = "TranslationText can not be empty!", Description = "TranslationText không được bỏ trống!" });

            rs.Add(new MessageModel { Status = 615, Function = "POSProductCombo", Message = "ProductComboID does not exists!", Description = "ProductComboID không tồn tại!" });
            rs.Add(new MessageModel { Status = 616, Function = "POSProductCombo", Message = "Translation does not exists!", Description = "Translation không tồn tại!" });
            rs.Add(new MessageModel { Status = 617, Function = "POSProductCombo", Message = "TranslationType can not be empty!", Description = "TranslationType không được bỏ trống!" });
            rs.Add(new MessageModel { Status = 618, Function = "POSProductCombo", Message = "TranslationText can not be empty!", Description = "TranslationText không được bỏ trống!" });

            rs.Add(new MessageModel { Status = 619, Function = "ForcedChoices", Message = "UniqueID does not exists!", Description = "UniqueID không tồn tại!" });
            rs.Add(new MessageModel { Status = 620, Function = "ForcedChoices", Message = "Translation does not exists!", Description = "Translation không tồn tại!" });
            rs.Add(new MessageModel { Status = 621, Function = "ForcedChoices", Message = "TranslationType can not be empty!", Description = "TranslationType không được bỏ trống!" });
            rs.Add(new MessageModel { Status = 622, Function = "ForcedChoices", Message = "TranslationText can not be empty!", Description = "TranslationText không được bỏ trống!" });

            rs.Add(new MessageModel { Status = 623, Function = "Questions", Message = "OptionIndex does not exists!", Description = "OptionIndex không tồn tại!" });
            rs.Add(new MessageModel { Status = 624, Function = "Questions", Message = "Translation does not exists!", Description = "Translation không tồn tại!" });
            rs.Add(new MessageModel { Status = 625, Function = "Questions", Message = "TranslationType can not be empty!", Description = "TranslationType không được bỏ trống!" });
            rs.Add(new MessageModel { Status = 626, Function = "Questions", Message = "TranslationText can not be empty!", Description = "TranslationText không được bỏ trống!" });

            rs.Add(new MessageModel { Status = 627, Function = "ReportCats", Message = "ReportCatID does not exists!", Description = "ReportCatID không tồn tại!" });
            rs.Add(new MessageModel { Status = 628, Function = "ReportCats", Message = "Translation does not exists!", Description = "Translation không tồn tại!" });
            rs.Add(new MessageModel { Status = 629, Function = "ReportCats", Message = "TranslationType can not be empty!", Description = "TranslationType không được bỏ trống!" });
            rs.Add(new MessageModel { Status = 630, Function = "ReportCats", Message = "TranslationText can not be empty!", Description = "TranslationText không được bỏ trống!" });

            rs.Add(new MessageModel { Status = 631, Function = "SyncData", Message = "SyncID can not be empty!", Description = "SyncID không được bỏ trống!" });
            rs.Add(new MessageModel { Status = 632, Function = "SyncData", Message = "SyncID does not exists!", Description = "SyncID không tồn tại!" });

            rs.Add(new MessageModel { Status = 633, Function = "SyncPOSSaleType", Message = "Data list POS Sale Type can not be empty!", Description = "Danh sách POS Sale Type không được bỏ trống!" });
            rs.Add(new MessageModel { Status = 634, Function = "SyncPOSSaleType", Message = "SaleTypeIndex can not be empty!", Description = "SaleTypeIndex không được bỏ trống!" });

            rs.Add(new MessageModel { Status = 643, Function = "SyncPOSSysInfo", Message = "Data list POS SysInfo can not be empty!", Description = "Danh sách POS SysInfo không được bỏ trống!" });
            //Wards 
            rs.Add(new MessageModel { Status = 701, Function = "Wards", Message = "Ward list cannot be empty!", Description = "Danh sách Wards không được để trống!" });
            rs.Add(new MessageModel { Status = 702, Function = "Wards", Message = "WardID cannot be empty!", Description = "WardID không được để trống!" });
            rs.Add(new MessageModel { Status = 703, Function = "Wards", Message = "WardNameEn cannot be empty!", Description = "WardNameEn không được để trống!" });
            rs.Add(new MessageModel { Status = 704, Function = "Wards", Message = "WardNameVn cannot be empty!", Description = "WardNameVn không được để trống!" });
            rs.Add(new MessageModel { Status = 705, Function = "Wards", Message = "WardID does not exists!", Description = "WardID không tồn tại!" });
            rs.Add(new MessageModel { Status = 706, Function = "Wards", Message = "WardID already exissts!", Description = "WardID đã tồn tại!" });

            //Rooms 
            rs.Add(new MessageModel { Status = 801, Function = "Rooms", Message = "Room list cannot be empty!", Description = "Danh sách Rooms không được để trống!" });
            rs.Add(new MessageModel { Status = 802, Function = "Rooms", Message = "WardID cannot be empty!", Description = "WardID không được để trống!" });
            rs.Add(new MessageModel { Status = 805, Function = "Rooms", Message = "WardID does not exists!", Description = "WardID không tồn tại!" });
            rs.Add(new MessageModel { Status = 806, Function = "Rooms", Message = "WardID already exissts!", Description = "WardID đã tồn tại!" });
            rs.Add(new MessageModel { Status = 807, Function = "Rooms", Message = "RoomID cannot be empty!", Description = "RoomID không được để trống!" });
            rs.Add(new MessageModel { Status = 808, Function = "Rooms", Message = "RoomID already exissts!", Description = "RoomID đã tồn tại!" });
            rs.Add(new MessageModel { Status = 809, Function = "Rooms", Message = "RoomNameEn cannot be empty!", Description = "RoomNameEn không được để trống!" });
            rs.Add(new MessageModel { Status = 810, Function = "Rooms", Message = "RoomNameVn cannot be empty!", Description = "RoomNameVn không được để trống!" });
            rs.Add(new MessageModel { Status = 811, Function = "Rooms", Message = "RoomCode cannot be empty!", Description = "RoomCode không được để trống!" });
            rs.Add(new MessageModel { Status = 812, Function = "Rooms", Message = "RoomID does not exists!", Description = "RoomID không tồn tại!" });

            //Beds 
            rs.Add(new MessageModel { Status = 901, Function = "Beds", Message = "Bed list cannot be empty!", Description = "Danh sách Beds không được để trống!" });
            rs.Add(new MessageModel { Status = 902, Function = "Beds", Message = "BedID cannot be empty!", Description = "BedID không được để trống!" });
            rs.Add(new MessageModel { Status = 903, Function = "Beds", Message = "RoomID does not exists!", Description = "RoomID không tồn tại!" });
            rs.Add(new MessageModel { Status = 904, Function = "Beds", Message = "RoomID cannot be empty!", Description = "RoomID không được để trống!" });
            rs.Add(new MessageModel { Status = 905, Function = "Beds", Message = "BedID already exists!", Description = "BedID đã tồn tại!" });
            rs.Add(new MessageModel { Status = 906, Function = "Beds", Message = "BedName cannot be empty!", Description = "BedName không được để trống!" });
            rs.Add(new MessageModel { Status = 907, Function = "Beds", Message = "RoomID does not exists!", Description = "RoomID không tồn tại!" });
            rs.Add(new MessageModel { Status = 908, Function = "Beds", Message = "BedCode cannot be empty!", Description = "BedCode không được để trống!" });
            rs.Add(new MessageModel { Status = 909, Function = "Beds", Message = "BedID does not exists!", Description = "BedID không tồn tại!" });
            //Foods 
            rs.Add(new MessageModel { Status = 1001, Function = "Foods", Message = "Food list cannot be empty!", Description = "Danh sách Foods không được để trống!" });
            rs.Add(new MessageModel { Status = 1002, Function = "Foods", Message = "FoodID cannot be empty!", Description = "FoodID không được để trống!" });
            rs.Add(new MessageModel { Status = 1003, Function = "Foods", Message = "FoodCode cannot be empty!", Description = "FoodCode không được để trống!" });
            rs.Add(new MessageModel { Status = 1004, Function = "Foods", Message = "FoodName cannot be empty!", Description = "FoodName không được để trống!" });
            rs.Add(new MessageModel { Status = 1005, Function = "Foods", Message = "FoodCode does not exists!", Description = "FoodCode không tồn tại!" });
            rs.Add(new MessageModel { Status = 1006, Function = "Foods", Message = "FoodCode already exissts!", Description = "FoodCode đã tồn tại!" });
            //Pantrys 
            rs.Add(new MessageModel { Status = 1101, Function = "Pantrys", Message = "Pantry list cannot be empty!", Description = "Danh sách Pantrys không được để trống!" });
            rs.Add(new MessageModel { Status = 1102, Function = "Pantrys", Message = "PantryID cannot be empty!", Description = "PantryID không được để trống!" });
            rs.Add(new MessageModel { Status = 1103, Function = "Pantrys", Message = "PantryCode cannot be empty!", Description = "PantryCode không được để trống!" });
            rs.Add(new MessageModel { Status = 1104, Function = "Pantrys", Message = "PantryName cannot be empty!", Description = "PantryName không được để trống!" });
            rs.Add(new MessageModel { Status = 1105, Function = "Pantrys", Message = "PantryCode does not exists!", Description = "PantryCode không tồn tại!" });
            rs.Add(new MessageModel { Status = 1106, Function = "Pantrys", Message = "PantryCode already exissts!", Description = "PantryCode đã tồn tại!" });
            //Kitchens 
            rs.Add(new MessageModel { Status = 1201, Function = "Kitchens", Message = "Kitchen list cannot be empty!", Description = "Danh sách Kitchens không được để trống!" });
            rs.Add(new MessageModel { Status = 1202, Function = "Kitchens", Message = "KitchenID cannot be empty!", Description = "KitchenID không được để trống!" });
            rs.Add(new MessageModel { Status = 1203, Function = "Kitchens", Message = "KitchenCode cannot be empty!", Description = "KitchenCode không được để trống!" });
            rs.Add(new MessageModel { Status = 1204, Function = "Kitchens", Message = "KitchenName cannot be empty!", Description = "KitchenName không được để trống!" });
            rs.Add(new MessageModel { Status = 1205, Function = "Kitchens", Message = "KitchenCode does not exists!", Description = "KitchenCode không tồn tại!" });
            rs.Add(new MessageModel { Status = 1206, Function = "Kitchens", Message = "KitchenCode already exissts!", Description = "KitchenCode đã tồn tại!" });
            //Snacks 
            rs.Add(new MessageModel { Status = 1301, Function = "Snacks", Message = "Snack list cannot be empty!", Description = "Danh sách Snacks không được để trống!" });
            rs.Add(new MessageModel { Status = 1302, Function = "Snacks", Message = "SnackID cannot be empty!", Description = "SnackID không được để trống!" });
            rs.Add(new MessageModel { Status = 1303, Function = "Snacks", Message = "SnackCode cannot be empty!", Description = "SnackCode không được để trống!" });
            rs.Add(new MessageModel { Status = 1304, Function = "Snacks", Message = "SnackName cannot be empty!", Description = "SnackName không được để trống!" });
            rs.Add(new MessageModel { Status = 1305, Function = "Snacks", Message = "SnackCode does not exists!", Description = "SnackCode không tồn tại!" });
            rs.Add(new MessageModel { Status = 1306, Function = "Snacks", Message = "SnackCode already exists!", Description = "SnackCode đã tồn tại!" });
            //Logins 
            rs.Add(new MessageModel { Status = 1401, Function = "Login", Message = "LoginType cannot be empty!", Description = "Loại Login không được để trống!" });
            rs.Add(new MessageModel { Status = 1402, Function = "Login", Message = "UserName cannot be empty!", Description = "UserName không được để trống!" });
            rs.Add(new MessageModel { Status = 1403, Function = "Login", Message = "Password cannot be empty!", Description = "Password không được để trống!" });
            rs.Add(new MessageModel { Status = 1404, Function = "Login", Message = "Email cannot be empty!", Description = "Email không được để trống!" });
            rs.Add(new MessageModel { Status = 1405, Function = "Login", Message = "EmployeeCode does not exists!", Description = "EmployeeCode không tồn tại!" });
            rs.Add(new MessageModel { Status = 1406, Function = "Login", Message = "EmployeeCode cannot be empty!", Description = "EmployeeCode không được để trống!" });
            rs.Add(new MessageModel { Status = 1407, Function = "Login", Message = "Login information is incorrect!", Description = "Thông tin Login không chính xác!" });
            //Tranlations 
            rs.Add(new MessageModel { Status = 1501, Function = "Translation", Message = "Tranlation Name cannot be empty!", Description = "Tranlations Name không được để trống!" });
            rs.Add(new MessageModel { Status = 1502, Function = "Translation", Message = "Tranlation Name already exists!", Description = "Tranlation Name đã tồn tại!" });
            rs.Add(new MessageModel { Status = 1503, Function = "Translation", Message = "TransactionID cannot be empty!", Description = "TransactionID không được để trống" });
            rs.Add(new MessageModel { Status = 1504, Function = "Translation", Message = "TransactionID does not exists!", Description = "TransactionID không tồn tại" });
            rs.Add(new MessageModel { Status = 1405, Function = "Login", Message = "EmployeeCode does not exists!", Description = "EmployeeCode không tồn tại!" });
            rs.Add(new MessageModel { Status = 1406, Function = "Login", Message = "EmployeeCode cannot be empty!", Description = "EmployeeCode không được để trống!" });
            rs.Add(new MessageModel { Status = 1407, Function = "Login", Message = "Login information is incorrect!", Description = "Thông tin Login không chính xác!" });
            return rs;
        }
    }
}