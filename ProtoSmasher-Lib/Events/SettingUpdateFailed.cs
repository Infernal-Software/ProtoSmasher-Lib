using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoSmasher_Lib.Events
{
    public class SettingUpdateFailed
    {
        public readonly string Reason;

        public SettingUpdateFailed(dynamic body)
        {
            if (body["Message"] is string message && !string.IsNullOrEmpty(message))
                Reason = message;
            else
                Reason = "Unknown reason";
        }
    }
}
