using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace BillSplitter.Common
{
	public static class Extensions
	{
		public static string Serialize<T>(this T source) where T : class
		{
			string result = null;
			using (var ms = new MemoryStream())
			{
				var serializerDcj = new DataContractJsonSerializer(source.GetType());
				serializerDcj.WriteObject(ms, source);
				ms.Seek(0, SeekOrigin.Begin);
				result = new StreamReader(ms).ReadToEnd();
			}
			return result;
		}

		public static T Deserialize<T>(this string source) where T : class
		{
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(source)))
			{
				var serializerDcj = new DataContractJsonSerializer(typeof(T));
				return serializerDcj.ReadObject(ms) as T;
			}
		}
	}
}
