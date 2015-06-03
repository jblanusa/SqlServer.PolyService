using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;

namespace SqlServer.PolyService
{
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined, MaxByteSize = 1000)]
    public class OData : RestWebService
    {

        public OData()
        {
        }

        public string Url
        {
            get
            {
                  return base.GetURL();
            }

        }

        #region Common inherited methods

        [SqlMethod(OnNullCall = false)]
        public new OData AddHeader(string property, string value)
        {
            base.AddHeader(property,value);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new OData AddParameter(string parameter, string value)
        {
              base.AddParameter(parameter,value);
              return this;
        }

        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public new string Get()
        {
            string data = base.Get();
            base.ClearParameters();
            return data;
        }

        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public new string Post(string body)
        {
            string data = base.Post(body);
            base.ClearParameters();
            return data;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static OData Null
        {
            get
            {
                return RestWebService.GetNullValue<OData>();
            }
        }

        [SqlMethod(OnNullCall = false)]
        public static OData Parse(SqlString s)
        {
            return RestWebService.Parse<OData>(s);
        }

        public override string GetURL()
        {
            return base.GetURL();
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
        }

        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);

        }

        public override bool IsEqual(RestWebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            OData odt = (OData) obj;
            return base.IsEqual(obj);
        }

        #endregion

        #region OData query parameters

        [SqlMethod(OnNullCall = false)]
        public OData OrderBy(SqlString field)
        {
            AddParameter("$orderby", field.Value);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public OData Filter(SqlString condition)
        {
            AddParameter("$filter", condition.Value);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public OData Skip(SqlInt64 number)
        {
            AddParameter("$skip", number.Value.ToString());
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public OData Take(SqlInt64 number)
        {
            AddParameter("$top", number.Value.ToString());
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public OData Returns(SqlString properties)
        {
            AddParameter("$select", properties.Value.ToString());
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public OData FormatResult(SqlString properties)
        {
            AddParameter("$format", properties.Value.ToString());
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public OData InlineCount(SqlString condition)
        {
            AddParameter("$inlinecount", condition.Value.ToString());
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public OData SkipToken(SqlString token)
        {
            AddParameter("$skiptoken", token.Value.ToString());
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public OData Expand(SqlString properties)
        {
            AddParameter("$expand", properties.Value.ToString());
            return this;
        }

        #endregion
    }
}
