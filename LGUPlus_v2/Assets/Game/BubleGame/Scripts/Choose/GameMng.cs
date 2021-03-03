using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Choose
{
    public class GameMng : MonoBehaviour
    {
        public void LoadScene(string _SceneName)
        {
            SceneManager.LoadScene(_SceneName);
        }
        void Start()
        {
            QualitySettings.vSyncCount = 2;


            //Screen.SetResolution(4320, 1280, false);
            Screen.SetResolution(1920, 1080, true);
        }

        void Update()
        {

        }
    }
}