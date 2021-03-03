using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedScript : MonoBehaviour
{
    private Rigidbody rigidBody;

    private GameScript gameScript;

    // Use this for initialization
    void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        MoveFeed();
    }

    void FixedUpdate()
    {
        Vector3 pos = GameObject.FindGameObjectWithTag("camera1").GetComponent<Camera>().WorldToViewportPoint(gameObject.transform.position);

        if (pos.y < DEFINE.OBJECT_IDLE_MIN_Y)
        {
            ObjectPool.Instance.PushToPool("Feed", gameObject);
        }
    }

    void MoveFeed()
    {
        rigidBody.velocity = new Vector3(0, DEFINE.FEED_SPEED_Y, 0);
    }
}
