--读取ab包的ItemData，返回值类型为TextAsset
local itemsTxt = ABMgr : LoadRes("json","itemData",typeof(TextAsset))
--使用JsonUtility读取，返回一张表，存储一行一行的json
local readJsonData = Json.decode(itemsTxt.text)

--定义一张表，索引为item的id，方便查询
ItemData = {}
for _, value in pairs(readJsonData) do
    ItemData[value.id] = value
end


