using UnityEngine;

namespace Tool.UI.UI
{
    public class UITest1 : UIBase
    {
        public override void Open()
        {
            base.Open();
            Debug.LogWarning("打开面板1");
        }

        public override void Close()
        {
            base.Close();
            Debug.LogWarning("关闭面板1");
        }
    }
}