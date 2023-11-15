using BELLib;
using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher.BusinessLogic;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty.LSPoint.Forms;
using ePlus.CommonEx;
using ePlus.KKMWrapper;
using ePlus.LSPoint.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.LSPoint
{
	internal class LSPointLoyaltyProgram
	{
		public IList<LSPointLoyaltyProgram.StockDetailInfo> NewItemsList = new List<LSPointLoyaltyProgram.StockDetailInfo>();

		private readonly LSPointSettings _lspointSettings;

		private readonly LSPointSettingsBl _lspointSettingsBl = new LSPointSettingsBl();

		private readonly STOCK_DETAIL_BL stockDetailBl = new STOCK_DETAIL_BL();

		private static Bel _belForm;

		private string _cardNumber;

		private CHEQUE _cheque;

		public bool ChequePrinted;

		public long BPRRN
		{
			get
			{
				return LSPointLoyaltyProgram._belForm.BPRRN;
			}
		}

		public string BpSId
		{
			get
			{
				return LSPointLoyaltyProgram._belForm.BpSId.Text;
			}
		}

		public string CardNumber
		{
			get
			{
				return this._cardNumber;
			}
			set
			{
				this._cardNumber = value;
			}
		}

		public string ECROpId
		{
			get
			{
				return LSPointLoyaltyProgram._belForm.ECROpId.Text;
			}
		}

		public decimal IncomeBonus
		{
			get;
			private set;
		}

		public decimal OutcomeBonus
		{
			get;
			private set;
		}

		public decimal PaidBonus
		{
			get
			{
				return LSPointLoyaltyProgram._belForm.AlreadyPayBonus;
			}
		}

		public decimal PaidCard
		{
			get
			{
				return LSPointLoyaltyProgram._belForm.AlreadyPayCard;
			}
		}

		public decimal PaidCash
		{
			get
			{
				return LSPointLoyaltyProgram._belForm.AlreadyPayCash;
			}
		}

		static LSPointLoyaltyProgram()
		{
			LSPointLoyaltyProgram._belForm = new Bel();
		}

		public LSPointLoyaltyProgram()
		{
			if (!AppConfigurator.EnableLSPoint || MultiServerBL.OnlineState == OnlineState.Offline || MultiServerBL.OnlineState == OnlineState.Disconnected)
			{
				return;
			}
			this._lspointSettings = this._lspointSettingsBl.Load(ServerType.Server) ?? new LSPointSettings();
			if (LSPointLoyaltyProgram._belForm.InitSuccess)
			{
				LSPointLoyaltyProgram._belForm.Check_UseKLProtocol.Checked = true;
			}
		}

		public void AddGoods(GoodsInfo info)
		{
			if (LSPointLoyaltyProgram._belForm.AddGoods(info))
			{
				LSPointLoyaltyProgram._belForm.GoodsList.Add(info);
			}
		}

		public bool EnterCardInfo(CHEQUE cheque)
		{
			bool flag;
			this._cheque = cheque;
			using (EnterCardInfoForm enterCardInfoForm = new EnterCardInfoForm(this))
			{
				enterCardInfoForm.StartPosition = FormStartPosition.CenterParent;
				if (enterCardInfoForm.ShowDialog() != DialogResult.OK)
				{
					return false;
				}
				else
				{
					flag = true;
				}
			}
			return flag;
		}

		public void Info()
		{
			while (LSPointLoyaltyProgram._belForm.Bpecr1.IsOperationRunning == 1)
			{
				Thread.Sleep(100);
			}
			LSPointLoyaltyProgram._belForm.Info();
			LSPointLoyaltyProgram.ProcessScreenPrinterMessages();
		}

		private bool ModifyCheque()
		{
			if (Bel.Instance.Bpecr1.goodCount <= 0)
			{
				return true;
			}
			Bel.Instance.Bpecr1.GetFirstGood();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < Bel.Instance.Bpecr1.goodCount; i++)
			{
				string bpecr1 = (string)Bel.Instance.Bpecr1.goodCode;
				bool flag = false;
				foreach (CHEQUE_ITEM cHEQUEITEM in this._cheque.CHEQUE_ITEMS)
				{
					if (cHEQUEITEM.CODE != bpecr1)
					{
						continue;
					}
					cHEQUEITEM.QUANTITY = (int)(Bel.Instance.Bpecr1.goodQuantity / new decimal(1000));
					flag = true;
					break;
				}
				if (!flag)
				{
					if ((string)Bel.Instance.Bpecr1.goodCode != "100000")
					{
						IEnumerable<STOCK_DETAIL> sTOCKDETAILs = this.stockDetailBl.List(null, new decimal(0), (string)Bel.Instance.Bpecr1.goodCode, null, null, DateTime.MinValue, null, true, null, (long)0, true);
						if (sTOCKDETAILs.Any<STOCK_DETAIL>())
						{
							LSPointLoyaltyProgram.StockDetailInfo stockDetailInfo = new LSPointLoyaltyProgram.StockDetailInfo()
							{
								StockDetail = sTOCKDETAILs.First<STOCK_DETAIL>(),
								Quantity = Bel.Instance.Bpecr1.goodQuantity
							};
							this.NewItemsList.Add(stockDetailInfo);
						}
					}
					else
					{
						decimal num = Bel.Instance.Bpecr1.goodQuantity / new decimal(1000);
						string str = (string)Bel.Instance.Bpecr1.goodName;
						stringBuilder.AppendFormat("1/1 {1}\n{0:0.00} x 1,00 = {0:0.00}\r\n", num, str);
					}
				}
				if (i < Bel.Instance.Bpecr1.goodCount)
				{
					Bel.Instance.Bpecr1.GetNextGood();
				}
			}
			if (AppConfigurator.KKMSettings.kkmEnable)
			{
				KkmWrapper.Driver.PrintNonFiscalDoc(stringBuilder.ToString(), false);
			}
			return true;
		}

		public bool PartialReturnPromo(CHEQUE baseCheque, CHEQUE cheque, PCX_CHEQUE pcxCheque)
		{
			this._cheque = baseCheque;
			LSPointLoyaltyProgram._belForm.MagTrack2Field.Text = pcxCheque.CLIENT_ID;
			decimal num = new decimal(0);
			if (this._cheque != null && this._cheque.CHEQUE_ITEMS != null && this._cheque.CHEQUE_ITEMS.Count > 0)
			{
				foreach (CHEQUE_ITEM cHEQUEITEM in this._cheque.CHEQUE_ITEMS)
				{
					GoodsInfo goodsInfo = new GoodsInfo()
					{
						BarCode = cHEQUEITEM.CODE,
						Flags = 0,
						Name = cHEQUEITEM.GOODS_NAME,
						Price = cHEQUEITEM.LOT_PRICE_VAT
					};
					num += cHEQUEITEM.LOT_PRICE_VAT;
					goodsInfo.Quantity = cHEQUEITEM.QUANTITY;
					this.AddGoods(goodsInfo);
				}
			}
			LSPointLoyaltyProgram._belForm.BpRrnField.Text = pcxCheque.TRANSACTION_ID;
			LSPointLoyaltyProgram._belForm.AmountForCancel.Text = num.ToString(CultureInfo.InvariantCulture);
			while (LSPointLoyaltyProgram._belForm.Bpecr1.IsOperationRunning == 1)
			{
				Thread.Sleep(100);
			}
			if (!LSPointLoyaltyProgram._belForm.IsPromoEnabled || !LSPointLoyaltyProgram._belForm.OnBpRrnCancel())
			{
				return false;
			}
			LSPointLoyaltyProgram.ProcessScreenPrinterMessages();
			if (cheque == null || cheque.CHEQUE_ITEMS.Count <= 0)
			{
				return true;
			}
			this._cheque = cheque;
			return this.Promo();
		}

		public bool PerformBprrnCancel(long BPRRN)
		{
			LSPointLoyaltyProgram._belForm.BpRrnField.Text = BPRRN.ToString();
			while (LSPointLoyaltyProgram._belForm.Bpecr1.IsOperationRunning == 1)
			{
				Thread.Sleep(100);
			}
			bool flag = LSPointLoyaltyProgram._belForm.OnBpRrnCancel();
			LSPointLoyaltyProgram.ProcessScreenPrinterMessages();
			return flag;
		}

		public bool PerformRollback(string BpSId, string ECROpId)
		{
			LSPointLoyaltyProgram._belForm.BpSId.Text = BpSId;
			LSPointLoyaltyProgram._belForm.ECROpId.Text = ECROpId;
			while (LSPointLoyaltyProgram._belForm.Bpecr1.IsOperationRunning == 1)
			{
				Thread.Sleep(100);
			}
			bool flag = LSPointLoyaltyProgram._belForm.OnPerformRollback(BpSId, ECROpId);
			LSPointLoyaltyProgram.ProcessScreenPrinterMessages();
			return flag;
		}

		private static void ProcessScreenPrinterMessages()
		{
			if (!string.IsNullOrEmpty(Bel.Instance.ScreenText.Text))
			{
				MessageBox.Show(Bel.Instance.ScreenText.Text);
			}
			if (!string.IsNullOrEmpty(Bel.Instance.PrnText.Text) && AppConfigurator.KKMSettings.kkmEnable)
			{
				KkmWrapper.Driver.PrintNonFiscalDoc(Bel.Instance.PrnText.Text, false);
			}
		}

		public bool Promo()
		{
			while (LSPointLoyaltyProgram._belForm.Bpecr1.IsOperationRunning == 1)
			{
				Thread.Sleep(100);
			}
			if (this._cheque != null && this._cheque.CHEQUE_ITEMS != null && this._cheque.CHEQUE_ITEMS.Count > 0)
			{
				foreach (CHEQUE_ITEM cHEQUEITEM in this._cheque.CHEQUE_ITEMS)
				{
					GoodsInfo goodsInfo = new GoodsInfo()
					{
						BarCode = cHEQUEITEM.CODE,
						Flags = 0,
						Name = cHEQUEITEM.GOODS_NAME,
						Price = cHEQUEITEM.LOT_PRICE_VAT,
						Quantity = cHEQUEITEM.QUANTITY
					};
					this.AddGoods(goodsInfo);
				}
			}
			while (LSPointLoyaltyProgram._belForm.Bpecr1.IsOperationRunning == 1)
			{
				Thread.Sleep(100);
			}
			if (!LSPointLoyaltyProgram._belForm.IsPromoEnabled || !LSPointLoyaltyProgram._belForm.Promo() || LSPointLoyaltyProgram._belForm.IsCancelled)
			{
				return false;
			}
			LSPointLoyaltyProgram.ProcessScreenPrinterMessages();
			PCX_CHEQUE_BL pCXCHEQUEBL = new PCX_CHEQUE_BL();
			PCX_CHEQUE pCXCHEQUE = new PCX_CHEQUE()
			{
				CLIENT_ID = LSPointLoyaltyProgram._belForm.MagTrack2Field.Text,
				CLIENT_ID_TYPE = 10,
				SCORE = new decimal(0),
				PARTNER_ID = string.Empty,
				LOCATION = string.Empty,
				TERMINAL = string.Empty
			};
			long bPRRN = LSPointLoyaltyProgram._belForm.BPRRN;
			pCXCHEQUE.TRANSACTION_ID = bPRRN.ToString(CultureInfo.InvariantCulture);
			pCXCHEQUE.ID_CHEQUE_GLOBAL = (this._cheque != null ? this._cheque.ID_CHEQUE_GLOBAL : Guid.Empty);
			pCXCHEQUE.SUMM_SCORE = this.PaidBonus / new decimal(100);
			pCXCHEQUE.SUMM = ((this.PaidCash + this.PaidCard) + this.PaidBonus) / new decimal(100);
			pCXCHEQUE.SUMM_MONEY = this.PaidCash / new decimal(100);
			pCXCHEQUE.CARD_SCORE = this.PaidCard / new decimal(100);
			pCXCHEQUE.OPER_TYPE = (this.PaidBonus == new decimal(0) ? "CHARGE" : "DEBIT");
			pCXCHEQUE.STATUS = pcxOperationStatus.Online.ToString();
			pCXCHEQUEBL.Save(pCXCHEQUE);
			if (!this.ModifyCheque())
			{
				MessageBox.Show("Недостаточное количество товаров на складе для продажи в соответствии с условием акции LSPoint. Начисление бонусов будет отменено.");
				this.PartialReturnPromo(this._cheque, null, pCXCHEQUE);
			}
			return true;
		}

		public class StockDetailInfo
		{
			public STOCK_DETAIL StockDetail;

			public int Quantity;

			public StockDetailInfo()
			{
			}
		}
	}
}