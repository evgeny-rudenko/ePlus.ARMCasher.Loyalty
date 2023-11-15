using Dapper;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.CommonEx;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty.AstraZeneca
{
	internal class AstraZenecaLoyaltyProgramRigla : AstraZenecaLoyaltyProgram
	{
		private List<AllowedBarcode> barcodeCache;

		public AstraZenecaLoyaltyProgramRigla(string publicId, string posId) : base(publicId, posId)
		{
		}

		private void FillBarcodeCache()
		{
			this.barcodeCache = new List<AllowedBarcode>();
			using (SqlConnection sqlConnection = new SqlConnection(MultiServerBL.ClientConnectionString))
			{
				string str = " \r\n                    select \r\n\t                    ID_GOODS_GLOBAL,\r\n                        BARCODE\r\n                            FROM ALLOWED_BARCODE ab                            \r\n                        where DATE_DELETED IS NULL";
				int? nullable = null;
				CommandType? nullable1 = null;
				List<AllowedBarcode> list = sqlConnection.Query<AllowedBarcode>(str, null, null, true, nullable, nullable1).ToList<AllowedBarcode>();
				if (list != null)
				{
					this.barcodeCache = list;
				}
			}
		}

		protected override string GetBarcode(CHEQUE_ITEM item)
		{
			string bARCODE;
			if (this.barcodeCache == null)
			{
				this.FillBarcodeCache();
			}
			AllowedBarcode allowedBarcode = this.barcodeCache.FirstOrDefault<AllowedBarcode>((AllowedBarcode ab) => ab.ID_GOODS_GLOBAL == item.idGoodsGlobal);
			if (allowedBarcode == null)
			{
				bARCODE = null;
			}
			else
			{
				bARCODE = allowedBarcode.BARCODE;
			}
			return bARCODE;
		}
	}
}