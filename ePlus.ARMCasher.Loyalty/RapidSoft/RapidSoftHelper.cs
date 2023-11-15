using System;
using System.Security.Cryptography;
using System.Text;

namespace ePlus.ARMCasher.Loyalty.RapidSoft
{
	internal static class RapidSoftHelper
	{
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

		public static string NumberToHash(string number)
		{
			SHA1Managed sHA1Managed = new SHA1Managed();
			byte[] bytes = Encoding.ASCII.GetBytes(number);
			return RapidSoftHelper.HexStringFromBytes(sHA1Managed.ComputeHash(bytes)).ToUpper();
		}

		internal static string OperationStatusText(OperationStatus status)
		{
			OperationStatus operationStatu = status;
			switch (operationStatu)
			{
				case OperationStatus.Success:
				{
					return "Операция успешно выполнена";
				}
				case OperationStatus.UnknownError:
				{
					return "Произошла неизвестная ошибка";
				}
				case OperationStatus.Denied:
				{
					return "Отказ процессинга";
				}
				case OperationStatus.NoLoyalty:
				{
					return "Программа лояльности не найдена";
				}
				default:
				{
					if (operationStatu == OperationStatus.AnotherLoyalty)
					{
						break;
					}
					else
					{
						return string.Empty;
					}
				}
			}
			return "Переданная карта принадлежит сторонней программе лояльности";
		}

		internal static string OperationStatusText(RollbackStatus status)
		{
			switch (status)
			{
				case RollbackStatus.Success:
				{
					return "Операция успешно выполнена";
				}
				case RollbackStatus.UnknownError:
				{
					return "Произошла неизвестная ошибка";
				}
				case RollbackStatus.Denied:
				{
					return "Отказ процессинга";
				}
				case RollbackStatus.UnknownError | RollbackStatus.Denied:
				case 4:
				{
					return string.Empty;
				}
				case RollbackStatus.Impossible:
				{
					return "Отмена невозможна";
				}
				default:
				{
					return string.Empty;
				}
			}
		}
	}
}