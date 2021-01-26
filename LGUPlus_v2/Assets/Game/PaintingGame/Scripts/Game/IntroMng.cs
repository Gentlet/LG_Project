using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Painting
{
    public class IntroMng : MonoBehaviour
    {
        public GameMng gameMng;

        public AudioSource audioSource;

        public GameObject JuJuAnimation;



        void OnEnable()
        {
            StartCoroutine(IntroStart());
        }

        IEnumerator IntroStart()
        {
            audioSource.time = PlayerPrefs.GetFloat("Painting_Game_Audio_Timeline", 0f);

            audioSource.Play();

            JuJuAnimation.SetActive(true);

            foreach (var animator in JuJuAnimation.GetComponentsInChildren<Animator>())
                animator.playbackTime = 0;

            yield return new WaitForSeconds(6f);

            JuJuAnimation.SetActive(false);

            gameMng.gameObject.SetActive(true);
            gameMng.GameStateStart();
            gameObject.SetActive(false);
        }
    }
}