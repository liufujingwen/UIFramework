LuaUIManager = Class("LuaUIManager")

require("Manager/LuaUI")

local ClassType = {}

--注册UI
-- @super 父类
-- @uiName UI名字
-- @uiType UI类型
-- @uiResType UI加载方式
-- @uiCloseTypeUI 关闭方式
-- @hasAnimation UI是否有动画
--function LuaUIManager.Register(super, uiName, uiType, uiResType, uiCloseType)
function LuaUIManager.Register(super, uiName, uiType, uiResType, uiCloseType, hasAnimation)

    print(tostring(super) .. "  " .. uiName .. "  " .. tostring(uiType) .. "  " .. tostring(uiResType) .. "  " .. tostring(uiCloseType))

    super = LuaUI or super
    UIManager.Instance:Register(uiName, uiType, uiResType, uiCloseType, hasAnimation, true)
    local uiObject = Class(uiName, super)
    ClassType[uiName] = uiObject
    return uiObject

end

--创建一个LuaUI的实例
--@name LuaUI脚本名字
--@gameUI C#的GameUI
function LuaUIManager.New(uiName, uiProxy)

    local baseName = uiName
    local class = ClassType[baseName]
    if not class then
        baseName = string.match(baseName, '%w*[^(%d)$*]')       -- 解析包含数字后缀的界面
        class = ClassType[baseName]
        if not class then
            XLog.Error("LuaUIManager.New error, class not exist, name: " .. uiName)
            return nil
        end
    end
    local obj = class.New(uiName, uiProxy)
    uiProxy:SetLuaTable(obj)
    return obj

end


--打开UI
--@uiName 打开的UI名字
function LuaUIManager.Open(uiName, ...)
    CsXUiManager.Instance:Open(uiName,...)
end


--关闭UI
--@uiName 关闭的UI名字
function LuaUIManager.Close(uiName)
    CsXUiManager.Instance:Close(uiName)
end
