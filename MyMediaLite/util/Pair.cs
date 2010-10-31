// This file is part of MyMediaLite.
// Its content is in the public domain.

namespace MyMediaLite.util
{
	public class Pair<T, U> {
	    public Pair() {
	    }

	    public Pair(T first, U second) {
	        this.First = first;
	        this.Second = second;
	    }

	    public T First { get; set; }
	    public U Second { get; set; }
	};
}