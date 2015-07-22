using Microsoft.SqlServer.Server;
using PolyService.Common;
using PolyService.Service;
using System;
using System.Data.SqlTypes;
using System.Text;

namespace PolyService.Azure
{
    [SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class DocumentDB : RestWebService, IAzureService, IQueryableService
    {
        private string versionString;

        public DocumentDB()
        {
            Version = "2014-08-21";
            Key = "";
        }

        #region properties

        public string Key { get; set; }

        public string Version
        {
            get
            {
                return versionString;
            }
            set
            {
                versionString = value;
                base.AddHeader("x-ms-version", versionString);
            }
        }

        public string Database { get; set; }

        public string Collection { get; set; }

        #endregion

        #region Common inherited methods

        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
            w.Write(Key);
            w.Write(Version);
            w.Write(Collection??string.Empty);
            w.Write(Database ?? string.Empty);
        }

        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);
            Key = r.ReadString();
            Version = r.ReadString();
            Collection = r.ReadString();
            Database = r.ReadString();
        }

        [SqlMethod(OnNullCall = false)]
        public static DocumentDB Parse(SqlString s)
        {
            return RestWebService.ParseLiteral<DocumentDB>(s);
        }

        public static DocumentDB Null
        {
            get
            {
                return RestWebService.GetNullValue<DocumentDB>();
            }
        }

        public override string GetURL()
        {
            if (string.IsNullOrEmpty(this.Database) || string.IsNullOrEmpty(this.Collection))
            {

                return base.GetURL().TrimEnd('/') + "/docs/";
            }
            else
            {
                return base.GetURL().TrimEnd('/') + "/dbs/" + this.Database + "/colls/" + this.Collection + "/docs/";
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool IsEqual(RestWebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            DocumentDB ddb = (DocumentDB)obj;
            if (this.Key != ddb.Key) return false;
            if (this.Version != ddb.Version) return false;
            return base.IsEqual(ddb);
        }

        protected override void AddParameter(string parameter, string value)
        {
            //DocumentDB doesn't have parameters
        }

        protected override void AddPersistantParameter(string parameter, string value)
        {
            //DocumentDB doesn't have parameters
        }

        #endregion

        #region DocumentDB specific methods

        [SqlMethod(OnNullCall = false)]
        public DocumentDB SetAccountKey(SqlString key)
        {
            Key = key.Value;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public DocumentDB SetDatabaseId(SqlString dbId)
        {
            this.Database = dbId.Value;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public DocumentDB SetCollectionId(SqlString collId)
        {
            this.Collection = collId.Value;
            return this;
        }

        private string getAuthorizationHeaderUsingMasterKey(string verb, string resourceType, string resourceId, string date, string masterKey)
        {
            verb = verb ?? "";
            resourceId = resourceId ?? "";
            resourceType = resourceType ?? "";
            date = date ?? "";
            masterKey = masterKey ?? "";

            string stringToSign = verb + "\n" +
                                  resourceType + "\n" +
                                  resourceId + "\n" +
                                  date + "\n"
                                  + "\n";

            byte[] SignatureBytes = Encoding.UTF8.GetBytes(stringToSign.ToLower());
            System.Security.Cryptography.HMACSHA256 SHA256 = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(masterKey));
            string signature = Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes));

            return Uri.EscapeDataString("type=master&ver=1.0&sig=" + signature);
        }

        /// <summary>
        /// Perform querying on collection determined from url
        /// </summary>
        /// <param name="query">Query body</param>
        /// <returns>Result of query in json format</returns>
        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public SqlChars ExecuteQuery(SqlString query)
        {
            if (!IsValidUrl)
            {
                throw new Exception("Url " + this.GetURL() + " not valid for querying document");
            }

            string date = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", new System.Globalization.CultureInfo("en-US")) + " GMT";
            base.AddHeader("Content-Type", "application/query+json");
            base.AddHeader("x-ms-documentdb-isquery", "True");
            base.AddHeader("Accept", "application/json");
            base.AddHeader("x-ms-date", date);
            string collectionId = string.IsNullOrEmpty(this.Collection) ? GetResourceID("colls") : this.Collection;

            base.AddHeader("Authorization", getAuthorizationHeaderUsingMasterKey("post", "docs", collectionId, date, Key));
            base.AddHeader("x-ms-version", versionString);

            string jsonQuery = "{\"query\":\"" + query.Value + "\"}";
            string response = Post(jsonQuery);
            return new SqlChars(response.ToCharArray());
        }

        /// <summary>
        /// Creates document on collection determined from url
        /// </summary>
        /// <param name="body">document in json format that is going to be created</param>
        /// <returns>This DocumentDB</returns>
        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public SqlChars CreateDocument(SqlString body)
        {
            if (!IsValidUrl)
            {
                throw new Exception("Url " + this.GetURL() + " not valid for querying document");
            }

            string date = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", new System.Globalization.CultureInfo("en-US")) + " GMT";
            base.AddHeader("Content-Type", "application/json");
            base.AddHeader("x-ms-date", date);
            string collectionId = string.IsNullOrEmpty(this.Collection) ? GetResourceID("colls") : this.Collection;
            base.AddHeader("Authorization", getAuthorizationHeaderUsingMasterKey("post", "docs", collectionId, date, Key));

            return new SqlChars(Post(body.Value).ToCharArray());
        }

        /// <summary>
        /// Creates document on collection determined from url
        /// </summary>
        /// <param name="body">document in json format that is going to be created</param>
        /// <returns>This DocumentDB</returns>
        [SqlMethod(OnNullCall = false)]
        public DocumentDB DeleteDocument(SqlString rid)
        {
            if (!IsValidUrl)
            {
                throw new Exception("Url " + this.GetURL() + " not valid for querying document");
            }
            const string resourceType = "docs";
            string resourceID = rid.Value;
            string date = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss", new System.Globalization.CultureInfo("en-US")) + " GMT";

            base.AddHeader("Content-Type", "application/json");
            base.AddHeader("x-ms-date", date);
            base.AddHeader("Authorization", getAuthorizationHeaderUsingMasterKey("delete", resourceType, resourceID, date, Key));
            
            Delete(this.GetURL() + '/' + resourceID);

            return this;
        }

        /// <summary>
        /// Checks whether current url corresponds to url that should
        /// query, create or delete document in DocumentDB
        /// </summary>
        private bool IsValidUrl
        {
            get
            {
                string[] urlSplit = this.GetURL().Split('/');
                if (urlSplit.Length < 8) return false;
                if (urlSplit[3] != "dbs" || urlSplit[5] != "colls" || urlSplit[7] != "docs") return false;
                return true;
            }
        }

        private string GetResourceID(string rtype)
        {
            string[] urlSplit = this.GetURL().Split('/');
            for (int i = 0; i < urlSplit.Length; i++)
            {
                if (urlSplit[i] == rtype && i < urlSplit.Length - 1)
                {
                    return urlSplit[i + 1];
                }

            }

            return "";
        }
        #endregion


        /// <summary>
        /// Creates DocumentDb service using the datasource.
        /// </summary>
        /// <param name="ds"></param>
        [SqlFunction()]
        public static DocumentDB CreateDocumentDbService(DataSource ds)
        {
            return RestWebService.CreateService<DocumentDB>(ds);
        }
    }
}
