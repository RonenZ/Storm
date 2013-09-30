using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storm
{
    public class Database
    {
        private DataAccess Dal;
        private Mapper mapService;
        private List<TableMapper<object>> tablesList;
        private Dictionary<Type, object> qbHash;

        /// <summary>
        /// Database object constructor
        /// </summary>
        /// <param name="ConStrName">Data Source Connection String Name in Web.Config</param>
        public Database(string ConStrName)
            : this(ConfigurationManager.ConnectionStrings[ConStrName].ConnectionString, ConfigurationManager.ConnectionStrings[ConStrName].ProviderName) { }

        /// <summary>
        /// Database object constructor
        /// </summary>
        /// <param name="ConnectionString">Data source connection string</param>
        /// <param name="ProviderName">Data source provider name</param>
        public Database(string ConnectionString, string ProviderName)
        {
            this.qbHash = new Dictionary<Type, object>();
            this.Dal = new DataAccess(ConnectionString, ProviderName);
            this.mapService = new Mapper();
            this.tablesList = new List<TableMapper<object>>();
        }



        #region CRUD

        /// <summary>
        /// Simple Generic Select query, returns Enumeration of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter">Where clause - without the word where</param>
        /// <param name="order">Order By clause - without the word order</param>
        /// <param name="prms">list of parameters @0, @1, @2...</param>
        /// <returns></returns>
        public IEnumerable<T> Select<T>(string filter = "", string order = "", params object[] prms)
            where T : class, new()
        {
            QueryBuilder<T> currQb;
            if(qbHash.ContainsKey(typeof(T)))
                currQb = (QueryBuilder<T>)qbHash[typeof(T)];
            else{
                currQb = new QueryBuilder<T>(mapService);
                qbHash.Add(typeof(T), currQb);
            }
            string query = currQb.GenerateSelect(filter, order);

            DbDataReader reader =  Dal.QueryWithParameters(query, Dal.CreateParams(prms));

            TableMapper<T> table = mapService.GetTableMapper<T>();

            IEnumerable<T> entities = mapService.ParseMultiple<T>(reader, table);

            Dal.Close();

            return entities;
        }

        /// <summary>
        /// Select query that yields paged list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageNum">Page Index = 1, 2, 3, 4</param>
        /// <param name="pageSize">how many elements in that page?</param>
        /// <param name="filter">Where clause - without the word where</param>
        /// <param name="order">Order By clause - without the word order</param>
        /// <param name="prms">list of parameters @0, @1, @2...</param>
        /// <returns></returns>
        public IEnumerable<T> Paged<T>(int pageNum, int pageSize, string filter = "", string order = "", params object[] prms)
            where T : class, new()
        {
            QueryBuilder<T> currQb;

            if (qbHash.ContainsKey(typeof(T)))
                currQb = (QueryBuilder<T>)qbHash[typeof(T)];
            else
            {
                currQb = new QueryBuilder<T>(mapService);
                qbHash.Add(typeof(T), currQb);
            }

            string query = currQb.GeneratePaged(filter, order);

            DbParameter pNum = Dal.CreateParam("PageNum", pageNum);
            DbParameter pSize = Dal.CreateParam("PageSize", pageSize);

            DbDataReader reader = Dal.QueryWithParameters(query, pNum, pSize);

            TableMapper<T> table = mapService.GetTableMapper<T>();

            IEnumerable<T> entities = mapService.ParseMultiple<T>(reader, table);

            Dal.Close();

            return entities;
        }

        /// <summary>
        /// Simple Generic Select query, returns Enumeration of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter">Where clause - without the word where</param>
        /// <param name="order">Order By clause - without the word order</param>
        /// <param name="prms">list of parameters @0, @1, @2...</param>
        /// <returns></returns>
        public IEnumerable<T> Fetch<T>(string filter = "", string order = "", params object[] prms)
            where T : class, new()
        {
            QueryBuilder<T> currQb;
            if (qbHash.ContainsKey(typeof(T)))
                currQb = (QueryBuilder<T>)qbHash[typeof(T)];
            else
            {
                currQb = new QueryBuilder<T>(mapService);
                qbHash.Add(typeof(T), currQb);
            }
            string query = currQb.GenerateSelect(filter, order);

            DbDataReader reader = Dal.QueryWithParameters(query, Dal.CreateParams(prms));

            TableMapper<T> table = mapService.GetTableMapper<T>();

            IEnumerable<T> entities = mapService.ParseMultiple<T>(reader, table);

            Dal.Close();

            return entities;
        }

        #endregion
    }
}
