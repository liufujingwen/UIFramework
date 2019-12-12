using UnityEngine;
using XLua;
using System.IO;
using UIFramework;

public class Main : MonoBehaviour
{
    public static Main Instance = null;
    public bool useGUI = false;
    public LuaEnv LuaEnv = new LuaEnv();
    string luaPath = "LuaScripts";

    public void Start()
    {
        Instance = this;

        //lua路径
        luaPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, luaPath);
        LuaEnv.AddLoader(CustomLoader);

        Import("Common");
        Import("Manager");
        Import("UI");

        DllHelper.Init();
        UIManager.instance.Init();
        UIManager.instance.Open("MainMenuUI");
    }

    private byte[] CustomLoader(ref string filepath)
    {
        byte[] bytes = null;
        string path = Path.Combine(luaPath, filepath + ".lua");
        if (File.Exists(path))
        {
            bytes = File.ReadAllBytes(path);
        }
        return bytes;
    }

    public void Import(string path)
    {
        string importPath = Path.Combine(luaPath, path);
        string[] luaFils = Directory.GetFiles(importPath, "*.lua", SearchOption.AllDirectories);
        if (luaFils != null && luaFils.Length > 0)
        {
            for (int i = 0; i < luaFils.Length; i++)
            {
                string luaFile = luaFils[i];
                byte[] bytes = File.ReadAllBytes(luaFile);
                LuaEnv.DoString(bytes, luaFile);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.I))
        {
            UIManager.instance.Open("DialogUI", 1, 2, 3);
        }
        else if (Input.GetKeyUp(KeyCode.O))
        {
            UIManager.instance.Close("DialogUI");
        }
        else if (Input.GetKeyUp(KeyCode.P))
        {
            UIManager.instance.InitUIRoot(UIResType.Resorces);
        }
        else if (Input.GetKeyUp(KeyCode.L))
        {
            //UIManager.Instance.PopThenOpen("HighScoreUI");
            UIManager.instance.PopThenOpen("MainMenuUI");
            //UIManager.Instance.Open("ParentUI");
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(Screen.height - 20);
        GUILayout.BeginHorizontal();
        GUILayout.Label("快捷键I: UIManager.Instance.Open(\"DialogUI\")");
        GUILayout.Space(10);
        GUILayout.Label("快捷键O: UIManager.Instance.Close(\"DialogUI\")");
        GUILayout.Space(10);
        GUILayout.Label("快捷键P: 切换UIRoot");
        GUILayout.EndHorizontal();
    }

}

