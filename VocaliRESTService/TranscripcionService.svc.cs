using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Services.Description;

namespace VocaliRESTService
{
    public class TranscripcionService : ITranscripcionService
    {
        [DataContract]
        public class MyCustomErrorDetail
        {
            public MyCustomErrorDetail(string errorInfo, string errorDetails)
            {
                ErrorInfo = errorInfo;
                ErrorDetails = errorDetails;
            }

            [DataMember]
            public string ErrorInfo { get; private set; }

            [DataMember]
            public string ErrorDetails { get; private set; }
        }

        private static readonly int maxFileSize = 5 * 1024 * 1024;
        private static ITranscripcionRepository repository = new TranscripcionRepository();
        internal static Log log = new Log();
        
        public List<Transcripcion> GetTranscripcionList()
        {
            return repository.SelectAll();
        }
        public List<Transcripcion> GetTranscripcionListPendientes()
        {
            return repository.SelectPendientes();
        }
        /// <summary>
        /// Recibe el listado por login.
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public List<TranscripcionCU2> GetTranscripcionListByLogin(string login)
        {
            return GetTranscripcionListByLoginFechaRecepcion(login, string.Empty);
        }
        /// <summary>
        /// Recibe el listado por login e intervalo de fechas de recepción.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="intervaloFechaRecepcion">Cadena con formato yyyyMMddHHmm-yyyyMMddHHmm.
        /// Como puede ser abierto, se puede omitir el intervalo desde o hasta:
        /// -yyyyMMddHHmm (Sin intervalo desde)
        /// yyyyMMddHHmm- (Sin intervalo hasta)</param>
        /// <returns></returns>
        public List<TranscripcionCU2> GetTranscripcionListByLoginFechaRecepcion(string login, string intervaloFechaRecepcion)
        {
            DateTime? desdeFecha = null;
            DateTime? hastaFecha = null;
            DateTime tmpfecha;
            string[] sFechas = intervaloFechaRecepcion.Split("-".ToCharArray());
            bool bError = false;
            string sError = string.Empty;
            if (sFechas.Length >= 1)
            {
                if (sFechas[0].Trim() != string.Empty)
                {
                    if (DateTime.TryParseExact(sFechas[0], "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out tmpfecha))
                    { desdeFecha = tmpfecha; }
                    else
                    { bError = true; }
                }
            }
            if (sFechas.Length >= 2)
            {
                if (sFechas[1].Trim() != string.Empty)
                {
                    if (DateTime.TryParseExact(sFechas[1], "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out tmpfecha))
                    {
                        // Añadimos un minuto para incluir los segundos posteriores al último minuto.
                        // Posteriormente haremos que la fecha hasta sea estríctamente inferior para no incluir el siguiente minuto.
                        hastaFecha = tmpfecha.AddMinutes(1);
                    }
                    else
                    { bError = true; }
                }
            }

            if (bError)
            {
                sError = "El intervalo de fechas especificado es incorrecto. Use el formato: fecha=[yyyyMMddHHmm]-[yyyyMMddHHmm]\r\n"
                    + "Ejemplo 1: fecha=201609010000-201609302359 // Desde 01/09/2016 0:00 hasta 30-09-2016 23:59\r\n"
                    + "Ejemplo 2: fecha=201609010000-             // Desde 01/09/2016 0:00";

                throw new WebFaultException(System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                return repository.SelectByLoginFechaRecepcion(login, desdeFecha, hastaFecha);
            }
        }

        public TranscripcionCU3 GetTranscripcionById(string id)
        {
            return repository.SelectByIdCU3(Numeros.ToInt(id));
        }

        public string AddTranscripcion(string login, string filename, byte[] fichero)
        {
            if (fichero != null) { throw new WebFaultException(System.Net.HttpStatusCode.BadRequest); }
            if (fichero.Length > maxFileSize) { throw new WebFaultException(System.Net.HttpStatusCode.BadRequest); }

            Transcripcion transcripcion = new Transcripcion();
            transcripcion.Login = login;
            transcripcion.Estado = EstadoTranscripcion.Pendiente;
            transcripcion.NombreFichero = filename;
            transcripcion.Fichero = fichero;
            transcripcion.FechaRecepcion = DateTime.Now;
            repository.Insert(transcripcion);

            return "id=" + transcripcion.IdTranscripcion;
        }
        public string AddTranscripcion(string login, string filename, System.IO.FileStream stream)
        {
            if (stream != null) { throw new WebFaultException(System.Net.HttpStatusCode.BadRequest); }

            // Convertimos Stream a byte[]
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            stream.CopyTo(ms);
            byte[] fichero = ms.ToArray();
            if (fichero.Length > maxFileSize) { throw new WebFaultException(System.Net.HttpStatusCode.BadRequest); }

            Transcripcion transcripcion = new Transcripcion();
            transcripcion.Login = login;
            transcripcion.Estado = EstadoTranscripcion.Pendiente;
            transcripcion.NombreFichero = filename;
            transcripcion.Fichero = fichero;
            transcripcion.FechaRecepcion = DateTime.Now;
            repository.Insert(transcripcion);

            return "id=" + transcripcion.IdTranscripcion;
        }

        public string UpdateTranscripcion(Transcripcion transcripcion, string id)
        {
            bool updated = repository.Update(transcripcion);
            if (updated)
                return "Transcripcion with id = " + id + " updated successfully";
            else
                return "Unable to update Transcripcion with id = " + id;
        }

        public string DeleteTranscripcion(string id)
        {
            bool deleted = repository.Delete(Numeros.ToInt(id));
            if (deleted)
            { return "Transcripcion with id = " + id + " deleted successfully."; }
            else
            { return "Unable to delete Transcripcion with id = " + id; }
        }
    }
}
