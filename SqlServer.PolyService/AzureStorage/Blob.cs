using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using System.Text;

namespace SqlServer.PolyService
{      

    /// <summary>
    ///UDT that represents one blob in some storage account.
    ///It is described with url and account key.
    /// </summary>
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class Blob : RestWebService, IAzureService
    {

        #region Constructors

        public Blob(string path, string key)
        {
            this.url = new StringBuilder(path);
            AccountKey = key;
            Version = "2009-09-19";
        }

        public Blob(string accName, string blobName, string key)
        {
            this.url = new StringBuilder("https://"+accName+".blob.core.windows.net/"+blobName);
            AccountKey = key;
            Version = "2009-09-19";
        }

        public Blob()
        {
            AccountKey = "";
            Version = "2009-09-19";
        }

        #endregion

        #region Properties

        private string AccountKey;

        public string Key
        {
            get
            {
                return AccountKey;
                
            }
            set { AccountKey = value; }
        }
        public string Version
        {
            get; set;
        }

        public string Url
        {
            get
            {
                return url.ToString();
            }
        }

        #endregion

        #region Common inherited methods

        /// <summary>
        /// Returns content of the blob
        /// </summary>
        /// <returns>Binary blob content</returns>
        [SqlMethod()]
        [return: SqlFacet(MaxSize = -1)]
        public new SqlBinary Get()
        {
            
            if (this.IsNull)
            {
                return null;
            }
            else
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

                    base.AddHeader("x-ms-date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT");
                    request.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT");
                    base.AddHeader("x-ms-version", Version);
                    request.Headers.Add("x-ms-version", Version);
                    request.Method = "GET";
                    request.ContentLength = 0;

                    base.AddHeader("Authorisation", BlobAuthorisation.GetAuthorisationHeader(request, AccountKey));

                    byte[] buff;
                    using (WebResponse response = request.GetResponse())
                    {
                        Stream stream = response.GetResponseStream();
                        long buffLen = response.ContentLength;
                        buff = new byte[buffLen];
                        stream.Read(buff, 0, (int)buffLen);
                        response.Close();
                    }
                    return buff;
                }
                catch (WebException we)
                {
                    HttpWebResponse errorResponse = we.Response as HttpWebResponse;
                    if (errorResponse != null && errorResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates a new blob with given content or changes existing one
        /// </summary>
        /// <param name="body">Content of a new blob </param>
        /// <returns>This blob</returns>
        [SqlMethod()]
        public Blob UploadContent(SqlBinary body)
        {
            if (this.IsNull)
            {
                return null;
            }
            else
            {

                var request = (HttpWebRequest)WebRequest.Create(this.ToString());

                base.AddHeader("x-ms-date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT");
                request.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT");
                base.AddHeader("x-ms-version", Version);
                request.Headers.Add("x-ms-version", Version);
                base.AddHeader("x-ms-blob-type", "BlockBlob");
                request.Headers.Add("x-ms-blob-type", "BlockBlob");
                request.Method = "PUT";

                byte[] postData = (byte[]) body;
                request.ContentLength = postData.Length;

                base.AddHeader("Authorisation", BlobAuthorisation.GetAuthorisationHeader(request, AccountKey));

                request.GetRequestStream().Write(postData, 0, postData.Length);
                var response = request.GetResponse();
                response.Close();
                return this;
            }
        }    

        public override string ToString()
        {
            return base.ToString();
        }

        public static Blob Null
        {
            get
            {
                return RestWebService.GetNullValue<Blob>();
            }
        }

        [SqlMethod(OnNullCall = false)]
        public static Blob Parse(SqlString s)
        {
            return RestWebService.Parse<Blob>(s);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
            w.Write(AccountKey);
            w.Write(Version);
        }

        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);
            AccountKey = r.ReadString();
            Version = r.ReadString();
        }

        public override bool IsEqual(RestWebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Blob blb = (Blob)obj;
            if (this.AccountKey != blb.AccountKey) return false;
            if (this.Version != blb.Version) return false;
            return base.IsEqual(obj);
        }
        #endregion

        #region Blob specific methods

        [SqlMethod(OnNullCall = false)]
        public Blob SetAccountKey(SqlString Key)
        {
            AccountKey = Key.Value;
            return this;
        }


        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public SqlBinary GetWithKey(string accKey)
        {
            AccountKey = accKey;
            return this.Get();          
        }

        #endregion
    }
}
