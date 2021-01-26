using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solo
{
    public class OuttroMng : MonoBehaviour
    {
        public Animator textAnimator;

        void OnEnable()
        {
            textAnimator.playbackTime = 0;
            Invoke("Restart", 5f);
        }

        void Restart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BubleGameChoose");
        }
    }
}