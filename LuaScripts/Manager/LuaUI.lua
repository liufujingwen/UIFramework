LuaUI = Class("LuaUI")

function LuaUI:Ctor(name, uiProxy)

    self.name = name
    self.uiProxy = uiProxy
    self.ui = uiProxy.ui

end

function LuaUI:SetGameObject()
    self.transform = self.ui.transform
    self.gameObject = self.ui.gameObject
end

function LuaUI:OnAwake()
end

function LuaUI:OnStart(...)
end

function LuaUI:OnEnable()
end

function LuaUI:OnDisable()
end

function LuaUI:OnDestroy()
end

function LuaUI:OnNotifiy(evt, ...)
end

function LuaUI:OnGetEvents()
end

--注册点击事件
function LuaUI:RegisterListener(button, handle, clear)

    clear = clear and true or false
    self.uiProxy:RegisterListener(button, function()
        if handle then
            handle(self)
        end
    end, clear)

end

function LuaUI:FindComponent(name, type)
    return self.uiProxy:FindComponent(name, type)
end

function LuaUI:OpenChildUI(childUIName, ...)
    self.uiProxy:OpenChildUI(childUIName, ...)
end

function LuaUI:CloseChildUI(childUIName)
    self.uiProxy:CloseChildUI(childUIName)
end

--快捷关闭界面
function LuaUI:Close()
    self.uiProxy:Close()
end