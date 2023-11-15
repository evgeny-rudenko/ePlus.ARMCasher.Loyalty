using System;
using System.Collections.Generic;

namespace ePlus.ARMCasher.Loyalty.PCX
{
	internal class ErrorMessage
	{
		private readonly static Dictionary<int, string> errorsDict;

		static ErrorMessage()
		{
			ErrorMessage.errorsDict = new Dictionary<int, string>()
			{
				{ 2, "Сеанс связи с ПЦ выполнен успешно, но истек лимит времени, требуется повторный вызов Flush." },
				{ 1, "Не удалось выполнить операцию по причине отсутствия доступа к ПЦ. Данный код ошибки возможен для операций: AuthPoints(начисление баллов), Refund(возврат начисления баллов) и Reverse." },
				{ 0, "Функция выполнена успешно." },
				{ -598, "Ошибка при сохранении отката в базу данных. PCX – не сможет отменить операцию самостоятельно, после восстановления доступа к базе данных PCX, необходимо вызвать функцию Reverse." },
				{ -595, "Вызвана функция Flush в момент выполнения автоматического сеанса связи." },
				{ -509, "Истек таймаут при ожидании ответа от ПЦ." },
				{ -481, "Истек таймаут при подключении к ПЦ" },
				{ -587, "Неверный тип идентификатора карты." },
				{ -586, "Не задано ни одно из свойств (CertSubjectName, CertFilePath, KeyFilePath, KeyPassword)." },
				{ -585, "Ошибка при инициализации модуля SSL. (неверный KeyPassword)" },
				{ -584, "Неверный формат данных запроса (отрицательная сумма, в текстовых параметрах присутствуют недопустимые символы и т.п.)" },
				{ -579, "PCX не инициализирован (не был вызван метод Init)." },
				{ -578, "Неверные аргументы (в качестве объектов-параметров были переданы объекты со значением null)." },
				{ -569, "Ошибка при обращении к базе данных." },
				{ -529, "Не удалось загрузить gcframework.dll" },
				{ -525, "PCX уже был инициализирован." },
				{ -519, "Сертификат CertSubjectName не найден." },
				{ -591, "Профиль уже используется другим приложением" },
				{ -518, "Неверный формат сертификата CertSubjectName." },
				{ -151, "Запрошено списание суммы, превышающей текущий остаток на счете." },
				{ -152, "Недопустимое списание. Например, в чеке есть запрещенные товары." },
				{ -162, "Карта заблокирована." },
				{ -163, "Карта не активирована" },
				{ -164, "Карта просрочена." },
				{ -165, "Карта уже активирована" },
				{ -136, "Счет карты заблокирован. В ПЦ возможна привязка нескольких карт к одному счету." },
				{ -157, "Карта ограничена. Списание запрещено." },
				{ -203, "Неизвестный идентификатор партнера (Участника коалиции)." },
				{ -214, "Неизвестная (незарегистрированная в ПЦ) карта." },
				{ -258, "Неизвестный терминал (касса)." },
				{ -320, "Неизвестное критическое расширение в запросе." },
				{ -321, "Плохой формат известного расширения." },
				{ -330, "Некорректное значение аргумента операции. В ответе с таким статусом тестовое сообщение description содержит имя некорректного параметра." },
				{ -340, "Запрошенная операция не поддерживается ПЦ." },
				{ -389, "Ошибка аутентификации.  (Пример – запрос операции с параметром (PCX.Location или PCX.PartnerID или другим)  - не соответствующим выданному CertSubjectName на данную торговую точку)" },
				{ -991, "Внутренняя ошибка ПЦ. Состояние счета не было изменено, откат не требуется." }
			};
		}

		public ErrorMessage()
		{
		}

		public static string GetErrorMessage(int errorCode, string errorMessage)
		{
			if (!ErrorMessage.errorsDict.ContainsKey(errorCode))
			{
				return string.Format("Код ошибки: {0}\r\n{1}", errorCode, errorMessage);
			}
			return string.Format("Код ошибки:{0}\r\n{1}", errorCode, ErrorMessage.errorsDict[errorCode]);
		}
	}
}