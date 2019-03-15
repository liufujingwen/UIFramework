LuaUI = Class("LuaUI")

function LuaUI:Ctor(name, uiProxy)

    self.Name = name
    self.UIProxy = uiProxy
    self.UIContext = uiProxy.UIContext
    self.Transform = self.UIContext.UI.Transform
    self.GameObject = self.UIContext.UI.GameObject

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

--注册点击事件
function LuaUI:RegisterListener(button, handle, clear)

    clear = clear and true or false
    self.UIProxy:RegisterListener(button, function()
        if handle then
            handle(self);
        end
    end, clear)

end

function LuaUI:FindComponent(name, type)

    return self.UIProxy:FindComponent(name, type);

end