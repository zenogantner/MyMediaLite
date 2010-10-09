// This file is part of MyMediaLite.
// Its content is in the public domain.

namespace MyMediaLite.util
{
	/// <summary>
	/// Generic triple class
	/// </summary>
	public class Triple<T, U, V> {
		/// <summary>
		/// Default constructor
		/// </summary>
	    public Triple() {
	    }

		/// <summary>
		/// Create a Triple object from existing data
		/// </summary>
		/// <param name="first">
		/// the first component
		/// </param>
		/// <param name="second">
		/// the second component
		/// </param>
		/// <param name="third">
		/// the third component
		/// </param>
	    public Triple(T first, U second, V third) {
	        this.First  = first;
	        this.Second = second;
			this.Third  = third;
	    }

		/// <summary>
		/// the first component
		/// </summary>
	    public T First  { get; set; }
		
		/// <summary>
		/// The second component
		/// </summary>
	    public U Second { get; set; }
		
		/// <summary>
		/// The third component
		/// </summary>
	    public V Third  { get; set; }
	};
}