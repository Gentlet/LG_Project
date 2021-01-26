using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Duo
{
    public class OuttroMng : MonoBehaviour
    {
        public Animator textAnimator;

        void OnEnable()
        {
            Invoke("Restart", 5f);
            textAnimator.playbackTime = 0;
        }

        void Restart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BubleGameChoose");
        }
    }
}