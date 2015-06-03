using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;

namespace SqlServer.PolyService
{
    /// <summary>
    /// UDT that represent Azure Search account.
    /// It is described with api-key and url
    /// </summary>
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined, MaxByteSize = 1000)]
    public class AzureSearch : OData, IAzureService
    {
        private string apiKey;
        private string versionString;

        /// <summary>
        /// Constructor adds two headers that is going to be used
        /// and sets Key to an empty string
        /// </summary>
        public AzureSearch()
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
                base.AddPersistantParameter("api-version", Version);
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
        public AzureSearch SetApiKey(SqlString aKey)
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

        public static AzureSearch Null
        {
            get
            {
                return RestWebService.GetNullValue<AzureSearch>();
            }
        }

        [SqlMethod(OnNullCall = false)]
        public static AzureSearch Parse(SqlString s)
        {
            AzureSearch azs = RestWebService.Parse<AzureSearch>(s);
            if (azs.persistantParameters.ContainsKey("api-version"))
            {
                azs.versionString = azs.persistantParameters["api-version"];
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

        public override bool IsEqual(RestWebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            AzureSearch azs = (AzureSearch)obj;
            if (this.apiKey != azs.apiKey) return false;
            if (this.Version != azs.Version) return false;
            return base.IsEqual(obj);
        }

        #endregion

        #region OData query parameters

        [SqlMethod(OnNullCall = false)]
        public new AzureSearch OrderBy(SqlString field)
        {
            base.OrderBy(field);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new AzureSearch Filter(SqlString condition)
        {
            base.Filter(condition);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new AzureSearch Skip(SqlInt64 number)
        {
            base.Skip(number);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new AzureSearch Take(SqlInt64 number)
        {
            base.Take(number);
           return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new AzureSearch Returns(SqlString properties)
        {
            base.Returns(properties);
           return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new AzureSearch FormatResult(SqlString properties)
        {
            base.FormatResult(properties);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new AzureSearch InlineCount(SqlString condition)
        {
            base.InlineCount(condition);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new AzureSearch SkipToken(SqlString token)
        {
            base.SkipToken(token);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new AzureSearch Expand(SqlString properties)
        {
            base.Expand(properties);
            return this;
        }

        #endregion

        #region Azure Search query parameters
        [SqlMethod(OnNullCall = false)]
        public AzureSearch Search(SqlString field)
        {
            AddParameter("search", field.Value);
            return this;
        }

        public AzureSearch SearchModeAll()
        {
            AddParameter("searchMode", "all");
            return this;
        }

        public AzureSearch Facet(SqlString field)
        {
            AddParameter("facet", field.Value);
            return this;
        }

        public AzureSearch SearchFields(SqlString field)
        {
            AddParameter("searchFields", field.Value);
            return this;
        }

        public AzureSearch Highlight(SqlString field)
        {
            AddParameter("highlight", field.Value);
            return this;
        }

        public AzureSearch HighlightPreTag(SqlString field)
        {
            AddParameter("highlightPreTag", field.Value);
            return this;
        }

        public AzureSearch HighlightPostTag(SqlString field)
        {
            AddParameter("highlightPostTag", field.Value);
            return this;
        }

        public AzureSearch ScoringProfile(SqlString field)
        {
            AddParameter("scoringProfile", field.Value);
            return this;
        }

        public AzureSearch ScoringParameter(SqlString field)
        {
            AddParameter("scoringParameter", field.Value);
            return this;
        }
        #endregion

    }
}
