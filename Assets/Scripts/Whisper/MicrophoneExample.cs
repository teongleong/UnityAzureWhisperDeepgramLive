using UnityEngine;
using System;
using System.IO;
using System.Text;

public class MicrophoneExample : MonoBehaviour
{
    // The maximum length of the recording in seconds
    public float maxRecordingLength = 10f;

    // The minimum length of the recording in seconds
    public float minRecordingLength = 1f;

    // The sample rate of the recording
    public int sampleRate = 44100;

    // The number of channels in the recording
    public int channels = 1;

    // The microphone device to use (null for default)
    public string microphoneDevice = null;

    // The AudioClip to hold the recorded audio data
    private AudioClip _audioClip;

    // Flag to check if recording is in progress
    private bool _isRecording = false;

    // Start recording
    public void StartRecording()
    {
        if (!_isRecording)
        {
            // Start recording from the microphone
            _audioClip = Microphone.Start(microphoneDevice, false, Mathf.CeilToInt(minRecordingLength), sampleRate);
            _isRecording = true;
        }
    }

    // Stop recording and return the recorded audio data as a byte array
    public byte[] StopRecording()
    {
        if (_isRecording)
        {
            // Stop recording from the microphone
            Microphone.End(microphoneDevice);

            // Get the length of the recording in samples
            int lengthInSamples = Mathf.RoundToInt(_audioClip.length * sampleRate);

            // Get the audio data from the AudioClip
            float[] audioData = new float[lengthInSamples];
            _audioClip.GetData(audioData, 0);

            // Convert the audio data to 16-bit signed integer format
            short[] audioDataShort = new short[lengthInSamples];
            for (int i = 0; i < lengthInSamples; i++)
            {
                audioDataShort[i] = (short)(audioData[i] * 32767f);
            }

            // Convert the audio data to a byte array
            byte[] audioDataBytes = new byte[lengthInSamples * 2];
            System.Buffer.BlockCopy(audioDataShort, 0, audioDataBytes, 0, audioDataBytes.Length);

            // Reset the AudioClip and flag
            _audioClip = null;
            _isRecording = false;

            return audioDataBytes;
        }
        else
        {
            return null;
        }
    }

    public static string AudioClipToBase64String(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null");
            return null;
        }

        byte[] bytes = AudioClipToWav(clip);

        // Convert byte array to base64 string
        return Convert.ToBase64String(bytes);
    }

    public static byte[] AudioClipToWav2(AudioClip audioClip)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        int channels = audioClip.channels;
        int frequency = audioClip.frequency;
        int samples = audioClip.samples;

        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(44 + samples * channels * 2); // File size - 8
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // Sub-chunk size (16 for PCM)
        writer.Write((ushort)1); // Audio format (1 = PCM)
        writer.Write((ushort)channels); // Number of channels
        writer.Write(frequency); // Sample rate
        writer.Write(frequency * channels * 2); // Byte rate
        writer.Write((ushort)(channels * 2)); // Block align
        writer.Write((ushort)16); // Bits per sample
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(samples * channels * 2); // Sub-chunk size (data)

        float[] data = new float[samples * channels];
        audioClip.GetData(data, 0);

        for (int i = 0; i < data.Length; i++)
        {
            writer.Write((short)(data[i] * short.MaxValue));
        }

        byte[] wavData = stream.ToArray();
        writer.Close();
        stream.Close();
        return wavData;
    }

    public static byte [] AudioClipToWav(AudioClip clip )
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null");
            return null;
        }

        // Convert AudioClip samples to float array
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // Convert float array to byte array
        byte[] bytes = new byte[samples.Length * 4];
        Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);

        return bytes;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartRecording();
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            byte[] audioData = StopRecording();
            //TranscribeAudio ta = FindAnyObjectByType<TranscribeAudio>();
            //ta.StartTranscription(audioData);

            string audioDataString = System.Convert.ToBase64String(audioData);
            WhisperAPIPostRequest apr = FindAnyObjectByType<WhisperAPIPostRequest>();

            //apr.WhisperTest(audioDataString);
            //apr.WhisperTest2(audioData);
            apr.WhisperTest3(audioData);


        }
    }
}