using UnityEngine;

namespace HVR.Android
{
#if UNITY_ANDROID
    public class AndroidAssetUnpacker
    {
        protected AndroidJavaObject currentActivity;
        protected AndroidJavaObject assetUnpacker;

        public AndroidAssetUnpacker()
        {
            currentActivity = AndroidUtils.GetCurrentActivity();
            assetUnpacker = new AndroidJavaObject("com.eighti.unity.androidutils.ObbUnpacker", currentActivity, Uniforms.buildDataPath);
        }

        public void Start()
        {
            if (!IsDone())
            {
                assetUnpacker.Call("unpack");
            }
        }

        public bool IsDone()
        {
            if (assetUnpacker == null)
                return false;

            return assetUnpacker.Call<bool>("isDone");
        }

        public float PercentComplete()
        {
            if (assetUnpacker == null)
                return 0f;

            return assetUnpacker.Call<float>("completedPercentage");
        }
    }
#endif
}
