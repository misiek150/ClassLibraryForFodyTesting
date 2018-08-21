using System;

namespace CommonComponents
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class LogMethodAttribute : Attribute
    {
    }
}
