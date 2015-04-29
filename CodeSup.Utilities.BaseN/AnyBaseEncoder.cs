using System;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace CodeSup.Utilities.BaseN {
	/// <summary>
	/// Encodes a BigInteger, Guid or byte array against an arbitrary radix
	/// number system.
	/// </summary>
	/// <remarks>
	/// This class is intended to encode short (up to 64 bytes) byte sequences as if they represent a single
	/// binary number. For example, it is well suited to encode IPV6 addresses, UUIDs,
	/// GUIDs, Hashes (SHA or MD5) etc.
	/// It isn't very efficient for long byte sequences, like binary files, images etc.
	/// Also, it doesn't insert any output formatting, marker, or fill characters.
	/// If you need something like this, you should use an ordinary Base64 etc. encoding.
	/// </remarks>
	public class AnyBaseEncoder {

		public const string WhitespaceAlphabet = " \t\n\r\u000B\u0085\u00A0\u2000\u2001\u2002\u2004\u2005\u2006\u2007\u2008\u2009\u200A"; // Funny: only whitespace...
		public const string Base32HexAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
		public const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
		public const string Base36Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		public const string Base52Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		public const string Base62Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		public const string Base64Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
		public const string Base85Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&()*+-;<=>?@^_`{|}~";
		public const string Z85Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.-:+=^!/*?&<>()[]{}@%$#";
		public const string Base91Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"";
		public const string Base94Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"-\\'";
		public const string Base98Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"-\\' \t\n\r";

		/// <summary>
		/// Works in the spirit of ordinary hex numbers, but with 32 different digits instead of 16
		/// </summary>
		public static readonly AnyBaseEncoder Base32Hex = new AnyBaseEncoder(Base32HexAlphabet);

		/// <summary>
		/// As opposed to Base32Hex, lower significance is represented by letters A-Z, and higher by numbers 2-7
		/// </summary>
		public static readonly AnyBaseEncoder Base32 = new AnyBaseEncoder(Base32Alphabet);
		
		/// <summary>
		/// Base36 is suitable for generating file names on case-insensitive filesystems, like e.g. on MS Windows.
		/// An 128bit integer value (e.g. MD5 or GUID) will require 25 characters
		/// </summary>
		public static readonly AnyBaseEncoder Base36 = new AnyBaseEncoder(Base36Alphabet);

		/// <summary>
		/// Base52 just uses upper- and lowercase alphabetical characters.
		/// This is well-suited for use in XML as anything that requires to
		/// be of the XML "NCName" type, like IDs, element or attribute names.
		/// An 128bit integer value (e.g. MD5 or GUID) will require 23 characters
		/// </summary>
		public static readonly AnyBaseEncoder Base52 = new AnyBaseEncoder(Base52Alphabet);
		
		/// <summary>
		/// Base62 uses digits and upper- and lowercase letters.
		/// </summary>
		public static readonly AnyBaseEncoder Base62 = new AnyBaseEncoder(Base62Alphabet);

		/// <summary>
		/// base64 is the typical alphabet used in email encoding of binary data.
		/// </summary>
		public static readonly AnyBaseEncoder Base64 = new AnyBaseEncoder(Base64Alphabet);

		/// <summary>
		/// Base85 uses characters typically allowed in case-sensitive filesystems as file names
		/// </summary>
		public static readonly AnyBaseEncoder Base85 = new AnyBaseEncoder(Base85Alphabet);

		/// <summary>
		/// Base91 uses all printable characters except dash -, backslash \, and apostrophe '
		/// </summary>
		public static readonly AnyBaseEncoder Base91 = new AnyBaseEncoder(Base91Alphabet);

		/// <summary>
		/// Base94 uses all printable ASCII characters except whitespace
		/// </summary>
		public static readonly AnyBaseEncoder Base94 = new AnyBaseEncoder(Base94Alphabet);

		/// <summary>
		/// Base98 uses all printable ASCII characters including newline and whitespace
		/// </summary>
		public static readonly AnyBaseEncoder Base98 = new AnyBaseEncoder(Base98Alphabet);

		private static readonly string[] Alphabets = { Base32HexAlphabet, Base36Alphabet, Base52Alphabet, Base62Alphabet, Base64Alphabet, Base85Alphabet, Base91Alphabet, Base94Alphabet };

		private static readonly BigInteger Min128Inv = (new BigInteger(1) << 127);

		private readonly char[] _alphabet;
		private readonly BigInteger _targetBase;

		public AnyBaseEncoder(string alphabet)
			: this(alphabet.ToCharArray()) {
		}

		public AnyBaseEncoder(char[] alphabet) {
			_alphabet = alphabet;
			_targetBase = alphabet.Length;
		}

		public AnyBaseEncoder(int radix)
			: this(FindAlphabet(radix)) {
		}

		private static string FindAlphabet(int radix) {
			for (int i = 0; i < Alphabets.Length; i++) {
				if (Alphabets[i].Length >= radix) {
					return Alphabets[i].Substring(0, radix);
				}
			}
			return Base94Alphabet;
		}

		public string Encode(Guid guid) {
			byte[] guidBytes = guid.ToByteArray();
			return Encode(guidBytes);
		}

		public string Encode(byte[] bytes) {
			// make little endian
			Array.Reverse(bytes);
			BigInteger guidInt = new BigInteger(bytes);
			return Encode(guidInt);
		}

		public string Encode(BigInteger bigInt) {
			BigInteger value = bigInt + Min128Inv;
			StringBuilder stringBuilder = new StringBuilder();
			do {
				BigInteger remainder;
				value = BigInteger.DivRem(value, _targetBase, out remainder);
				stringBuilder.Append(_alphabet[(int)remainder]);
			}
			while (value > 0);
			return stringBuilder.ToString();
		}

		public BigInteger Decode(char[] encoded) {
			BigInteger sum = 0;
			int charLen = encoded.Length;
			for (int i = 0; i < charLen; i++) {
				sum += (BigInteger.Pow(_targetBase, i) * Array.IndexOf(_alphabet, encoded[i]));
			}
			return sum - Min128Inv;
		}

		public BigInteger Decode(string encoded) {
			return Decode(encoded.ToCharArray());
		}

		public Guid DecodeGuid(string encoded) {
			BigInteger bigInt = Decode(encoded);
			byte[] bytes = bigInt.ToByteArray();
			Array.Reverse(bytes);
			return new Guid(bytes);
		}

	}
}
