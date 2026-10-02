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

//I have no idea if this is actually necessary but it seems to be for some reason, so here we are. 
//This is a shim to make the compiler shut up about nullable reference types
//It doesn't actually do anything at runtime, it's just there to satisfy the compiler.
//I stole this from someone.