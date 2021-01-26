using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    private AudioSource _audio;
    public AudioClip backgroundSound;
    // Start is called before the first frame update
    void Start()
    {
        _audio =  GetComponent<AudioSource>();
       // playsound(backgroundSound , _audio);


    }

    // Update is called once per frame
   public static void playsound(AudioClip clip , AudioSource audioPlayer , bool  _loop)
    {
        audioPlayer.Stop();
        audioPlayer.clip = clip;
        audioPlayer.loop = _loop;
        audioPlayer.time = 0;
        audioPlayer.Play();
    }
}
