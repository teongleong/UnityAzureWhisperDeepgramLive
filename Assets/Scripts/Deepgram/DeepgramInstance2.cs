using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using TMPro;
using NativeWebSocket;
using System.Threading.Tasks;

public class DeepgramInstance2 : MonoBehaviour
{
    WebSocket websocket;

    [System.Serializable]
    public class DeepgramResponse
    {
        public int[] channel_index;
        public bool is_final;
        public Channel channel;
    }

    [System.Serializable]
    public class Channel
    {
        public Alternative[] alternatives;
    }

    [System.Serializable]
    public class Alternative
    {
        public string transcript;
    }

    [SerializeField]
    bool connected = false;

    [SerializeField]
    TextMeshProUGUI resultText;

    // placeholders

    public int recognitionDelay = 800;
    public string result = "";
    public bool isUserSpeaking = false;

    public void ClearResult() {

    }

    async void Start()
    {
        if (resultText != null) {
            resultText.text = "";
        }

        DeepgramStart();
    }

    async void DeepgramStart() {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", "Token "+GlobalSecrets.deepgram_apiKey }
        };
        websocket = new WebSocket(
            "wss://api.deepgram.com/v1/listen?encoding=linear16&sample_rate=" + 
            AudioSettings.outputSampleRate.ToString(), headers);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connected to Deepgram!");
            connected = true;
        };

        websocket.OnError += (e) =>
        {
            if (e.ToString() == "Unable to connect to the remote server") {
                Debug.LogWarning("Deepgram: check api key in GlobalSecrets");
            }
            Debug.Log("Error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            connected = false;
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            //Debug.Log("OnMessage: " + message);
            
            DeepgramResponse deepgramResponse = new DeepgramResponse();
            object boxedDeepgramResponse = deepgramResponse;
            EditorJsonUtility.FromJsonOverwrite(message, boxedDeepgramResponse);
            deepgramResponse = (DeepgramResponse) boxedDeepgramResponse;
            if (deepgramResponse.is_final)
            {
                var transcript = deepgramResponse.channel.alternatives[0].transcript;
                if (string.IsNullOrWhiteSpace(transcript)) return;
                Debug.Log("Deepgram: "+transcript);
                if (resultText != null) {
                    resultText.text = transcript + "\n" + resultText.text;
                }
            }
        };

        await websocket.Connect();
    }

    bool process_once = false;
    byte [] wav_audioData;
    void Update()
    {
        //print("update");
    #if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            websocket.DispatchMessageQueue();
        }
        // if (ws2 != null && ws2.State == WebSocketState.Open)
        // {
        //     //print("ws2 dispatch");
        //     if (!process_once && wav_audioData != null && wav_audioData.Length == 387152) {
        //         print("process_once");
        //         ProcessAudioSerial(ws2, wav_audioData, 70000);
        //         process_once = true;
        //     }
        //     ws2.DispatchMessageQueue();
        // }
    #endif
        // if (Input.GetKeyDown(KeyCode.D))
        // {
        //     DeepgramTranscribeAudioFile();
        // }
        
        // if (ws2 != null)
        // {
        //     print(ws2.State);
        // }
    }

    private async void OnApplicationQuit()
    {
        //await ws2.Close();
        await websocket.Close();
    }

    public async void ProcessAudio(byte[] audio)
    {
        if (gameObject.activeSelf == false) return;
        if (isActiveAndEnabled == false) return;
        if (websocket.State == WebSocketState.Open)
        {
            //print("sending audio to deepgram");
            await websocket.Send(audio);
        }
    }

    // public async Task OpenChecker(WebSocket websocket, byte[] audio) {
    //     while (websocket.State != WebSocketState.Open) {
    //         print("waiting for websocket to open");
    //         await Task.Delay(2000);
    //     }
    //     await ProcessAudioSerial( websocket, audio, 8192);
    // }

    // process audio in chunks
    // public async Task ProcessAudioSerial(WebSocket websocket, byte[] audio, int byteSize = 8192)
    // {
    //     print("here1");
    //     if (gameObject.activeSelf == false) return;
    //     print("here2");
    //     if (isActiveAndEnabled == false) return;
    //     print("here3 "+websocket.State);
    //     print("here3-2 "+WebSocketState.Open);
    //     if (websocket.State == WebSocketState.Open)
    //     {
    //         print("here4");
    //         for (int i = 0; i < audio.Length; i += byteSize)
    //         {
    //             int chunkSize = Math.Min(byteSize, audio.Length - i);
    //             byte[] chunk = new byte[chunkSize];
    //             Array.Copy(audio, i, chunk, 0, chunkSize);
    //             await Task.Delay(1000);
    //             print("sending audio to deepgram "+chunkSize);
    //             await websocket.Send(chunk);
    //         }
    //     }
    // }

    // Split given array of bytes split into chunks of given size
    // public static IEnumerable<byte[]> Split(byte[] array, int size)
    // {
    //     for (var i = 0; i < array.Length; i += size)
    //     {
    //         yield return array.SubArray(i, Math.Min(size, array.Length - i));
    //     }
    // }

    // public WebSocket ws2;

    // // does not work, just clog up deepgram and stop it from working
    // async void DeepgramTranscribeAudioFile() {

    //     var headers = new Dictionary<string, string>
    //     {
    //         { "Authorization", "Token "+GlobalSecrets.deepgram_apiKey }
    //     };

    //      // make it local
    //     ws2 = new WebSocket(
    //         "wss://api.deepgram.com/v1/listen?"+
    //         "encoding=linear16"+
    //         "&sample_rate=" + AudioSettings.outputSampleRate.ToString() +
    //         "&language=en"
    //         , headers);

    //     ws2.OnOpen += () =>
    //     {
    //         Debug.Log("Connected to Deepgram!");
    //         LoadAudioData();
    //         //connected = true;
    //     };

    //     ws2.OnError += (e) =>
    //     {
    //         Debug.Log("Error: " + e);
    //     };

    //     ws2.OnClose += (e) =>
    //     {
    //         Debug.Log("Connection closed!");
    //         connected = false;
    //     };

    //     ws2.OnMessage += (bytes) =>
    //     {
    //         var message = System.Text.Encoding.UTF8.GetString(bytes);
    //         //Debug.Log("OnMessage: " + message);
            
    //         DeepgramResponse deepgramResponse = new DeepgramResponse();
    //         object boxedDeepgramResponse = deepgramResponse;
    //         EditorJsonUtility.FromJsonOverwrite(message, boxedDeepgramResponse);
    //         deepgramResponse = (DeepgramResponse) boxedDeepgramResponse;
    //         if (deepgramResponse.is_final)
    //         {
    //             var transcript = deepgramResponse.channel.alternatives[0].transcript;
    //             if (string.IsNullOrWhiteSpace(transcript)) return;
    //             Debug.Log("Deepgram: "+transcript);
    //             if (resultText != null) {
    //                 resultText.text = transcript + "\n" + resultText.text;
    //             }
    //         }
    //     };

    //     await ws2.Connect();
    //     LoadAudioData();
    //    // ProcessAudioSerial(ws2, audioData);
    //    //await  OpenChecker( ws2, audioData);
    // }

    // async Task LoadAudioData() {
    //     print("DeepgramTranscribeAudioFile");
    //     var local1 = "Assets/Resource/iqbal_0_4.wav";
    //     wav_audioData = File.ReadAllBytes(local1);
    //     print("audioData.Length: "+wav_audioData.Length);
    // }
}

