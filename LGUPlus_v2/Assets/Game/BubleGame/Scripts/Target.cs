using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Sprite touchSprite;

    public AudioSource audioSource;
    public AudioClip touchEffect;

    private SpriteRenderer renderer;

    private bool isTouched;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Finish") && isTouched == false)
        {
            transform.localScale -= Vector3.one * 0.008f;

            if (transform.localScale.x < 0)
                Destroy(gameObject);
        }
    }

    public void Touched()
    {
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = touchSprite;

        transform.GetChild(0).rotation = Quaternion.identity;
        transform.GetChild(0).gameObject.SetActive(true);
        isTouched = true;

        Destroy(GetComponent<Rigidbody2D>());
        Destroy(GetComponent<Collider2D>());

        audioSource.PlayOneShot(touchEffect);

        StartCoroutine(Effect());
    }
    
    IEnumerator Effect()
    {
        float size = transform.localScale.x * 1.2f;

        while(transform.localScale.x < size)
        {
            transform.localScale *= 1.05f;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(0.15f);

        Destroy(gameObject);
    }

}