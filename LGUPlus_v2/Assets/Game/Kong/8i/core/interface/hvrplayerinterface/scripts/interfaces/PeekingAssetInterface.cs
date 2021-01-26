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
  
    public class PeekingAssetInterface : BaseAssetInterface
    {
        private object mutex = new object();

        private List<byte[]> codecSet = new List<byte[]>();
        private bool finishedPeeking = false;

        protected override void OnAssetInitialised(
            int error,
            IntPtr userData
        )
        {
            lock(mutex)
            {
                finishedPeeking = true;
                m_lastError = error;
            }
        }

        public List<string> GetCodecList() {
            lock(mutex)
            {
                List<string> codeList = new List<string>();
                foreach(byte[] byteArray in codecSet)
                {
                    codeList.Add(ByteArrayStringHelper.GetStringFromByteArray(byteArray));
                }

                return codeList;
            }
        }

        public bool IsPeekingFinished() {
            lock(mutex)
            {
                return finishedPeeking;
            }
        }

        public bool HasError() {
            lock(mutex)
            {
                return m_lastError != ErrorCodes.HVR_ERROR_SUCCESS;
            }
        }

        public void Repeek() {

            lock(mutex)
            {
                m_lastError = ErrorCodes.HVR_ERROR_SUCCESS;
                codecSet.Clear();
                finishedPeeking = false;
            }

            string path = assetCreationInfo.assetPath;
            Create(path);
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

        protected override bool OnAssetSelectRepresentation(
            CommonTypes.HVRAdaptationSet adaptionSet,
            uint representationIndex,
            CommonTypes.HVRRepresentation[] representations,
            uint representationCount,
            IntPtr userData
        )
        {
            lock(mutex)
            {
                byte[] mimeType = ByteArrayStringHelper.GetByteArrayFromRawMemory(adaptionSet.mimeType);
                byte[] codec = ByteArrayStringHelper.GetByteArrayFromRawMemory(adaptionSet.codec);

                if (ByteArrayStringHelper.ByteArrayCompare(mimeType, volumetricMimeType, volumetricMimeType.Length))
                {
                    foreach(byte[] byteArray in codecSet)
                    {
                        if (!ByteArrayStringHelper.ByteArrayCompare(byteArray, codec, codec.Length))
                        {
                            codecSet.Add(codec);
                            break;
                        }
                    }
                }
                
            }

            return true;
        }

        
    }
}
