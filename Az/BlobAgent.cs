using Azure.Storage.Blobs;
using MerakiHAWatch.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace MerakiHAWatch.Az
{
    public class BlobAgent: IPersistFailover
    {

        private BlobServiceClient _blobServiceClient;
        private string _containername;
        private string _blobname;

        public BlobAgent(string connectionString, string containername, string blobname)
        {
            if (String.IsNullOrEmpty(connectionString)){
                throw new Exception("Storage connection string cannot be blank");
            }
            if (String.IsNullOrEmpty(containername))
            {
                throw new Exception("Storage container name cannot be blank");
            }
            if (String.IsNullOrEmpty(blobname))
            {
                throw new Exception("Storage blob name (Load Balancer Name) cannot be blank");
            }
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containername = containername;
            _blobname = blobname;
        }


        public VmxHaPair GetNodes()
        {
            // download blob            
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containername);
            var blobClient = containerClient.GetBlobClient(_blobname);
            Stream target = new MemoryStream();
            blobClient.DownloadTo(target);
            target.Position = 0;
            StreamReader reader = new StreamReader(target);
            string text = reader.ReadToEnd();
            // deserialize into VmxHaPair
            VmxHaPair record = JsonConvert.DeserializeObject<VmxHaPair>(text);
            return record;                        
        }

        public void SaveNodes(VmxHaPair record)
        {
            // serialize and convert to byte array
            string json = JsonConvert.SerializeObject(record);
            byte[] bytes = Encoding.ASCII.GetBytes(json);
            Stream source = new MemoryStream(bytes);
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containername);
            var blobClient = containerClient.GetBlobClient(_blobname);
            blobClient.Upload(source, true);            
        }
    }
}
