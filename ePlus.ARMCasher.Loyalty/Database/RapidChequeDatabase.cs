using ePlus.CommonEx;
using ePlus.MetaData.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace ePlus.ARMCasher.Loyalty.Database
{
	internal class RapidChequeDatabase
	{
		private readonly SqlLoader<RapidCheque> _loader = new SqlLoader<RapidCheque>();

		public RapidChequeDatabase()
		{
		}

		private void Delete(RapidCheque item)
		{
			string str = string.Format("DELETE from [RAPID_CHEQUE] where [ClientId] = '{0}' and [ChequeId] = '{1}' and [RequestId] = '{2}'", item.ClientId, item.ChequeId, item.RequestId);
			SqlCommand sqlCommand = new SqlCommand(str)
			{
				CommandType = CommandType.Text
			};
			MultiServerBL.RunSqlCommand(ref sqlCommand);
		}

		public void DeleteDiscount(Guid chequeId)
		{
			SqlCommand sqlCommand = new SqlCommand("\r\n                delete from DISCOUNT2_MAKE_ITEM\r\n                where ID_CHEQUE_ITEM_GLOBAL in \r\n                (\r\n\t                select ID_CHEQUE_ITEM_GLOBAL from CHEQUE_ITEM where ID_CHEQUE_GLOBAL = '{0}'\r\n                )\r\n                and ID_DISCOUNT2_CARD_GLOBAL = '{1}'\r\n            ")
			{
				CommandType = CommandType.Text
			};
			MultiServerBL.RunSqlCommand(ref sqlCommand, ServerType.Local);
		}

		public RapidCheque Load(string clientId, string chequeId)
		{
			string str = string.Format("SELECT [RequestId], [ChequeId], [clientId], [Operation], [Summ], [Date] from [RAPID_CHEQUE] where [ChequeId] = '{0}' and [ClientId] = '{1}' order by [Date] desc ", chequeId.ToUpper(), clientId.ToUpper());
			SqlCommand sqlCommand = new SqlCommand(str)
			{
				CommandType = CommandType.Text
			};
			DataSet dataSet = MultiServerBL.RunSqlCommand(ref sqlCommand);
			List<RapidCheque> list = this._loader.GetList(dataSet.Tables[0]);
			if (list.Count <= 0)
			{
				return null;
			}
			return list[0];
		}

		public RapidCheque Load(Guid chequeId)
		{
			string str = string.Format("SELECT [RequestId], [ChequeId], [clientId], [Operation], [Summ], [Date] from [RAPID_CHEQUE] where [ChequeId] = '{0}' order by [Date] desc ", chequeId.ToString().ToUpper());
			SqlCommand sqlCommand = new SqlCommand(str)
			{
				CommandType = CommandType.Text
			};
			DataSet dataSet = MultiServerBL.RunSqlCommand(ref sqlCommand);
			List<RapidCheque> list = this._loader.GetList(dataSet.Tables[0]);
			if (list.Count <= 0)
			{
				return null;
			}
			return list[0];
		}

		public void Save(RapidCheque item)
		{
			NumberFormatInfo numberFormatInfo = new NumberFormatInfo()
			{
				NumberDecimalSeparator = "."
			};
			object[] chequeId = new object[] { item.ChequeId, item.ClientId, item.Operation, null, null, null };
			chequeId[3] = item.Summ.ToString(numberFormatInfo);
			chequeId[4] = item.Date;
			chequeId[5] = item.RequestId;
			SqlCommand sqlCommand = new SqlCommand(string.Format("INSERT INTO [RAPID_CHEQUE] ([ChequeId], [ClientId], [Operation], [Summ], [Date], [RequestId]) values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", chequeId))
			{
				CommandType = CommandType.Text
			};
			SqlCommand sqlCommand1 = sqlCommand;
			MultiServerBL.RunSqlCommand(ref sqlCommand1);
		}

		public void Update(RapidCheque item)
		{
			string str;
			if (item.Operation == "ROLLBACK")
			{
				this.Delete(item);
				return;
			}
			string str1 = "\r\n                select 1 from [RAPID_CHEQUE]\r\n                where [ClientId] = '{0}' and [ChequeId] = '{1}' and [RequestId] = '{2}' \r\n            ";
			str1 = string.Format(str1, item.ClientId, item.ChequeId, item.RequestId);
			SqlCommand sqlCommand = new SqlCommand(str1)
			{
				CommandType = CommandType.Text
			};
			DataSet dataSet = MultiServerBL.RunSqlCommand(ref sqlCommand);
			if (dataSet == null || dataSet.Tables.Count <= 0 || dataSet.Tables[0].Rows.Count <= 0)
			{
				str = "\r\n                    insert into [RAPID_CHEQUE] ([Operation], [Date], [ClientId], [ChequeId], [RequestId])\r\n                    values ('{0}', '{1}', '{2}', '{3}', '{4}')\r\n                ";
				object[] operation = new object[] { item.Operation, item.Date, item.ClientId, item.ChequeId, item.RequestId };
				str = string.Format(str, operation);
			}
			else
			{
				object[] objArray = new object[] { item.Operation, item.Date, item.ClientId, item.ChequeId, item.RequestId };
				str = string.Format("UPDATE [RAPID_CHEQUE] set [Operation] = '{0}', [Date] = '{1}' where [ClientId] = '{2}' and [ChequeId] = '{3}' and [RequestId] = '{4}'", objArray);
			}
			sqlCommand = new SqlCommand(str);
			MultiServerBL.RunSqlCommand(ref sqlCommand);
		}
	}
}