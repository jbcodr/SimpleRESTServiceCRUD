using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ServiceModel;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.Data;

namespace SimpleRESTServiceCRUD
{
    [DataContract]
    public class Transcripcion
    {
        public enum EstadoTranscripcion
        {
            Pendiente,
            EnProceso,
            Realizada,
            Error,
        }

        [DataMember]
        public int IdTranscripcion { get; set; }
        [DataMember]
        public string Login { get; set; }
        [DataMember]
        public EstadoTranscripcion Estado { get; set; }
        [DataMember]
        public string NombreFichero { get; set; }
        [DataMember]
        public byte[] Fichero { get; set; }
        [DataMember]
        public DateTime FechaRecepcion { get; set; }
        [DataMember]
        public DateTime? FechaTranscripcion { get; set; }   // Declaramos como DateTime? para permitir valores null.
        [DataMember]
        public string TextoTranscripcion { get; set; }
    }

    public interface ITranscripcionRepository
    {
        List<Transcripcion> SelectAll();
        Transcripcion SelectById(int id);
        List<Transcripcion> SelectByLoginFechaRecepcion(string login, DateTime? desdeFechaRecepcion, DateTime? hastaFechaRecepcion);
        List<Transcripcion> SelectPendientes();
        Transcripcion Insert(Transcripcion item);
        bool Delete(int id);
        bool Update(Transcripcion item);
    }

    public class TranscripcionRepository : ITranscripcionRepository
    {
        TranscripcionDAL dal = new TranscripcionDAL();
        //CRUD Operations
        //1. CREAT
        public Transcripcion Insert(Transcripcion transcripcion)
        {
            if (transcripcion == null)
            { throw new ArgumentNullException("transcripcion"); }

            dal.Insert(transcripcion);
            return transcripcion;
        }

        //2. RETRIEVE /ALL
        public List<Transcripcion> SelectAll()
        {
            return dal.SelectAll();
        }

        //3. RETRIEVE /By TranscripcionId
        public Transcripcion SelectById(int idTranscripcion)
        {
            return dal.Select(idTranscripcion);
        }

        //4. RETRIEVE /By Login FechaRecepcion
        public List<Transcripcion> SelectByLoginFechaRecepcion(string login, DateTime? desdeFechaRecepcion, DateTime? hastaFechaRecepcion)
        {
            return dal.SelectByLoginFechaRecepcion(login, desdeFechaRecepcion, hastaFechaRecepcion);
        }

        //5. RETRIEVE /By Estado Pendientes
        public List<Transcripcion> SelectPendientes()
        {
            return dal.SelectPendientes();
        }

        //4. UPDATE
        public bool Update(Transcripcion transcripcion)
        {
            if (transcripcion == null)
            { throw new ArgumentNullException("updatedTranscripcion"); }

            dal.Update(transcripcion);

            return true;
        }

        //5. DELETE
        public bool Delete(int idTranscripcion)
        {
            dal.Delete(idTranscripcion);
            return true;
        }
    }

    public class TranscripcionDAL
    {
        private SqlConnection conn;
        private string connString;
        private SqlCommand command;

        public TranscripcionDAL() : this(System.Configuration.ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString)
        {; }
        public TranscripcionDAL(string connString)
        {
            this.connString = connString;
        }
        /// <summary>
        /// Database INSERT Transcripcion
        /// </summary>
        /// <param name="transcripcion"></param>
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
                throw;
            }
        }
        /// <summary>
        /// Database UPDATE Transcripcion
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
                throw;
            }
        }
        /// <summary>
        /// Database DELETE Transcripcion
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
                throw;
            }
        }
        /// <summary>
        /// Database SELECT Transcripcion
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
                    transcripcion.Estado = (Transcripcion.EstadoTranscripcion)Numeros.ToInt(reader["Estado"]);
                    transcripcion.NombreFichero = reader["NombreFichero"].ToString();
                    if (reader["Fichero"] != DBNull.Value) { transcripcion.Fichero = (byte[])reader["Fichero"]; }
                    transcripcion.FechaRecepcion = (DateTime)reader["FechaRecepcion"];
                    if (reader["FechaTranscripcion"] != DBNull.Value) { transcripcion.FechaTranscripcion = (DateTime?)reader["FechaTranscripcion"]; }
                    transcripcion.TextoTranscripcion = reader["TextoTranscripcion"].ToString();
                }
                command.Connection.Close();

                //SqlDataReader dataReader = command.ExecuteReader();
                //DataTable dt = new DataTable("Table1");
                //if (dataReader != null) { dt.Load(dataReader); }
                //command.Connection.Close();
                //if (dt.Rows.Count > 0)
                //{
                //    DataRow dr = dt.Rows[0];
                //    transcripcion = new Transcripcion();
                //    transcripcion.IdTranscripcion = Numeros.ToInt(dr["IdTranscripcion"]);
                //    transcripcion.Login = dr["Login"].ToString();
                //    transcripcion.Fichero = (byte[])dr["Fichero"];

                //}

                return transcripcion;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// Database SELECT Transcripcion
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public List<Transcripcion> SelectAll()
        {
            //try
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
                    transcripcion.Estado = (Transcripcion.EstadoTranscripcion)Numeros.ToInt(reader["Estado"]);
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
            //catch (Exception ex)
            //{
            //    throw;
            //}
        }
        /// <summary>
        /// Database SELECT with filters Transcripcion Estado = Pendiente (0)
        /// </summary>
        /// <returns>Transcripcion</returns>
        public List<Transcripcion> SelectPendientes()
        {
            try
            {
                List<Transcripcion> lista = new List<Transcripcion>();

                conn = new SqlConnection(connString);

                string sqlSelectString = "SELECT IdTranscripcion, Login, Estado, NombreFichero, Fichero, FechaRecepcion, FechaTranscripcion, TextoTranscripcion FROM Transcripcion WHERE" +
                    " Estado = @Estado";
                command = new SqlCommand(sqlSelectString, conn);
                command.Parameters.Add(new SqlParameter("@Estado", Transcripcion.EstadoTranscripcion.Pendiente));
                command.Connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Transcripcion transcripcion = new Transcripcion();
                    transcripcion.IdTranscripcion = Numeros.ToInt(reader["IdTranscripcion"]);
                    transcripcion.Login = reader["Login"].ToString();
                    transcripcion.Estado = (Transcripcion.EstadoTranscripcion)Numeros.ToInt(reader["Estado"]);
                    transcripcion.NombreFichero = reader["NombreFichero"].ToString();
                    transcripcion.Fichero = (byte[])reader["Fichero"];
                    transcripcion.FechaRecepcion = (DateTime)reader["FechaRecepcion"];
                    transcripcion.FechaTranscripcion = (DateTime?)reader["FechaTranscripcion"];
                    transcripcion.TextoTranscripcion = reader["TextoTranscripcion"].ToString();
                    lista.Add(transcripcion);
                }
                command.Connection.Close();

                return lista;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// Database SELECT with filters Login FechaRecepcion
        /// </summary>
        /// <returns>Transcripcion</returns>
        public List<Transcripcion> SelectByLoginFechaRecepcion(string login, DateTime? desdeFechaRecepcion, DateTime? hastaFechaRecepcion)
        {
            try
            {
                List<Transcripcion> lista = new List<Transcripcion>();
                string sqlSelectString = "SELECT IdTranscripcion, Login, Estado, NombreFichero, Fichero, FechaRecepcion, FechaTranscripcion, TextoTranscripcion FROM Transcripcion WHERE" +
                    " Login = @Login AND" +
                    " (@DesdeFechaRecepcion IS NULL OR FechaRecepcion >= @DesdeFechaRecepcion)" +
                    " (@HastaFechaRecepcion IS NULL OR FechaRecepcion >= @HastaFechaRecepcion)";

                conn = new SqlConnection(connString);
                command = new SqlCommand(sqlSelectString, conn);
                command.Parameters.Add(new SqlParameter("@Login", login));
                command.Parameters.Add(new SqlParameter("@DesdeFechaRecepcion", (object)desdeFechaRecepcion ?? DBNull.Value));
                command.Parameters.Add(new SqlParameter("@HastaFechaRecepcion", (object)hastaFechaRecepcion ?? DBNull.Value));
                command.Connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Transcripcion transcripcion = new Transcripcion();
                    transcripcion.IdTranscripcion = Numeros.ToInt(reader["IdTranscripcion"]);
                    transcripcion.Login = reader["Login"].ToString();
                    transcripcion.Estado = (Transcripcion.EstadoTranscripcion)Numeros.ToInt(reader["Estado"]);
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
                throw;
            }
        }
    }
}