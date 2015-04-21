using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeSup.Utilities.BaseN {
	[TestClass]
	public class AnyBaseEncoderTest {
		public static readonly AnyBaseEncoder Base36Encoder = new AnyBaseEncoder(36);

		[TestMethod]
		public void TestEncode() {
			//printAll();
			PrintMinMax(128);
			PrintMinMax(64);
			PrintMinMax(32);
			PrintMinMax(16);
			PrintMinMax(8);
			Guid guid = Guid.NewGuid();
			Console.WriteLine("GUID: {0}", guid);
			string encoded = Base36Encoder.Encode(guid);
			Console.WriteLine("GUID enc: {0}", encoded);
			Console.WriteLine("GUID dec: {0}", Base36Encoder.DecodeGuid(encoded));
			Console.ReadLine();
		}

		private void printAll() {
			BigInteger min128 = -(new BigInteger(1) << 127);
			BigInteger max128 = -min128 - 1;
			long min64 = long.MinValue;
			long max64 = long.MaxValue;
			int min32 = int.MinValue;
			int max32 = int.MaxValue;
			PrintInt("min128", min128);
			PrintInt("bigInteger", max128);
			PrintInt("min64", min64);
			PrintInt("max64", max64);
			PrintInt("min32", min32);
			PrintInt("max32", max32);
			PrintInt("zero", 0);
		}

		public void PrintMinMax(int numBits) {
			BigInteger min = -(new BigInteger(1) << (numBits - 1));
			BigInteger max = -min - 1;
			BigInteger umax = (new BigInteger(1) << numBits) - 1;

			PrintInt("min" + numBits, min);
			PrintInt("max" + numBits, max);
			PrintInt("umax" + numBits, umax);
		}

		public void PrintInt(string name, BigInteger bigInteger) {
			Console.WriteLine("{1}(16): {0:X}", bigInteger, name);
			Console.WriteLine("{1}(10): {0}", bigInteger, name);
			string encoded = Base36Encoder.Encode(bigInteger);
			Console.WriteLine("enc {1}(36): {0}", encoded, name);
			Console.WriteLine("Encoded version takes {0} chars.", encoded.Length);
			BigInteger decoded = Base36Encoder.Decode(encoded);
			Console.WriteLine("dec {1}(10): {0}", decoded, name);
		}


	}
}
