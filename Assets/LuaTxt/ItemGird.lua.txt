print("加载ItemGird脚本")
BasePanel:subClass("ItemGird")

ItemGird.panelObj = nil
--控间
ItemGird.imgItem = nil
ItemGird.textCount = nil

--初始化
function ItemGird:Init(father)
    self.base.Init(self, "ItemGird", father)
end

--设置数据
--data数据包含 id，count
function ItemGird:InitData(data)
    --加载ab包的icon图集
    local icon = ABMgr:LoadRes("ui", "Icon", typeof(SpriteAtlas))
    --获取图片
    local itemDataTable = ItemData[data.id]
    local strs = string.split(itemDataTable.icon, '_')
    --设置信息
    if strs ~= nil then
        self:GetComponent("ImgItem", "Image").sprite = icon:GetSprite(strs[2])
    end

    self:GetComponent("TextCount", "Text").text = tostring(data.count)
end

function ItemGird:Destroy()
    GameObject.Destroy(self.panelObj)
    self.panelObj = nil
end
