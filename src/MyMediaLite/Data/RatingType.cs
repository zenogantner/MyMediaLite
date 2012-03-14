// This file is part of MyMediaLite.
// Its content is in the public domain.

namespace MyMediaLite.Data
{
	/// <summary>Represent different numerical types that are used to store the ratings</summary>
	public enum RatingType : byte
	{
		/// <summary>byte (1 byte per rating)</summary>
		BYTE,
		/// <summary>float (4 bytes per rating)</summary>
		FLOAT,
		/// <summary>double (8 bytes per rating)</summary>
		DOUBLE
	}
}