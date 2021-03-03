using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class APPManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Screen.SetResolution(4320, 1920 , false);
        Screen.SetResolution(1920, 1080 , true);
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        int _width = Screen.width;
        if (_width != 4320)
        {
          //  AppQuit();
        }
        //Debug.Log(_width);
    }

    public void AppQuit()
    {
        Application.Quit();
    }

    public void AppRestart(){
        SceneManager.LoadScene("Main");
    }

}
