using System;
using System.Collections.Generic;
using TurnerTablet.Core.Scaffolding.Features.Ais;
using static E1Translator.Core.AIS.E1;

namespace E1Translator.Core.Helpers
{
    public static class Dir
    {
        public const string Asc = "ASC";
        public const string Desc = "DESC";
    }

    public static class Op
    {
        public const string Equal = "EQUAL";
        public const string NotEqual = "NOT_EQUAL";
        public const string StartsWith = "STR_START_WITH";
        public const string EndsWith = "STR_END_WITH";
        public const string LessThan = "LESS";
        public const string LessThanEqual = "LESS_EQUAL";
        public const string GreaterThan = "GREATER";
        public const string GreaterThanEqual = "GREATER_EQUAL";
        public const string Blank = "STR_BLANK";
        public const string NotBlank = "STR_NOT_BLANK";
        public const string Between = "BETWEEN";
        public const string List = "LIST";
    }

    public static class AisQueryHelper
    {
        public static AisOrderBy Ascending(string column)
        {
            return new AisOrderBy { Column = column, Direction = Dir.Asc };
        }

        public static AisOrderBy Descending(string column)
        {
            return new AisOrderBy { Column = column, Direction = Dir.Desc };
        }
    }


    public static class Condition
    {
        public static AisCondition Create(string op, string column, string value = null)
        {
            var cond = new AisCondition
            {
                ControlId = column,
                Operator = op
            };

            if (value != null)
            {
                cond.Value = new List<AisQueryValue>
                    {
                        Literal(value)
                    };
            }

            return cond;
        }

        public static AisCondition Equal(string column, string value)
        {
            return Create(Op.Equal, column, value);
        }

        public static AisCondition LessThan(string column, string value)
        {
            return Create(Op.LessThan, column, value);
        }

        public static AisCondition IsBlank(string column)
        {
            return Create(Op.Blank, column);
        }

        public static AisCondition GreaterThan(string column, string value)
        {
            return Create(Op.GreaterThan, column, value);
        }

        private static AisQueryValue Literal(string value)
        {
            return new AisQueryValue
            {
                Content = value,
                SpecialValueId = "LITERAL"
            };
        }
    }

    public static class FormAction
    {
        /*
           Allowed Values: [ "SetQBEValue", "SetControlValue", 
               "SetRadioButton", "SetComboValue", "DoAction", 
               "SetCheckboxValue", "SelectRow", "UnSelectRow", 
               "UnSelectAllRows", "SelectAllRows", "ClickGridCell", 
               "ClickGridColumnAggregate", "NextGrid" ]
       */
        public static AisFormAction Create(string command, string ctrl, string value = null)
        {
            return new AisFormAction
            {
                Command = command,
                ControlID = ctrl,
                Value = value
            };
        }

        public static AisFormAction DoAction(string ctrl)
        {
            return Create(Commands.DoAction, ctrl);
        }

        public static AisFormAction SetControlValue(string ctrl, string value)
        {
            return Create(Commands.SetControlValue, ctrl, value);
        }

        public static AisFormAction SetRadioButton(string ctrl)
        {
            return Create(Commands.SetRadioButton, ctrl);
        }

        internal static object SetRadioButton(object unitAreaMode)
        {
            throw new NotImplementedException();
        }
    }
}
