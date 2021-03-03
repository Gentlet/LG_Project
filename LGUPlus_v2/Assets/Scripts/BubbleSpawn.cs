using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleSpawn : MonoBehaviour {
	// Use this for initialization
	void Start () {
        Invoke("SpawnBubble", Random.Range(DEFINE.BUBBLE_TIME_RANDOM_MIN, DEFINE.BUBBLE_TIME_RANDOM_MAX) * DEFINE.BUBBLE_TIME_RANDOM_MULTI);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void SpawnBubble()
    {
        GameObject instance = ObjectPool.Instance.PopFromPool("Bubble");
        instance.transform.position = gameObject.transform.position;
        instance.SetActive(true);

        Invoke("SpawnBubble", Random.Range(DEFINE.BUBBLE_TIME_RANDOM_MIN, DEFINE.BUBBLE_TIME_RANDOM_MAX) * DEFINE.BUBBLE_TIME_RANDOM_MULTI);
    }
}
