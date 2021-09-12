using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Realestate_portal.Controllers.BlobStorage
{
    public class ConnectionString
    {
        
            static string account = ConfigurationManager.AppSettings["StorageAccountName"];
            static string key = ConfigurationManager.AppSettings["StorageAccountKey"];
            public static CloudStorageAccount GetConnectionString()
            {
                string connectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", account, key);
                return CloudStorageAccount.Parse(connectionString);
            }
        
    }
}