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
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please add FileName and Path!");
                return 1;
            }
            {
                var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=pooryou;AccountKey=qLUkcGy//+Fg/B4tgXEevDKnJjEcDsA8sn4vEGD46o6skvfsvpzdCcWt8L1Jk3oNxA05uqvQmGTcCVaF7iQ3ww==");
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
