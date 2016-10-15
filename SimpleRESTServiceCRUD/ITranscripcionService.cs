using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace SimpleRESTServiceCRUD
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ITranscripcionService" in both code and config file together.
    [ServiceContract]
    public interface ITranscripcionService
    {
        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "Transcripciones/")]
        List<Transcripcion> GetTranscripcionList();

        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "Transcripciones/pendientes")]
        List<Transcripcion> GetTranscripcionListPendientes();

        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "Transcripciones/login={login}")]
        List<TranscripcionCU2> GetTranscripcionListByLogin(string login);

        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "Transcripciones/login={login}/fecha={fecha}")]
        List<TranscripcionCU2> GetTranscripcionListByLoginFechaRecepcion(string login, string fecha);

        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetTranscripcion/{id}")]
        string GetTranscripcionById(string id);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "AddTranscripcion?login={login}&filename={filename}")]
        string AddTranscripcion(string login, string filename, System.IO.FileStream stream);

        //[OperationContract]
        //[WebInvoke(Method = "PUT", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "UpdateTranscripcion/{id}")]
        //string UpdateTranscripcion(Transcripcion Transcripcion, string id);

        //[OperationContract]
        //[WebInvoke(Method = "PUT", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "AddTranscripcion/login={login}/filename={filename}")]
        //string AddTranscripcion(string login, string filename, byte[] fichero);
    }
}
