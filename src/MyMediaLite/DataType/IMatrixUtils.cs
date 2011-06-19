// Copyright (C) 2010, 2011 Zeno Gantner
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
using System.Globalization;
using System.IO;
using System.Reflection;
using MyMediaLite.Util;

namespace MyMediaLite.DataType
{
	/// <summary>Utilities to work with matrices</summary>
	public class IMatrixUtils
	{
		/// <summary>Write a matrix of doubles to a StreamWriter object</summary>
		/// <param name="writer">a <see cref="StreamWriter"/></param>
		/// <param name="matrix">the matrix of doubles to write out</param>
		static public void WriteMatrix(StreamWriter writer, IMatrix<double> matrix)
		{
			writer.WriteLine(matrix.NumberOfRows + " " + matrix.NumberOfColumns);
			for (int i = 0; i < matrix.NumberOfRows; i++)
				for (int j = 0; j < matrix.NumberOfColumns; j++)
					writer.WriteLine(i + " " + j + " " + matrix[i, j].ToString(CultureInfo.InvariantCulture));
			writer.WriteLine();
		}

		/// <summary>Write a matrix of floats to a StreamWriter object</summary>
		/// <param name="writer">a <see cref="StreamWriter"/></param>
		/// <param name="matrix">the matrix of floats to write out</param>
		static public void WriteMatrix(StreamWriter writer, IMatrix<float> matrix)
		{
			writer.WriteLine(matrix.NumberOfRows + " " + matrix.NumberOfColumns);
			for (int i = 0; i < matrix.NumberOfRows; i++)
				for (int j = 0; j < matrix.NumberOfColumns; j++)
					writer.WriteLine(i + " " + j + " " + matrix[i, j].ToString(CultureInfo.InvariantCulture));
			writer.WriteLine();
		}

		/// <summary>Write a matrix of integers to a StreamWriter object</summary>
		/// <param name="writer">a <see cref="StreamWriter"/></param>
		/// <param name="matrix">the matrix of doubles to write out</param>
		static public void WriteMatrix(StreamWriter writer, IMatrix<int> matrix)
		{
			writer.WriteLine(matrix.NumberOfRows + " " + matrix.NumberOfColumns);
			for (int i = 0; i < matrix.NumberOfRows; i++)
				for (int j = 0; j < matrix.NumberOfColumns; j++)
					writer.WriteLine(i + " " + j + " " + matrix[i, j].ToString());
			writer.WriteLine();
		}

		/// <summary>Write a sparse matrix of doubles to a StreamWriter object</summary>
		/// <param name="writer">a <see cref="StreamWriter"/></param>
		/// <param name="matrix">the matrix of doubles to write out</param>
		static public void WriteSparseMatrix(StreamWriter writer, SparseMatrix<double> matrix)
		{
			writer.WriteLine(matrix.NumberOfRows + " " + matrix.NumberOfColumns);
			foreach (var index_pair in matrix.NonEmptyEntryIDs)
			   	writer.WriteLine(index_pair.First + " " + index_pair.Second + " " + matrix[index_pair.First, index_pair.Second].ToString(CultureInfo.InvariantCulture));
			writer.WriteLine();
		}

		/// <summary>Write a sparse matrix of floats to a StreamWriter object</summary>
		/// <param name="writer">a <see cref="StreamWriter"/></param>
		/// <param name="matrix">the matrix of floats to write out</param>
		static public void WriteSparseMatrix(StreamWriter writer, SparseMatrix<float> matrix)
		{
			writer.WriteLine(matrix.NumberOfRows + " " + matrix.NumberOfColumns);
			foreach (var index_pair in matrix.NonEmptyEntryIDs)
			   	writer.WriteLine(index_pair.First + " " + index_pair.Second + " " + matrix[index_pair.First, index_pair.Second].ToString(CultureInfo.InvariantCulture));
			writer.WriteLine();
		}

		/// <summary>Write a sparse matrix of integers to a StreamWriter object</summary>
		/// <param name="writer">a <see cref="StreamWriter"/></param>
		/// <param name="matrix">the matrix of doubles to write out</param>
		static public void WriteSparseMatrix(StreamWriter writer, SparseMatrix<int> matrix)
		{
			writer.WriteLine(matrix.NumberOfRows + " " + matrix.NumberOfColumns);
			foreach (var index_pair in matrix.NonEmptyEntryIDs)
			   	writer.WriteLine(index_pair.First + " " + index_pair.Second + " " + matrix[index_pair.First, index_pair.Second].ToString());
			writer.WriteLine();
		}

		/// <summary>Read a matrix from a TextReader object</summary>
		/// <param name="reader">the <see cref="TextReader"/> object to read from</param>
		/// <param name="example_matrix">matrix of the type of matrix to create</param>
		/// <returns>a matrix of doubles</returns>
		static public IMatrix<double> ReadMatrix(TextReader reader, IMatrix<double> example_matrix)
		{
			string[] numbers = reader.ReadLine().Split(' ');
			int dim1 = int.Parse(numbers[0]);
			int dim2 = int.Parse(numbers[1]);

			IMatrix<double> matrix = example_matrix.CreateMatrix(dim1, dim2);

			while ((numbers = reader.ReadLine().Split(' ')).Length == 3)
			{
				int i = int.Parse(numbers[0]);
				int j = int.Parse(numbers[1]);
				double v = double.Parse(numbers[2], CultureInfo.InvariantCulture);

				if (i >= dim1)
					throw new IOException("i = " + i + " >= " + dim1);
				if (j >= dim2)
					throw new IOException("j = " + j + " >= " + dim2);

				matrix[i, j] = v;
			}

			return matrix;
		}

		/// <summary>Read a matrix from a TextReader object</summary>
		/// <param name="reader">the <see cref="TextReader"/> object to read from</param>
		/// <param name="example_matrix">matrix of the type of matrix to create</param>
		/// <returns>a matrix of float</returns>
		static public IMatrix<float> ReadMatrix(TextReader reader, IMatrix<float> example_matrix)
		{
			string[] numbers = reader.ReadLine().Split(' ');
			int dim1 = int.Parse(numbers[0]);
			int dim2 = int.Parse(numbers[1]);

			IMatrix<float> matrix = example_matrix.CreateMatrix(dim1, dim2);

			while ((numbers = reader.ReadLine().Split(' ')).Length == 3)
			{
				int i = int.Parse(numbers[0]);
				int j = int.Parse(numbers[1]);
				float v = float.Parse(numbers[2], CultureInfo.InvariantCulture);

				if (i >= dim1)
					throw new IOException("i = " + i + " >= " + dim1);
				if (j >= dim2)
					throw new IOException("j = " + j + " >= " + dim2);

				matrix[i, j] = v;
			}

			return matrix;
		}

		/// <summary>Read a matrix of integers from a TextReader object</summary>
		/// <param name="reader">the <see cref="TextReader"/> object to read from</param>
		/// <param name="example_matrix">matrix of the type of matrix to create</param>
		/// <returns>a matrix of integers</returns>
		static public IMatrix<int> ReadMatrix(TextReader reader, IMatrix<int> example_matrix)
		{
			string[] numbers = reader.ReadLine().Split(' ');
			int dim1 = int.Parse(numbers[0]);
			int dim2 = int.Parse(numbers[1]);

			IMatrix<int> matrix = example_matrix.CreateMatrix(dim1, dim2);

			while ((numbers = reader.ReadLine().Split(' ')).Length == 3)
			{
				int i = int.Parse(numbers[0]);
				int j = int.Parse(numbers[1]);
				int v = int.Parse(numbers[2]);

				if (i >= dim1)
					throw new IOException("i = " + i + " >= " + dim1);
				if (j >= dim2)
					throw new IOException("j = " + j + " >= " + dim2);

				matrix[i, j] = v;
			}

			return matrix;
		}
	}
}