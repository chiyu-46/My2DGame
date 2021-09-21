using System;
using System.Collections;
using System.Collections.Generic;
using FSM;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;


/// <summary>
/// 测试类。
/// </summary>
/// <remarks>
/// 已得结论：
/// 1.一个类实例化一个有事件字段的类，可以将自己的private（私有）方法添加到另一个类的事件。并且正常运行（即使涉及自己的私有字段，
/// 效果和直接在本类调用效果相同。）。
/// 2.事件如果为空，直接调用会引发空引用异常。
/// </remarks>
public class Test : MonoBehaviour
{
    #region 测试事件能否处理私有字段，运行私有方法

        // private TestA testA = new TestA();
        // //private bool sign = false;
        //
        // private void Start()
        // {
        //     //testA.a = BB;
        //     Debug.Log("尝试执行测试方法。");
        //     //testA.a();
        //     if (!(testA.a is null))
        //     {
        //         testA.a();
        //     }
        //     Debug.Log("成功执行测试方法。");
        //     //Debug.Log("当前sign值：" + sign);
        //     
        // }
        //
        // private void BB()
        // {
        //     //sign = true;
        //     Debug.Log("成功执行测试方法。");
        // }

    #endregion

    #region 测试C#作业系统的基础使用

    // 将两个浮点值相加的作业
    // public struct MyJob : IJob
    // {
    //     public float a;
    //     public float b;
    //     public NativeArray<float> result;
    //
    //     public void Execute()
    //     {
    //         result[0] = a + b;
    //     }
    // }
    //
    // // 将一个值加一的作业
    // public struct AddOneJob : IJob
    // {
    //     public NativeArray<float> result;
    //
    //     public void Execute()
    //     {
    //         result[0] = result[0] + 1;
    //     }
    // }
    //
    // private void Start()
    // {
    //     // 创建单个浮点数的本机数组以存储结果。此示例等待作业完成
    //     NativeArray<float> result = new NativeArray<float>(1, Allocator.TempJob);
    //     
    //     // 设置作业 #1 的数据
    //     MyJob jobData = new MyJob();
    //     jobData.a = 10;
    //     jobData.b = 10;
    //     jobData.result = result;
    //     
    //     // 调度作业 #1
    //     JobHandle firstHandle = jobData.Schedule();
    //     
    //     // 设置作业 #2 的数据
    //     AddOneJob incJobData = new AddOneJob();
    //     incJobData.result = result;
    //     
    //     // 调度作业 #2
    //     JobHandle secondHandle = incJobData.Schedule(firstHandle);
    //     
    //     // 等待作业 #2 完成
    //     secondHandle.Complete();
    //     
    //     // NativeArray 的所有副本都指向同一内存，您可以在"您的"NativeArray 副本中访问结果
    //     float aPlusB = result[0];
    //     Debug.Log(aPlusB);
    //     
    //     // 释放由结果数组分配的内存
    //     result.Dispose();
    // }

    #endregion
    
    #region 测试C#作业系统的能否使用静态变量

    // /**
    //  * 可以使用静态变量
    //  * 并且可以使用静态变量访问字符串
    //  */
    // public static int aa = 10;
    // public static string str = "10";
    //
    // // 将两个浮点值相加的作业
    // public struct MyJob : IJob
    // {
    //     public float a;
    //     public float b;
    //     public NativeArray<float> result;
    //
    //     public void Execute()
    //     {
    //         Debug.Log(str);
    //         result[0] = a + b + aa;
    //     }
    // }
    //
    // private void Start()
    // {
    //     // 创建单个浮点数的本机数组以存储结果。此示例等待作业完成
    //     NativeArray<float> result = new NativeArray<float>(1, Allocator.TempJob);
    //     
    //     // 设置作业 #1 的数据
    //     MyJob jobData = new MyJob();
    //     jobData.a = 10;
    //     jobData.b = 10;
    //     jobData.result = result;
    //     
    //     // 调度作业 #1
    //     JobHandle firstHandle = jobData.Schedule();
    //     firstHandle.Complete();
    //     // NativeArray 的所有副本都指向同一内存，您可以在"您的"NativeArray 副本中访问结果
    //     float aPlusB = result[0];
    //     Debug.Log(aPlusB);
    //     
    //     // 释放由结果数组分配的内存
    //     result.Dispose();
    // }

    #endregion
    
    #region 测试作业系统用于文件处理
    
    // public struct MyJob : IJob
    // {
    //     [NativeDisableUnsafePtrRestriction]
    //     public IntPtr add;
    //     public void Execute()
    //     {
    //         
    //         using (BinaryWriter writer = new BinaryWriter(File.Open(Marshal.PtrToStringAnsi(add), FileMode.Create)))
    //         {
    //             writer.Write(1.250F);
    //             writer.Write(@"c:\Temp");
    //             writer.Write(10);
    //             writer.Write(true);
    //         }
    //     }
    // }
    //
    // private void OnEnable()
    // {
    //     string pathname = Application.persistentDataPath + "/AppSettings.dat";
    //     //Debug.Log(pathname);
    //     // GCHandle h = GCHandle.Alloc(pathname, GCHandleType.Pinned);
    //     // IntPtr addr = h.AddrOfPinnedObject();
    //     // Debug.Log(addr);
    //     // MyJob jobData = new MyJob();
    //     // jobData.add = addr;
    //     // JobHandle firstHandle = jobData.Schedule();
    //     // firstHandle.Complete();
    //     //h.Free();
    //     
    //     IntPtr p=Marshal.StringToHGlobalAnsi(pathname);
    //     string s=Marshal.PtrToStringAnsi(p);
    //     Debug.Log(s);
    //     MyJob jobData = new MyJob();
    //     jobData.add = p;
    //     JobHandle firstHandle = jobData.Schedule();
    //     firstHandle.Complete();
    //     Marshal.FreeHGlobal(p);
    // }

    #endregion

    #region 测试ReadBytes

    //结论：与Read（byte[],int,int）相同
    // private void OnEnable()
    // {
    //     //密钥文件地址。
    //     string keyPath = Application.persistentDataPath + "/AppSettings.dat";
    //
    //     //加密密钥与IV
    //     byte[] key = new byte[32];
    //     byte[] IV = new byte[16];
    //     
    //     //读取Key与IV，从文件。
    //     if (File.Exists(keyPath))
    //     {
    //         using (BinaryReader reader = new BinaryReader(File.Open(keyPath, FileMode.Open)))
    //         {
    //             key = reader.ReadBytes(32);
    //             IV = reader.ReadBytes(16);
    //             
    //             // reader.Read(key,0,32);
    //             // reader.Read(IV,0,16);
    //         }
    //         Debug.Log("-----------------------------------");
    //         foreach (var b in key)
    //         {
    //             Debug.Log(b);
    //         }
    //         Debug.Log("-----------------------------------");
    //         foreach (var b in IV)
    //         {
    //             Debug.Log(b);
    //         }
    //     }
    //}

    #endregion
    
    #region 测试加密文件处理
    
    // static void EncryptStringToFile_Aes(String FileName,string plainText, byte[] Key, byte[] IV)
    //     {
    //         // Check arguments.
    //         if (plainText == null || plainText.Length <= 0)
    //             throw new ArgumentNullException("plainText");
    //         if (Key == null || Key.Length <= 0)
    //             throw new ArgumentNullException("Key");
    //         if (IV == null || IV.Length <= 0)
    //             throw new ArgumentNullException("IV");
    //         byte[] encrypted;
    //
    //         // Create an AesCryptoServiceProvider object
    //         // with the specified key and IV.
    //         using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
    //         {
    //             aesAlg.Key = Key;
    //             aesAlg.IV = IV;
    //
    //             // Create an encryptor to perform the stream transform.
    //             ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
    //
    //             // Create the streams used for encryption.
    //             using (FileStream fStream = File.Open(FileName,FileMode.OpenOrCreate))
    //             {
    //                 using (CryptoStream csEncrypt = new CryptoStream(fStream, encryptor, CryptoStreamMode.Write))
    //                 {
    //                     using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
    //                     {
    //                         //Write all data to the stream.
    //                         swEncrypt.Write(plainText);
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //
    // static string DecryptStringFromFile_Aes(String FileName, byte[] Key, byte[] IV)
    //     {
    //         // Check arguments.
    //         if (Key == null || Key.Length <= 0)
    //             throw new ArgumentNullException("Key");
    //         if (IV == null || IV.Length <= 0)
    //             throw new ArgumentNullException("IV");
    //
    //         // Declare the string used to hold
    //         // the decrypted text.
    //         string plaintext = null;
    //
    //         // Create an AesCryptoServiceProvider object
    //         // with the specified key and IV.
    //         using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
    //         {
    //             aesAlg.Key = Key;
    //             aesAlg.IV = IV;
    //
    //             // Create a decryptor to perform the stream transform.
    //             ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
    //
    //             // Create the streams used for decryption.
    //             using (FileStream fStream = File.Open(FileName,FileMode.OpenOrCreate))
    //             {
    //                 using (CryptoStream csDecrypt = new CryptoStream(fStream, decryptor, CryptoStreamMode.Read))
    //                 {
    //                     using (StreamReader srDecrypt = new StreamReader(csDecrypt))
    //                     {
    //
    //                         // Read the decrypted bytes from the decrypting stream
    //                         // and place them in a string.
    //                         plaintext = srDecrypt.ReadToEnd();
    //                     }
    //                 }
    //             }
    //         }
    //
    //         return plaintext;
    //     }
    //     
    // void OnEnable()
    // {
    //     //数据文件地址与密钥文件地址。
    //     string pathname = Application.persistentDataPath + "/A.dat";
    //     string keyPath = Application.persistentDataPath + "/AppSettings.dat";
    //
    //     //待加密数据。
    //     string original = "Here is some data to encrypt!";
    //
    //     //加密密钥与IV
    //     byte[] key = new byte[32];
    //     byte[] IV = new byte[16];
    //     
    //     //读取Key与IV，从文件。
    //     if (File.Exists(keyPath))
    //     {
    //         using (BinaryReader reader = new BinaryReader(File.Open(keyPath, FileMode.Open)))
    //         {
    //             // key = reader.ReadBytes(32);
    //             // IV = reader.ReadBytes(16);
    //             
    //             reader.Read(key,0,32);
    //             reader.Read(IV,0,16);
    //         }
    //         // Debug.Log("-----------------------------------");
    //         // foreach (var b in key)
    //         // {
    //         //     Debug.Log(b);
    //         // }
    //         // Debug.Log("-----------------------------------");
    //         // foreach (var b in IV)
    //         // {
    //         //     Debug.Log(b);
    //         // }
    //     }
    //     
    //     
    //     // Debug.Log("-----------------------------------");
    //     
    //     // Create a new instance of the AesCryptoServiceProvider
    //     // class.  This generates a new key and initialization
    //     // vector (IV).
    //     using (AesCryptoServiceProvider myAes = new AesCryptoServiceProvider())
    //     {
    //         // foreach (var b in myAes.Key)
    //         // {
    //         //     Debug.Log(b);
    //         // }
    //         // foreach (var b in myAes.IV)
    //         // {
    //         //     Debug.Log(b);
    //         // }
    //         
    //         
    //         myAes.Key = key;
    //         myAes.IV = IV;
    //         
    //         
    //         // Encrypt the string to an array of bytes.
    //         // EncryptStringToFile_Aes(pathname,original, myAes.Key, myAes.IV);
    //         // Decrypt the bytes to a string.
    //         string roundtrip = DecryptStringFromFile_Aes(pathname, myAes.Key, myAes.IV);
    //         
    //         
    //         //保存Key与IV到文件。
    //         // using (BinaryWriter writer = new BinaryWriter(File.Open(keyPath, FileMode.Create)))
    //         // {
    //         //     writer.Write(myAes.Key);
    //         //     writer.Write(myAes.IV);
    //         // }
    //         
    //         
    //         // Debug.Log(myAes.Key.Length);
    //         // Debug.Log(myAes.KeySize);
    //         // Debug.Log(myAes.IV.Length);
    //         Debug.Log(roundtrip);
    //     
    //     }
    // }

    #endregion

    #region 测试作业系统用于加密文件处理
    //
    // /**
    //  * ExecutionEngineException: String conversion error: Illegal byte sequence encounted in the input.
    //  * 如果出现这种错误，如果在使用指针，请检查是否过早释放了指针，导致使用指针时，指针实际指向的值已经改变了。
    // */
    //
    
    // static void EncryptStringToFile_Aes(String FileName,string plainText, byte[] Key, byte[] IV)
    //     {
    //         // Check arguments.
    //         if (plainText == null || plainText.Length <= 0)
    //             throw new ArgumentNullException("plainText");
    //         if (Key == null || Key.Length <= 0)
    //             throw new ArgumentNullException("Key");
    //         if (IV == null || IV.Length <= 0)
    //             throw new ArgumentNullException("IV");
    //         byte[] encrypted;
    //
    //         // Create an AesCryptoServiceProvider object
    //         // with the specified key and IV.
    //         using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
    //         {
    //             aesAlg.Key = Key;
    //             aesAlg.IV = IV;
    //
    //             // Create an encryptor to perform the stream transform.
    //             ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
    //
    //             // Create the streams used for encryption.
    //             using (FileStream fStream = File.Open(FileName,FileMode.OpenOrCreate))
    //             {
    //                 using (CryptoStream csEncrypt = new CryptoStream(fStream, encryptor, CryptoStreamMode.Write))
    //                 {
    //                     using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
    //                     {
    //                         //Write all data to the stream.
    //                         swEncrypt.Write(plainText);
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //
    // static string DecryptStringFromFile_Aes(String FileName, byte[] Key, byte[] IV)
    //     {
    //         // Check arguments.
    //         if (Key == null || Key.Length <= 0)
    //             throw new ArgumentNullException("Key");
    //         if (IV == null || IV.Length <= 0)
    //             throw new ArgumentNullException("IV");
    //
    //         // Declare the string used to hold
    //         // the decrypted text.
    //         string plaintext = null;
    //
    //         // Create an AesCryptoServiceProvider object
    //         // with the specified key and IV.
    //         using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
    //         {
    //             aesAlg.Key = Key;
    //             aesAlg.IV = IV;
    //
    //             // Create a decryptor to perform the stream transform.
    //             ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
    //
    //             // Create the streams used for decryption.
    //             using (FileStream fStream = File.Open(FileName,FileMode.OpenOrCreate))
    //             {
    //                 using (CryptoStream csDecrypt = new CryptoStream(fStream, decryptor, CryptoStreamMode.Read))
    //                 {
    //                     using (StreamReader srDecrypt = new StreamReader(csDecrypt))
    //                     {
    //
    //                         // Read the decrypted bytes from the decrypting stream
    //                         // and place them in a string.
    //                         plaintext = srDecrypt.ReadToEnd();
    //                     }
    //                 }
    //             }
    //         }
    //
    //         return plaintext;
    //     }
    //
    // public struct ReadKeyAndIV : IJob
    // {
    //     //加密密钥与IV
    //     public NativeArray<byte> NativeArrayKey;
    //     public NativeArray<byte> NativeArrayIV;
    //     [NativeDisableUnsafePtrRestriction]
    //     public IntPtr addr;
    //     public void Execute()
    //     {
    //         Debug.Log(addr);
    //         Debug.Log(Marshal.PtrToStringAnsi(addr));
    //         byte[] key = new byte[32];
    //         byte[] IV = new byte[16];
    //         //读取Key与IV，从文件。
    //         if (File.Exists(Marshal.PtrToStringAnsi(addr)))
    //         {
    //             using (BinaryReader reader = new BinaryReader(File.Open(Marshal.PtrToStringAnsi(addr), FileMode.Open)))
    //             {
    //                 reader.Read(key,0,32);
    //                 reader.Read(IV,0,16);
    //             }
    //         }
    //         for (int i = 0; i < 32; i++)
    //         {
    //             NativeArrayKey[i] = key[i];
    //         }
    //         for (int i = 0; i < 16; i++)
    //         {
    //             NativeArrayIV[i] = IV[i];
    //         }
    //         Debug.Log(Marshal.PtrToStringAnsi(addr));
    //         Debug.Log(key[0]);
    //         Debug.Log(NativeArrayKey[0]);
    //     }
    // }
    // public struct DecryptStringFromFile : IJob
    // {
    //     //加密密钥与IV
    //     public NativeArray<byte> NativeArrayKey;
    //     public NativeArray<byte> NativeArrayIV;
    //     public NativeArray<IntPtr> NativeArrayDecryptString;
    //     [NativeDisableUnsafePtrRestriction]
    //     public IntPtr addr;
    //     
    //     public void Execute()
    //     {
    //         byte[] key = new byte[32];
    //         byte[] IV = new byte[16];
    //         
    //         for (int i = 0; i < 32; i++)
    //         {
    //             key[i] = NativeArrayKey[i];
    //         }
    //         for (int i = 0; i < 16; i++)
    //         {
    //             IV[i] = NativeArrayIV[i];
    //         }
    //
    //         string s = DecryptStringFromFile_Aes(Marshal.PtrToStringAnsi(addr), key, IV);
    //         NativeArrayDecryptString[0] = Marshal.StringToHGlobalAnsi(s);
    //     }
    // }
    //
    // void OnEnable()
    // {
    //     //数据文件地址与密钥文件地址。
    //     string pathname = Application.persistentDataPath + "/A.dat";
    //     string keyPath = Application.persistentDataPath + "/AppSettings.dat";
    //
    //     //待加密数据。
    //     string original = "Here is some data to encrypt!";
    //
    //     //加密密钥与IV
    //     // byte[] key = new byte[32];
    //     // byte[] IV = new byte[16];
    //
    //     //使用工作系统获得密钥与IV。
    //     IntPtr p=Marshal.StringToHGlobalAnsi(keyPath);
    //     // string str=Marshal.PtrToStringAnsi(p);
    //     // Debug.Log(p);
    //     // Debug.Log(str);
    //     ReadKeyAndIV job1 = new ReadKeyAndIV();
    //     NativeArray<byte> NativeArrayKey = new NativeArray<byte>(32, Allocator.Persistent);
    //     NativeArray<byte> NativeArrayIV = new NativeArray<byte>(16, Allocator.Persistent);
    //     job1.NativeArrayKey = NativeArrayKey;
    //     job1.NativeArrayIV = NativeArrayIV;
    //     job1.addr = p;
    //     JobHandle jobHandle1 = job1.Schedule();
    //     // jobHandle1.Complete();
    //     
    //
    //     // foreach (var item in NativeArrayKey)
    //     // {
    //     //     Debug.Log(item);
    //     // }
    //     // Debug.Log("-------------------------------------------------------");
    //     // foreach (var item in NativeArrayIV)
    //     // {
    //     //     Debug.Log(item);
    //     // }
    //     
    //     IntPtr a=Marshal.StringToHGlobalAnsi(pathname);
    //     DecryptStringFromFile job2 = new DecryptStringFromFile();
    //     NativeArray<IntPtr> NativeArrayDecryptString = new NativeArray<IntPtr>(1, Allocator.Persistent);
    //     job2.NativeArrayKey = NativeArrayKey;
    //     job2.NativeArrayIV = NativeArrayIV;
    //     job2.NativeArrayDecryptString = NativeArrayDecryptString;
    //     job2.addr = a;
    //     JobHandle jobHandle2 = job2.Schedule(jobHandle1);
    //     jobHandle2.Complete();
    //     Marshal.FreeHGlobal(a);
    //     string s=Marshal.PtrToStringAnsi(job2.NativeArrayDecryptString[0]);
    //     Marshal.FreeHGlobal(job2.NativeArrayDecryptString[0]);
    //     Debug.Log("-------------------------------------------------------");
    //     Debug.Log(s);
    //     string str=Marshal.PtrToStringAnsi(p);
    //     Debug.Log(str);
    //     Marshal.FreeHGlobal(p);
    //     NativeArrayKey.Dispose();
    //     NativeArrayIV.Dispose();
    //     NativeArrayDecryptString.Dispose();
    // }

    #endregion

    #region 测试不同文件路径

    // //注意：：：：：get_dataPath不允许从MonoBehaviour构造函数（或实例字段初始化器）中调用
    // // private string s1 = Application.dataPath + "/AppSettings.dat";
    // // private string s2 = Application.streamingAssetsPath + "/AppSettings.dat";
    // // private string s3 = Application.persistentDataPath + "/AppSettings.dat";
    // // private string s4 = Application.temporaryCachePath + "/AppSettings.dat";
    // private void Start()
    // {
    //     string s1 = Application.dataPath + "/AppSettings.dat";
    //     // string s2 = Application.streamingAssetsPath + "/AppSettings.dat";
    //     // string s3 = Application.persistentDataPath + "/AppSettings.dat";
    //     // string s4 = Application.temporaryCachePath + "/AppSettings.dat";
    //     string path = s1;
    //     Debug.Log(path);
    //     using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
    //     {
    //         writer.Write(1.250F);
    //         writer.Write(@"c:\Temp");
    //         writer.Write(10);
    //         writer.Write(true);
    //     }
    // }

    #endregion

    #region 测试C#原生多线程

    // private static int GetReturn()
    // {
    //     Thread.Sleep(5000);
    //     return 1;
    // }
    //
    // private void Start()
    // {
    //     Debug.Log("start");
    //     // var t = Task<int>.Run( () => {
    //     //     // Just loop.
    //     //     Thread.Sleep(5000);
    //     //     return 1;
    //     // } );
    //     // Debug.Log(t.Result);
    //
    //     Task<int> t = Task<int>.Run(GetReturn); 
    //     StartCoroutine(WaitForThread(t));
    // }
    //
    // IEnumerator WaitForThread(Task<int> task)
    // {
    //     while (true)
    //     {
    //         Debug.Log(task.Status);
    //         if (task.Status == TaskStatus.RanToCompletion)
    //         {
    //             Debug.Log(task.Result);
    //             yield break;
    //         }
    //
    //         yield return null;
    //     }
    // }

    
    //测试debug.log()能否在子线程访问。
    //结果：可以。
    
    
    #endregion

    #region 原生多线程的堆栈追踪、异常捕获

    // /**
    //  * 可以通过try-catch捕获产生的异常，如果不再抛出原异常（throw;），将正常执行异常后面的代码，完成后线程状态为RanToCompletion。
    //  * 选择抛出原异常（throw;），catch后的代码不执行，线程状态为faulted。
    //  * 可以使用Debug.Log()在控制台进行堆栈追踪。
    //  */
    // private void Start()
    // {
    //     Debug.Log("start");
    //     var t = Task<int>.Run( () => {
    //         // Just loop.
    //         Debug.Log("在子线程显示消息。");
    //         try
    //         {
    //             Debug.Log(Application.persistentDataPath);
    //         }
    //         catch (Exception e)
    //         {
    //             Debug.Log(e);
    //             throw;
    //         }
    //         Debug.Log("在子线程末尾显示消息。");
    //         return 1;
    //     } );
    //     StartCoroutine(WaitForThread(t));
    // }
    // IEnumerator WaitForThread(Task<int> task)
    // {
    //     while (true)
    //     {
    //         Debug.Log(task.Status);
    //         if (task.IsCompleted)
    //         {
    //             Debug.Log(task.Status);
    //             yield break;
    //         }
    //
    //         yield return null;
    //     }
    // }

    #endregion

    #region 测试二进制文件存取

    // private const string FILENAME = "/AppSettings.dat";
    //
    // public static void WriteDefaultValues()
    // {
    //     string path = Application.persistentDataPath + "/AppSettings.dat";
    //     using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
    //     {
    //         writer.Write(1.250F);
    //         writer.Write(@"c:\Temp");
    //         writer.Write(10);
    //         writer.Write(true);
    //     }
    // }
    //
    // public static void DisplayValues()
    // {
    //     float aspectRatio;
    //     string tempDirectory;
    //     int autoSaveTime;
    //     bool showStatusBar;
    //
    //     string path = Application.persistentDataPath + FILENAME;
    //     if (File.Exists(path))
    //     {
    //         using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
    //         {
    //             aspectRatio = reader.ReadSingle();
    //             tempDirectory = reader.ReadString();
    //             autoSaveTime = reader.ReadInt32();
    //             showStatusBar = reader.ReadBoolean();
    //         }
    //
    //         Debug.Log("Aspect ratio set to: " + aspectRatio);
    //         Debug.Log("Temp directory is: " + tempDirectory);
    //         Debug.Log("Auto save time set to: " + autoSaveTime);
    //         Debug.Log("Show status bar: " + showStatusBar);
    //     }
    // }

    #endregion
}
