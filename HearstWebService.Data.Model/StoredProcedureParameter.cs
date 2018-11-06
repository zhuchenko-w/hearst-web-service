using System.Data;

namespace HearstWebService.Data.Models
{
    public class StoredProcedureParameter
    {
        public string Name { get; set; }
        public SqlDbType Type { get; set; }
        public object Value { get; set; }
    }
}
