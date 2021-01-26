using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LightTwingle : MonoBehaviour
{
    List<Image> images = new List<Image>();

    float alpha = 0f;
    float alphaValue = 1f;


    void Start()
    {
        if (transform.childCount > 1)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.AddComponent<LightTwingle>();

            Destroy(this);
        }
        else
        {
            images.Add(GetComponent<Image>());

            if (GetComponentsInChildren<Image>() != null)
                images.AddRange(GetComponentsInChildren<Image>());

        }

        alpha = Random.Range(2.5f, 7f) / 1000f;
    }

    void Update()
    {
        alphaValue += alpha;

        if (1 <= alphaValue)
            alpha = Mathf.Abs(alpha) * -1;
        else if (alphaValue <= 0)
            alpha = Mathf.Abs(alpha);

        alphaValue += alpha;

        foreach (var image in images)
            image.color += new Color(0, 0, 0, alpha);
    }
}
