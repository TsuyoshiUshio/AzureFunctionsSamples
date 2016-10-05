using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using System.IO;


namespace AzureBlobUploader
{
    class Program
    {
        private static string STORAGE_END_POINT = "{Put your Storage Endpoint in here}";
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please add FileName and Path!");
                return 1;
            }
            {
                var storageAccount = CloudStorageAccount.Parse(STORAGE_END_POINT);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("pictures");
                if (System.IO.File.Exists(args[0]))
                {
                    var fileName = Path.GetFileName((args[0]));
                    var blockBlob = container.GetBlockBlobReference(fileName);
                    blockBlob.UploadFromFile(args[0]);
                } else
                {
                    Console.WriteLine($"Can not the file: {args[0]}");
                }
                return 0;
            }
        }
    }
}
