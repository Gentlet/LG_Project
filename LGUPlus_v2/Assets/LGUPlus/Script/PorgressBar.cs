using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PorgressBar : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("OBJECTS")]
    public Transform loadingBar;
    public Transform textPercent;

    [Range(0, 100)] public float currentPercent;
    [Range(0, 100)] public int speed;

    public float MaxTime = 20f;
    public float ActiveTime = 0f;
    public float percent = 0;



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        ActiveTime += Time.deltaTime;
        float per = ActiveTime / MaxTime;
        //Debug.Log("per >>>> " + per);
        //currentPercent = per;
        //slowDownFillImg.fillAmount = Mathf.Lerp(0, 1, percent);

    }
}
