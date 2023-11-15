using ePlus.ARMBusinessLogic;
using ePlus.CommonEx;
using ePlus.MetaData.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ePlus.ARMCasher.Loyalty.AstraZeneca
{
	internal class AzTransactionsBl
	{
		private SqlLoaderEx<CHEQUE_ITEM_TRANSACTION> loader = new SqlLoaderEx<CHEQUE_ITEM_TRANSACTION>();

		private DataService_BL dataService = new DataService_BL(MultiServerBL.ClientConnectionString);

		public AzTransactionsBl()
		{
		}

		public IEnumerable<CHEQUE_ITEM_TRANSACTION> GetListUnconfirmed()
		{
			string str = string.Format("EXEC USP_CHEQUE_ITEM_TRANSACTION_AZ_LOAD", new object[0]);
			DataSet dataSet = this.dataService.Execute(str);
			List<CHEQUE_ITEM_TRANSACTION> list = this.loader.GetList(dataSet.Tables[0]) ?? new List<CHEQUE_ITEM_TRANSACTION>();
			return list;
		}

		public void SaveEx(IEnumerable<CHEQUE_ITEM_TRANSACTION> transactions)
		{
			string str = "#CHEQUE_ITEM_TRANSACTION_AZ";
			using (SqlConnection sqlConnection = new SqlConnection(MultiServerBL.ClientConnectionString))
			{
				using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(sqlConnection))
				{
					using (DataTable dataTable = this.loader.CreateDataTable(transactions))
					{
						using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
						{
							using (SqlCommand sqlCommand1 = sqlConnection.CreateCommand())
							{
								sqlCommand.CommandText = this.loader.GenerateCreateTableScript(str);
								sqlConnection.Open();
								sqlCommand.ExecuteNonQuery();
								sqlBulkCopy.DestinationTableName = str;
								sqlBulkCopy.WriteToServer(dataTable);
								sqlCommand1.CommandText = "EXEC USP_CHEQUE_ITEM_TRANSACTION_AZ_SAVE";
								sqlCommand1.ExecuteNonQuery();
							}
						}
					}
				}
			}
		}
	}
}