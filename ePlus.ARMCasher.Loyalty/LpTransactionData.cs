using System;
using System.Runtime.CompilerServices;

namespace ePlus.ARMCasher.Loyalty
{
	public class LpTransactionData
	{
		public Guid ChequeID
		{
			get;
			private set;
		}

		public Guid ChequeOperationType
		{
			get;
			private set;
		}

		public LpTransactionData(Guid chequeID, Guid chequeOperationType)
		{
			this.ChequeID = chequeID;
			this.ChequeOperationType = chequeOperationType;
		}
	}
}