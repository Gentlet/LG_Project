using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solo
{
    public class IntroMng : MonoBehaviour
    {
        public GameMng gameMng;

        public GameObject aniObject;
        public AudioSource audioSource;

        public GameObject[] kongAnimations;

        public Animator textAnimator;

        void OnEnable()
        {
            StartCoroutine(IntroStart());

        }

        IEnumerator IntroStart()
        {
            aniObject.SetActive(true);

            audioSource.time = 0;
            audioSource.Play();

            textAnimator.playbackTime = 0;


            for (int i = 0; i < kongAnimations.Length; i++)
            {
                kongAnimations[i].SetActive(true);

                foreach (var animator in kongAnimations[i].GetComponentsInChildren<Animator>())
                    animator.playbackTime = 0;

                yield return new WaitForSeconds(3f);

                kongAnimations[i].SetActive(false);
            }


            aniObject.SetActive(false);

            gameMng.gameObject.SetActive(true);
            gameMng.GameStateStart();
            gameObject.SetActive(false);
        }

    }
}