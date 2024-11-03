using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhipWeb.Models
{
    // This the data used to initialize a DataTable object for Google Charts.  It will be serialized to JSON automatically.
    //
    // https://developers.google.com/chart/interactive/docs/reference#DataTable
    public class DataTable
    {
        public List<ColInfo> cols { get; set; }
        public List<DataPointSet> rows { get; set; }
        public Dictionary<string, string> p { get; set; }
    }

    public class ColInfo
    {
        public string id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string role { get; set; }
        public Dictionary<string, string> p { get; set; }
    }

    public class DataPointSet
    {
        public DataPoint[] c { get; set; }
    }

    public class DataPoint
    {
        public string v { get; set; }   // Value
        public string f { get; set; }   // Format
        public Dictionary<string, string> p { get; set; }   // Cell-level properties
    }

    // Interal point object for stacked ScatterChart
    public class Point
    {
        public int Id { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public string Label { get; set; }
        public string Series { get; set; }
    }
}
