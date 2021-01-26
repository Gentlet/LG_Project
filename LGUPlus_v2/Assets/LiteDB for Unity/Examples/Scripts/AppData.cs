using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppData 
{
    // Start is called before the first frame update

    public int Id { get; set; }
    public string Score { get; set; }
    public string TotalScore { get; set; }


    public static AppData GetDefaultSettings()
    {
        return new AppData
        {
            Score = "",
            TotalScore = "",

        };
        
    }
}

