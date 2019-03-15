local HighScoreUI = LuaUIManager.Register(LuaUI, "HighScoreUI", UIType.Normal, UIResType.Resorces, UICloseType.Hide, true);

function HighScoreUI:Ctor(name, context)

    self.super.Ctor(self, name, context);

end

function HighScoreUI:OnAwake()

    Debug.Log("HighScoreUI OnAwake" .. self.Name)
    local BackButton = self.GameObject:FindGameObject("BackButton")
    self.super.RegisterListener(self, "BackButton", self.OnClickExitBtn)
    self.super.RegisterListener(self, BackButton, self.OnClickExitBtn)

end

function HighScoreUI:OnClickExitBtn(eventData)
    UIManager.Instance:Pop();
end

function HighScoreUI:OnStart(args)
    Debug.Log("HighScoreUI OnStart");
end

function HighScoreUI:OnEnable()
    Debug.Log("HighScoreUI OnEnable");
end

function HighScoreUI:OnDisable()
    Debug.Log("HighScoreUI OnDisable");
end

function HighScoreUI:OnDestroy()
    Debug.Log("HighScoreUI OnDestroy");
end

function HighScoreUI:OnNotifiy(evt, ...)
    
end