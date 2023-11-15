using ePlus.ARMCasher.Loyalty;
using System;
using System.Runtime.CompilerServices;
using winpcxLib;

namespace ePlus.ARMCasher.Loyalty.PCX
{
	internal class PCXTransactionData : LpTransactionData
	{
		public winpcxTransaction Transaction
		{
			get;
			private set;
		}

		public PCXTransactionData(Guid chequeID, Guid chequeOperationType, winpcxTransaction pcxTransaction) : base(chequeID, chequeOperationType)
		{
			this.Transaction = pcxTransaction;
		}
	}
}