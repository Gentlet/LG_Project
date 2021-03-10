using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorBtnController : MonoBehaviour
{
    public Image[] btns;

    void Start()
    {

#if UNITY_EDITOR
        foreach (var btn in btns)
        {
            btn.color = Color.white;
            btn.transform.GetChild(0).gameObject.SetActive(true);
        }
#endif
    }

}
