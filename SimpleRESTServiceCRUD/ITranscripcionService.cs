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
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Xml, UriTemplate = "Transcripciones/")]
        List<Transcripcion> GetTranscripcionList();

        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Xml, UriTemplate = "Transcripciones/pendientes")]
        List<Transcripcion> GetTranscripcionListPendientes();

        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Xml, UriTemplate = "Transcripciones/login={login};desdefr={desdefr};hastafr={hastafr}")]
        List<Transcripcion> GetTranscripcionListByLoginFechaRecepcion(string login, string desdefr, string hastafr);

        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "Transcripciones/{id}")]
        Transcripcion GetTranscripcionById(string id);

        [OperationContract]
        [WebInvoke(Method = "PUT", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "AddTranscripcion/{id}")]
        string AddTranscripcion(Transcripcion Transcripcion, string id);

        [OperationContract]
        [WebInvoke(Method = "PUT", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "UpdateTranscripcion/{id}")]
        string UpdateTranscripcion(Transcripcion Transcripcion, string id);

        [OperationContract]
        [WebInvoke(Method = "DELETE", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "DeleteTranscripcion/{id}")]
        string DeleteTranscripcion(string id);

    }
}
