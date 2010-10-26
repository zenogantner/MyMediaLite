// Copyright (C) 2010 Tina Lichtenthäler
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
using System.Collections;
using System.Collections.Generic;
using MyMediaLite.data_type;
using NUnit.Framework;


namespace MyMediaLite
{
	/// <summary>
	/// Class for testing the data_type classes
	/// </summary>
	[TestFixture()]
	public class data_type_test
	{
		/// <summary>
		/// Unit test of Matrix.GetRow()
		/// and Matrix.SetRow()
		/// </summary>
		[Test()] public void GetRow ()
		{
			Matrix<int> testMatrix = new Matrix<int>(5, 5);
			int[] row = { 1, 2, 3, 4, 5 };
			testMatrix.SetRow(3, row);
			Assert.AreEqual(testMatrix.GetRow(3), row);
		}

		/// <summary>
		/// Unit test of Matrix.GetColumn()
		/// and Matrix.SetColumn()
		/// </summary>
		[Test()] public void GetColumn ()
		{
			Matrix<int> testMatrix = new Matrix<int>(5, 5);
			int[] column = { 1, 2, 3, 4, 5 };
			testMatrix.SetColumn(3, column);
			Assert.AreEqual(testMatrix.GetColumn(3), column);
		}

		/// <summary>
		/// Unit test of Matrix.Init(T d)
		/// </summary>
		[Test()] public void Init()
		{
			Matrix<int> testMatrix = new Matrix<int>(5, 5);
			int[] row = { 2, 2, 2, 2, 2 };
			testMatrix.Init(2);
			Assert.AreEqual(testMatrix.GetRow(2), row);
		}

		/// <summary>
		/// Unit test of Matrix.SetRowToOneValue(int i, T v)
		/// </summary>
		[Test()] public void SetRowToOneValue()
		{
			Matrix<int> testMatrix = new Matrix<int>(5, 5);
			int[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				testMatrix.SetRow(i, row);
			testMatrix.SetRowToOneValue(3, 10);
			int[] testrow = { 10, 10, 10, 10, 10 };
			Assert.AreEqual(testrow, testMatrix.GetRow(3));
		}


		/// <summary>
		/// Unit test of Matrix.SetColumnToOneValue(int j, T v)
		/// </summary>
		[Test()] public void SetColumnToOneValue()
		{
			Matrix<int> testMatrix = new Matrix<int>(5, 5);;
			int[] row = { 1, 2, 3, 4, 5};
			for (int i = 0; i < 5; i++)
				testMatrix.SetRow(i, row);
			testMatrix.SetColumnToOneValue(3, 10);
			int[] testcolumn = { 10, 10, 10, 10, 10 };
			Assert.AreEqual(testcolumn, testMatrix.GetColumn(3));
		}



		/// <summary>
		/// Unit test of MatrixUtils.Inc(Matrix&lt;double&gt; matrix, int i, int j, double v)
		/// </summary>
		[Test()] public void Inc()
		{
			var testMatrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				testMatrix.SetRow(i, row);
			MatrixUtils.Inc(testMatrix, 3, 4, 2.5);
			Assert.AreEqual(7.5, testMatrix[3, 4]);
		}


		/// <summary>
		/// Unit test of MatrixUtils.Inc(Matrix&lt;double&gt; matrix1, Matrix&lt;double&gt; matrix2)
		/// </summary>
		[Test()] public void Inc2()
		{
			var testMatrix1 = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				testMatrix1.SetRow(i, row);

			var testMatrix2 = new Matrix<double>(5, 5);
			for (int i = 0; i < 5; i++)
				testMatrix2.SetRow(i, row);
			double[] testrow = {2, 4, 6, 8, 10};
			MatrixUtils.Inc(testMatrix1, testMatrix2);
			Assert.AreEqual(testrow, testMatrix1.GetRow(2));

		}

		/// <summary>
		/// Unit test of MatrixUtils.ColumnAverage(Matrix&lt;double&gt; matrix, int col)
		/// </summary>
		[Test()] public void ColumnAverage()
		{
			var testMatrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				testMatrix.SetRow(i, row);
			Assert.AreEqual(2.0, MatrixUtils.ColumnAverage(testMatrix, 1));
			Assert.AreEqual(5.0, MatrixUtils.ColumnAverage(testMatrix, 4));
		}

		/// <summary>
		/// Unit test of MatrixUtils.RowAverage(Matrix&lt;double&gt; matrix, int row)
		/// </summary>
		[Test()] public void RowAverage()
		{
			var testMatrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				testMatrix.SetRow(i, row);
			Assert.AreEqual(3.0, MatrixUtils.RowAverage(testMatrix, 1));
			Assert.AreEqual(3.0, MatrixUtils.RowAverage(testMatrix, 4));
		}

		/// <summary>
		/// Unit test of MatrixUtils.Multiply(Matrix&lt;double&gt; matrix, double d)
		/// </summary>
		[Test()] public void Multiply()
		{
			var testMatrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i<5; i++)
				testMatrix.SetRow(i, row);
			MatrixUtils.Multiply(testMatrix, 2.5);
			double[] testrow = { 2.5, 5, 7.5, 10, 12.5 };
			Assert.AreEqual(testrow, testMatrix.GetRow(3));
		}

		/// <summary>
		/// Unit test of MatrixUtils.FrobeniusNorm(Matrix&lt;double&gt; matrix)
		/// </summary>
		[Test()] public void FrobeniusNorm()
		{
			var testMatrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				testMatrix.SetRow(i, row);
			double result =Math.Sqrt(275.0);
			Assert.AreEqual(result,MatrixUtils.FrobeniusNorm(testMatrix));
		}

		/// <summary>
		/// Unit test of MatrixUtils.RowScalarProduct(Matrix&lt;double&gt; matrix, int i, double[] vector)
		/// </summary>
		[Test()] public void RowScalarProduct()
		{
			var testMatrix = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				testMatrix.SetRow(i, row);
			double[] vector = { 1, 2, 3, 4, 5 };
			double result = 55;
			Assert.AreEqual(result, MatrixUtils.RowScalarProduct(testMatrix, 2, vector));
		}

		/// <summary>
		/// Unit test of MatrixUtils.ContainsNaN(Matrix&lt;double&gt; matrix)
		/// </summary>
		[Test()] public void ContainsNaN()
		{
			var testMatrixfalse = new Matrix<double>(5, 5);
			double[] row = { 1, 2, 3, 4, 5 };
			for (int i = 0; i < 5; i++)
				testMatrixfalse.SetRow(i, row);
			var testMatrixtrue = new Matrix<double>(5, 5);
			double[] row2 = { 1, 2, 3, 4, 5 };
			//row2[3] /= 0;
			for (int i = 0; i < 5; i++)
				testMatrixtrue.SetRow(i, row2);

			Assert.IsFalse(MatrixUtils.ContainsNaN(testMatrixfalse));
			// TODO insert Nan in testMatrix
			// Assert.IsTrue(MatrixUtils.ContainsNaN(testMatrixtrue));
		}

		/// <summary>
		/// Unit test of SparseBooleanMatrix.NonEmptyRows
		/// </summary>
		[Test()] public void NonEmptyRows()
		{
			SparseBooleanMatrix testMatrix = new SparseBooleanMatrix();
			for (int i = 0; i < 5; i++)
				if (i != 2)
				{
					testMatrix[i, 1]= true;
					testMatrix[i, 4]= true;
				}
			Assert.IsTrue(testMatrix[0, 1]);
			IList<KeyValuePair<int, HashSet<int>>> nonEmptyRows = testMatrix.NonEmptyRows;
			Assert.AreEqual(4, nonEmptyRows.Count);

			// TODO test contents
		}

		/// <summary>
		/// Unit test of SparseBooleanMatrix.NonEmptyRowIDs
		/// </summary>
		[Test()] public void NonEmptyRowIDs()
		{
			SparseBooleanMatrix testMatrix = new SparseBooleanMatrix();
			for (int i = 0; i < 5; i++)
				if (i != 2 && i !=3)
				{
					testMatrix[i, 1]= true;
					testMatrix[i, 4]= true;
				}
			ICollection<int> rowIDs = testMatrix.NonEmptyRowIDs;
			IEnumerator <int> rowIDsEnum = rowIDs.GetEnumerator();
			rowIDsEnum.MoveNext();
			Assert.AreEqual(0, rowIDsEnum.Current);
			rowIDsEnum.MoveNext();
			Assert.AreEqual(1, rowIDsEnum.Current);
			rowIDsEnum.MoveNext();
			rowIDsEnum.MoveNext();
			Assert.AreEqual(4, rowIDsEnum.Current);
			Assert.IsFalse(rowIDsEnum.MoveNext());
		}

		/// <summary>
		/// Unit test of SparseBooleanMatrix.NumberOfRows
		/// </summary>
		[Test()] public void NumberOfRows()
		{
			SparseBooleanMatrix testMatrix = new SparseBooleanMatrix();
			for (int i = 0; i < 5; i++)
			{
				testMatrix[i, 1]= true;
				testMatrix[i, 4]= true;
			}
			Assert.AreEqual(5, testMatrix.NumberOfRows);
		}

		/// <summary>
		/// Unit test of SparseBooleanMatrix.NumberOfEntries
		/// </summary>
		[Test()] public void NumberOfEntries()
		{
			SparseBooleanMatrix testMatrix = new SparseBooleanMatrix();
			for (int i = 0; i < 5; i++)
				if (i != 2 && i != 4)
				{
					testMatrix[i, 1]= true;
					testMatrix[i, 4]= false;
				}
			Assert.AreEqual(3, testMatrix.NumberOfEntries);
		}

		/// <summary>
		/// Unit test of SparseBooleanMatrix.RemoveColumn(int y)
		/// </summary>
		[Test()] public void RemoveColumn()
		{
			SparseBooleanMatrix testMatrix = new SparseBooleanMatrix();
			for (int i = 0; i < 5; i++)
				if (i != 2 && i != 4)
				{
					testMatrix[i, 1]= true;
					testMatrix[i, 4]= true;
				}
			testMatrix[2, 2] = true;

			testMatrix.RemoveColumn(2);

			Assert.IsTrue(testMatrix[0, 3]);
			Assert.IsTrue(testMatrix[1, 3]);
			Assert.IsTrue(testMatrix[3, 3]);
			Assert.IsTrue(testMatrix[1, 1]);
		}

		/// <summary>
		/// Unit test of SparseBooleanMatrix.RemoveColumn(int[] delete_columns)
		/// </summary>
		[Test()] public void RemoveColumns()
		{
			SparseBooleanMatrix testMatrix = new SparseBooleanMatrix();
			for (int i = 0; i < 7; i++)
				if(i != 2 && i != 4)
				{
					testMatrix[i, 1]= true;
					testMatrix[i, 4]= true;
				}
			testMatrix[2, 2] = true;
			testMatrix[2, 5] = true;
			testMatrix[4, 3] = true;
			int[] delete_columns = {2, 4};
			testMatrix.RemoveColumn(delete_columns);

			// test the new columns
			Assert.IsTrue(testMatrix[4, 2]);
			Assert.IsTrue(testMatrix[2, 3]);
			Assert.IsFalse(testMatrix[1, 3]);
			Assert.IsFalse(testMatrix[4, 3]);
		}

		/// <summary>
		/// Unit tests for SparseBooleanMatrix.Transpose()
		/// </summary>
		[Test()] public void Transpose()
		{
			SparseBooleanMatrix testMatrix = new SparseBooleanMatrix();
			for (int i = 0; i < 7; i++)
				if(i != 2 && i != 4)
				{
					testMatrix[i, 1]= true;
					testMatrix[i, 4]= true;
				}
			testMatrix[2, 2] = true;
			testMatrix[2, 5] = true;
			testMatrix[4, 3] = true;
			// transpose the matrix
			SparseBooleanMatrix transposedMatrix = testMatrix.Transpose();
			// test the transposed matrix
			Assert.IsTrue(transposedMatrix[1,0]);
			Assert.IsTrue(transposedMatrix[4, 6]);
			Assert.IsFalse(transposedMatrix[3, 1]);
			Assert.IsFalse(transposedMatrix[5, 4]);
		}

		/// <summary>
		/// Unit test of SparseBooleanMatrix.Overlap(SparseBooleanMatrix s)
		/// </summary>
		[Test()] public void Overlap()
		{
			SparseBooleanMatrix testMatrix = new SparseBooleanMatrix();
			testMatrix[2, 2] = true;
			testMatrix[2, 5] = true;
			testMatrix[4, 3] = true;
			testMatrix[4, 6] = true;
			testMatrix[5, 1] = true;
			testMatrix[5, 5] = true;

			SparseBooleanMatrix overlapMatrix = new SparseBooleanMatrix();
			overlapMatrix[2, 1] = true;
			overlapMatrix[2, 5] = true; //same entry
			overlapMatrix[4, 4] = true;
			overlapMatrix[4, 6] = true; //same entry
			overlapMatrix[5, 2] = true;
			overlapMatrix[5, 5] = true; //same entry

			Assert.AreEqual(3, testMatrix.Overlap(overlapMatrix));

		}

		/// <summary>Unit test for VectorUtils.EuclideanNorm(ICollection&lt;double&gt; vector)</summary>
		[Test()] public void EuclideanNorm()
		{
			List<double> testVector = new List<double>();
			testVector.Add(2);
			testVector.Add(5);
			testVector.Add(3);
			testVector.Add(7);
			testVector.Add(5);
			testVector.Add(3);
			double result = 11;
			Assert.AreEqual(result, VectorUtils.EuclideanNorm(testVector));
		}
	}
}