using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace PaintingChoose
{
    public class GameMng : MonoBehaviour
    {
        public AudioSource audio;

        public void LoadScene(string _SceneName)
        {
            SceneManager.LoadScene(_SceneName);

            PlayerPrefs.SetFloat("Painting_Game_Audio_Timeline", audio.time);
        }
        void Start()
        {
            //Screen.SetResolution(4320, 1280, false);
            Screen.SetResolution(1920, 1080, true);
        }

        void Update()
        {

        }
    }
}