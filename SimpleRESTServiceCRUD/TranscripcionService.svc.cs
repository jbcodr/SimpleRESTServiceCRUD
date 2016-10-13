using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SimpleRESTServiceCRUD
{
    public class TranscripcionService : ITranscripcionService
    {
        static ITranscripcionRepository repository = new TranscripcionRepository();
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
        public List<Transcripcion> GetTranscripcionListByLogin(string login)
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
        public List<Transcripcion> GetTranscripcionListByLoginFechaRecepcion(string login, string intervaloFechaRecepcion)
        {
            DateTime? desdeFecha = null;
            DateTime? hastaFecha = null;
            DateTime tmpfecha;
            string[] sFechas = intervaloFechaRecepcion.Split("-".ToCharArray());
            if (sFechas.Length >= 1)
            {
                if (DateTime.TryParseExact(sFechas[0], "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out tmpfecha)) { desdeFecha = tmpfecha; }
            }
            if (sFechas.Length >= 2)
            {
                if (DateTime.TryParseExact(sFechas[1], "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out tmpfecha))
                {
                    // Al recibir el intervalo de fechas para listar las transcripciones, permitimos una precisión de hasta minutos.
                    // Por tanto hay que tener en cuenta que en el intervalo hasta se incluyan los registros del último minuto:
                    // Por ejemplo hasta las 23:59h debería incluir los recibidos a las 23h59m59s, pero no los de las 24h00m00s
                    // Para esto, vamos a añadir un minuto a la consulta, pero haremos que la fecha de recepción sea estríctamente inferior a este parámetro
                    // para no incuir los registros de las 00:00h del día siguiente, pero sí cualquiera anterior (23h:59m59,999999s)

                    hastaFecha = tmpfecha.AddMinutes(1);
                }
            }

            return repository.SelectByLoginFechaRecepcion(login, desdeFecha, hastaFecha);
        }

        public Transcripcion GetTranscripcionById(string id)
        {
            return repository.SelectById(Numeros.ToInt(id));
        }

        public string AddTranscripcion(string login, string filename, byte[] fichero)
        {
            Transcripcion transcripcion = new Transcripcion();
            transcripcion.Login = login;
            transcripcion.Estado = Transcripcion.EstadoTranscripcion.Pendiente;
            transcripcion.NombreFichero = filename;
            transcripcion.Fichero = fichero;
            transcripcion.FechaRecepcion = DateTime.Now;
            repository.Insert(transcripcion);

            return "id=" + transcripcion.IdTranscripcion;
        }
        //public string AddTranscripcion(Transcripcion transcripcion, string id)
        //{
        //    Transcripcion newTranscripcion = repository.Insert(transcripcion);
        //    return "id=" + newTranscripcion.IdTranscripcion;
        //}
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
