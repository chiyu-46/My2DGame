using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// 此静态类用于实现一个加密文件系统。
/// </summary>
/// <remarks>
/// 按照预想，几乎所有文件操作都在子线程中执行，而Application.persistentDataPath等获取文件路径的属性需要在主线程调用。
/// 所以此类不能在子线程中实现初始化，请在使用前，调用FileSystem.Initialize()方法手动初始化此类。
/// </remarks>
public static class FileSystem
{
    /// <summary>
    /// 作为参数传递到文件处理方法中，用于确定将要使用的文件路径。
    /// </summary>
    public enum UsedFilePath
    {
        /// <summary>
        /// 核心数据。
        /// </summary>
        CoreConfig,
        /// <summary>
        /// 玩家存档。
        /// </summary>
        PlayerArchive
    }
    
    /// <summary>
    /// 所有加密文件共享的AES密钥。
    /// </summary>
    private static byte[] SharedKey;
    /// <summary>
    /// 所有加密文件共享的AES初始化向量。
    /// </summary>
    private static byte[] SharedIV;
    
    /// <summary>
    /// 核心数据文件地址。
    /// </summary>
    private static string CoreConfigPath;
    /// <summary>
    /// 玩家存档文件地址。
    /// </summary>
    private static string PlayerArchivePath;

    /// <summary>
    /// 用于将UsedFilePath枚举转换为实际使用的字符串。
    /// </summary>
    /// <param name="type">UsedFilePath枚举类型。</param>
    /// <returns></returns>
    private static string GetFilePath(UsedFilePath type)
    {
        switch (type)
        {
            case UsedFilePath.CoreConfig:
                return CoreConfigPath;
            case UsedFilePath.PlayerArchive:
                return PlayerArchivePath;
            default:
                return String.Empty;
        }
    }

    #region 初始化文件系统

    /// <summary>
    /// 初始化文件系统。包括设置使用的文件地址，获取通用密钥。在游戏开始时由GameManager调用。
    /// </summary>
    /// <returns>
    /// 有如下返回值：
    /// 0:文件系统初始化正常完成。
    /// -1:密钥文件不存在。
    /// -2:读取密钥失败。
    /// </returns>
    public static int Initialize()
    {
        //初始化文件路径。
        CoreConfigPath = Application.dataPath + "/DATA/CoreConfig";
        PlayerArchivePath = Application.persistentDataPath + "/PlayerArchive.dat";
        //初始化密钥与初始化向量字段。
        SharedKey = new byte[32];
        SharedIV = new byte[16];
        //开始读取密钥。
        if (File.Exists(CoreConfigPath))
        {
            BinaryReader reader = null;
            Stream fileStream = null;
            AesCryptoServiceProvider aesTester = new AesCryptoServiceProvider();
            try
            {
                fileStream = File.Open(CoreConfigPath, FileMode.Open);
                reader = new BinaryReader(fileStream);
                reader.Read(SharedKey, 0, 32);
                reader.Read(SharedIV, 0, 16);
                //测试获取的密钥能否作为密钥使用。即判断获取的密钥是否符合密钥设计规则。
                aesTester.Key = SharedKey;
                aesTester.IV = SharedIV;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                //方法因读取密钥失败而退出。
                return -2;
            }
            finally
            {
                if (!(reader is null))
                {
                    reader.Dispose();
                }
                if (!(fileStream is null))
                {
                    fileStream.Dispose();
                }
                aesTester.Dispose();
            }
        }
        else
        {
            //方法因为密钥文件不存在而退出。
            return -1;
        }

        //方法正常退出。
        return 0;
    }
    
    /// <summary>
    /// 用于获取随机密钥并存入文件。
    /// </summary>
    /// <remarks>
    /// 此方法仅在开发者确定密钥时使用，不能由用户调用。否则，新的密钥会导致所有使用原密钥的文件无法解密。
    /// </remarks>
    public static void InitializeKey()
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(CoreConfigPath, FileMode.Create)))
        {
            using (AesCryptoServiceProvider myAes = new AesCryptoServiceProvider())
            {
                writer.Write(myAes.Key);
                writer.Write(myAes.IV);
            }
        }
    }

    #endregion

    #region 文件操作

    /// <summary>
    /// 使用AES加密算法加密字符串并保存到指定文件。
    /// </summary>
    /// <param name="plainText">原始字符串。</param>
    /// <param name="filetype">目标文件路径类型，对应UsedFilePath枚举。</param>
    /// <exception cref="ArgumentNullException">如果传入参数或者密钥不存在，则抛出空引用异常。</exception>
    public static void EncryptStringToFile_Aes(string plainText, UsedFilePath filetype)
    { 
        string fileName = GetFilePath(filetype);
        // Check arguments.
        if (plainText == null || plainText.Length <= 0) 
            throw new ArgumentNullException("plainText");
        if (SharedKey == null || SharedKey.Length <= 0) 
            throw new ArgumentNullException("Key");
        if (SharedIV == null || SharedIV.Length <= 0) 
            throw new ArgumentNullException("IV");
        if (fileName.Equals(String.Empty)) 
            throw new ArgumentNullException("Path");
        
        // Create an AesCryptoServiceProvider object
        // with the specified key and IV.
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        { 
            aesAlg.Key = SharedKey;
            aesAlg.IV = SharedIV;
    
            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
    
            // Create the streams used for encryption.
            using (FileStream fStream = File.Open(fileName,FileMode.OpenOrCreate))
            { 
                using (CryptoStream csEncrypt = new CryptoStream(fStream, encryptor, CryptoStreamMode.Write))
                { 
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                } 
            }
        }
    }
    
    /// <summary>
    /// 从指定文件加载加密内容并使用AES加密技术解密为字符串。
    /// </summary>
    /// <param name="filetype">原始文件路径类型，对应UsedFilePath枚举。</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">如果传入参数或者密钥不存在，则抛出空引用异常。</exception>
    public static string DecryptStringFromFile_Aes(UsedFilePath filetype)
    {
        string fileName = GetFilePath(filetype);
        // Check arguments.
        if (SharedKey == null || SharedKey.Length <= 0) 
            throw new ArgumentNullException("Key");
        if (SharedIV == null || SharedIV.Length <= 0) 
            throw new ArgumentNullException("IV");
        if (fileName.Equals(String.Empty)) 
            throw new ArgumentNullException("Path");
        
        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;
    
        // Create an AesCryptoServiceProvider object
        // with the specified key and IV.
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        { 
            aesAlg.Key = SharedKey; 
            aesAlg.IV = SharedIV;
    
            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
    
            // Create the streams used for decryption.
            using (FileStream fStream = File.Open(fileName,FileMode.OpenOrCreate))
            { 
                using (CryptoStream csDecrypt = new CryptoStream(fStream, decryptor, CryptoStreamMode.Read))
                { 
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt)) 
                    {
                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    
        return plaintext;
    }    
    
    #endregion
}
