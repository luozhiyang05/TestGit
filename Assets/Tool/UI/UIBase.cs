using System;
using Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tool.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIBase : MonoBehaviour, IController
    {
        [NonSerialized] public CanvasGroup canvasGroup;
        public bool isOpen;

        public void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Open()
        {
            isOpen = true;
        }

        public virtual void Close()
        {
            isOpen = false;
        }

        public IMgr Ins => Global.Instance;
    }
}