using Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Data.Csv
{
	public static class DataSetCsvReaderWriter
	{
		public static void toCSV(DataSet data,string path)
		{
			File.Delete(path);

			using (var file = File.AppendText(path))
			{
				file.WriteLine(string.Join(",", data.Columns));

				foreach (int row in Enumerable.Range(0, data.Count))
				{
					file.WriteLine(string.Join(",", data.Columns.Select(c => data[row, c])));
				}
			}
		}

		public static DataSet fromCSV(string path)
		{
			DataSet dataSet = new DataSet();

			var lines = File.ReadLines(path);

			string[] headers = lines.First().Split(',');
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
					dataSet[row, headers[i]] = cells[i];
				}
			}

			return dataSet;
		}
	}
}
