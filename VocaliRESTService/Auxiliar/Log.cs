using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace VocaliRESTService
{
    /// <summary>
    /// Gestiona la conversión de números.
    /// </summary>
    public class Log
    {
        private string filepath;

        public Log(string filepath = "")
        {
            if (string.IsNullOrEmpty(filepath)) { filepath = @"C:\logs\RESTServiceTest.log"; }
            if (File.Exists(filepath)) { File.Delete(filepath); }
        }

        public void Append(string texto)
        {
            File.AppendAllText(filepath, texto);
        }

        public static Log log = null;
        public static void AppendText(string texto)
        {
            (log = log?? new Log()).Append(texto);
        }
    }
}