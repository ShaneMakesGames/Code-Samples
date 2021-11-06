using UnityEngine;
using System.IO;
using Steamworks;

public static class SaveSystem
{
    public static void SaveData (string FileName, byte[] data)
    {
        var b64 = System.Convert.ToBase64String(data); // Base 64 Encoded Method

        if (SteamManager.Instance != null && SteamManager.Instance.SteamInitialized)
        {
            SteamRemoteStorage.FileWrite(FileName, data); // Writes file to Steam Cloud
        }

        string path = Path.Combine(Application.persistentDataPath, FileName);
        StreamWriter streamWriter = new StreamWriter(path);
        streamWriter.Write(b64);
        streamWriter.Close();
    }

    public static byte[] LoadData(string FileName)
    {
        System.DateTime localWriteTime = new System.DateTime(), remoteWriteTime = new System.DateTime();
        
        string path = Path.Combine(Application.persistentDataPath, FileName);
        bool localFileExists, remoteFileExists;
        localFileExists = File.Exists(path);
        if (SteamManager.Instance != null && SteamManager.Instance.SteamInitialized)
        {
            remoteFileExists = SteamRemoteStorage.FileExists(FileName);
            if (localFileExists && remoteFileExists)
            {
                localWriteTime = File.GetLastWriteTime(path);
                remoteWriteTime = SteamRemoteStorage.FileTime(FileName);

                if (localWriteTime.ToUniversalTime() > remoteWriteTime.ToUniversalTime()) // Local file is newer
                {
                    StreamReader streamReader = new StreamReader(path);
                    var b64 = streamReader.ReadToEnd();
                    streamReader.Close();
                    //Debug.Log("Local");
                    return System.Convert.FromBase64String(b64);
                }
                else if (remoteWriteTime.ToUniversalTime() > localWriteTime.ToUniversalTime()) // Remote file is newer
                {
                    //Debug.Log("Remote");
                    return SteamRemoteStorage.FileRead(FileName);
                }
                else
                {
                    //Debug.LogError("Time Check Error");
                    return null;
                }
            }
            else if (remoteFileExists)
            {
                //Debug.Log("No Local File Found, but Remote File Found " + FileName);
                return SteamRemoteStorage.FileRead(FileName);
            }
            else if (localFileExists)
            {
                //Debug.Log("No Remote File Found, but Local File at " + path);
                StreamReader streamReader = new StreamReader(path);
                var b64 = streamReader.ReadToEnd();
                streamReader.Close();
                return System.Convert.FromBase64String(b64);
            }
            else
            {
                //Debug.LogError("Save file not found in Steam Cloud or on Disk");
                return null;
            }
        }
        else if (File.Exists(path)) // If Steam is not online, use local files
        {
            // Base 64 Encoded Method
            StreamReader streamReader = new StreamReader(path);
            var b64 = streamReader.ReadToEnd();
            streamReader.Close();
            return System.Convert.FromBase64String(b64);
        }
        else
        {
            //Debug.Log("Steam is offline and no file found on Disk");
            return null;
        }
    }

    public static void DeleteFile(string fileName)
    {
        if (SteamManager.Instance != null && SteamManager.Instance.SteamInitialized)
        {
            if (SteamRemoteStorage.FileExists(fileName))
            {
                SteamRemoteStorage.FileDelete(fileName);
                //Debug.Log("Deleted " + fileName + " from Steam Cloud");
            }
        }

        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            //Debug.Log("Deleted data from " + path);
        }
    }

    public static void DeleteAllFiles()
    {
        DeleteFile("save_data");
        DeleteFile("achievement_data");
        DeleteFile("time_trial_data");
        DeleteFile("endless_data");
    }

}