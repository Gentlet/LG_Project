using System;
using System.IO;
using LiteDB;
using UnityEngine;
using UnityEngine.UI;

public class SaveAppData : MonoBehaviour
{
    [Tooltip("Determines which folder the app settings database should be saved in for this example.")]
    public SaveLocationDB SaveLocationDB = SaveLocationDB.PersistentFolder;

    [Tooltip("Determines the name of the database file for this example.")]
    public string DatabaseFilename = "lguplus.db";
    private LiteDatabase _db;



    // UI Controls:
    public Image ImageFill;

    public CanvasGroup[] popup;

    public TMPro.TextMeshProUGUI txtPer;

    private float currentCount = 0f;

    private float totalCount = 0;


    void Start()
    {
        string dbPath = GetDbFilePath();
        _db = new LiteDatabase(dbPath);
        LoadSettings();
        Debug.Log("The database file is located at >>>>>  " + dbPath);

       // popup[0].alpha = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadSettings()
    {
        LiteCollection<AppData> settingsCollection = _db.GetCollection<AppData>();
        AppData appData = settingsCollection.FindOne(Query.All());
        if (appData == null)
        {
            appData = AppData.GetDefaultSettings();
          //  Debug.Log("Existing settings were not found. Using default settings.");
        }
        else
        {
           // Debug.Log("Loading settings from the database.");
        }
        UpdateUIFromAppSettings(appData);
    }

    public void SaveSettings(int idx )
    {
        currentCount += idx;
      
        _db.GetCollection<AppData>();
        LiteCollection<AppData> settingsCollection = _db.GetCollection<AppData>();
        AppData appDatas = settingsCollection.FindOne(Query.All());

        if (appDatas == null)
        {
            // 초기화.. 
            appDatas = AppData.GetDefaultSettings();
        }
        Debug.Log("SaveSettings >>>>>  " + currentCount);

        GetAppSettingsFromUI(appDatas);

        if (settingsCollection.Upsert( appDatas ))
        {
            Debug.Log("The app settings were successfully inserted.");
        }
        else
        {
            Debug.Log("The existing app settings were updated.");
        }
    }

    public void ResetSettings()
    {
        _db.GetCollection<AppData>();
        LiteCollection<AppData> settingsCollection = _db.GetCollection<AppData>();
        AppData appDatas = settingsCollection.FindOne(Query.All());

        if (appDatas == null)
        {
            // 초기화.. 
            appDatas = AppData.GetDefaultSettings();
        }

        GetAppRersetSettingsFromUI(appDatas);

        if (settingsCollection.Upsert(appDatas))
        {
            Debug.Log("The app settings were successfully inserted.");
        }
        else
        {
            Debug.Log("The existing app settings were updated.");
        }
    }
    private void GetAppRersetSettingsFromUI(AppData appDatas)
    {
        appDatas.Score = "0";
        ImageFill.GetComponent<Image>().fillAmount = 0;
        
        popup[0].alpha = 0;
        popup[1].alpha = 0;
        popup[2].alpha = 0;
        popup[3].alpha = 0;

    }


        private void GetAppSettingsFromUI(AppData appDatas)
    {
        // 데이터 저장.
        appDatas.Score = currentCount.ToString();
        appDatas.TotalScore = totalCount.ToString();

        Debug.Log("GetAppSettingsFromUI  >  " + currentCount + "   total count :  " + +totalCount);

        //float value = currentCount * 0.005f;
        float value = currentCount * 0.000066f;

        ImageFill.GetComponent<Image>().fillAmount = value;
        float per = value * 100;
        // per = 0.3624800155f;
        txtPer.text = GetN2(per) + "%";//per.ToString()+"%";


        // text  저장 
        if (value > 0.1f &&  value < 0.2f)
        {
            popup[0].alpha = 1;
        } else if (value > 0.4f &&  value < 0.5f)
        {
            popup[1].alpha = 1;
        } else if (value > 0.6f && value < 0.7f)
        {
            popup[2].alpha = 1;
        } else if (value > 0.8f && value < 0.9f)
        {
            popup[3].alpha = 1;
        }

    }

    private string GetN2(float A)
    {
        string result = string.Empty;

        if (A == (int)A)
            result = A.ToString();
        else
            result = A.ToString("N1");

        return result;
    }


    private void OnDestroy()
    {
        // LiteDatabase instances should be disposed when they are no longer being used.
        // This will clean up any locks and resources they are using. This can also be
        // done with a "using" block.
        _db.Dispose();
    }

    public void SetToDefaultValues()
    {
        UpdateUIFromAppSettings(AppData.GetDefaultSettings());
    }


    private void UpdateUIFromAppSettings(AppData appDatas)
    {
        // 데이터 로드.
        float _score = float.Parse(appDatas.Score);

        currentCount = _score;

        //txtPer.text = _score

        //float value = currentCount * 0.005f;
        float value = currentCount * 0.000066f;

        ImageFill.GetComponent<Image>().fillAmount = value;
        //value = 0.2547845f;
        float per = value * 100;
        // per = 0.3624800155f;
        txtPer.text = GetN2(per) + "%";

        //Debug.Log("UpdateUIFromAppSettings      /      " + per + "    /    " + value);

        if (value > 0.1f && value < 0.2f)
        {
            popup[0].alpha = 1;
        }
        else if (value > 0.4f && value < 0.5f)
        {
            popup[0].alpha = 1;
            popup[1].alpha = 1;
        }
        else if (value > 0.6f && value < 0.7f)
        {
            popup[0].alpha = 1;
            popup[1].alpha = 1;
            popup[2].alpha = 1;
        }
        else if (value > 0.8f && value < 0.9f)
        {
            popup[0].alpha = 1;
            popup[1].alpha = 1;
            popup[2].alpha = 1;
            popup[3].alpha = 1;
        }


    }


    public string GetDbFilePath()
    {
        string folder;
        switch (SaveLocationDB)
        {
            case SaveLocationDB.PersistentFolder:
                folder = Application.persistentDataPath;
                break;
            case SaveLocationDB.TempCacheFolder:
                folder = Application.temporaryCachePath;
                break;
            case SaveLocationDB.LocalFolder:
                folder = ".";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        string path = Path.Combine(folder, DatabaseFilename);
        return path;
    }
}


public enum SaveLocationDB
{
    PersistentFolder,
    TempCacheFolder,
    LocalFolder
}

