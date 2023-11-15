using ePlus.Loyalty;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace ePlus.ARMCasher.Loyalty
{
	public abstract class LpTransResultBase : ILpTransResult
	{
		public decimal Balance
		{
			get
			{
				return JustDecompileGenerated_get_Balance();
			}
			set
			{
				JustDecompileGenerated_set_Balance(value);
			}
		}

		private decimal JustDecompileGenerated_Balance_k__BackingField;

		public decimal JustDecompileGenerated_get_Balance()
		{
			return this.JustDecompileGenerated_Balance_k__BackingField;
		}

		public void JustDecompileGenerated_set_Balance(decimal value)
		{
			this.JustDecompileGenerated_Balance_k__BackingField = value;
		}

		public decimal ChargedSum
		{
			get
			{
				return JustDecompileGenerated_get_ChargedSum();
			}
			set
			{
				JustDecompileGenerated_set_ChargedSum(value);
			}
		}

		private decimal JustDecompileGenerated_ChargedSum_k__BackingField;

		public decimal JustDecompileGenerated_get_ChargedSum()
		{
			return this.JustDecompileGenerated_ChargedSum_k__BackingField;
		}

		public void JustDecompileGenerated_set_ChargedSum(decimal value)
		{
			this.JustDecompileGenerated_ChargedSum_k__BackingField = value;
		}

		public decimal DebitSum
		{
			get
			{
				return JustDecompileGenerated_get_DebitSum();
			}
			set
			{
				JustDecompileGenerated_set_DebitSum(value);
			}
		}

		private decimal JustDecompileGenerated_DebitSum_k__BackingField;

		public decimal JustDecompileGenerated_get_DebitSum()
		{
			return this.JustDecompileGenerated_DebitSum_k__BackingField;
		}

		public void JustDecompileGenerated_set_DebitSum(decimal value)
		{
			this.JustDecompileGenerated_DebitSum_k__BackingField = value;
		}

		public string ID
		{
			get;
			set;
		}

		public Guid IdChequeGlobal
		{
			get
			{
				return JustDecompileGenerated_get_IdChequeGlobal();
			}
			set
			{
				JustDecompileGenerated_set_IdChequeGlobal(value);
			}
		}

		private Guid JustDecompileGenerated_IdChequeGlobal_k__BackingField;

		public Guid JustDecompileGenerated_get_IdChequeGlobal()
		{
			return this.JustDecompileGenerated_IdChequeGlobal_k__BackingField;
		}

		public void JustDecompileGenerated_set_IdChequeGlobal(Guid value)
		{
			this.JustDecompileGenerated_IdChequeGlobal_k__BackingField = value;
		}

		public bool IsRefund
		{
			get
			{
				return JustDecompileGenerated_get_IsRefund();
			}
			set
			{
				JustDecompileGenerated_set_IsRefund(value);
			}
		}

		private bool JustDecompileGenerated_IsRefund_k__BackingField;

		public bool JustDecompileGenerated_get_IsRefund()
		{
			return this.JustDecompileGenerated_IsRefund_k__BackingField;
		}

		public void JustDecompileGenerated_set_IsRefund(bool value)
		{
			this.JustDecompileGenerated_IsRefund_k__BackingField = value;
		}

		public bool IsRegistered
		{
			get
			{
				return JustDecompileGenerated_get_IsRegistered();
			}
			set
			{
				JustDecompileGenerated_set_IsRegistered(value);
			}
		}

		private bool JustDecompileGenerated_IsRegistered_k__BackingField;

		public bool JustDecompileGenerated_get_IsRegistered()
		{
			return this.JustDecompileGenerated_IsRegistered_k__BackingField;
		}

		public void JustDecompileGenerated_set_IsRegistered(bool value)
		{
			this.JustDecompileGenerated_IsRegistered_k__BackingField = value;
		}

		public LoyaltyType LpType
		{
			get;
			set;
		}

		public string PointsTitle
		{
			get;
			set;
		}

		public bool RoundingInCheque
		{
			get;
			set;
		}

		public string TransactionId
		{
			get
			{
				return JustDecompileGenerated_get_TransactionId();
			}
			set
			{
				JustDecompileGenerated_set_TransactionId(value);
			}
		}

		private string JustDecompileGenerated_TransactionId_k__BackingField;

		public string JustDecompileGenerated_get_TransactionId()
		{
			return this.JustDecompileGenerated_TransactionId_k__BackingField;
		}

		public void JustDecompileGenerated_set_TransactionId(string value)
		{
			this.JustDecompileGenerated_TransactionId_k__BackingField = value;
		}

		protected LpTransResultBase()
		{
		}

		protected abstract ILpTransResult Create(string id, decimal chargedSum, decimal debitSum, decimal balance, string pointsTitle, bool isRefund);

		protected virtual string Format(decimal value)
		{
			string empty = string.Empty;
			if (!this.RoundingInCheque)
			{
				return value.ToString();
			}
			return Math.Truncate(value).ToString("");
		}

		public virtual ILpTransResult Merge(ILpTransResult result)
		{
			ILpTransResult lpTransResult = this.Create(this.ID, result.ChargedSum + this.ChargedSum, result.DebitSum + this.DebitSum, result.Balance, this.PointsTitle, this.IsRefund);
			return lpTransResult;
		}

		public virtual string ToSlipCheque(string header = null, string footer = null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(header))
			{
				stringBuilder.AppendLine(header);
			}
			stringBuilder.Append("ШК ").Append(this.ID).AppendLine();
			stringBuilder.AppendFormat("Списано {0}: {1}", this.PointsTitle, this.Format(this.DebitSum)).AppendLine();
			stringBuilder.AppendFormat("Начислено {0}: {1}", this.PointsTitle, this.Format(this.ChargedSum)).AppendLine();
			stringBuilder.AppendFormat("Баланс {0}: {1}", this.PointsTitle, this.Format(this.Balance)).AppendLine();
			if (!string.IsNullOrEmpty(footer))
			{
				stringBuilder.AppendLine(footer);
			}
			return stringBuilder.ToString();
		}
	}
}