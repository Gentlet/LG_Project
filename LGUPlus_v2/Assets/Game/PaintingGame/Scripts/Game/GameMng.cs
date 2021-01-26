using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Painting
{
    public class GameMng : MonoBehaviour
    {
        public OuttroMng outtroMng;

        public Camera camera;

        public GameObject waittingObject;
        public GameObject playObject;
        public GameObject resultObject;

        public AudioClip[] bgms;

        public GameObject colorObjects;
        public int colorActive = 0;

        public AudioSource bgAudioSource;

        public AudioSource audioSource;

        public ParticleSystem touchEffect;

        private bool isPlay;

        private void Update()
        {
            if (isPlay)
            {
                //for (int i = 0; i < Input.touchCount; i++)
                //{
                //    OnTouch(Input.touches[i].position);

                //    for (int j = 0; j < colorObjects.transform.childCount; j++)
                //    {
                //        if (colorObjects.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled == false)
                //            return;
                //    }

                //    StopAllCoroutines();
                //    GameEnd();
                //}

                //Debug.Log(Input.GetTouch(0));

                if (Input.GetMouseButtonDown(0) && isPlay)
                {
                    OnTouch(Input.mousePosition);

                    if (colorActive >= colorObjects.transform.childCount)
                    {
                        for (int i = 0; i < colorObjects.transform.childCount; i++)
                        {
                            if (colorObjects.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled == false)
                                return;
                        }

                        StopAllCoroutines();
                        StartCoroutine(GameEnd());
                    }
                }
            }
        }

        public void GameStateStart()
        {
            waittingObject.SetActive(true);
            playObject.SetActive(false);
            resultObject.SetActive(false);
        }

        public void GamePlayStart()
        {
            playObject.SetActive(true);
            waittingObject.SetActive(false);
            resultObject.SetActive(false);

            bgAudioSource.time = 0;
            bgAudioSource.clip = bgms[1];
            bgAudioSource.Play();

            StartCoroutine(NoUser());

            isPlay = true;
        }

        public IEnumerator GameEnd()
        {
            isPlay = false;

            yield return new WaitForSeconds(1.05f);

            var ps = Instantiate(touchEffect, Vector3.zero, Quaternion.identity, transform);
            ps.transform.localScale = Vector3.one * 2.5f;
            Destroy(ps.gameObject, 1.05f);
            audioSource.PlayOneShot(audioSource.clip);

            yield return new WaitForSeconds(4f);

            resultObject.SetActive(true);
            playObject.SetActive(false);
            waittingObject.SetActive(false);

            bgAudioSource.clip = bgms[2];
            bgAudioSource.Play();

        }


        public void Restart()
        {
            StopAllCoroutines();
            GamePlayStart();

            for (int i = 0; i < colorObjects.transform.childCount; i++)
            {
                colorObjects.transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = false;
                colorObjects.transform.GetChild(i).GetComponent<Collider2D>().enabled = true;
            }
        }

        public void StopGame()
        {
            outtroMng.gameObject.SetActive(true);
            gameObject.SetActive(false);


            bgAudioSource.gameObject.SetActive(false);
        }

        public IEnumerator NoUser()
        {
            yield return new WaitForSeconds(10f);

            if (gameObject.activeSelf == true)
                UnityEngine.SceneManagement.SceneManager.LoadScene("PaintingGameChoose");
        }

        public void OnTouch(Vector2 _pos)
        {
            StopAllCoroutines();

            Ray ray = camera.ScreenPointToRay(_pos);

            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.transform != null)
            {
                hit.collider.GetComponent<SpriteRenderer>().enabled = true;
                hit.collider.enabled = false;

                colorActive += 1;

                var ps = Instantiate(touchEffect, transform);
                ps.transform.position = hit.point;

                Destroy(ps.gameObject, 2f);

                audioSource.PlayOneShot(audioSource.clip);
            }

            StartCoroutine(NoUser());
        }
    }
}