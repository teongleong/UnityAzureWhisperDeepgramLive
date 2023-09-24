using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

public class AzureTranscribeFile : MonoBehaviour
{
    

    // Start is called before the first frame update
    async Task Start()
    {
        // SpeechConfig speechConfig = GetSpeechConfig();
        // await FromFile( speechConfig, "Assets/Resource/allwav/A.wav", WriteResponseToFile);    
        TranscribeDirectory();
    }

    static SpeechConfig GetSpeechConfig() {
        SpeechConfig speechConfig;
        try
        {
            speechConfig = SpeechConfig.FromSubscription(
                GlobalSecrets.azure_key, 
                "southeastasia");
        }
        catch (System.Exception)
        {
            Debug.LogWarning("GlobalSecrets.key needs a valid key");
            return null;
            //throw;
        }
        return speechConfig;
    }

    async static Task  TranscribeDirectory() {
        string folderPath = @"Assets/Resource/allwav";
        string[] files = Directory.GetFiles(folderPath);

        SpeechConfig speechConfig = GetSpeechConfig();

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
            await FromFile( GetSpeechConfig(), filePath, WriteResponseToFile);
            await Task.Delay(1000);
            count++;
            //await FromFile(fileName, filePath);
        }
        print("total transcription: "+count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async static void WriteResponseToFile(string filename, string content) {
        //string filePath = @"C:\Name\Folder\example.txt";
        //string textToAppend = "This is a sample text to append.";

        using (StreamWriter sw = new StreamWriter("Assets/Resource/azure_transcription.txt", true))
        {
            await sw.WriteLineAsync(filename);
            await sw.WriteLineAsync(content);
            print("filename: "+filename);
            print("content: "+content);
        }
    }

    // async static Task WriteToFile(string filePath, string content) {
    //     //string filePath = @"C:\Name\Folder\example.txt";
    //     //string textToAppend = "This is a sample text to append.";

    //     using (StreamWriter sw = new StreamWriter(filePath, true))
    //     {
    //         sw.WriteLine(content);
    //     }
    // }

    async static Task FromFile(SpeechConfig speechConfig, string pathToFile, Action<string, string> callback=null)
    {
        using var audioConfig = AudioConfig.FromWavFileInput(pathToFile);
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

        var result = await speechRecognizer.RecognizeOnceAsync();
        //print($"RECOGNIZED: Text={result.Text}");
        string filename = Path.GetFileName(pathToFile);
        callback?.Invoke(filename, result.Text);
    }
}
