using System;
using System.Collections.Generic;
using Tool.ResourceMgr;
using Tool.Single;
using Tool.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Tool.UI
{
    public enum EuiLayer
    {
        Top, //提示，警告
        Mid, //人物信息，血量，按钮UI
        Down, //二级弹窗
        System //UI交互
    }


    public class UIManager : Singleton<UIManager>
    {
        private Vector2 Resolution
        {
            get => _canvasScaler.referenceResolution;
            set => _canvasScaler.referenceResolution = value;
        }

        private Canvas _canvas;
        private RectTransform _canvasRectTrans;
        private CanvasScaler _canvasScaler;

        private Transform _top, _mid, _down, _system;
        private Stack<UIBase> _topStack, _midStack, _downStack, _systemStack;
        private Dictionary<string, UIBase> _openPanelDic = new Dictionary<string, UIBase>();

        protected override void OnInit()
        {
            #region 初始化UI栈和UI字典

            _topStack = new Stack<UIBase>();
            _midStack = new Stack<UIBase>();
            _downStack = new Stack<UIBase>();
            _systemStack = new Stack<UIBase>();
            _openPanelDic = new Dictionary<string, UIBase>();

            #endregion

            #region 创建UICanvas和EventSystem

            //创建画布
            var canvasObj = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))
            {
                //设置UI
                layer = LayerMask.NameToLayer("UI")
            };

            //创建事件系统
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            //销毁保护
            Object.DontDestroyOnLoad(canvasObj);
            //销毁保护
            Object.DontDestroyOnLoad(eventSystem);

            #endregion

            #region 赋值

            //获取UICanvas的组件
            _canvas = canvasObj.GetComponent<Canvas>();
            _canvasScaler = canvasObj.GetComponent<CanvasScaler>();
            _canvasRectTrans = canvasObj.GetComponent<RectTransform>();

            //设置值
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            _canvasScaler.referenceResolution = new Vector2(1920, 1080);
            //画布坐标清零
            _canvasRectTrans.localPosition = Vector3.zero;
            _canvasRectTrans.localScale = Vector3.one;
            _canvasRectTrans.offsetMax = Vector2.zero;
            _canvasRectTrans.offsetMin = Vector2.zero;

            #endregion

            #region 生成UI层

            //生成UI层
            _system = new GameObject("System").transform;
            _system.SetParent(_canvasRectTrans);
            _system.localPosition = Vector3.zero;

            _down = new GameObject("Down").transform;
            _down.SetParent(_canvasRectTrans);
            _down.localPosition = Vector3.zero;

            _mid = new GameObject("Mid").transform;
            _mid.SetParent(_canvasRectTrans);
            _mid.localPosition = Vector3.zero;

            _top = new GameObject("Top").transform;
            _top.SetParent(_canvasRectTrans);
            _top.localPosition = Vector3.zero;

            #endregion
        }

        public void Start(Vector2 resolution) => Resolution = resolution;


        /// <summary>
        /// 打开面板
        /// </summary>
        /// <param name="eUILayer"></param>
        /// <param name="callBack"></param>
        /// <typeparam name="T"></typeparam>
        public void OpenPanel<T>(EuiLayer eUILayer, Action<T> callBack = null) where T : UIBase
        {
            //根据UI层次获取队列
            Stack<UIBase> uiStack = GetUIStack(eUILayer);
            string name = typeof(T).Name;

            //先判断UI字典有无,有的话判断是否打开
            if (_openPanelDic.TryGetValue(name, out UIBase uiBase))
            {
                //已经打开，不处理
                if (uiBase.isOpen) return;

                //打开面板
                uiBase.gameObject.SetActive(true);
                callBack?.Invoke(uiBase as T);
                uiBase.Open();

                //判断当前队列是否有UI,有的话当前最顶端UI失去交互
                if (uiStack.TryPeek(out var oldPeekUIBase)) oldPeekUIBase.canvasGroup.interactable = false;

                //将要打开的队列入栈
                uiStack.Push(uiBase);
                if (uiStack.TryPeek(out var newPeekUIBase)) newPeekUIBase.canvasGroup.interactable = true;

                return;
            }

            //字典没有UI面板，需要加载UI面板
            ResMgr.GetInstance().AsyncLoad<GameObject>("Panel/" + name, loadUIBaseGo =>
            {
                //设置层级
                loadUIBaseGo.transform.SetParent(GetFatherLayer(eUILayer));

                //坐标清零
                loadUIBaseGo.transform.localPosition = Vector3.zero;
                loadUIBaseGo.transform.localScale = Vector3.one;

                //锚点初始化
                var uiRectTrans = loadUIBaseGo.GetComponent<RectTransform>();
                uiRectTrans.offsetMax = Vector2.zero;
                uiRectTrans.offsetMin = Vector2.zero;

                //设置UI长宽为分辨率
                uiRectTrans.sizeDelta = Resolution;

                //打开面板
                loadUIBaseGo.gameObject.SetActive(true);
                var newUIBase = loadUIBaseGo.GetComponent<T>();
                callBack?.Invoke(newUIBase);
                newUIBase.Open();

                //判断当前队列是否有UI,有的话当前最顶端UI失去交互
                if (uiStack.TryPeek(out var oldPeekUIBase)) oldPeekUIBase.canvasGroup.interactable = false;

                //将要打开的队列入栈
                uiStack.Push(newUIBase);
                if (uiStack.TryPeek(out var newPeekUIBase)) newPeekUIBase.canvasGroup.interactable = true;

                //存入字典
                _openPanelDic.Add(name, newUIBase);
            });
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        /// <param name="eUILayer"></param>
        public void ClosePanel(EuiLayer eUILayer)
        {
            //根据UI层次获取队列
            Stack<UIBase> uiStack = GetUIStack(eUILayer);
            if (!uiStack.TryPeek(out _)) return;

            //队列最顶端UI出列，失活
            if (!uiStack.TryPop(out var closeUIBase)) return;
            closeUIBase.canvasGroup.interactable = false;
            closeUIBase.Close();
            closeUIBase.gameObject.SetActive(false);
        }

        public T GetUIPanel<T>() where T : UIBase
        {
            var uiBase = _openPanelDic[typeof(T).Name];
            if (!uiBase) throw new Exception(nameof(T) + "未加载");
            return uiBase as T;
        }


        private Stack<UIBase> GetUIStack(EuiLayer eUILayer)
        {
            //根据UI层次获取队列
            return eUILayer switch
            {
                EuiLayer.System => _systemStack,
                EuiLayer.Down => _downStack,
                EuiLayer.Top => _topStack,
                EuiLayer.Mid => _midStack,
                _ => throw new ArgumentOutOfRangeException(nameof(eUILayer), eUILayer, null)
            };
        }

        private Transform GetFatherLayer(EuiLayer eUILayer)
        {
            switch (eUILayer)
            {
                case EuiLayer.Top: return _top;
                case EuiLayer.Mid: return _mid;
                case EuiLayer.Down: return _down;
                case EuiLayer.System: return _system;
                default: return null;
            }
        }
    }
}