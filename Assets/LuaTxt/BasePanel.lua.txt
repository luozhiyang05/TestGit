print("加载BasePanel脚本")
Object:subClass("BasePanel")

BasePanel.panelObj = nil
BasePanel.panelName = nil
BasePanel.nodeTable = {}
BasePanel.isInit = false

function BasePanel:Init(panelName,father)
    if self.isInit ~= true then
        --加载ab包的ui，MainPanel资源
        self.panelObj = ABMgr:LoadRes("ui", panelName, typeof(GameObject))
        self.panelObj = GameObject.Instantiate(self.panelObj)
        self.panelObj.transform:SetParent(GameObject.Find(father).transform, false)
        self.panelName = panelName

        --返回组件数组
        local controlArray = self.panelObj.transform:GetComponentsInChildren(typeof(UIBehaviour))
        for i = 0, controlArray.Length - 1 do
            --用节点的名字作为key,Component.gameObject表示组件挂载的对象，用组件作为value
            local nodeName = controlArray[i].gameObject.name
            --组件类型名字
            local typeName = controlArray[i]:GetType().Name 
            --节点表存储的信息是一张组件表，组件表存储的格式是 组件类型名-组件信息
            if self.nodeTable[nodeName] ~= nil then
                self.nodeTable[nodeName][typeName] = controlArray[i]
            else
                self.nodeTable[nodeName] = { [typeName] = controlArray[i] }
            end
        end

        --标记已经初始化
        self.isInit = true
    end
end

function BasePanel:GetComponent(nodeName, typeName)
    local targetNode = self.nodeTable[nodeName]
    if targetNode == nil then return nil end
    return targetNode[typeName]
end

function BasePanel:Find(nodePath)
    return self.panelObj.transform:Find(nodePath)
end

function BasePanel:Show()
    self.panelObj:SetActive(true)
end

function BasePanel:Hide()
    self.panelObj:SetActive(false)
end
