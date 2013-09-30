using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Storm
{
    public class Mapper
    {
        private static int counter = 0;
        /// <summary>
        /// returns enumeration of T from DbDataReader
        /// </summary>
        /// <returns></returns>
        public T Parse<T>(DbDataReader reader, TableMapper<T> table) where T : class, new()
        {
            if (!reader.HasRows)
                return null;

            counter = 0;
            T entity = new T();
            int ordinal = 0;
            foreach (var field in table.FieldsMapping)
            {
                ordinal = reader.GetOrdinal(field.Value);

                if (ordinal < 0)
                    continue;

                Type t = reader.GetFieldType(ordinal);

                try
                {
                    if (t.Name.Equals("DBNull"))
                        continue;
                    field.Key.SetValue(entity, reader[field.Value]);
                }
                catch { }
            }

            return entity;
        }

        /// <summary>
        /// returns enumeration of T from DbDataReader
        /// </summary>
        /// <returns></returns>
        public T ParseSingle<T>(DbDataReader reader, TableMapper<T> table) where T : class, new()
        {
            counter = 0;
            reader.Read();

            return Parse(reader, table);
        }

        /// <summary>
        /// returns enumeration of T from DbDataReader
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> ParseMultiple<T>(DbDataReader reader, TableMapper<T> table) where T : class, new()
        {
            List<T> entities = new List<T>();

            while (reader.Read())           
            {
                T entity = this.Parse(reader, table);
                
                if (entity == null)
                    continue;

                entities.Add(entity);
            }

            return entities;
        }

        /// <summary>
        /// returns attribute value, seeking attribute array for a specific name
        /// </summary>
        /// <param name="attList">Array of Custom Attributes</param>
        /// <param name="attName">name to seek for in attributes</param>
        /// <returns></returns>
        private string GetAttributeValueByName(IEnumerable<CustomAttributeData> attList, string attName)
        {
            CustomAttributeData result = attList.Where(w => w.AttributeType.Name == attName).FirstOrDefault();

            if (result == null || result.ConstructorArguments.Count == 0)
                return string.Empty;

            return result.ConstructorArguments[0].Value.ToString();
        }


        /// <summary>
        /// returns Id attribute value (mapping Id name)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private string GetIdentityName<T>()
        {
            string attValue = string.Empty;
            IEnumerable<PropertyInfo> props = typeof(T).GetProperties();

            //searching for key attribute
            foreach (var p in props)
            {
                attValue = GetAttributeValueByName(p.CustomAttributes, "KeyAttribute");

                if (!string.IsNullOrEmpty(attValue))
                    return attValue;
            }

            string propName, containId = string.Empty;

            //if no key attribute searching Id or first name containing Id
            foreach (var p in props)
            {
                propName = p.Name.Trim().ToLower();

                if (propName.Equals("id"))
                    return p.Name;

                if (string.IsNullOrEmpty(containId) && propName.Contains("id"))
                    containId = propName;
            }

            return containId;
        }

        /// <summary>
        /// returns table attribute value (mapping table name)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>table attribute value</returns>
        private string GetTableName<T>()
        {
            IEnumerable<CustomAttributeData> custAtts = typeof(T).CustomAttributes;
            string attValue = GetAttributeValueByName(custAtts, "TableAttribute");

            if (string.IsNullOrEmpty(attValue))
                return typeof(T).Name;

            return attValue;
        }

        /// <summary>
        /// returns columns names array
        /// </summary>
        /// <returns>string array of columns</returns>
        private IDictionary<PropertyInfo, string> GetColumns<T>()
        {
            Dictionary<PropertyInfo, string> result = new Dictionary<PropertyInfo, string>();

            //reviewing all properties in class
            IEnumerable<PropertyInfo> props = typeof(T).GetProperties();

            string attValue;

            foreach (var p in props)
            {
                attValue = this.GetAttributeValueByName(p.CustomAttributes, "ColumnAttribute");

                if (string.IsNullOrEmpty(attValue))
                    result.Add(p, p.Name);
                else
                    result.Add(p, attValue);
            }

            return result;
        }

        /// <summary>
        /// return an TableMapper object with class table mapping data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TableMapper<T> GetTableMapper<T>()
        {
            TableMapper<T> result = new TableMapper<T>();
            result.Table = this.GetTableName<T>();
            result.Id = this.GetIdentityName<T>();
            result.FieldsMapping = this.GetColumns<T>();
            result.Columns = result.FieldsMapping.Select(s => s.Value).ToArray();

            return result;
        }
    }
}
