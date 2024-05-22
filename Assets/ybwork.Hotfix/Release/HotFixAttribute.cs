using System;

namespace Hotfix
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class HotfixAttribute : Attribute
    {
        public readonly string Name;

        public HotfixAttribute() { }

        public HotfixAttribute(string name)
        {
            Name = name;
        }
    }
}
