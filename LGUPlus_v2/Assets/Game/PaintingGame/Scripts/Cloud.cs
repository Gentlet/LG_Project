using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    RectTransform trans;

    public float swipping;
    public float speed;
    int dir = 1;

    void Start()
    {
        trans = transform.GetComponent<RectTransform>();

        StartCoroutine(Swipping());
    }

    void Update()
    {

    }

    IEnumerator Swipping()
    {
        dir = Random.Range(0, 2) == 0 ? 1 : -1;
        speed *= Random.Range(0.8f, 1.2f);

        while(true)
        {
            if (Mathf.Abs(trans.localPosition.y + swipping * speed) > swipping)
            {
                dir *= -1;

                trans.localPosition += Vector3.up * swipping * speed * dir;
            }

            trans.localPosition += Vector3.up * swipping * speed * dir;


            yield return new WaitForSeconds(0.01f);
        }
    }

}
