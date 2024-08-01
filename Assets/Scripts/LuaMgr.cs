using System.IO;
using Tool.ResourceMgr;
using Tool.Single;
using UnityEngine;
using XLua;

public class LuaMgr : Singleton<LuaMgr>
{
    private LuaEnv _luaEnv;

    /// <summary>
    /// 获取lua的大G表
    /// </summary>
    public LuaTable Global => _luaEnv?.Global;

    protected override void OnInit(){}
    public void Init()
    {
        if (_luaEnv != null)
            return;
        _luaEnv = new LuaEnv();
        //重定向加载ab中的lua脚本，最后测试热更新和最终打包才会去ab包加载lua
        //_luaEnv.AddLoader(CustomLoader);
        _luaEnv.AddLoader(CustomAbLoader);//读取ab包的lua
    }
    
    /// <summary>
    /// 重定向读取lua文件的路径
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    private byte[] CustomLoader(ref string filepath)
    {
        //传入的参数是lua文件名，拼接重定向路径
        string path = Application.dataPath + "/Lua/" + filepath + ".lua";
        
        //跳过处理，emmy_core在lua调试脚本里加载
        if (filepath.Contains("emmy_core")) return null;

        //读取文件的字节数据到字节数组后返回
        if (File.Exists(path)) return File.ReadAllBytes(path);
        Debug.Log("找不到lua文件,文件名为" + filepath);
        return null;
    }

    /// <summary>
    /// 重定向从ab包中读取lua文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private byte[] CustomAbLoader(ref string filePath)
    {
        //打包成ab包的lua文件后缀需要加.txt才能打包
        //使用AbMgr加载,参数1为包名，参数2为文件命不带后缀.txt
        var lua = AssetBundleMgr.GetInstance().LoadRes<TextAsset>("lua", filePath + ".lua");
        
        //返回字节数组
        if (lua != null) return lua.bytes;
        Debug.LogWarning("lua文件重定向失败，文件名为：" + filePath);
        return null;
    }

    public void DoLuaFile(string fileName)
    {
        if(_luaEnv==null)
        {
            Debug.LogWarning("luaEnv已销毁");
            return;
        }
        _luaEnv.DoString($"require('{fileName}')");
    }

    public void DoString(string luaStr)
    {
        if(_luaEnv==null)
        {
            Debug.LogWarning("luaEnv已销毁");
            return;
        }
        _luaEnv.DoString(luaStr);
    }

    public void Tick()
    {
        if(_luaEnv==null)
        {
            Debug.LogWarning("luaEnv已销毁");
            return;
        }
        _luaEnv.Tick();
    }

    public void Dispose()
    {
        if(_luaEnv==null)
        {
            Debug.LogWarning("luaEnv已销毁");
            return;
        }
        _luaEnv.Dispose();
        _luaEnv = null;
    }
}