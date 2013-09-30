using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storm
{
    public class QueryBuilder<T>
    {
        private Dictionary<string, string> queries;
        private Mapper mapService;
        protected TableMapper<T> table;
        private StringBuilder sb;

        public QueryBuilder()
            : this(new Mapper())
        {
        }

        public QueryBuilder(Mapper _mapService)
        {
            queries = new Dictionary<string, string>();
            this.mapService = _mapService;
            this.table = this.mapService.GetTableMapper<T>();
        }

        /// <summary>
        /// appends all array with commas to seperate.. useful for many cases in SQL
        /// </summary>
        /// <param name="values"></param>
        private void AppendArrayWithCommas(IEnumerable<string> values)
        {
            foreach (var col in values)
            {
                sb.AppendFormat(" {0},", col);
            }

            sb.Remove(sb.Length - 1, 1);
        }

        /// <summary>
        /// return all properties value in the same order of columns
        /// </summary>
        /// <param name="Obj">the object to extract the values from</param>
        /// <returns></returns>
        private Dictionary<string, object> GetObjectValues(T Obj)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            foreach (var prop in typeof(T).GetProperties())
            {
                values.Add(prop.Name, prop.GetValue(Obj));
            }

            return values;
        }

        /// <summary>
        /// generating select query from table mapper object
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public string GenerateSelect(string filter = "", string order = "")
        {
            if (!queries.ContainsKey("GenerateSelect"))
            {
                sb = new StringBuilder();
                sb.AppendFormat("SELECT * FROM {0}", table.Table);
                queries.Add("GenerateSelect", sb.ToString());
            }
            else
                sb = new StringBuilder(queries["GenerateSelect"]);

            if (!string.IsNullOrEmpty(filter))
                sb.AppendFormat(" WHERE {0}", filter);

            if (!string.IsNullOrEmpty(order))
                sb.AppendFormat(" ORDER BY {0}", order);

            return sb.ToString();
        }

        /// <summary>
        /// generating select query from table mapper object
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public string GenerateInsert(string filter = "")
        {
            if (!queries.ContainsKey("GenerateInsert"))
            {
                sb = new StringBuilder();
                sb.AppendFormat("INSERT INTO {0} ", table.Table);

                sb.AppendFormat(" (");
                this.AppendArrayWithCommas(table.Columns);
                sb.AppendFormat(") ");

                sb.AppendFormat(" VALUES ", table.Table);

                sb.AppendFormat(" (");
                this.AppendArrayWithCommas(table.Columns.Select(s => string.Format("@{0}", s)));
                sb.AppendFormat(") ");

                queries.Add("GenerateInsert", sb.ToString());
            }
            else
                sb = new StringBuilder(queries["GenerateInsert"]);

            if (!string.IsNullOrEmpty(filter))
                sb.AppendFormat(" WHERE {0}", filter);

            return sb.ToString();
        }

        /// <summary>
        /// generating select query from table mapper object
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public string GenerateUpdate(string filter = "")
        {
            if (!queries.ContainsKey("GenerateUpdate"))
            {
                sb = new StringBuilder();
                sb.AppendFormat("UPDATE {0} SET ", table.Table);

                this.AppendArrayWithCommas(table.Columns.Select(s => string.Format("{0} = @{0}", s)));

                queries.Add("GenerateUpdate", sb.ToString());
            }
            else
                sb = new StringBuilder(queries["GenerateUpdate"]);

            if (!string.IsNullOrEmpty(filter))
                sb.AppendFormat(" WHERE {0}", filter);
            else
                sb.AppendFormat(" WHERE {0} = @{0}", table.Id);

            return sb.ToString();
        }

        /// <summary>
        /// generating delete query from table mapper object
        /// </summary>
        /// <param name="filter">where statement</param>
        /// <returns></returns>
        public string GenerateDelete(string filter = "")
        {
            if (!queries.ContainsKey("GenerateDelete"))
            {
                sb = new StringBuilder();
                sb.AppendFormat("DELETE {0} ", table.Table);

                queries.Add("GenerateDelete", sb.ToString());
            }
            else
                sb = new StringBuilder(queries["GenerateDelete"]);

            if (!string.IsNullOrEmpty(filter))
                sb.AppendFormat(" WHERE {0}", filter);
            else
                sb.AppendFormat(" WHERE {0} = @{0}", table.Id);

            return sb.ToString();
        }

        /// <summary>
        /// return a paging query of a particular query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public string GeneratePaged(string filter = "", string order = "")
        {
            if (!queries.ContainsKey("GeneratePaged"))
            {
                sb = new StringBuilder();

                //order and filter formating
                order = string.IsNullOrEmpty(order) ? table.Id : string.Format("{0}", order);
                filter = string.IsNullOrEmpty(filter) ? string.Empty : string.Format("WHERE {0}", filter);

                sb.AppendLine("SELECT ");
                this.AppendArrayWithCommas(table.Columns.Select(s => string.Format("[t1].{0}", s)));
                sb.AppendFormat(" FROM (SELECT ROW_NUMBER() OVER (ORDER BY {0}) AS [ROW_NUMBER], ", order);
                this.AppendArrayWithCommas(table.Columns.Select(s => string.Format("[t0].{0}", s)));
                sb.AppendFormat(" FROM [dbo].[{0}] AS [t0] ) AS [t1]", table.Table);
                sb.AppendLine(" WHERE [t1].[ROW_NUMBER] BETWEEN @PageNum + 1 AND @PageNum + @PageSize");
                sb.AppendLine(" ORDER BY [t1].[ROW_NUMBER]");

                queries.Add("GeneratePaged", sb.ToString());
            }

            return queries["GeneratePaged"];
        }
    }
}
