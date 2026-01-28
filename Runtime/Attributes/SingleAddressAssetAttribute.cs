using System;

namespace CFramework.Attachment.com.cnoom.cframework.systems.Runtime.Attributes
{
    /// <summary>
    /// 生成具有该标签的资源时会自动加入到对应的资源地址中
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class SingleAddressAssetAttribute : Attribute
    {
        public string Path { get; private set; }
        public string Group { get; private set; }
        public string Label { get; private set; }

        public SingleAddressAssetAttribute(string path)
        {
            Path = path;
            Group = string.Empty;
            Label = string.Empty;
        }

        public SingleAddressAssetAttribute(string path, string group)
        {
            Path = path;
            Group = group;
            Label = string.Empty;
        }

        public SingleAddressAssetAttribute(string path, string group, string label)
        {
            Path = path;
            Group = group;
            Label = label;
        }
    }
}