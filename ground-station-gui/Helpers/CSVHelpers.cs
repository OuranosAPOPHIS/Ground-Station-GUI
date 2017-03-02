using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APOPHIS.GroundStation.Helpers
{
    static class CSVHelpers
    {
        public static string ToCSV<T>(this IEnumerable<T> objList, string separator = ",")
        {
            Type t = typeof(T);
            FieldInfo[] fields = t.GetFields();

            string header = ToCSVHeader<T>(separator, fields);

            StringBuilder csvdata = new StringBuilder();
            csvdata.AppendLine(header);

            foreach (var obj in objList)
            {
                csvdata.AppendLine(obj.ToCSV<T>(separator, fields));
            }

            return csvdata.ToString();
        }
        public static string ToCSV<T>(this object obj, string separator = ",", FieldInfo[] fields = null)
        {
            if (fields == null) fields = typeof(T).GetFields();

            StringBuilder line = new StringBuilder();

            foreach (var f in fields)
            {
                if (line.Length > 0) line.Append(separator);

                var x = f.GetValue(obj);

                if (x != null) line.Append(x.ToString());
            }

            return line.ToString();
        }

        public static string ToCSVHeader<T>(string separator = ",", FieldInfo[] fields = null)
        {
            if (fields == null) fields = typeof(T).GetFields();
            return string.Join(separator, fields.Select(f => f.Name).ToArray());
        }        
    }
}
