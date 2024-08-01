using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CopayLuaToTxt
{
    [MenuItem("XLua/复制Lua文件到LuaTxt")]
    public static void CopyLua()
    {
        //找到Lua文件夹
        string luaDicPath = Application.dataPath + "/Lua/";
        if (!Directory.Exists(luaDicPath)) return; //文件夹不存在就不操作

        //获取该文件夹下所有lua文件,参数2的*.lua可以指定获取指定后缀文件
        string[] filePaths = Directory.GetFiles(luaDicPath, "*.lua");

        //找到LuaTxt文件夹
        string newDirPath = Application.dataPath + "/LuaTxt";
        if (!Directory.Exists(newDirPath)) Directory.CreateDirectory(newDirPath);
        else
        {
            //如果LuaTxt存在，就清空里面的旧文件
            string[] oldFilePaths = Directory.GetFiles(newDirPath, "*.txt");
            for (int i = 0; i < oldFilePaths.Length; i++)
            {
                File.Delete(oldFilePaths[i]);
            }
        }

        //复制到LuaTxt
        StringBuilder sb = new StringBuilder();
        List<string> filePathsList = new List<string>();
        for (int i = 0; i < filePaths.Length; i++)
        {
            //修改后缀，拼接新路径
            sb.Clear();
            sb.Append(newDirPath + "/" + filePaths[i].Substring(filePaths[i].LastIndexOf("/") + 1) + ".txt");
            filePathsList.Add(sb.ToString());
            File.Copy(filePaths[i], sb.ToString());
        }

        //刷新编辑器
        AssetDatabase.Refresh();
        //修改AssetBundle包名

        //获取资源的编辑器面板，路径为相对于Asset的路径，然后设置ab包名字
        StringBuilder relativeAssetPath = new StringBuilder();
        AssetImporter importer;
        for (int i = 0; i < filePathsList.Count; i++)
        {
            //Assets/LuaTxt/MainPanel.lua
            relativeAssetPath.Clear();
            relativeAssetPath.Append(filePathsList[i].Substring(filePathsList[i].LastIndexOf("Asset")));
            Debug.LogWarning(relativeAssetPath);
            importer = AssetImporter.GetAtPath(relativeAssetPath.ToString());
            importer.assetBundleName = "lua";
        }

    }
}