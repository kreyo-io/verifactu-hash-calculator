// Polyfill for init-only setters in .NET Standard 2.0
#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}
#endif
