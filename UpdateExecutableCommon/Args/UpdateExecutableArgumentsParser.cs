using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fclp;

namespace UpdateExecutableCommon.Args
{
    public class UpdateExecutableArgumentsParser
    {
        /// <summary>
        /// Command line parser
        /// </summary>
        private FluentCommandLineParser<UpdateExecutableArguments> _Parser = null;

        public UpdateExecutableArgumentsParser()
        {
            //Create the internal parser and setup the property matching
            _Parser = new FluentCommandLineParser<UpdateExecutableArguments>();
            SetupArgumentMapping();
        }

        /// <summary>
        /// Parse the args array and return a UpdaterArguments object
        /// </summary>
        public UpdateExecutableArguments ParseArguments(string[] args)
        {
            var result = _Parser.Parse(args);

            if (result.HasErrors)
            {
                throw new Exception(result.ErrorText);
            }

            return _Parser.Object;
        }

        private void SetupArgumentMapping()
        {
            _Parser.Setup(arg => arg.ClientUpdateDirectoryPath)
                .As("ClientUpdateDirectoryPath");

            _Parser.Setup(arg => arg.CustomerGuid)
                .As("CustomerGuid");

            _Parser.Setup(arg => arg.CustomerName)
                .As("CustomerName");

            _Parser.Setup(arg => arg.UpdaterWebApiBaseUrl)
                .As("UpdaterWebApiBaseUrl");
            
            _Parser.Setup(arg => arg.AppDirectoryPath)
                .As("AppDirectoryPath");

            _Parser.Setup(arg => arg.AppOldVersion)
                .As("AppOldVersion");

            _Parser.Setup(arg => arg.AppNewVersion)
                .As("AppNewVersion");

            _Parser.Setup(arg => arg.AppZipFileUrl)
                .As("AppZipFileUrl");
        }
    }
}
