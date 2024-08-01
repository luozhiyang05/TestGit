print('加载Object脚本')
Object = {}

--继承
function Object : subClass(className)
    _G[className] = {}
    local obj = _G[className]
    self.__index = self
    obj.base = self
    setmetatable(obj,self)
end

--实例化对象
function Object : new()
    local obj = {}
    self.__index = self
    setmetatable(obj,self)
    return obj
end
