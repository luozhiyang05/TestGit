using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Tool.Single;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Tool.AB
{
    public class AbUpdateMgr : MonoSingleton<AbUpdateMgr>
    {
        /// <summary>
        /// 远程ab包信息字典
        /// </summary>
        private Dictionary<string, AbInfo> _remoteAbInfoDic = new Dictionary<string, AbInfo>();

        /// <summary>
        /// 本地ab包信息字典
        /// </summary>
        private Dictionary<string, AbInfo> _localAbInfoDic = new Dictionary<string, AbInfo>();

        /// <summary>
        /// 待更新ab包名字列表
        /// </summary>
        private List<string> _updateAbList = new List<string>();

        /// <summary>
        /// 检查ab包更新
        /// </summary>
        /// <param name="overCallBack"></param>
        public void CheckUpdate(UnityAction<bool> overCallBack)
        {
            _remoteAbInfoDic.Clear();
            _localAbInfoDic.Clear();
            _updateAbList.Clear();

            //1：获取远端ab包对比文件
            //下载ab包对比文件
            DownloadRemoteAbCompareFile(isFinish =>
            {
                if (isFinish)
                {
                    Debug.LogWarning("远端ab包对比文件下载成功");

                    //获取新的ab包对比文件信息,
                    var removeAbCompareInfo = File.ReadAllLines(Application.persistentDataPath + "/" + "AbCompare_TMP.txt");

                    //写入远端ab包信息字典
                    GetAbCompareFileInfo(removeAbCompareInfo, _remoteAbInfoDic);
                    Debug.LogWarning("远端ab包信息获取完毕");

                    //2：获取本地ab包对比文件
                    GetLocalAbCompareFile(isFinish =>
                    {
                        if (isFinish)
                        {
                            Debug.LogWarning("本地ab包信息获取完毕");

                            //3：对比远端和本地的ab包对比文件
                            CompareAb();

                            //4: 下载更新和删除ab
                            DownloadAbFile(isFinish =>
                            {
                                if (isFinish)
                                {
                                    //新的ab对比文件覆盖旧的
                                    File.WriteAllLines(Application.persistentDataPath + "/AbCompare.txt", removeAbCompareInfo);
                                }

                                overCallBack.Invoke(isFinish);
                            }, Debug.LogWarning);
                        }
                        else
                        {
                            overCallBack.Invoke(false);
                        }
                    });
                }
                else
                {
                    Debug.LogWarning("找不到远端ab对比文件");
                    overCallBack.Invoke(false);
                }
            });
        }

        /// <summary>
        /// 对比远端和本地ab包信息字典
        /// </summary>
        private void CompareAb()
        {
            //遍历远端ab包信息字典
            foreach (var abName in _remoteAbInfoDic.Keys)
            {
                //本地中没有目标ab，则添加到更新队列
                if (!_localAbInfoDic.TryGetValue(abName, out var value))
                    _updateAbList.Add(abName);
                //本地中有目标ab，则对比md5码
                else
                {
                    //md5码不正确，则加入更新队列
                    if (!value.Equals(_remoteAbInfoDic[abName]))
                        _updateAbList.Add(abName);
                    _localAbInfoDic.Remove(abName); //移除出本地ab包信息字典
                }
            }

            //本地ab包信息字典中剩下的就是需要删除的ab包
            foreach (var abName in _localAbInfoDic.Keys.Where(abName =>
                         File.Exists(Application.persistentDataPath + "/" + abName)))
            {
                File.Delete(Application.persistentDataPath + "/" + abName);
            }
        }

        /// <summary>
        /// 下载远端ab包对比文件
        /// </summary>
        /// <param name="overCallBack"></param>
        private void DownloadRemoteAbCompareFile(Action<bool> overCallBack = null)
        {
            bool isFinishDownload = false;
            int maxDownloadNum = 5;

            //避免网络波动导致下载对比文件失败
            while (!isFinishDownload && maxDownloadNum != 0)
            {
                //下载到临时文件，为了避免网络波动重新下载时，新的对比文件覆盖了旧的对比文件
                isFinishDownload =
                    FtpDownloadFIle(Application.persistentDataPath + "/" + "AbCompare_TMP.txt", "AbCompare.txt");
                maxDownloadNum--;
            }

            //回调函数
            overCallBack?.Invoke(isFinishDownload);
        }

        /// <summary>
        /// 获取本地ab包对比文件
        /// </summary>
        /// <param name="overCallBack"></param>
        private void GetLocalAbCompareFile(UnityAction<bool> overCallBack)
        {
            //使用UnityWebRequest读取文件，ios，pc，android在读取pd时需要加file:///提示是用文件流，读取sa时android不需要加,android读取时默认会有jar:///前缀

            //先检查pd路径，如果不存在则表示第一次打开游戏，去检查sa路径的默认资源。如果存在，则表示不是第一次打开，默认资源已经更新好，去找pd路径的资源
            if (File.Exists(Application.persistentDataPath + "/" + "AbCompare.txt"))
            {
                StartCoroutine(GetLocalAbCompareFile("file:///" + Application.persistentDataPath + "/" + "AbCompare.txt",
                    overCallBack));
            }
            //如果有默认资源，则获取默认资源
            else if (File.Exists(Application.streamingAssetsPath + "/" + "AbCompare.txt"))
            {
                string path =
#if UNITY_ANDROID
            Application.streamingAssetsPath;
#else
                    "file:///" + Application.streamingAssetsPath;
#endif
                StartCoroutine(GetLocalAbCompareFile(path + "/" + "AbCompare.txt",
                    overCallBack));
            }
            else overCallBack(true);
        }

        /// <summary>
        /// 获取本地ab包对比文件信息
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="overCallBack"></param>
        /// <returns></returns>
        private IEnumerator GetLocalAbCompareFile(string filePath, UnityAction<bool> overCallBack)
        {
            //使用UnityWebRequest读取文件
            UnityWebRequest req = UnityWebRequest.Get(filePath);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var fileInfo = req.downloadHandler.text;
                //写入本地ab包信息字典
                GetAbCompareFileInfo(fileInfo.Trim().Split('\n'), _localAbInfoDic);
                overCallBack.Invoke(true);
            }
            else overCallBack.Invoke(false);
        }

        /// <summary>
        /// 获取远端ab包对比文件信息
        /// </summary>
        private void GetAbCompareFileInfo(string[] infos, Dictionary<string, AbInfo> abInfoDic)
        {
            //4:拆分对比文件
            foreach (var infoStr in infos)
            {
                string[] concreteInfo = infoStr.Trim().Split('|');
                //加入ab包信息字典
                abInfoDic.Add(concreteInfo[0], new AbInfo(concreteInfo[0], concreteInfo[1], concreteInfo[2]));
            }
        }


        private void DownloadAbFile(Action<bool> overCallBack, Action<string> downloadPro)
        {
            //1:更新需要更新的ab
            int maxUpdateNum = 5;
            int maxUpdateFileNum = _updateAbList.Count;
            int nowFinishUpdateFileCount = 0;
            while (_updateAbList.Count != 0 && maxUpdateNum != 0)
            {
                for (var i = 0; i < _updateAbList.Count; i++)
                {
                    var isFinish = FtpDownloadFIle(Application.persistentDataPath + "/" + _updateAbList[i],
                        _updateAbList[i]);

                    if (!isFinish) continue;
                    _updateAbList.RemoveAt(i);
                    i--;

                    downloadPro.Invoke("下载中..." + ++nowFinishUpdateFileCount + "/" + maxUpdateFileNum);
                }

                maxUpdateNum--;
            }


            //回调函数
            overCallBack?.Invoke(_updateAbList.Count == 0);
        }

        /// <summary>
        /// 从远端下载文件
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private bool FtpDownloadFIle(string localPath, string fileName)
        {
            FtpWebRequest ftp;

            try
            {
                string path =
#if UNITY_ANDROID
            "Android";
#elif UNITY_IOS
            "IOS";
#else
                    "PC";
#endif
                //1：创建Ftp链接
                ftp = FtpWebRequest.Create(new Uri("ftp://127.0.0.1/AB/" + path + "/" + fileName)) as FtpWebRequest;
                NetworkCredential networkCredential = new NetworkCredential("MrLuo", "luo123");
                ftp.Credentials = networkCredential;
                ftp.Proxy = null;
                ftp.KeepAlive = false;
                ftp.Method = WebRequestMethods.Ftp.DownloadFile;
                ftp.UseBinary = true;
            }
            catch (Exception ex)
            {
                throw new Exception("创建ftp连接失败" + ex);
            }

            try
            {
                //2：创建ftp流
                FtpWebResponse webResponse = ftp.GetResponse() as FtpWebResponse;
                Stream downloadStream = webResponse.GetResponseStream();

                //3：下载到可读可写文件路径
                using (FileStream file = File.Create(localPath))
                {
                    byte[] bytes = new byte[2048];
                    int length = downloadStream.Read(bytes, 0, bytes.Length);

                    while (length != 0)
                    {
                        file.Write(bytes, 0, length);
                        length = downloadStream.Read(bytes, 0, bytes.Length);
                    }

                    file.Close();
                    downloadStream.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("文件下载失败" + ex);
                return false;
            }
        }


        /// <summary>
        /// ab包信息类
        /// </summary>
        private class AbInfo
        {
            public string abName;
            public long size;
            public string md5;

            public AbInfo(string abName, string size, string md5)
            {
                this.abName = abName;
                this.size = long.Parse(size);
                this.md5 = md5;
            }

#pragma warning disable CS0659 // 类型重写 Object.Equals(object o)，但不重写 Object.GetHashCode()
            public override bool Equals(object obj)
#pragma warning restore CS0659 // 类型重写 Object.Equals(object o)，但不重写 Object.GetHashCode()
            {
                return obj is AbInfo abInfo && md5.Equals(abInfo.md5);
            }
        
        }
    }
}