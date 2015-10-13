using Microsoft.SqlServer.Server;
using PolyService.Service;
using System;
using System.Data.SqlTypes;

namespace PolyService.Azure
{
    /// <summary>
    /// User defined type that represents some Azure Machine Learning application
    /// from Azure Marketplace.
    /// </summary>
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class AzureMLApp : RestWebService, IAzureService
    {
        private string accountKey = "";
        private string username = "";

        public AzureMLApp()
        {
            base.AddHeader("Accept","application/json");
            Version = "";
        }

        #region properties
        public string Key
        {
            get
            {
                return accountKey;
            }
            set
            {
                accountKey = value;
                if (username != "")
                {
                    base.AddHeader("Authorization", CreateAuthenticationHeader(username, accountKey));
                }
            }
        }

        public string UserName
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
                if (accountKey != "")
                {
                    base.AddHeader("Authorization", CreateAuthenticationHeader(username, accountKey));
                }
            }
        }

        public string Version
        {
            get;
            set;
        }

        public string Url
        {
            get
            {
                return base.GetURL();
            }

        }
        #endregion

        #region Common inherited methods

        /// <summary>
        /// Downloading content from Azure Machine Learning app as string
        /// </summary>
        /// <returns>json formatted string</returns>
        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public new string Get()
        {
            return base.SendGetRequest();
        }

        /// <summary>
        /// Uploading content to Azure Machine Learning
        /// </summary>
        /// <param name="body">json formatted string</param>
        /// <returns>web response as json string</returns>
        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public new string Post(string body)
        {
            return base.SendPostRequest(body);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static AzureMLApp Null
        {
            get
            {
                return RestWebService.GetNullValue<AzureMLApp>();
            }
        }

        [SqlMethod(OnNullCall = false)]
        public static AzureMLApp Parse(SqlString s)
        {
            return RestWebService.ParseLiteral<AzureMLApp>(s);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
            w.Write(Key);
            w.Write(UserName);
            w.Write(Version);
        }

        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);
            Key = r.ReadString();
            UserName = r.ReadString();
            Version = r.ReadString();
        }

        public override bool IsEqual(WebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            AzureMLApp aml = (AzureMLApp)obj;
            if (this.Key != aml.Key) return false;
            if (this.UserName != aml.UserName) return false;
            if (this.Version != aml.Version) return false;
            return base.IsEqual(obj);
        }

        #endregion

        #region AzureMLApp specific methods
        /// <summary>
        /// Sets account key for this app
        /// </summary>
        /// <param name="aKey">Account Key</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public AzureMLApp SetAccountKey(SqlString aKey)
        {
            Key = aKey.Value;
            return this;
        }

        /// <summary>
        /// Sets user name for this app
        /// </summary>
        /// <param name="uname">User Name</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public AzureMLApp SetUserName(SqlString uname)
        {
            UserName = uname.Value;
            return this;
        }

        /// <summary>
        /// Sets both user name and account key for this app
        /// </summary>
        /// <param name="uname">User Name</param>
        /// <param name="akey">Account Key</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public AzureMLApp SetNamePass(SqlString uname, SqlString akey)
        {
            UserName = uname.Value;
            Key = akey.Value;
            return this;
        }

        /// <summary>
        /// Creates authentication header
        /// </summary>
        /// <param name="username">User Name</param>
        /// <param name="accountKey">Account Key</param>
        /// <returns>Authentication header</returns>
        protected virtual string CreateAuthenticationHeader(string username, string accountKey)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(username + ":" + accountKey);
            return "Basic " + Convert.ToBase64String(byteArray);
        }
        #endregion
    }
}
