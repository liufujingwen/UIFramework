using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIFramework
{
    public class CustomStack<T>
    {
        List<T> list = new List<T>();

        public int Count
        {
            get { return list.Count; }
        }

        public T Peek()
        {
            return list.Count > 0 ? list[list.Count - 1] : default(T);
        }

        public void Push(T value)
        {
            list.Add(value);
        }

        public T Pop()
        {
            T pop = default(T);
            if (list.Count > 0)
            {
                pop = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
            }
            return pop;
        }

        public void Clear()
        {
            list.Clear();
        }

        public List<T> GetList()
        {
            return list;
        }

        public bool Contains(T value)
        {
            return list.Contains(value);
        }

        public void Remove(T value)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                T temp = list[i];
                if (temp.Equals(value))
                    list.RemoveAt(i);
            }
        }

        public void RemoveOne(T value)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                T temp = list[i];
                if (temp.Equals(value))
                {
                    list.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
