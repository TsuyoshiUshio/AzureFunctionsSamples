#r "Microsoft.WindowsAzure.Storage"
#r "System.Runtime"
#r "System.Threading.Tasks"
#r "System.IO"
#r "System.Data"
using System;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Data.SqlClient;

private static string FACE_API_SUBSCRIPTION_KEY = "{Put your face api subscription in here}";
private static string CONNECTION_STRING = "{put your SQL Server DB connection String here}";

public async static Task Run(CloudBlockBlob myBlob, TraceWriter log)
{
    var uri = myBlob.Uri;
    log.Info($"C# Blob trigger function processed: {uri}");
    await DetectFaces(myBlob, log);
}

private async static Task DetectFaces(CloudBlockBlob myBlob, TraceWriter log)
{
    var uri = myBlob.Uri.ToString();
    IFaceServiceClient faceServiceClient = new FaceServiceClient(FACE_API_SUBSCRIPTION_KEY); 
    FaceAttributeType[] faceAttributes = { FaceAttributeType.Age, FaceAttributeType.FacialHair, FaceAttributeType.Gender, FaceAttributeType.Glasses, FaceAttributeType.HeadPose, FaceAttributeType.Smile };
    var faces = await faceServiceClient.DetectAsync(uri, true, false, faceAttributes);
    faceDump(faces, log);
    await saveCustomer(faces);
}

private static async Task saveCustomer(Face[] faces)
{
    foreach(var face in faces)
    {
        var customer = new Customer();
        customer.Gender = face.FaceAttributes.Gender;
        customer.Age = (float)face.FaceAttributes.Age;
        customer.Smile = (float)face.FaceAttributes.Smile;
        customer.Mustache = (float)face.FaceAttributes.FacialHair.Moustache;
        customer.Beard = (float)face.FaceAttributes.FacialHair.Beard;
        customer.SideBurns = (float)face.FaceAttributes.FacialHair.Sideburns;
        await insertRecord(customer);
    }
}

        private static void  faceDump(Face[] faces, TraceWriter log)
        {
            foreach (var face in faces)
            {
                log.Info($"age: {face.FaceAttributes.Age}");
                log.Info($"gender: {face.FaceAttributes.Gender}");
                log.Info($"smile: {face.FaceAttributes.Smile}");
                log.Info($"facialHair.mustache: {face.FaceAttributes.FacialHair.Moustache}");
                log.Info($"facialHair.beard: {face.FaceAttributes.FacialHair.Beard}");
                log.Info($"facialHair.sideburns: {face.FaceAttributes.FacialHair.Sideburns}");
            }
        }

    private class Customer
    {
        public int CustomerId { get; set; }
        public string Gender { get; set; }
        public float Age { get; set; }
        public float Smile { get; set; }
        public float Mustache { get; set; }
        public float Beard { get; set; }
        public float SideBurns { get; set; }
    }        

    private static async Task insertRecord(Customer customer)
    {
        using (var conn = new SqlConnection(CONNECTION_STRING))
        {
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText = @"INSERT Customers(Gender, Age, Smile, Mustache, Beard, SideBurns) VALUES (@Gender, @Age, @Smile, @Mustache, @Beard, @SideBurns)";
            command.Parameters.AddWithValue("@Gender", customer.Gender);
            command.Parameters.AddWithValue("@Age", customer.Age);
            command.Parameters.AddWithValue("@Smile", customer.Smile);
            command.Parameters.AddWithValue("@Mustache", customer.Mustache);
            command.Parameters.AddWithValue("@Beard", customer.Beard);
            command.Parameters.AddWithValue("@SideBurns", customer.SideBurns);
            await command.ExecuteScalarAsync();
        }
    }