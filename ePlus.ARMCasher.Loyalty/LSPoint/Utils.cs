using Microsoft.VisualBasic;
using System;

namespace ePlus.ARMCasher.Loyalty.LSPoint
{
	internal static class Utils
	{
		public static string PrepareString0D0A(ref string strIn)
		{
			string str = null;
			int i = 0;
			int length = 0;
			str = strIn;
			length = str.Length;
			for (i = 0; i < length; i++)
			{
				if (str[i] == Strings.Chr(10) | str[i] == Strings.Chr(13))
				{
					i++;
					if (i >= length)
					{
						str = str.Remove(i - 1, 1);
						int num = Strings.Chr(13) + Strings.Chr(10);
						str = str.Insert(i - 1, num.ToString());
					}
					else if (str[i - 1] == Strings.Chr(10) & str[i] == Strings.Chr(13))
					{
						str = str.Remove(i - 1, 2);
						int num1 = Strings.Chr(13) + Strings.Chr(10);
						str = str.Insert(i - 1, num1.ToString());
						length = str.Length;
					}
					else if (str[i - 1] == Strings.Chr(13) & str[i] == Strings.Chr(10))
					{
						str = str.Remove(i - 1, 2);
						int num2 = Strings.Chr(13) + Strings.Chr(10);
						str = str.Insert(i - 1, num2.ToString());
						length = str.Length;
					}
					else if (str[i - 1] == str[i])
					{
						str = str.Remove(i - 1, 1);
						int num3 = Strings.Chr(13) + Strings.Chr(10);
						str = str.Insert(i - 1, num3.ToString());
						length = str.Length;
					}
					else if (!(str[i] != Strings.Chr(0) & str[i] != Strings.Chr(13) & str[i] != Strings.Chr(10)))
					{
						str = str.Remove(i - 1, 1);
						int num4 = Strings.Chr(13) + Strings.Chr(10);
						str = str.Insert(i - 1, num4.ToString());
						length = str.Length;
						i--;
					}
					else
					{
						str = str.Remove(i - 1, 1);
						int num5 = Strings.Chr(13) + Strings.Chr(10);
						str = str.Insert(i - 1, num5.ToString());
						length = str.Length;
					}
				}
			}
			return str;
		}

		public static string Str2Hex(string input)
		{
			string str = "";
			for (int i = 1; i <= Strings.Len(input); i++)
			{
				str = string.Concat(str, Conversion.Hex(Strings.Asc(Strings.Mid(input, i, 1))));
			}
			return str;
		}
	}
}