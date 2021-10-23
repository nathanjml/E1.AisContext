using System;
using System.Collections.Generic;
using System.Text;

namespace E1Translator.Core.AIS
{
    public static class E1
    {
        public static class Endpoints
        {
            public static string TokenRequest(string version = "v2") => $"/jderest/${version}/tokenrequest";
            public static string ValidateToken(string version = "v2") => $"/jderest/${version}/tokenrequest/validate";
            public static string TokenLogout(string version = "v2") => $"/jderest/${version}/tokenrequest/logout";
            public static string DataService(string version = "v2") => $"/jderest/${version}/dataservice";
            public static string AppStack(string version = "v2") => $"/jderest/${version}/appstack";
            public static string Orchestrator(string version = "v2") => $"/jderest/${version}/orchestrator";
        }

        public static class Actions
        {
            public const string Open = "open";
            public const string Execute = "execute";
            public const string Close = "close";
        }

        public static class Commands
        {
            public const string DoAction = "DoAction";
            public const string SetControlValue = "SetControlValue";
            public const string SetGridCellValue = "SetGridCellValue";
            public const string SetGridComboValue = "SetGridComboValue";
            public const string SelectRow = "SelectRow";
            public const string SetQBEValue = "SetQBEValue";
            public const string SetRadioButton = "SetRadioButton";
        }
    }
}
