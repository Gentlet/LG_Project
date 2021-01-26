using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LightSpakling : MonoBehaviour
{
    List<Image> images = new List<Image>();

    float alpha = 0f;
    float alphaValue = 1f;


    void Start()
    {
        if (transform.childCount > 3)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.AddComponent<LightSpakling>();

            Destroy(this);
        }
        else
        {
            images.Add(GetComponent<Image>());
            images.AddRange(GetComponentsInChildren<Image>());

        }

        StartCoroutine(Spakling());
    }

    IEnumerator Spakling()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 8f));

            foreach (var image in images)
                image.color = Color.white - (Color.black * 0.7f);

            for (int i = 0; i < 2; i++)
            {
                foreach (var image in images)
                    image.color = Color.white;

                yield return new WaitForSeconds(0.02f);

                foreach (var image in images)
                    image.color = Color.white - (Color.black * 0.7f);

                yield return new WaitForSeconds(0.02f);
            }

            yield return new WaitForSeconds(0.08f);

            foreach (var image in images)
                image.color = Color.white;

            yield return new WaitForSeconds(0.02f);

            foreach (var image in images)
                image.color = Color.white - (Color.black * 0.7f);

            yield return new WaitForSeconds(0.02f);

            foreach (var image in images)
                image.color = Color.white;
        }
    }
}
