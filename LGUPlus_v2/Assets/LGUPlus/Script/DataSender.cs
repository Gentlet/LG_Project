using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSender : MonoBehaviour
{

    private static DataSender _instance;
    public static DataSender Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log(" PlayData is null");
            }
            return _instance;
        }
    }
    void Awake()
    {
        if (_instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public int danceIndex;

    public void ChangeDanceIndex(int _idx)
    {
        danceIndex = _idx;
    }
}
