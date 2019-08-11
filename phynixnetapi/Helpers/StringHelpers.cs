using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phynixnetapi.Helpers
{
	public static class StringHelper
	{
		public static String EncodeString(this string value)
		{
			try
			{
				if (value != null)
				{
					var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(value);
					return System.Convert.ToBase64String(plainTextBytes);
				}
				return value;
			}
			catch (Exception)
			{
				return value;
			}
		}

		public static string DecodeString(this string value)
		{
			if (value != null)
			{
				var base64EncodedBytes = System.Convert.FromBase64String(value);
				return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
			}
			return value;
		}
	}
}
