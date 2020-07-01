using MerakiHAWatch.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MerakiHAWatch.Az
{
    public class AzMonLogger : ILog
    {
        private string _workspaceId;
        private string _workspaceKey;

        public AzMonLogger(string workspaceId, string workspaceKey)
        {
            _workspaceId = workspaceId;
            _workspaceKey = workspaceKey;
        }

        public void Log(string logName, string json)
        {
            if (String.IsNullOrEmpty(_workspaceId) || String.IsNullOrEmpty(_workspaceKey)) return;
            try
            {
                // Create a hash for the API signature
                var datestring = DateTime.UtcNow.ToString("r");
                string stringToHash = "POST\n" + json.Length + "\napplication/json\n" + "x-ms-date:" + datestring + "\n/api/logs";
                string hashedString = BuildSignature(stringToHash, _workspaceKey);
                string signature = "SharedKey " + _workspaceId + ":" + hashedString;
                // post the json to Log Analytics
                PostData(_workspaceId, logName, signature, datestring, json);
            }
            catch (Exception)
            {               
                // do nothing, fail silently
            }
        }



        // Build the API signature
        private string BuildSignature(string message, string secret)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = Convert.FromBase64String(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hash = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Send a request to the POST API endpoint
        private void PostData(string omsWorkspaceId, string logName, string signature, string date, string json)
        {
                string url = "https://" + omsWorkspaceId + ".ods.opinsights.azure.com/api/logs?api-version=2016-04-01";

                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Log-Type", logName);
                client.DefaultRequestHeaders.Add("Authorization", signature);
                client.DefaultRequestHeaders.Add("x-ms-date", date);
                client.DefaultRequestHeaders.Add("time-generated-field", "");

                System.Net.Http.HttpContent httpContent = new StringContent(json, Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                Task<System.Net.Http.HttpResponseMessage> response = client.PostAsync(new Uri(url), httpContent);

                System.Net.Http.HttpContent responseContent = response.Result.Content;
                string result = responseContent.ReadAsStringAsync().Result;
            
        }
    }
}


