using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace SimpleRESTServiceCRUD
{
    /// <summary>
    /// Mock Up del servicio Invox Medical. Un método estático devolverá un texto predefinido entre 4 posibles
    /// o bien un error genérico con una probabilidad del 5%.
    /// </summary>
    public static class InvoxService
    {
        public class InvoxResult
        {
            public int IdTranscripcion { get; set; }
            public DateTime? FechaTranscripcion { get; set; }
            public string TextoTranscripcion { get; set; }
            public bool Error { get; set; }
        }

        /// <summary>
        /// Array con 4 textos predefinidos.
        /// </summary>
        private static readonly string[] textos = new string[] 
        {
            "Cuando estás solucionando un problema, no tienes que preocuparte. Ahora, después de que has resuelto el problema, entonces sí es momento de preocuparse.", // Feynman
            "Estudia mucho lo que sea de interés para ti y hazlo de la forma más indisciplinada, irreverente y original posible", // Feynman
            "La suerte favorece solo a la mente preparada", // Asimov
            "La violencia es el último recurso del incompetente" // Asimov
        };
        public static InvoxResult ProcesarTranscripcion(int IdTranscripcion, string login, byte[] fichero)
        {
            InvoxResult invoxResult = new InvoxResult();
            invoxResult.IdTranscripcion = IdTranscripcion;

            // Simulamos el procesado del archivo empleando entre 1 y 10 segundos.
            System.Threading.Thread.Sleep(Numeros.Rnd.Next(1000, 10000));

            invoxResult.FechaTranscripcion = DateTime.Now;
            invoxResult.Error = (Numeros.Rnd.NextDouble() < .05); // Simulación de un error genérico con probabilidad del 5%
            if (!invoxResult.Error)
            {
                int index = Numeros.Rnd.Next(textos.Length); // Obtiene un número entre 0 y (textos.Length - 1).
                invoxResult.TextoTranscripcion = textos[index];
            }

            return invoxResult;
        }
    }
}
