using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace SpeedTOHAPI.Codes
{
    public class GetCloud
    {
        public static APIResult GetStoreByGUID()
        {
            APIResult result = new APIResult();
            string StoreGUID = System.Configuration.ConfigurationManager.AppSettings["StoreGUIDTickX"].ToString();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(System.Configuration.ConfigurationManager.AppSettings["ServerTickX"].ToString() + "/api/Store?GUID=" + StoreGUID + "");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 100000;
            httpWebRequest.Headers.Add("PartnerGUID", System.Configuration.ConfigurationManager.AppSettings["PartnerKeyTickX"].ToString());
            httpWebRequest.Headers.Add("Token", System.Configuration.ConfigurationManager.AppSettings["TokenTickX"].ToString());
            httpWebRequest.Headers.Add("CurrentUTC", System.Configuration.ConfigurationManager.AppSettings["CurrentUTCTickX"].ToString());
            httpWebRequest.Headers.Add("ClientGUID", System.Configuration.ConfigurationManager.AppSettings["ClientGUIDTickX"].ToString());

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var srPayPos = new StreamReader(httpResponse.GetResponseStream()))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<APIResult>(srPayPos.ReadToEnd());
            }
        }
    }
}