using UnityEngine;
using System.IO;

// ذخیره و بارگذاری وضعیت بازی رو یه فایل JSON روی خودِ دستگاه (Application.persistentDataPath)
public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    public static bool HasSave() => File.Exists(SavePath);

    public static void Save(SaveData data)
    {
        File.WriteAllText(SavePath, JsonUtility.ToJson(data));
    }

    public static SaveData Load()
    {
        if (!HasSave()) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
    }

    public static void DeleteSave()
    {
        if (HasSave()) File.Delete(SavePath);
    }
}
