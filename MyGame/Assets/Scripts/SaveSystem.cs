using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static void Save(PlayerArchive so,FileSystem.UsedFilePath pathType)
    {
        string json = JsonUtility.ToJson(so);
        FileSystem.EncryptStringToFile_Aes(json,pathType);
    }

    public static void Load(ref PlayerArchive so,FileSystem.UsedFilePath pathType)
    {
        string json = FileSystem.DecryptStringFromFile_Aes(pathType);
        JsonUtility.FromJsonOverwrite(json,so);
    }
}
