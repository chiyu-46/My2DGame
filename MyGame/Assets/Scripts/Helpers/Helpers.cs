using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Helper
{
    /// <summary>
    /// 执行异步保存存档的帮助类。
    /// </summary>
    public class SaveHelper
    {
        private PlayerArchive so;
        private FileSystem.UsedFilePath pathType;

        /// <summary>
        /// 异步保存帮助类的构造函数。
        /// </summary>
        /// <param name="so">要保存的存档ScriptableObject。</param>
        /// <param name="pathType">文件保存路径。</param>
        public SaveHelper(PlayerArchive so, FileSystem.UsedFilePath pathType)
        {
            this.so = so;
            this.pathType = pathType;
        }

        /// <summary>
        /// 异步线程执行的方法。
        /// </summary>
        public void ThreadProc()
        {
            SaveSystem.Save(so,pathType);
        }
    }
    
    /// <summary>
    /// 执行异步读取存档的帮助类。
    /// </summary>
    public class LoadHelper
    {
        private PlayerArchive so;
        private FileSystem.UsedFilePath pathType;

        public PlayerArchive So => so;

        /// <summary>
        /// 执行异步读取存档的帮助类的构造函数。
        /// </summary>
        /// <param name="so">要保存的存档ScriptableObject。</param>
        /// <param name="pathType">文件保存路径。</param>
        public LoadHelper(PlayerArchive so, FileSystem.UsedFilePath pathType)
        {
            this.so = so;
            this.pathType = pathType;
        }

        /// <summary>
        /// 异步线程执行的方法。
        /// </summary>
        public void ThreadProc()
        {
            SaveSystem.Load(ref so,pathType);
        }
    }
}

