namespace PolyService.Service
{
    interface IOData
    {
        OData Expand(global::System.Data.SqlTypes.SqlString properties);
        OData Filter(global::System.Data.SqlTypes.SqlString condition);
        OData FormatResult(global::System.Data.SqlTypes.SqlString properties);
        OData InlineCount(global::System.Data.SqlTypes.SqlString condition);
        OData SelectFields(global::System.Data.SqlTypes.SqlString properties);
        OData Skip(global::System.Data.SqlTypes.SqlInt64 number);
        OData SkipToken(global::System.Data.SqlTypes.SqlString token);
        OData Take(global::System.Data.SqlTypes.SqlInt64 number);
    }
}
