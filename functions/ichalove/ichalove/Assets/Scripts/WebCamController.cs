using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine.UI;
using System;
using Newtonsoft.Json.Linq;

public class WebCamController : MonoBehaviour {

    private static string UPLOADER_FILE = "C:\\Users\\tsushi\\Codes\\FujimotoApps\\AzureBlobUploader.exe"; // Edit this
    private static string SUBSCRIPTION_KEY = "{Put your subscription key in here}";

    public WebCamTexture webcamTexture;
    public Color32[] color32;
    GameObject girl;
    private Image backGroundImg;

    private Sprite restaurant;
    private Sprite night;
    private Sprite bar;

    //public Text res;
    // Use this for initialization
    void Start () {
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();
        GetComponent<Renderer>().material.mainTexture = webcamTexture;
        girl = GameObject.Find("Girl");
        backGroundImg = GameObject.Find("Canvas/Image").GetComponent<Image>();
        restaurant = Resources.Load<Sprite>("rissyoku00a");
        bar = Resources.Load<Sprite>("bar00");
        night = Resources.Load<Sprite>("bed2ira");
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            color32 = webcamTexture.GetPixels32();

            Texture2D texture = new Texture2D(webcamTexture.width, webcamTexture.height);
            GameObject.Find("ImageQuad").GetComponent<Renderer>().material.mainTexture = texture;
            
            texture.SetPixels32(color32);
            texture.Apply();
            byte[] bytes = texture.EncodeToPNG();
            savePhoto(bytes);
            StartCoroutine(WaitForRes(bytes));
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            backGroundImg.sprite = restaurant;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            backGroundImg.sprite = night;
        }

    }
    System.Diagnostics.Process process;
    private void savePhoto(byte[] image)
    {
        var date = DateTime.Now;
        var fileName = date.ToString("yyyyMMddHHmm");
        var path = System.IO.Path.Combine(Application.persistentDataPath, fileName + ".png");
        File.WriteAllBytes(path, image);
        process = new System.Diagnostics.Process();
        process.StartInfo.FileName = UPLOADER_FILE;
        process.StartInfo.Arguments = path;
        process.Start();
    }

    // WWWクラスを使って Emotion API を呼び出す関数
    IEnumerator WaitForRes(byte[] bytes)
    {
        // Emotion REST API
        string url = "https://api.projectoxford.ai/emotion/v1.0/recognize";

        // リクエストヘッダー
        Dictionary<string, string> header = new Dictionary<string, string>();
        header.Add("Content-Type", "application/octet-stream");
        header.Add("Ocp-Apim-Subscription-Key", SUBSCRIPTION_KEY);
        
        // bytesはカメラ画像
        WWW www = new WWW(url, bytes, header);
        // 非同期なのでレスポンスを待つ
        yield return www;
        Debug.Log(www.error);

        // エラーじゃなければ解析結果のJSONを取得
        if (www.error == null)
        {
      //      Debug.Log(www.text);
            var emotionJsonArray = JArray.Parse(www.text);
            JObject scores = (JObject) emotionJsonArray[0]["scores"];
            string emotion = getEmotion(scores);
            Girl g = girl.GetComponent<Girl>();
            g.SetFace(emotion);
        }
    }

      //"anger": 0.00300731952,
      //"contempt": 5.14648448E-08,
      //"disgust": 9.180124E-06,
      //"fear": 0.0001912825,
      //"happiness": 0.9875571,
      //"neutral": 0.0009861537,
      //"sadness": 1.889955E-05,
      //"surprise": 0.008229999

    public string getEmotion(JObject obj)
    {
        string strongEmotion = "";
        double score = 0;
        foreach (var x in obj)
        {
            string name = x.Key;
            double value = Convert.ToDouble(x.Value);

            if (value > score)
            {
                score = value;
                strongEmotion = name;
            }
        }
        Debug.Log(strongEmotion);
        return strongEmotion;
    }
}
