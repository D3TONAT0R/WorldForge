using System;
using System.Collections.Generic;
using System.Text;

namespace MCUtils {
	public static class Extensions {

		public static string Last(this string[] arr) {
			return arr[arr.Length - 1];
		}
	}
}
