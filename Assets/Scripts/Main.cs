using System;
using Tool.AB;
using Tool.ResourceMgr;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Main : MonoBehaviour
{
    private void Start()
    {
        AbUpdateMgr.GetInstance().CheckUpdate(value =>
        {
            if (value)
            {
                Debug.LogWarning("更新完毕");
            }
            LuaMgr.Instance.Init();
            //LuaMgr.Instance.DoLuaFile("EmmyLuaDebugger"); //Rider使用的lua调试脚本
            LuaMgr.Instance.DoLuaFile("Main");
        });


    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
    }
}