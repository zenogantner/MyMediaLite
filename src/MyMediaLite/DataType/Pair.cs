// This file is part of MyMediaLite.
// Its content is in the public domain.

namespace MyMediaLite.DataType
{
	/// <summary>Generic pair class</summary>
	public sealed class Pair<T, U>
	{
		// TODO consider having a value type here
		
		/// <summary>Create a Pair object from existing data</summary>
		/// <param name="first">the first component</param>
		/// <param name="second">the second component</param>
		public Pair(T first, U second)
		{
			this.First = first;
			this.Second = second;
		}

		/// <summary>the first component</summary>
		public T First { get; private set; }

		/// <summary>the second component</summary>
		public U Second { get; private set; }
	}
}