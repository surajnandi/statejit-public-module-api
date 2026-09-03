namespace sjam.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class NoAuthorizeAttribute : Attribute
    {
    }
}
