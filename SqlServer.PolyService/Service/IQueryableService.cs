using System.Data.SqlTypes;
namespace PolyService.Service
{
    /// <summary>
    /// This should be interface for any service where can be executed query.
    /// </summary>
    interface IQueryableService
    {
        SqlChars ExecuteQuery(SqlString query);
    }
}
