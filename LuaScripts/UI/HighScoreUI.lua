local HighScoreUI = LuaUIManager.Register(LuaUI, "HighScoreUI", UIType.Normal, UIResType.Resorces, UICloseType.Hide, true);

function HighScoreUI:Ctor(name, context)

    self.super.Ctor(self, name, context);

end

function HighScoreUI:OnAwake()

    Debug.Log("Lua HighScoreUI OnAwake" .. self.Name)
    local BackButton = self.GameObject:FindGameObject("BackButton")
    self.super.RegisterListener(self, "BackButton", self.OnClickExitBtn)
    self.super.RegisterListener(self, BackButton, self.OnClickExitBtn)
    
end

function HighScoreUI:OnClickExitBtn(eventData)

    UIManager.Instance:Pop()

end

function HighScoreUI:OnStart(args)
    Debug.Log("Lua HighScoreUI OnStart")
end

function HighScoreUI:OnEnable()
    Debug.Log("Lua HighScoreUI OnEnable")
end

function HighScoreUI:OnDisable()
    Debug.Log("Lua HighScoreUI OnDisable")
end

function HighScoreUI:OnDestroy()
    Debug.Log("Lua HighScoreUI OnDestroy")
end

function HighScoreUI:OnNotifiy(evt, ...)
    
end