using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Remocontroller : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioSource guidaudioSource;
    public AudioClip guideSound;

    private float touchTime;

    public bool FastPlay = false;
    private void Start()
    {
        touchTime = Time.time;

        if (FastPlay)
            touchTime -= 27f;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            touchTime = Time.time;

        if (Time.time - touchTime >= 30f)
        {
            audioSource.volume = 0.2f;
            guidaudioSource.PlayOneShot(guideSound);
            touchTime = Time.time;

            Invoke("VolumReset", guideSound.length);
        }
    }
    public void VolumReset()
    {
        audioSource.volume = 1.0f;
    }

    public void LoadScene(string name)
    {
        DataSender.Instance.OpenGameBtns = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
}
