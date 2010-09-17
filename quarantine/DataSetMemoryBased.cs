using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MyMediaProject.RecommenderSystem.Framework.EntityRelationAlgorithm;

namespace MyMediaLite.util
{
    /// <summary>
    /// Memory-based implementation of the relational data reader.
    /// The implementation is for experimental purposes only.
    /// It works with CSV files; no database is required.
    /// </summary>
    /// <author>Steffen Rendle, Zeno Gantner, University of Hildesheim</author>
    public class DataSetMemoryBased : IEntityRelationDataReader
    {
        public enum DataType
        {
            dt_undef  = 0,
            dt_int    = 1,
            dt_double = 2,
        }

        public class DataArray
        {
            public virtual void setSize(int row_count) {}
            public virtual void read(int row_index, string s) {}
        }

		public class DataArrayInteger: DataArray
        {
            public int[] data;
            public override void setSize(int row_count) {
                data = new int[row_count];
            }
            public override void read(int row_index, string s) {
				try
				{
                	data[row_index] = Convert.ToInt32(s);
				}
				catch (Exception)
				{
					Console.Error.WriteLine("'{0}'", s );
					throw;
				}
            }
        }

		public class DataArrayDouble: DataArray
        {
            public double[] data;
            public override void setSize(int row_count) {
                data = new double[row_count];
            }
            public override void read(int row_index, string s) {
				try
				{
                	data[row_index] = Convert.ToDouble(s, System.Globalization.CultureInfo.InvariantCulture);
				}
				catch (Exception)
				{
					Console.Error.WriteLine("'{0}'", s );
					throw;
				}
            }
        }

        public class DataStorage
        {
            public List<DataArray> columns = new List<DataArray>();
            public DataStorage(DataType[] data_types)
            {
                foreach (DataType dt in data_types)
                {
                    switch (dt)
                    {
                        case DataType.dt_int:
                            columns.Add(new DataArrayInteger());
                            break;
                        case DataType.dt_double:
                            columns.Add(new DataArrayDouble());
                            break;
                        default:
                            throw new ArgumentException(String.Format("Unknown data type {0}", dt));
                    }
                }
            }
            public void setSize(int num_rows) {
                for (int i = 0; i < columns.Count; i++) {
                    columns[i].setSize(num_rows);
                }
            }
        }

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are.
        ~DataSetMemoryBased()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            // free managed resources

            }
        }


        public DataStorage data;

        protected int current_row;
        protected int num_rows;

        string fileToRead;
        bool isOpen = false;

        public DataSetMemoryBased(DataType[] data_types, String filename)
        {
            data = new DataStorage(data_types);

			if (File.Exists(filename))
				fileToRead = filename;
			else
				throw new FileNotFoundException("Could not find file " + filename);
        }

        private void init(int num_rows) {
            this.num_rows = num_rows;
            data.setSize(num_rows);
            current_row = -1;
            isOpen = true;
        }

        public DataSetMemoryBased(DataType[] data_types, int num_rows)
        {
            data = new DataStorage(data_types);
            init(num_rows);
        }

        public void Close()
        {
            //isOpen = false;
        }

        public int ColumnCount
        {
            get { return data.columns.Count; }
        }
        public int RowCount
        {
            get { return num_rows; }
        }

        public double GetDouble(int colIndex)
        {
            if (current_row < 0)
            {
                throw new Exception("bof");
            }
            if (current_row >= num_rows)
            {
                throw new Exception("end of file");
            }
            if (colIndex >= ColumnCount)
            {
                throw new ArgumentException(String.Format("column index {0} exceeds column count", colIndex));
            }

            if (data.columns[colIndex] is DataArrayDouble)
            {
                return (data.columns[colIndex] as DataArrayDouble).data[current_row];
            }
            else if (data.columns[colIndex] is DataArrayInteger)
            {
                return (data.columns[colIndex] as DataArrayInteger).data[current_row];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public short GetInt16(int colIndex)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int colIndex)
        {
            if (current_row < 0)
            {
                throw new Exception("bof");
            }
            if (current_row >= num_rows)
            {
                throw new Exception("end of file");
            }
            if (colIndex >= ColumnCount)
            {
                throw new ArgumentException("column index " + colIndex + " exceeds column count " + ColumnCount);
            }

            if (data.columns[colIndex] is DataArrayDouble)
            {
                throw new ArgumentException("column is double; cannot read an int");
            }
            else if (data.columns[colIndex] is DataArrayInteger)
            {
                return (data.columns[colIndex] as DataArrayInteger).data[current_row];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public long GetInt64(int colIndex)
        {
            throw new NotImplementedException();
        }

        public String GetString(int colIndex)
        {
            throw new NotImplementedException();
        }

        public object GetObject(int colIndex)
        {
            throw new NotImplementedException();
        }


        public bool IsOpen
        {
            get { return isOpen; }
        }

        public void Open()
        {
        	if (!isOpen)
            {
        		int cnt = 0;
        		{
        			using ( StreamReader reader = new StreamReader(fileToRead) )
        				while ((reader.ReadLine()) != null)
                    		cnt++;
        		}
        		this.num_rows = cnt;
        		init(num_rows);
        		{
        			using ( StreamReader reader = new StreamReader(fileToRead) )
					{
        				string line;
        				cnt = 0;
						char[] split_chars = new char[]{ '\t', ' ' };
                    	while ((line = reader.ReadLine()) != null)
                    	{
							if (line.Trim().Equals(String.Empty))
								continue;

                        	string[] tokens = line.Split(split_chars);
                        	for (int i = 0; i < ColumnCount; i++)
                        	{
								try
								{
                            		data.columns[i].read(cnt, tokens[i]);
								}
								catch (Exception)
								{
									Console.Error.WriteLine("line: '{0}'", line);
									throw;
								}
                        	}
                        	cnt++;
                    	}
					}
                }
                isOpen = true;
            }

            current_row = -1;
        }

        public bool Read()
        {
            if (current_row >= (num_rows - 1))
            {
                return false;
            }
            current_row++;

            return true;
        }

		/// <summary>
		/// Gets the number of columns of the first non-empty line of a file.
		/// </summary>
		/// <param name="filename">the name of the file</param>
		/// <returns>the number of columns</returns>
		static public int GetNumberOfColumns(string filename)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException("Could not find file " + filename);

			using ( StreamReader reader = new StreamReader(filename) ) 
			{
				// TODO: allow comments
				char[] split_chars = new char[]{ '\t', ' ' };
				string line;
	           	while ((line = reader.ReadLine()) != null)
	            {
					if (line.Trim().Equals(String.Empty))
						continue;
	
	                string[] tokens = line.Split(split_chars);
					return tokens.Length;
				}
				return 0;
			}
		}
    }
}