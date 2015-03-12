using System;
using System.Collections.Generic;
using System.Linq;

namespace Objects
{
    public class DataSet
    {
        private Dictionary<string, List<string>> data;
        public int Count { get; private set; }

        #region Public Fields

        public IReadOnlyDictionary<string,string> this[int row]
        {
            get
            {
                return data.ToDictionary(c => c.Key, c => c.Value[row]) as IReadOnlyDictionary<string, string>;
            }
        }

        public string this[int row, string field]
        {
            get
            {
                return getColumn(field)[row];
            }
            set
            {
                getColumn(field)[row] = value;
            }
        }

        public IReadOnlyList<string> Columns
        {
            get
            {
                return data.Select(c => c.Key).ToList() as IReadOnlyList<string>;
            }
        }
        
        public IEnumerable<IReadOnlyDictionary<string, string>> Rows
        {
            get
            {
                return Enumerable.Range(0, Count).Select(i => this[i]);
            }
        }

        #endregion

        public DataSet()
        {
            data = new Dictionary<string, List<string>>();
        }

        public int Add()
        {
            Count++;
            foreach (var c in data)
            {
                c.Value.Add(null);
            }
            return Count - 1;
        }

        private List<string> getColumn(string field)
        {
            if (!data.ContainsKey(field))
            {
                data[field] = Enumerable.Repeat<string>(null, Count).ToList();
            }
            return data[field];
        }

        #region CSV

        public string toTable()
        {
            string html = "<table class='table'>";

            html += "<thead><tr><th>" + string.Join("</th><th>", data.Select(c => c.Key).Where(k=>!k.Contains("Wk"))) + "</th></tr></thead>";

            html += "<tbody>";
            foreach (int row in Enumerable.Range(0, Count))
            {
                if (bool.Parse(this[row, "Picked"])) continue;
                html += "<tr><td>" + string.Join("</td><td>", data.Where(k=>!k.Key.Contains("Wk")).Select(c => c.Value[row])) + "</td></tr>";
            }
            html += "</tbody>";
            html += "</table>";

            return html;
        }

        #endregion
    }
}
