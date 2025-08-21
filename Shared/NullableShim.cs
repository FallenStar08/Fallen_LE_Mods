namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class NullableAttribute : Attribute
    {
        public NullableAttribute(byte b) { }
        public NullableAttribute(byte[] b) { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
    internal sealed class NullableContextAttribute : Attribute
    {
        public NullableContextAttribute(byte flag) { }
    }
}