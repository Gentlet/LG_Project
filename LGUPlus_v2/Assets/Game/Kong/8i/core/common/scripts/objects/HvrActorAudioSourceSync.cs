using UnityEngine;

namespace HVR
{
    [AddComponentMenu("8i/HvrActorAudioSourceSync")]
    public class HvrActorAudioSourceSync : MonoBehaviour, ITimestampProvider
    {
        public HvrActor actor;
        public AudioSource audioSource;

        void Awake()
        {
            audioSource.Stop();
        }

        void OnEnable()
        {
            ConnectHvrActor(actor);
        }

        void OnDisable()
        {
            DisconnectHvrActor();
        }

        void Update()
        {
            if (actor == null || actor.assetInterface == null || audioSource == null || audioSource.clip == null)
                return;

            if (!audioSource.isActiveAndEnabled)
                return;

        }


        ///////////////////////////////////////////////////////////////////////
        // ITimestampProvider
        public bool ProvidesTimestamp()
        {
            if (audioSource != null && audioSource.clip != null)
            {
                return true;
            }

            return false;
        }

        public float GetTimestamp()
        {
            if (audioSource != null)
            {
                return (audioSource.time) * Helper.AUDIO_TO_VIDEO_FIX;
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
            if (audioSource != null && actor != null)
            {
                audioSource.Play();

                actor.assetInterface.Play();
            }
        }

        public void Pause()
        {
            if (audioSource != null && actor != null)
            {
                audioSource.Pause();
                audioSource.time = actor.assetInterface.GetActualTime() * Helper.VIDEO_TO_AUDIO_FIX;

                actor.assetInterface.Pause();
            }
        }

        public void Stop()
        {
            if (audioSource != null && actor != null)
            {
                audioSource.Stop();
                audioSource.time = 0;

                actor.assetInterface.Stop();
            }
        }

        public void Seek(float time)
        {
            if (audioSource != null && actor != null)
            {
                audioSource.time = time * Helper.VIDEO_TO_AUDIO_FIX;

                actor.assetInterface.Seek(time);
            }
        }

        public void SetLooping(bool loop)
        {
            if (audioSource != null && actor != null)
            {
                audioSource.loop = loop;

                actor.assetInterface.SetLooping(loop);
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
            // do nothing
        }

        public bool IsCaching()
        {
            return false;
        }
        

    }


}