using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 *	
 *  
 *
 *	by Xuanyi
 *
 */

namespace UIFramework
{
    public class GameRoot : MonoBehaviour
    {
        public static GameRoot instance = null;

        public void Start()
        {
            instance = this;
            DllHelper.Init();
            UIManager.Instance.Init();
            //UIManager.Instance.LoadUIAsync("MaskUI").ConfigureAwait(true);
            UIManager.Instance.Push("MainMenuUI");
            //UIManager.Instance.Push("MaskUI");
        }
    }
}
