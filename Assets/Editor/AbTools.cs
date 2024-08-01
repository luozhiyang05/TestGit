using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// 继承EditorWindow表示自身为窗口类，在获取窗口后，OnGUI才回开始工作
    /// </summary>
    public class AbTools : EditorWindow
    {
        private static int _selectIndex = 0;
        private static string[] _plasticsName = new[] { "PC", "IOS", "Android" };
        private static string _serverIp = "ftp://127.0.0.1";

        [MenuItem("AB包工具/打开ab工具窗口")]
        public static void OpenWindow()
        {
            //返回当前屏幕上第一个 t 类型的 EditorWindow，utility参数为是否浮动窗口
            AbTools abTools = GetWindowWithRect<AbTools>(new Rect(0, 0, 360, 190), false, "Ab工具窗口");
            abTools.Show(); //默认打开
        }

        //IMGUI即GUI只能再OnGUi生命周期函数中每一帧执行，在鼠标指针移动时执行
        //在LateUpdate之后，OnDisable之前
        private void OnGUI()
        {
            //生成Label，x，y基于窗口左上角开始偏移
            GUI.Label(new Rect(10, 22.5f, 100, 20), "平台选择");

            //创建平台选择的工具栏，返回的是当前选择的按钮下标
            _selectIndex = GUI.Toolbar(new Rect(100, 20, 200, 25), _selectIndex, _plasticsName);

            //创建输入框
            //输入资源服务器IP
            GUI.Label(new Rect(10, 60, 100, 20), "资源服务器IP");
            _serverIp = GUI.TextField(new Rect(100, 60, 200, 20), _serverIp);

            //创建按键，当按钮按下时返回true，否则返回false
            //创建对比文件
            if (GUI.Button(new Rect(10, 100, 120, 30), "创建对比文件"))
                CreateAbCompareFile();

            //保存默认资源到StreamingAsset
            if (GUI.Button(new Rect(150, 100, 200, 30), "保存默认资源到StreamingAsset"))
                MoveAssetBundleToStreamingAssets();

            //上传Ab包和对比文件
            if (GUI.Button(new Rect(10, 150, 340, 30), "上传Ab包和对比文件"))
                UploadAbFile();
        }

        private static void CreateAbCompareFile()
        {
            //1:找到Ab包路径，在指定路径中创建所有目录和子目录，除非它们已经存在，则返回该目录
            DirectoryInfo directoryInfo =
                Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/" + _plasticsName[_selectIndex] + "/");

            //2：遍历路径下所有AB包，封装信息
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            StringBuilder sb = new StringBuilder();
            foreach (var fileInfo in fileInfos)
            {
                Debug.LogWarning(fileInfo);
                //无后缀名的是ab包
                if ("".Equals(fileInfo.Extension))
                {
                    Debug.LogWarning(fileInfo.Name);
                    sb.Append(fileInfo.Name + "|" + fileInfo.Length + "|" + GetMD5(fileInfo.FullName) + "\n");
                }
            }

            if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);

            //3:写入对比文件
            //创建一个新文件，向其中写入内容，然后关闭文件。 如果目标文件已存在，则会将其截断并覆盖。
            File.WriteAllText(directoryInfo.FullName + "/AbCompare.txt", sb.ToString());

            //4:刷新编辑器
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Md5码，16字节（128位）散列值，用来确保文件的一致性。文件修改，md5码也会修改，使用MD5类和MD5CryptoServiceProvider生成类
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static string GetMD5(string filePath)
        {
            //1：以流的方式打开文件

            using (FileStream file = new FileStream(filePath, FileMode.Open))
            {
                //2：获取文件的MD5码
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(file);

                //3：将Md5码转为十六进制字符串存储（占用内存小）
                StringBuilder sb = new StringBuilder();
                for (var i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// 移动默认资源到Sa路径
        /// </summary>
        private static void MoveAssetBundleToStreamingAssets()
        {
            //1:选中编辑器选中的ab
            //参数1是类型，使用编辑器的Object，参数2是模式
            //Asset ：仅返回选择的资源对象。DeepAsset：如果包含文件夹，则包含所有资源和子目录
            var objects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            if (objects.Length == 0) return;

            //2:将ab复制到sa
            StringBuilder sb = new StringBuilder();
            foreach (var obj in objects)
            {
                //获取资源在项目中1路径，从根目录往下找，包含了文件夹，所以资源名不能和文件夹名字一致
                var assetPath = AssetDatabase.GetAssetPath(obj);
                var assetName = assetPath[assetPath.LastIndexOf('/')..];

                //获取资源的所有信息，排除非ab包资源和文件夹
                FileInfo fileInfo = new FileInfo(assetPath);
                if (!"".Equals(fileInfo.Extension) || Directory.Exists(assetPath)) continue;
                Debug.LogWarning("文件" + assetName + "已设置为默认资源");

                //复制资源，参数1是待复制的资源路径，参数2是新路径，都是相对于项目文件夹的路径
                AssetDatabase.CopyAsset(assetPath, "Assets/StreamingAssets" + assetName);

                //拼接ab包信息
                sb.Append(fileInfo.Name + "|" + fileInfo.Length + "|" + GetMD5(fileInfo.FullName) + "\n");
            }

            //3:生成默认资源的ab对比文件
            File.WriteAllText(Application.streamingAssetsPath + "/AbCompare.txt", sb.ToString());

            //刷新编辑器
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 上传ab包到服务器
        /// </summary>
        private static void UploadAbFile()
        {
            //1：找到ab包目录
            DirectoryInfo directoryInfo =
                Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/" + _plasticsName[_selectIndex] + "/");
            //2:遍历目录的ab包文件
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                if ("".Equals(fileInfo.Extension) ||
                    ".txt".Equals(fileInfo.Extension))
                {
                    //3:将ab包和对比文件上传
                    FtpUploadFile(fileInfo.FullName, fileInfo.Name);
                }
            }
        }

        /// <summary>
        /// 通过ftp流上传ab包和对比文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <exception cref="Exception"></exception>
        private static void FtpUploadFile(string filePath, string fileName)
        {
            FtpWebRequest ftp;
            try
            {
                //1：创建ftp链接，没有远端ftp服务器，就填本机ip地址
                ftp = FtpWebRequest.Create(new Uri(_serverIp + "/AB/" + _plasticsName[_selectIndex] + "/" + fileName))
                    as
                    FtpWebRequest;
                // 设置通讯凭证
                NetworkCredential networkCredential = new NetworkCredential("MrLuo", "luo123");
                if (ftp != null)
                {
                    ftp.Credentials = networkCredential;
                    // 代理为空
                    ftp.Proxy = null;
                    // 请求链接后是否关闭链接
                    ftp.KeepAlive = false;
                    // 操作命令，上传
                    ftp.Method = WebRequestMethods.Ftp.UploadFile;
                    // 指定传输类型 二进制
                    ftp.UseBinary = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("创建ftp链接失败：" + ex.Message);
            }

            try
            {
                //2:获取ftp流对象
                Stream upLoadStream = ftp.GetRequestStream();

                //2:打开文件流
                using (FileStream file = new FileStream(filePath, FileMode.Open))
                {
                    //3:通过文件流方式，2kb地读进ftp流
                    byte[] bytes = new byte[2048];
                    int length = file.Read(bytes, 0, bytes.Length);

                    //循环读取
                    while (length != 0)
                    {
                        //写入ftp流
                        upLoadStream.Write(bytes, 0, length);
                        length = file.Read(bytes, 0, bytes.Length);
                    }

                    //关闭流
                    upLoadStream.Close();
                    file.Close();

                    Debug.LogWarning("上传成功 :" + fileName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("文件上传失败：" + ex.Message);
            }
        }
    }
}