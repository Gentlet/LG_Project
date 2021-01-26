using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HScroolBarManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform hBar;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdataScrool( float  value)
    {

        this.GetComponent<Image>().fillAmount += value;
      
    }


}
