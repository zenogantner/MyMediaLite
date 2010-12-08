// This file is part of MyMediaLite.
// Its content is in the public domain.


namespace MyMediaLite.Util
{
	/// <summary>Generic pair class</summary>
	public class Pair<T, U>
	{
		/// <summary>Default constructor</summary>
	    public Pair() { }

		/// <summary>Create a Pair object from existing data</summary>
		/// <param name="first">the first component</param>
		/// <param name="second">the second component</param>
	    public Pair(T first, U second)
		{
	        this.First = first;
	        this.Second = second;
	    }

		/// <summary>the first component</summary>
	    public T First { get; set; }

		/// <summary>the second component</summary>
	    public U Second { get; set; }
	}
}