using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ThumbnailManager : MonoBehaviour
{

    //public Image
    // Start is called before the first frame update
    private float tempX = 0;
    private int count = 0;
    public Transform ThumObj;




    public TMPro.TextMeshProUGUI _countTxt;
    public TMPro.TextMeshProUGUI _titleNametxt;

    public Sprite arrowImgOn;
    public Sprite arrowImgOff;

    public Image leftBtnImg;
    public Image rightBtnImg;

    public GameObject leftBtn;
    public GameObject rightBtn;


    public CanvasGroup trave_thum;
    public CanvasGroup story_thum;

    public string[] _txtName;
    public float[] _vidoeTimeSection;

    //public float thumW = 175;
    public float thumW = 540;

    private bool isContral;
    private bool isThum = false;

    void Start()
    {
        _countTxt.text = "01";

        leftBtn.GetComponent<Image>().sprite = arrowImgOff;
        rightBtn.GetComponent<Image>().sprite = arrowImgOn;
        isContral = false;
    }

    void Update()
    {
        
    }

    public void OnThumbnail(bool isOn)
    {
        isContral = isOn;
        
        if (isOn)
        {

            trave_thum.alpha = 1;
            story_thum.alpha = 0;
            leftBtn.SetActive(true);
            rightBtn.SetActive(true);
            _titleNametxt.text = "우주 여행 1";

        } else
        {
            _titleNametxt.text = "아기돼지 삼형제";
            trave_thum.alpha = 0;
            story_thum.alpha = 1;
            leftBtn.SetActive(false);
            rightBtn.SetActive(false);
        }
    }

    public void init()
    {
        count  = 0;
        tempX =  0;
        _countTxt.text = "01";
        ThumObj.DOLocalMoveX( 0 , 0.5f );
        isContral = true;

        leftBtn.GetComponent<Image>().sprite = arrowImgOff;
        rightBtn.GetComponent<Image>().sprite = arrowImgOn;
    }
    
    public void LeftThum(bool isClick)
    {
     if (isContral == false) return;
        count--;
        if (count < 0)
        {
            count = 0;
            return;
        }

        if (count == 0)
        {
            Debug.Log("Left =================0  ");
            leftBtn.GetComponent<Image>().sprite = arrowImgOff;
        } else
        {

            leftBtn.GetComponent<Image>().sprite = arrowImgOn;
            rightBtn.GetComponent<Image>().sprite = arrowImgOn;
        }
     
        int _idx = count + 1;
        if (_idx == 0)
        {
            _countTxt.text = "01";
        }
        else
        {
            _countTxt.text = _idx.ToString("00");
        }
        tempX += thumW;
        ThumObj.DOLocalMoveX(tempX ,0.5f ).SetEase(Ease.OutQuint);

        if (isClick)
        {
            float timeNum = _vidoeTimeSection[count];
            GameObject.Find("VCR").GetComponent<RenderHeads.Media.AVProVideo.Demos.VCRContral>().OnVidepSeekValue(timeNum);
        }
        _titleNametxt.text = _txtName[count];
    }

    public void RightThum( bool isClick )
    {
  
        if (isContral == false) return;
        if (isThum== true) return;

        count++;

        if (count > 9){
            count = 0;

            float timeNum = _vidoeTimeSection[count];
            GameObject.Find("VCR").GetComponent<RenderHeads.Media.AVProVideo.Demos.VCRContral>().OnVidepSeekValue(timeNum);
            ThumObj.localPosition = new Vector3(175 , 0, 0);
            isThum = true;
            tempX = 0;
            ThumObj.DOLocalMoveX(tempX, 0.5f).SetEase(Ease.OutQuint); 

            leftBtn.GetComponent<Image>().sprite = arrowImgOff;
            rightBtn.GetComponent<Image>().sprite = arrowImgOn;
            StartCoroutine(delayTime());

        } else
        {
            GameObject.Find("VCR").GetComponent<RenderHeads.Media.AVProVideo.Demos.VCRContral>().IsNext();

            if (isClick)
            {
                float timeNum = _vidoeTimeSection[count];
                GameObject.Find("VCR").GetComponent<RenderHeads.Media.AVProVideo.Demos.VCRContral>().OnVidepSeekValue(timeNum);
            }
            tempX -= thumW;
            isThum = true;
            ThumObj.DOLocalMoveX(tempX, 0.5f).SetEase(Ease.OutQuint);
            leftBtn.GetComponent<Image>().sprite = arrowImgOn;
            rightBtn.GetComponent<Image>().sprite = arrowImgOn;
            StartCoroutine(delayTime());
        
        }
        _countTxt.text = (count+1).ToString("00");
        _titleNametxt.text = _txtName[count];
  
    }

        
    IEnumerator delayTime()
    {
        yield return new WaitForSeconds(1f);
        isThum = false;
    }
 





}
