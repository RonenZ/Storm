using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Storm
{
    public class TableMapper<T>
    {
        public string Table { get; internal set; }
        public string Id { get; internal set; }
        public IEnumerable<string> Columns { get; internal set; }
        public IDictionary<PropertyInfo, string> FieldsMapping { get; internal set; }

        public TableMapper()
        {
        }

        public TableMapper(string _Table, string _Id, string[] _Columns, IDictionary<PropertyInfo, string> _FieldsMapping)
        {
            this.Table = _Table;
            this.Id = _Id;
            this.Columns = _Columns;
            this.FieldsMapping = _FieldsMapping;
        }
    }
}
