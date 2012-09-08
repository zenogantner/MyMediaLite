// Copyright (C) 2011, 2012 Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;

namespace MyMediaLite.DataType
{
	/// <summary>Class for storing dense matrices</summary>
	/// <remarks>
	/// The data is stored in row-major mode.
	/// Indexes are zero-based.
	/// </remarks>
	/// <typeparam name="T">the type of the matrix entries</typeparam>
	public class SymmetricMatrix<T> : IMatrix<T>
	{
		/// <summary>Data array: data is stored in columns.</summary>
		protected internal T[][] data;
		/// <summary>Dimension, the number of rows and columns</summary>
		public int dim;

		///
		public virtual bool IsSymmetric { get { return true; } }

		///
		public int NumberOfRows { get { return dim; } }

		///
		public int NumberOfColumns { get { return dim; } }

		/// <summary>Initializes a new instance of the SymmetricMatrix class</summary>
		/// <param name="dim">the number of rows and columns</param>
		public SymmetricMatrix(int dim)
		{
			if (dim < 0)
				throw new ArgumentOutOfRangeException("dim must be at least 0");

			this.dim = dim;
			this.data = new T[dim][];
			for (int i = 0; i < dim; i++)
				data[i] = new T[i + 1];
		}

		///
		public IMatrix<T> CreateMatrix(int num_rows, int num_columns)
		{
			if (num_rows != num_columns)
				throw new ArgumentException("num_rows must equal num_columns for symmetric matrices");
			return new SymmetricMatrix<T>(num_rows);
		}

		///
		public IMatrix<T> Transpose()
		{
			throw new NotImplementedException();
		}

		///
		public virtual T this [int i, int j]
		{
			get {
				if (i >= j)
					return data[i][j];
				else
					return data[j][i];
			}
			set {
				if (i >= j)
					data[i][j] = value;
				else
					data[j][i] = value;
			}
		}

		/// <summary>Resize to the given size</summary>
		/// <param name="size">the size</param>
		public void Resize(int size)
		{
			Resize (size, size);
		}

		///
		public void Resize(int num_rows, int num_columns)
		{
			if (num_rows != num_columns)
				throw new ArgumentException("num_rows must equal num_columns for symmetric matrices");

			if (num_rows != dim)
			{
				// create new data structure
				var new_data = new T[num_rows][];
				for (int i = 0; i < num_rows; i++)
					new_data[i] = new T[i + 1];

				for (int i = 0; i < num_rows; i++)
					for (int j = 0; j <= i; j++)
						new_data[i][j] = this[i, j];

				// replace old data structure
				this.dim = num_rows;
				this.data = new_data;
			}
		}
	}
}