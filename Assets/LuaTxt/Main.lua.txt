print("加载Main脚本")
--初始化类
require('InitClass')

--初始化数据
require("ItemData")
require("PlayerData")

PlayerData:Init()


--面板初始化
require("BasePanel") --加载面板基类

require("MainPanel") --加载主面板类
require("ItemGird")
require("BagPanel") --加载背包面板


MainPanel:Init("Canvas")
-- MainPanel:Show()
-- MainPanel:Hide()
-- require("BagPanel")

--玩家行走脚本
require("PlayerController")


