using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ServerCommunication : Singleton<ServerCommunication>
{
    #region [Server Communication]

    private void SendRequest<T>(string url, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        StartCoroutine(RequestCoroutine(url, callbackOnSuccess, callbackOnFail));
    }

    private IEnumerator RequestCoroutine<T> (string url, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        var www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
            callbackOnFail?.Invoke(www.error);
        } else
        {
            Debug.Log(www.downloadHandler.text);
            ParseResponse(www.downloadHandler.text, callbackOnSuccess, callbackOnFail);
        }
    }

    private void ParseResponse<T> (string data, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        var parseData = JsonUtility.FromJson<T>(data);
        callbackOnSuccess?.Invoke(parseData);
    }

    #endregion

    public void GetReq(UnityAction<ReqData> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        SendRequest(string.Format(ServerConfig.SERVER_API_URL_FORMAT, ServerConfig.API_GET_REQ), callbackOnSuccess, callbackOnFail);
    }

    public void GetRes(UnityAction<ResData> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        SendRequest(string.Format(ServerConfig.SERVER_API_URL_FORMAT, ServerConfig.API_GET_RES), callbackOnSuccess, callbackOnFail);
    }
}
