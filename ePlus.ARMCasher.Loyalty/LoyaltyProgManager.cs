using ePlus.ARMBusinessLogic;
using ePlus.ARMBusinessLogic.Caches.StocksCaches;
using ePlus.ARMCacheManager;
using ePlus.ARMCasher;
using ePlus.ARMCasher.BusinessLogic;
using ePlus.ARMCasher.BusinessLogic.Events;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty.AstraZeneca;
using ePlus.ARMCasher.Loyalty.Domestic;
using ePlus.ARMCasher.Loyalty.Forms;
using ePlus.ARMCasher.Loyalty.GoldenMiddle;
using ePlus.ARMCasher.Loyalty.Mindbox;
using ePlus.ARMCasher.Loyalty.Olextra;
using ePlus.ARMCasher.Loyalty.PCX;
using ePlus.ARMCasher.Loyalty.RapidSoft;
using ePlus.ARMCasher.Loyalty.SailPlay;
using ePlus.ARMCasher.Loyalty.Xml;
using ePlus.ARMCommon.Log;
using ePlus.ARMUtils;
using ePlus.CommonEx;
using ePlus.Loyalty;
using ePlus.Loyalty.AstraZeneca;
using ePlus.Loyalty.Domestic;
using ePlus.Loyalty.Interfaces;
using ePlus.Loyalty.Mindbox;
using ePlus.Loyalty.Olextra;
using ePlus.Loyalty.PharmacyWallet;
using ePlus.Loyalty.SailPlay;
using ePlus.Loyalty.Sber;
using ePlus.MetaData.Client;
using ePlus.MetaData.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty
{
	public static class LoyaltyProgManager
	{
		private static Dictionary<LoyaltyType, ePlus.Loyalty.LoyaltySettings[]> _loyaltySettings;

		private static bool? _isAutoChargeSberBonusOnSale;

		private static Dictionary<LoyaltyType, bool> _isLpCompatibility;

		private static Dictionary<LoyaltyType, Dictionary<Guid, DataRowItem>> _excludedPrograms;

		private static LoyaltyDomesticWebApi _domesticWebApi;

		private static string _sailPlayCardPrefix;

		private static SailPlayWebApi _spWebApi;

		private static ePlus.Loyalty.SailPlay.Settings _spSettings;

		private static List<MarketingAction> _marketingActions;

		private static DateTime dtEndMarketingActionDefault;

		private static string pharmacyWalletCardPrefix;

		private static Dictionary<string, Guid> m_mindboxCardPrefix;

		private static List<string> astraZenecaCardPrefix;

		private static string olextraCardPrefix;

		public static List<string> AstraZenecaCardPrefix
		{
			get
			{
				if (LoyaltyProgManager.astraZenecaCardPrefix == null)
				{
					Dictionary<LoyaltyType, ePlus.Loyalty.LoyaltySettings[]> loyaltySettings = LoyaltyProgManager.LoyaltySettings;
					if (loyaltySettings != null && loyaltySettings.ContainsKey(LoyaltyType.AstraZeneca))
					{
						try
						{
							ePlus.Loyalty.AstraZeneca.Settings setting = (new SettingsModel()).Deserialize<ePlus.Loyalty.AstraZeneca.Settings>(loyaltySettings[LoyaltyType.AstraZeneca].First<ePlus.Loyalty.LoyaltySettings>().SETTINGS, "Settings");
							if (setting == null || string.IsNullOrWhiteSpace(setting.CardPrefix))
							{
								LoyaltyProgManager.astraZenecaCardPrefix = new List<string>();
							}
							else
							{
								string str = setting.CardPrefix.Trim();
								char[] chrArray = new char[] { ',' };
								LoyaltyProgManager.astraZenecaCardPrefix = (
									from x in str.Split(chrArray)
									select x.Trim()).ToList<string>();
								ARMLogger.Info(string.Format("Используем префиксы карты из настроек АстраЗенека [CardPrefix={0}]", string.Join(", ", LoyaltyProgManager.astraZenecaCardPrefix)));
							}
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							ARMLogger.Error("Ошибка чтения настроек АстраЗенека.");
							ARMLogger.Error(exception.ToString());
						}
					}
				}
				return LoyaltyProgManager.astraZenecaCardPrefix;
			}
		}

		public static bool IsAutoChargeSberBonusOnSale
		{
			get
			{
				if (!LoyaltyProgManager._isAutoChargeSberBonusOnSale.HasValue)
				{
					try
					{
						SettingsModel settingsModel = new SettingsModel();
						ePlus.Loyalty.Sber.Params param = settingsModel.Deserialize<ePlus.Loyalty.Sber.Params>(LoyaltyProgManager.LoyaltySettings[LoyaltyType.Sberbank].First<ePlus.Loyalty.LoyaltySettings>().PARAMS, "Params");
						LoyaltyProgManager._isAutoChargeSberBonusOnSale = new bool?(param.AutoCharge);
					}
					catch (Exception exception)
					{
						LoyaltyProgManager._isAutoChargeSberBonusOnSale = new bool?(false);
					}
				}
				return LoyaltyProgManager._isAutoChargeSberBonusOnSale.Value;
			}
		}

		public static Dictionary<LoyaltyType, ePlus.Loyalty.LoyaltySettings[]> LoyaltySettings
		{
			get
			{
				if (LoyaltyProgManager._loyaltySettings == null)
				{
					List<ePlus.Loyalty.LoyaltySettings> loyaltySettings = (new SettingsModel()).List(ServerType.Local);
					Dictionary<LoyaltyType, ePlus.Loyalty.LoyaltySettings[]> loyaltyTypes = new Dictionary<LoyaltyType, ePlus.Loyalty.LoyaltySettings[]>();
					foreach (IGrouping<LoyaltyType, ePlus.Loyalty.LoyaltySettings> loyaltyTypes1 in 
						from x in loyaltySettings
						group x by x.Type)
					{
						loyaltyTypes.Add(loyaltyTypes1.Key, loyaltyTypes1.ToArray<ePlus.Loyalty.LoyaltySettings>());
					}
					LoyaltyProgManager._loyaltySettings = loyaltyTypes;
				}
				return LoyaltyProgManager._loyaltySettings;
			}
		}

		public static List<MarketingAction> MarketingActions
		{
			get
			{
				return LoyaltyProgManager._marketingActions;
			}
			set
			{
				LoyaltyProgManager._marketingActions = value;
			}
		}

		public static Dictionary<string, Guid> MindboxCardPrefix
		{
			get
			{
				if (LoyaltyProgManager.m_mindboxCardPrefix == null)
				{
					Dictionary<LoyaltyType, ePlus.Loyalty.LoyaltySettings[]> loyaltySettings = LoyaltyProgManager.LoyaltySettings;
					LoyaltyProgManager.m_mindboxCardPrefix = new Dictionary<string, Guid>();
					if (loyaltySettings != null && loyaltySettings.ContainsKey(LoyaltyType.Mindbox))
					{
						foreach (string str in 
							from x in (IEnumerable<ePlus.Loyalty.LoyaltySettings>)loyaltySettings[LoyaltyType.Mindbox]
							select x.PARAMS)
						{
							try
							{
								ePlus.Loyalty.Mindbox.Params param = (new SettingsModel()).Deserialize<ePlus.Loyalty.Mindbox.Params>(str, "Params");
								if (param != null && !string.IsNullOrWhiteSpace(param.CardPrefix))
								{
									string cardPrefix = param.CardPrefix;
									char[] chrArray = new char[] { ',' };
									foreach (string str1 in 
										from x in cardPrefix.Split(chrArray)
										select x.Trim())
									{
										LoyaltyProgManager.m_mindboxCardPrefix.Add(str1, param.IdContractorGroupGlobal);
										ARMLogger.Info(string.Format("Используем префикс карты из настроек Mindbox [CardPrefix={0}]", str1));
									}
								}
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								ARMLogger.Error("Ошибка чтения настроек Mindbox.");
								ARMLogger.Error(exception.ToString());
							}
						}
					}
				}
				return LoyaltyProgManager.m_mindboxCardPrefix;
			}
		}

		public static string OlextraCardPrefix
		{
			get
			{
				if (LoyaltyProgManager.olextraCardPrefix == null)
				{
					Dictionary<LoyaltyType, ePlus.Loyalty.LoyaltySettings[]> loyaltySettings = LoyaltyProgManager.LoyaltySettings;
					if (loyaltySettings != null && loyaltySettings.ContainsKey(LoyaltyType.Olextra))
					{
						try
						{
							ePlus.Loyalty.Olextra.Settings setting = (new SettingsModel()).Deserialize<ePlus.Loyalty.Olextra.Settings>(loyaltySettings[LoyaltyType.Olextra].First<ePlus.Loyalty.LoyaltySettings>().SETTINGS, "Settings");
							if (setting == null || string.IsNullOrWhiteSpace(setting.CardPrefix))
							{
								LoyaltyProgManager.olextraCardPrefix = string.Empty;
							}
							else
							{
								LoyaltyProgManager.olextraCardPrefix = setting.CardPrefix.Trim();
								ARMLogger.Info(string.Format("Используем префикс карты из настроек Олекстра [CardPrefix={0}]", LoyaltyProgManager.olextraCardPrefix));
							}
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							ARMLogger.Error("Ошибка чтения настроек Олекстры.");
							ARMLogger.Error(exception.ToString());
						}
					}
				}
				return LoyaltyProgManager.olextraCardPrefix;
			}
		}

		public static string PharmacyWalletCardPrefix
		{
			get
			{
				if (string.IsNullOrWhiteSpace(LoyaltyProgManager.pharmacyWalletCardPrefix))
				{
					LoyaltyProgManager.pharmacyWalletCardPrefix = "31";
					Dictionary<LoyaltyType, ePlus.Loyalty.LoyaltySettings[]> loyaltySettings = LoyaltyProgManager.LoyaltySettings;
					if (loyaltySettings != null && loyaltySettings.ContainsKey(LoyaltyType.PharmacyWallet))
					{
						try
						{
							ePlus.Loyalty.PharmacyWallet.Settings setting = (new SettingsModel()).Deserialize<ePlus.Loyalty.PharmacyWallet.Settings>(loyaltySettings[LoyaltyType.PharmacyWallet].First<ePlus.Loyalty.LoyaltySettings>().SETTINGS, "Settings");
							if (setting != null && !string.IsNullOrWhiteSpace(setting.CardPrefix))
							{
								LoyaltyProgManager.pharmacyWalletCardPrefix = setting.CardPrefix.Trim();
								ARMLogger.Info(string.Format("Используем префикс карты из настроек Аптечного Кошелька [CardPrefix={0}]", LoyaltyProgManager.pharmacyWalletCardPrefix));
							}
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							ARMLogger.Error("Ошибка чтения параметров Аптечного Кошелька.");
							ARMLogger.Error(exception.ToString());
						}
					}
				}
				return LoyaltyProgManager.pharmacyWalletCardPrefix;
			}
		}

		public static string SailPlayCardPrefix
		{
			get
			{
				if (string.IsNullOrEmpty(LoyaltyProgManager._sailPlayCardPrefix))
				{
					SettingsModel settingsModel = new SettingsModel();
					ePlus.Loyalty.SailPlay.Settings setting = settingsModel.Deserialize<ePlus.Loyalty.SailPlay.Settings>(LoyaltyProgManager.LoyaltySettings[LoyaltyType.SailPlay].First<ePlus.Loyalty.LoyaltySettings>().SETTINGS, "Settings");
					LoyaltyProgManager._sailPlayCardPrefix = (string.IsNullOrEmpty(setting.CardPrefix) ? "#null" : setting.CardPrefix);
				}
				if (LoyaltyProgManager._sailPlayCardPrefix != "#null")
				{
					return LoyaltyProgManager._sailPlayCardPrefix;
				}
				return null;
			}
		}

		public static IEnumerable<LoyaltyType> SupportPhoneAsIdentity
		{
			get;
			private set;
		}

		static LoyaltyProgManager()
		{
			LoyaltyProgManager._isLpCompatibility = new Dictionary<LoyaltyType, bool>();
			LoyaltyProgManager._excludedPrograms = new Dictionary<LoyaltyType, Dictionary<Guid, DataRowItem>>();
			LoyaltyProgManager.dtEndMarketingActionDefault = new DateTime(2099, 12, 31);
			LoyaltyProgManager.pharmacyWalletCardPrefix = null;
			LoyaltyProgManager.m_mindboxCardPrefix = null;
			LoyaltyProgManager.astraZenecaCardPrefix = null;
			LoyaltyProgManager.olextraCardPrefix = null;
			List<LoyaltyType> loyaltyTypes = new List<LoyaltyType>()
			{
				LoyaltyType.Domestic,
				LoyaltyType.SailPlay,
				LoyaltyType.Mindbox,
				LoyaltyType.AstraZeneca,
				LoyaltyType.Olextra,
				LoyaltyType.DiscountMobile
			};
			LoyaltyProgManager.SupportPhoneAsIdentity = loyaltyTypes;
		}

		public static string CardNumberToHash(string number, LoyaltyType loyaltyType = 0)
		{
			SHA1Managed sHA1Managed = new SHA1Managed();
			byte[] bytes = Encoding.ASCII.GetBytes(number);
			return LoyaltyProgManager.HexStringFromBytes(sHA1Managed.ComputeHash(bytes)).ToUpper();
		}

		public static LoyaltyCard CreateLoyaltyCard(ILoyaltyProgram lp)
		{
			LoyaltyCardInfo loyaltyCardInfo = lp.GetLoyaltyCardInfo(false);
			LoyaltyCard loyaltyCard = null;
			switch (lp.LoyaltyType)
			{
				case LoyaltyType.RapidSoft:
				{
					RapidSoftCard rapidSoftCard = new RapidSoftCard()
					{
						ID_DISCOUNT2_CARD_GLOBAL = new Guid("D04FD3C9-82D9-46B1-BDA8-9A728AB5E7C1"),
						NUMBER = loyaltyCardInfo.CardNumber,
						BARCODE = loyaltyCardInfo.ClientId,
						RapidSoftName = lp.Name
					};
					loyaltyCard = rapidSoftCard;
					return loyaltyCard;
				}
				case 3:
				case 5:
				case LoyaltyType.RapidSoft | LoyaltyType.Svyaznoy | LoyaltyType.Sberbank:
				case 9:
				case LoyaltyType.LsPoint:
				case LoyaltyType.RapidSoft | LoyaltyType.LsPoint | LoyaltyType.DiscountMobile:
				case LoyaltyType.eFarmaDiscount:
				{
					return loyaltyCard;
				}
				case LoyaltyType.Svyaznoy:
				case LoyaltyType.Sberbank:
				case LoyaltyType.PharmacyWallet:
				{
					PCXDiscount2Card pCXDiscount2Card = new PCXDiscount2Card((int)lp.LoyaltyType)
					{
						BARCODE = loyaltyCardInfo.ClientId,
						NUMBER = loyaltyCardInfo.CardNumber,
						ACCUMULATE_SUM = loyaltyCardInfo.Balance,
						SumScore = loyaltyCardInfo.Points,
						Recived = true
					};
					loyaltyCard = pCXDiscount2Card;
					return loyaltyCard;
				}
				case LoyaltyType.DiscountMobile:
				{
					DiscountMobileCard discountMobileCard = new DiscountMobileCard()
					{
						NUMBER = loyaltyCardInfo.ClientId,
						BARCODE = loyaltyCardInfo.ClientId
					};
					loyaltyCard = discountMobileCard;
					return loyaltyCard;
				}
				case LoyaltyType.Domestic:
				{
					LoyaltyDomestic loyaltyDomestic = new LoyaltyDomestic()
					{
						NUMBER = loyaltyCardInfo.CardNumber,
						BARCODE = loyaltyCardInfo.ClientId
					};
					loyaltyCard = loyaltyDomestic;
					return loyaltyCard;
				}
				case LoyaltyType.GoldenMiddle:
				{
					LoyaltyGoldenMiddleCard loyaltyGoldenMiddleCard = new LoyaltyGoldenMiddleCard()
					{
						NUMBER = loyaltyCardInfo.CardNumber,
						BARCODE = loyaltyCardInfo.ClientId
					};
					loyaltyCard = loyaltyGoldenMiddleCard;
					return loyaltyCard;
				}
				case LoyaltyType.SailPlay:
				{
					SailPlayCard sailPlayCard = new SailPlayCard()
					{
						NUMBER = loyaltyCardInfo.ClientId,
						BARCODE = loyaltyCardInfo.ClientId
					};
					loyaltyCard = sailPlayCard;
					return loyaltyCard;
				}
				case LoyaltyType.Mindbox:
				{
					loyaltyCard = lp.GetLoyaltyCard();
					loyaltyCard.NUMBER = loyaltyCardInfo.ClientId;
					loyaltyCard.BARCODE = loyaltyCardInfo.ClientId;
					return loyaltyCard;
				}
				case LoyaltyType.AstraZeneca:
				{
					AstraZenecaCard astraZenecaCard = new AstraZenecaCard()
					{
						NUMBER = loyaltyCardInfo.ClientId,
						BARCODE = loyaltyCardInfo.ClientId
					};
					loyaltyCard = astraZenecaCard;
					return loyaltyCard;
				}
				case LoyaltyType.Olextra:
				{
					OlextraCard olextraCard = new OlextraCard()
					{
						NUMBER = loyaltyCardInfo.ClientId,
						BARCODE = loyaltyCardInfo.ClientId
					};
					loyaltyCard = olextraCard;
					return loyaltyCard;
				}
				default:
				{
					return loyaltyCard;
				}
			}
		}

		public static Cart CreateSailPlayCart(CHEQUE cheque)
		{
			Cart carts = new Cart();
			StocksCache stocksCache = new StocksCache(new MemoryCacheManager(MemoryCache.Default));
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				STOCK_DETAIL stockDetail = stocksCache.GetStockDetail(cHEQUEITEM);
				PurchaseItem purchaseItem = new PurchaseItem()
				{
					Sku = LoyaltyProgManager.GetGoodCode(cHEQUEITEM),
					Price = cHEQUEITEM.SUMM,
					Qantity = (cHEQUEITEM.QUANTITY * stockDetail.NUMERATOR) / stockDetail.DENOMINATOR,
					PurchasePrice = cHEQUEITEM.PRICE_SUP,
					Markup = (float)((float)cHEQUEITEM.MARGIN_PCT)
				};
				PurchaseItem purchaseItem1 = purchaseItem;
				Guid dLOTGLOBAL = cHEQUEITEM.ID_LOT_GLOBAL;
				carts.AddPurchase(purchaseItem1, Math.Abs(dLOTGLOBAL.GetHashCode()));
			}
			return carts;
		}

		public static LoyaltyType DetectLoyaltyTypeByBin(string track2)
		{
			ARMLogger.Trace("Определение ПЛ по БИНу карты");
			string cardNumberFromTrack2 = LoyaltyProgManager.GetCardNumberFromTrack2(track2);
			string last4DigitFromTrack2 = LoyaltyProgManager.GetLast4DigitFromTrack2(track2);
			ARMLogger.Trace("Последние 4 цифры номера карты: {0}", new object[] { last4DigitFromTrack2 });
			string hash = LoyaltyProgManager.CardNumberToHash(cardNumberFromTrack2, LoyaltyType.Sberbank);
			List<ePlus.Loyalty.LoyaltySettings> loyaltySettings = (new SettingsModel()).List(ServerType.Local);
			if (loyaltySettings.Any<ePlus.Loyalty.LoyaltySettings>((ePlus.Loyalty.LoyaltySettings x) => x.Type == LoyaltyType.Sberbank) && (new SberbankLoyaltyProgram(hash, last4DigitFromTrack2)).ValidateBin(cardNumberFromTrack2))
			{
				return LoyaltyType.Sberbank;
			}
			if (loyaltySettings.Any<ePlus.Loyalty.LoyaltySettings>((ePlus.Loyalty.LoyaltySettings x) => x.Type == LoyaltyType.RapidSoft))
			{
				return LoyaltyType.RapidSoft;
			}
			return LoyaltyType.Unknown;
		}

		public static DiscountMobilePurchaseResponse DiscountMobileApplyCoupon(CHEQUE cheque, bool submit, out ILpTransResult result)
		{
			result = null;
			if (!cheque.LoyaltyPrograms.ContainsKey(LoyaltyType.DiscountMobile))
			{
				return null;
			}
			DiscountMobileLoyaltyProgram item = (DiscountMobileLoyaltyProgram)cheque.LoyaltyPrograms[LoyaltyType.DiscountMobile];
			return item.DiscountMobileApplyCoupon(cheque, submit, out result);
		}

		public static IDictionary<Guid, decimal> Distribute(IEnumerable<KeyValuePair<Guid, decimal>> chequeItems, decimal chequeSum, decimal discount, bool isDiscountScore = false)
		{
			decimal num;
			if (chequeItems == null)
			{
				throw new ArgumentNullException("chequeItems");
			}
			List<KeyValuePair<Guid, decimal>> list = (
				from ci in chequeItems
				orderby ci.Value
				select ci).ToList<KeyValuePair<Guid, decimal>>();
			if (list.Count == 0)
			{
				throw new ArgumentException("Количество позиций равно нулю");
			}
			if (chequeSum <= new decimal(0))
			{
				throw new ArgumentException("Сумма чека меньше или равна нулю");
			}
			if (discount > chequeSum && !isDiscountScore)
			{
				throw new InvalidLoyaltySumException("Сумма скидки больше суммы чека");
			}
			if (chequeSum != list.Sum<KeyValuePair<Guid, decimal>>((KeyValuePair<Guid, decimal> item) => item.Value))
			{
				throw new ArgumentException("Сумма чека не равна сумме позиций");
			}
			IDictionary<Guid, decimal> guids = new Dictionary<Guid, decimal>();
			decimal num1 = new decimal(0);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				KeyValuePair<Guid, decimal> keyValuePair = list[i];
				num = (count - 1 != i ? Math.Round((discount * keyValuePair.Value) / chequeSum, 2) : discount - num1);
				guids[keyValuePair.Key] = num;
				num1 += num;
			}
			return guids;
		}

		public static IDictionary<Guid, decimal> DistributeInteger(IEnumerable<KeyValuePair<Guid, decimal>> chequeItems, decimal chequeSum, decimal discount, bool isDiscountScore = false)
		{
			decimal num;
			List<KeyValuePair<Guid, decimal>> list = (
				from ci in chequeItems
				orderby ci.Value
				select ci).ToList<KeyValuePair<Guid, decimal>>();
			IDictionary<Guid, decimal> guids = new Dictionary<Guid, decimal>();
			decimal num1 = new decimal(0);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				KeyValuePair<Guid, decimal> item = list[i];
				num = (count - 1 != i ? Math.Truncate(Math.Round((discount * item.Value) / chequeSum, 2)) : discount - num1);
				guids[item.Key] = num;
				num1 += num;
			}
			return guids;
		}

		private static void EmailEditUserSailPlay(LoyaltyCardInfo cardInfo, FrmDebit frm)
		{
			ARMLogger.Trace("Вызван метод редактирования Email пользователя SailPlay");
			try
			{
				FormSailPlayUserRegister formSailPlayUserRegister = new FormSailPlayUserRegister()
				{
					Owner = frm,
					OnlyEmailEdit = true
				};
				using (UserRegisterPresenter userRegisterPresenter = new UserRegisterPresenter(formSailPlayUserRegister))
				{
					UserInfoResult userInfo = (UserInfoResult)cardInfo.UserInfo;
					userInfo.OnlyEmailEdit = true;
					UserInfoResult userInfoResult = userRegisterPresenter.ShowView(userInfo);
					if (userInfoResult != null)
					{
						if (LoyaltyProgManager._spSettings == null)
						{
							LoyaltyProgManager.LoadSailPlaySettings();
						}
						SailPlayWebApi sailPlayWebApi = new SailPlayWebApi(LoyaltyProgManager._spSettings, cardInfo.ClientIdType, cardInfo.ClientId);
						string clientEmail = cardInfo.ClientEmail;
						string eMail = userInfoResult.EMail;
						UserUpdateResult userUpdateResult = null;
						if (!string.IsNullOrWhiteSpace(clientEmail))
						{
							userUpdateResult = sailPlayWebApi.UserUpdateNewEmail(eMail, false);
							ARMLogger.Trace("Вызван метод редактирования Email пользователя SailPlay: UserUpdateNewEmail");
						}
						else
						{
							userUpdateResult = sailPlayWebApi.UserUpdateAddEmail(eMail, false);
							ARMLogger.Trace("Вызван метод редактирования Email пользователя SailPlay: UserUpdateAddEmail");
						}
						if (!userUpdateResult.IsOk)
						{
							UtilsArm.ShowMessageExclamationOK(string.Format("Неуспешное изменение Email пользователя SailPlay. {0}", userUpdateResult.Message), "Ошибка");
						}
						else
						{
							frm.Email = eMail;
						}
					}
				}
			}
			catch (Exception exception)
			{
				ARMLogger.ErrorException("Ошибка при выполнении метода редактирования Email пользователя SailPlay", exception);
			}
		}

		public static string FormatPublicId(string publicId, PublicIdType publicIdType)
		{
			string str;
			try
			{
				str = (publicIdType == PublicIdType.Phone ? string.Format("+{0:# (###) ### ## ##}", long.Parse(publicId)) : publicId);
			}
			catch
			{
				str = publicId;
			}
			return str;
		}

		private static void frmDebit_EditEmailEvent(object sender, EventArgs e)
		{
		}

		public static ILoyaltyProgram GetAnonimousLoyaltyProgram(LoyaltyType type, object parameters = null)
		{
			if (!LoyaltyProgManager.LoyaltySettings.ContainsKey(type))
			{
				return null;
			}
			if (type != LoyaltyType.Mindbox)
			{
				throw new NotImplementedException(string.Format("Method: {0}, LoyaltyType: {1}", "GetAnonimousLoyaltyProgram", type));
			}
			return MindboxLoyaltyProgram.Create(parameters);
		}

		private static ILoyaltyProgram GetAstraZenecaLoyaltyProgram(string publicId)
		{
			if (string.IsNullOrEmpty(publicId))
			{
				throw new ArgumentNullException("clientId", "Для использования программы лояльности \"АстраЗенека\" необходимо указать номер карты");
			}
			string str = DataSyncBL.Instance.CashRegisterSelf.ID_CASH_REGISTER_GLOBAL.ToString();
			if (AppConfigurator.IsRiglaLic)
			{
				return new AstraZenecaLoyaltyProgramRigla(publicId, str);
			}
			return new AstraZenecaLoyaltyProgram(publicId, str);
		}

		public static string GetCardNumberFromTrack2(string track2)
		{
			return track2.Substring(0, 16);
		}

		private static ILoyaltyProgram GetDomesticLoyaltyProgram(string clientId)
		{
			if (string.IsNullOrEmpty(clientId))
			{
				throw new ArgumentNullException("clientId", "Для использования программы лояльности \"Аптечка\" необходимо указать номер карты");
			}
			return new DomesticLoyaltyProgram(clientId);
		}

		private static ILoyaltyProgram GetGoldenMiddleLoyaltyProgram(string clientId)
		{
			if (string.IsNullOrEmpty(clientId))
			{
				throw new ArgumentNullException("clientId", "Для использования программы лояльности \"Золотая Середина\" необходимо указать номер карты");
			}
			return new GoldenMiddleLoyaltyProgram(clientId);
		}

		private static string GetGoodCode(CHEQUE_ITEM chequeItem)
		{
			if (string.IsNullOrEmpty(chequeItem.CODE))
			{
				return "-100";
			}
			return chequeItem.CODE;
		}

		public static string GetLast4DigitFromTrack2(string track2)
		{
			return track2.Substring(12, 4);
		}

		public static IEnumerable<Card> GetLoyaltyDomesticCards(string phone)
		{
			LoyaltyProgManager.InitLoyaltyDomesticWebApi();
			return LoyaltyProgManager._domesticWebApi.GetClientCardsInfo(new ApiCardListRequest()
			{
				Phone = phone
			});
		}

		public static ILoyaltyProgram GetLoyaltyProgram(LoyaltyType loyaltyId, Guid loyaltyInstance, string clientId, string cardNumber = null)
		{
			switch (loyaltyId)
			{
				case LoyaltyType.RapidSoft:
				{
					return LoyaltyProgManager.GetRapidSoftLoyaltyProgram(clientId, cardNumber);
				}
				case 3:
				case 5:
				case LoyaltyType.RapidSoft | LoyaltyType.Svyaznoy | LoyaltyType.Sberbank:
				case 9:
				case LoyaltyType.LsPoint:
				case LoyaltyType.RapidSoft | LoyaltyType.LsPoint | LoyaltyType.DiscountMobile:
				case LoyaltyType.eFarmaDiscount:
				{
					throw new Exception("Поставщик программы лояльности не найден");
				}
				case LoyaltyType.Svyaznoy:
				{
					return LoyaltyProgManager.GetSvyaznoyLoyaltyProgram(clientId);
				}
				case LoyaltyType.Sberbank:
				{
					return LoyaltyProgManager.GetSberLoyaltyProgram(clientId, cardNumber);
				}
				case LoyaltyType.DiscountMobile:
				{
					return LoyaltyProgManager.GetMobileDiscountLoyaltyProgram(clientId);
				}
				case LoyaltyType.Domestic:
				{
					return LoyaltyProgManager.GetDomesticLoyaltyProgram(clientId);
				}
				case LoyaltyType.PharmacyWallet:
				{
					return LoyaltyProgManager.GetPharmacyWalletLoyaltyProgram(clientId);
				}
				case LoyaltyType.GoldenMiddle:
				{
					return LoyaltyProgManager.GetGoldenMiddleLoyaltyProgram(clientId);
				}
				case LoyaltyType.SailPlay:
				{
					return LoyaltyProgManager.GetSailPlayLoyaltyProgram(clientId);
				}
				case LoyaltyType.Mindbox:
				{
					return LoyaltyProgManager.GetMindboxLoyaltyProgram(clientId, loyaltyInstance);
				}
				case LoyaltyType.AstraZeneca:
				{
					return LoyaltyProgManager.GetAstraZenecaLoyaltyProgram(clientId);
				}
				case LoyaltyType.Olextra:
				{
					return LoyaltyProgManager.GetOlextraLoyaltyProgram(clientId);
				}
				default:
				{
					throw new Exception("Поставщик программы лояльности не найден");
				}
			}
		}

		public static string GetLoyaltyProgramName(LoyaltyType loyaltyType, LoyaltyCard card = null)
		{
			if (card != null && card is MindboxCard)
			{
				return (card as MindboxCard).loyaltyProgram.Name;
			}
			if (!LoyaltyProgManager.LoyaltySettings.ContainsKey(loyaltyType))
			{
				return "";
			}
			return LoyaltyProgManager.LoyaltySettings[loyaltyType].First<ePlus.Loyalty.LoyaltySettings>().NAME;
		}

		public static string GetMarketingActionsText(CHEQUE cheque)
		{
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				if (LoyaltyProgManager._spWebApi == null)
				{
					if (LoyaltyProgManager._spSettings == null)
					{
						LoyaltyProgManager.LoadSailPlaySettings();
					}
					LoyaltyProgManager._spWebApi = new SailPlayWebApi(LoyaltyProgManager._spSettings, PublicIdType.Unknown, null);
				}
				Cart carts = LoyaltyProgManager.CreateSailPlayCart(cheque);
				CalcResult calcResult = LoyaltyProgManager._spWebApi.CalcMaxDiscount(carts, (int)cheque.SUMM);
				if (LoyaltyProgManager.InitMarketingActions() && calcResult != null && calcResult.MarketingActionsApplied != null && calcResult.MarketingActionsApplied.Count<MarketingActionResult>() > 0)
				{
					foreach (MarketingActionResult marketingActionResult in calcResult.MarketingActionsApplied.ToList<MarketingActionResult>().FindAll((MarketingActionResult x) => LoyaltyProgManager._marketingActions.Any<MarketingAction>((MarketingAction y) => y.@alias == x.Alias)))
					{
						if (marketingActionResult.ClientMessage == null)
						{
							continue;
						}
						stringBuilder.AppendLine(marketingActionResult.ClientMessage);
					}
				}
			}
			catch (Exception exception)
			{
				ARMLogger.Trace(string.Concat("Не удалось получить информацию о примененных акциях в SailPlay. Ошибка: ", exception.Message));
			}
			return stringBuilder.ToString();
		}

		public static IEnumerable<ILpTransResult> GetMergedLpResults(Dictionary<LoyaltyType, ILpTransResult> first, Dictionary<LoyaltyType, ILpTransResult> second)
		{
			ILpTransResult lpTransResult;
			foreach (LoyaltyType loyaltyType in first.Keys.Union<LoyaltyType>(second.Keys))
			{
				if (!first.ContainsKey(loyaltyType) || !second.ContainsKey(loyaltyType))
				{
					lpTransResult = (!first.ContainsKey(loyaltyType) ? second[loyaltyType] : first[loyaltyType]);
				}
				else
				{
					lpTransResult = first[loyaltyType].Merge(second[loyaltyType]);
				}
				yield return lpTransResult;
			}
		}

		private static ILoyaltyProgram GetMindboxLoyaltyProgram(string publicId, Guid loyaltyInstance)
		{
			string pointOfContactForMindbox = LoyaltyProgManager.GetPointOfContactForMindbox();
			return MindboxLoyaltyProgram.Create(new { publicId = publicId, pointOfContact = pointOfContactForMindbox, instance = loyaltyInstance });
		}

		private static ILoyaltyProgram GetMobileDiscountLoyaltyProgram(string clientId)
		{
			if (string.IsNullOrEmpty(clientId))
			{
				throw new ArgumentNullException("clientId", "Для использования программы лояльности \"Discount Mobile\" необходимо указать номер карты");
			}
			return new DiscountMobileLoyaltyProgram(clientId, clientId);
		}

		private static ILoyaltyProgram GetOlextraLoyaltyProgram(string publicId)
		{
			if (string.IsNullOrEmpty(publicId))
			{
				throw new ArgumentNullException("clientId", "Для использования программы лояльности \"Олекстра\" необходимо указать номер карты");
			}
			Guid dCASHREGISTERGLOBAL = DataSyncBL.Instance.CashRegisterSelf.ID_CASH_REGISTER_GLOBAL;
			return new OlextraLoyaltyProgram(publicId, dCASHREGISTERGLOBAL.ToString());
		}

		private static ILoyaltyProgram GetPharmacyWalletLoyaltyProgram(string clientId)
		{
			if (string.IsNullOrEmpty(clientId))
			{
				throw new ArgumentNullException("clientId", "Для использования программы лояльности \"Аптечный кошелёк\" необходимо указать номер карты");
			}
			return new PharmacyWalletLoyaltyProgram(clientId);
		}

		public static string GetPointOfContactForMindbox()
		{
			string str;
			if (!AppConfigurator.IsRiglaLic)
			{
				string farmDataCode = DataSyncBL.Instance.ContractorSelf.FarmDataCode;
				if (string.IsNullOrEmpty(farmDataCode))
				{
					throw new ApplicationException("Не удалось найти адрес точки контакта: вероятно, не заполнены данные ФармаДата. Обмен с MindBox невозможен");
				}
				str = string.Concat("PA_", farmDataCode);
			}
			else
			{
				str = DataSyncBL.Instance.ContractorSelf.A_COD.ToString();
			}
			return str;
		}

		public static string GetPublicIdTypeTitle(PublicIdType idType)
		{
			switch (idType)
			{
				case PublicIdType.CardNumber:
				{
					return "Номер карты";
				}
				case PublicIdType.Phone:
				{
					return "Номер телефона";
				}
				case PublicIdType.EMail:
				{
					return "E-mail";
				}
			}
			return LoyaltyProgManager.GetPublicIdTypeTitle(PublicIdType.CardNumber);
		}

		private static ILoyaltyProgram GetRapidSoftLoyaltyProgram(string clientId, string cardNumber)
		{
			if (string.IsNullOrEmpty(clientId))
			{
				throw new ArgumentNullException("clientId", "Для использования программы лояльности \"Bonus Back\" необходимо указать хэш значение карты");
			}
			return new RapidSoftLoyaltyProgram(clientId, cardNumber);
		}

		private static ILoyaltyProgram GetSailPlayLoyaltyProgram(string publicId)
		{
			if (string.IsNullOrEmpty(publicId))
			{
				throw new ArgumentNullException("clientId", "Для использования программы лояльности \"Мое здоровье\" необходимо указать номер карты");
			}
			return new SailPlayLoyaltyProgram(publicId);
		}

		private static ILoyaltyProgram GetSberLoyaltyProgram(string clientId, string cardNumber)
		{
			if (string.IsNullOrEmpty(clientId))
			{
				throw new ArgumentNullException("clientId", "Для использования программы лояльности \"Cпасибо от Сбербанка\" необходимо указать хэш значение карты");
			}
			return new SberbankLoyaltyProgram(clientId, cardNumber);
		}

		private static ILoyaltyProgram GetSvyaznoyLoyaltyProgram(string clientId)
		{
			if (string.IsNullOrEmpty(clientId))
			{
				throw new ArgumentNullException("clientId", "Для использования программы лояльности \"Связной\" необходимо указать номер карты");
			}
			return new SvyaznoyLoyaltyProgram(clientId);
		}

		private static string HexStringFromBytes(byte[] bytes)
		{
			StringBuilder stringBuilder = new StringBuilder();
			byte[] numArray = bytes;
			for (int i = 0; i < (int)numArray.Length; i++)
			{
				string str = numArray[i].ToString("x2");
				stringBuilder.Append(str);
			}
			return stringBuilder.ToString();
		}

		public static void Init()
		{
			BusinessLogicEvents.Instance.ZReportPrinting += new EventHandler<ZReportPrintingEventArgs>(LoyaltyProgManager.ZReportPrinting);
		}

		private static void InitLoyaltyDomesticWebApi()
		{
			if (LoyaltyProgManager._domesticWebApi == null)
			{
				SettingsModel settingsModel = new SettingsModel();
				ePlus.Loyalty.LoyaltySettings loyaltySetting = settingsModel.Load(LoyaltyType.Domestic, Guid.Empty, ServerType.Local);
				ePlus.Loyalty.Domestic.Settings setting = settingsModel.Deserialize<ePlus.Loyalty.Domestic.Settings>(loyaltySetting.SETTINGS, "Settings");
				LoyaltyProgManager._domesticWebApi = new LoyaltyDomesticWebApi(setting, SecurityContextEx.Context.User.Password_hash);
			}
		}

		public static bool InitMarketingActions()
		{
			if (LoyaltyProgManager._spSettings == null)
			{
				LoyaltyProgManager.LoadSailPlaySettings();
			}
			if (LoyaltyProgManager._marketingActions == null)
			{
				if (LoyaltyProgManager._spWebApi == null)
				{
					LoyaltyProgManager._spWebApi = new SailPlayWebApi(LoyaltyProgManager._spSettings, PublicIdType.Unknown, null);
				}
				ActionsResult marketingActions = LoyaltyProgManager._spWebApi.GetMarketingActions();
				if (marketingActions != null && marketingActions.MarketingActions != null)
				{
					SettingsModel settingsModel = new SettingsModel();
					ePlus.Loyalty.LoyaltySettings loyaltySetting = settingsModel.Load(LoyaltyType.SailPlay, Guid.Empty, ServerType.Local);
					if (loyaltySetting != null)
					{
						ePlus.Loyalty.SailPlay.Params param = settingsModel.Deserialize<ePlus.Loyalty.SailPlay.Params>(loyaltySetting.PARAMS, "Params");
						if (param != null)
						{
							IEnumerable<MarketingAction> marketingActions1 = marketingActions.MarketingActions.Where<MarketingAction>((MarketingAction x) => {
								int? groupId = x.group_id;
								long dActionGroup = param.IDActionGroup;
								if ((long)groupId.GetValueOrDefault() != dActionGroup)
								{
									return false;
								}
								return groupId.HasValue;
							});
							if (marketingActions1.Any<MarketingAction>())
							{
								LoyaltyProgManager._marketingActions = marketingActions1.ToList<MarketingAction>();
								LoyaltyProgManager._marketingActions.ForEach((MarketingAction a) => {
									if (!a.dt_end.HasValue)
									{
										a.dt_end = new DateTime?(LoyaltyProgManager.dtEndMarketingActionDefault);
									}
									if (!a.dt_start.HasValue)
									{
										a.dt_start = new DateTime?(DateTime.Now);
									}
								});
							}
						}
					}
				}
			}
			if (LoyaltyProgManager._marketingActions != null && LoyaltyProgManager._marketingActions.Count != 0)
			{
				return true;
			}
			return false;
		}

		public static bool IsCompatible(LoyaltyType loyaltyType, Guid discountId)
		{
			if (loyaltyType.IsUsedAsDiscount())
			{
				return true;
			}
			if (!LoyaltyProgManager._isLpCompatibility.ContainsKey(loyaltyType))
			{
				ePlus.Loyalty.LoyaltySettings loyaltySetting = (new SettingsModel()).Load(loyaltyType, Guid.Empty, ServerType.Local);
				LoyaltyProgManager._isLpCompatibility.Add(loyaltyType, loyaltySetting.COMPATIBILITY);
				if (loyaltySetting.COMPATIBILITY)
				{
					Dictionary<Guid, DataRowItem> guids = new Dictionary<Guid, DataRowItem>()
					{
						{ loyaltySetting.ID_LOYALITY_PROGRAM_GLOBAL, null }
					};
					LoyaltyProgManager._excludedPrograms.Add(loyaltyType, guids);
					foreach (DataRowItem excludeList in loyaltySetting.CompatibilitiesDCT.ExcludeList)
					{
						guids.Add(excludeList.Guid, excludeList);
					}
					foreach (DataRowItem dataRowItem in loyaltySetting.CompatibilitiesDP.ExcludeList)
					{
						guids.Add(dataRowItem.Guid, dataRowItem);
					}
					foreach (DataRowItem excludeList1 in loyaltySetting.CompatibilitiesPL.ExcludeList)
					{
						guids.Add(excludeList1.Guid, excludeList1);
					}
				}
			}
			if (!LoyaltyProgManager._isLpCompatibility[loyaltyType])
			{
				return false;
			}
			return !LoyaltyProgManager._excludedPrograms[loyaltyType].ContainsKey(discountId);
		}

		public static bool IsLoyalityProgramEnabled(LoyaltyType lpType)
		{
			Dictionary<LoyaltyType, ePlus.Loyalty.LoyaltySettings[]> loyaltySettings = LoyaltyProgManager.LoyaltySettings;
			if (loyaltySettings == null || !loyaltySettings.ContainsKey(lpType))
			{
				return false;
			}
			return loyaltySettings[lpType].First<ePlus.Loyalty.LoyaltySettings>().IS_ENABLED;
		}

		public static bool IsNeedToBeSentToSailPlay(CHEQUE cheque)
		{
			ARMLogger.Trace("Чтение настроек SailPlay");
			LoyaltyProgManager.LoadSailPlaySettings();
			ARMLogger.Trace(string.Format("Мое здоровье. Автоматическая отправка анонимных чеков: {0}", LoyaltyProgManager._spSettings.IsAutoSendAllCheques));
			if (!LoyaltyProgManager._spSettings.IsAutoSendAllCheques)
			{
				return false;
			}
			return !cheque.LoyaltyPrograms.ContainsKey(LoyaltyType.SailPlay);
		}

		public static bool IsUsedAsDiscount(this ILoyaltyProgram lp)
		{
			return lp.LoyaltyType.IsUsedAsDiscount();
		}

		public static bool IsUsedAsDiscount(this LoyaltyType type)
		{
			return false;
		}

		private static bool IsValidEmail(string email)
		{
			bool address;
			string str = email.Trim();
			if (str.EndsWith("."))
			{
				return false;
			}
			try
			{
				address = (new MailAddress(email)).Address == str;
			}
			catch
			{
				address = false;
			}
			return address;
		}

		public static void LoadGlobalConfigFromDinectSettings()
		{
			DiscountMobileLoyaltyProgram.LoadGlobalConfigFromDinectSettings();
		}

		private static void LoadSailPlaySettings()
		{
			ePlus.Loyalty.LoyaltySettings[] loyaltySettingsArray;
			if (!LoyaltyProgManager.LoyaltySettings.TryGetValue(LoyaltyType.SailPlay, out loyaltySettingsArray))
			{
				throw new ArgumentNullException("Настройки SailPlay не найдены");
			}
			SettingsModel settingsModel = new SettingsModel();
			LoyaltyProgManager._spSettings = settingsModel.Deserialize<ePlus.Loyalty.SailPlay.Settings>(loyaltySettingsArray.First<ePlus.Loyalty.LoyaltySettings>().SETTINGS, "Settings");
		}

		public static void MakePreOrderRecalculation(CHEQUE cheque, LoyaltyType type)
		{
			if (!cheque.LoyaltyPrograms.ContainsKey(type))
			{
				return;
			}
			ILoyaltyProgram item = cheque.LoyaltyPrograms[type];
			if (item.IsPreOrderCalculationRequired)
			{
				item.PreOrderCalculation(cheque);
			}
		}

		public static void SendChequeToSailPlay(CHEQUE cheque)
		{
			try
			{
				if (LoyaltyProgManager.IsNeedToBeSentToSailPlay(cheque))
				{
					if (LoyaltyProgManager._spWebApi == null)
					{
						LoyaltyProgManager._spWebApi = new SailPlayWebApi(LoyaltyProgManager._spSettings, PublicIdType.Unknown, null);
					}
					Cart carts = LoyaltyProgManager.CreateSailPlayCart(cheque);
					ARMLogger.Trace(string.Concat("Создана корзина для отпавки в SailPlay: ", carts.ToString()));
					LoyaltyProgManager._spWebApi.PurchasesNewAnonymous(carts, cheque.ID_CHEQUE_GLOBAL);
					ARMLogger.Trace("Информация о чеке успешно отправлена в SailPlay");
				}
			}
			catch (Exception exception)
			{
				ARMLogger.Trace(string.Concat("Не удалось отпавить информацию о чеке в SailPlay. Ошибка: ", exception.Message));
			}
		}

		public static void SendChequeToSailPlayAsync(CHEQUE cheque)
		{
			if (LoyaltyProgManager.IsLoyalityProgramEnabled(LoyaltyType.SailPlay))
			{
				(new Task(() => LoyaltyProgManager.SendChequeToSailPlay(cheque))).Start();
			}
		}

		public static bool SetLoyaltyProgramDebit(LoyaltyProgramDebitArgs args)
		{
			LoyaltyCard delegateGetContainsLoyaltyCard;
			bool flag;
			Func<LoyaltyCard, ILoyaltyProgram, bool> delegateAddLoyaltyCard = (LoyaltyCard arg1, ILoyaltyProgram arg2) => true;
			ILoyaltyProgram delegateGetContainsLoyaltyProgram = args.DelegateGetContainsLoyaltyProgram(args.LoyaltyType);
			bool flag1 = false;
			if (delegateGetContainsLoyaltyProgram != null)
			{
				delegateGetContainsLoyaltyCard = args.DelegateGetContainsLoyaltyCard(args.LoyaltyType);
			}
			else
			{
				string empty = string.Empty;
				string clientId = args.ClientId;
				CustomerCardInfo customerCardInfo = new CustomerCardInfo()
				{
					ClientId = args.ClientId
				};
				if (string.IsNullOrEmpty(args.ClientId) && !args.DelegateCardReader(out customerCardInfo))
				{
					return false;
				}
				empty = customerCardInfo.Last4Digit;
				clientId = customerCardInfo.ClientId;
				if (string.IsNullOrEmpty(clientId))
				{
					clientId = "$EmptyForPromocode$";
				}
				delegateGetContainsLoyaltyProgram = LoyaltyProgManager.GetLoyaltyProgram(args.LoyaltyType, args.LoyaltyInstance, clientId, empty);
				delegateGetContainsLoyaltyCard = args.DelegateCreateLoyaltyCard(delegateGetContainsLoyaltyProgram);
				if (delegateGetContainsLoyaltyCard is ILoyaltyPromocodeList && !string.IsNullOrEmpty(customerCardInfo.Promocode))
				{
					(delegateGetContainsLoyaltyCard as ILoyaltyPromocodeList).AddPromocode(customerCardInfo.Promocode);
				}
				if (delegateGetContainsLoyaltyProgram is MindboxLoyaltyProgram)
				{
					MindboxLoyaltyProgram mindboxLoyaltyProgram = (MindboxLoyaltyProgram)delegateGetContainsLoyaltyProgram;
					if (mindboxLoyaltyProgram.SendPersonalRecomendationRequests)
					{
						mindboxLoyaltyProgram.GetClientRecomendations(args.Cheque);
					}
				}
				delegateAddLoyaltyCard = args.DelegateAddLoyaltyCard;
				flag1 = true;
			}
			if (!delegateAddLoyaltyCard(delegateGetContainsLoyaltyCard, delegateGetContainsLoyaltyProgram))
			{
				return false;
			}
			LoyaltyCardInfo loyaltyCardInfo = delegateGetContainsLoyaltyProgram.GetLoyaltyCardInfo(false);
			if (!AppConfigurator.IsRiglaLic)
			{
				if (string.IsNullOrWhiteSpace(loyaltyCardInfo.ClientEmail) || !LoyaltyProgManager.IsValidEmail(loyaltyCardInfo.ClientEmail))
				{
					if (!string.IsNullOrWhiteSpace(loyaltyCardInfo.ClientPhone))
					{
						if (!loyaltyCardInfo.ClientPhone.All<char>((char x) => {
							if (char.IsDigit(x))
							{
								return true;
							}
							return x == '+';
						}) || !loyaltyCardInfo.ClientPhone.Skip<char>(1).All<char>(new Func<char, bool>(char.IsDigit)))
						{
							goto Label1;
						}
						args.Cheque.SetDigitalChequeInfo(loyaltyCardInfo.ClientPhone, loyaltyCardInfo.ClientEmail);
						goto Label0;
					}
				Label1:
					args.Cheque.SetDigitalChequeInfo(null, null);
				}
				else
				{
					args.Cheque.SetDigitalChequeInfo(loyaltyCardInfo.ClientEmail, loyaltyCardInfo.ClientEmail);
				}
			}
		Label0:
			if (args.Cheque.HasPrepaymentItems())
			{
				args.Cheque.CustomerInfo.CardNumber = loyaltyCardInfo.CardNumber;
				args.Cheque.CustomerInfo.Name = loyaltyCardInfo.ClientName;
				args.Cheque.CustomerInfo.Phone = loyaltyCardInfo.ClientPhone;
				delegateGetContainsLoyaltyCard.DiscountSum = new decimal(0);
				return true;
			}
			if (delegateGetContainsLoyaltyProgram.LoyaltyType == LoyaltyType.AstraZeneca || delegateGetContainsLoyaltyProgram.LoyaltyType == LoyaltyType.Olextra)
			{
				delegateGetContainsLoyaltyCard.DiscountSum = new decimal(0);
				return true;
			}
			if (delegateGetContainsLoyaltyProgram.LoyaltyType == LoyaltyType.DiscountMobile && (delegateGetContainsLoyaltyProgram as DiscountMobileLoyaltyProgram).GetProgramType() != DiscountMobileLoyalty.LoyalityProgramType.Bonus)
			{
				delegateGetContainsLoyaltyCard.DiscountSum = new decimal(0);
				return true;
			}
			if (loyaltyCardInfo.CardStatusId == LoyaltyCardStatus.Limited)
			{
				UtilsArm.ShowMessageInformationOK(string.Format("Статус карты {0} \"Ограничена\". Списание невозможно.", loyaltyCardInfo.CardNumber));
				delegateGetContainsLoyaltyCard.DiscountSum = new decimal(0);
				return true;
			}
			if (loyaltyCardInfo.CardStatusId == LoyaltyCardStatus.Blocked && UtilsArm.ShowMessageQuestionYesNo("Карта \"Заблокирована\". Использование для списания/начисления невозможно. Желаете посмотреть подробную информацию по карте?") == DialogResult.No)
			{
				args.DelegateRemoveLoyaltyProgram(delegateGetContainsLoyaltyProgram);
				throw new LoyaltyCardIsBlockedException(delegateGetContainsLoyaltyProgram);
			}
			LoyaltyProgManager.TakePersonalAdditions(delegateGetContainsLoyaltyProgram);
			using (FrmDebit frmDebit = new FrmDebit())
			{
				decimal delegateChequeSum = args.DelegateChequeSum() + (args.ResetDiscount ? new decimal(0) : delegateGetContainsLoyaltyCard.DiscountSum);
				frmDebit.Bind(delegateGetContainsLoyaltyProgram, delegateGetContainsLoyaltyCard.DiscountSum, args.Cheque);
				frmDebit.EmailEditEvent += new EventHandler((object sender, EventArgs e) => LoyaltyProgManager.EmailEditUserSailPlay(loyaltyCardInfo, frmDebit));
				DialogResult dialogResult = frmDebit.ShowDialog();
				LoyaltyProgManager.UpdateLoyaltyDiscount(frmDebit.DiscountSum, frmDebit.MaxAllowSum);
				if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Cancel && (delegateGetContainsLoyaltyProgram.LoyaltyType == LoyaltyType.Svyaznoy || delegateGetContainsLoyaltyProgram.LoyaltyType == LoyaltyType.PharmacyWallet || delegateGetContainsLoyaltyProgram.LoyaltyType == LoyaltyType.Mindbox))
				{
					delegateGetContainsLoyaltyCard.DiscountSum = frmDebit.DiscountSum;
					flag = true;
				}
				else if (dialogResult != DialogResult.Cancel || !args.UseMaxAllowScoresOnCancel)
				{
					if (flag1)
					{
						args.DelegateRemoveLoyaltyProgram(delegateGetContainsLoyaltyProgram);
					}
					if (loyaltyCardInfo.CardStatusId == LoyaltyCardStatus.Blocked)
					{
						throw new LoyaltyCardIsBlockedException(delegateGetContainsLoyaltyProgram);
					}
					return false;
				}
				else
				{
					decimal num = delegateGetContainsLoyaltyProgram.CalculateMaxSumBonus(args.Cheque);
					delegateGetContainsLoyaltyCard.DiscountSum = (num < delegateGetContainsLoyaltyCard.DiscountSum ? num : delegateGetContainsLoyaltyCard.DiscountSum);
					flag = true;
				}
			}
			return flag;
		}

		public static void ShowLoyaltyProgramBalance(CardReader cardReader, LoyaltyType loyaltyType, Guid LoyaltyInstance, string clientId = null)
		{
			string empty = string.Empty;
			CustomerCardInfo customerCardInfo = null;
			if (!string.IsNullOrEmpty(clientId) || cardReader != null && cardReader(out customerCardInfo))
			{
				if (customerCardInfo != null)
				{
					clientId = customerCardInfo.ClientId;
					empty = customerCardInfo.Last4Digit;
				}
				ILoyaltyProgram loyaltyProgram = LoyaltyProgManager.GetLoyaltyProgram(loyaltyType, LoyaltyInstance, clientId, empty);
				using (FrmBallance frmBallance = new FrmBallance())
				{
					frmBallance.Bind(loyaltyProgram.GetLoyaltyCardInfo(false), loyaltyProgram.Name);
					frmBallance.ShowDialog();
				}
			}
		}

		private static void TakePersonalAdditions(ILoyaltyProgram lp)
		{
			List<string> list;
			lp.RequestPersonalAdditionSales();
			if (lp.PersonalAdditionsSale != null)
			{
				list = lp.PersonalAdditionsSale.ToList<string>();
			}
			else
			{
				list = null;
			}
			List<string> strs = list;
			if (strs != null)
			{
				BusinessLogicEvents.Instance.OnTakePersonalAdditions(new TakePersonalAdditionsEventArgs(strs));
			}
		}

		private static void UpdateLoyaltyDiscount(decimal discountSum, decimal maxAllowSum)
		{
			BusinessLogicEvents.Instance.OnUpdateLoyaltyDiscount(new UpdateLoyaltyDiscountEventArgs(discountSum, maxAllowSum));
		}

		public static bool ValidateCountTransactions(LoyaltyType loyaltyType, ILoyaltyProgram lp, LoyaltyCardInfo cardInfo)
		{
			bool flag = true;
			if (loyaltyType == LoyaltyType.Domestic)
			{
				ePlus.Loyalty.Domestic.Params loyaltyParams = lp.GetLoyaltyParams() as ePlus.Loyalty.Domestic.Params;
				if (loyaltyParams != null)
				{
					if (loyaltyParams.MaxCardTransactionsInDay != 0 && cardInfo.TransactionsCountInDay != 0 && cardInfo.TransactionsCountInDay >= loyaltyParams.MaxCardTransactionsInDay)
					{
						ARMLogger.Trace("Операция начисления ПЛ Аптечка не может быть выполнена. Превышено количество операций по карте в сутки.");
						flag = false;
					}
					else if (loyaltyParams.MaxCardTransactionsInMonth != 0 && cardInfo.TransactionsCountInMonth != 0 && cardInfo.TransactionsCountInMonth >= loyaltyParams.MaxCardTransactionsInMonth)
					{
						ARMLogger.Trace("Операция начисления ПЛ Аптечка не может быть выполнена. Превышено количество операций по карте в месяц.");
						flag = false;
					}
				}
			}
			return flag;
		}

		private static void ZReportPrinting(object sender, EventArgs e)
		{
			try
			{
				string str = DataSyncBL.Instance.CashRegisterSelf.ID_CASH_REGISTER_GLOBAL.ToString();
				(new AstraZenecaLoyaltyProgram(string.Empty, str)).ConfirmStoredAzTransactions();
			}
			catch (Exception exception)
			{
				ARMLogger.ErrorException("Ошбика при попытке отправки подтверждения операций Астразенека", exception);
			}
		}
	}
}