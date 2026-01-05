namespace WebAnalytics.CustomMiddleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class SkipSessionCheckAttribute : Attribute 

    {
    }
}
