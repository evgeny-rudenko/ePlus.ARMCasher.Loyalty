using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher.BusinessLogic;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty.DiscountMobile.Forms;
using ePlus.ARMCasher.Loyalty.Xml;
using ePlus.ARMCommon.Config;
using ePlus.ARMUtils;
using ePlus.CommonEx;
using ePlus.Discount2.BusinessObjects;
using ePlus.Discount2.Server;
using ePlus.DiscountMobile.Client;
using ePlus.DiscountMobile.Client.Database;
using ePlus.Loyalty;
using ePlus.MetaData.Client;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ePlus.ARMCasher.Loyalty
{
	internal class DiscountMobileLoyaltyProgram : BaseLoyaltyProgramEx
	{
		private decimal _dmScorePerSum;

		private decimal _dmMinPayPercent;

		private DiscountMobileCard _discountCard;

		private bool _isInit;

		private readonly SettingsDatabase _settingsDatabase = new SettingsDatabase();

		private readonly PosSettingsDatabase _posSettingsDatabase = new PosSettingsDatabase();

		private SettingsItem _settingsItem;

		private PosSettingsItem _posSettingsItem;

		private string _clientId;

		private DiscountMobilePosTokenStatus _discountMobilePosTokenStatus;

		private DiscountMobileLoyalty _discountMobileLoyalty;

		private DiscountMobilePurchaseResponse _discountMobilePurchaseResponse;

		protected ePlus.ARMCasher.Loyalty.Xml.DiscountMobileUserResponse DiscountMobileUserResponse;

		private static Guid _id;

		private Dictionary<long, DiscountMobileCouponItem> _couponDict;

		private readonly static Logger logger;

		private DISCOUNT2_MEMBER _discountMember;

		private string _discountMobileCouponStatus;

		protected string CardNumber
		{
			get;
			set;
		}

		private List<long> Coupons
		{
			get;
			set;
		}

		public override Guid IdGlobal
		{
			get
			{
				return DiscountMobileLoyaltyProgram._id;
			}
		}

		protected bool IsDiscountMobilePosTokenStatusActive
		{
			get
			{
				this.DiscountMobileCheckPosTokenStatus();
				return this._discountMobilePosTokenStatus.IsActive == "True";
			}
		}

		public override string Name
		{
			get
			{
				return "Dinect";
			}
		}

		protected override bool OnIsExplicitDiscount
		{
			get
			{
				return false;
			}
		}

		static DiscountMobileLoyaltyProgram()
		{
			DiscountMobileLoyaltyProgram._id = new Guid("96F37FA8-0CD9-45A4-8DF2-5BACB8158F96");
			DiscountMobileLoyaltyProgram.logger = LogManager.GetLogger("DiscountMobileLogger");
		}

		public DiscountMobileLoyaltyProgram(string clientId, string cardNumber) : base(ePlus.Loyalty.LoyaltyType.DiscountMobile, clientId, cardNumber, "DISCOUNT_MOBILE")
		{
			long num;
			DiscountMobileLoyaltyProgram.logger.Info("Инициализация модуля DM.");
			ServicePointManager.Expect100Continue = false;
			this._clientId = clientId;
			this.Coupons = new List<long>();
			this.CardNumber = cardNumber;
			try
			{
				if (AppConfigurator.EnableDiscountMobile && !this._isInit)
				{
					this.InitInternal();
				}
			}
			catch (Exception exception)
			{
				this._isInit = false;
				UtilsArm.ShowMessageErrorOK(string.Concat("Ошибка инициализации ПЦ.", Environment.NewLine, "Работа с программами лояльности невозможна."));
			}
			if (this.CardNumber.StartsWith("+") && long.TryParse(this.CardNumber.Substring(1), out num))
			{
				this.CardNumber = this.DiscountMobileLoadPhone(num);
				if (string.IsNullOrEmpty(this.CardNumber))
				{
					throw new LoyaltyException(this, string.Concat("Пользователь по телефону ", clientId, " не найден в системе Dinnect"));
				}
			}
		}

		private void AddCommonRequestParams(ref HttpWebRequest req, bool dmToken)
		{
			req.Headers.Add("DM-Authorization", string.Concat("dmapptoken ", this._settingsItem.AppToken));
			req.UserAgent = "eFarma2.ARMCacher";
			req.KeepAlive = true;
			req.Timeout = 10000;
			req.ReadWriteTimeout = 10000;
			req.Accept = "application/xml";
			if (dmToken)
			{
				req.Headers.Add("Authorization", string.Concat("dmtoken ", this._posSettingsItem.PosToken));
			}
			if (!this._settingsItem.UseProxy)
			{
				return;
			}
			WebProxy webProxy = new WebProxy()
			{
				Address = new Uri(string.Concat(this._settingsItem.ProxyUrl, ":", this._settingsItem.ProxyPort)),
				Credentials = new NetworkCredential(this._settingsItem.ProxyUser, this._settingsItem.ProxyPass)
			};
			req.Proxy = webProxy;
			req.ProtocolVersion = HttpVersion.Version10;
		}

		private void AddCoupon(long couponId)
		{
			if (!this._discountCard.Coupons.Contains(couponId))
			{
				this._discountCard.Coupons.Add(couponId);
			}
			this._discountCard.SumDiscount = new decimal(0);
		}

		private void AddCoupons(IEnumerable<long> list)
		{
			foreach (long num in list)
			{
				if (this._discountCard.Coupons.Contains(num))
				{
					continue;
				}
				this._discountCard.Coupons.Add(num);
			}
			this._discountCard.SumDiscount = new decimal(0);
		}

		private IEnumerable<DISCOUNT_VALUE_INFO> ApplyDiscountMobile(CHEQUE cheque, DISCOUNT2_CARD_POLICY sDiscountCard)
		{
			ILpTransResult lpTransResult;
			long num;
			DiscountMobileCard sumDiscount = sDiscountCard as DiscountMobileCard;
			if (sumDiscount == null)
			{
				return null;
			}
			if (sumDiscount.LoyaltyType != ePlus.Loyalty.LoyaltyType.DiscountMobile)
			{
				return null;
			}
			if (sumDiscount.State == DiscountMobileCard.CardStates.Active)
			{
				DiscountMobilePurchaseResponse discountMobilePurchaseResponse = LoyaltyProgManager.DiscountMobileApplyCoupon(cheque, false, out lpTransResult);
				if (discountMobilePurchaseResponse == null)
				{
					sumDiscount.SumDiscount = new decimal(0);
					sumDiscount.DiscountSum = new decimal(0);
					sumDiscount.SpecialDiscountParam = null;
					sumDiscount.ChequeItems.Clear();
				}
				else
				{
					sumDiscount.SumDiscount = discountMobilePurchaseResponse.SumDiscount;
					SpecialDiscountParamDecimal specialDiscountParamDecimal = new SpecialDiscountParamDecimal()
					{
						Value = sumDiscount.SumDiscount
					};
					sumDiscount.SpecialDiscountParam = new SpecialDiscountParam()
					{
						Value = specialDiscountParamDecimal
					};
				}
			}
			List<DISCOUNT_VALUE_INFO> dISCOUNTVALUEINFOs = new List<DISCOUNT_VALUE_INFO>();
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				DISCOUNT_VALUE_INFO dISCOUNTVALUEINFO = new DISCOUNT_VALUE_INFO()
				{
					ID_DISCOUNT2_GLOBAL = ARM_DISCOUNT2_PROGRAM.DiscountMobileDiscountGUID,
					TYPE = base.DiscountType,
					BARCODE = sDiscountCard.BARCODE
				};
				decimal vALUE = sumDiscount.SumDiscount;
				if (sumDiscount.ChequeItems.Count > 0)
				{
					long.TryParse(cHEQUEITEM.INTERNAL_BARCODE, out num);
					DiscountMobileCard.DiscountItem discountItem = sumDiscount.ChequeItems.Find((DiscountMobileCard.DiscountItem item1) => item1.Id == num);
					dISCOUNTVALUEINFO.VALUE = discountItem.Price - discountItem.PriceDiscounted;
					vALUE -= dISCOUNTVALUEINFO.VALUE;
				}
				dISCOUNTVALUEINFO.ID_LOT_GLOBAL = cHEQUEITEM.ID_LOT_GLOBAL;
				dISCOUNTVALUEINFOs.Add(dISCOUNTVALUEINFO);
			}
			return dISCOUNTVALUEINFOs;
		}

		private void AuthPointsDiscountMobile(CHEQUE cheque, PCX_QUERY_LOG logRecord, OperTypeEnum operType)
		{
			object[] url = new object[] { this._settingsItem.Url, "users/", this.DiscountMobileUserResponse.Id, "/purchases/" };
			Uri uri = new Uri(string.Concat(url));
			DiscountMobileLoyaltyProgram.logger.Info(uri.AbsoluteUri);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			this.AddCommonRequestParams(ref httpWebRequest, true);
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			Dictionary<string, string> strs = new Dictionary<string, string>()
			{
				{ "doc_id", cheque.ID_CHEQUE_GLOBAL.ToString() }
			};
			decimal sumWithoutDiscount = cheque.SumWithoutDiscount;
			strs.Add("sum_total", sumWithoutDiscount.ToString(CultureInfo.InvariantCulture));
			decimal sUMDISCOUNT = cheque.SUM_DISCOUNT * this._dmScorePerSum;
			if (operType != OperTypeEnum.Charge && this._discountCard.DiscountPercent == 0)
			{
				int num = (int)sUMDISCOUNT;
				strs.Add("bonus_payment", num.ToString(CultureInfo.InvariantCulture));
			}
			strs.Add("commit", "on");
			if (this._settingsItem.RedeemAuto)
			{
				strs.Add("redeem_auto", "true");
			}
			string str = "";
			foreach (string key in strs.Keys)
			{
				string str1 = str;
				string[] strArrays = new string[] { str1, HttpUtility.UrlEncode(key), "=", HttpUtility.UrlEncode(strs[key]), "&" };
				str = string.Concat(strArrays);
			}
			str = str.Substring(0, str.Length - 1);
			byte[] bytes = Encoding.ASCII.GetBytes(str);
			Stream requestStream = httpWebRequest.GetRequestStream();
			requestStream.Write(bytes, 0, (int)bytes.Length);
			requestStream.Close();
			DiscountMobileLoyaltyProgram.logger.Info(string.Format("{0}\nPOST DATA: {1}", uri.AbsoluteUri, str));
			HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
			Stream responseStream = response.GetResponseStream();
			if (responseStream == null)
			{
				throw new ApplicationException("Не удалось получить данные от ПЦ.");
			}
			StreamReader streamReader = new StreamReader(responseStream);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(DiscountMobilePurchaseResponse));
			string end = streamReader.ReadToEnd();
			DiscountMobileLoyaltyProgram.logger.Info(end);
			using (StringReader stringReader = new StringReader(end))
			{
				this._discountMobilePurchaseResponse = (DiscountMobilePurchaseResponse)xmlSerializer.Deserialize(stringReader);
			}
			this._discountMobilePurchaseResponse = (DiscountMobilePurchaseResponse)xmlSerializer.Deserialize(streamReader);
			response.Close();
			this.DiscountMobileLoadUser();
			this.CreateAndSavePCXChequeItemList(cheque.CHEQUE_ITEMS, cheque.SUMM + this._discountCard.SumDiscount, new decimal(0), new decimal(0), this._discountMobilePurchaseResponse.Id.ToString(CultureInfo.InvariantCulture), this.DiscountMobileUserResponse.Card.ToString(CultureInfo.InvariantCulture), false);
			logRecord.DATE_REQUEST = DateTime.Now;
			logRecord.TYPE = 2;
			logRecord.CLIENT_ID = this._discountCard.BARCODE;
			(new PCX_QUERY_LOG_BL()).Save(logRecord);
		}

		public override decimal CalculateMaxSumBonus(CHEQUE cheque)
		{
			decimal sUMM = cheque.SUMM + (this._discountCard == null ? new decimal(0) : this._discountCard.SumDiscount);
			LoyaltyCardInfo loyaltyCardInfo = base.GetLoyaltyCardInfo(false);
			return Math.Min(sUMM, loyaltyCardInfo.Balance);
		}

		protected Dictionary<Guid, decimal> CalculatePCXDiscount(Dictionary<Guid, decimal> itemSumDict, decimal totalSum, decimal pcxPaymentSum)
		{
			decimal num;
			Dictionary<Guid, decimal> guids = new Dictionary<Guid, decimal>();
			IEnumerator<Guid> enumerator = itemSumDict.Keys.GetEnumerator();
			enumerator.Reset();
			decimal num1 = new decimal(0);
			for (int i = 0; i < itemSumDict.Count; i++)
			{
				enumerator.MoveNext();
				if (i != itemSumDict.Count - 1)
				{
					num = UtilsArm.Round((pcxPaymentSum * itemSumDict[enumerator.Current]) / totalSum);
					num1 += num;
				}
				else
				{
					num = pcxPaymentSum - num1;
				}
				guids.Add(enumerator.Current, num);
			}
			return guids;
		}

		public bool ChequeItemsEquals(CHEQUE_ITEM c, CHEQUE_ITEM cc)
		{
			if (c.ID_LOT_GLOBAL != cc.ID_LOT_GLOBAL)
			{
				return false;
			}
			return c.QUANTITY == cc.QUANTITY;
		}

		private void CreateAndSavePCXChequeItemList(IEnumerable<CHEQUE_ITEM> chequeItemList, decimal totalSum, decimal pcxSumMoney, decimal pcxSumScore, string transactionId, string clientId, bool isRefund = false)
		{
			List<CHEQUE_ITEM> cHEQUEITEMs = new List<CHEQUE_ITEM>();
			foreach (CHEQUE_ITEM cHEQUEITEM in chequeItemList)
			{
				cHEQUEITEMs.Add(cHEQUEITEM);
			}
			Dictionary<Guid, decimal> guids = new Dictionary<Guid, decimal>();
			for (int i = 0; i < cHEQUEITEMs.Count; i++)
			{
				CHEQUE_ITEM item = cHEQUEITEMs[i];
				decimal sUMM = item.SUMM;
				foreach (DISCOUNT2_MAKE_ITEM discount2MakeItemList in item.Discount2MakeItemList)
				{
					if (discount2MakeItemList.TYPE == null)
					{
						continue;
					}
					sUMM += discount2MakeItemList.AMOUNT;
				}
				guids.Add(item.ID_CHEQUE_ITEM_GLOBAL, sUMM);
			}
			Dictionary<Guid, decimal> guids1 = this.CalculatePCXDiscount(guids, totalSum, pcxSumMoney);
			Dictionary<Guid, decimal> guids2 = this.CalculatePCXDiscount(guids, totalSum, pcxSumScore);
			List<PCX_CHEQUE_ITEM> pCXCHEQUEITEMs = new List<PCX_CHEQUE_ITEM>();
			foreach (CHEQUE_ITEM cHEQUEITEM1 in cHEQUEITEMs)
			{
				PCX_CHEQUE_ITEM pCXCHEQUEITEM = new PCX_CHEQUE_ITEM()
				{
					TRANSACTION_ID = transactionId,
					CLIENT_ID = clientId,
					CLIENT_ID_TYPE = 8
				};
				pCXCHEQUEITEMs.Add(pCXCHEQUEITEM);
				pCXCHEQUEITEM.SUMM_SCORE = Math.Abs(guids2[cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL]);
				pCXCHEQUEITEM.SUMM = Math.Abs(guids1[cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL]);
				pCXCHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL = cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL;
				if (isRefund)
				{
					pCXCHEQUEITEM.OPER_TYPE = (pcxSumMoney >= new decimal(0) ? PCX_CHEQUE_ITEM.operTypeArr[3] : PCX_CHEQUE_ITEM.operTypeArr[2]);
				}
				else
				{
					pCXCHEQUEITEM.OPER_TYPE = (pcxSumMoney >= new decimal(0) ? PCX_CHEQUE_ITEM.operTypeArr[1] : PCX_CHEQUE_ITEM.operTypeArr[0]);
				}
			}
			if (pCXCHEQUEITEMs.Count > 0)
			{
				(new PCX_CHEQUE_ITEM_BL()).Save(pCXCHEQUEITEMs);
			}
		}

		protected PCX_QUERY_LOG CreateLogQueryLog()
		{
			PCX_QUERY_LOG pCXQUERYLOG = new PCX_QUERY_LOG()
			{
				ID_USER_GLOBAL = SecurityContextEx.USER_GUID,
				ID_QUERY_GLOBAL = Guid.NewGuid(),
				STATE = 1,
				ID_CASH_REGISTER = AppConfigManager.IdCashRegister,
				CLIENT_ID_TYPE = 8
			};
			if (this._discountCard != null)
			{
				pCXQUERYLOG.CLIENT_ID = this._discountCard.BARCODE;
			}
			return pCXQUERYLOG;
		}

		protected PCX_CHEQUE CreatePCXCheque()
		{
			PCX_CHEQUE pCXCHEQUE = new PCX_CHEQUE()
			{
				CLIENT_ID = this._discountCard.BARCODE,
				CLIENT_ID_TYPE = 8,
				SUMM = new decimal(0),
				SUMM_MONEY = new decimal(0),
				SCORE = new decimal(0),
				SUMM_SCORE = new decimal(0),
				PARTNER_ID = string.Empty,
				LOCATION = string.Empty,
				TERMINAL = string.Empty
			};
			return pCXCHEQUE;
		}

		public DiscountMobilePurchaseResponse DiscountMobileApplyCoupon(CHEQUE cheque, bool submit, out ILpTransResult result)
		{
			HttpWebResponse response;
			long num;
			DiscountMobilePurchaseResponse discountMobilePurchaseResponse;
			result = null;
			if (cheque == null || cheque.CHEQUE_ITEMS == null || cheque.CHEQUE_ITEMS.Count == 0)
			{
				return null;
			}
			this.FindUser(this.CardNumber, cheque);
			object[] url = new object[] { this._settingsItem.Url, "users/", this.DiscountMobileUserResponse.Id, "/purchases/" };
			Uri uri = new Uri(string.Concat(url));
			DiscountMobileLoyaltyProgram.logger.Info(uri.AbsoluteUri);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			this.AddCommonRequestParams(ref httpWebRequest, true);
			httpWebRequest.Method = "POST";
			Dictionary<string, string> strs = new Dictionary<string, string>()
			{
				{ "doc_id", cheque.ID_CHEQUE_GLOBAL.ToString() }
			};
			string empty = string.Empty;
			foreach (long coupon in this._discountCard.Coupons)
			{
				if (!string.IsNullOrEmpty(empty))
				{
					empty = string.Concat(empty, ",");
				}
				empty = string.Concat(empty, coupon.ToString(CultureInfo.InvariantCulture));
			}
			if (!string.IsNullOrEmpty(empty))
			{
				strs.Add("coupons", empty);
			}
			decimal num1 = new decimal(0);
			string str = this.MakeChequeItemsString(cheque, ref num1, submit, false);
			strs.Add("sum_total", num1.ToString("0.00", CultureInfo.InvariantCulture));
			if (submit)
			{
				strs.Add("commit", "true");
			}
			if (this._settingsItem.RedeemAuto)
			{
				strs.Add("redeem_auto", "true");
			}
			strs.Add("curr_iso_name", "RUB");
			strs.Add("curr_iso_code", "643");
			if (this._discountCard.DiscountSum > new decimal(0))
			{
				decimal discountSum = this._discountCard.DiscountSum;
				strs.Add("bonus_payment", discountSum.ToString("0.00", CultureInfo.InvariantCulture));
			}
			string str1 = "";
			foreach (string key in strs.Keys)
			{
				string str2 = str1;
				string[] strArrays = new string[] { str2, HttpUtility.UrlEncode(key), "=", HttpUtility.UrlEncode(strs[key]), "&" };
				str1 = string.Concat(strArrays);
			}
			str1 = str1.Substring(0, str1.Length - 1);
			if (!string.IsNullOrEmpty(str))
			{
				str1 = string.Concat(str1, "&", str);
			}
			byte[] bytes = Encoding.ASCII.GetBytes(str1);
			DiscountMobileLoyaltyProgram.logger.Info(string.Format("{0}\nPOST DATA: {1}", uri.AbsoluteUri, str1));
			Stream requestStream = httpWebRequest.GetRequestStream();
			requestStream.Write(bytes, 0, (int)bytes.Length);
			requestStream.Close();
			this._discountMobileCouponStatus = string.Empty;
			try
			{
				response = (HttpWebResponse)httpWebRequest.GetResponse();
				goto Label0;
			}
			catch (WebException webException1)
			{
				WebException webException = webException1;
				if (webException.ToString().Contains("400"))
				{
					WebResponse webResponse = webException.Response;
					Stream responseStream = webResponse.GetResponseStream();
					if (responseStream == null)
					{
						throw new ApplicationException("Не удалось получить данные от ПЦ.");
					}
					StreamReader streamReader = new StreamReader(responseStream);
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(DiscountMobileUtil.DiscountMobileErrorList));
					string end = streamReader.ReadToEnd();
					DiscountMobileLoyaltyProgram.logger.Info(end);
					DiscountMobileUtil.DiscountMobileErrorList discountMobileErrorList = null;
					using (StringReader stringReader = new StringReader(end))
					{
						discountMobileErrorList = (DiscountMobileUtil.DiscountMobileErrorList)xmlSerializer.Deserialize(stringReader);
					}
					string empty1 = string.Empty;
					if (discountMobileErrorList != null && discountMobileErrorList.Errors != null && discountMobileErrorList.Errors.Errors != null && discountMobileErrorList.Errors.Errors.Count > 0)
					{
						foreach (string error in discountMobileErrorList.Errors.Errors)
						{
							empty1 = string.Concat(empty1, error, "\n");
						}
					}
					MessageBox.Show(string.Concat("Процессинговый центр не принял запрос.\n", empty1), "Ошибка расчёта скидки");
					webResponse.Close();
					this._discountMobileCouponStatus = "(Сертификат не может быть использован для данной покупки.)";
					discountMobilePurchaseResponse = null;
				}
				else if (!webException.ToString().Contains("417"))
				{
					throw;
				}
				else
				{
					MessageBox.Show("Процессинговый центр не принял запрос.\nПрокси-сервер скрыл код ошибки.", "Ошибка расчёта скидки");
					discountMobilePurchaseResponse = null;
				}
			}
			return discountMobilePurchaseResponse;
		Label0:
			Stream stream = response.GetResponseStream();
			if (stream == null)
			{
				throw new ApplicationException("Не удалось получить данные от ПЦ.");
			}
			StreamReader streamReader1 = new StreamReader(stream);
			XmlSerializer xmlSerializer1 = new XmlSerializer(typeof(DiscountMobilePurchaseResponse));
			string end1 = streamReader1.ReadToEnd();
			DiscountMobileLoyaltyProgram.logger.Info(end1);
			using (StringReader stringReader1 = new StringReader(end1))
			{
				this._discountMobilePurchaseResponse = (DiscountMobilePurchaseResponse)xmlSerializer1.Deserialize(stringReader1);
			}
			this.ParseChequeItems();
			response.Close();
			if (!submit)
			{
				return this._discountMobilePurchaseResponse;
			}
			this.DiscountMobileLoadUser();
			decimal sumBonus = this._discountMobilePurchaseResponse.SumBonus;
			decimal sumBonus1 = this._discountMobilePurchaseResponse.SumBonus;
			this.CreateAndSavePCXChequeItemList(cheque.CHEQUE_ITEMS, cheque.SUMM + this._discountCard.DiscountSum, sumBonus, sumBonus1, this._discountMobilePurchaseResponse.Id.ToString(CultureInfo.InvariantCulture), this.DiscountMobileUserResponse.Card.ToString(CultureInfo.InvariantCulture), false);
			PCX_QUERY_LOG now = this.CreateLogQueryLog();
			now.DATE_REQUEST = DateTime.Now;
			now.ID_CHEQUE_GLOBAL = cheque.ID_CHEQUE_GLOBAL;
			now.SUMM = Math.Abs(this._discountMobilePurchaseResponse.SumBonus);
			now.TYPE = 2;
			now.STATE = 4;
			now.TRANSACTION_TERMINAL = this._discountMobilePurchaseResponse.Pos;
			now.TRANSACTION_LOCATION = this._discountMobilePurchaseResponse.Pos;
			now.CLIENT_ID = this._discountCard.BARCODE;
			(new PCX_QUERY_LOG_BL()).Save(now);
			PCX_CHEQUE_BL pCXCHEQUEBL = new PCX_CHEQUE_BL();
			PCX_CHEQUE dCHEQUEGLOBAL = this.CreatePCXCheque();
			dCHEQUEGLOBAL.ID_CHEQUE_GLOBAL = cheque.ID_CHEQUE_GLOBAL;
			dCHEQUEGLOBAL.SUMM_SCORE = Math.Abs(this._discountMobilePurchaseResponse.SumBonus);
			dCHEQUEGLOBAL.SUMM = Math.Abs(this._discountMobilePurchaseResponse.SumBonus);
			dCHEQUEGLOBAL.SUMM_MONEY = new decimal(0);
			dCHEQUEGLOBAL.OPER_TYPE = (this._discountMobilePurchaseResponse.SumBonus >= new decimal(0) ? PCX_CHEQUE_ITEM.operTypeArr[1] : PCX_CHEQUE_ITEM.operTypeArr[0]);
			pCXCHEQUEBL.Save(dCHEQUEGLOBAL);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Номер карты Dinect:");
			stringBuilder.AppendLine(this._discountCard.NUMBER);
			if (!string.IsNullOrEmpty(this._discountMobilePurchaseResponse.Coupons))
			{
				stringBuilder.AppendLine("Автоматически применены купоны:");
				string[] strArrays1 = this._discountMobilePurchaseResponse.Coupons.Split(new char[] { ',' });
				for (int i = 0; i < (int)strArrays1.Length; i++)
				{
					long.TryParse(strArrays1[i], out num);
					if (this._couponDict.ContainsKey(num))
					{
						DiscountMobileCouponItem item = this._couponDict[num];
						stringBuilder.AppendLine(string.Concat(this._settingsItem.CouponPrefix, item.Number));
					}
				}
			}
			if (this._discountCard.Coupons.Count > 0)
			{
				stringBuilder.AppendLine("Использованы купоны:");
				foreach (long coupon1 in this._discountCard.Coupons)
				{
					if (!this._couponDict.ContainsKey(coupon1))
					{
						continue;
					}
					DiscountMobileCouponItem discountMobileCouponItem = this._couponDict[coupon1];
					stringBuilder.AppendLine(string.Concat(this._settingsItem.CouponPrefix, discountMobileCouponItem.Number));
				}
			}
			result = new LpTransResult(cheque.ID_CHEQUE_GLOBAL, this._discountCard.NUMBER, (this._discountMobilePurchaseResponse.SumBonus > new decimal(0) ? this._discountMobilePurchaseResponse.SumBonus : new decimal(0)), (this._discountMobilePurchaseResponse.SumBonus < new decimal(0) ? this._discountMobilePurchaseResponse.SumBonus * new decimal(-1) : new decimal(0)), this.DiscountMobileUserResponse.Bonus, "баллов", false, true);
			return null;
		}

		protected List<DiscountMobileCouponItem> DiscountMobileCheckCoupons(string couponsUrl, int pageNumber, CHEQUE cheque)
		{
			Uri uri;
			HttpWebResponse response;
			List<DiscountMobileCouponItem> discountMobileCouponItems;
			decimal num = new decimal(0);
			string str = this.MakeChequeItemsString(cheque, ref num, false, true);
			uri = (couponsUrl.Contains("?id=") || couponsUrl.Contains("items=") && couponsUrl.Contains("status=ACTIVE") ? new Uri(couponsUrl) : new Uri(string.Concat(couponsUrl, "?items=", str, "&status=ACTIVE")));
			DiscountMobileLoyaltyProgram.logger.Info(uri.AbsoluteUri);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			this.AddCommonRequestParams(ref httpWebRequest, true);
			httpWebRequest.Method = "GET";
			Dictionary<string, string> strs = new Dictionary<string, string>();
			try
			{
				response = (HttpWebResponse)httpWebRequest.GetResponse();
				goto Label0;
			}
			catch (WebException webException1)
			{
				WebException webException = webException1;
				DiscountMobileLoyaltyProgram.logger.Error(string.Format("HTTP Exception: {0}", webException.Message));
				discountMobileCouponItems = null;
			}
			return discountMobileCouponItems;
		Label0:
			Stream responseStream = response.GetResponseStream();
			if (responseStream == null)
			{
				throw new ApplicationException("Не удалось получить данные от ПЦ.");
			}
			StreamReader streamReader = new StreamReader(responseStream);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(DiscountMobileCouponList));
			string end = streamReader.ReadToEnd();
			DiscountMobileLoyaltyProgram.logger.Info(end);
			DiscountMobileCouponList discountMobileCouponList = null;
			using (StringReader stringReader = new StringReader(end))
			{
				discountMobileCouponList = (DiscountMobileCouponList)xmlSerializer.Deserialize(stringReader);
			}
			response.Close();
			List<DiscountMobileCouponItem> discountMobileCouponItems1 = new List<DiscountMobileCouponItem>();
			foreach (DiscountMobileCouponItem item in discountMobileCouponList.CouponList.Items)
			{
				discountMobileCouponItems1.Add(item);
			}
			if (pageNumber == 1)
			{
				this._couponDict = new Dictionary<long, DiscountMobileCouponItem>();
			}
			foreach (DiscountMobileCouponItem discountMobileCouponItem in discountMobileCouponItems1)
			{
				this._couponDict.Add((long)discountMobileCouponItem.Id, discountMobileCouponItem);
			}
			if (discountMobileCouponList.Page < discountMobileCouponList.Pages)
			{
				discountMobileCouponItems1.AddRange(this.DiscountMobileCheckCoupons(discountMobileCouponList.NextPage, discountMobileCouponList.Page + 1, cheque));
			}
			return discountMobileCouponItems1;
		}

		protected void DiscountMobileCheckPosTokenStatus()
		{
			if (!this.MakeApiRequest<DiscountMobilePosTokenStatus>(string.Concat(this._settingsItem.Url, "tokens/", this._posSettingsItem.PosToken), false, out this._discountMobilePosTokenStatus))
			{
				return;
			}
			if (this._discountMobilePosTokenStatus.IsActive != "True")
			{
				throw new ApplicationException("Токен DiscountMobile для ККМ не активен.");
			}
		}

		private bool DiscountMobileDeletePurchase(string purchaseUrl)
		{
			Uri uri = new Uri(purchaseUrl);
			DiscountMobileLoyaltyProgram.logger.Info(uri.AbsoluteUri);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			this.AddCommonRequestParams(ref httpWebRequest, true);
			httpWebRequest.Method = "DELETE";
			HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
			if (response.StatusCode == HttpStatusCode.NoContent)
			{
				response.Close();
				return true;
			}
			response.Close();
			return false;
		}

		protected bool DiscountMobileDeleteToken()
		{
			bool flag;
			Uri uri = new Uri(string.Concat(this._settingsItem.Url, "tokens/", this._posSettingsItem.PosToken));
			DiscountMobileLoyaltyProgram.logger.Info(uri.AbsoluteUri);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			this.AddCommonRequestParams(ref httpWebRequest, false);
			httpWebRequest.Method = "DELETE";
			try
			{
				if (((HttpWebResponse)httpWebRequest.GetResponse()).StatusCode != HttpStatusCode.NoContent)
				{
					return false;
				}
				else
				{
					this._discountMobilePosTokenStatus.IsActive = "False";
					flag = true;
				}
			}
			catch (WebException webException)
			{
				if (!webException.ToString().Contains("404"))
				{
					throw;
				}
				else
				{
					this.DiscountMobileCheckPosTokenStatus();
					flag = false;
				}
			}
			return flag;
		}

		private DiscountMobilePurchaseList DiscountMobileFindPurchases(string chequeId)
		{
			DiscountMobilePurchaseList discountMobilePurchaseList;
			object[] url = new object[] { this._settingsItem.Url, "users/", this.DiscountMobileUserResponse.Id, "/purchases/?doc_id=", chequeId };
			if (!this.MakeApiRequest<DiscountMobilePurchaseList>(string.Concat(url), true, out discountMobilePurchaseList))
			{
				throw new ApplicationException("Не удалось получить данные от ПЦ.");
			}
			return discountMobilePurchaseList;
		}

		public string DiscountMobileGetCouponStatus()
		{
			return this._discountMobileCouponStatus;
		}

		private DiscountMobileCouponItem DiscountMobileLoadCoupon(string cardCode, string url)
		{
			DiscountMobileCouponItem discountMobileCouponItem;
			DiscountMobileCouponItem discountMobileCouponItem1;
			Uri uri = new Uri(url);
			DiscountMobileLoyaltyProgram.logger.Info(uri.AbsoluteUri);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			this.AddCommonRequestParams(ref httpWebRequest, true);
			HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
			Stream responseStream = response.GetResponseStream();
			if (responseStream == null)
			{
				throw new ApplicationException("Не удалось получить данные от ПЦ.");
			}
			StreamReader streamReader = new StreamReader(responseStream);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(DiscountMobileCouponList));
			string end = streamReader.ReadToEnd();
			DiscountMobileLoyaltyProgram.logger.Info(end);
			DiscountMobileCouponList discountMobileCouponList = null;
			using (StringReader stringReader = new StringReader(end))
			{
				discountMobileCouponList = (DiscountMobileCouponList)xmlSerializer.Deserialize(stringReader);
			}
			if (discountMobileCouponList != null && discountMobileCouponList.CouponList != null && discountMobileCouponList.CouponList.Items != null && discountMobileCouponList.CouponList.Items.Count > 0)
			{
				List<DiscountMobileCouponItem>.Enumerator enumerator = discountMobileCouponList.CouponList.Items.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						DiscountMobileCouponItem current = enumerator.Current;
						if (current.Number != cardCode)
						{
							continue;
						}
						response.Close();
						discountMobileCouponItem1 = current;
						return discountMobileCouponItem1;
					}
					response.Close();
					if (discountMobileCouponList != null && discountMobileCouponList.Page < discountMobileCouponList.Pages)
					{
						discountMobileCouponItem = this.DiscountMobileLoadCoupon(cardCode, discountMobileCouponList.NextPage);
						if (discountMobileCouponItem != null)
						{
							return discountMobileCouponItem;
						}
					}
					return null;
				}
				finally
				{
					((IDisposable)enumerator).Dispose();
				}
				return discountMobileCouponItem1;
			}
			response.Close();
			if (discountMobileCouponList != null && discountMobileCouponList.Page < discountMobileCouponList.Pages)
			{
				discountMobileCouponItem = this.DiscountMobileLoadCoupon(cardCode, discountMobileCouponList.NextPage);
				if (discountMobileCouponItem != null)
				{
					return discountMobileCouponItem;
				}
			}
			return null;
		}

		private void DiscountMobileLoadLoyalty(string loyaltyUrl)
		{
			if (!this.MakeApiRequest<DiscountMobileLoyalty>(loyaltyUrl, true, out this._discountMobileLoyalty))
			{
				return;
			}
			if (this._discountMobileLoyalty == null)
			{
				return;
			}
			if (this._discountMobileLoyalty.Bonus2Amount != null && this._discountMobileLoyalty.Bonus2Amount.Items != null && this._discountMobileLoyalty.Bonus2Amount.Items.Count > 0)
			{
				this._dmScorePerSum = this._discountMobileLoyalty.Bonus2Amount.Items[0];
			}
			this._dmMinPayPercent = 100 - this._discountMobileLoyalty.MaxPurchasePercentage;
		}

		protected string DiscountMobileLoadPhone(long phone)
		{
			Uri uri = new Uri(string.Concat(this._settingsItem.Url, "users/?phone=", phone));
			DiscountMobileLoyaltyProgram.logger.Info(uri.AbsoluteUri);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			this.AddCommonRequestParams(ref httpWebRequest, true);
			HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
			Stream responseStream = response.GetResponseStream();
			if (responseStream == null)
			{
				throw new ApplicationException("Не удалось получить данные от ПЦ.");
			}
			StreamReader streamReader = new StreamReader(responseStream);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(DiscountMobileUserList));
			string end = streamReader.ReadToEnd();
			DiscountMobileLoyaltyProgram.logger.Info(end);
			DiscountMobileUserList discountMobileUserList = null;
			using (StringReader stringReader = new StringReader(end))
			{
				discountMobileUserList = (DiscountMobileUserList)xmlSerializer.Deserialize(stringReader);
			}
			if (discountMobileUserList == null || discountMobileUserList.Items == null || discountMobileUserList.Items.Count <= 0)
			{
				response.Close();
				return null;
			}
			response.Close();
			return discountMobileUserList.Items[0].Card;
		}

		private void DiscountMobileLoadUser()
		{
			this.DiscountMobileLoadUser(0);
		}

		private void DiscountMobileLoadUser(int id)
		{
			Uri uri;
			uri = (id == 0 ? new Uri(string.Concat(this._settingsItem.Url, "users/", this.DiscountMobileUserResponse.Id)) : new Uri(string.Concat(this._settingsItem.Url, "users/", id)));
			DiscountMobileLoyaltyProgram.logger.Info(uri.AbsoluteUri);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			this.AddCommonRequestParams(ref httpWebRequest, true);
			HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
			Stream responseStream = response.GetResponseStream();
			if (responseStream == null)
			{
				throw new ApplicationException("Не удалось получить данные от ПЦ.");
			}
			StreamReader streamReader = new StreamReader(responseStream);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ePlus.ARMCasher.Loyalty.Xml.DiscountMobileUserResponse));
			string end = streamReader.ReadToEnd();
			DiscountMobileLoyaltyProgram.logger.Info(end);
			using (StringReader stringReader = new StringReader(end))
			{
				this.DiscountMobileUserResponse = (ePlus.ARMCasher.Loyalty.Xml.DiscountMobileUserResponse)xmlSerializer.Deserialize(stringReader);
			}
			response.Close();
		}

		protected override void DoCharge(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
		}

		protected override void DoDebit(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
			bool flag = true;
			DiscountMobileCard discountMobileCard = cheque.DiscountCardPolicyList.Find((DISCOUNT2_CARD_POLICY c) => c is DiscountMobileCard) as DiscountMobileCard;
			if (discountMobileCard == null || discountMobileCard.LoyaltyType != ePlus.Loyalty.LoyaltyType.DiscountMobile)
			{
				throw new Exception("Code, that was depricated");
			}
			this.DiscountMobileApplyCoupon(cheque, flag, out result);
		}

		protected override LoyaltyCardInfo DoGetLoyaltyCardInfoFromService(Form form)
		{
			DiscountMobileUserList discountMobileUserList;
			LoyaltyCardInfo bonus = new LoyaltyCardInfo()
			{
				ClientId = base.ClientId,
				CardNumber = base.ClientPublicId
			};
			string str = string.Concat(this._settingsItem.Url, "users/?auto=", this.CardNumber);
			if (!this.MakeApiRequest<DiscountMobileUserList>(str, true, out discountMobileUserList))
			{
				return bonus;
			}
			if (discountMobileUserList.Items.Count <= 0)
			{
				throw new Exception("Карта или телефон не найдена");
			}
			bonus.Balance = discountMobileUserList.Items[0].Bonus;
			this.DiscountMobileLoadLoyalty(discountMobileUserList.Items[0].LoyaltyUrl);
			return bonus;
		}

		protected override bool DoIsCompatibleTo(Guid discountId)
		{
			return true;
		}

		protected override void DoPreOrderCalculation(CHEQUE cheque)
		{
			DiscountMobileCard discountMobileCard = cheque.DiscountCardPolicyList.Find((DISCOUNT2_CARD_POLICY c) => c is DiscountMobileCard) as DiscountMobileCard;
			if (discountMobileCard != null && discountMobileCard.LoyaltyType == ePlus.Loyalty.LoyaltyType.DiscountMobile)
			{
				discountMobileCard.ExtraDiscounts.Clear();
				discountMobileCard.ExtraDiscounts.AddRange(this.ApplyDiscountMobile(cheque, discountMobileCard));
			}
		}

		protected override void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = null;
		}

		protected override void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			if (!baseCheque.CHEQUE_ITEMS.TrueForAll((CHEQUE_ITEM c) => returnCheque.CHEQUE_ITEMS.Find((CHEQUE_ITEM rc) => this.ChequeItemsEquals(c, rc)) != null))
			{
				MessageBox.Show("Для программы лояльности Динект частичный возврат невозможен", "Ошибка");
				throw new Exception("Для программы лояльности Динект частичный возврат невозможен");
			}
			Dictionary<Guid, CHEQUE_ITEM> guids = new Dictionary<Guid, CHEQUE_ITEM>();
			foreach (CHEQUE_ITEM cHEQUEITEM in returnCheque.CHEQUE_ITEMS)
			{
				CHEQUE_ITEM cHEQUEITEM1 = (CHEQUE_ITEM)cHEQUEITEM.Clone();
				guids.Add(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, cHEQUEITEM1);
			}
			result = this.RefundDebitDiscountMobile(this._clientId, guids, baseCheque, returnCheque);
		}

		protected override void DoRollback(out string slipCheque)
		{
			slipCheque = null;
			throw new NotImplementedException();
		}

		protected void FindUser(string cardCode, CHEQUE cheque)
		{
			string str;
			DiscountMobileUserList discountMobileUserList;
			str = (!this.IsCoupon(cardCode) ? string.Concat(this._settingsItem.Url, "users/?auto=", cardCode) : string.Concat(this._settingsItem.Url, "users/?coupon=", cardCode));
			if (!this.MakeApiRequest<DiscountMobileUserList>(str, true, out discountMobileUserList))
			{
				return;
			}
			if (discountMobileUserList.Items.Count == 1)
			{
				if (this._discountCard == null)
				{
					this._discountCard = cheque.DiscountCardPolicyList.Find((DISCOUNT2_CARD_POLICY c) => c is DiscountMobileCard) as DiscountMobileCard;
					if (this._discountCard == null)
					{
						this._discountCard = new DiscountMobileCard();
					}
					this.DiscountMobileCheckCoupons(discountMobileUserList.Items[0].CouponsUrl, 1, cheque);
				}
				DISCOUNT2_CARD_BL dISCOUNT2CARDBL = new DISCOUNT2_CARD_BL(MultiServerBL.ServerConnectionString);
				List<DISCOUNT2_CARD> dISCOUNT2CARDs = dISCOUNT2CARDBL.List(string.Format("BARCODE = '{0}'", cardCode));
				if (dISCOUNT2CARDs.Count <= 0)
				{
					this._discountCard.ID_DISCOUNT2_CARD_GLOBAL = Guid.NewGuid();
				}
				else
				{
					this._discountCard.ID_DISCOUNT2_CARD_GLOBAL = dISCOUNT2CARDs[0].ID_DISCOUNT2_CARD_GLOBAL;
				}
				this._discountCard.NUMBER = discountMobileUserList.Items[0].Card;
				this._discountCard.BARCODE = discountMobileUserList.Items[0].Card;
				if (!string.IsNullOrEmpty(discountMobileUserList.Items[0].LoyaltyUrl))
				{
					this.DiscountMobileLoadLoyalty(discountMobileUserList.Items[0].LoyaltyUrl);
				}
				this._discountCard.DiscountPercent = discountMobileUserList.Items[0].Discounts;
				this._discountCard.SumScore = discountMobileUserList.Items[0].Bonus;
				this._discountCard.Recived = true;
				this._discountCard.State = DiscountMobileCard.CardStates.Active;
				this._discountCard.MEMBER_FULLNAME = string.Format("{0} {1} {2}", discountMobileUserList.Items[0].FirstName, discountMobileUserList.Items[0].MiddleName, discountMobileUserList.Items[0].LastName);
				if (!string.IsNullOrEmpty(discountMobileUserList.Items[0].FirstName))
				{
					this._discountMember = new DISCOUNT2_MEMBER()
					{
						LASTNAME = discountMobileUserList.Items[0].FirstName
					};
					if (!string.IsNullOrEmpty(discountMobileUserList.Items[0].LastName))
					{
						this._discountMember.MIDDLENAME = string.Concat(discountMobileUserList.Items[0].LastName.Substring(0, 1).ToUpper(), ".");
					}
					if (!string.IsNullOrEmpty(discountMobileUserList.Items[0].MiddleName))
					{
						this._discountMember.FIRSTNAME = string.Concat(discountMobileUserList.Items[0].MiddleName.Substring(0, 1).ToUpper(), ".");
					}
					this._discountMember.ID_DISCOUNT2_MEMBER_GLOBAL = Guid.NewGuid();
					this._discountCard.ID_DISCOUNT2_MEMBER_GLOBAL = this._discountMember.ID_DISCOUNT2_MEMBER_GLOBAL;
					this.DiscountMobileLoadUser(discountMobileUserList.Items[0].Id);
				}
				if (!string.IsNullOrEmpty(discountMobileUserList.Items[0].LoyaltyUrl))
				{
					this.DiscountMobileLoadLoyalty(discountMobileUserList.Items[0].LoyaltyUrl);
				}
			}
		}

		public DiscountMobileLoyalty.LoyalityProgramType GetProgramType()
		{
			if (this._discountMobileLoyalty == null)
			{
				return DiscountMobileLoyalty.LoyalityProgramType.Nothing;
			}
			return this._discountMobileLoyalty.Type;
		}

		protected ePlus.Loyalty.LoyaltyType GetTypeDiscountCard(int clientIdType)
		{
			ePlus.Loyalty.LoyaltyType loyaltyType = ePlus.Loyalty.LoyaltyType.Unknown;
			if (Enum.IsDefined(typeof(ePlus.Loyalty.LoyaltyType), clientIdType))
			{
				loyaltyType = (ePlus.Loyalty.LoyaltyType)Enum.Parse(typeof(ePlus.Loyalty.LoyaltyType), clientIdType.ToString(CultureInfo.InvariantCulture));
			}
			return loyaltyType;
		}

		private new void InitInternal()
		{
			try
			{
				this._settingsItem = this._settingsDatabase.Load();
				this._posSettingsItem = this._posSettingsDatabase.Load(AppConfigManager.IdCashRegister);
				if (this._settingsItem == null || this._posSettingsItem == null)
				{
					throw new Exception("Ошибка при загрузке настроек ПЦ из БД");
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				DiscountMobileLoyaltyProgram.logger.Error(string.Format("Ошибка при загрузке настроек ПЦ из БД", exception.Message));
				return;
			}
			AppConfigurator.EnableDiscountMobile = true;
			AppConfigurator.DiscountMobilePrefix = this._settingsItem.CardPrefix;
			AppConfigurator.DiscountMobileCouponPrefix = this._settingsItem.CouponPrefix;
			if (AppConfigurator.EnableDiscountMobile)
			{
				this._isInit = true;
				return;
			}
		}

		public bool IsBonusProgram()
		{
			return this.GetProgramType() == DiscountMobileLoyalty.LoyalityProgramType.Bonus;
		}

		protected bool IsCard(string cardCode)
		{
			if (this._settingsItem == null || this._settingsItem.CardPrefix == null)
			{
				return false;
			}
			return cardCode.StartsWith(this._settingsItem.CardPrefix);
		}

		private bool IsCoupon(string cardCode)
		{
			if (string.IsNullOrWhiteSpace(AppConfigurator.DiscountMobileCouponPrefix))
			{
				return false;
			}
			return cardCode.StartsWith(AppConfigurator.DiscountMobileCouponPrefix);
		}

		public static void LoadGlobalConfigFromDinectSettings()
		{
			DiscountMobileLoyaltyProgram discountMobileLoyaltyProgram = new DiscountMobileLoyaltyProgram("", "");
		}

		private bool MakeApiRequest<T>(string url, bool sendToken, out T result)
		{
			bool flag;
			result = default(T);
			Uri uri = new Uri(url);
			DiscountMobileLoyaltyProgram.logger.Info(uri.AbsoluteUri);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			this.AddCommonRequestParams(ref httpWebRequest, sendToken);
			try
			{
				HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
				Stream responseStream = response.GetResponseStream();
				if (responseStream == null)
				{
					throw new ApplicationException("Не удалось получить данные от ПЦ.");
				}
				StreamReader streamReader = new StreamReader(responseStream);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
				string end = streamReader.ReadToEnd();
				DiscountMobileLoyaltyProgram.logger.Info(end);
				using (StringReader stringReader = new StringReader(end))
				{
					result = (T)xmlSerializer.Deserialize(stringReader);
				}
				response.Close();
				return true;
			}
			catch (WebException webException)
			{
				if (!webException.ToString().Contains("401"))
				{
					throw;
				}
				else
				{
					MessageBox.Show("Ошибка авторизации, проверьте токен для работы с DiscountMobile.");
					flag = false;
				}
			}
			return flag;
		}

		private string MakeChequeItemsString(CHEQUE cheque, ref decimal sumTotal, bool submit, bool useGTIN)
		{
			string empty = string.Empty;
			int num = 0;
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				if (!string.IsNullOrEmpty(empty))
				{
					empty = string.Concat(empty, "&");
				}
				object[] nTERNALBARCODE = new object[] { num, cHEQUEITEM.INTERNAL_BARCODE, null, null };
				decimal qUANTITY = cHEQUEITEM.QUANTITY;
				nTERNALBARCODE[2] = qUANTITY.ToString("0.000", CultureInfo.InvariantCulture);
				decimal pRICE = cHEQUEITEM.PRICE * cHEQUEITEM.QUANTITY;
				nTERNALBARCODE[3] = pRICE.ToString("0.00", CultureInfo.InvariantCulture);
				empty = string.Concat(empty, string.Format("item_{0}_gtin={1}&item_{0}_q={2}&item_{0}_sum={3}", nTERNALBARCODE));
				sumTotal = sumTotal + (cHEQUEITEM.PRICE * cHEQUEITEM.QUANTITY);
				num++;
			}
			return empty;
		}

		protected override void OnInitInternal()
		{
		}

		protected override void OnInitSettings()
		{
		}

		private void ParseChequeItems()
		{
			long num;
			decimal num1;
			decimal num2;
			decimal num3;
			this._discountCard.ChequeItems.Clear();
			foreach (DiscountMobilePurchaseItem item in this._discountMobilePurchaseResponse.Items.Items)
			{
				DiscountMobileCard.DiscountItem discountItem = new DiscountMobileCard.DiscountItem();
				long.TryParse(item.ItemGtin, out num);
				discountItem.Id = num;
				decimal.TryParse(item.SumTotal, NumberStyles.Any, CultureInfo.InvariantCulture, out num1);
				discountItem.Price = num1;
				decimal.TryParse(item.SumWithDiscount, NumberStyles.Any, CultureInfo.InvariantCulture, out num2);
				discountItem.PriceDiscounted = num2;
				decimal.TryParse(item.Quantity, out num3);
				discountItem.Quantity = num3;
				this._discountCard.ChequeItems.Add(discountItem);
			}
		}

		private ILpTransResult RefundDebitDiscountMobile(string clientId, Dictionary<Guid, CHEQUE_ITEM> returnedChequeItemList, CHEQUE baseCheque, CHEQUE returnCheque)
		{
			this.FindUser(clientId, baseCheque);
			DiscountMobilePurchaseList discountMobilePurchaseList = this.DiscountMobileFindPurchases(baseCheque.ID_CHEQUE_GLOBAL.ToString());
			DiscountMobilePurchase item = null;
			if (discountMobilePurchaseList != null && discountMobilePurchaseList.Results != null && discountMobilePurchaseList.Results.Items != null && discountMobilePurchaseList.Results.Items.Count > 0)
			{
				item = discountMobilePurchaseList.Results.Items[0];
			}
			if (item == null)
			{
				return null;
			}
			if (!this.DiscountMobileDeletePurchase(item.Url))
			{
				return null;
			}
			PCX_QUERY_LOG now = this.CreateLogQueryLog();
			now.DATE_REQUEST = DateTime.Now;
			now.TYPE = 5;
			now.CLIENT_ID = clientId;
			now.ID_CHEQUE_GLOBAL = returnCheque.ID_CHEQUE_GLOBAL;
			(new PCX_QUERY_LOG_BL()).Save(now);
			this.CreateAndSavePCXChequeItemList(returnCheque.CHEQUE_ITEMS, baseCheque.SUMM + item.SumDiscount, item.SumBonus, item.SumBonus, item.Id.ToString(CultureInfo.InvariantCulture), clientId, true);
			PCX_CHEQUE_BL pCXCHEQUEBL = new PCX_CHEQUE_BL();
			PCX_CHEQUE dCHEQUEGLOBAL = this.CreatePCXCheque();
			dCHEQUEGLOBAL.ID_CHEQUE_GLOBAL = returnCheque.ID_CHEQUE_GLOBAL;
			dCHEQUEGLOBAL.SUMM_SCORE = Math.Abs(item.SumBonus);
			dCHEQUEGLOBAL.SUMM = Math.Abs(item.SumBonus);
			dCHEQUEGLOBAL.SUMM_MONEY = Math.Abs(item.SumBonus);
			dCHEQUEGLOBAL.OPER_TYPE = (item.SumBonus >= new decimal(0) ? PCX_CHEQUE_ITEM.operTypeArr[3] : PCX_CHEQUE_ITEM.operTypeArr[2]);
			pCXCHEQUEBL.Save(dCHEQUEGLOBAL);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Отмена списания баллов");
			stringBuilder.AppendLine("Номер карты Dinect:");
			stringBuilder.AppendLine(clientId);
			stringBuilder.AppendLine("Дата/время:");
			DateTime dATEREQUEST = now.DATE_REQUEST;
			stringBuilder.AppendLine(dATEREQUEST.ToString("dd.MM.yy HH:mm:ss"));
			stringBuilder.AppendLine(string.Concat("Транзакция:", item.Id.ToString(CultureInfo.InvariantCulture)));
			stringBuilder.AppendLine(string.Concat("Торг. точка:", this._settingsItem.ShopDin));
			stringBuilder.AppendLine(string.Concat("Касса:", item.Pos));
			decimal sumBonus = item.SumBonus * new decimal(-1);
			stringBuilder.AppendLine(string.Concat("Начислено:", sumBonus.ToString(CultureInfo.InvariantCulture), " баллов"));
			this.DiscountMobileLoadUser(this.DiscountMobileUserResponse.Id);
			stringBuilder.AppendLine(string.Concat("Баланс:", this.DiscountMobileUserResponse.Bonus.ToString(CultureInfo.InvariantCulture), " баллов"));
			stringBuilder.AppendLine();
			ILpTransResult lpTransResult = new LpTransResult(returnCheque.ID_CHEQUE_GLOBAL, this._discountCard.NUMBER, (item.SumBonus < new decimal(0) ? Math.Abs(item.SumBonus) : new decimal(0)), (item.SumBonus > new decimal(0) ? item.SumBonus : new decimal(0)), this.DiscountMobileUserResponse.Bonus, "баллов", true, true);
			return lpTransResult;
		}

		protected bool SetPaymentSumDiscountMobile(decimal maxSum, decimal chequeSum, bool blockForm)
		{
			bool flag;
			decimal num = (chequeSum * (new decimal(100) - this._dmMinPayPercent)) / new decimal(100);
			if (maxSum > num)
			{
				maxSum = num;
			}
			maxSum *= this._dmScorePerSum;
			chequeSum *= this._dmScorePerSum;
			if (this._dmScorePerSum <= new decimal(0))
			{
				MessageBox.Show("Неправильно задан курс баллов в конфигурации DiscountMobile.\nСписание баллов невозможно.", "Ошибка");
				return false;
			}
			using (FrmAddPaymentDiscountMobile frmAddPaymentDiscountMobile = new FrmAddPaymentDiscountMobile())
			{
				frmAddPaymentDiscountMobile.BlockForm = blockForm;
				frmAddPaymentDiscountMobile.MaxSum = maxSum;
				frmAddPaymentDiscountMobile.ChequeSum = chequeSum;
				frmAddPaymentDiscountMobile.ScorePerSum = this._dmScorePerSum;
				frmAddPaymentDiscountMobile.Sum = new decimal(0);
				if (frmAddPaymentDiscountMobile.ShowDialog() != DialogResult.OK)
				{
					flag = false;
				}
				else
				{
					DiscountMobileCard sumDiscount = this._discountCard;
					sumDiscount.SumDiscount = sumDiscount.SumDiscount + (frmAddPaymentDiscountMobile.Sum / this._dmScorePerSum);
					this._discountCard.BonusDiscount = frmAddPaymentDiscountMobile.Sum;
					SpecialDiscountParamDecimal specialDiscountParamDecimal = new SpecialDiscountParamDecimal()
					{
						Value = this._discountCard.SumDiscount
					};
					if (this._discountCard.SpecialDiscountParam == null)
					{
						this._discountCard.SpecialDiscountParam = new SpecialDiscountParam();
					}
					this._discountCard.SpecialDiscountParam.Value = specialDiscountParamDecimal;
					flag = true;
				}
			}
			return flag;
		}
	}
}