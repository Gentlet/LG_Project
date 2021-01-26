// These classes are used in the Settings Example. They define the data schema of the document storing application settings.

public class AppSettings
{
    public int Id { get; set; }
    public string CharacterName { get; set; }
    public AudioSettings AudioSettings { get; set; }
    public VideoSettings VideoSettings { get; set; }

    public static AppSettings GetDefaultSettings()
    {
        return new AppSettings
        {
            CharacterName = "",
            AudioSettings = new AudioSettings
            {
                IsVoiceChatEnabled = true,
                MasterVolume = 100f,
                MusicVolume = 100f,
                SoundEffectsVolume = 100f
            },
            VideoSettings = new VideoSettings
            {
                HorizontalResolution = 1920,
                VerticalResolution = 1080,
                GraphicsQuality = GraphicsQualityOption.Medium
            },
        };
    }
}

public class AudioSettings
{
    public float MasterVolume { get; set; }
    public float MusicVolume { get; set; }
    public float SoundEffectsVolume { get; set; }
    public bool IsVoiceChatEnabled { get; set; }
}

public class VideoSettings
{
    public int HorizontalResolution { get; set; }
    public int VerticalResolution { get; set; }
    public GraphicsQualityOption GraphicsQuality { get; set; }
}

public enum GraphicsQualityOption
{
    Low,
    Medium,
    High
}