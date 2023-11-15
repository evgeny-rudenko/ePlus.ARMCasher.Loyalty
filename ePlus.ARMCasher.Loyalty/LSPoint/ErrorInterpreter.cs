using System;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.LSPoint
{
	internal class ErrorInterpreter
	{
		public const short RET_CODE_OK = 0;

		public const short RET_CODE_NO_TERM_CONNECTION = 1;

		public const short RET_CODE_COMMUNICATION_ERROR = 2;

		public const short RET_CODE_SERVER_SEND_DECLINE = 3;

		public const short RET_CODE_TECHNICAL_PROBLEM = 4;

		public const short RET_CODE_FILE_CREATE_FAILED = 5;

		public const short RET_CODE_PROTO_SEQ_ERROR = 6;

		public const short RET_CODE_NOT_IMPLEMENTED = 7;

		public const short RET_CODE_FC_PROBLEM = 8;

		public const short RET_CODE_UNKNOWN_COMMAND = 9;

		public const short RET_CODE_ILLEGAL_PARAMETERS_SET = 10;

		public const short RET_CODE_PAY_TYPE_NOT_ALLOWED = 11;

		public const short RET_CODE_OPERATION_CANCELED = 12;

		public const short RET_CODE_TERMINAL_TECH_PROBLEM = 13;

		public const short RET_CODE_TERMINAL_BLOCKED = 14;

		public const short RET_CODE_NOT_FOUND = 15;

		public const short RET_CODE_REJECTED_CANCELED = 16;

		public const short RET_CODE_TYPES_MISMATCH = 17;

		public const short RET_CODE_NOT_SUPPORTED = 18;

		public const short RET_CODE_ANSWER_TYPE_ERROR = 19;

		public const short RET_CODE_ILLEGAL_ANSWER_TYPE = 20;

		public const short RET_CODE_CONFIG_PARAMS_INVALID = 21;

		public const short RET_CODE_CONFIG_PARAMS_ABSENT = 22;

		public const short RET_CODE_EXTRA_MESSAGES_ABSENT = 24;

		public const short RET_CODE_EM_PROBLEM = 25;

		public const short RET_CODE_PAYMENTS_PROBLEM = 26;

		public const short RET_CODE_CONNECT_BREAK_OFF = 27;

		public const short RET_CODE_SERVER_COMMUNICATION_ERROR = 28;

		public const short RET_CODE_MESSAGE_CERTIFICATE_ERROR = 29;

		public const short RET_CODE_IN_MESSAGE_CONTAIN_CERTIFICATE = 30;

		public const short RET_CODE_IN_CANCEL_BY_USER = 31;

		public ErrorInterpreter()
		{
		}

		public static void OutputErrorInfo(ErrorInterpreter.ReturnCode code, string errorMessage)
		{
			DialogResult dialogResult;
			switch (code)
			{
				case ErrorInterpreter.ReturnCode.NoTermConnection:
				{
					MessageBox.Show("Нет соединения с терминалом", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.CommunicationError:
				{
					MessageBox.Show(string.Concat("Ошибка связи.", Environment.NewLine, "Перезапустите кассовую систему!"), errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				case ErrorInterpreter.ReturnCode.ServerSendDecline:
				{
					MessageBox.Show("Сервер отклонил запрос", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.TechnicalProblem:
				{
					MessageBox.Show(string.Concat("Технические проблемы.", Environment.NewLine, "Перезапустите кассовую систему!"), errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				case ErrorInterpreter.ReturnCode.FileCreateFailed:
				{
					MessageBox.Show(string.Concat("Ошибки работы с файловой системой.", Environment.NewLine, "Перезапустите кассовую систему!"), errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				case ErrorInterpreter.ReturnCode.ProtoSeqError:
				{
					MessageBox.Show(string.Concat("Ошибка последовательности обслуживания.", Environment.NewLine, "Перезапустите кассовую систему!"), errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				case ErrorInterpreter.ReturnCode.NotImplemented:
				{
					MessageBox.Show("Данная функция не доступна при выбранной схеме взаимодействия!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.FcProblem:
				{
					MessageBox.Show("Проблема с запаковкой фискального чека. Возможно, отсутствуют товары или файл с чеком!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				case ErrorInterpreter.ReturnCode.UnknownCommand:
				{
					MessageBox.Show("Неизвестный тип команды!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				case ErrorInterpreter.ReturnCode.IllegalParametersSet:
				{
					MessageBox.Show("Неверный набор параметров!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				case ErrorInterpreter.ReturnCode.PayTypeNotAllowed:
				{
					MessageBox.Show("Выбранный тип оплаты запрещён!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					return;
				}
				case ErrorInterpreter.ReturnCode.OperationCanceled:
				{
					MessageBox.Show("Операция отменена!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.TerminalTechProblem:
				{
					MessageBox.Show("Технические проблемы в терминале!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.TerminalBlocked:
				{
					MessageBox.Show("Терминал заблокирован!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.NotFound:
				{
					MessageBox.Show("Операция отмены отклонена – оригинальная операция не найдена в журнале терминала!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.RejectedCanceled:
				{
					MessageBox.Show("Повтор/Rollback отклонён – оригинальная операция была отменена!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.TypesMismatch:
				{
					MessageBox.Show("Повтор операции отклонён – тип найденной операции не соответствует типу в запросе от ККМ!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.NotSupported:
				{
					MessageBox.Show("Повтор/Rollback данной операции не поддерживается!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.AnswerTypeError:
				{
					MessageBox.Show("На POS произошла внештатная ситуация.", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.IllegalAnswerType:
				{
					MessageBox.Show("Недопустимый тип ответа!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.ConfigParamsInvalid:
				{
					MessageBox.Show("Некорректные конфигурационные параметры!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.ConfigParamsAbsent:
				{
					MessageBox.Show("Отсутствуют конфигурационные параметры!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.NoTermConnection | ErrorInterpreter.ReturnCode.CommunicationError | ErrorInterpreter.ReturnCode.ServerSendDecline | ErrorInterpreter.ReturnCode.TechnicalProblem | ErrorInterpreter.ReturnCode.FileCreateFailed | ErrorInterpreter.ReturnCode.ProtoSeqError | ErrorInterpreter.ReturnCode.NotImplemented | ErrorInterpreter.ReturnCode.RejectedCanceled | ErrorInterpreter.ReturnCode.TypesMismatch | ErrorInterpreter.ReturnCode.NotSupported | ErrorInterpreter.ReturnCode.AnswerTypeError | ErrorInterpreter.ReturnCode.IllegalAnswerType | ErrorInterpreter.ReturnCode.ConfigParamsInvalid | ErrorInterpreter.ReturnCode.ConfigParamsAbsent:
				{
					dialogResult = MessageBox.Show(string.Concat("Ошибка обслуживания: ", (int)code, ".\r\nПерезапустите кассовую систему!"), errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				case ErrorInterpreter.ReturnCode.ExtraMessagesAbsent:
				{
					MessageBox.Show("Отсутствуют экстра сообщения!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.EmProblem:
				{
					MessageBox.Show("Ошибка работы с экстра сообщением!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.PaymentsProblem:
				{
					MessageBox.Show("Ошибка работы с тегом Payments!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.ConnectBreakOff:
				{
					MessageBox.Show("Клиент разорвал установленное соединение!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				case ErrorInterpreter.ReturnCode.ServerCommunicationError:
				{
					MessageBox.Show("Ошибка связи с сервером обслуживания!", errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
				default:
				{
					dialogResult = MessageBox.Show(string.Concat("Ошибка обслуживания: ", (int)code, ".\r\nПерезапустите кассовую систему!"), errorMessage, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
			}
		}

		public enum ReturnCode
		{
			Ok = 0,
			NoTermConnection = 1,
			CommunicationError = 2,
			ServerSendDecline = 3,
			TechnicalProblem = 4,
			FileCreateFailed = 5,
			ProtoSeqError = 6,
			NotImplemented = 7,
			FcProblem = 8,
			UnknownCommand = 9,
			IllegalParametersSet = 10,
			PayTypeNotAllowed = 11,
			OperationCanceled = 12,
			TerminalTechProblem = 13,
			TerminalBlocked = 14,
			NotFound = 15,
			RejectedCanceled = 16,
			TypesMismatch = 17,
			NotSupported = 18,
			AnswerTypeError = 19,
			IllegalAnswerType = 20,
			ConfigParamsInvalid = 21,
			ConfigParamsAbsent = 22,
			ExtraMessagesAbsent = 24,
			EmProblem = 25,
			PaymentsProblem = 26,
			ConnectBreakOff = 27,
			ServerCommunicationError = 28
		}
	}
}