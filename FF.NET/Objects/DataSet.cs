using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Objects
{
    public class DataSet
    {
        private Dictionary<string, List<string>> data;
        private int Count { get; set; }

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

        public void toCSV(string path)
        {
            File.Delete(path);

            using (var file = File.AppendText(path))
            {
                file.WriteLine(string.Join(",", data.Select(c => c.Key)));

                foreach (int row in Enumerable.Range(0, Count))
                {
                    file.WriteLine(string.Join(",", data.Select(c => c.Value[row])));
                }
            }
        }

        public static DataSet fromCSV(string path)
        {
            DataSet dataSet = new DataSet();

            var lines = File.ReadLines(path);

            string[] headers = lines.First().Split(',');
            dataSet.data = headers.ToDictionary(h => h, h => new List<string>());
            foreach (string line in lines.Skip(1))
            {
                string[] cells = line.Split(',');

                if (cells.Length != headers.Length)
                {
                    throw new Exception("Cell count doesn't match header count.");
                }

                int row = dataSet.Add();

                for (int i = 0; i < cells.Length; i++)
                {
                    dataSet.data[headers[i]][row] = cells[i];
                }
            }

            return dataSet;
        }

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
