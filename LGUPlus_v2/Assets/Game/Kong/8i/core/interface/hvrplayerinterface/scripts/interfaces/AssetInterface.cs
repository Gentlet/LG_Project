using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

using AOT;

namespace HVR.Interface
{
  
    public class AssetInterface : BaseAssetInterface
    {
        class AssetRepresentationDataCache {
            public byte[] mimeType;
            public byte[] codec;
            public float startTime;
            public byte[] data;
            public uint dataSize;
            public IntPtr userData;
        }
        List<AssetRepresentationDataCache> dataCacheList = new List<AssetRepresentationDataCache>();



        private byte[] selectedCodecAnsi = null;

        private static IntPtr RAW_HVR_ERROR_MANIFEST_NOT_FOUND         = ByteArrayStringHelper.CreateRawMemoryFromString("HVR_ERROR_MANIFEST_NOT_FOUND");
        private static IntPtr RAW_HVR_ERROR_MANIFEST_INVALID           = ByteArrayStringHelper.CreateRawMemoryFromString("HVR_ERROR_MANIFEST_INVALID");
        private static IntPtr RAW_HVR_ERROR_REPRESENTATION_NOT_FOUND   = ByteArrayStringHelper.CreateRawMemoryFromString("HVR_ERROR_REPRESENTATION_NOT_FOUND");
        private static IntPtr RAW_HVR_ERROR_REPRESENTATION_INVALID     = ByteArrayStringHelper.CreateRawMemoryFromString("HVR_ERROR_REPRESENTATION_INVALID");
        private static IntPtr RAW_HVR_ERROR_NO_VALID_DECODER_FOUND     = ByteArrayStringHelper.CreateRawMemoryFromString("HVR_ERROR_NO_VALID_DECODER_FOUND");
        private static IntPtr RAW_HVR_ERROR_OFFLINE_CACHE_INVALID      = ByteArrayStringHelper.CreateRawMemoryFromString("HVR_ERROR_OFFLINE_CACHE_INVALID");
        private static IntPtr RAW_HVR_ERROR_NO_VOLUMETRIC_TRACK        = ByteArrayStringHelper.CreateRawMemoryFromString("HVR_ERROR_NO_VOLUMETRIC_TRACK");
        private static IntPtr RAW_HVR_ERROR_FAILED_TO_READ_FRAMES      = ByteArrayStringHelper.CreateRawMemoryFromString("HVR_ERROR_FAILED_TO_READ_FRAMES");
        private static IntPtr RAW_HVR_ERROR_FAILED_TO_DECODE_FRAME     = ByteArrayStringHelper.CreateRawMemoryFromString("HVR_ERROR_FAILED_TO_DECODE_FRAME");
        private static IntPtr RAW_HVR_ERROR_UNKNOWN                    = ByteArrayStringHelper.CreateRawMemoryFromString("HVR_ERROR_UNKNOWN");

        protected override void OnAssetInitialised(
            int error,
            IntPtr userData
        )
        {
            if (!IsValid())
                return;

            UnityInterface.Lock();

            FireOnAssetInitializedEvent(error, userData);

            if (error != ErrorCodes.HVR_ERROR_SUCCESS)
            {
                m_lastError = error;

                switch (error)
                {
                    case ErrorCodes.HVR_ERROR_MANIFEST_NOT_FOUND:
                        UnityInterfaceAPI.LogBuffer_Add(0, RAW_HVR_ERROR_MANIFEST_NOT_FOUND);
                        break;
                    case ErrorCodes.HVR_ERROR_MANIFEST_INVALID:
                        UnityInterfaceAPI.LogBuffer_Add(0, RAW_HVR_ERROR_MANIFEST_INVALID);
                        break;
                    case ErrorCodes.HVR_ERROR_REPRESENTATION_NOT_FOUND:
                        UnityInterfaceAPI.LogBuffer_Add(0, RAW_HVR_ERROR_REPRESENTATION_NOT_FOUND);
                        break;
                    case ErrorCodes.HVR_ERROR_REPRESENTATION_INVALID:
                        UnityInterfaceAPI.LogBuffer_Add(0, RAW_HVR_ERROR_REPRESENTATION_INVALID);
                        break;
                    case ErrorCodes.HVR_ERROR_NO_VALID_DECODER_FOUND:
                        UnityInterfaceAPI.LogBuffer_Add(0, RAW_HVR_ERROR_NO_VALID_DECODER_FOUND);
                        break;
                    case ErrorCodes.HVR_ERROR_OFFLINE_CACHE_INVALID:
                        UnityInterfaceAPI.LogBuffer_Add(0, RAW_HVR_ERROR_OFFLINE_CACHE_INVALID);
                        break;
                    case ErrorCodes.HVR_ERROR_NO_VOLUMETRIC_TRACK:
                        UnityInterfaceAPI.LogBuffer_Add(0, RAW_HVR_ERROR_NO_VOLUMETRIC_TRACK);
                        break;
                    case ErrorCodes.HVR_ERROR_FAILED_TO_READ_FRAMES:
                        UnityInterfaceAPI.LogBuffer_Add(0, RAW_HVR_ERROR_FAILED_TO_READ_FRAMES);
                        break;
                    case ErrorCodes.HVR_ERROR_FAILED_TO_DECODE_FRAME:
                        UnityInterfaceAPI.LogBuffer_Add(0, RAW_HVR_ERROR_FAILED_TO_DECODE_FRAME);
                        break;
                    default:
                        UnityInterfaceAPI.LogBuffer_Add(0, RAW_HVR_ERROR_UNKNOWN);
                        break;
                }
            }

            UnityInterface.Unlock();
        }


        static readonly byte[] volumetricMimeType = new byte[] {
            (byte)'v',
            (byte)'o',
            (byte)'l',
            (byte)'u',
            (byte)'m',
            (byte)'e',
            (byte)'t',
            (byte)'r',
            (byte)'i',
            (byte)'c',
            (byte)'/'
        };

        public void SetSelectedCodec(string codec)
        {
            if (string.IsNullOrEmpty(codec))
            {
                selectedCodecAnsi = null;
                return;
            }

            selectedCodecAnsi = new byte[codec.Length + 1];
            for(int i = 0; i < codec.Length; ++i)
            {
                selectedCodecAnsi[i] = (byte)codec[i];
            }

            selectedCodecAnsi[codec.Length] = (byte)0;
        }

        protected override bool OnAssetSelectRepresentation(
            CommonTypes.HVRAdaptationSet adaptionSet,
            uint representationIndex,
            CommonTypes.HVRRepresentation[] representations,
            uint representationCount,
            IntPtr userData
        )
        {
            if (!IsValid())
                return false;

            // Why not just convert to string and compare? Unity has trouble doing so in native code callbacked managed code
            // Seems making native call or just plain C# code is more reliable.
            byte[] mimeType = ByteArrayStringHelper.GetByteArrayFromRawMemory(adaptionSet.mimeType);
            byte[] codec = ByteArrayStringHelper.GetByteArrayFromRawMemory(adaptionSet.codec);

            if (!ByteArrayStringHelper.ByteArrayCompare(mimeType, volumetricMimeType, volumetricMimeType.Length))
            {
                // select all other data representation than volumetric, like audio
                return true;
            }

            
            // exclude unwanted codec
            if (selectedCodecAnsi != null && !ByteArrayStringHelper.ByteArrayCompare(codec, selectedCodecAnsi, selectedCodecAnsi.Length))
            {
                return false;
            }

#if UNITY_WEBGL
            if(representations.Length == 1) 
            {
                if(representations[representationIndex].maxVoxelCount > 600000) 
                {
                    Debug.LogWarning("Attempting to load EightI asset that may negatively impact playback performance");
                }
                return true;
            }
            // Limit to 30fps / 600k voxels
            if(representations[representationIndex].maxFPS <= 15.0 && representations[representationIndex].maxVoxelCount <= 600000) 
            {
                return true;
            }

            return false;
#else

            return true;
#endif
            
        }

        
        protected override void OnAssetRepresentationDataReceived(
            [In] IntPtr mimeType,
            [In] IntPtr codec,
            float startTime,
            [In] IntPtr data,
            uint dataSize,
            IntPtr userData
        )
        {
            if (!IsValid())
                return;

            UnityInterface.Lock();

            // clear cache before triggering events
            ClearAssetRepresentationDataCache();

            FireOnAssetRepresentationDataReceivedEvent(mimeType, codec, startTime, data, dataSize, userData);

            // cache data for late-register callbacks
            AssetRepresentationDataCache cache = new AssetRepresentationDataCache {
                mimeType = ByteArrayStringHelper.GetByteArrayFromRawMemory(mimeType),
                codec = ByteArrayStringHelper.GetByteArrayFromRawMemory(codec),
                startTime = startTime,
                dataSize = dataSize,
                userData = userData
            };

            // allocate unmanaged data and bitwise copy data
            cache.data = new byte[dataSize];
            Marshal.Copy(data, cache.data, 0, (int)dataSize);

            dataCacheList.Add(cache);

            UnityInterface.Unlock();
        }

        public void ClearAssetRepresentationDataCache()
        {
            dataCacheList.Clear();
        }

        public void FetchAssetRepresentationDataCache(OnAssetRepresentationDataReceivedHandler handler)
        {
            if (!IsValid())
                return;

            if (handler != null)
            {
                foreach(var cache in dataCacheList)
                {
                    IntPtr unmanagedBlock = Marshal.AllocHGlobal((int)cache.dataSize);
                    Marshal.Copy(cache.data, 0, unmanagedBlock, (int)cache.dataSize);

                    IntPtr mimeType, codec;
                    mimeType = Marshal.AllocHGlobal((int)cache.mimeType.Length+1);
                    codec = Marshal.AllocHGlobal((int)cache.codec.Length+1);
                    ByteArrayStringHelper.WriteRawMemoryFromByteArray(mimeType, cache.mimeType);
                    ByteArrayStringHelper.WriteRawMemoryFromByteArray(codec, cache.codec);

                    handler(mimeType, codec, cache.startTime, unmanagedBlock, cache.dataSize, cache.userData);

                    Marshal.FreeHGlobal(mimeType);
                    Marshal.FreeHGlobal(codec);
                    Marshal.FreeHGlobal(unmanagedBlock);
                }
            }
        }
    }
}
