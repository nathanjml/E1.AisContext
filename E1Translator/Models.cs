using System;
using System.Collections.Generic;
using System.Linq;

namespace E1Translator
{
    public class AisAuthInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Environment { get; set; }
        public string Role { get; set; }
        public string DeviceName { get; set; }
    }

    public class AisException
    {
        public string Message { get; set; }
        public string Exception { get; set; }
        public string Timestamp { get; set; }
    }


    public class AisQueryValue
    {
        public string Content { get; set; }
        public string SpecialValueId { get; set; } = "LITERAL";
    }

    public class AisCondition
    {
        public IEnumerable<AisQueryValue> Value { get; set; }
        public string ControlId { get; set; }
        public string Operator { get; set; }
    }



    public class AisComplexQuery
    {
        public string AndOr { get; set; } = "AND";
        public AisQuery? Query { get; set; }
    }


    public class AisQuery
    {
        public bool AutoFind { get; set; } = true;
        public IEnumerable<AisComplexQuery>? ComplexQuery { get; set; }
        public IEnumerable<AisCondition>? Condition { get; set; }
    }



    public class AisOrderBy
    {
        public AisOrderBy() { }

        public AisOrderBy(string column, string dir)
        {
            Column = column;
            Direction = dir;
        }

        public string Column { get; set; }
        public string Direction { get; set; }
    }

    public class AisAggregation
    {
        public IEnumerable<AisOrderBy> OrderBy { get; set; }
    }

    public class AisRequest
    {
        public string ReturnControlIDs { get; set; }
    }

    public class AisDataServiceRequest : AisRequest
    {
        public string TargetName { get; set; }
        public string TargetType { get; set; }
        public string DataServiceType { get; set; }
        public string MaxPageSize { get; set; }
        public AisQuery Query { get; set; }
        public AisAggregation Aggregation { get; set; }
        public bool EnableNextPageProcessing { get; set; }

        public string Token { get; set; }
        public string DeviceName { get; set; }
    }


    public class AisGridDataSummary
    {
        public int Records { get; set; }
        public bool MoreRecords { get; set; }
    }

    public class AisGridData<T>
    {
        public int Id { get; set; }
        public string FullGridId { get; set; }
        public Dictionary<string, AisColumn> ColumnInfo { get; set; }
        public IEnumerable<T> RowSet { get; set; }
        public AisGridDataSummary Summary { get; set; } = new AisGridDataSummary();
    }

    public class AisColumn
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public IEnumerable<AisListOption> List { get; set; }
    }

    public class AisListOption
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class AisDataItem
    {
        public int Id { get; set; }
        public object InternalValue { get; set; }
        public int DataType { get; set; }
        public string Title { get; set; }
        public bool Visible { get; set; }
        public string LongName { get; set; }
        public string AssocDesc { get; set; }
    }

    public class AisData<T>
    {
        public IDictionary<string, AisDataItem> RelatedData { get; set; }
        public AisGridData<T> GridData { get; set; } = new AisGridData<T>();
    }

    public class AisDataBrowser<T>
    {
        public string Title { get; set; }
        public AisData<T> Data { get; set; }
        public IDictionary<string, AisActionControl> ActionControls { get; set; }
        public IEnumerable<AisAppError> Errors { get; set; }
        public IEnumerable<AisAppError> Warnings { get; set; }
    }

    public class AisActionControl
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool Enabled { get; set; }
    }

    public class AisAppError
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string ErrorControl { get; set; }
        public string Desc { get; set; }
        public string Mobile { get; set; }
    }

    public class AisLink
    {
        public string Rel { get; set; }
        public string Href { get; set; }
    }

    public class AisErrorMessage
    {
        public string Title { get; set; }
        public string Desc { get; set; }
    }

    public abstract class BaseE1Response<T>
    {

    }

    public class NoResult { }

    public class AisResponse : BaseE1Response<NoResult>
    {
        public int StackId { get; set; }
        public int StateId { get; set; }
        public string Rid { get; set; }
        public string CurrentApp { get; set; } //"DATABROWSE_V5541021",
        public string TimeStamp { get; set; } // "2019-06-18:18.05.32",
        public IEnumerable<AisErrorMessage> SysErrors { get; set; }
        public IEnumerable<AisLink> Links { get; set; }
    }

    public class AisResponse<TGridData> : AisResponse
    {
        public AisDataBrowser<TGridData> DataBrowser { get; set; }

        public bool HasErrors => (SysErrors?.Any() ?? false) ||
                                 (DataBrowser?.Errors?.Any() ?? false);
    }

    public class NoResponse { }

    public class AisUserInfo
    {
        public string Token { get; set; }
        public string LangPref { get; set; }
        public string Locale { get; set; }
        public string DateFormat { get; set; }
        public string DateSeperator { get; set; }
        public string SimpleDateFormat { get; set; }
        public string DecimalFormat { get; set; }
        public string AddressNumber { get; set; }
        public string AlphaName { get; set; }
        public string AppsRelease { get; set; }
        public string Country { get; set; }
        public string Username { get; set; }
    }

    public class AisAuthResponse
    {
        public string Username { get; set; }
        public string Environment { get; set; }
        public string Role { get; set; }
        public string JasServer { get; set; }
        public AisUserInfo UserInfo { get; set; }
        public bool UserAuthorized { get; set; }
        public string Version { get; set; }
        public string AisSessionCookie { get; set; }
        public bool AdminAuthorized { get; set; }
        public bool PasswordAboutToExpire { get; set; }
    }
}
