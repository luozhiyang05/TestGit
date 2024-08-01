print("加载MainPanel脚本")
BasePanel:subClass("MainPanel")

--控件
MainPanel.panelObj = nil
MainPanel.btnRole = nil
MainPanel.btnTask = nil

--变量
MainPanel.isOpenBagPanel = false
MainPanel.isAddEvent = false
MainPanel.isInitBagPanel = false

--点击BtnRole逻辑
function MainPanel:OnClickBtnRole()
    print("点击BtnRole")
    --打开背包面板
    if self.isInitBagPanel == false then
        BagPanel:Init("Canvas")
        self.isInitBagPanel = true
    else
        BagPanel:Show()
    end
end

--点击BtnTask逻辑
function MainPanel:OnClickBtnTask()
    print("点击BtnTask")
end

--初始化方法
--fatherCanvasName画布名字
function MainPanel:Init(fatherCanvasName)
    --初始化面板
    self.base.Init(self, "MainPanel", fatherCanvasName)

    --为按钮绑定事件
    if self.isAddEvent == false then
        self:GetComponent("BtnRole", "Button").onClick:AddListener(function()
            self:OnClickBtnRole()
        end)
        self:GetComponent("BtnTask", "Button").onClick:AddListener(function()
            self:OnClickBtnTask()
        end)
        self.isAddEvent = true
    end
end
