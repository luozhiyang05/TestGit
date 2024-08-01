print("加载BagPanel脚本")
BasePanel:subClass("BagPanel")

--控间
BagPanel.panelObj = nil
BagPanel.btnClose = nil

BagPanel.togEquip = nil
BagPanel.togItem = nil
BagPanel.togGen = nil

BagPanel.svBar = nil
BagPanel.content = nil

--其他变量
BagPanel.isAddEvent = nil

--存放格子的表
BagPanel.item = {}

--分类按钮逻辑
--type 1是装备，2是物品，3是珠宝
function BagPanel:ClickTog(type)

    --获取按钮分类
    local itemTable = nil
    if type==1 then
        itemTable = PlayerData:GetEquip()
    elseif type==2 then
        itemTable = PlayerData:GetItem()
    else 
        itemTable = PlayerData:GetGens()
    end

    --清除旧的格子
    for i = 1, #self.item do
        self.item[i]:Destroy()
    end
    self.item = {}

    --遍历表的元素，然后生成格子
    for i = 1, #itemTable do
        --生成格子
        local gird = ItemGird:new()
        gird:Init("SvBar/Viewport/Content")
        --初始化格子数据
        gird:InitData(itemTable[i])
        --插入格子表
        table.insert(self.item,gird)
    end

end

--初始化方法
function BagPanel:Init(fatherCanvasName)
     --初始化面板
     self.base.Init(self, "BagPanel", fatherCanvasName)

    if isAddEvent==nil then
        
        --获取子节点
        self.svBar = self:Find("SvBar")
        self.content = self:Find("SvBar/Viewport/Content")

        --为按钮绑定事件
        self:GetComponent("BtnClose","Button").onClick:AddListener(function()
            self:Hide()
        end)

        self:GetComponent("TogEquip","Toggle").onValueChanged:AddListener(function(value)
            if value==true then
                print("获取equip分类")
                self:ClickTog(1)
            end
        end)

        self:GetComponent("TogItem","Toggle").onValueChanged:AddListener(function(value)
            if value==true then
                print("获取Item分类")
                self:ClickTog(2)
            end
        end)

        self:GetComponent("TogGen","Toggle").onValueChanged:AddListener(function(value)
            if value==true then
                print("获取gens分类")
                self:ClickTog(3)
            end
        end)

        self.isAddEvent = true
    end
end

