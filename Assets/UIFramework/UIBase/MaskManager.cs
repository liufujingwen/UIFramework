using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIFramework
{
    public class MaskManager : Singleton<MaskManager>
    {
        private int MaskCount = 0;
        bool loadState = false;

        public async Task LoadMask()
        {
            if (!loadState)
            {
                await UIManager.Instance.LoadUIAsync("MaskUI");
                loadState = true;
            }
        }

        public void SetActive(bool visible)
        {
            MaskCount += visible ? 1 : -1;

            if (MaskCount > 0)
                UIManager.Instance.Open("MaskUI");
            else
                UIManager.Instance.Close("MaskUI");
        }

        public void Close()
        {
            MaskCount = 0;
            UIManager.Instance.Close("MaskUI");
        }
    }
}
