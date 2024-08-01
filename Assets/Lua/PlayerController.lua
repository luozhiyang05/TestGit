GameObject = CS.UnityEngine.GameObject
Vector3 = CS.UnityEngine.Vector3
Input = CS.UnityEngine.Input
KeyCode = CS.UnityEngine.KeyCode
PlayerMove = CS.PlayerMove

--寻找玩家移动脚本
PlayerController = GameObject.Find("Player").transform:GetComponent(typeof(PlayerMove))

--编写行走逻辑
local update = function()
    if Input.GetKeyDown(KeyCode.W) then
        PlayerController.transform:Translate(Vector3(0,5,0))
    end
    if Input.GetKeyDown(KeyCode.A) then
        PlayerController.transform:Translate(Vector3(-5,0,0))
    end
    if Input.GetKeyDown(KeyCode.S) then
        PlayerController.transform:Translate(Vector3(0,-5,0))
    end
    if Input.GetKeyDown(KeyCode.D) then
        PlayerController.transform:Translate(Vector3(5,0,0))
    end
end

--添加行走事件
PlayerController:moveEvent("+",update)