using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppCommon.Args
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Output the properties of item T as an array of string arguments.
        /// This can be used to serialize an objec T into arguments that can be passed to a console app.
        /// </summary>
        public static string[] ToArgumentsArray<T>(this T item)
        {
            if (item == null)
                return null;

            //Get properties of object and loop through them.
            var result = new List<string>();
            var props = item.GetType().GetProperties().Where(prop => !prop.IsDefined(typeof(DoNotExportAsArgumentAttribute), false));
            foreach (var pi in props)
            {
                var value = item.GetType().GetProperty(pi.Name).GetValue(item, null);
                var valueString = value != null ? value.ToString() : string.Empty;

                //escape things that will break
                valueString = valueString.Replace(System.Environment.NewLine, " "); //replace line break with space
                valueString = string.Format("\"{0}\"", valueString); //put quotes around values               

                result.Add("/" + pi.Name + ":" + valueString);
            }

            return result.ToArray<string>();
        }

        /// <summary>
        /// Output the arguments of a string array as a space delimited string of those arguments. Ideal for sending to Process.Start().
        /// This can be used to format an array of string arguments into a string that can be passed to a Process.Start() method./// 
        /// </summary>
        public static string ToArgumentsString(this string[] args)
        {
            string result = string.Empty;
            foreach (string arg in args)
            {
                result += arg + " ";
            }
            return result;
        }
    }
}
