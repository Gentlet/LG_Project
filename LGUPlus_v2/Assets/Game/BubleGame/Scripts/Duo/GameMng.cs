using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Duo
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
        public Text[] scoreTexts;
        public Text[] timerTexts;

        public Text delayTimerText;

        public GameObject[] newRecordPopups;
        public GameObject[] resultPopups;

        public Text[] resultScoreTexts;
        public Text[] resultHighScoreTexts;

        public AudioSource bgAudioSource;

        public AudioSource audioSource;

        public AudioClip readySound;
        public AudioClip countSound;
        public AudioClip startSound;
        public AudioClip resultSound;


        private int[] score = new int[2];
        private float timer;

        private bool isPlay;

        private bool debugValueChange = false;

        string[] texts = new string[4] {
            PlayerPrefs.GetFloat("DuoMinSize", 0.3f).ToString(),
            PlayerPrefs.GetFloat("DuoMaxSize", 0.7f).ToString(),
            PlayerPrefs.GetFloat("DuoGravityScale", 1f).ToString(),
            PlayerPrefs.GetFloat("DuoGenTime", 0.6f).ToString(),
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

                PlayerPrefs.SetFloat("DuoMinSize", float.Parse(texts[0]));
                PlayerPrefs.SetFloat("DuoMaxSize", float.Parse(texts[1]));
                PlayerPrefs.SetFloat("DuoGravityScale", float.Parse(texts[2]));
                PlayerPrefs.SetFloat("DuoGenTime", float.Parse(texts[3]));
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
            foreach (var scoreText in scoreTexts)
                scoreText.gameObject.SetActive(true);
            foreach (var timerText in timerTexts)
                timerText.gameObject.SetActive(true);


            Timer = 60f;
            score[0] = 0;
            score[1] = 0;

            foreach (var scoreText in scoreTexts)
                scoreText.text = "0";

            StartCoroutine(GamePlayDelay());
        }

        public void GameEnd()
        {
            resultObject.SetActive(true);
            waittingObject.SetActive(false);
            playObject.SetActive(false);


            titleText.SetActive(true);
            foreach (var scoreText in scoreTexts)
                scoreText.gameObject.SetActive(false);
            foreach (var timerText in timerTexts)
                timerText.gameObject.SetActive(false);

            foreach (var newRecordPopup in newRecordPopups)
                newRecordPopup.SetActive(false);
            foreach (var resultPopup in resultPopups)
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

            for (int i = 0; i < 2; i++)
            {
                string scorestring = PlayerPrefs.GetString("HighScore", "0");
                if (int.Parse(scorestring) < score[i] && score[i] >= score[(i + 1) % score.Length]) 
                {
                    PlayerPrefs.SetString("HighScore", score[i].ToString());

                    newRecordPopups[i].SetActive(true);

                    resultHighScoreTexts[i].text = PlayerPrefs.GetString("HighScore", "0");
                    resultScoreTexts[i].text = score[i].ToString();
                    i = -1;
                }
                else if (i == 1 && int.Parse(scorestring) == score[i] && score[0] == score[1])
                {
                    PlayerPrefs.SetString("HighScore", score[i].ToString());

                    newRecordPopups[i].SetActive(true);

                    resultHighScoreTexts[i].text = PlayerPrefs.GetString("HighScore", "0");
                    resultScoreTexts[i].text = score[i].ToString();
                }
                else
                {
                    resultPopups[i].SetActive(true);

                    resultHighScoreTexts[i].text = PlayerPrefs.GetString("HighScore", "0");
                    resultScoreTexts[i].text = score[i].ToString();
                }
            }

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

                if (hit.transform.position.x < 0)
                {
                    score[0] += 1;
                    scoreTexts[0].text = score[0].ToString();
                }
                else
                {
                    score[1] += 1;
                    scoreTexts[1].text = score[1].ToString();
                }
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
                for (int i = 0; i < 2; i++)
                {
                    var obj = Instantiate(targetPrefab, targetParent);
                    obj.gameObject.SetActive(true);

                    obj.transform.position = new Vector3(
                        (i == 0 ? Random.Range(-3.8f, -1.8f) : Random.Range(1.8f, 3.8f)), 10, 0);

                    obj.transform.localScale = Vector3.one * Random.Range(PlayerPrefs.GetFloat("DuoMinSize", 0.3f), PlayerPrefs.GetFloat("DuoMaxSize", 0.6f));
                    //obj.GetComponent<Rigidbody2D>().gravityScale = PlayerPrefs.GetFloat("DuoGravityScale", 1f);
                }

                yield return new WaitForSeconds(/*PlayerPrefs.GetFloat("DuoGenTime", 0.6f)*/2f + Random.Range(-0.15f, 0.15f));
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

                foreach (var timerText in timerTexts)
                    timerText.text = ((int)timer / 60).ToString("0") + ":" + ((int)timer % 60).ToString("00");

                if ((int)timer < 6)
                {
                    bgAudioSource.volume = 0.4f;

                    delayTimerText.gameObject.SetActive(true);

                    if(string.Compare(delayTimerText.text, ((int)timer % 60).ToString("0")) == 1)
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

        #endregion
    }
}