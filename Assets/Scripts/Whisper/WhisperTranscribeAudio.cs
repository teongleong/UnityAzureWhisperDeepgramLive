using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using System.Threading.Tasks;
using Proyecto26;
using Newtonsoft.Json;

public class WhisperTranscribeAudio : MonoBehaviour
{
    //private const string apiUrl = "https://api.openai.com/v1/audio/transcriptions";

    public void StartTranscription(byte[] audioData)
    {
        string apiUrl = "https://api.openai.com/v1/audio/transcriptions";
        //string filePath = "path/to/your/audio.wav"; // Replace with the path to your audio file
        string model = "whisper-1";
        string responseFormat = "text";
        StartCoroutine(Transcribe2(apiUrl, audioData, model, responseFormat));
    }

    [Serializable]
    public class WhisperRequestBody
    {
        public string model;
        public string file;
    }

    async void Start() {
        await TrancribeDirectory();
    }

    async Task TrancribeDirectory() {
        string folderPath = @"Assets/Resource/allwav";
        string[] files = Directory.GetFiles(folderPath);

        int count = 0;
        //string currentfilePath = "";
        foreach (var filePath in files)
        {
            
            //string fileName = Path.GetFileName(filePath);
            //print(fileName); // file name

            // skip meta file
            if (filePath.Contains(".meta")) {
                continue;
            }
            print(filePath); // full path
            await WhisperTranscribeAudioFile(filePath, WriteResponseToFile);
            await Task.Delay(2000);
            count++;
            //await FromFile(fileName, filePath);
        }
        print("total transcription: "+count);
    }

    static void WriteResponseToFile(string filename, string content) {
        //string filePath = @"C:\Name\Folder\example.txt";
        //string textToAppend = "This is a sample text to append.";

        using (StreamWriter sw = new StreamWriter("Assets/Resource/response.txt", true))
        {
            sw.WriteLine(filename);
            sw.WriteLine(content);
        }
    }

    async static Task WriteToFile(string filePath, string content) {
        //string filePath = @"C:\Name\Folder\example.txt";
        //string textToAppend = "This is a sample text to append.";

        using (StreamWriter sw = new StreamWriter(filePath, true))
        {
            sw.WriteLine(content);
        }
    }

    static async Task WhisperTranscribeAudioFile(string localFilePath, Action<string, string> callback = null) {
        //var local1 = "Assets/Resource/iqbal_0_4.wav";
        //byte[] audioData = File.ReadAllBytes(localFilePath);
        //WhisperTest3(audioData);
        string auth_string = WhisperAPIPostRequest.CreateAuthString(GlobalSecrets.whisper_apiKey);
        await UploadFile2(auth_string, WhisperAPIPostRequest.whisper_api, localFilePath, callback);
    }

    // duplicated from WhisperAPIPoseRequest
    static async Task UploadFile2(string auth_string, string url, string filepath, Action<string, string> responseCallback=null)
    {
        byte[] audioData = File.ReadAllBytes(filepath);

        RestClient.Request(new RequestHelper
        {
            Uri = url,
            Method = "POST",
            Headers = new Dictionary<string, string> {
                { "Authorization", auth_string },
                //{ "Content-Type", "multipart/form-data" }
            },

            FormSections = new List<IMultipartFormSection>() {
                new MultipartFormFileSection( "file", audioData, filepath, "audio/wav"),
                new MultipartFormDataSection( "model", "whisper-1"),
                new MultipartFormDataSection( "language", "en")
            },
        }).Then(response => {
            //print("done");

            //Debug.Log(response.Text);
            string result = WhisperAPIPostRequest.json_parse(response.Text);
            string filename = Path.GetFileName(filepath);
            responseCallback?.Invoke(filename, result);
            print("Whisper: "+result);
        }).Catch(err => {
            var error = err as RequestException;
            Debug.LogError(error.Response);
            //EditorUtility.DisplayDialog("Error Response", error.Response, "Ok");
        });
    }


    private IEnumerator Transcribe2(string url, byte[] audioData, string model, string responseFormat)
    {
        print("model here " + model);
        // Create a new UnityWebRequest object and set the method to POST
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        //request.SetRequestHeader("Content-Type", "application/json");

        // approach 1

        // Create a List of MultipartFormDataSection objects to store the fields
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        //// Add the model field
        //formData.Add(new MultipartFormDataSection("model", model));

        //// Add the response_format field
        //formData.Add(new MultipartFormDataSection("response_format", responseFormat));

        //// Add the audio data as a MultipartFormFileSection
        //formData.Add(new MultipartFormFileSection("file", audioData, "audio.wav", "audio/wav"));

        //// Set the request headers to include your API key and specify the content type
        //string boundary = GenerateBoundary();
        //byte[] serializedFormData = SerializeFormSections(formData, boundary);

        //UnityWebRequest request = UnityWebRequest.Post(url, formData);

        //request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        //request.SetRequestHeader("Authorization", $"Bearer {ApiKey}");

        //// Set the request method, upload handler, download handler, and content type header
        //request.method = UnityWebRequest.kHttpVerbPOST;
        //request.uploadHandler = (UploadHandler)new UploadHandlerRaw(serializedFormData);
        //request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        //request.SetRequestHeader("Content-Type", "multipart/form-data; boundary=" + boundary);


        //approach 2
        
        //string audioDataString = System.Convert.ToBase64String(audioData);

        //// Create the request body as a JSON object
        //var requestBody = new
        //{
        //    file = audioDataString,
        //    model = model,
        //    response_format = responseFormat,
        //};
        //string requestBodyJson = JsonUtility.ToJson(requestBody);

        //// Set the request body
        //byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBodyJson);

        //UnityWebRequest request = new UnityWebRequest(url, "POST");
        //request.uploadHandler = new UploadHandlerRaw(bodyRaw);

        // approach 3

        //// Create the form data
        //var formData = new Dictionary<string, string>();
        //formData["model"] = model;
        //formData["response_format"] = responseFormat;

        //// Encode the audio data as a Base64 string
        //string audioDataString = System.Convert.ToBase64String(audioData);
        //formData["file"] = audioDataString;

        //// Set the form data as the request body
        //byte[] formDataBytes = UnityWebRequest.SerializeSimpleForm(formData);
        //request.uploadHandler = new UploadHandlerRaw(formDataBytes);


        //Debug.Log("Request URL: " + request.url);
        //Debug.Log("Request Headers: " + request.GetRequestHeader("Content-Type") + ", " + request.GetRequestHeader("Authorization"));
        //Debug.Log("Request Body Length: " + request.uploadHandler.data.Length);

        //// Send the request and wait for a response
        yield return request.SendWebRequest();

        //Debug.Log("Response Status: " + request.responseCode);
        //Debug.Log("Response Text: " + request.downloadHandler.text);

        //if (request.result == UnityWebRequest.Result.Success)
        //{
        //    Debug.Log(request.downloadHandler.text);
        //}
        //else
        //{
        //    Debug.LogError(request.error);
        //}
    }


    private IEnumerator Transcribe(string url, byte[] audioData, string model, string responseFormat)
    {

        var request = new UnityWebRequest(url, "POST");

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        formData.Add(new MultipartFormDataSection("model", model));
        formData.Add(new MultipartFormDataSection("response_format", responseFormat));
        formData.Add(new MultipartFormFileSection("file", audioData, "audio.wav", "audio/wav"));

        //request.SetRequestHeader("Content-Type", "multipart/form-data");
        request.SetRequestHeader("Authorization", $"Bearer {GlobalSecrets.whisper_apiKey}");

        string boundary = GenerateBoundary();
        request.SetRequestHeader("Content-Type", "multipart/form-data; boundary=" + boundary);
        byte[] serializedFormData = SerializeFormSections(formData, boundary);
        request.uploadHandler = new UploadHandlerRaw(serializedFormData);
        request.downloadHandler = new DownloadHandlerBuffer();

        //string requestBody = JsonUtility.ToJson(new
        //{
        //    file = System.Convert.ToBase64String(audioData),
        //    model = model,
        //    response_format = responseFormat,
        //    // Add other optional parameters as needed
        //});

        //byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
        //request.uploadHandler = new UploadHandlerRaw(bodyRaw);


        // Before sending the request
        Debug.Log("Request URL: " + request.url);
        Debug.Log("Request Headers: " + request.GetRequestHeader("Content-Type") + ", " + request.GetRequestHeader("Authorization"));
        Debug.Log("Request Body Length: " + request.uploadHandler.data.Length);

        yield return request.SendWebRequest();

        // After receiving the response
        Debug.Log("Response Status: " + request.responseCode);
        Debug.Log("Response Text: " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError(request.error);
        }
            
    }

    private string GenerateBoundary()
    {
        return "------------------------" + System.DateTime.Now.Ticks.ToString("x");
    }

    private byte[] SerializeFormSections(List<IMultipartFormSection> formData, string boundary)
    {
        byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
        byte[] trailerBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");
        using (var memoryStream = new MemoryStream())
        {
            // Write each form section to the memory stream
            foreach (var section in formData)
            {
                memoryStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                byte[] sectionBytes = section.sectionData;
                memoryStream.Write(sectionBytes, 0, sectionBytes.Length);
            }

            // Write the trailer
            memoryStream.Write(trailerBytes, 0, trailerBytes.Length);

            // Return the serialized form data
            return memoryStream.ToArray();
        }
    }

}