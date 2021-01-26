using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Solo
{
    public class GameMng : MonoBehaviour
    {
        public OuttroMng outtroMng;

        public Camera camera;

        public GameObject waittingObject;
        public GameObject playObject;
        public GameObject resultObject;

        public Transform targetParent;
        public Target targetPrefab;

        public GameObject titleText;
        public Text scoreText;
        public Text timerText;

        public Text delayTimerText;

        public GameObject newRecordPopup;
        public GameObject resultPopup;

        public Text resultScoreText;
        public Text resultHighScoreText;

        public AudioSource bgAudioSource;

        public AudioSource audioSource;

        public AudioClip readySound;
        public AudioClip countSound;
        public AudioClip startSound;
        public AudioClip resultSound;

        private int score;
        private float timer;


        private bool isPlay;

        private bool debugValueChange = false;

        string[] texts = new string[4] {
            PlayerPrefs.GetFloat("SoloMinSize", 0.3f).ToString(),
            PlayerPrefs.GetFloat("SoloMaxSize", 0.7f).ToString(),
            PlayerPrefs.GetFloat("SoloGravityScale", 1f).ToString(),
            PlayerPrefs.GetFloat("SoloGenTime", 0.6f).ToString(),
        };

        void OnGUI()
        {
            if (debugValueChange)
            {
                var fontset = new GUIStyle();
                fontset.fontSize = 40;

                texts[0] = GUI.TextField(new Rect(0, 0, 200, 50), texts[0], fontset);
                texts[1] = GUI.TextField(new Rect(0, 50, 200, 50), texts[1], fontset);
                texts[2] = GUI.TextField(new Rect(0, 100, 200, 50), texts[2], fontset);
                texts[3] = GUI.TextField(new Rect(0, 150, 200, 50), texts[3], fontset);

                PlayerPrefs.SetFloat("SoloMinSize", float.Parse(texts[0]));
                PlayerPrefs.SetFloat("SoloMaxSize", float.Parse(texts[1]));
                PlayerPrefs.SetFloat("SoloGravityScale", float.Parse(texts[2]));
                PlayerPrefs.SetFloat("SoloGenTime", float.Parse(texts[3]));
            }
        }

        private void Update()
        {
            if (isPlay)
            {
                Timer -= Time.deltaTime;

                if (Input.GetMouseButtonDown(0))
                    OnTouch(Input.mousePosition);

            }

            if (Input.GetKeyDown(KeyCode.D))
                debugValueChange = !debugValueChange;
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

            titleText.SetActive(false);
            scoreText.gameObject.SetActive(true);
            timerText.gameObject.SetActive(true);


            Timer = 60f;
            Score = 0;

            StartCoroutine(GamePlayDelay());
        }

        public void GameEnd()
        {
            resultObject.SetActive(true);
            waittingObject.SetActive(false);
            playObject.SetActive(false);


            titleText.SetActive(true);
            scoreText.gameObject.SetActive(false);
            timerText.gameObject.SetActive(false);

            newRecordPopup.SetActive(false);
            resultPopup.SetActive(false);

            delayTimerText.gameObject.SetActive(false);

            while (targetParent.childCount > 0)
                DestroyImmediate(targetParent.GetChild(0).gameObject);

            audioSource.PlayOneShot(resultSound);

            int day = PlayerPrefs.GetInt("Day", 0);

            if (day != System.DateTime.Now.Day)
            {
                PlayerPrefs.SetInt("Day", System.DateTime.Now.Day);
                PlayerPrefs.SetString("HighScore", "0");
            }


            string scorestring = PlayerPrefs.GetString("HighScore", "0");
            if (int.Parse(scorestring) < score)
            {
                PlayerPrefs.SetString("HighScore", score.ToString());

                newRecordPopup.SetActive(true);
            }
            else
            {
                resultPopup.SetActive(true);
            }


            resultHighScoreText.text = PlayerPrefs.GetString("HighScore", "0");
            resultScoreText.text = score.ToString();

            StartCoroutine(NoUser());
        }

        public void Restart()
        {
            StopAllCoroutines();
            GamePlayStart();
        }

        public void StopGame()
        {
            outtroMng.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        public IEnumerator NoUser()
        {
            yield return new WaitForSeconds(5f);

            if (gameObject.activeSelf == true)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        }

        public void OnTouch(Vector2 _pos)
        {
            Ray ray = camera.ScreenPointToRay(_pos);

            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit != null && hit.transform != null && hit.transform.GetComponent<Target>() != null)
            {
                hit.transform.GetComponent<Target>().Touched();
                Score += 1;
            }
        }

        IEnumerator GamePlayDelay()
        {
            delayTimerText.gameObject.SetActive(true);

            bgAudioSource.volume = 0.4f;

            audioSource.PlayOneShot(readySound);
            delayTimerText.text = "준비";

            yield return new WaitForSeconds(1.2f);

            for (int i = 5; i > 0; i--)
            {
                audioSource.PlayOneShot(countSound);
                delayTimerText.text = i.ToString();

                yield return new WaitForSeconds(1f);
            }

            audioSource.PlayOneShot(startSound);
            delayTimerText.text = "시작~~!";

            yield return new WaitForSeconds(1.2f);

            bgAudioSource.volume = 1f;

            delayTimerText.gameObject.SetActive(false);

            isPlay = true;
            StartCoroutine(TargetCreate());
        }

        IEnumerator TargetCreate()
        {
            while (isPlay)
            {
                var obj = Instantiate(targetPrefab, targetParent);
                obj.gameObject.SetActive(true);

                obj.transform.position = new Vector3(Random.Range(-3.8f, 3.8f), 10, 0);

                obj.transform.localScale = Vector3.one * Random.Range(PlayerPrefs.GetFloat("SoloMinSize", 0.3f), PlayerPrefs.GetFloat("SoloMaxSize", 0.7f));
                //obj.GetComponent<Rigidbody2D>().gravityScale = PlayerPrefs.GetFloat("SoloGravityScale", 1f);

                yield return new WaitForSeconds(/*PlayerPrefs.GetFloat("SoloGenTime", 0.6f)*/ 2f + Random.Range(-0.15f, 0.15f));
            }
        }

        #region Properties
        public float Timer
        {
            get
            {
                return timer;
            }
            set
            {
                timer = value;

                timerText.text = ((int)timer / 60).ToString("0") + ":" + ((int)timer % 60).ToString("00");

                if ((int)timer < 6)
                {
                    bgAudioSource.volume = 0.4f;

                    delayTimerText.gameObject.SetActive(true);

                    if (string.Compare(delayTimerText.text, ((int)timer % 60).ToString("0")) == 1)
                        audioSource.PlayOneShot(countSound);

                    delayTimerText.text = ((int)timer % 60).ToString("0");
                }

                if (timer < 1)
                {
                    audioSource.PlayOneShot(countSound);
                    isPlay = false;

                    delayTimerText.text = "끝~~!";

                    Invoke("GameEnd", 1f);
                }
            }
        }

        public int Score
        {
            get
            {
                return score;
            }

            set
            {
                score = value;

                scoreText.text = score.ToString();
            }
        }

        #endregion
    }
}