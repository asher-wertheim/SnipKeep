using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SnipKeep
{
    /// <summary>User settings, stored as JSON in %APPDATA%\SnipKeep\settings.json.</summary>
    [DataContract]
    internal sealed class Settings
    {
        public const string DefaultNamePattern = "Screenshot {yyyy-MM-dd HH.mm.ss}";

        [DataMember(Name = "folder", Order = 1)]
        public string Folder { get; set; }

        [DataMember(Name = "namePattern", Order = 2)]
        public string NamePattern { get; set; }

        [DataMember(Name = "screenshotsOnly", Order = 3)]
        public bool ScreenshotsOnly { get; set; }

        [DataMember(Name = "startWithWindows", Order = 4)]
        public bool StartWithWindows { get; set; }

        [DataMember(Name = "paused", Order = 5)]
        public bool Paused { get; set; }

        /// <summary>Round-trip ISO 8601 time, or empty when the pause has no end.</summary>
        [DataMember(Name = "pausedUntil", Order = 6)]
        public string PausedUntil { get; set; }

        [DataMember(Name = "firstSaveShown", Order = 7)]
        public bool FirstSaveShown { get; set; }

        public static string DefaultFolder
        {
            get
            {
                var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                return Path.Combine(pictures, "Screenshots");
            }
        }

        public static Settings CreateDefault()
        {
            var settings = new Settings();
            settings.ApplyDefaults();
            return settings;
        }

        /// <summary>Fills in values that are missing, for example after an upgrade.</summary>
        private void ApplyDefaults()
        {
            Folder = DefaultFolder;
            NamePattern = DefaultNamePattern;
            ScreenshotsOnly = true;
            StartWithWindows = true;
            Paused = false;
            PausedUntil = "";
            FirstSaveShown = false;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            ApplyDefaults();
        }

        public DateTime? PausedUntilTime
        {
            get
            {
                DateTime value;
                if (!string.IsNullOrEmpty(PausedUntil) &&
                    DateTime.TryParse(PausedUntil, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out value))
                {
                    return value.ToLocalTime();
                }
                return null;
            }
            set
            {
                PausedUntil = value.HasValue
                    ? value.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)
                    : "";
            }
        }

        public static string Serialize(Settings settings)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, false, true, "  "))
                {
                    new DataContractJsonSerializer(typeof(Settings)).WriteObject(writer, settings);
                }
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static Settings Deserialize(string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var settings = (Settings)new DataContractJsonSerializer(typeof(Settings)).ReadObject(stream);
                if (string.IsNullOrWhiteSpace(settings.Folder)) settings.Folder = DefaultFolder;
                if (string.IsNullOrWhiteSpace(settings.NamePattern)) settings.NamePattern = DefaultNamePattern;
                if (settings.PausedUntil == null) settings.PausedUntil = "";
                return settings;
            }
        }
    }

    /// <summary>Reads and writes the settings file.</summary>
    internal sealed class SettingsStore
    {
        private readonly string _path;

        public SettingsStore(string path)
        {
            _path = path;
        }

        public static SettingsStore ForCurrentUser()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return new SettingsStore(Path.Combine(appData, "SnipKeep", "settings.json"));
        }

        public bool Exists
        {
            get { return File.Exists(_path); }
        }

        /// <summary>Loads settings. Returns defaults when the file is missing or damaged.</summary>
        public Settings Load()
        {
            try
            {
                if (File.Exists(_path)) return Settings.Deserialize(File.ReadAllText(_path, Encoding.UTF8));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SerializationException)
            {
                // Fall back to defaults. The file is rewritten on the next save.
            }
            return Settings.CreateDefault();
        }

        public void Save(Settings settings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            var temp = _path + ".tmp";
            File.WriteAllText(temp, Settings.Serialize(settings), new UTF8Encoding(false));
            if (File.Exists(_path)) File.Replace(temp, _path, null);
            else File.Move(temp, _path);
        }
    }
}
