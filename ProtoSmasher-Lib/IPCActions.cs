using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ProtoSmasher_Lib
{
    internal enum ActionTypes
    {
		Acknowledged = 1,
		ClearConsole = 2,
		ExecuteSource = 3,
		ExecuteFile = 4,
		GetSettings = 5,
		UpdateSetting = 6,
		UpdateSettingFailed = 7,
		Output = 8
	}

	internal class Action
	{
		public readonly ActionTypes Type;
		public readonly dynamic Body;

		public Action(ActionTypes type, dynamic body)
		{
			Type = type;
			Body = body;
		}

		public static Action FromJson(string s)
		{
			int action;
			dynamic json = JsonConvert.DeserializeObject(s);

			if (json["Action"] is int jsonAction)
				action = jsonAction;
			else if (!int.TryParse((string)json["Action"], out action))
				return default;

			if (action > (int)LastAction || action < 1)
				return default;

			json["Action"] = null;
			return new Action((ActionTypes)action, json);
		}

		private static readonly ActionTypes LastAction = Enum.GetValues(typeof(ActionTypes))
			.Cast<ActionTypes>()
			.Last();
	}
}
