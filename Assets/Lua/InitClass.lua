print("加载InitClass脚本")
--面向对象
require("Object")
--字符串解析
require("SplitTools")
--Json解析
Json = require("JsonUtility")

--UnityEngine系统类
UnityEngine = CS.UnityEngine
GameObject = UnityEngine.GameObject
Transform = UnityEngine.Transform
RectTransform = UnityEngine.RectTransform
Vector3 = UnityEngine.Vector3
Vector2 = UnityEngine.Vector2
TextAsset = UnityEngine.TextAsset

--UI
SpriteAtlas = UnityEngine.U2D.SpriteAtlas
UI = UnityEngine.UI
Image = UI.Image
Text = UI.Text
Button = UI.Button
Slider = UI.Slider
Toggle = UI.Toggle
ScrollRect = UI.ScrollRect
UIBehaviour = UnityEngine.EventSystems.UIBehaviour


--Ab管理器
ABMgr = CS.Tool.ResourceMgr.AssetBundleMgr.GetInstance()