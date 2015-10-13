using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;

namespace PolyService.Service
{
    /// <summary>
    /// CLR UDT that enables user to send Http requests to OData service.
    /// </summary>
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class OData : RestWebService, PolyService.Service.IOData
    {
        /// <summary>
        /// Creates OData client instance that read all fields from the OData service.
        /// </summary>
        public OData()
        {
            // By default select everything.
            base.AddHeader("$select", "*");
        }

        #region Common inherited methods

        /// <summary>
        /// Add new header value.
        /// </summary>
        /// <param name="property">Name of the HTTP header parameter.</param>
        /// <param name="value">Value of the HTTP header parameter.</param>
        /// <returns>OData instance configured with new header parameter.</returns>
        [SqlMethod(OnNullCall = false)]
        public new OData AddHeader(string property, string value)
        {
            base.AddHeader(property,value);
            return this;
        }

        /// <summary>
        /// Add new URL parameter.
        /// </summary>
        /// <param name="parameter">Name of the parameter.</param>
        /// <param name="value">Value of the  parameter.</param>
        /// <returns>OData instance configured with new parameter.</returns>
        [SqlMethod(OnNullCall = false)]
        public new OData Param(string parameter, string value)
        {
              base.AddRequestParameter(parameter,value);
              return this;
        }

        /// <summary>
        /// Get content from OData source.
        /// </summary>
        /// <returns></returns>
        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public new string Get()
        {
            string data = base.SendGetRequest();
            base.ClearRequestParameters();
            return data;
        }

        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public string Post(string body)
        {
            string data = base.SendPostRequest(body);
            base.ClearRequestParameters();
            return data;
        }


        [SqlMethod(OnNullCall = false)]
        public static new OData Parse(SqlString s)
        {
            return WebService.ParseLiteral<OData>(s);
        }

        public override string GetURL()
        {
            return base.GetURL();
        }

        public override bool IsEqual(WebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            OData odt = (OData) obj;
            return base.IsEqual(obj);
        }

        #endregion

        #region CLR UDT implementation

        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
        }

        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);

        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static new OData Null
        {
            get
            {
                return WebService.GetNullValue<OData>();
            }
        }

        #endregion

        #region OData query parameters

        /// <summary>
        /// Order results using specified property in the data source ($orderby parameter).
        /// </summary>
        /// <param name="field">Property in the data source that wil be used to order results.</param>
        /// <returns>OData instance configured to order results.</returns>
        [SqlMethod(OnNullCall = false)]
        public OData OrderBy(SqlString field)
        {
            AddRequestParameter("$orderby", field.Value);
            return this;
        }

        /// <summary>
        /// Applies filter condition ($filter parameter).
        /// </summary>
        /// <param name="condition">Condition that will be applied in OData HTTP query.</param>
        /// <returns>OData instance configured to filter results using the specified criterion.</returns>
        [SqlMethod(OnNullCall = false)]
        public OData Filter(SqlString condition)
        {
            AddRequestParameter("$filter", condition.Value);
            return this;
        }

        /// <summary>
        /// Skips first N records that match criterion ($skip parameter).
        /// </summary>
        /// <param name="number">Number of records that should be skipped.</param>
        /// <returns>OData instance configured to skip <paramref name="number"/> results.</returns>
        [SqlMethod(OnNullCall = false)]
        public OData Skip(SqlInt64 number)
        {
            AddRequestParameter("$skip", number.Value.ToString());
            return this;
        }

        /// <summary>
        /// Takes first N records from the data source and ignore others ($top parameter).
        /// </summary>
        /// <param name="number">Number of records that should be returned</param>
        /// <returns>OData instance configured to skip <paramref name="number"/> results.</returns>
        [SqlMethod(OnNullCall = false)]
        public OData Take(SqlInt64 number)
        {
            AddRequestParameter("$top", number.Value.ToString());
            return this;
        }

        /// <summary>
        /// Specifies what fields in the OData data source should be fetched ($select parameter).
        /// </summary>
        /// <param name="properties">Properties in the data source that should be fetched in the OData request.</param>
        /// <returns>OData instance configured to select only specified <paramref name="properties"/>.</returns>
        [SqlMethod(OnNullCall = false)]
        public OData SelectFields(SqlString properties)
        {
            AddRequestParameter("$select", properties.Value.ToString());
            return this;
        }

        /// <summary>
        /// Format results as JSON or XML/ATOM.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        [SqlMethod(OnNullCall = false)]
        public OData FormatResult(SqlString properties)
        {
            AddRequestParameter("$format", properties.Value.ToString());
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public OData InlineCount(SqlString condition)
        {
            AddRequestParameter("$inlinecount", condition.Value.ToString());
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public OData SkipToken(SqlString token)
        {
            AddRequestParameter("$skiptoken", token.Value.ToString());
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public OData Expand(SqlString properties)
        {
            AddRequestParameter("$expand", properties.Value.ToString());
            return this;
        }

        #endregion
    }
}
