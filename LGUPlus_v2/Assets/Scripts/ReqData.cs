using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
#if OFFLINE
public class ReqData
{
    public string id;
    public string cmd;
    public string posX;
    public string posY;
    public string type;
    public Texture texture;
}
#else
public class ReqData
{
    public string id;
    public string cmd;
    public string posX;
    public string posY;
    public string type;
    public string texture;
}
#endif

