using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeTime : MonoBehaviour {

	public float time;
    public string itemName;

    void Start()
    {
        
    }

    private void OnEnable()
    {
        Invoke("Die", time);
    }

    // Update is called once per frame
    void Update () {

    }

	private void Die() {
        ObjectPool.Instance.PushToPool(itemName, gameObject);
	}
}
