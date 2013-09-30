using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storm
{
    public class DataAccess : IDisposable
    {
        #region Private Variables
        protected string conStr;
        protected string prName;
        private DbConnection cn;
        private DbCommand com;
        private DbProviderFactory factory;
        #endregion

        /// <summary>
        /// Basic DAL object to communicate with data source
        /// </summary>
        /// <param name="ConStrName">Data Source Connection String Name in Web.Config</param>
        public DataAccess(string ConStrName)
            : this(ConfigurationManager.ConnectionStrings[ConStrName].ConnectionString, ConfigurationManager.ConnectionStrings[ConStrName].ProviderName) { }

        /// <summary>
        /// Basic DAL object to communicate with data source
        /// </summary>
        /// <param name="ConnectionString">Data source connection string</param>
        /// <param name="ProviderName">Data source provider name</param>
        public DataAccess(string ConnectionString, string ProviderName)
        {
            this.conStr = ConnectionString;
            this.prName = ProviderName;

            factory = DbProviderFactories.GetFactory(prName);
            cn = factory.CreateConnection();
            com = factory.CreateCommand();
            com.Connection = cn;
            cn.ConnectionString = conStr;
        }

        /// <summary>
        /// returns DbType according to object type..
        /// </summary>
        /// <param name="Obj">object to be checked..</param>
        /// <returns></returns>
        public DbType GetDbType(object Obj)
        {
            DbType dbt;

            //figuring out what DbType to send...
            if (Obj is int)
                dbt = DbType.Int32;
            else if (Obj is string)
                dbt = DbType.String;
            else if (Obj is DateTime)
                dbt = DbType.DateTime;
            else if (Obj is bool)
                dbt = DbType.Boolean;
            else if (Obj is double)
                dbt = DbType.Double;
            else if (Obj is float)
                dbt = DbType.Decimal;
            else
                dbt = DbType.Object;

            return dbt;
        }

        /// <summary>
        /// reutrns a DbParameter with the sent properties
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <param name="Type">Db type</param>
        /// <param name="Value">Value</param>
        /// <returns></returns>
        public DbParameter CreateParam(string Name, DbType Type, object Value)
        {
            DbParameter prm = factory.CreateParameter();
            prm.DbType = Type;
            prm.ParameterName = Name;
            prm.Value = Value;

            return prm;
        }

        /// <summary>
        /// reutrns a DbParameter with the sent properties
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <param name="Value">Value</param>
        /// <returns></returns>
        public DbParameter CreateParam(string Name, object Value)
        {
            DbType dbt = GetDbType(Value);

            return CreateParam(Name, dbt, Value);
        }

        /// <summary>
        /// reutrns a set of DbParameters with the sent properties
        /// </summary>
        /// <param name="Name">Parameter name</param>
        /// <param name="Value">Value</param>
        /// <returns></returns>
        public DbParameter[] CreateParams(IEnumerable<object> Values)
        {
            DbParameter[] resultArr = new DbParameter[Values.Count()];
            DbType dbt;
            int i = 0;

            foreach (var val in Values)
            {
                dbt = GetDbType(val);
                resultArr[i] = CreateParam(string.Format("@{0}", i++), dbt, val);
            }

            return resultArr;
        }

        /// <summary>
        /// opening connection
        /// </summary>
        public void Open()
        {
            if (cn.State != ConnectionState.Open)
                cn.Open();
        }

        /// <summary>
        /// query db 
        /// </summary>
        /// <param name="commandText">Select Query/Command Text</param>
        /// <param name="Params">DbParameters</param>
        /// <returns></returns>
        public DbDataReader ExecuteQuery(string commandText)
        {

            com.CommandText = commandText;
            Open();

            return com.ExecuteReader();
        }

        /// <summary>
        /// query db with command text and dbparameters
        /// </summary>
        /// <param name="commandText">Select Query/Command Text</param>
        /// <param name="Params">DbParameters</param>
        /// <returns></returns>
        public DbDataReader QueryWithParameters(string commandText, params DbParameter[] Params)
        {
            try
            {
                com.Parameters.AddRange(Params);

                return ExecuteQuery(commandText);
            }
            finally
            {
                com.Parameters.Clear();
            }
        }

        /// <summary>
        /// execute insert/update/delete commands
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string commandText, params DbParameter[] Params)
        {
            try
            {
                com.Parameters.AddRange(Params);
                com.CommandText = commandText;
                return com.ExecuteNonQuery();
            }
            finally
            {
                com.Parameters.Clear();
            }
        }

        /// <summary>
        /// execute Stored Procedures
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public DbDataReader ExecuteSP(string spName, params DbParameter[] Params)
        {
            try
            {
                com.CommandType = CommandType.StoredProcedure;
                return QueryWithParameters(spName, Params);
            }
            finally
            {
                com.CommandType = CommandType.Text;
            }
        }

        /// <summary>
        /// closing connection
        /// </summary>
        public void Close()
        {
            if (cn.State != ConnectionState.Closed)
                cn.Close();
        }

        /// <summary>
        /// closing Dbconnection and disposing of objects...
        /// </summary>
        public void Dispose()
        {
            Close();
            
            if(com != null)
                com.Dispose();

            if(cn != null)
                cn.Dispose();
        }

        ///// <summary>
        ///// distructor function...
        ///// </summary>
        //~DataAccess()
        //{
        //    this.Dispose();
        //}
    }
}
