using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bookcase : MonoBehaviour
{

    public Image image;

    public Sprite[] sprites;

    public int index;
    public float speed;

    void Start()
    {
        index = 0;
        StartCoroutine(BookcaseEffect());
    }


    IEnumerator BookcaseEffect()
    {
        while (true)
        {
            image.sprite = sprites[index % sprites.Length];
            image.color = Color.white - Color.black;

            while (image.color.a < 1)
            {
                image.color += Color.black * speed;

                yield return new WaitForSeconds(0.01f);
            }

            while (0 < image.color.a)
            {
                image.color -= Color.black * speed;

                yield return new WaitForSeconds(0.01f);
            }

            index += 1;
        }
    }
}
