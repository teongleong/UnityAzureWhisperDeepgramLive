
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Collections;
using TMPro;
using System.Collections.Generic;
//using Unity.VisualScripting;
using System.Diagnostics.Tracing;
using System;
using System.Threading.Tasks;

public class AzureASR : MonoBehaviour
{
    SpeechRecognizer recognizer;
    SpeechConfig speechConfig;
    AudioConfig audioConfig;
    bool speechStarted = false;
    [SerializeField] string message = "";
    [SerializeField] List<string> resultHistory = new List<string>();
    [SerializeField] List<string> microphoneDeviceName = new List<string>();
    [SerializeField] int DelayMS = 800;

    public UnityAction<string> transcribedEvent;

    [SerializeField]
    Queue<string> resultQueue = new Queue<string>();
    [SerializeField]
    TextMeshProUGUI resultText;



    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        message = e.Result.Text;
        resultHistory.Add(message);
        //Debug.Log("Recognized message: " + message);
        //Debug.Log("Cancellation reason: " + e.Result.Reason);

        // put result into a queue
        resultQueue.Enqueue(message);
    }

    private async void ToggleRecognitionAsync()
    {
        if (speechStarted)
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            speechStarted = false;
            //Debug.Log("STT message: " + message);
        }
        else
        {
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            speechStarted = true;
        }
    }

    void PopulateMicrophoneDeviceName() {
        foreach (var device in Microphone.devices)
        {
            microphoneDeviceName.Add(device.ToString());
        }
    }

    async void Start()
    {
        print("AzureASR Start");
        if (resultText != null) {
            resultText.text = "";
        }

        bool keyCheck = false;
        //startRecordButton.onClick.AddListener(ButtonClick);
        try
        {
            speechConfig = SpeechConfig.FromSubscription(
                GlobalSecrets.azure_key, 
                "southeastasia");
            keyCheck = true;
        }
        catch (System.Exception)
        {
            Debug.LogWarning("Azure: GlobalSecrets.key needs a valid key");
            //throw;
        }
        print("keycheck "+keyCheck);
        if (!keyCheck) return;

        speechConfig.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, DelayMS.ToString());
        PopulateMicrophoneDeviceName();

        // get input from default microphone device (first one)
        audioConfig = AudioConfig.FromDefaultMicrophoneInput();

        transcribedEvent += PrintResult;

        recognizer = new SpeechRecognizer(speechConfig, audioConfig);
        recognizer.Recognized += RecognizedHandler;
        ToggleRecognitionAsync();

        //yield return null;
    }

    private void Update()
    {
       ConsumeResultQueue();
    }

    void ConsumeResultQueue() {
        if (resultQueue.Count > 0) {
            string currentMessage = resultQueue.Dequeue();
            if (transcribedEvent != null) {
                transcribedEvent.Invoke(currentMessage);
            }
        }
    }

    void PrintResult(string result) {
        if (string.IsNullOrWhiteSpace(result)) return;
        Debug.Log("Azure: " + result);
        if (resultText != null) {
            resultText.text = result + "\n" + resultText.text;
        }
    }

    private void OnDestroy()
    {
        if (recognizer != null)
        {
            recognizer.Recognized -= RecognizedHandler;
            recognizer.StopContinuousRecognitionAsync().Wait();
            recognizer.Dispose();
        }
    }
    // IEnumerator LoadWavFile(string url)
    // {
    //     using (var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
    //     {
    //         yield return www.SendWebRequest();

    //         if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
    //         {
    //             Debug.LogError(www.error);
    //         }
    //         else
    //         {
    //             AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
    //             ProcessAudioClip(audioClip);
    //         }
    //     }
    // }

    // private async Task<AudioClip> LoadWavFileAsync(string url)
    // {
    //    using (var www = new UnityWebRequest(url))
    //    {
    //        www.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);
    //        await www.SendWebRequest();

    //        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
    //        {
    //            Debug.LogError(www.error);
    //            return null;
    //        }

    //        return DownloadHandlerAudioClip.GetContent(www);
    //    }
    // }

//     private async void ProcessAudioClip(AudioClip audioClip)
//     {
//         using (var audioInputStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM((uint)audioClip.frequency, (byte)audioClip.channels)))
//         {
//             byte[] audioData = audioClip.GetData();
//             await audioInputStream.WriteAsync(audioData);

//             using (var recognizer = CreateSpeechRecognizer())
//             {
//                 var result = await recognizer.RecognizeOnceAsync();
//                 Debug.Log("Recognized text: " + result.Text);
//             }
//         }
//     }

//        private SpeechRecognizer CreateSpeechRecognizer(string audioFilePath)
//    {
//        var config = SpeechConfig.FromSubscription(API_KEY, REGION);
//        config.EndpointId = ENDPOINT_ID;
//        var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);
//        return new SpeechRecognizer(config, audioConfig);
//    }

//        private async Task StartRecognitionAsync(SpeechRecognizer recognizer)
//    {
//        recognizer.Recognizing += (s, e) =>
//        {
//            Debug.Log($"RECOGNIZING: Text={e.Result.Text}");
//        };

//        recognizer.Recognized += (s, e) =>
//        {
//            if (e.Result.Reason == ResultReason.RecognizedSpeech)
//            {
//                Debug.Log($"RECOGNIZED: Text={e.Result.Text}");
//            }
//            else if (e.Result.Reason == ResultReason.NoMatch)
//            {
//                Debug.Log("NOMATCH: Speech could not be recognized.");
//            }
//        };

//        recognizer.Canceled += (s, e) =>
//        {
//            Debug.Log($"CANCELED: Reason={e.Reason}");

//            if (e.Reason == CancellationReason.Error)
//            {
//                Debug.Log($"CANCELED: ErrorCode={e.ErrorCode}");
//                Debug.Log($"CANCELED: ErrorDetails={e.ErrorDetails}");
//                Debug.Log("CANCELED: Did you set the speech key and region correctly?");
//            }
//        };

//        recognizer.SessionStopped += (s, e) =>
//        {
//            Debug.Log("\n    Session stopped event.");
//        };

//        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
//    }

//       public async Task StartRecognitionWithWavFileAsync(string wavFilePath)
//    {
//        var recognizer = CreateSpeechRecognizer(wavFilePath);
//        await StartRecognitionAsync(recognizer);
//    }



}