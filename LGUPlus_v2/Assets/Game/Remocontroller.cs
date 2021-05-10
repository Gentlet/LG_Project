using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Remocontroller : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip guideSound;

    private float touchTime;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            touchTime = Time.time;

        if (Time.time - touchTime >= 30f)
        {
            audioSource.PlayOneShot(guideSound);
            touchTime = Time.time;
        }
    }
    public void LoadScene(string name)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }
}
