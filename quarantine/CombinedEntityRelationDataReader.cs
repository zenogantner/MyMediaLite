using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using MyMediaProject.RecommenderSystem.Framework.EntityRelationAlgorithm;

namespace MyMediaLite.util
{
	/// <description>Not thread safe</description>
    /// <author>Zeno Gantner, University of Hildesheim</author>
    public class CombinedEntityRelationDataReader : IEntityRelationDataReader
	{
        protected int current_reader = 0;

        bool isOpen = false;

		protected IList<IEntityRelationDataReader> readers;

		public int ColumnCount  {
			get { return readers[current_reader].ColumnCount; }
		}
		
        public CombinedEntityRelationDataReader()
        {
			readers = new List<IEntityRelationDataReader>();
        }
		
		public void Dispose() { } 
		
		public void Add(IEntityRelationDataReader reader)
		{
			reader.Open();
			readers.Add(reader);
		}

        public void Close()
        {
            //isOpen = false;
        }

        public double GetDouble(int colIndex)
        {
			return readers[current_reader].GetDouble(colIndex);
        }

        public short GetInt16(int colIndex)
        {
            return readers[current_reader].GetInt16(colIndex);
        }

        public int GetInt32(int colIndex)
        {
			return readers[current_reader].GetInt32(colIndex);
        }

        public long GetInt64(int colIndex)
        {
            return readers[current_reader].GetInt64(colIndex);
        }

        public String GetString(int colIndex)
        {
            return readers[current_reader].GetString(colIndex);
        }

		/*
        public object GetObject(int colIndex)
        {
            return readers[current_reader].GetObject(colIndex);
        }
        */

        public bool IsOpen
        {
            get { return isOpen; }
        }

        public void Open()
        {
            isOpen = true;
            current_reader = 0;
        }

        public bool Read()
        {
			if (current_reader >= readers.Count)
				return false;

			if (readers[current_reader].Read())
			{
				return true;
			}

			readers[current_reader].Close();
			//Console.WriteLine("Next reader!");
			current_reader++;
			return this.Read();
        }

	}
}