using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dance : MonoBehaviour
{
    public List<Sprite> DanceImgList = new List<Sprite>();
    public List<string> DanceList = new List<string>();

    public MediaPlayer _mediaPlayer;

    public Image stillcut;

    public Text numText;

    public int index = 0;

    //public bool isOpen = false;
    // Start is called before the first frame update
    void Start()
    {
        string[] DanceListDatas = Resources.Load("DanceDatas/Dance").ToString().Replace('\r', ' ').Split('\n');


        for (int i = 0; i < DanceListDatas.Length; i++)
        {
            DanceImgList.Add(Resources.Load<Sprite>("Dance/" + DanceListDatas[i].Trim().Replace("_", " ")));
            DanceList.Add(DanceListDatas[i].Trim());
        }

        if (DataSender.Instance != null)
            index = DataSender.Instance.danceIndex;
        numText.text = (index + 1).ToString();
        stillcut.sprite = DanceImgList[index];

        DanceRead();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            DanceEnd();
    }

    public void Right()
    {
        if (index < DanceList.Count - 1)
            index += 1;

        stillcut.sprite = DanceImgList[index];
        numText.text = (index + 1).ToString();
    }
    public void Left()
    {
        if (0 < index)
            index -= 1;

        stillcut.sprite = DanceImgList[index];
        numText.text = (index + 1).ToString();
    }

    public void DanceRead()
    {
        stillcut.sprite = DanceImgList[index];
        _mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, "DanceVideos/" + DanceList[index] + ".mp4");
    }

    public void DanceEnd()
    {
        _mediaPlayer.Pause();
    }

    public void ChangeScene()
    {
        //    if (isOpen)
        //        DanceEnd();
        //    else
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
}
