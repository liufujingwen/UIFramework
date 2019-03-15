local HighScoreUI = LuaUIManager.Register(LuaUI, "HighScoreUI", UIType.Normal, UIResType.Resorces, UICloseType.Hide, true);

function HighScoreUI:Ctor(name, context)

    self.super.Ctor(self, name, context);

end

function HighScoreUI:OnInit()

    Debug.Log("HighScoreUI OnInit" .. self.Name)
    local BackButton = self.GameObject:FindGameObject("BackButton")
    self.super.RegisterListener(self, "BackButton", self.OnClickExitBtn)
    self.super.RegisterListener(self, BackButton, self.OnClickExitBtn)

end

function HighScoreUI:OnClickExitBtn(eventData)
    UIManager.Instance:Pop();
end

function HighScoreUI:OnEnter(args)
    Debug.Log("HighScoreUI OnEnter");
end

function HighScoreUI:OnPause()
    Debug.Log("HighScoreUI OnPause");
end

function HighScoreUI:OnResume()
    Debug.Log("HighScoreUI OnResume");
end

function HighScoreUI:OnExit()
    Debug.Log("HighScoreUI OnExit");
end

function HighScoreUI:OnNotifiy(evt, ...)
    
end