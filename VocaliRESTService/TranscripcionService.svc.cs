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
        private static readonly int maxFileSize = 5 * 1024 * 1024;
        private static ITranscripcionRepository repository = new TranscripcionRepository();

        public List<Transcripcion> GetTranscripcionList()
        {
            List<Transcripcion> lista = repository.SelectAll();
            Log.AppendText("OK: GetTranscripcionList()");
            return lista;
        }
        public List<Transcripcion> GetTranscripcionListPendientes()
        {
            List<Transcripcion> lista = repository.SelectPendientes();
            Log.AppendText("OK: GetTranscripcionListPendientes()");
            return lista;
        }
        /// <summary>
        /// Recibe el listado por login.
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public List<TranscripcionCU2> GetTranscripcionListByLogin(string login)
        {
            List<TranscripcionCU2> lista = GetTranscripcionListByLoginFechaRecepcion(login, string.Empty);
            Log.AppendText(string.Format("OK: GetTranscripcionListByLogin(string login:{0})",
                login));
            return lista;
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
                Log.AppendText(string.Format("Error: GetTranscripcionListByLoginFechaRecepcion(string login:{0}, string intervaloFechaRecepcion:{1})\r\n{2}",
                    login, intervaloFechaRecepcion, sError));
                throw new WebFaultException(System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                List<TranscripcionCU2> lista = repository.SelectByLoginFechaRecepcion(login, desdeFecha, hastaFecha);
                Log.AppendText(string.Format("OK: GetTranscripcionListByLoginFechaRecepcion(string login:{0}, string intervaloFechaRecepcion:{1})",
                    login, intervaloFechaRecepcion));
                return lista;
            }
        }

        public TranscripcionCU3 GetTranscripcionById(string id)
        {
            TranscripcionCU3 transcripcionCU3 = repository.SelectByIdCU3(Numeros.ToInt(id));
            Log.AppendText(string.Format("OK: GetTranscripcionById(string id:{0})",
                id));
            return transcripcionCU3;
        }

        public string AddTranscripcion(string login, string filename, byte[] fichero)
        {
            bool error = false;
            string mensaje = string.Empty;
            if (fichero == null)
            {
                error = true;
                mensaje = "Error CU1: Fichero null";
            }

            if (!error && fichero.Length > maxFileSize)
            {
                error = true;
                mensaje = string.Format("Error CU1: Fichero excede capacidad máxima de 5MB ({0:0.00}MB)",
                    ((float)fichero.Length / (1024 * 1024)));
            }

            if (!error)
            {
                Transcripcion transcripcion = new Transcripcion();
                transcripcion.Login = login;
                transcripcion.Estado = EstadoTranscripcion.Pendiente;
                transcripcion.NombreFichero = filename;
                transcripcion.Fichero = fichero;
                transcripcion.FechaRecepcion = DateTime.Now;
                repository.Insert(transcripcion);

                Log.AppendText(string.Format("OK: AddTranscripcion(login:{0}, string filename:{1}, byte[] fichero)",
                    login, fichero));
                mensaje = string.Format("Fichero registrado satisfactoriamente (id={0})",
                    transcripcion.IdTranscripcion);
            }

            return mensaje;
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

        #region Código comentado

        //public string AddTranscripcion(string login, string filename, System.IO.FileStream stream)
        //{
        //    if (stream != null) { throw new WebFaultException(System.Net.HttpStatusCode.BadRequest); }

        //    // Convertimos Stream a byte[]
        //    System.IO.MemoryStream ms = new System.IO.MemoryStream();
        //    stream.CopyTo(ms);
        //    byte[] fichero = ms.ToArray();
        //    if (fichero.Length > maxFileSize) { throw new WebFaultException(System.Net.HttpStatusCode.BadRequest); }

        //    Transcripcion transcripcion = new Transcripcion();
        //    transcripcion.Login = login;
        //    transcripcion.Estado = EstadoTranscripcion.Pendiente;
        //    transcripcion.NombreFichero = filename;
        //    transcripcion.Fichero = fichero;
        //    transcripcion.FechaRecepcion = DateTime.Now;
        //    repository.Insert(transcripcion);

        //    return "id=" + transcripcion.IdTranscripcion;
        //}

        //[DataContract]
        //public class MyCustomErrorDetail
        //{
        //    public MyCustomErrorDetail(string errorInfo, string errorDetails)
        //    {
        //        ErrorInfo = errorInfo;
        //        ErrorDetails = errorDetails;
        //    }

        //    [DataMember]
        //    public string ErrorInfo { get; private set; }

        //    [DataMember]
        //    public string ErrorDetails { get; private set; }
        //}

        //if (fichero == null) { throw new WebFaultException(System.Net.HttpStatusCode.BadRequest); }
        //if (fichero.Length > maxFileSize) { throw new WebFaultException(System.Net.HttpStatusCode.BadRequest); }

        #endregion
    }
}
