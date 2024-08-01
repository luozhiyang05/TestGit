--定义玩家物品表
PlayerData = {}

--玩家物品表的分类表
PlayerData.equip = {}
PlayerData.item = {}
PlayerData.gens = {}

--定义初始化方法
function PlayerData:Init()
    --理应读取json保存的背包信息

    table.insert(self.equip,{id = 1,count = 1})
    table.insert(self.equip,{id = 2,count = 1})

    table.insert(self.item,{id = 4,count = 2})
    table.insert(self.gens,{id = 5,count = 3})
end

--获取背包信息
function PlayerData:GetEquip()
    return self.equip
end
function PlayerData:GetItem()
    return self.item
end
function PlayerData:GetGens()
    return self.gens
end