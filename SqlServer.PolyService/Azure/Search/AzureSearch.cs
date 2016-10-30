using Microsoft.SqlServer.Server;
using PolyService.Service;
using SqlServer.PolyService;
using System.Data.SqlTypes;

namespace PolyService.Azure
{
    /// <summary>
    /// UDT that represent Azure Search account.
    /// It is described with api-key and url
    /// </summary>
    [Microsoft.SqlServer.Server.SqlUserDefinedTypeAttribute(Microsoft.SqlServer.Server.Format.UserDefined, MaxByteSize = 1000)]
    public class SearchService : OData, IAzureService, IOData
    {
        private string apiKey;
        private string versionString;

        /// <summary>
        /// Constructor adds two headers that is going to be used
        /// and sets Key to an empty string
        /// </summary>
        public SearchService()
        {
            base.AddHeader("Content-Type", "application/json");
            base.AddHeader("Accept", "application/json");
            Key = "";
        }

        #region properties
        /// <summary>
        /// Implementation of property Key from IAzureService interface
        /// </summary>
        public string Key
        {
            get
            {
                return apiKey;
            }
            set
            {
                apiKey = value;
                base.AddHeader("api-key", apiKey);
            }
        }

        /// <summary>
        /// Implementation of property Version from IAzureService interface
        /// </summary>
        public string Version
        {
            get
            {
                return versionString;
            }
            set
            {
                versionString = value;
                base.AddUrlParameter("api-version", Version);
            }
        }

        public string Url
        {
            get
            {
                return base.GetURL();
            }

        }

        #endregion

        /// <summary>
        /// Sql Method that is used for setting api key
        /// </summary>
        /// <param name="aKey">api-key</param>
        /// <returns>this AzureSearch</returns>
        [SqlMethod(OnNullCall = false)]
        public SearchService SetApiKey(SqlString aKey)
        {
            Key = aKey.Value;
            return this;
        }

        #region Common inherited methods

        /// <summary>
        /// Downloading content from Azure Search service as string
        /// </summary>
        /// <returns>json formatted string</returns>
        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public new string Get()
        {
            return base.Get();
        }

        /// <summary>
        /// Uploading content to Azure Search service
        /// </summary>
        /// <param name="body">json formatted string</param>
        /// <returns>web response as json string</returns>
        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public new string Post(string body)
        {
            return base.Post(body);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static new SearchService Null
        {
            get
            {
                return RestWebService.GetNullValue<SearchService>();
            }
        }

        [SqlMethod(OnNullCall = false)]
        public static new SearchService Parse(SqlString s)
        {
            SearchService azs = RestWebService.ParseLiteral<SearchService>(s);
            if (azs.urlParameters.ContainsKey("api-version"))
            {
                azs.versionString = azs.urlParameters["api-version"];
            }
            else
            {
                azs.Version = "2014-07-31-Preview";
            }
            return azs;
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
            w.Write(Key);
            w.Write(Version);
        }

        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);
            Key = r.ReadString();
            Version = r.ReadString();
        }

        public override bool IsEqual(WebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            SearchService azs = (SearchService)obj;
            if (this.apiKey != azs.apiKey) return false;
            if (this.Version != azs.Version) return false;
            return base.IsEqual(obj);
        }

        #endregion

        #region OData query parameters

        [SqlMethod(OnNullCall = false)]
        public new SearchService OrderBy(SqlString field)
        {
            base.OrderBy(field);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new SearchService Filter(SqlString condition)
        {
            base.Filter(condition);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new SearchService Skip(SqlInt64 number)
        {
            base.Skip(number);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new SearchService Take(SqlInt64 number)
        {
            base.Take(number);
           return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new SearchService Select(SqlString properties)
        {
            base.Select(properties);
           return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new SearchService Format(SqlString properties)
        {
            base.Format(properties);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new SearchService InlineCount(SqlString condition)
        {
            base.InlineCount(condition);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new SearchService SkipToken(SqlString token)
        {
            base.SkipToken(token);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new SearchService Expand(SqlString properties)
        {
            base.Expand(properties);
            return this;
        }

        #endregion

        #region Azure Search query parameters
        [SqlMethod(OnNullCall = false)]
        public SearchService Search(SqlString field)
        {
            AddRequestParameter("search", field.Value);
            return this;
        }

        public SearchService SearchModeAll()
        {
            AddRequestParameter("searchMode", "all");
            return this;
        }

        public SearchService Facet(SqlString field)
        {
            AddRequestParameter("facet", field.Value);
            return this;
        }

        public SearchService SearchFields(SqlString field)
        {
            AddRequestParameter("searchFields", field.Value);
            return this;
        }

        public SearchService Highlight(SqlString field)
        {
            AddRequestParameter("highlight", field.Value);
            return this;
        }

        public SearchService HighlightPreTag(SqlString field)
        {
            AddRequestParameter("highlightPreTag", field.Value);
            return this;
        }

        public SearchService HighlightPostTag(SqlString field)
        {
            AddRequestParameter("highlightPostTag", field.Value);
            return this;
        }

        public SearchService ScoringProfile(SqlString field)
        {
            AddRequestParameter("scoringProfile", field.Value);
            return this;
        }

        public SearchService ScoringParameter(SqlString field)
        {
            AddRequestParameter("scoringParameter", field.Value);
            return this;
        }
        #endregion

    }
}
