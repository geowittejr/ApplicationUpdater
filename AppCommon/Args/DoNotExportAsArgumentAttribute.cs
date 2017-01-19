using System;

namespace AppCommon.Args
{
    /// <summary>
    /// Attribute can be applied to class property that should not be exported as an argument
    /// </summary>
    public class DoNotExportAsArgumentAttribute : Attribute
    {
    }
}
