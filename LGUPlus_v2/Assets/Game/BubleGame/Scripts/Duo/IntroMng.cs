using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Duo
{
    public class IntroMng : MonoBehaviour
    {
        public GameMng gameMng;

        public GameObject aniObject;
        public AudioSource audioSource;

        public GameObject kongAnimation;

        public Animator textAnimator;


        void OnEnable()
        {
            StartCoroutine(IntroStart());

        }

        IEnumerator IntroStart()
        {
            aniObject.SetActive(true);

            audioSource.Play();
            textAnimator.playbackTime = 0;

            kongAnimation.SetActive(true);

            textAnimator.playbackTime = 0;

            foreach (var animator in kongAnimation.GetComponentsInChildren<Animator>())
                animator.playbackTime = 0;

            yield return new WaitForSeconds(6f);

            kongAnimation.SetActive(false);


            aniObject.SetActive(false);

            gameMng.gameObject.SetActive(true);
            gameMng.GameStateStart();
            gameObject.SetActive(false);
        }

    }
}