using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public BadGuy BadGuyPrefab;
    public Hero Hero;
    public float MinTimeBetweenSpawns = 0.3f;
    public float MaxTimeBetweenSpawns = 10f;
    public float SpawnAreaWidth = 30f;
    public float SpawnAreaLength = 30f;
    public float SpawnHeight = 1f;

    public Text KillCountText;
    public Text DeathCountText;
    public Dropdown SavesDropdown;

    private int _kills = 0;
    private int _deaths = 0;
    private LiteDatabase _db;

    void Start()
    {
        _db = new LiteDatabase(Path.Combine(Application.persistentDataPath, "SaveGameExample.db"));
        var coll = _db.GetCollection<SavedGame>();
        coll.EnsureIndex("SaveTime");

        UpdateSaveDropdown(coll);

        StartCoroutine(SpawnCoroutine(10));
    }

    void OnDestroy()
    {
        _db.Dispose();
    }

    /// <summary>
    /// Creates a new SavedGame state in the database.
    /// </summary>
    public void SaveGame()
    {
        var coll = _db.GetCollection<SavedGame>();
        SavedGame save = CreateSavedGameData();
        coll.Insert(save);

        UpdateSaveDropdown(coll);
    }

    private SavedGame CreateSavedGameData()
    {
        BadGuy[] badGuys = FindObjectsOfType<BadGuy>();
        var badGuyOrientations = from bg in badGuys
                                 select new Orientation
                                 {
                                     Position = bg.transform.position,
                                     Rotation = bg.transform.rotation
                                 };

        var save = new SavedGame
        {
            SaveTime = DateTime.UtcNow,
            KillCount = _kills,
            DeathCount = _deaths,
            Hero = new Orientation { Position = Hero.transform.position, Rotation = Hero.transform.rotation },
            BadGuys = badGuyOrientations.ToList()
        };
        return save;
    }

    /// <summary>
    /// Load the most recent save.
    /// </summary>
    public void LoadLastSave()
    {
        var coll = _db.GetCollection<SavedGame>();
        SavedGame savedGame = coll.FindOne(Query.All("SaveTime", Query.Descending));
        if (savedGame == null)
        {
            Debug.Log("No saved game was found.");
            return;
        }

        LoadGame(savedGame);
    }

    /// <summary>
    /// Load the saved game that has been selected in the dropdown.
    /// </summary>
    public void LoadSelectedSave()
    {
        // Check if any options exist and that something has been selected.
        if (!SavesDropdown.options.Any() || SavesDropdown.value < 0)
            return;

        // Get the selected ID.
        var selectedOption = SavesDropdown.options[SavesDropdown.value] as OptionDataExt;
        int id = (int) selectedOption.Tag;

        // Load the saved game data.
        var coll = _db.GetCollection<SavedGame>();
        SavedGame savedGame = coll.FindById(id);
        if (savedGame == null)
        {
            Debug.LogWarning("The selected saved game could not be found! Id: " + id);
            return;
        }
        
        LoadGame(savedGame);
    }

    private void LoadGame(SavedGame savedGame)
    {
        Debug.Log("Loading saved game from " + savedGame.SaveTime.ToString("g"));

        // Load stats.
        _deaths = savedGame.DeathCount;
        _kills = savedGame.KillCount;
        UpdateCountText();

        // Disable the hero while we load bad guys so that that the hero doesn't interact with any changes.
        Hero.gameObject.SetActive(false);

        // Load bad guys.
        RemoveExistingBadGuys();
        foreach (Orientation badGuy in savedGame.BadGuys)
        {
            Instantiate(BadGuyPrefab, badGuy.Position, badGuy.Rotation);
        }

        // Load hero position.
        Hero.transform.position = savedGame.Hero.Position;
        Hero.transform.rotation = savedGame.Hero.Rotation;
        Hero.gameObject.SetActive(true);
    }

    private void RemoveExistingBadGuys()
    {
        foreach (var badGuy in FindObjectsOfType<BadGuy>())
        {
            Destroy(badGuy.gameObject);
        }
    }

    /// <summary>
    /// Reads save game data from the collection, and uses it to populate the drop down list.
    /// In situations where the amount of data saved for each save game is large, the data
    /// should be split up into separate collections so that all the data would not have to be read
    /// to populate a list.
    /// </summary>
    /// <param name="coll"></param>
    private void UpdateSaveDropdown(LiteCollection<SavedGame> coll)
    {
        // Load the saved game data from the database and use it to create a list of dropdown options.
        List<SavedGame> savedGames = coll.Find(Query.All("SaveTime", Query.Descending), limit: 50).ToList();

        var options = new List<Dropdown.OptionData>(savedGames.Count);
        foreach (var savedGame in savedGames)
        {
            // LiteDB saves DateTimes in UTC, so we want to convert it to the local time to display it to the user.
            string timeText = savedGame.SaveTime.ToLocalTime().ToString("M/d/yy h:mm:ss tt");

            // Our version of OptionDataExt allows us to tag each option in the dropdown with the Id of the SaveGame,
            // so that we know which one is selected.
            options.Add(new OptionDataExt(timeText, savedGame.Id));
        }

        SavesDropdown.ClearOptions();
        SavesDropdown.AddOptions(options);
    }

    IEnumerator SpawnCoroutine(float spawnWaitTime)
    {
        yield return new WaitForSeconds(spawnWaitTime);

        SpawnBadGuyAtRandomLocation();

        // Set the next spawn coroutine.
        float nextWaitTime = Random.Range(MinTimeBetweenSpawns, MaxTimeBetweenSpawns);
        StartCoroutine(SpawnCoroutine(nextWaitTime));
    }

    private void SpawnBadGuyAtRandomLocation()
    {
        Vector3 spawnPos = new Vector3(Random.Range(-SpawnAreaWidth/2f, SpawnAreaWidth/2f), 1, Random.Range(-SpawnAreaLength / 2f, SpawnAreaLength / 2f));
        Instantiate(BadGuyPrefab, spawnPos, Quaternion.identity);
    }

    public void RegisterKill()
    {
        _kills++;
        UpdateCountText();
    }

    public void RegisterDeath()
    {
        _deaths++;
        UpdateCountText();
    }

    private void UpdateCountText()
    {
        KillCountText.text = _kills.ToString();
        DeathCountText.text = _deaths.ToString();
    }
}
