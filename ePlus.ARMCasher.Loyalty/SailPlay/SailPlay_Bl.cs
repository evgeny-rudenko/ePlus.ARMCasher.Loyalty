using ePlus.CommonEx;
using System;
using System.Data;
using System.Data.SqlClient;

namespace ePlus.ARMCasher.Loyalty.SailPlay
{
	public class SailPlay_Bl
	{
		public SailPlay_Bl()
		{
		}

		public void SaveRegisterCardMyHealth(long idUser)
		{
			using (SqlConnection sqlConnection = new SqlConnection(MultiServerBL.ClientConnectionString))
			{
				using (SqlCommand sqlCommand = new SqlCommand("USP_REGISTRY_MZ_SAVE", sqlConnection)
				{
					CommandType = CommandType.StoredProcedure
				})
				{
					sqlCommand.Parameters.AddWithValue("ID_USER_DATA", idUser);
					sqlConnection.Open();
					sqlCommand.ExecuteNonQuery();
				}
			}
		}
	}
}