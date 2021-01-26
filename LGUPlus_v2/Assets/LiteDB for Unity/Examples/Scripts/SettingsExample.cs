using System;
using System.IO;
using LiteDB;
using UnityEngine;
using UnityEngine.UI;

public class SettingsExample : MonoBehaviour
{
    [Tooltip("Determines which folder the app settings database should be saved in for this example.")]
    public SaveLocation SaveLocation = SaveLocation.PersistentFolder;

    [Tooltip("Determines the name of the database file for this example.")]
    public string DatabaseFilename = "SettingsExample.db";

    // UI Controls:
    public InputField CharacterName;
    public Slider MasterVolume;
    public Slider MusicVolume;
    public Slider SoundEffectsVolume;
    public Toggle VoiceChatToggle;
    public Dropdown ResolutionDropdown;
    public Dropdown QualityDropdown;


    private LiteDatabase _db;

    void Start()
    {
        string dbPath = GetDbFilePath();
        Debug.Log("The database file is located at: " + dbPath);

        _db = new LiteDatabase(dbPath);

        LoadSettings();
    }

    private void OnDestroy()
    {
        // LiteDatabase instances should be disposed when they are no longer being used.
        // This will clean up any locks and resources they are using. This can also be
        // done with a "using" block.
        _db.Dispose();
    }

    /// <summary>
    /// Resets the UI to the default settings values. These values are not saved until
    /// SaveSettings() is called.
    /// </summary>
    public void SetToDefaultValues()
    {
        UpdateUIFromAppSettings(AppSettings.GetDefaultSettings());
    }

    /// <summary>
    /// Loads the AppSettings from the database and updates UI controls with them.
    /// If AppSettings have not yet been saved, default values are used instead.
    /// </summary>
    public void LoadSettings()
    {
        LiteCollection<AppSettings> settingsCollection = _db.GetCollection<AppSettings>();
        AppSettings appSettings = settingsCollection.FindOne(Query.All());
        if (appSettings == null)
        {
            appSettings = AppSettings.GetDefaultSettings();
            Debug.Log("Existing settings were not found. Using default settings.");
        }
        else
        {
            Debug.Log("Loading settings from the database.");
        }

        UpdateUIFromAppSettings(appSettings);
    }

    /// <summary>
    /// Saves the values entered on the UI to the database.
    /// </summary>
    public void SaveSettings()
    {
        // Check to see if there is an existing saved AppSettings in the database.
        // If there's not, we'll just create a new one.
        _db.GetCollection<AppSettings>();
        LiteCollection<AppSettings> settingsCollection = _db.GetCollection<AppSettings>();
        AppSettings appSettings = settingsCollection.FindOne(Query.All());

        if (appSettings == null)
        {
            appSettings = AppSettings.GetDefaultSettings();
        }

        // Grab the settings from the UI.
        GetAppSettingsFromUI(appSettings);

        // Save the settings.
        // Note: Upsert() always succeeds. It returns true if a new document was inserted, or false
        // if an existing document was updated.
        if (settingsCollection.Upsert(appSettings))
        {
            Debug.Log("The app settings were successfully inserted.");
        }
        else
        {
            Debug.Log("The existing app settings were updated.");
        }
    }

    /// <summary>
    /// Makes the UI controls display the values contained in appSettings.
    /// </summary>
    private void UpdateUIFromAppSettings(AppSettings appSettings)
    {
        CharacterName.text = appSettings.CharacterName ?? "";
        MasterVolume.value = appSettings.AudioSettings.MasterVolume;
        MusicVolume.value = appSettings.AudioSettings.MusicVolume;
        SoundEffectsVolume.value = appSettings.AudioSettings.SoundEffectsVolume;
        VoiceChatToggle.isOn = appSettings.AudioSettings.IsVoiceChatEnabled;
        ResolutionDropdown.value = GetResolutionDropdownIndex(appSettings.VideoSettings.HorizontalResolution, appSettings.VideoSettings.VerticalResolution);
        QualityDropdown.value = GetQualityDropdownIndex(appSettings.VideoSettings.GraphicsQuality);
    }

    /// <summary>
    /// Updates the appSettings instance with values from the UI controls.
    /// </summary>
    private void GetAppSettingsFromUI(AppSettings appSettings)
    {
        appSettings.CharacterName = CharacterName.text ?? "";
        appSettings.AudioSettings.MasterVolume = MasterVolume.value;
        appSettings.AudioSettings.MusicVolume = MusicVolume.value;
        appSettings.AudioSettings.SoundEffectsVolume = SoundEffectsVolume.value;
        appSettings.AudioSettings.IsVoiceChatEnabled = VoiceChatToggle.isOn;

        int horizontalResolution, verticalResolution;
        GetResolutionFromDropdown(out horizontalResolution, out verticalResolution);
        appSettings.VideoSettings.HorizontalResolution = horizontalResolution;
        appSettings.VideoSettings.VerticalResolution = verticalResolution;

        // The names used in the dropdown are the same as the names used in the enum, so we can use Enum.Parse to determine the value to save.
        appSettings.VideoSettings.GraphicsQuality = (GraphicsQualityOption) Enum.Parse(typeof(GraphicsQualityOption),
            QualityDropdown.options[QualityDropdown.value].text);
    }

    /// <summary>
    /// Given a resolution, this method will determine which option in the dropdown control
    /// it corresponds to and returns the index of the option. The given resolution must
    /// correspond to an option known to exist in the dropdown.
    /// </summary>
    private int GetResolutionDropdownIndex(int horizontalResolution, int verticalResolution)
    {
        string resolutionString = horizontalResolution + " x " + verticalResolution;
        int index = ResolutionDropdown.options.FindIndex(optionData => optionData.text == resolutionString);

        if (index < 0)
        {
            Debug.LogWarning("The specified video resolution could not be found.");
            return 0;
        }
        return index;
    }

    /// <summary>
    /// Gets the horizontal and vertical resolution selected in the dropdown control. This method
    /// assumes the options are formatted as "width x height".
    /// </summary>
    private void GetResolutionFromDropdown(out int horizontalResolution, out int verticalResolution)
    {
        string resolutionString = ResolutionDropdown.options[ResolutionDropdown.value].text;
        // Split and remove the " x " from the resolution option string.
        string[] split = resolutionString.Split(new [] {" x "}, StringSplitOptions.RemoveEmptyEntries);
        horizontalResolution = Int32.Parse(split[0]);
        verticalResolution = Int32.Parse(split[1]);
    }

    /// <summary>
    /// Finds the index of the option in dropdown control that the value of 'quality' corresponds to.
    /// It is assumed that the dropdown control contains a corresponding option.
    /// </summary>
    private int GetQualityDropdownIndex(GraphicsQualityOption quality)
    {
        int index = QualityDropdown.options.FindIndex(optionData => optionData.text == quality.ToString());

        if (index < 0)
        {
            Debug.LogWarning("The specified graphics quality could not be found.");
            return 0;
        }
        return index;
    }

    /// <summary>
    /// Returns the path where the database should be saved based on the values of the fields SaveLocation
    /// and DatabaseFilename. 
    /// </summary>
    public string GetDbFilePath()
    {
        string folder;
        switch (SaveLocation)
        {
            case SaveLocation.PersistentFolder:
                folder = Application.persistentDataPath;
                break;
            case SaveLocation.TempCacheFolder:
                folder = Application.temporaryCachePath;
                break;
            case SaveLocation.LocalFolder:
                folder = ".";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        string path = Path.Combine(folder, DatabaseFilename);
        return path;
    }
}

public enum SaveLocation
{
    PersistentFolder,
    TempCacheFolder,
    LocalFolder
}
