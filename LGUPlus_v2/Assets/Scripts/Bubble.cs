using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    private float speed;
    private Rigidbody2D rigidBody2D;

    // Use this for initialization
    void Start()
    {
        rigidBody2D = gameObject.GetComponent<Rigidbody2D>();

        float randomMin = DEFINE.BUBBLE_SPEED_RANDOM_MIN;
        float randomMax = DEFINE.BUBBLE_SPEED_RANDOM_MAX;
        float randomDiv = DEFINE.BUBBLE_SPEED_RANDOM_DIV;
        float speedDiv = DEFINE.BUBBLE_SCALE_PER_SPEED_DIV;
        float speedBase = DEFINE.BUBBLE_SPEED_BASE_X;

        //bubble의 speed를 random으로 생성
        speed = Random.Range(randomMin, randomMax) / randomDiv;
        
        //bubble의 speed에 따라 scale 조절
        Vector3 scale = gameObject.transform.localScale;
        scale.x += speed / speedDiv - randomMin / speedDiv;
        scale.y = scale.x;
        gameObject.transform.localScale = scale;

        rigidBody2D.velocity = new Vector2(speed / DEFINE.BUBBLE_SPEED_X_PER_Y_DIV, speed);

        //bubble을 좌우로 흔듬
        InvokeRepeating("BubbleMove", speedBase / speed, speedBase / speed);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(gameObject.transform.position);

        if (pos.y > 1f) ObjectPool.Instance.PushToPool("Bubble", gameObject);
    }

    //bubble 의 x축 속도를 반전
    private void BubbleMove()
    {
        rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x * -1, speed);
    }
}
