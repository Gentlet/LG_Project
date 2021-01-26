// #define VERBOSE_LOGGING
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using HVR;
using HVR.Interface;
using System.Runtime.InteropServices;


// Should handle WAV/MP3 now
public class VODWavAudioHandler : MonoBehaviour, ITimestampProvider
{
    enum PlayState {
        PLAYING = 0x1,
        CACHING = 0x2,
        PAUSED  = 0x4,
        STOPPED = 0x8
    }

    public HvrActor actor;
    public AudioSource audioSource;
    public float audioTrim = 0.0f;

    AudioClip m_audioClip;
    byte[] m_audioClipRawBytes;
    AudioType m_audioType = AudioType.UNKNOWN;

    float hvrTime;
    Mutex dataBufferLock = new Mutex();
    AudioClip audioStreamClip;
    bool hasDecodedFrame;
    int playState = (int)PlayState.STOPPED;
    bool hasDestructed = false;

    bool audioLoop = false;
    bool waitForAssetRewind = false;
    float decodeAudioTimestampDiff = 0;

    void OnEnable()
    {
        playState = (int)PlayState.STOPPED;
        
        waitForAssetRewind = false;

        if (actor != null)
        {
            actor.assetInterface.onAssetRepresentationDataReceived -= OnAssetRepresentationDataReceived;
            actor.assetInterface.onAssetRepresentationDataReceived += OnAssetRepresentationDataReceived;

            ConnectHvrActor(actor);
        }
    }

    void OnDisable()
    {
        DisconnectHvrActor();

        if (actor != null)
        {
            actor.assetInterface.onAssetRepresentationDataReceived -= OnAssetRepresentationDataReceived;
        }
    }
    
    void Start()
    {
        if (actor == null)
        {
            actor = GetComponent<HvrActor>();
        }

        if (actor != null)
        { 
            // Get reprensetation data cache from AssetInterface, if some data has been transferred before Start() of VODWavAudioHandler
            actor.assetInterface.FetchAssetRepresentationDataCache(OnAssetRepresentationDataReceived);
            actor.assetInterface.onAssetRepresentationDataReceived -= OnAssetRepresentationDataReceived;
            actor.assetInterface.onAssetRepresentationDataReceived += OnAssetRepresentationDataReceived;
        }
        else
        {
            Debug.LogError("Cannot find HvrActor.");
        }


        hasDecodedFrame = false;
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        GUI.Label(new Rect(20, 0, 200, 20), "T: " + hvrTime.ToString() );
        GUI.Label(new Rect(20, 20, 200, 20), "T(C): " + actor.assetInterface.GetCurrentTime().ToString() );
        GUI.Label(new Rect(20, 40, 200, 20), "DIFF: " + decodeAudioTimestampDiff );
        if (audioSource != null)
        {
            GUI.Label(new Rect(20, 60, 200, 20), "AS.T: " + audioSource.time.ToString() );

            if (audioSource.clip != null)
            {
                GUI.Label(new Rect(20, 80, 200, 20), "A DUR: " + audioSource.clip.length.ToString() );
            }
        }

        GUI.Label(new Rect(20, 100, 200, 20), "FR: " + actor.assetInterface.GetBufferFillRatio());
        GUI.Label(new Rect(20, 120, 200, 20), "VOX: " + actor.assetInterface.GetVoxelCount());
    }
#endif

    void EnsureAudioSetup()
    {
        if (m_audioClip != null)
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    // still null, add one
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            audioSource.loop = false; // always manually loop
            audioSource.clip = m_audioClip;
        }
    }


    void OnApplicationQuit()
    {
        hasDestructed = true;
    }

    void OnDestroy()
    {
        hasDestructed = true;
    }

    bool HasDecodedFrame()
    {
        return hasDecodedFrame;
    }

    IEnumerator WaitTillRewindAndPlayAsset()
    {
        waitForAssetRewind = true;
        float endTime = actor.assetInterface.GetActualTime();
        while(actor.assetInterface.GetActualTime() >= endTime)
        {
            yield return null;
        }

        waitForAssetRewind = false;

        Play(true);
    }

    IEnumerator WaitTillRewindAssetToZero()
    {
        waitForAssetRewind = true;
        while(actor.assetInterface.GetActualTime() > 0)
        {
            yield return null;
        }

        waitForAssetRewind = false;
    }

    void CorrectDecodingAndAudioTimestampDifference()
    {
        float videoTime = actor.assetInterface.GetCurrentTime() * Helper.VIDEO_TO_AUDIO_FIX;
        float audioTime = audioSource.time;

        float duration = GetIntersectDuration();
        videoTime = Mathf.Min(videoTime, duration);
        audioTime = Mathf.Min(audioTime, duration);

        decodeAudioTimestampDiff = videoTime - audioTime;

        audioSource.time += decodeAudioTimestampDiff;
    }

    bool IsAudioAtEnd()
    {
        float duration = GetIntersectDuration();
        return duration - audioSource.time < 0.033334f;
    }

    float GetIntersectDuration()
    {
        float videoDuration = actor.assetInterface.GetDuration() * Helper.VIDEO_TO_AUDIO_FIX;
        return Mathf.Min(videoDuration, GetDuration());
    }

    void Update()
    {
        dataBufferLock.WaitOne();

        if (m_audioClipRawBytes != null)
        {
            // Can only call StartCoroutine in main thread
            StartCoroutine(LoadAudioFileFromRawBytes(m_audioClipRawBytes));
            m_audioClipRawBytes = null;
        }

        hvrTime = actor.assetInterface.GetActualTime(); 
        hasDecodedFrame = actor.assetInterface.GetVoxelCount() > 0;

        EnsureAudioSetup();

        if (m_audioClip != null)
        {
            if (audioSource.isActiveAndEnabled)
            {
                if (audioSource.isPlaying && IsAudioAtEnd())
                {
                    if (audioLoop)
                    {
                        // this "play - stop - wait and play" idiom 
                        // because streaming Asset won't rewind if not in play mode that's why play - stop
                        // The reason behind "wait" is, if no waiting is done, then the second play will just start with 'wrong' timestamp
                        // due to this global time wrapping idea in engine sequencer.
                        actor.assetInterface.SetLooping(false);
                        actor.assetInterface.Play();
                        actor.assetInterface.Stop();

                        // force rip off play flag
                        playState |= (int)PlayState.STOPPED;
                        playState &= ~(int)PlayState.PLAYING;
                        playState &= ~(int)PlayState.PAUSED;

                        // make sure the Play() is executed                        
                        StartCoroutine(WaitTillRewindAndPlayAsset());
                    }
                    else 
                    {
                        Stop();
                    }
                }
                else if ((playState & (int)PlayState.PLAYING) != 0 &&
                    (playState & (int)PlayState.CACHING) == 0 &&
                    !waitForAssetRewind &&
                    !audioSource.isPlaying &&
                    HasDecodedFrame())
                {
                    // push playing for any reason it hasn't done yet
                    audioSource.Play();

                    CorrectDecodingAndAudioTimestampDifference();
                }
            }

            
        }

        dataBufferLock.ReleaseMutex();
    }

    static readonly byte[] wavMimeType = new byte[] {
        (byte)'a',
        (byte)'u',
        (byte)'d',
        (byte)'i',
        (byte)'o',
        (byte)'/',
        (byte)'w',
        (byte)'a',
        (byte)'v'
    };

    static readonly byte[] xWavMimeType = new byte[] {
        (byte)'a',
        (byte)'u',
        (byte)'d',
        (byte)'i',
        (byte)'o',
        (byte)'/',
        (byte)'x',
        (byte)'-',
        (byte)'w',
        (byte)'a',
        (byte)'v'
    };

    static readonly byte[] mp3MimeType = new byte[] {
        (byte)'a',
        (byte)'u',
        (byte)'d',
        (byte)'i',
        (byte)'o',
        (byte)'/',
        (byte)'m',
        (byte)'p',
        (byte)'e',
        (byte)'g'
    };

    static readonly byte[] wavCodec = new byte[] {
        (byte)'w',
        (byte)'a',
        (byte)'v'
    };

    static readonly byte[] pcmCodec = new byte[] {
        (byte)'p',
        (byte)'c',
        (byte)'m'
    };

    static readonly byte[] mp3Codec = new byte[] {
        (byte)'m',
        (byte)'p',
        (byte)'e',
        (byte)'g'
    };

    void OnAssetRepresentationDataReceived(
            [In] IntPtr mimeType,
            [In] IntPtr codec,
            float startTime,
            [In] IntPtr data,
            uint dataSize,
            IntPtr userData
        )
    {
        if (hasDestructed)
            return;

        byte[] mimeTypeByteArray = ByteArrayStringHelper.GetByteArrayFromRawMemory(mimeType);
        byte[] codecByteArray = ByteArrayStringHelper.GetByteArrayFromRawMemory(codec);

        //Debug.Log("Data received: " + mimeType + " codec: " + codec + " size: " + dataSize);
        dataBufferLock.WaitOne();

        if (// WAV
            (ByteArrayStringHelper.ByteArrayCompare(mimeTypeByteArray, wavMimeType, wavMimeType.Length) || ByteArrayStringHelper.ByteArrayCompare(mimeTypeByteArray, xWavMimeType, xWavMimeType.Length)) && 
            (ByteArrayStringHelper.ByteArrayCompare(codecByteArray, wavCodec, wavCodec.Length) || ByteArrayStringHelper.ByteArrayCompare(codecByteArray, pcmCodec, pcmCodec.Length)))
        {
            m_audioClipRawBytes = new byte[dataSize];
            Marshal.Copy(data, m_audioClipRawBytes, 0, (int)dataSize);

            m_audioType = AudioType.WAV;
        }
        else 
        if (// MP3
            (ByteArrayStringHelper.ByteArrayCompare(mimeTypeByteArray, mp3MimeType, mp3MimeType.Length)) && 
            (ByteArrayStringHelper.ByteArrayCompare(codecByteArray, mp3Codec, mp3Codec.Length))
            )
        {
            m_audioClipRawBytes = new byte[dataSize];
            Marshal.Copy(data, m_audioClipRawBytes, 0, (int)dataSize);

            m_audioType = AudioType.MPEG;
            
        }

        dataBufferLock.ReleaseMutex();
    }

    IEnumerator LoadAudioFileFromRawBytes(byte[] rawBytes)
    {
        string tempFilename = Path.Combine(Application.temporaryCachePath, string.Format(@"{0}.bin", Guid.NewGuid()));
        System.IO.File.WriteAllBytes(tempFilename, rawBytes);

        WWW loader = new WWW("file://" + tempFilename);
        yield return loader;

        if(!System.String.IsNullOrEmpty(loader.error))
            Debug.LogError(loader.error);

        dataBufferLock.WaitOne();

        m_audioClip = loader.GetAudioClip(false, false, m_audioType);

        dataBufferLock.ReleaseMutex();

        // delete the temp file
        System.IO.File.Delete(tempFilename);
    }
    
    ///////////////////////////////////////////////////////////////////////
    // ITimestampProvider
    public bool ProvidesTimestamp()
    {
        // We don't know if the stream will come or not
        if (!HasDecodedFrame())
            return true;

        // Audio properly setup
        if (audioSource != null && audioSource.clip != null)
        {
            return true;
        }


        // The stream is not going to work
        return false;
    }

    public float GetTimestamp()
    {
        if (audioSource != null)
        {
            float t = (audioSource.time) * Helper.AUDIO_TO_VIDEO_FIX;
            t = Mathf.Max(0.0f, Mathf.Min(t, actor.assetInterface.GetDuration() - 0.03333334f)); // do not return bigger timestamp than the video duration, otherwise the streaming could have requested wrong data from server and stalls playback
            return t;
        }

        return 0;
    }

    public void ConnectHvrActor(HvrActor hvrActor)
    {
        if (hvrActor != null)
        {
            hvrActor.timestampProvider = this;
        }
    }

    public void DisconnectHvrActor()
    {
        if (actor != null && actor.timestampProvider == this)
        {
            actor.timestampProvider = null;
        }
    }

    public void Play()
    {
        Play(true);
    }

    public void Play(bool adjustTimestampDifference)
    {
        if (audioSource != null && !waitForAssetRewind)
        {
            audioSource.Play();

            if (adjustTimestampDifference)
                CorrectDecodingAndAudioTimestampDifference();

            // NOTE: disable looping in asset interface because timestamp won't be reliable after looping
            actor.assetInterface.SetLooping(false);

            // drives the asset interface
            actor.assetInterface.Play();

            playState |= (int)PlayState.PLAYING;
            playState &= ~(int)PlayState.PAUSED;
            playState &= ~(int)PlayState.STOPPED;
        }
    }

    public void Pause()
    {
        if (audioSource != null && (playState & (int)PlayState.PAUSED) == 0)
        {
            audioSource.Pause();

            // drives the asset interface
            actor.assetInterface.Pause();

            playState |= (int)PlayState.PAUSED;
            playState &= ~(int)PlayState.STOPPED;
            playState &= ~(int)PlayState.PLAYING;
        }
    }

    public void Stop()
    {
        Stop(true);
    }

    public void Stop(bool waitTillRewindAssetToZero)
    {
        if (audioSource != null && (playState & (int)PlayState.STOPPED) == 0 && !waitForAssetRewind)
        {
            audioSource.Stop();

            audioSource.time = 0;

            // drives the asset interface
            actor.assetInterface.Stop();

            if (waitTillRewindAssetToZero)
                StartCoroutine(WaitTillRewindAssetToZero());

            playState |= (int)PlayState.STOPPED;
            playState &= ~(int)PlayState.PLAYING;
            playState &= ~(int)PlayState.PAUSED;
        }
    }

    public void Seek(float time)
    {
        if (audioSource != null  
            && (playState & (int)PlayState.PLAYING) != 0) // FIXME: seek at all the time
        {
            float timeAtSeek = time * Helper.VIDEO_TO_AUDIO_FIX;
            audioSource.time = timeAtSeek;

            // drives the asset interface
            actor.assetInterface.Seek(time);
        }
    }

    public void SetLooping(bool loop)
    {
        if (audioSource != null)
        {
            // audioSource.loop will always be false
            audioSource.loop = false;

            // set another flag to indicate the real intention of users
            audioLoop = loop;

            // NOTE: we don't set Asset looping here as the timestamp won't be reliable after looping
        }
    }

    public bool IsLooping()
    {
        if (audioSource != null)
        {
            return audioSource.loop;
        }

        return false;
    }

    public float GetDuration()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            return audioSource.clip.length;
        }

        return 0;
    }

    public void MarkAsCaching(bool caching)
    {
        if (caching)
        {
            playState |= (int)PlayState.CACHING;
            audioSource.Pause();

            actor.assetInterface.MarkAsCaching(true);
        }
        else
        {
            actor.assetInterface.MarkAsCaching(false);
            playState &= ~(int)PlayState.CACHING;

            if (((playState & (int)PlayState.PLAYING) != 0) &&
                !audioSource.isPlaying &&
                HasDecodedFrame()
                ) 
            {
                audioSource.Play();

                CorrectDecodingAndAudioTimestampDifference();
            }
        }
    }

    public bool IsCaching()
    {
        return (playState & (int)PlayState.CACHING) != 0;
    }
}