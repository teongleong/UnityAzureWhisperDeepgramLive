using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System; // for Bitconverter
using UnityEditor; // for AssetDatabase
using SimpleJSON;
using System.Linq;
using Proyecto26;
using TMPro;
using System.Threading.Tasks;

public class WhisperAPIPostRequest : MonoBehaviour
{
    const string moderation_api = "https://api.openai.com/v1/moderations";
    
    public static string whisper_api = "https://api.openai.com/v1/audio/transcriptions";

    public AudioClip ac;

    [SerializeField]
    TextMeshProUGUI resultText;

    void Start()
    {
        print("Whisper start");
        if (resultText != null) {
            resultText.text = "";
        }

       string auth_string = CreateAuthString(GlobalSecrets.whisper_apiKey);
       
       //UploadFile(auth_string, whisper_api, local1);

        // var local2 = "Assets/Resource/iqbal_0_4_2.wav";

        // var url4 = "https://file.io/0sxUX9NJm5uq";
        // //var local2 = "iqbal_0_4.mp3";
        // var local3_stereo = "Assets/Resource/no-thats-not-gonna-do-it.wav";
        // var local3_mono = "Assets/Resource/no-thats-not-gonna-do-it-mono.wav";
        // var url1 = "https://www.text2speech.org/FW/getfile.php?file=51fa3e63749ac45de27e702989044058%2Fspeech.mp3";
        // var url2 = "https://en-audio.howtopronounce.com/bb615515277bd4d4cd22dddbc90529f7.mp4";
        // var url3 = "https://blend-voices-catalog.s3.us-east-1.amazonaws.com/talents/37cffae7-7132-e111-9057-001b21a73d70/samples/56d38fa7-01aa-ec11-983f-000d3abb8f90.mp3";
        // var url5 = "https://file.io/0sxUX9NJm5uq";

        // print(audioData.Length);

        // //StartCoroutine(PostAudioClip4(auth_string, whisper_api, url4));
        // // StartCoroutine(PostAudioClip6(auth_string, whisper_api, audioData));

        //PostAudioClip6(string auth_string, string url, byte[] audioData);
        // // StartCoroutine(AlternateGetAudioByte(local2));

        
        //var local1 = "Assets/Resource/iqbal_0_4.wav";
    }

    string SaveFile(AudioClip clip, string fileName)
    {
        // Save AudioClip to a WAV file
        string tempFolderPath = Path.Combine(Application.temporaryCachePath, "Audio");
        if (!Directory.Exists(tempFolderPath))
        {
            Directory.CreateDirectory(tempFolderPath);
        }
        string filePath = Path.Combine(tempFolderPath, fileName);
        SavWav.Save(filePath, clip);

        return filePath;
    }
    
    public string SaveByteArrayToTempAudioFile(byte[] audioData, string fileName)
    {
        string tempFolderPath = Path.Combine(Application.temporaryCachePath, "Audio");
        if (!Directory.Exists(tempFolderPath))
        {
            Directory.CreateDirectory(tempFolderPath);
        }
        string filePath = Path.Combine(tempFolderPath, fileName);
        File.WriteAllBytes(filePath, audioData);
        return filePath;
    }

    public void WhisperTest2(byte[] audioData)
    {
        string auth_string = CreateAuthString(GlobalSecrets.whisper_apiKey);
        //byte[] audioData = File.ReadAllBytes(local1);
        //print(audioData.Length);

        // Load the audio clip from the file path or URL
        AudioClip audioClip = CreateAudioClip(audioData, 1, 44100);
        string filePath = SaveFile(audioClip, "tmp.wav");

        //int sampleRate = GetSampleRateWav(filePath);

        //StartCoroutine(PostAudioClip4(auth_string, whisper_api, url4));
        // StartCoroutine(PostAudioClip6(auth_string, whisper_api, audioData));
        UploadFile2(auth_string, whisper_api, filePath);
    }

    public void WhisperTest3(byte[] audioData)
    {
        string auth_string = CreateAuthString(GlobalSecrets.whisper_apiKey);

        //print("audiodata len " + audioData.Length);

        // Load the audio clip from the file path or URL
        AudioClip audioClip = CreateAudioClip(audioData, 1, 44100);
        string filePath = SaveFile(audioClip, "tmp.wav");

        UploadFile2(auth_string, whisper_api, filePath);
    }

    int GetSampleRateWav(string filepath)
    {
        // Open the WAV file
        FileStream fs = new FileStream(filepath, FileMode.Open);

        // Read the sample rate from the header
        fs.Seek(24, SeekOrigin.Begin);
        byte[] buffer = new byte[4];
        fs.Read(buffer, 0, 4);
        int sampleRate = BitConverter.ToInt32(buffer, 0);

        // Close the file stream
        fs.Close();

        // Output the sample rate
        Debug.Log("Sample rate: " + sampleRate);
        return sampleRate;
    }

    void PlayAudioFile2(string filepath )
    {
        AudioClip audioClip = Resources.Load<AudioClip>(filepath);

        //Assign the audio clip to the audio source
        GetComponent<AudioSource>().clip = audioClip;

        //Play the audio clip
        //GetComponent<AudioSource>().Play();

        print("playing 2");
    }

    void PlayAudioFile(string filepath, int channelCount)
    {
        // Get a reference to the AudioSource component on this game object
        var audioSource = GetComponent<AudioSource>();

        int sampleRate = GetSampleRateWav(filepath);

        // Load the audio clip from the file path or URL
        AudioClip audioClip = LoadAudioClip(filepath, sampleRate, channelCount);

        // Set the audio clip on the AudioSource component
        audioSource.clip = audioClip;

        // Play the audio clip
        //audioSource.Play();
    }

    AudioClip myClip;

    IEnumerator GetWebAudioClip(string url)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            //if (www.isNetworkError)
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                myClip = DownloadHandlerAudioClip.GetContent(www);

                AudioSource audioSource = GetComponent<AudioSource>();
                audioSource.clip = myClip;
                //audioSource.Play();
            }
        }
    }

    private AudioClip LoadAudioClip(string filePathOrUrl, int sampleRate, int channelCount)
    {
        AudioClip audioClip = null;

        // Check if the file path or URL is a web URL
        if (filePathOrUrl.StartsWith("http://") || filePathOrUrl.StartsWith("https://"))
        {
            // Load the audio data from the web URL using UnityWebRequest
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePathOrUrl, AudioType.UNKNOWN);
            www.SendWebRequest();
            while (!www.isDone) { }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                // Convert the audio data to an AudioClip
                audioClip = DownloadHandlerAudioClip.GetContent(www);
            }

            print("went into if");
        }
        else
        {
            print("came into else ");
            // Load the audio file data from the local file path
            byte[] audioData = File.ReadAllBytes(filePathOrUrl);


            //// Convert the audio file data to an AudioClip
            //audioClip = ToAudioClip(audioData);

            //float[] floatData = new float[audioData.Length / 4];
            //Buffer.BlockCopy(audioData, 0, floatData, 0, audioData.Length);


            //int channelCount = 1;
            float[] floatData = DecodeAudioData2(audioData, channelCount, false);


            audioClip = AudioClip.Create("MyClip", floatData.Length, channelCount, sampleRate, false);
            audioClip.SetData(floatData, 0);
        }

        return audioClip;
    }

    AudioClip CreateAudioClip(byte[] audioData, int channelCount, int sampleRate)
    {
        float[] floatData = DecodeAudioData2(audioData, channelCount, false);


        AudioClip audioClip = AudioClip.Create("MyClip", floatData.Length, channelCount, sampleRate, false);
        audioClip.SetData(floatData, 0);

        return audioClip;
    }

    //float[] DecodeAudioData(byte[] data, int channelCount, bool IsLittleEndian=false)
    //{
    //    float[] audioData = null;
    //    if (channelCount == 2)
    //    {
    //        // stereo
    //        audioData = new float[(data.Length / 4)];//* 2
    //        for (int i = 0; i < audioData.Length; i++)
    //        {
    //            short sample = BitConverter.ToInt16(data, (i * 4));// / 2
    //            if (IsLittleEndian)
    //            {
    //                sample = (short)((sample >> 8) | (sample << 8));
    //            }

    //            audioData[i] = sample / 32768.0f;
    //        }
    //    }
        
    //    else if (channelCount == 1)
    //    {
    //        //mono
    //        audioData = new float[(data.Length / 4) * 2];
    //        for (int i = 0; i < audioData.Length; i++)
    //        {
    //            short sample = BitConverter.ToInt16(data, (i * 4) / 2);
    //            if (IsLittleEndian)
    //            {
    //                sample = (short)((sample >> 8) | (sample << 8));
    //            }
    //            audioData[i] = sample / 32768.0f;
    //        }
    //    }

    //    return audioData;
    //}

    float[] DecodeAudioData2(byte[] data, int channelCount, bool IsLittleEndian = false)
    {
        float[] audioData = null;

        // channelCount doesn't matter (just need audio clip to know num of channel)
        audioData = new float[(data.Length / 2)];
        for (int i = 0; i < audioData.Length; i++)
        {
            short sample = BitConverter.ToInt16(data, (i * 2));
            if (IsLittleEndian)
            {
                sample = (short)((sample >> 8) | (sample << 8));
            }
            audioData[i] = sample / 32768.0f;
        }

        return audioData;
    }

    //void DecodeAudio2(string audioFilePath, int numChannels, int sampleRate)
    //{
    //    print("size of float " + sizeof(float));
    //    // Load the raw audio data from the file
    //    byte[] audioData = File.ReadAllBytes(audioFilePath);

    //    // Calculate the number of audio samples
    //    int numSamples = audioData.Length / (sizeof(float) * numChannels);

    //    // Create a new float array to hold the audio data
    //    float[] audioSamples = new float[numSamples * numChannels];

    //    // Convert the raw audio data to floats
    //    int sampleIndex = 0;
    //    for (int i = 0; i < audioData.Length; i += sizeof(float))
    //    {
    //        float sample = BitConverter.ToSingle(audioData, i);

    //        // Check if we need to swap endianness
    //        if (BitConverter.IsLittleEndian)
    //        {
    //            byte[] bytes = BitConverter.GetBytes(sample);
    //            Array.Reverse(bytes);
    //            sample = BitConverter.ToSingle(bytes, 0);
    //        }

    //        audioSamples[sampleIndex++] = sample;
    //    }

    //    // Create a new audio clip from the float array
    //    AudioClip audioClip = AudioClip.Create("AudioClip", numSamples, numChannels, sampleRate, false);
    //    audioClip.SetData(audioSamples, 0);

    //    // Play the audio clip
    //    AudioSource audioSource = gameObject.AddComponent<AudioSource>();
    //    audioSource.clip = audioClip;
    //    audioSource.Play();
    //}

    //AudioClip ToAudioClip(byte [] audioData)
    //{
    //    AudioClip audioClip = AudioClip.Create("Audio Clip", audioData.Length / 2, 1, 44100, false);

    //    // Set the audio clip data
    //    audioClip.SetData(DecodeAudioData(audioData), 0);
    //    return audioClip;
    //}

    //// Decode the audio data from the file


    void ModerationTest(string auth_string)
    {
        // Create a dictionary and add key-value pairs
        Dictionary<string, object> moderation_dict = new Dictionary<string, object>
        {
            { "input", "I want to kill them" },
        };

        byte[] jsonToSend = CreateJson(moderation_dict);
        StartCoroutine(PostRequest(moderation_api, auth_string, jsonToSend));
    }

    public void WhisperTest(string audioData)
    {
        string auth_string = CreateAuthString(GlobalSecrets.whisper_apiKey);
        WhisperTest(auth_string, audioData);
    }

    void WhisperTest(string auth_string, string audioData)
    {
        // Create a dictionary and add key-value pairs
        Dictionary<string, object> whisper_dict = new Dictionary<string, object>
        {
            { "file", audioData },
            { "model", "whisper-1" },
        };

        byte[] jsonToSend = CreateJson(whisper_dict);
        
        StartCoroutine(PostRequest(whisper_api, auth_string, jsonToSend));
    }

    public static string CreateAuthString(string apikey)
    {
        if (apikey == "" || apikey == null)
        {
            Debug.LogWarning("Whisper: whisper_apikey in GlobalSecrets is empty");
            return "";
        }
        return $"Bearer {apikey}";
    }

    byte[] CreateJson(Dictionary<string, object> dict_data)
    {
        // Serialize the dictionary to a JSON string using JsonConvert.SerializeObject()
        string jsonData = JsonConvert.SerializeObject(dict_data);

        byte[] jsonByte = new System.Text.UTF8Encoding().GetBytes(jsonData);
        return jsonByte;
    }

    IEnumerator PostRequest(string url, string authString, byte[] jsonToSend)
    {
        // Create a new UnityWebRequest, set the method to POST, and set the Content-Type header
        UnityWebRequest www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Authorization", $"Bearer {GlobalSecrets.whisper_apiKey}");


        www.uploadHandler = new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = new DownloadHandlerBuffer();

        // Send the request and wait for a response
        yield return www.SendWebRequest();

        // After receiving the response
        Debug.Log("Response Status: " + www.responseCode);
        Debug.Log("Response Text: " + www.downloadHandler.text);

        // Handle the response
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
            Debug.LogError(www.result);
        }
        else
        {
            Debug.Log("POST successful! Response:");
            Debug.Log(www.downloadHandler.text);
        }
    }

    public void PostWhisperUsingFormData(AudioClip clip)
    {
        string auth_string = CreateAuthString(GlobalSecrets.whisper_apiKey);
        StartCoroutine(PostAudioClip(auth_string, whisper_api, clip));
    }

    IEnumerator PostAudioClip3(string url, AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null");
            yield break;
        }

        // Convert AudioClip to WAV
        byte[] wavData = MicrophoneExample.AudioClipToWav2(clip);

        // Create a list of IMultipartFormSection and add the audio data and the model
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
    {
        new MultipartFormFileSection("file", wavData, "audio.wav", "audio/wav"),
        new MultipartFormDataSection("model", "whisper-1")
    };

        using (UnityWebRequest www = UnityWebRequest.Post(url, formData))
        {
            www.SetRequestHeader("Authorization", $"Bearer {GlobalSecrets.whisper_apiKey}");

            // Send the request and wait for a response
            yield return www.SendWebRequest();

            // Handle the response
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("POST successful! Response:");
                Debug.Log(www.downloadHandler.text);
            }
        }
    }


    //public async void  PostAudioClip2(AudioClip audioClip)
    //{
    //    if (audioClip == null)
    //    {
    //        Debug.LogError("AudioClip is null");
    //        return;
    //    }

    //    // Convert AudioClip to WAV
    //    byte[] wavData = MicrophoneExample.AudioClipToWav2(audioClip);

    //    // Create a HttpClient
    //    HttpClient httpClient = new HttpClient();
    //   // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

    //    // Create a MultipartFormDataContent
    //    MultipartFormDataContent formData = new MultipartFormDataContent();
    //    formData.Add(new ByteArrayContent(wavData), "file", "audio.wav");
    //    formData.Add(new StringContent("whisper-1"), "model");

    //    // Send the POST request and get the response
    //    HttpResponseMessage response = await httpClient.PostAsync(whisper_api, formData);
    //    string responseBody = await response.Content.ReadAsStringAsync();

    //    // Check the response
    //    if (response.IsSuccessStatusCode)
    //    {
    //        Debug.Log("POST successful! Response:");
    //        Debug.Log(responseBody);
    //    }
    //    else
    //    {
    //        Debug.LogError("POST failed. Response:");
    //        Debug.LogError(responseBody);
    //    }
    //}


    IEnumerator PostAudioClip(string auth_string, string url, AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null");
            yield break;
        }

        // Convert AudioClip to WAV
        byte[] wavData = MicrophoneExample.AudioClipToWav2(clip);

        // Create a MultipartFormDataSection containing the audio data and the model
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        formData.Add(new MultipartFormFileSection("file", wavData, "audio.wav", "audio/wav"));
        formData.Add(new MultipartFormDataSection("model" , "whisper-1"));

        using (UnityWebRequest www = UnityWebRequest.Post(url, formData))
        {
            www.SetRequestHeader("Authorization", auth_string);
            www.SetRequestHeader("Content-Type", "multipart/form-data");

            www.downloadHandler = new DownloadHandlerBuffer();

            // Send the request and wait for a response
            yield return www.SendWebRequest();
            Debug.Log("Response Status: " + www.responseCode);
            Debug.Log("Response Text: " + www.downloadHandler.text);


            // Handle the response
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("POST successful! Response:");
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    IEnumerator AlternateGetAudioByte(string filepath)
    {
        print("file path " + filepath);
        string fullPath = Path.Combine(Application.dataPath, "/" + filepath.Replace("Assets", ""));
        print("full path " + fullPath);
        print("sigh " + Application.dataPath);

        var fullPath2 = Application.dataPath + filepath.Replace("Assets", "");
        print("fullpath2 " + fullPath2);

        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fullPath2, AudioType.WAV);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
            yield break;
        }

        // Extract the raw audio data as a byte array
        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
        float[] audioData = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(audioData, 0);
        byte[] audioBytes = new byte[audioData.Length * 2];
        Buffer.BlockCopy(audioData, 0, audioBytes, 0, audioBytes.Length);

        //return audioBytes;

        yield return PostAudioClip5(audioBytes);
    }


    IEnumerator PostAudioClip4(string auth_string, string url, string filepath)
    {
        // Create a MultipartFormDataSection containing the audio data and the model
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        //byte[] audioData = File.ReadAllBytes(filepath);
        //byte[] audioData2 = AlternateGetAudioByte(filepath);

        formData.Add(new MultipartFormDataSection("file", @"@" + filepath));
        //formData.Add(new MultipartFormDataSection("file", audioData));
        formData.Add(new MultipartFormDataSection("model", "whisper-1"));

        using (UnityWebRequest www = UnityWebRequest.Post(url, formData))
        {
            www.SetRequestHeader("Authorization", auth_string);
            www.SetRequestHeader("Content-Type", "multipart/form-data");

            www.downloadHandler = new DownloadHandlerBuffer();

            // Send the request and wait for a response
            yield return www.SendWebRequest();
            Debug.Log("Response Status: " + www.responseCode);
            Debug.Log("Response Text: " + www.downloadHandler.text);

            // Handle the response
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                print("error here ");
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("POST successful! Response:");
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    IEnumerator PostAudioClip5(byte[] audioData)
    {
        var auth_string = CreateAuthString(GlobalSecrets.whisper_apiKey);
        var url = whisper_api;

        // Create a MultipartFormDataSection containing the audio data and the model
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        //byte[] audioData = File.ReadAllBytes(filepath);
        //byte[] audioData2 = AlternateGetAudioByte(filepath);

        //formData.Add(new MultipartFormDataSection("file", @"@" + filepath));

        print("audio date " + audioData);
        formData.Add(new MultipartFormDataSection("file", audioData));
        formData.Add(new MultipartFormDataSection("model", "whisper-1"));

        using (UnityWebRequest www = UnityWebRequest.Post(url, formData))
        {
            www.SetRequestHeader("Authorization", auth_string);
            www.SetRequestHeader("Content-Type", "multipart/form-data");

            www.downloadHandler = new DownloadHandlerBuffer();

            // Send the request and wait for a response
            yield return www.SendWebRequest();
            Debug.Log("Response Status: " + www.responseCode);
            Debug.Log("Response Text: " + www.downloadHandler.text);

            // Handle the response
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                print("error here ");
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("POST successful! Response:");
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    IEnumerator PostAudioClip6(string auth_string, string url, byte[] audioData)
    {
        // Create a new web request
        UnityWebRequest request = UnityWebRequest.Post(url, "POST");
        request.SetRequestHeader("Authorization", auth_string);
        request.SetRequestHeader("Content-Type", "multipart/form-data");

        // Create a new form data object and add the file to it
        WWWForm formData = new WWWForm();
        //byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        formData.AddBinaryData("file", audioData, "audio.wav", "audio/wav");
        formData.AddField("model", "whisper-1");

        // Set the form data as the body of the request
        byte[] data = formData.data;
        request.uploadHandler = new UploadHandlerRaw(data);
        request.downloadHandler = new DownloadHandlerBuffer();

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("File upload successful!");
        }
    }

    void RCPost(string auth_string, string url, byte[] audioData)
    {
        RestClient.Request(new RequestHelper
        {
            Uri = "https://jsonplaceholder.typicode.com/photos",
            Method = "POST",

        }).Then(response => {
            //Get resources via downloadHandler to get more control!
            Texture texture = ((DownloadHandlerTexture)response.Request.downloadHandler).texture;
            AudioClip audioClip = ((DownloadHandlerAudioClip)response.Request.downloadHandler).audioClip;
            AssetBundle assetBundle = ((DownloadHandlerAssetBundle)response.Request.downloadHandler).assetBundle;

            EditorUtility.DisplayDialog("Status", response.StatusCode.ToString(), "Ok");
        }).Catch(err => {
            var error = err as RequestException;
            EditorUtility.DisplayDialog("Error Response", error.Response, "Ok");
        });
    }

    public UnityWebRequest CreateMultipartFormRequest(string url, string filePath, string fieldName)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        List<IMultipartFormSection> form = new List<IMultipartFormSection>();
        form.Add(new MultipartFormFileSection(fieldName, fileData, filePath, "application/octet-stream"));

        // You can also add other fields to the form data like this:
        // form.Add(new MultipartFormDataSection("field_name", "field_value"));

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        return request;
    }

    public void UploadFile(string auth_string, string url, string filepath)
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
            }, 
        }).Then(response => {
            print("done");
            Debug.Log(response.Text);
        }).Catch(err => {
            var error = err as RequestException;
            EditorUtility.DisplayDialog("Error Response", error.Response, "Ok");
        });
    }

    private AudioClip recordAudio(int durationInSeconds)
    {
        string microphoneDevice = null; // Or specify the microphone device name if needed
        int frequency = 44100; // Sample rate in Hz, you can adjust this value
        AudioClip recordedAudio = Microphone.Start(microphoneDevice, false, durationInSeconds, frequency);
        return recordedAudio;
    }

    private byte[] stopRecordingAndGetData(AudioClip recordedAudio)
    {
        Microphone.End(null); // Stop recording from the microphone
        float[] audioSamples = new float[recordedAudio.samples * recordedAudio.channels];
        recordedAudio.GetData(audioSamples, 0);

        byte[] audioData = new byte[audioSamples.Length * 4];
        Buffer.BlockCopy(audioSamples, 0, audioData, 0, audioData.Length);

        return audioData;
    }

    public void UploadFile2(string auth_string, string url, string filepath, Action<string> responseCallback=null)
    {
        //print("upload 2");
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
            string result = json_parse(response.Text);
            print("Whisper: "+result);
            responseCallback?.Invoke(result);
            if (resultText != null) {
                resultText.text = result + "\n" + resultText.text;
            }
        }).Catch(err => {
            var error = err as RequestException;
            Debug.LogError(error.Response);
            //EditorUtility.DisplayDialog("Error Response", error.Response, "Ok");
        });
    }

    public static string json_parse(string inputString) {
        JSONNode data = JSON.Parse(inputString);
        string result = data["text"];
        result = result.Replace("\"", "");

        return result;
    }

    public void UploadFile3(string auth_string, string url, byte[] audioData)
    {
        //byte[] audioData = File.ReadAllBytes(filepath);
        string filePath = SaveByteArrayToTempAudioFile(audioData, "tmpAudio.wav");
        print("filepath " + filePath);
        RestClient.Request(new RequestHelper
        {
            Uri = url,
            Method = "POST",
            Headers = new Dictionary<string, string> {
                { "Authorization", auth_string },
                //{ "Content-Type", "multipart/form-data" }
            },

            FormSections = new List<IMultipartFormSection>() {
                new MultipartFormFileSection( "file", audioData, filePath, "audio/wav"),
                new MultipartFormDataSection( "model", "whisper-1"),
            },
        }).Then(response => {
            print("done");
            Debug.Log(response.Text);
        }).Catch(err => {
            var error = err as RequestException;
            Debug.LogError(error.Response);
            //EditorUtility.DisplayDialog("Error Response", error.Response, "Ok");
        });
    }

    public IEnumerator SendMultipartFormData(string auth_string, string url, string filePath)
    {
        List<IMultipartFormSection> form = new List<IMultipartFormSection>();

        // Add the file to the form
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        form.Add(new MultipartFormFileSection("file", fileData, filePath, "application/octet-stream"));
        form.Add(new MultipartFormDataSection("model", "whisper-1"));

        // Create and send the UnityWebRequest
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Authorization", auth_string);
        request.SetRequestHeader("Content-Type", "multipart/form-data");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Debug.Log("Request successfully sent. Response: " + request.downloadHandler.text);
        }
    }
}
