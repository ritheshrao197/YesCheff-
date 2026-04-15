using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using YesChef.Core;

public static class SaveSystem
{
    private const string SAVE_FILE_NAME = "gameData.rdx";
    private static EncryptionConfig config;
    private static bool useEncryption = false; // set to true to enable encryption

    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    /// <summary>
    /// Enable or disable encryption. If enabled, the EncryptionConfig asset must exist in Resources.
    /// </summary>
    public static bool UseEncryption
    {
        get => useEncryption;
        set
        {
            useEncryption = value;
        }

    }

    public static void Initialise(EncryptionConfig encryptionConfig)
    {
        config = encryptionConfig;
        UseEncryption = true;
    }

    public static bool Save<T>(T data)
    {
        try
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            string finalContent = json;

            if (UseEncryption)
            {
                if (config == null) return false;
                finalContent = Encrypt(json);
            }

            File.WriteAllText(SaveFilePath, finalContent);
            GameLogger.Info(GameLogCategory.Game, $"Game data saved to {SaveFilePath}");
            return true;
        }
        catch (Exception e)
        {
            GameLogger.Error(GameLogCategory.Game, $"Save failed: {e.Message}");
            return false;
        }
    }

    public static T Load<T>(bool deleteOnMismatch = true) where T : new()
    {
        if (!HasSaveFile()) return new T();

        try
        {
            string content = File.ReadAllText(SaveFilePath);
            if (UseEncryption && config != null)
                content = Decrypt(content);

            T data = JsonUtility.FromJson<T>(content);
            if (data == null) throw new Exception("Deserialized object is null.");
            return data;
        }
        catch (Exception e)
        {
            GameLogger.Error(GameLogCategory.Game, $"Load failed: {e.Message}");
            if (deleteOnMismatch)
            {
                GameLogger.Warning(GameLogCategory.Game, "Deleting incompatible save file. A new one will be created.");
                DeleteSave();
            }
            return new T();
        }
    }

    public static bool HasSaveFile() => File.Exists(SaveFilePath);
    public static void DeleteSave() { if (HasSaveFile()) File.Delete(SaveFilePath); }

    // ---------------------- Encryption (using ScriptableObject keys) ----------------------
    private static string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = config.KeyBytes;
            aes.IV = config.IVBytes;
            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(cipherBytes);
        }
    }

    private static string Decrypt(string cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = config.KeyBytes;
            aes.IV = config.IVBytes;
            ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }


}
[System.Serializable]
public class SaveContainer
{
    public int version = 1;
    public int Highscore;
}