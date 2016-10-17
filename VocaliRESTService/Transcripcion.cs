using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ServiceModel;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.Data;

namespace VocaliRESTService
{
    public enum EstadoTranscripcion
    {
        Pendiente,
        EnProgreso,
        Realizada,
        Error,
    }

    [DataContract]
    public class Transcripcion
    {
        [DataMember(Order = 1)]
        public int IdTranscripcion { get; set; }
        [DataMember(Order = 2)]
        public string Login { get; set; }
        [DataMember(Order = 3)]
        public EstadoTranscripcion Estado { get; set; }
        [DataMember(Order = 4)]
        public string NombreFichero { get; set; }
        [DataMember(Order = 5)]
        public byte[] Fichero { get; set; }
        [DataMember(Order = 6)]
        public DateTime FechaRecepcion { get; set; }
        [DataMember(Order = 7)]
        public DateTime? FechaTranscripcion { get; set; }   // Declaramos como DateTime? para permitir valores null.
        [DataMember(Order = 8)]
        public string TextoTranscripcion { get; set; }

        public override string ToString()
        {
            return string.Format("Transcripcion {{IdTranscripcion: {0}, Login: {1}, Estado: {2}, NombreFichero: {3}, FechaRecepcion: {4}}}",
                IdTranscripcion, Login, Estado, NombreFichero, FechaRecepcion);
        }
    }
    [DataContract]
    public class TranscripcionCU2
    {
        [DataMember(Order = 1)]
        public string NombreFichero { get; set; }
        [DataMember(Order = 2)]
        public DateTime FechaRecepcion { get; set; }
        [DataMember(Order = 3)]
        public EstadoTranscripcion Estado { get; set; }
        [DataMember(Order = 4)]
        public DateTime? FechaTranscripcion { get; set; }
    }
    [DataContract]
    public class TranscripcionCU3
    {
        [DataMember(Order = 1)]
        public string Error { get; set; }
        [DataMember(Order = 2)]
        public string TextoTranscripcion { get; set; }
    }

    public interface ITranscripcionRepository
    {
        Transcripcion Insert(Transcripcion item);
        List<TranscripcionCU2> SelectByLoginFechaRecepcion(string login, DateTime? desdeFechaRecepcion, DateTime? hastaFechaRecepcion);
        TranscripcionCU3 SelectByIdCU3(int id);
        List<Transcripcion> SelectPendientes();
        bool Update(Transcripcion item);
        bool UpdateCU4ini(int idTranscripcion);
        bool UpdateCU4fin(int idTranscripcion, bool esError, DateTime fechaTranscripcion, string textoTranscripcion);
        List<Transcripcion> SelectAll();
        bool Delete(int id);
    }

    public class TranscripcionRepository : ITranscripcionRepository
    {
        TranscripcionDAL dal = new TranscripcionDAL();

        //CRUD Operations

        /// <summary>
        /// 1. CREATE
        /// </summary>
        /// <param name="transcripcion"></param>
        /// <returns></returns>
        public Transcripcion Insert(Transcripcion transcripcion)
        {
            return dal.Insert(transcripcion);
        }
        /// <summary>
        /// 2a. RETRIEVE /ALL
        /// </summary>
        /// <returns></returns>
        public List<Transcripcion> SelectAll()
        {
            return dal.SelectAll();
        }
        /// <summary>
        /// 2b. RETRIEVE /By IdTranscripcion
        /// </summary>
        /// <param name="idTranscripcion"></param>
        /// <returns></returns>
        public Transcripcion SelectById(int idTranscripcion)
        {
            return dal.Select(idTranscripcion);
        }
        /// <summary>
        /// 2c. RETRIEVE /By IdTranscripcion
        /// </summary>
        /// <param name="idTranscripcion"></param>
        /// <returns></returns>
        public TranscripcionCU3 SelectByIdCU3(int idTranscripcion)
        {
            TranscripcionCU3 transcripcionCU3 = new TranscripcionCU3();
            Transcripcion transcripcion = dal.Select(idTranscripcion);
            if (transcripcion == null)
            {
                transcripcionCU3.Error = "ErrCU3.FljA: Transcripción no encontrada.";
            }
            else
            {
                if (transcripcion.Estado == EstadoTranscripcion.Pendiente
                    || transcripcion.Estado == EstadoTranscripcion.EnProgreso)
                {
                    transcripcionCU3.Error = "ErrCU3.FljB: Transcripción no procesada.";
                }
                else if (transcripcion.Estado == EstadoTranscripcion.Error)
                {
                    transcripcionCU3.Error = "ErrCU3.FljC: Error al procesar la transcripción.";
                }
                else if (transcripcion.Estado == EstadoTranscripcion.Realizada)
                {
                    transcripcionCU3.TextoTranscripcion = transcripcion.TextoTranscripcion;
                }
            }

            return transcripcionCU3;
        }
        /// <summary>
        /// 2d. RETRIEVE /By Login FechaRecepcion
        /// </summary>
        /// <param name="login"></param>
        /// <param name="desdeFechaRecepcion"></param>
        /// <param name="hastaFechaRecepcion"></param>
        /// <returns></returns>
        public List<TranscripcionCU2> SelectByLoginFechaRecepcion(string login, DateTime? desdeFechaRecepcion, DateTime? hastaFechaRecepcion)
        {
            return dal.SelectByLoginFechaRecepcion(login, desdeFechaRecepcion, hastaFechaRecepcion);
        }
        /// <summary>
        /// 2e. RETRIEVE /By Estado Pendientes
        /// </summary>
        /// <returns></returns>
        public List<Transcripcion> SelectPendientes()
        {
            return dal.SelectPendientes();
        }
        /// <summary>
        /// 3a. UPDATE
        /// </summary>
        /// <param name="transcripcion"></param>
        /// <returns></returns>
        public bool Update(Transcripcion transcripcion)
        {
            if (transcripcion == null)
            { throw new ArgumentNullException("updatedTranscripcion"); }

            dal.Update(transcripcion);

            return true;
        }
        /// <summary>
        /// 3b. UPDATE
        /// </summary>
        /// <param name="idTranscripcion"></param>
        /// <returns></returns>
        public bool UpdateCU4ini(int idTranscripcion)
        {
            dal.UpdateCU4ini(idTranscripcion);
            Log.AppendText(string.Format("OK: UpdateCU4ini(int idTranscripcion:{0})",
                idTranscripcion));

            return true;
        }
        /// <summary>
        /// 3c. UPDATE
        /// </summary>
        /// <param name="idTranscripcion"></param>
        /// <param name="esError"></param>
        /// <param name="fechaTranscripcion"></param>
        /// <param name="textoTranscripcion"></param>
        /// <returns></returns>
        public bool UpdateCU4fin(int idTranscripcion, bool esError, DateTime fechaTranscripcion, string textoTranscripcion)
        {
            dal.UpdateCU4fin(idTranscripcion, esError, fechaTranscripcion, textoTranscripcion);
            Log.AppendText(string.Format("OK: UpdateCU4fin(int idTranscripcion:{0}, bool esError:{1}, DateTime fechaTranscripcion:{2}, string textoTranscripcion:{3})",
                idTranscripcion, esError, fechaTranscripcion, textoTranscripcion));

            return true;
        }
        /// <summary>
        /// 4. DELETE
        /// </summary>
        /// <param name="idTranscripcion"></param>
        /// <returns></returns>
        public bool Delete(int idTranscripcion)
        {
            dal.Delete(idTranscripcion);
            return true;
        }
    }

    public class TranscripcionDAL
    {
        private readonly string connStringBase = @"Data Source=.\NCS_2008;Initial Catalog=Company;Integrated Security=True";
        private string connString;
        private SqlConnection conn;
        private SqlCommand command;

        /// <summary>
        /// Constructor por defecto. Usando la cadena de conexión establecida en Web.config
        /// </summary>
        public TranscripcionDAL()
        {
            try
            {
                connString = System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            }
            catch
            {
                connString = connStringBase;
            }
        }
        /// <summary>
        /// Constructor usando la cadena de conexión especificada.
        /// </summary>
        /// <param name="connString"></param>
        public TranscripcionDAL(string connString)
        {
            this.connString = connString;
        }
        /// <summary>
        /// Inserta un registros con los valores del objeto Transcripcion especificado y devuelve el objeto con el IdTranscripcion asignado.
        /// </summary>
        /// <param name="transcripcion"></param>
        /// <returns></returns> 
        public Transcripcion Insert(Transcripcion transcripcion)
        {
            try
            {
                string sqlInserString = "INSERT INTO Transcripcion (Login, Estado, NombreFichero, Fichero, FechaRecepcion, FechaTranscripcion, TextoTranscripcion) VALUES (@Login, @Estado, @NombreFichero, @Fichero, @FechaRecepcion, @FechaTranscripcion, @TextoTranscripcion); "
                    + " SELECT SCOPE_IDENTITY();";

                //transcripcion.Fichero = new byte[] { 55, 56, 57 };
                conn = new SqlConnection(connString);
                command = new SqlCommand(sqlInserString, conn);
                command.Parameters.AddWithValue("Login", transcripcion.Login);
                command.Parameters.AddWithValue("Estado", transcripcion.Estado);
                command.Parameters.AddWithValue("NombreFichero", (object)transcripcion.NombreFichero ?? DBNull.Value);
                SqlParameter fichParam = command.Parameters.AddWithValue("Fichero", (object)transcripcion.Fichero ?? DBNull.Value);
                fichParam.DbType = DbType.Binary;

                command.Parameters.AddWithValue("FechaRecepcion", transcripcion.FechaRecepcion);
                command.Parameters.AddWithValue("FechaTranscripcion", (object)transcripcion.FechaTranscripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("TextoTranscripcion", (object)transcripcion.TextoTranscripcion ?? DBNull.Value);

                command.Connection.Open();
                int id = Numeros.ToInt(command.ExecuteScalar());
                command.Connection.Close();
                if (id > 0) { transcripcion.IdTranscripcion = id; }

                return transcripcion;
            }
            catch (Exception ex)
            {
                Log.AppendText(string.Format("Error TranscripcionDAL.Insert:\r\n{0}\r\n{1}"
                    , transcripcion
                    , ex.Message));
                throw;
            }
        }
        /// <summary>
        /// Actualiza el registro con los del objeto Transcripcion especificado según su IdTranscripcion.
        /// </summary>
        /// <param name="transcripcion"></param>
        public void Update(Transcripcion transcripcion)
        {
            try
            {
                string sqlUpdateString = "UPDATE Transcripcion SET Login = @Login, Estado = @Estado, NombreFichero = @NombreFichero, Fichero = @Fichero, FechaRecepcion = @FechaRecepcion, FechaTranscripcion = @FechaTranscripcion, TextoTranscripcion = @TextoTranscripcion WHERE IdTranscripcion = @IdTranscripcion";

                conn = new SqlConnection(connString);
                command = new SqlCommand(sqlUpdateString, conn);
                command.Parameters.AddWithValue("IdTranscripcion", transcripcion.IdTranscripcion);
                command.Parameters.AddWithValue("Login", transcripcion.Login);
                command.Parameters.AddWithValue("Estado", transcripcion.Estado);
                command.Parameters.AddWithValue("NombreFichero", (object)transcripcion.NombreFichero ?? DBNull.Value);
                SqlParameter fichParam = command.Parameters.AddWithValue("Fichero", (object)transcripcion.Fichero ?? DBNull.Value);
                fichParam.DbType = DbType.Binary;

                command.Parameters.AddWithValue("FechaRecepcion", transcripcion.FechaRecepcion);
                command.Parameters.AddWithValue("FechaTranscripcion", (object)transcripcion.FechaTranscripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("TextoTranscripcion", (object)transcripcion.TextoTranscripcion ?? DBNull.Value);

                command.Connection.Open();
                command.ExecuteNonQuery();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Log.AppendText(string.Format("Error TranscripcionDAL.Update:\r\n{0}\r\n{1}"
                    , transcripcion
                    , ex.Message));
                throw;
            }
        }
        /// <summary>
        /// Actualiza el registro con los del objeto Transcripcion especificado según su IdTranscripcion.
        /// </summary>
        /// <param name="idTranscripcion"></param>
        public void UpdateCU4ini(int idTranscripcion)
        {
            try
            {
                string sqlUpdateString = "UPDATE Transcripcion SET Estado = @Estado WHERE IdTranscripcion = @IdTranscripcion";

                conn = new SqlConnection(connString);
                command = new SqlCommand(sqlUpdateString, conn);
                command.Parameters.AddWithValue("IdTranscripcion", idTranscripcion);
                command.Parameters.AddWithValue("Estado", EstadoTranscripcion.EnProgreso);

                command.Connection.Open();
                command.ExecuteNonQuery();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Log.AppendText(string.Format("Error TranscripcionDAL.UpdateCU4ini:\r\nid:{0}\r\n{1}"
                    , idTranscripcion
                    , ex.Message));
                throw;
            }
        }
        /// <summary>
        /// Actualiza el registro con los del objeto Transcripcion especificado según su IdTranscripcion.
        /// </summary>
        /// <param name=""></param>
        /// <param name="idTranscripcion"></param>
        /// <param name="estado"></param>
        /// <param name="texto"></param>
        public void UpdateCU4fin(int idTranscripcion, bool esError, DateTime fechaTranscripcion, string textoTranscripcion)
        {
            try
            {
                string sqlUpdateString = "UPDATE Transcripcion SET Estado = @Estado, FechaTranscripcion = @FechaTranscripcion, TextoTranscripcion = @TextoTranscripcion WHERE IdTranscripcion = @IdTranscripcion";

                conn = new SqlConnection(connString);
                command = new SqlCommand(sqlUpdateString, conn);
                command.Parameters.AddWithValue("IdTranscripcion", idTranscripcion);
                command.Parameters.AddWithValue("Estado", esError ? EstadoTranscripcion.Error : EstadoTranscripcion.Realizada);
                command.Parameters.AddWithValue("FechaTranscripcion", fechaTranscripcion);
                command.Parameters.AddWithValue("TextoTranscripcion", (object)textoTranscripcion ?? DBNull.Value);

                command.Connection.Open();
                command.ExecuteNonQuery();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Log.AppendText(string.Format("Error TranscripcionDAL.UpdateCU4ini:\r\nid:{0}, err:{1}, fechatr:{2}, textotr:{3}\r\n{4}"
                    , idTranscripcion, esError, fechaTranscripcion, textoTranscripcion
                    , ex.Message));
                throw;
            }
        }
        /// <summary>
        /// Elimina el registro con el IdTranscripcion especificado.
        /// </summary>
        /// <param name="idTranscripcion"></param>
        public void Delete(int idTranscripcion)
        {
            try
            {
                string sqlDeleteString = "DELETE FROM Transcripcion WHERE IdTranscripcion = @IdTranscripcion";

                conn = new SqlConnection(connString);
                command = new SqlCommand(sqlDeleteString, conn);
                command.Parameters.AddWithValue("IdTranscripcion", idTranscripcion);
                command.Connection.Open();
                command.ExecuteNonQuery();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                Log.AppendText(string.Format("Error TranscripcionDAL.Delete:\r\nid:{0}\r\n{1}"
                    , idTranscripcion
                    , ex.Message));
                throw;
            }
        }
        /// <summary>
        /// Selecciona el registro con el IdTranscripcion especificado.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Transcripcion Select(int idTranscripcion)
        {
            try
            {
                Transcripcion transcripcion = null;
                string sqlSelectString = "SELECT IdTranscripcion, Login, Estado, NombreFichero, Fichero, FechaRecepcion, FechaTranscripcion, TextoTranscripcion FROM Transcripcion WHERE IdTranscripcion = @IdTranscripcion;";

                conn = new SqlConnection(connString);
                command = new SqlCommand(sqlSelectString, conn);
                command.Parameters.AddWithValue("IdTranscripcion", idTranscripcion);
                command.Connection.Open();


                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    transcripcion = new Transcripcion();
                    transcripcion.IdTranscripcion = Numeros.ToInt(reader["IdTranscripcion"]);
                    transcripcion.Login = reader["Login"].ToString();
                    transcripcion.Estado = (EstadoTranscripcion)Numeros.ToInt(reader["Estado"]);
                    transcripcion.NombreFichero = reader["NombreFichero"].ToString();
                    if (reader["Fichero"] != DBNull.Value) { transcripcion.Fichero = (byte[])reader["Fichero"]; }
                    transcripcion.FechaRecepcion = (DateTime)reader["FechaRecepcion"];
                    if (reader["FechaTranscripcion"] != DBNull.Value) { transcripcion.FechaTranscripcion = (DateTime?)reader["FechaTranscripcion"]; }
                    transcripcion.TextoTranscripcion = reader["TextoTranscripcion"].ToString();
                }
                command.Connection.Close();

                return transcripcion;
            }
            catch (Exception ex)
            {
                Log.AppendText(string.Format("Error TranscripcionDAL.Select:\r\nid:{0}\r\n{1}"
                    , idTranscripcion
                    , ex.Message));
                throw;
            }
        }
        /// <summary>
        /// Selecciona todos los registros de la tabla Transcripcion
        /// </summary>
        /// <returns></returns>
        public List<Transcripcion> SelectAll()
        {
            try
            {
                List<Transcripcion> lista = new List<Transcripcion>();
                string sqlSelectString = "SELECT IdTranscripcion, Login, Estado, NombreFichero, Fichero, FechaRecepcion, FechaTranscripcion, TextoTranscripcion FROM Transcripcion";

                conn = new SqlConnection(connString);
                command = new SqlCommand(sqlSelectString, conn);
                command.Connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Transcripcion transcripcion = new Transcripcion();
                    transcripcion = new Transcripcion();
                    transcripcion.IdTranscripcion = Numeros.ToInt(reader["IdTranscripcion"]);
                    transcripcion.Login = reader["Login"].ToString();
                    transcripcion.Estado = (EstadoTranscripcion)Numeros.ToInt(reader["Estado"]);
                    transcripcion.NombreFichero = reader["NombreFichero"].ToString();
                    if (reader["Fichero"] != DBNull.Value) { transcripcion.Fichero = (byte[])reader["Fichero"]; }
                    transcripcion.FechaRecepcion = (DateTime)reader["FechaRecepcion"];
                    if (reader["FechaTranscripcion"] != DBNull.Value) { transcripcion.FechaTranscripcion = (DateTime?)reader["FechaTranscripcion"]; }
                    transcripcion.TextoTranscripcion = reader["TextoTranscripcion"].ToString();
                    lista.Add(transcripcion);
                }
                command.Connection.Close();

                return lista;
            }
            catch (Exception ex)
            {
                Log.AppendText(string.Format("Error TranscripcionDAL.SelectAll:\r\n{0}"
                    , ex.Message));
                throw;
            }
        }
        /// <summary>
        /// Selecciona los registros pendientes para el envío al servicio de reconocimiento de voz.
        /// </summary>
        /// <returns></returns>
        public List<Transcripcion> SelectPendientes()
        {
            try
            {
                List<Transcripcion> lista = new List<Transcripcion>();

                conn = new SqlConnection(connString);

                string sqlSelectString = "SELECT IdTranscripcion, Login, Estado, NombreFichero, Fichero, FechaRecepcion, FechaTranscripcion, TextoTranscripcion FROM Transcripcion" +
                    " WHERE Estado = @Estado" +
                    " ORDER BY FechaRecepcion"; // Procesamos los registros pendientes por orden de recepción (FIFO).
                command = new SqlCommand(sqlSelectString, conn);
                command.Parameters.Add(new SqlParameter("@Estado", EstadoTranscripcion.Pendiente));
                command.Connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Transcripcion transcripcion = new Transcripcion();
                    transcripcion.IdTranscripcion = Numeros.ToInt(reader["IdTranscripcion"]);
                    transcripcion.Login = reader["Login"].ToString();
                    transcripcion.Estado = (EstadoTranscripcion)Numeros.ToInt(reader["Estado"]);
                    transcripcion.NombreFichero = reader["NombreFichero"].ToString();
                    if (reader["Fichero"] != DBNull.Value) { transcripcion.Fichero = (byte[])reader["Fichero"]; }
                    transcripcion.FechaRecepcion = (DateTime)reader["FechaRecepcion"];
                    if (reader["FechaTranscripcion"] != DBNull.Value) { transcripcion.FechaTranscripcion = (DateTime?)reader["FechaTranscripcion"]; }
                    transcripcion.TextoTranscripcion = reader["TextoTranscripcion"].ToString();
                    lista.Add(transcripcion);
                }
                command.Connection.Close();

                return lista;
            }
            catch (Exception ex)
            {
                Log.AppendText(string.Format("Error TranscripcionDAL.SelectPendientes:\r\n{0}"
                    , ex.Message));
                throw;
            }
        }
        /// <summary>
        /// Selecciona los registros del usuario e intervalo de fechas de recepción especificados.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="desdeFechaRecepcion"></param>
        /// <param name="hastaFechaRecepcion"></param>
        /// <returns></returns>
        public List<TranscripcionCU2> SelectByLoginFechaRecepcion(string login, DateTime? desdeFechaRecepcion, DateTime? hastaFechaRecepcion)
        {
            try
            {
                List<TranscripcionCU2> lista = new List<TranscripcionCU2>();
                string sqlSelectString = "SELECT NombreFichero, FechaRecepcion, Estado, FechaTranscripcion FROM Transcripcion WHERE" +
                    " Login = @Login AND" +
                    " (@DesdeFechaRecepcion IS NULL OR FechaRecepcion >= @DesdeFechaRecepcion) AND" +
                    " (@HastaFechaRecepcion IS NULL OR FechaRecepcion < @HastaFechaRecepcion)";

                conn = new SqlConnection(connString);
                command = new SqlCommand(sqlSelectString, conn);
                command.Parameters.Add(new SqlParameter("@Login", login));
                command.Parameters.Add(new SqlParameter("@DesdeFechaRecepcion", (object)desdeFechaRecepcion ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@HastaFechaRecepcion", (object)hastaFechaRecepcion ?? DBNull.Value));
                command.Connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    TranscripcionCU2 transcripcion = new TranscripcionCU2();
                    transcripcion.Estado = (EstadoTranscripcion)Numeros.ToInt(reader["Estado"]);
                    transcripcion.NombreFichero = reader["NombreFichero"].ToString();
                    transcripcion.FechaRecepcion = (DateTime)reader["FechaRecepcion"];
                    if (reader["FechaTranscripcion"] != DBNull.Value) { transcripcion.FechaTranscripcion = (DateTime?)reader["FechaTranscripcion"]; }
                    lista.Add(transcripcion);
                }
                command.Connection.Close();

                return lista;
            }
            catch (Exception ex)
            {
                Log.AppendText(string.Format("Error TranscripcionDAL.SelectByLoginFechaRecepcion:\r\nlogin:{0}, desdefecha:{1}, hastafecha:{2}\r\n{3}"
                    , login, desdeFechaRecepcion, hastaFechaRecepcion
                    , ex.Message));
                throw;
            }
        }
    }
}