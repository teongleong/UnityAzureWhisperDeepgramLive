using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using WebRtcVadSharp;

[RequireComponent(typeof(AudioSource))]
public class WhisperMicrophoneInstance : MonoBehaviour
{
    AudioSource _audioSource;
    int lastPosition, currentPosition;

    public bool userSpeaking = false;
    byte[] audioBuffer = new byte[0];
    List<byte> audioBuffer2 = new List<byte>();

    //public WhisperTranscribeAudio ta;
    public WhisperAPIPostRequest api_post_request;
    public DeepgramInstance2 deepgramInstance2;

    public GameObject sphere;

    WebRtcVad vad;
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (Microphone.devices.Length > 0)
        {
            //print(Microphone.devices.Length);
            _audioSource.clip = Microphone.Start(null, true, 10, AudioSettings.outputSampleRate);
            _audioSource.mute = true;
            _audioSource.volume = 0;
        }
        else
        {
            Debug.Log("This will crash! "+Microphone.devices.Length);
        }

        userSpeaking = false;

        //_audioSource.Play();
        vad = new WebRtcVad();

        // set aggressiveness to 0, 1, 2, or 3
        vad.OperatingMode = OperatingMode.HighQuality;

        //vad.SampleRate = SampleRate.Is48kHz;
    }

    bool DoesFrameContainSpeech(byte[] audioFrame)
    {
        return vad.HasSpeech(audioFrame, SampleRate.Is48kHz, FrameLength.Is20ms);
    }

    byte [] SampleMicrophoneShortByte(AudioSource _audioSource)
    {
        float[] samples = new float[(currentPosition - lastPosition) * _audioSource.clip.channels];
        _audioSource.clip.GetData(samples, lastPosition);
        //VolumeMeasurement(samples, Time.time);

        short[] samplesAsShorts = new short[(currentPosition - lastPosition) * _audioSource.clip.channels];
        for (int i = 0; i < samples.Length; i++)
        {
            samplesAsShorts[i] = f32_to_i16(samples[i]);
        }

        var samplesAsBytes = new byte[samplesAsShorts.Length * 2];
        System.Buffer.BlockCopy(samplesAsShorts, 0, samplesAsBytes, 0, samplesAsBytes.Length);

        return samplesAsBytes;
    }

    [SerializeField]
    float lastSpokenTimestamp = 0;

    [SerializeField]
    float currentTime = 0;

    float silenceThreshold = 1.0f;

    int bufferLengthThreshold = 88020; // half a 
    void Update()
    {
        if ((currentPosition = Microphone.GetPosition(null)) > 0)
        {
            if (lastPosition > currentPosition)
            {
                lastPosition = 0;
            }

            if (currentPosition - lastPosition > 0)
            {
                byte [] samplesAsBytes = SampleMicrophoneShortByte(_audioSource);

                byte [] copySamplesAsBytes = new byte[samplesAsBytes.Length];
                Array.Copy(samplesAsBytes, copySamplesAsBytes, samplesAsBytes.Length);

               
               // deepgram operates by just taking all samplesAsBytes as they come
               if (deepgramInstance2 != null) {
                    deepgramInstance2.ProcessAudio(copySamplesAsBytes);
                }

               // acumulate audioBuffer
                

                // if frame contains speech set sphere to active
                // only do it if sphere is not null
                userSpeaking = DoesFrameContainSpeech(samplesAsBytes);
                if (sphere != null )
                {
                    // if userSpeaking is true, set sphere to active
                    // if userSpeaking is false, set sphere to inactive
                    sphere.SetActive(userSpeaking);
                }
                
                currentTime = Time.time;
                if (userSpeaking) {
                    lastSpokenTimestamp = Time.time; 
                    AccumulateAudioBuffer(samplesAsBytes);
                }

                // transcribe audio after 1.5 seconds of silence from last spoken timestamp
                if (Time.time > lastSpokenTimestamp + silenceThreshold && audioBuffer.Length > bufferLengthThreshold) {
                    TranscribeAudio();
                }

                // print("sampleasbytes length: " + samplesAsBytes.Length);
                // //int threshold = 1920 * 360;
                // int onesec = 44100 * 2;
                // int threesec = onesec * 3;

                // int ms100 = 4410 * 2;

                //print("audioBuffer2.Count: " + audioBuffer2.Count);
                //int thresholdCount = ms100;
                // if audioBuffer2 count exceeds thresholdCount clear audiobuffer2
                // if (audioBuffer2.Count > thresholdCount) {
                //     audioBuffer2.Clear();
                //     print("cleared");
                // }

                //DoesFrameContainSpeech(samplesAsBytes);

                lastPosition = currentPosition;
            }
        }
    }

    void TranscribeAudio() {
        if (api_post_request != null && api_post_request.isActiveAndEnabled)
            {
                //StartCoroutine(TranscribeAudioCoroutine(audioBuffer));
                //TranscribeAudioCoroutine(audioBuffer);
                api_post_request.WhisperTest3(audioBuffer);
                audioBuffer = new byte[0]; // Clear the buffer after sending the data
            }
    }

    void AccumulateAudioBuffer(byte [] samplesAsBytes) {
        int oldLength = audioBuffer.Length;
        Array.Resize(ref audioBuffer, oldLength + samplesAsBytes.Length);
        Array.Copy(samplesAsBytes, 0, audioBuffer, oldLength, samplesAsBytes.Length);
        // foreach (byte b in samplesAsBytes) {
        //     audioBuffer2.Add(b);
        // }
    }

    // void TranscribeAudioCoroutine(byte[] audioData)
    // {
    //     //yield return new WaitForSeconds(1f); // Add a 1-second delay between each request
    //     //ta.StartTranscription(audioData);
    //     api_post_request.WhisperTest3(audioData);
    // }

    short f32_to_i16(float sample)
    {
        sample = sample * 32768;
        if (sample > 32767)
        {
            return 32767;
        }
        if (sample < -32768)
        {
            return -32768;
        }
        return (short)sample;
    }

}

