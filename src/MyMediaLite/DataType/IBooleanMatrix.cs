// Copyright (C) 2011 Zeno Gantner
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
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace MyMediaLite.DataType
{
	/// <summary>Interface for boolean matrices</summary>
	public interface IBooleanMatrix : IMatrix<bool>
	{
		/// <summary>Indexer to access the rows of the matrix</summary>
		/// <param name="x">the row ID</param>
		ICollection<int> this [int x] { get; }

		/// <summary>The number of (true) entries</summary>
		int NumberOfEntries { get; }

		/// <summary>The IDs of the non-empty rows in the matrix (the ones that contain at least one true entry)</summary>
		ICollection<int> NonEmptyRowIDs { get; }

		/// <summary>The IDs of the non-empty columns in the matrix (the ones that contain at least one true entry)</summary>
		ICollection<int> NonEmptyColumnIDs { get; }

		/// <summary>Get all true entries (column IDs) of a row</summary>
		/// <param name="row_id">the row ID</param>
		/// <returns>a list of column IDs</returns>
		IList<int> GetEntriesByRow(int row_id);

		/// <summary>Get all the number of entries in a row</summary>
		/// <param name="row_id">the row ID</param>
		/// <returns>the number of entries in row row_id</returns>
		int NumEntriesByRow(int row_id);

		/// <summary>Get all true entries (row IDs) of a column</summary>
		/// <param name="column_id">the column ID</param>
		/// <returns>a list of row IDs</returns>
		IList<int> GetEntriesByColumn(int column_id);

		/// <summary>Get all the number of entries in a column</summary>
		/// <param name="column_id">the column ID</param>
		/// <returns>the number of entries in column column_id</returns>
		int NumEntriesByColumn(int column_id);

		/// <summary>Get the overlap of two matrices, i.e. the number of true entries where they agree</summary>
		/// <param name="s">the <see cref="IBooleanMatrix"/> to compare to</param>
		/// <returns>the number of entries that are true in both matrices</returns>
		int Overlap(IBooleanMatrix s);
	}
}

