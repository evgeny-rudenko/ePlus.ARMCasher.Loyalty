using ePlus.CommonEx;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace ePlus.ARMCasher.Loyalty.GoldenMiddle
{
	public class GoldenMiddle_Bl
	{
		public GoldenMiddle_Bl()
		{
		}

		public Dictionary<long, string> GetGoodsGroups(IEnumerable<long> goodsIds)
		{
			Dictionary<long, string> nums = new Dictionary<long, string>();
			object[] xElement = new object[] { new XElement("XML", 
				from g in goodsIds
				select new XElement("ID_GOODS", g.ToString())) };
			XDocument xDocument = new XDocument(xElement);
			using (SqlConnection sqlConnection = new SqlConnection(MultiServerBL.ClientConnectionString))
			{
				using (SqlCommand sqlCommand = new SqlCommand("USP_GM_GET_GOODS_GROUPS", sqlConnection)
				{
					CommandType = CommandType.StoredProcedure
				})
				{
					sqlCommand.Parameters.AddWithValue("XML_DATA", xDocument.ToString());
					sqlConnection.Open();
					using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
					{
						int ordinal = sqlDataReader.GetOrdinal("ID_GOODS");
						int num = sqlDataReader.GetOrdinal("GROUP_NAME");
						while (sqlDataReader.Read())
						{
							long num1 = sqlDataReader.GetInt64(ordinal);
							string str = sqlDataReader.GetString(num);
							int num2 = str.IndexOf('[');
							int num3 = str.IndexOf(']');
							try
							{
								str = str.Substring(num2 + 1, num3 - num2 - 1);
							}
							catch
							{
								throw new Exception(string.Format("Не удалось получить категорию товара из группы\"{0}\"", str));
							}
							nums.Add(num1, str);
						}
					}
				}
			}
			return nums;
		}
	}
}