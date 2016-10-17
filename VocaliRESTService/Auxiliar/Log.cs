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

        public Log(string filepath = "", bool recreate = false)
        {
            if (string.IsNullOrEmpty(filepath)) { this.filepath = @"C:\logs\VocaliRESTService.log"; }
            if (recreate && File.Exists(filepath)) { File.Delete(filepath); }
        }

        public void Append(string texto)
        {
            File.AppendAllText(filepath, string.Format("{0:dd.MM.yyyy HH:mm:ss.fff}: {1}\r\n", DateTime.Now, texto));
        }

        public static Log log = null;
        public static void AppendText(string texto)
        {
            (log = log?? new Log()).Append(texto);
        }
    }
}