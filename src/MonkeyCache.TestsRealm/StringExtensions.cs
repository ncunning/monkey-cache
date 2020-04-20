using System;
using System.Security.Cryptography;
using System.Text;

namespace MonkeyCache.Helpers
{
	public static class StringExtensions
	{
		/// <summary>
		/// Checks if the provided string has any value in it or not
		/// </summary>
		/// <param name="strng">Input string</param>
		/// <returns>True if string is null or empty or white space, False otherwise</returns>
		public static bool IsEmpty(this string strng)
		{
			return string.IsNullOrEmpty(strng) || string.IsNullOrWhiteSpace(strng);
		}

		/// <summary>
		/// Checks if the provided string has any value in it or not
		/// </summary>
		/// <param name="strng">Input string</param>
		/// <returns>True if string has value, False if string is Null, Empty or Whitespace</returns>
		public static bool IsNotEmpty(this string strng)
		{
			return !(string.IsNullOrEmpty(strng) || string.IsNullOrWhiteSpace(strng));
		}

		/// <summary>
		/// Converts provided string to it's SHA Hash string
		/// </summary>
		/// <param name="strng">Input string</param>
		/// <returns>SHA Hash string</returns>
		public static string ToSHAHash(this string strng)
		{
			StringBuilder stringBuilder = new StringBuilder();
			using (var hash = SHA256.Create())
			{
				Encoding enc = Encoding.UTF8;
				var result = hash.ComputeHash(enc.GetBytes(strng));
				foreach (var b in result)
				{
					stringBuilder.Append(b.ToString("x2"));
				}
			}

			var shaHash = stringBuilder.ToString().ToUpper();
			return shaHash;
		}
	}
}