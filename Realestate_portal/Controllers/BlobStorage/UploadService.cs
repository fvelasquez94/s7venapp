using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Realestate_portal.Controllers.BlobStorage
{
    public class UploadService
    {
        public async Task<string> UploadAsync(string fileToUpload, string urlpathdisk,string ContentType)
        {
            if (fileToUpload == null || fileToUpload.Length == 0)
            {
                return "";
            }
            try
            {
                CloudStorageAccount cloudStorageAccount = ConnectionString.GetConnectionString();
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("sevenfiles");
                await cloudBlobContainer.SetPermissionsAsync(
                       new BlobContainerPermissions
                       {
                           PublicAccess = BlobContainerPublicAccessType.Blob
                       }
                       );
                //if (await cloudBlobContainer.CreateIfNotExistsAsync())
                //{
                //    await cloudBlobContainer.SetPermissionsAsync(
                //        new BlobContainerPermissions
                //        {
                //            PublicAccess = BlobContainerPublicAccessType.Blob
                //        }
                //        );
                //}
                string imageName = fileToUpload;

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(imageName);
                cloudBlockBlob.Properties.ContentType = ContentType;
                await cloudBlockBlob.UploadFromFileAsync(urlpathdisk);

                return cloudBlockBlob.Uri.ToString();
            }
            catch (Exception ex)
            {
                return "Error uploading to Cloud: " + ex.Message;
            }

        }

        public async Task<bool> DeleteImageAsync(string filename)
        {
            if (filename == null || filename == "")
            {
                return false;
            }
            try
            {
                CloudStorageAccount cloudStorageAccount = ConnectionString.GetConnectionString();
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("sevenfiles");
                await cloudBlobContainer.SetPermissionsAsync(
                       new BlobContainerPermissions
                       {
                           PublicAccess = BlobContainerPublicAccessType.Blob
                       }
                       );

                

                CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(filename);//Changed path to fileName
                await blob.DeleteAsync();


            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}