using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty.Forms;
using ePlus.ARMCommon.Config;
using ePlus.ARMCommon.Log;
using ePlus.ARMUtils;
using ePlus.CommonEx;
using ePlus.Discount2.BusinessObjects;
using ePlus.Loyalty;
using ePlus.MetaData.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty
{
	public abstract class BaseLoyaltyProgramEx : ILoyaltyProgram
	{
		protected readonly static object SyncObj;

		protected readonly static ConnectionStateSqlLoader<PCX_CHEQUE> PCXChequeLoader;

		protected readonly static ConnectionStateSqlLoader<PCX_CHEQUE_ITEM> PCXChequeItemLoader;

		protected readonly static PCX_QUERY_LOG_BL QueryLogBl;

		protected readonly static PCX_CHEQUE_BL PcxChequeBl;

		protected readonly static PCX_CHEQUE_ITEM_BL PcxChequeItemBl;

		private Stack<LoyaltyTransaction> _transactionStack = new Stack<LoyaltyTransaction>();

		private LoyaltyCardInfo? _loyaltyCardInfo;

		private Stack<LoyaltyTransaction> TransactionStack = new Stack<LoyaltyTransaction>();

		private List<ILpTransResult> operations = new List<ILpTransResult>();

		protected FrmWaiting waitingForm;

		private Regex phoneRegex = new Regex("^[+]{0,1}7[0-9]{10}$");

		protected string ClientId
		{
			get;
			private set;
		}

		protected string ClientPublicId
		{
			get;
			private set;
		}

		protected PublicIdType ClientPublicIdType
		{
			get;
			private set;
		}

		protected string DiscountType
		{
			get;
			private set;
		}

		public abstract Guid IdGlobal
		{
			get;
		}

		private bool IscompatibilityEnabled
		{
			get;
			set;
		}

		public bool IsExplicitDiscount
		{
			get
			{
				return this.OnIsExplicitDiscount;
			}
		}

		protected virtual bool IsInitialized
		{
			get;
			set;
		}

		public virtual bool IsPreOrderCalculationRequired
		{
			get
			{
				return false;
			}
		}

		protected virtual bool IsSettingsInitialized
		{
			get;
			set;
		}

		protected bool IsTransactionProcessing
		{
			get;
			private set;
		}

		public virtual bool IsUpdateLoyaltyCardInfoSupported
		{
			get
			{
				return false;
			}
		}

		public Guid LoyaltyInstance
		{
			get
			{
				return JustDecompileGenerated_get_LoyaltyInstance();
			}
			set
			{
				JustDecompileGenerated_set_LoyaltyInstance(value);
			}
		}

		private Guid JustDecompileGenerated_LoyaltyInstance_k__BackingField;

		public Guid JustDecompileGenerated_get_LoyaltyInstance()
		{
			return this.JustDecompileGenerated_LoyaltyInstance_k__BackingField;
		}

		public void JustDecompileGenerated_set_LoyaltyInstance(Guid value)
		{
			this.JustDecompileGenerated_LoyaltyInstance_k__BackingField = value;
		}

		public ePlus.Loyalty.LoyaltyType LoyaltyType
		{
			get
			{
				return JustDecompileGenerated_get_LoyaltyType();
			}
			set
			{
				JustDecompileGenerated_set_LoyaltyType(value);
			}
		}

		private ePlus.Loyalty.LoyaltyType JustDecompileGenerated_LoyaltyType_k__BackingField;

		public ePlus.Loyalty.LoyaltyType JustDecompileGenerated_get_LoyaltyType()
		{
			return this.JustDecompileGenerated_LoyaltyType_k__BackingField;
		}

		private void JustDecompileGenerated_set_LoyaltyType(ePlus.Loyalty.LoyaltyType value)
		{
			this.JustDecompileGenerated_LoyaltyType_k__BackingField = value;
		}

		public abstract string Name
		{
			get;
		}

		protected virtual bool OnIsExplicitDiscount
		{
			get
			{
				return true;
			}
		}

		public virtual IEnumerable<string> PersonalAdditionsSale
		{
			get;
			set;
		}

		protected int SendRecvTimeout
		{
			get;
			set;
		}

		public virtual int SortOrder
		{
			get
			{
				return 0;
			}
		}

		public virtual bool SuccessCodeConfirmation
		{
			get;
			set;
		}

		static BaseLoyaltyProgramEx()
		{
			BaseLoyaltyProgramEx.SyncObj = new object();
			BaseLoyaltyProgramEx.PCXChequeLoader = new ConnectionStateSqlLoader<PCX_CHEQUE>("PCX_CHEQUE");
			BaseLoyaltyProgramEx.PCXChequeItemLoader = new ConnectionStateSqlLoader<PCX_CHEQUE_ITEM>("PCX_CHEQUE_ITEM");
			BaseLoyaltyProgramEx.QueryLogBl = new PCX_QUERY_LOG_BL();
			BaseLoyaltyProgramEx.PcxChequeBl = new PCX_CHEQUE_BL();
			BaseLoyaltyProgramEx.PcxChequeItemBl = new PCX_CHEQUE_ITEM_BL();
		}

		public BaseLoyaltyProgramEx(ePlus.Loyalty.LoyaltyType loyaltyType, string clientId, string clientPublicId, string discountType)
		{
			this.LoyaltyType = loyaltyType;
			this.ClientId = clientId;
			this.ClientPublicId = clientPublicId;
			this.DiscountType = discountType;
		}

		private void AddOperation(ILpTransResult operation)
		{
			if (operation != null)
			{
				this.operations.Add(operation);
			}
		}

		public void BeginTransaction()
		{
			this.TransactionStack.Clear();
			this.IsTransactionProcessing = true;
		}

		public decimal CalculateDiscountSum(CHEQUE cheque)
		{
			return cheque.CHEQUE_ITEMS.Sum<CHEQUE_ITEM>((CHEQUE_ITEM ci) => ci.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM mi) => {
				if (mi.TYPE == this.DiscountType)
				{
					return mi.AMOUNT;
				}
				return new decimal(0, 0, 0, false, 1);
			}));
		}

		public abstract decimal CalculateMaxSumBonus(CHEQUE cheque);

		public ILpTransResult Charge(CHEQUE cheque, decimal discountSum)
		{
			this.InitInternal();
			ILpTransResult lpTransResult = null;
			using (FrmWaiting frmWaiting = new FrmWaiting())
			{
				frmWaiting.Text = "Начисление";
				frmWaiting.WaitingTimeout = this.SendRecvTimeout;
				frmWaiting.BkWorker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs e) => this.DoCharge(cheque, discountSum, out lpTransResult));
				if (frmWaiting.ShowDialog() != DialogResult.Yes)
				{
					throw frmWaiting.Exception;
				}
			}
			this.AddOperation(lpTransResult);
			return lpTransResult;
		}

		protected virtual void CheckCodeConfirmation(CHEQUE cheque, decimal discountSum)
		{
			this.SuccessCodeConfirmation = true;
		}

		private void ClearOperations()
		{
			this.operations.Clear();
		}

		public virtual void CodeConfirmation(CHEQUE cheque, string code)
		{
		}

		public void Commit()
		{
			this.IsTransactionProcessing = false;
			this.TransactionStack.Clear();
			this.RegisterOperations();
		}

		protected PCX_QUERY_LOG CreateLogQueryLog(Guid idChequeGlobal, decimal sum, int type)
		{
			PCX_QUERY_LOG pCXQUERYLOG = new PCX_QUERY_LOG()
			{
				ID_USER_GLOBAL = SecurityContextEx.USER_GUID,
				ID_QUERY_GLOBAL = Guid.NewGuid(),
				STATE = 1,
				ID_CASH_REGISTER = AppConfigManager.IdCashRegister,
				DATE_REQUEST = DateTime.Now,
				ID_CHEQUE_GLOBAL = idChequeGlobal,
				SUMM = sum,
				TYPE = type,
				CLIENT_ID_TYPE = (int)this.LoyaltyType,
				CLIENT_ID = this.ClientId
			};
			return pCXQUERYLOG;
		}

		protected PCX_CHEQUE CreatePCXCheque(Guid idChequeGlobal, decimal sumMoney, decimal chequeSum, string operType, decimal sumScore)
		{
			PCX_CHEQUE pCXCHEQUE = new PCX_CHEQUE()
			{
				CLIENT_ID = this.ClientId,
				CLIENT_ID_TYPE = (int)this.LoyaltyType,
				SUMM = chequeSum,
				SUMM_MONEY = sumMoney,
				SCORE = new decimal(0),
				SUMM_SCORE = sumScore,
				PARTNER_ID = string.Empty,
				LOCATION = string.Empty,
				TERMINAL = string.Empty,
				ID_CHEQUE_GLOBAL = idChequeGlobal,
				OPER_TYPE = operType,
				CARD_NUMBER = this.GetLoyaltyCardInfo(false).CardNumber
			};
			return pCXCHEQUE;
		}

		private PCX_CHEQUE_ITEM CreatePcxChequeItem(Guid idChequeItemGlobal, decimal quantity, decimal itemSum)
		{
			PCX_CHEQUE_ITEM pCXCHEQUEITEM = new PCX_CHEQUE_ITEM()
			{
				ID_CHEQUE_ITEM_GLOBAL = idChequeItemGlobal,
				QUANTITY = quantity,
				PRICE = UtilsArm.Round(itemSum / quantity),
				SUMM = UtilsArm.RoundDown(itemSum)
			};
			return pCXCHEQUEITEM;
		}

		public ILpTransResult Debit(CHEQUE cheque, decimal discountSum, bool submit)
		{
			decimal num = discountSum;
			this.InitInternal();
			ILpTransResult lpTransResult = null;
			using (FrmWaiting frmWaiting = new FrmWaiting())
			{
				this.waitingForm = frmWaiting;
				frmWaiting.Text = "Списание";
				frmWaiting.WaitingTimeout = this.SendRecvTimeout;
				if (num > new decimal(0))
				{
					this.CheckCodeConfirmation(cheque, num);
					if (!this.SuccessCodeConfirmation)
					{
						num = new decimal(0);
					}
				}
				frmWaiting.BkWorker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs e) => this.DoDebit(cheque, num, out lpTransResult));
				DialogResult dialogResult = frmWaiting.ShowDialog();
				this.waitingForm = null;
				if (dialogResult != DialogResult.Yes)
				{
					throw frmWaiting.Exception;
				}
			}
			this.AddOperation(lpTransResult);
			return lpTransResult;
		}

		protected abstract void DoCharge(CHEQUE cheque, decimal discountSum, out ILpTransResult result);

		protected abstract void DoDebit(CHEQUE cheque, decimal discountSum, out ILpTransResult result);

		protected virtual LoyaltyCardInfo? DoGetLoyaltyCardInfoByCheque(CHEQUE cheque)
		{
			return null;
		}

		protected abstract LoyaltyCardInfo DoGetLoyaltyCardInfoFromService(Form form);

		protected abstract bool DoIsCompatibleTo(Guid discountId);

		protected virtual void DoPreOrderCalculation(CHEQUE cheque)
		{
			this.CalculateMaxSumBonus(cheque);
		}

		[Obsolete]
		protected abstract void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result);

		protected virtual void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, IEnumerable<CHEQUE> refundedCheques, out ILpTransResult result)
		{
			this.DoRefundCharge(baseCheque, returnCheque, out result);
		}

		[Obsolete]
		protected abstract void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result);

		protected virtual void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, IEnumerable<CHEQUE> refundedCheques, out ILpTransResult result)
		{
			this.DoRefundDebit(baseCheque, returnCheque, out result);
		}

		protected abstract void DoRollback(out string slipCheque);

		protected virtual bool DoUpdateLoyaltyCardInfo(LoyaltyCardInfo oldInfo, LoyaltyCardInfo newInfo)
		{
			return false;
		}

		protected virtual string FormatMessage(string message, params object[] obj)
		{
			string str = string.Format(string.Concat(this.Name, ": ", message), obj);
			return str;
		}

		protected decimal GetChequeItemSideDiscountSum(CHEQUE_ITEM chequeItem)
		{
			return chequeItem.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM mi) => {
				if (mi.TYPE == null || !mi.TYPE.StartsWith(this.DiscountType))
				{
					return mi.AMOUNT;
				}
				return new decimal(0, 0, 0, false, 1);
			});
		}

		public virtual string GetDebitOperationDescription()
		{
			return string.Format("Списание баллов по ПЛ \"{0}\"", this.Name);
		}

		protected decimal GetDiscountSum(CHEQUE cheque)
		{
			return cheque.CHEQUE_ITEMS.Sum<CHEQUE_ITEM>((CHEQUE_ITEM ci) => this.GetDiscountSum(ci));
		}

		protected decimal GetDiscountSum(CHEQUE_ITEM chequeItem)
		{
			return chequeItem.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM mi) => {
				if (mi.TYPE != null && mi.TYPE.StartsWith(this.DiscountType))
				{
					return mi.AMOUNT;
				}
				return new decimal(0);
			});
		}

		public virtual LoyaltyCard GetLoyaltyCard()
		{
			return null;
		}

		public LoyaltyCardInfo GetLoyaltyCardInfo(bool resetCache = false)
		{
			this.InitInternal();
			if (!this._loyaltyCardInfo.HasValue || resetCache)
			{
				using (FrmWaiting frmWaiting = new FrmWaiting())
				{
					frmWaiting.Text = "Получение данных по карте";
					frmWaiting.WaitingTimeout = this.SendRecvTimeout;
					frmWaiting.BkWorker.DoWork += new DoWorkEventHandler((object param0, DoWorkEventArgs param1) => this._loyaltyCardInfo = new LoyaltyCardInfo?(this.DoGetLoyaltyCardInfoFromService(frmWaiting)));
					if (frmWaiting.ShowDialog() != DialogResult.Yes)
					{
						throw frmWaiting.Exception;
					}
				}
			}
			return this._loyaltyCardInfo.Value;
		}

		public LoyaltyCardInfo? GetLoyaltyCardInfo(CHEQUE cheque)
		{
			this.InitInternal();
			LoyaltyCardInfo? nullable = null;
			using (FrmWaiting frmWaiting = new FrmWaiting())
			{
				frmWaiting.Text = "Получение данных по карте";
				frmWaiting.WaitingTimeout = this.SendRecvTimeout;
				frmWaiting.BkWorker.DoWork += new DoWorkEventHandler((object param0, DoWorkEventArgs param1) => nullable = this.DoGetLoyaltyCardInfoByCheque(cheque));
				if (frmWaiting.ShowDialog() != DialogResult.Yes)
				{
					throw frmWaiting.Exception;
				}
			}
			if (nullable.HasValue)
			{
				this._loyaltyCardInfo = nullable;
				this.ClientPublicId = nullable.Value.ClientId;
				this.ClientPublicIdType = nullable.Value.ClientIdType;
			}
			return nullable;
		}

		protected string GetLoyaltyOperationDescription(OperTypeEnum loyaltyOperation)
		{
			switch (loyaltyOperation)
			{
				case OperTypeEnum.Debit:
				{
					return "Списание";
				}
				case OperTypeEnum.Charge:
				{
					return "Начисление";
				}
				case OperTypeEnum.DebitRefund:
				{
					return "Возврат списания";
				}
				case OperTypeEnum.ChargeRefund:
				{
					return "Возврат начисления";
				}
				case OperTypeEnum.Rollback:
				{
					return "Откат транзакции";
				}
			}
			return "";
		}

		public virtual LoyaltyParams GetLoyaltyParams()
		{
			return null;
		}

		protected virtual PublicIdType GetPublicIdType(string publicId)
		{
			if (this.phoneRegex.IsMatch(publicId))
			{
				return PublicIdType.Phone;
			}
			if (publicId.Contains("@"))
			{
				return PublicIdType.EMail;
			}
			return PublicIdType.CardNumber;
		}

		protected void InitInternal()
		{
			if (!this.IsSettingsInitialized)
			{
				if (this.ClientPublicIdType == PublicIdType.Unknown)
				{
					this.ClientPublicIdType = this.GetPublicIdType(this.ClientPublicId);
					if (this.ClientPublicIdType == PublicIdType.Phone)
					{
						this.ClientPublicId = this.ClientPublicId.Replace("+", "");
					}
				}
				this.OnInitSettings();
				this.IsSettingsInitialized = true;
			}
			if (this.IsInitialized)
			{
				return;
			}
			lock (BaseLoyaltyProgramEx.SyncObj)
			{
				if (!this.IsInitialized)
				{
					try
					{
						this.OnInitInternal();
					}
					catch (NonCriticalInitializationException nonCriticalInitializationException)
					{
						ARMLogger.Error(nonCriticalInitializationException.ToString());
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendLine("Ошибка инициализации объекта");
						stringBuilder.Append("Работа с программой лояльности \"").Append(this.Name).AppendLine("\" невозможна");
						stringBuilder.AppendLine(exception.Message);
						throw new Exception(stringBuilder.ToString());
					}
					this.IsInitialized = true;
				}
			}
		}

		public bool IsCompatibleTo(Guid discountId)
		{
			this.InitInternal();
			if (this.LoyaltyType.IsUsedAsDiscount())
			{
				return true;
			}
			return this.DoIsCompatibleTo(discountId);
		}

		protected void Log(OperTypeEnum loyaltyOperation, decimal sum, CHEQUE cheque, int? resultCode = null, string transactionId = null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('\"').Append(this.Name).Append('\"');
			stringBuilder.Append(" Номер карты: ").Append(this.ClientPublicId).Append(", ");
			stringBuilder.Append(this.GetLoyaltyOperationDescription(loyaltyOperation));
			stringBuilder.Append(",  Скидка: ").Append(sum.ToString("N2"));
			stringBuilder.Append(" Чек: ").Append(cheque.ID_CHEQUE_GLOBAL);
			if (!string.IsNullOrEmpty(transactionId))
			{
				stringBuilder.Append(" ID транзакции: ").Append(transactionId);
			}
			if (resultCode.HasValue)
			{
				stringBuilder.Append(" Результат операции: ").Append((resultCode.Value < 0 ? "Ошибка: " : "OK: ")).Append(resultCode);
			}
			LoyaltyLogger.Info(stringBuilder.ToString());
		}

		protected void Log(string message)
		{
			LoyaltyLogger.Info(message);
		}

		protected void LogError(OperTypeEnum loyaltyOperation, string message)
		{
			string loyaltyOperationDescription = this.GetLoyaltyOperationDescription(loyaltyOperation);
			object[] name = new object[] { this.Name, this.ClientPublicId, loyaltyOperationDescription, message };
			LoyaltyLogger.Error(string.Format("\"{0}\" Номер карты: {1}, Операция: {2}, Сообщение: {3}", name));
		}

		protected void LogError(OperTypeEnum loyaltyOperation, Exception exception)
		{
			this.LogError(loyaltyOperation, exception.Message);
		}

		protected void LogMsg(OperTypeEnum loyaltyOperation, string message)
		{
			string loyaltyOperationDescription = this.GetLoyaltyOperationDescription(loyaltyOperation);
			object[] name = new object[] { this.Name, this.ClientPublicId, loyaltyOperationDescription, message };
			LoyaltyLogger.Error(string.Format("\"{0}\" Номер карты: {1}, Операция: {2}, Сообщение: {3}", name));
		}

		protected void LogMsg(string message)
		{
			LoyaltyLogger.Error(string.Format("\"{0}\" Номер карты: {1}, Сообщение: {2}", this.Name, this.ClientPublicId, message));
		}

		protected virtual void OnErrorMessage(string message, params object[] obj)
		{
			ARMLogger.Error(this.FormatMessage(message, obj));
		}

		protected virtual void OnInfoMessage(string message, params object[] obj)
		{
			ARMLogger.Info(this.FormatMessage(message, obj));
		}

		protected abstract void OnInitInternal();

		protected abstract void OnInitSettings();

		protected virtual bool OnSendAuthenticationCode()
		{
			return false;
		}

		protected virtual void OnTraceMessage(string message, params object[] obj)
		{
			ARMLogger.Trace(this.FormatMessage(message, obj));
		}

		protected bool PopLastTransaction(out LoyaltyTransaction transaction)
		{
			if (this._transactionStack.Count <= 0)
			{
				transaction = null;
				return false;
			}
			transaction = this._transactionStack.Pop();
			return true;
		}

		public void PreOrderCalculation(CHEQUE cheque)
		{
			if (this.IsExplicitDiscount)
			{
				return;
			}
			this.InitInternal();
			using (FrmWaiting frmWaiting = new FrmWaiting())
			{
				frmWaiting.Text = "Предварительный расчет заказа";
				frmWaiting.WaitingTimeout = this.SendRecvTimeout;
				frmWaiting.BkWorker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs e) => this.DoPreOrderCalculation(cheque));
				if (frmWaiting.ShowDialog() != DialogResult.Yes)
				{
					throw frmWaiting.Exception;
				}
			}
		}

		public ILpTransResult RefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, IEnumerable<CHEQUE> refundCheques)
		{
			this.InitInternal();
			ILpTransResult lpTransResult = null;
			using (FrmWaiting frmWaiting = new FrmWaiting())
			{
				frmWaiting.Text = "Возврат начисления";
				frmWaiting.WaitingTimeout = this.SendRecvTimeout;
				frmWaiting.BkWorker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs e) => this.DoRefundCharge(baseCheque, returnCheque, refundCheques, out lpTransResult));
				if (frmWaiting.ShowDialog() != DialogResult.Yes)
				{
					throw frmWaiting.Exception;
				}
			}
			this.AddOperation(lpTransResult);
			return lpTransResult;
		}

		public ILpTransResult RefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, IEnumerable<CHEQUE> refundedCheques)
		{
			this.InitInternal();
			ILpTransResult lpTransResult = null;
			using (FrmWaiting frmWaiting = new FrmWaiting())
			{
				frmWaiting.Text = "Возврат списания";
				frmWaiting.WaitingTimeout = this.SendRecvTimeout;
				frmWaiting.BkWorker.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs e) => this.DoRefundDebit(baseCheque, returnCheque, refundedCheques, out lpTransResult));
				if (frmWaiting.ShowDialog() != DialogResult.Yes)
				{
					throw frmWaiting.Exception;
				}
			}
			this.AddOperation(lpTransResult);
			return lpTransResult;
		}

		protected virtual void RegisterInDatabase(ILpTransResult result)
		{
			if (result == null || result.IsRegistered)
			{
				return;
			}
			PCX_QUERY_LOG now = this.CreateLogQueryLog(result.IdChequeGlobal, new decimal(0), 0);
			now.DATE_REQUEST = DateTime.Now;
			now.DATE_RESPONSE = now.DATE_REQUEST;
			now.STATE = 4;
			now.STATUS = "Online";
			if (result.ChargedSum > new decimal(0))
			{
				this.SaveLoyaltyInfo(result.IdChequeGlobal, result.ChargedSum, (result.IsRefund ? "DEBIT_REFUND" : "CHARGE"), result.TransactionId);
				now.SUMM = result.ChargedSum;
				now.TYPE = (result.IsRefund ? 4 : 3);
				this.SaveQueryLog(now);
			}
			if (result.DebitSum > new decimal(0))
			{
				PCX_QUERY_LOG debitSum = (PCX_QUERY_LOG)now.Clone();
				debitSum.ID_QUERY_GLOBAL = Guid.NewGuid();
				this.SaveLoyaltyInfo(result.IdChequeGlobal, result.DebitSum, (result.IsRefund ? "CHARGE_REFUND" : "DEBIT"), result.TransactionId);
				debitSum.SUMM = result.DebitSum;
				debitSum.TYPE = (result.IsRefund ? 5 : 2);
				this.SaveQueryLog(debitSum);
			}
		}

		private void RegisterOperations()
		{
			this.operations.ForEach((ILpTransResult op) => this.RegisterInDatabase(op));
		}

		public virtual bool RequestCodeConfirmation(CHEQUE cheque)
		{
			return true;
		}

		public virtual void RequestPersonalAdditionSales()
		{
		}

		public virtual void Rollback(out string slipCheque)
		{
			this.ClearOperations();
			this.DoRollback(out slipCheque);
		}

		protected void SaveLoyaltyInfo(Guid idChequeGlobal, decimal sum, string operationType, string transactionId)
		{
			PCX_CHEQUE pCXCHEQUE = new PCX_CHEQUE()
			{
				ID_CHEQUE_GLOBAL = idChequeGlobal,
				CLIENT_ID = this.ClientPublicId,
				CLIENT_ID_TYPE = (int)this.LoyaltyType,
				SUMM = sum,
				CARD_NUMBER = this.ClientPublicId,
				OPER_TYPE = operationType,
				TRANSACTION_ID = transactionId
			};
			BaseLoyaltyProgramEx.PcxChequeBl.Save(pCXCHEQUE);
		}

		protected void SavePcxCheque(CHEQUE cheque, decimal discountSum, string operationType, string transactionId)
		{
			PCX_CHEQUE pCXCHEQUE = new PCX_CHEQUE()
			{
				CLIENT_ID = this.ClientId,
				CLIENT_ID_TYPE = (int)this.LoyaltyType,
				SUMM = cheque.SUMM + discountSum,
				SUMM_MONEY = cheque.SUMM + discountSum,
				SCORE = new decimal(0),
				SUMM_SCORE = new decimal(0),
				PARTNER_ID = string.Empty,
				LOCATION = string.Empty,
				TERMINAL = string.Empty,
				ID_CHEQUE_GLOBAL = cheque.ID_CHEQUE_GLOBAL,
				OPER_TYPE = operationType,
				CARD_NUMBER = this.ClientPublicId
			};
			PCX_CHEQUE pCXCHEQUE1 = pCXCHEQUE;
			List<PCX_CHEQUE_ITEM> pCXCHEQUEITEMs = new List<PCX_CHEQUE_ITEM>();
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				decimal num = cHEQUEITEM.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM i) => {
					if (i.TYPE == this.DiscountType)
					{
						return i.AMOUNT;
					}
					return new decimal(0, 0, 0, false, 1);
				});
				PCX_CHEQUE_ITEM pCXCHEQUEITEM = new PCX_CHEQUE_ITEM()
				{
					CLIENT_ID = this.ClientId,
					CLIENT_ID_TYPE = (int)this.LoyaltyType,
					ID_CHEQUE_ITEM_GLOBAL = cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL,
					ID_CHEQUE_ITEM_GLOBAL_NEW = cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL,
					OPER_TYPE = operationType,
					PRICE = cHEQUEITEM.PRICE,
					QUANTITY = cHEQUEITEM.QUANTITY,
					STATUS = string.Empty,
					SUMM = num,
					SUMM_SCORE = num,
					TRANSACTION_ID = transactionId
				};
				pCXCHEQUEITEMs.Add(pCXCHEQUEITEM);
			}
			BaseLoyaltyProgramEx.PcxChequeBl.Save(pCXCHEQUE1);
			BaseLoyaltyProgramEx.PcxChequeItemBl.Save(pCXCHEQUEITEMs);
		}

		protected void SaveQueryLog(PCX_QUERY_LOG log)
		{
			BaseLoyaltyProgramEx.QueryLogBl.Save(log);
		}

		protected void SaveQueryLog(Guid idChequeGlobal, decimal summ, pcxOperation operationType, DateTime responseDate)
		{
			this.SaveQueryLog(this.CreateLogQueryLog(idChequeGlobal, summ, (int)operationType));
		}

		protected LoyaltyTransaction SaveTransaction(OperTypeEnum operation, decimal operationSum, LpTransactionData transactionData)
		{
			if (!this.IsTransactionProcessing)
			{
				this._transactionStack.Clear();
			}
			LoyaltyTransaction loyaltyTransaction = new LoyaltyTransaction(operation)
			{
				OperationSum = operationSum,
				Data = transactionData
			};
			LoyaltyTransaction loyaltyTransaction1 = loyaltyTransaction;
			this._transactionStack.Push(loyaltyTransaction1);
			return loyaltyTransaction1;
		}

		public bool SendAuthenticationCode()
		{
			this.InitInternal();
			bool flag = false;
			using (FrmWaiting frmWaiting = new FrmWaiting())
			{
				frmWaiting.Text = "Отправка SMS кода подтверждения...";
				frmWaiting.WaitingTimeout = this.SendRecvTimeout;
				frmWaiting.BkWorker.DoWork += new DoWorkEventHandler((object param0, DoWorkEventArgs param1) => flag = this.OnSendAuthenticationCode());
				if (frmWaiting.ShowDialog() != DialogResult.Yes)
				{
					throw frmWaiting.Exception;
				}
			}
			return flag;
		}

		protected void UpdateClientPublicId(string newClientPublicId)
		{
			this.ClientPublicId = newClientPublicId;
			this.ClientPublicIdType = this.GetPublicIdType(this.ClientPublicId);
		}

		public bool UpdateLoyaltyCardInfo(LoyaltyCardInfo oldInfo, LoyaltyCardInfo newInfo)
		{
			this.InitInternal();
			bool flag = false;
			using (FrmWaiting frmWaiting = new FrmWaiting())
			{
				frmWaiting.Text = "Замена/восстановление карты";
				frmWaiting.WaitingTimeout = this.SendRecvTimeout;
				frmWaiting.BkWorker.DoWork += new DoWorkEventHandler((object param0, DoWorkEventArgs param1) => flag = this.DoUpdateLoyaltyCardInfo(oldInfo, newInfo));
				if (frmWaiting.ShowDialog() != DialogResult.Yes)
				{
					throw frmWaiting.Exception;
				}
			}
			return flag;
		}

		public virtual event EventHandler CheckCodeConfirmationEvent;
	}
}