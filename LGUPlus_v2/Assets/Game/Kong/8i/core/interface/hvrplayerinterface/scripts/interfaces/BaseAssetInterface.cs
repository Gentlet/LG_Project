using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif


namespace HVR.Interface
{
	public class BaseAssetInterface : System.Object, ITimestampProvider
    {
        public int handle 
        { 
        	get { return m_handle; } 
        	protected set 
        	{ 
        		m_handle = value;
        	}
        }

        public Types.AssetCreationInfo assetCreationInfo { get { return m_assetCreationInfo; } }

        public enum AssetSource
        {
            Local,
            RealTime,
            VOD
        }

        public AssetSource assetSource { get { return m_assetType; } }

        private int m_handle = Types.INVALID_HANDLE;
        protected int m_lastError = ErrorCodes.HVR_ERROR_SUCCESS;

        private Types.AssetCreationInfo m_assetCreationInfo;

        private AssetSource m_assetType;

        private CommonTypes.Bounds lastFrameBounds;
        private int lastFrameVoxelCount = 0;

        private bool m_isLooping;
        private bool m_userPlaying = false;
        private bool m_markedAsCaching = false;
        private GCHandle m_gcHandle;

        public delegate void OnAssetInitialisedHandler(
            int error,
            IntPtr userData
        );

        public delegate bool OnAssetSelectRepresentationHandler(
            CommonTypes.HVRAdaptationSet adaptionSet,
            uint representationIndex,
            CommonTypes.HVRRepresentation[] representations,
            uint representationCount,
            IntPtr userData
        );


        public delegate void OnAssetRepresentationDataReceivedHandler(
            IntPtr mimeType,
            IntPtr codec,
            float startTime,
            IntPtr data,
            uint dataSize,
            IntPtr userData
        );

        public event OnAssetInitialisedHandler onAssetInitialized;
        public event OnAssetRepresentationDataReceivedHandler onAssetRepresentationDataReceived;

        public delegate void OnPlayEvent();
        public OnPlayEvent onPlay;
        public delegate void OnSeekEvent(float time);
        public OnSeekEvent onSeek;
        public delegate void OnPauseEvent();
        public OnPauseEvent onPause;
        public delegate void OnStopEvent();
        public OnStopEvent onStop;

#if UNITY_2017_1_OR_NEWER && UNITY_EDITOR
        static BaseAssetInterface()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        static void OnBeforeAssemblyReload()
        {
            UnityInterfaceAPI.Callback_Set_OnAssetInitialised(null);
            UnityInterfaceAPI.Callback_Set_OnAssetSelectRepresentation(null);
            UnityInterfaceAPI.Callback_Set_OnAssetRepresentationDataReceived(null);
        }

        static void OnAfterAssemblyReload()
        {
            UnityInterfaceAPI.Callback_Set_OnAssetInitialised(PInvoke_OnAssetInitialised);
            UnityInterfaceAPI.Callback_Set_OnAssetSelectRepresentation(PInvoke_OnAssetSelectRepresentation);
            UnityInterfaceAPI.Callback_Set_OnAssetRepresentationDataReceived(PInvoke_OnAssetRepresentationDataReceived);
        }

        #if UNITY_EDITOR    
        [DidReloadScripts(1000)]
        private static void OnScriptsReloaded() {
            // Reset UnityInterface callbacks on assets
            UnityInterfaceAPI.Callback_Set_OnAssetInitialised(PInvoke_OnAssetInitialised);
            UnityInterfaceAPI.Callback_Set_OnAssetSelectRepresentation(PInvoke_OnAssetSelectRepresentation);
            UnityInterfaceAPI.Callback_Set_OnAssetRepresentationDataReceived(PInvoke_OnAssetRepresentationDataReceived);
        }
        #endif
#else
        static BaseAssetInterface()
        {
            UnityInterfaceAPI.Callback_Set_OnAssetInitialised(PInvoke_OnAssetInitialised);
            UnityInterfaceAPI.Callback_Set_OnAssetSelectRepresentation(PInvoke_OnAssetSelectRepresentation);
            UnityInterfaceAPI.Callback_Set_OnAssetRepresentationDataReceived(PInvoke_OnAssetRepresentationDataReceived);
        }        
#endif
        [MonoPInvokeCallback(typeof(Types.OnAssetInitialised))]
        private static void PInvoke_OnAssetInitialised(
                int error,
                IntPtr userData
            )
        {
            if (userData != IntPtr.Zero)
            {
                var asset = Helper.GCHandleToObject<BaseAssetInterface>((GCHandle)userData);

                if (asset != null)
                    asset.OnAssetInitialised(error, userData);
            }
        }

        [MonoPInvokeCallback(typeof(Types.OnAssetSelectRepresentation))]
        private static bool PInvoke_OnAssetSelectRepresentation(
                [In] IntPtr adaptionSet,
                uint representationIndex,
                [In] IntPtr representations,
                uint representationCount,
                IntPtr userData
            )
        {
            CommonTypes.HVRAdaptationSet _adaptionSet = Helper.PtrToStruct<CommonTypes.HVRAdaptationSet>(adaptionSet);

            CommonTypes.HVRRepresentation[] _representations = new CommonTypes.HVRRepresentation[(int)representationCount];

            int representationStructSize = Marshal.SizeOf(typeof(CommonTypes.HVRRepresentation));

            for (int i = 0; i < _representations.Length; ++i)
            {
                IntPtr dataPtr = new IntPtr(representations.ToInt64() + representationStructSize * i);
                _representations[i] = Helper.PtrToStruct<CommonTypes.HVRRepresentation>(dataPtr);
            }

            if (userData != IntPtr.Zero)
            {
                var asset = Helper.GCHandleToObject<BaseAssetInterface>((GCHandle)userData);

                if (asset != null)
                    return asset.OnAssetSelectRepresentation(_adaptionSet, representationIndex, _representations, representationCount, userData);
            }

            return true;
        }

        [MonoPInvokeCallback(typeof(Types.OnAssetRepresentationDataReceived))]
        private static void PInvoke_OnAssetRepresentationDataReceived(
                [In] IntPtr mimeType,
                [In] IntPtr codec,
                float startTime,
                [In] IntPtr data,
                uint dataSize,
                IntPtr userData
            )
        {
            if (userData != IntPtr.Zero)
            {
                var asset = Helper.GCHandleToObject<BaseAssetInterface>((GCHandle)userData);

                if (asset != null)
                    asset.OnAssetRepresentationDataReceived(mimeType, codec, startTime, data, dataSize, userData);
            }
        }

        public void Create(string fileFolder)
        {
            m_handle = Types.INVALID_HANDLE;

            m_assetCreationInfo = new Types.AssetCreationInfo();
            m_assetCreationInfo.assetPath = fileFolder;
            m_assetCreationInfo.cacheDir = null; // TODO: assetCreationInfo.cacheDir will be deprecated.
            m_assetCreationInfo.bufferTime = 2.0f;
            m_assetCreationInfo.disableCaching = true;

            Create(m_assetCreationInfo);
        }

        public void Create(Types.AssetCreationInfo info)
        {
            m_handle = Types.INVALID_HANDLE;

            if (!HvrHelper.Support.IsApplicationStateSupported())
                return;
#if UNITY_EDITOR
            if (BuildPipeline.isBuildingPlayer)
                return;
#endif

            try
            {
                UnityInterface.Lock();

                if (HvrPlayerInterface.Initialise())
                {
                    m_assetCreationInfo = info;

                    m_assetCreationInfo.structSize = (uint)Marshal.SizeOf(typeof(Types.AssetCreationInfo));

                    m_gcHandle = GCHandle.Alloc(this);
                    Helper.RegisterGCHandle(m_gcHandle);
                    IntPtr assetPtr = (IntPtr)m_gcHandle;
                    m_assetCreationInfo.userData = assetPtr;

                    // https://answers.unity.com/questions/1229036/callbacks-from-c-to-c-are-not-working-in-5   0f3.html?sort=votes
                    // For both IL2CPP and Mono on an AOT platform (like iOS) it is not possible to marshal an instance method to a function pointer
                    // that can be called from native code. Only static methods in C# code be called from native code like this.

                    m_assetCreationInfo.onInitialized = UnityInterfaceAPI.StaticHandler_OnAssetInitialised;
                    m_assetCreationInfo.onSelectRepresentation = UnityInterfaceAPI.StaticHandler_OnAssetSelectRepresentation;
                    m_assetCreationInfo.onRepresentationDataRecieved = UnityInterfaceAPI.StaticHandler_OnAssetRepresentationDataReceived;

                    m_assetCreationInfo.disableCaching = true;

                    m_handle = HvrPlayerInterfaceAPI.Asset_CreateFromInfo(ref m_assetCreationInfo);
                    if (m_lastError != ErrorCodes.HVR_ERROR_SUCCESS)
                    {
                        // creation failed, early quit
                        m_handle = Types.INVALID_HANDLE;
                    }
                    else {
                        if (m_assetCreationInfo.assetPath.StartsWith("tcp"))
                            m_assetType = AssetSource.RealTime;
                        else
                        if (m_assetCreationInfo.assetPath.EndsWith("8imanifest"))
                            m_assetType = AssetSource.VOD;
                        else
                            m_assetType = AssetSource.Local;

    #if VERBOSE_LOGGING
                        Debug.Log("Create " + GetType().Name + " Handle:" + handle);
    #endif

                        UnityInterface.SceneObjectsAdd(handle, GetType().Name + handle, GetType().FullName);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                UnityInterface.Unlock();
            }
        }


        public void Delete()
        {
            if (handle == Types.INVALID_HANDLE)
                return;
#if UNITY_EDITOR
            if (BuildPipeline.isBuildingPlayer)
                return;
#endif
            try
            {
                UnityInterface.Lock();

#if VERBOSE_LOGGING
                Debug.Log("Delete " + GetType().Name + " Handle:" + handle);
#endif
                onAssetInitialized = null;
                onAssetRepresentationDataReceived = null;

                HvrPlayerInterfaceAPI.Asset_Delete(handle);
                HvrPlayerInterfaceAPI.Interface_Update();

                UnityInterface.SceneObjectsRemove(handle);

                m_handle = Types.INVALID_HANDLE;

                Helper.DeregisterGCHandle(m_gcHandle);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                UnityInterface.Unlock();
            }
        }

        public bool IsValid()
        {
            if (handle == Types.INVALID_HANDLE)
                return false;

            return HvrPlayerInterfaceAPI.Asset_IsValid(handle);
        }

        public void Update(float absoluteTime)
        {
            if (handle == Types.INVALID_HANDLE)
                return;

            HvrPlayerInterfaceAPI.Asset_Update(handle, absoluteTime);
        }

        public void LogMeta()
        {
            if (handle == Types.INVALID_HANDLE)
                return;

            //// TODO: There was an API change
            //int count = HvrPlayerInterfaceAPI.Asset_GetMetaCount(handle);

            //for (int i = 0; i < count; ++i)
            //{
            //    StringBuilder key = new StringBuilder(256);
            //    StringBuilder val = new StringBuilder(256);
            //    string log = i + " - ";
            //    if (HvrPlayerInterfaceAPI.Asset_GetMetaEntry(handle, i, key, val))
            //    {
            //        log += key.ToString() + " = " + val.ToString();
            //    }
            //    Debug.Log(log + "\n");
            //}
        }

        public void Play()
        {
            if (handle == Types.INVALID_HANDLE)
                return;

            HvrPlayerInterfaceAPI.Asset_Play(handle);
            m_userPlaying = true;

            if (onPlay != null)
                onPlay();
        }

        public void Pause()
        {
            if (handle == Types.INVALID_HANDLE)
                return;

            HvrPlayerInterfaceAPI.Asset_Pause(handle);
            m_userPlaying = false;

            if (onPause != null)
                onPause();
        }

        public void MarkAsCaching(bool caching)
        {
            if (handle == Types.INVALID_HANDLE)
                return;

            m_markedAsCaching = caching;

            if (m_userPlaying)
            {
                if (m_markedAsCaching)
                {
                    HvrPlayerInterfaceAPI.Asset_Pause(handle);
                }
                else 
                {
                    HvrPlayerInterfaceAPI.Asset_Play(handle);
                }
            }
        }

        public bool IsCaching()
        {
            if (handle == Types.INVALID_HANDLE)
                return false;

            return m_markedAsCaching;
        }

        public void Seek(float time)
        {
            if (handle == Types.INVALID_HANDLE)
                return;

            HvrPlayerInterfaceAPI.Asset_Seek(handle, time);
        }

        public void Stop()
        {
            if (handle == Types.INVALID_HANDLE)
                return;

            HvrPlayerInterfaceAPI.Asset_Pause(handle);
            HvrPlayerInterfaceAPI.Asset_Seek(handle, 0);
            m_userPlaying = false;

            if (onStop != null)
                onStop();
        }

        public void Step(int frames)
        {
            if (handle == Types.INVALID_HANDLE)
                return;

            HvrPlayerInterfaceAPI.Asset_Step(handle, frames);

            if (onStop != null)
                onStop();
        }

        public bool IsLooping()
        {
            if (handle == Types.INVALID_HANDLE)
                return false;

            return m_isLooping;
        }

        public void SetLooping(bool looping)
        {
            if (handle == Types.INVALID_HANDLE)
                return;

            m_isLooping = looping;

            HvrPlayerInterfaceAPI.Asset_SetLooping(handle, looping);
        }

        public Bounds GetBounds()
        {
            if (handle == Types.INVALID_HANDLE)
            {
                return new Bounds();
            }

            var b = HvrPlayerInterfaceAPI.Asset_GetBounds(handle);
            b.center.x *= -1.0f;

            var center = new Vector3(b.center.x, b.center.y, b.center.z);
            var size = new Vector3(b.halfDims.x, b.halfDims.y, b.halfDims.z);
            size *= 2.0f;

            center = center * Helper.CENTIMETRES_TO_METRES;
            size = size * Helper.CENTIMETRES_TO_METRES;

            return new Bounds(center, size);
        }

        public int GetState()
        {
            if (handle == Types.INVALID_HANDLE)
                return 0;

            return HvrPlayerInterfaceAPI.Asset_GetState(handle);
        }

        public float GetCurrentTime()
        {
            if (handle == Types.INVALID_HANDLE)
                return 0;

            return HvrPlayerInterfaceAPI.Asset_GetCurrentTime(handle);
        }

        public float GetActualTime()
        {
            if (handle == Types.INVALID_HANDLE)
                return 0;

            return HvrPlayerInterfaceAPI.Asset_GetActualTime(handle);
        }

        public float GetDuration()
        {
            if (handle == Types.INVALID_HANDLE)
                return 0;

            return HvrPlayerInterfaceAPI.Asset_GetDuration(handle);
        }

        public string GetCodec()
        {
            if (handle == Types.INVALID_HANDLE)
                return string.Empty;

            StringBuilder stringBuilder = new StringBuilder(256);
            if (HvrPlayerInterfaceAPI.Asset_GetCodec(handle, stringBuilder, stringBuilder.Capacity))
            {
                return stringBuilder.ToString();
            }

            return string.Empty;
        }

        public float GetDecodeTime()
        {
            if (handle == Types.INVALID_HANDLE)
                return 0;
            return HvrPlayerInterfaceAPI.Asset_GetDecodeTime(handle);
        }

        public int GetVoxelCount()
        {
            if (handle == Types.INVALID_HANDLE)
                return 0;

            return HvrPlayerInterfaceAPI.Asset_GetVoxelCount(handle);
        }

        public float GetVoxelSize()
        {
            if (handle == Types.INVALID_HANDLE)
                return 0;

            return HvrPlayerInterfaceAPI.Asset_GetVoxelSize(handle);
        }

        public int GetCurrentBandwidth()
        {
            if (handle == Types.INVALID_HANDLE)
                return 0;

            return HvrPlayerInterfaceAPI.Asset_GetCurrentBandwidth(handle);
        }

        public bool HasNewFrameDecoded()
        {
            if (handle == Types.INVALID_HANDLE)
                return false;

            if (GetVoxelCount() != lastFrameVoxelCount)
            {
                lastFrameVoxelCount = GetVoxelCount();
                return true;
            }
            
            CommonTypes.Bounds b = HvrPlayerInterfaceAPI.Asset_GetBounds(handle);
            if ( b.center != lastFrameBounds.center &&
                 b.halfDims != lastFrameBounds.halfDims)
            {
                lastFrameBounds = HvrPlayerInterfaceAPI.Asset_GetBounds(handle);
                return true;
            }

            return false;
        }

        public float GetBufferFillRatio()
        {
            if (handle == Types.INVALID_HANDLE)
                return 0;

            return HvrPlayerInterfaceAPI.Asset_GetBufferFillRatio(handle);

        }

        // ITimestampProvider
        private HvrActor connectedHvrActor;
        public bool ProvidesTimestamp()
        {
            return true;
        }

        public float GetTimestamp()
        {
            return Helper.GetCurrentTime();
        }

        public void ConnectHvrActor(HvrActor actor)
        {
            if (actor != null)
            {
                actor.timestampProvider = this;
            }

            connectedHvrActor = actor;
        }

        public void DisconnectHvrActor()
        {
            if (connectedHvrActor != null && connectedHvrActor.timestampProvider == this)
            {
                connectedHvrActor.timestampProvider = null;
            }
        }

        // DEPRECATED
        public bool IsReadyToPlay()
        {
            return !IsInitializing();
        }

        public bool IsInitializing()
        {
            return (GetState() & HvrPlayerInterfaceAPI.ASSET_STATE_INITIALISING) != 0;
        }

        public bool IsPlaying()
        {
            return (GetState() & HvrPlayerInterfaceAPI.ASSET_STATE_PLAYING) != 0;
        }

        public bool IsSeeking()
        {
            return (GetState() & HvrPlayerInterfaceAPI.ASSET_STATE_SEEKING) != 0;
        }

        public bool IsOffline()
        {
            return (GetState() & HvrPlayerInterfaceAPI.ASSET_STATE_OFFLINE) != 0;
        }

        public bool IsFullyCached()
        {
            return (GetState() & HvrPlayerInterfaceAPI.ASSET_STATE_FULLY_CACHED) != 0;
        }

        public bool IsInvalid()
        {
            return (GetState() & HvrPlayerInterfaceAPI.ASSET_STATE_INVALID) != 0;
        }

        protected virtual void OnAssetInitialised(
            int error,
            IntPtr userData
        )
        {
        }

        protected virtual bool OnAssetSelectRepresentation(
            CommonTypes.HVRAdaptationSet adaptionSet,
            uint representationIndex,
            CommonTypes.HVRRepresentation[] representations,
            uint representationCount,
            IntPtr userData
        )
        {
        	return true;
        }

        
        protected virtual void OnAssetRepresentationDataReceived(
            [In] IntPtr mimeType,
            [In] IntPtr codec,
            float startTime,
            [In] IntPtr data,
            uint dataSize,
            IntPtr userData
        )
        {
        }

        protected void FireOnAssetRepresentationDataReceivedEvent(
        	[In] IntPtr mimeType,
            [In] IntPtr codec,
            float startTime,
            [In] IntPtr data,
            uint dataSize,
            IntPtr userData)
        {
        	if (onAssetRepresentationDataReceived != null)
        		onAssetRepresentationDataReceived(mimeType, codec, startTime, data, dataSize, userData);
        }

        protected void FireOnAssetInitializedEvent(int error, IntPtr userData)
        {
        	if (onAssetInitialized != null)
                onAssetInitialized(error, userData);
        }
    }
}
