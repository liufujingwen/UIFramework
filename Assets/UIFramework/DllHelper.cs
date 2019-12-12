﻿using System;
using System.Linq;
using System.Reflection;

namespace UIFramework
{
    public static class DllHelper
    {
        private static Type[] types = null;
        private static Assembly assembly = null;

        public static void Init()
        {
            assembly = typeof(DllHelper).Assembly;
            types = assembly.GetTypes().ToArray();
        }

        public static Type[] GetMonoTypes()
        {
            return types;
        }
    }
}