using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;
using System.IO;
using UIFramework;

public class Main : MonoBehaviour
{
    public static Main Instance = null;
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
        UIManager.Instance.Init();
        UIManager.Instance.Push("MainMenuUI");
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

}

