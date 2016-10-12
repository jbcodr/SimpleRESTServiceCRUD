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
        public List<Transcripcion> GetTranscripcionListByLoginFechaRecepcion(string login, string desdeFechaRecepcion, string hastaFechaRecepcion)
        {
            DateTime? desdeFecha = null;
            DateTime? hastaFecha = null;
            DateTime tmpfecha;
            if (DateTime.TryParse(desdeFechaRecepcion, out tmpfecha)) { desdeFecha = tmpfecha; }
            if (DateTime.TryParse(hastaFechaRecepcion, out tmpfecha)) { hastaFecha = tmpfecha; }

            return repository.SelectByLoginFechaRecepcion(login, desdeFecha, hastaFecha);
        }

        public Transcripcion GetTranscripcionById(string id)
        {
            return repository.SelectById(Numeros.ToInt(id));
        }

        public string AddTranscripcion(Transcripcion transcripcion, string id)
        {
            Transcripcion newTranscripcion = repository.Insert(transcripcion);
            return "id=" + newTranscripcion.IdTranscripcion;
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
