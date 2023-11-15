using System;

namespace ePlus.ARMCasher.Loyalty.PCX
{
	public static class PCXUtils
	{
		public static string GetCardNumberMasked(string cardNumber)
		{
			return string.Format("** {0}", PCXUtils.GetLast4Digit(cardNumber));
		}

		private static string GetLast4Digit(string cardNumber)
		{
			string empty = string.Empty;
			if (!string.IsNullOrWhiteSpace(cardNumber))
			{
				string str = cardNumber.Trim();
				int length = str.Length;
				if (length <= 4)
				{
					empty = str;
				}
				else if (length > 4)
				{
					empty = str.Substring(length - 4);
				}
			}
			return empty;
		}

		public static int TruncateNonZero(decimal value)
		{
			if (value == new decimal(0))
			{
				return 0;
			}
			int num = Convert.ToInt32(Math.Truncate(value));
			if (num == 0)
			{
				num++;
			}
			return num;
		}
	}
}