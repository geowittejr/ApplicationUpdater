using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ExceptionHandling;
using UpdateExecutableCommon.Utilities;

namespace CentralizedUpdateWebApi.Utilities
{
    public class GlobalExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            string msg = "CentralizedUpdateWebApi exception. Request url: \"" + context.RequestContext.Url + "\".";
            Logger.LogError(context.Exception, msg, "");
        }
    }
}