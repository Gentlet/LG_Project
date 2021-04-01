using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UniRx;
using UniRx.Triggers;
using System;

public class GameScript : MonoBehaviour
{
    public bool isRelease;
    private string domain;

    public DEFINE.SwitchStatus SpawnStatus;

    private string deviceId;

    private Camera camera1, camera2, camera3, camera4;

    private List<Vector3> mpList = new List<Vector3>();

    private bool isStarted;
    private float elapsedTime;
    private float limitTime = 1200f;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Cursor.visible = false;
        //MultiDisplay();
        deviceId = SystemInfo.deviceUniqueIdentifier;
        InitCamera();

        //string releaseDomain = "http://lguplusmegastore.com/";
        //string releaseDomain = "http://aqua.htmik.com/";
        string releaseDomain = "http://aqua1.bytechtree.com/";

        domain = isRelease ? releaseDomain : "http://th.htmik.com/";
        StartCoroutine(CallRespawnAPI());
    }

    // Use this for initialization
    void Start()
    {
        isStarted = true;
        //req 반복 호출 시작
        StartCoroutine(Req());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            elapsedTime = 0f;
            StartCoroutine(LeftClick());
        }

        if (Input.GetMouseButtonDown(1))
        {
            elapsedTime = 0f;
            RightClick();
        }

        if (isStarted)
        {
            elapsedTime += Time.deltaTime;

            if(elapsedTime >= limitTime)
            {
                isStarted = false;
                elapsedTime = 0f;
                SceneManager.LoadScene("Main");
            }
        }
    }

    private IEnumerator LeftClick()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        Vector3 mp = Input.mousePosition;
        int camera = 1;

        // 가능하다면 이부분에서 camera3 과 camera4 의 보정 방식을 통일하자
        if(false && mp.x > 3840 && mp.x < 4920) // htmik safe
        {
            mp.x -= 460;
        } else if(mp.x > 5760)
        {
            camera = 4;
        }

        if (camera == 4)
        {
            StartCoroutine(Step(ScreenToWorldPointByCamera(mp, camera)));
            yield break;
        }

        foreach (Vector3 item in mpList)
        {
            // double click
            if((mp - item).sqrMagnitude < 30 * 30)
            {
                mpList.Remove(item);
                DropObject("Feed", ScreenToWorldPointByCamera(mp, camera));
                yield break;
            }
        }

        mpList.Add(mp);
        yield return wait;
        if(mpList.IndexOf(mp) != -1)
        {
            // single click
            mpList.Remove(mp);
            DropObject("Boom", ScreenToWorldPointByCamera(mp, camera));
        }
    }

    //우클릭
    private void RightClick()
    {
        DropObject("Feed", ScreenToWorldPointByCamera(Input.mousePosition));
    }

    private GameObject DropObject(string itemName, Vector3 wp)
    {
        GameObject gameObject = ObjectPool.Instance.PopFromPool(itemName);
        gameObject.transform.position = wp;
        gameObject.SetActive(true);

        return gameObject;
    }

    private IEnumerator DropObjectDelay(string itemName, Vector3 wp, float time)
    {
        WaitForSeconds wait = new WaitForSeconds(time);
        yield return wait;

        GameObject gameObject = ObjectPool.Instance.PopFromPool(itemName);
        gameObject.transform.position = wp;
        gameObject.SetActive(true);
    }

    private Vector3 ScreenToWorldPointByCamera(Vector3 mp, int cameraNum = 1)
    {
        Camera camera;
        Vector3 ret;

        // 가능하다면, camera3번과 4번의 보정 방식을 통일할 것
        // camera4 번의 vp.x 가 0일때 mp.x 값 n을 구해서, 5760 - n 하면 될듯
        switch(cameraNum)
        {
            case 4:
                camera = camera4;
                Vector3 vp = camera.ScreenToViewportPoint(new Vector3(mp.x, mp.y, DEFINE.BASE_POSITION_Z));
                vp.x -= 3.945204f;
                ret = camera.ViewportToWorldPoint(vp);
                break;
            case 1:
            case 2:
            case 3:
            default:
                camera = camera1;
                ret = camera.ScreenToWorldPoint(new Vector3(mp.x, mp.y, DEFINE.BASE_POSITION_Z));
                break;
        }

        return ret;
    }

    private IEnumerator Step(Vector3 wp)
    {
        bool b = false;
        Collider[] colliders = Physics.OverlapSphere(wp, 0.35f);

        foreach (Collider collider in colliders)
        {
            GameObject fish = collider.gameObject;
            string tag = fish.tag;
            if (tag.Contains("Fish") && fish.activeSelf)
            {
                b = true;
                Vector3 p = fish.transform.position;
                p.z = 4.9f;
                DropObject("Step2", p);
                StartCoroutine(DropObjectDelay("Step2-2", p, 3f));
                StartCoroutine(HideFish(fish));
                StartCoroutine(ShowFish(fish));
            }
        }

        if(!b)
        {
            DropObject("Step1", wp);
        }

        yield break;
    }

    private IEnumerator HideFish(GameObject fish)
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        yield return wait;

        fish.SetActive(false);
    }

    private IEnumerator ShowFish(GameObject fish)
    {
        WaitForSeconds wait = new WaitForSeconds(4f);
        yield return wait;

        fish.SetActive(true);
    }

    //서버 Req.php에 요청
    private IEnumerator Req()
    {
        WaitForSeconds wait = new WaitForSeconds(5f);
        while (true)
        {
            if (SpawnStatus == DEFINE.SwitchStatus.On)
            {                
                using (UnityWebRequest www = UnityWebRequest.Get(domain + "main/req?deviceid=" + deviceId))
                {
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        ReqDataCollection reqDatas = null;
                        try
                        {
                            reqDatas = JsonUtility.FromJson<ReqDataCollection>(www.downloadHandler.text);
                            Debug.Log("parsing JSON");
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);
                        }

                        if (reqDatas.reqDatas[0].id != "")
                        {
                            foreach(ReqData reqData in reqDatas.reqDatas)
                            {
                                yield return APICallSucceed(reqData);
                            }
                        } else
                        {
                            Debug.Log("reqDatas is null");
                        }
                    }

                    yield return wait;
                }
            }
            else
            {
                yield return wait;
            }
        }
    }

    //서버 Res.php로 응답을 보냄
    private IEnumerator Res(string id, string cmd)
    {        
        using (UnityWebRequest www = UnityWebRequest.Get(domain + "main/res?id=" + id + "&cmd=" + cmd + "&deviceid=" + deviceId))
        {
            yield return www.SendWebRequest();
            
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                ResData resData = JsonUtility.FromJson<ResData>(www.downloadHandler.text);
                Debug.Log("response success! : " + resData.id + resData.cmd);
            }
        }
    }

    //Req.php 콜백
    private IEnumerator APICallSucceed(ReqData reqData)
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag("Fish");
        GameObject fish = null;
        foreach (GameObject obj in fishes)
        {
            if (obj.GetComponent<Fish>().id == reqData.id)
            {
                fish = obj;
                break;
            }
        }

        //생성
        if (reqData.cmd == "1" && fish == null)
        {
            fish = ObjectPool.Instance.PopFromPool("Fish" + reqData.type);
            fish.GetComponent<Fish>().id = reqData.id;

            Vector3 vp = new Vector3(float.Parse(reqData.posX), float.Parse(reqData.posY), 5);
            Vector3 wp;
            if (false) { //htmik safe
                if(vp.x == 0 && vp.y == 0)
                {
                    vp.x = vp.y = 0.5f;
                }
                wp = (DEFINE.GetPositionType(int.Parse(reqData.type)) == DEFINE.PositionType.Normal ? camera1 : camera4).ViewportToWorldPoint(vp);
                fish.transform.position = wp;
            } else {
                vp.x = vp.y = 0.5f;
                wp = camera1.ViewportToWorldPoint(vp);
                fish.transform.position = wp;
            }

            yield return StartCoroutine(Work(fish, reqData.texture));

            GameObject bubble = ObjectPool.Instance.PopFromPool("Bubble");
            wp.z = 4.9f;
            bubble.transform.position = wp;

            bubble.SetActive(true);
            fish.SetActive(true);
        }
        //제거
        else if (reqData.cmd == "2" && fish != null)
        {
            ObjectPool.Instance.PushToPool("Fish" + fish.GetComponent<Fish>().fishType, fish);
        }

        yield return StartCoroutine(Res(reqData.id, reqData.cmd));
    }

    //texture 다운로드 후 object texture에 적용
    private IEnumerator Work(GameObject instance, string url)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Texture texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
                foreach(Renderer renderer in renderers)
                {
                    renderer.material.mainTexture = texture;
                }
            }
        }
    }

    private IEnumerator CallRespawnAPI()
    {        
        using (UnityWebRequest www = UnityWebRequest.Get(domain + "main/respawn"))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                //ResData resData = JsonUtility.FromJson<ResData>(www.downloadHandler.text);
                //Debug.Log("response success! : " + resData.id + resData.cmd);
            }
        }
    }

    private void MultiDisplay()
    {
        Debug.Log("displays connected: " + Display.displays.Length);
        // Display.displays[0] is the primary, default display and is always ON.
        // Check if additional displays are available and activate each.
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        if (Display.displays.Length > 2)
            Display.displays[2].Activate();
        if (Display.displays.Length > 3)
            Display.displays[3].Activate();
        if (Display.displays.Length > 4)
            Display.displays[4].Activate();
    }

    private void InitCamera()
    {
        try
        {
            //camera1 = GameObject.FindGameObjectWithTag("camera1").GetComponent<Camera>();
            //camera2 = GameObject.FindGameObjectWithTag("camera2").GetComponent<Camera>();
            //camera3 = GameObject.FindGameObjectWithTag("camera3").GetComponent<Camera>();
            //camera4 = GameObject.FindGameObjectWithTag("camera4").GetComponent<Camera>();

            camera1 = GameObject.FindGameObjectWithTag("camera1").GetComponent<Camera>();
            camera2 = GameObject.FindGameObjectWithTag("camera1").GetComponent<Camera>();
            camera3 = GameObject.FindGameObjectWithTag("camera1").GetComponent<Camera>();
            camera4 = GameObject.FindGameObjectWithTag("camera1").GetComponent<Camera>();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    [Serializable]
    public class ReqDataCollection
    {
        public ReqData[] reqDatas;
    }
}
