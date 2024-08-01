using UnityEngine;

namespace Tool.UI.UI
{
    public class UITest2 : UIBase
    {
        public override void Open()
        {
            base.Open();
            Debug.LogWarning("打开面板2");
        }

        public override void Close()
        {
            base.Close();
            Debug.LogWarning("关闭面板2");
        }
    }
}