using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOneShotPlay : MonoBehaviour
{
    public AudioClip audioClip;
    public AudioSource audioSources;

    private void Awake()
    {
        StartCoroutine(SoundPlayDelayTime(4f));
        StartCoroutine(SoundPlayTime(60f));
    }

    IEnumerator SoundPlayTime(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        audioSources.Play();
        StartCoroutine(SoundPlayTime(60f));
    }

    IEnumerator SoundPlayDelayTime(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        audioSources.Play();
    }
}
