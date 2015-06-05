﻿using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;
using System.IO;
using System.Text;

namespace SqlServer.PolyService
{
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class TableStorage : OData, IAzureService
    {
        
        public TableStorage()
        {
            AccountKey = "";
            Version = "2014-02-14";
        }


        #region Properties

        private string AccountKey;
        private string versionString;

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
        public string Url
        {
            get
            {
                return url.ToString();
            }
        }


        #endregion

        #region Common inherited methods


        [SqlMethod()]
        [return: SqlFacet(MaxSize = -1)]
        public new string Get()
        {
            string dateString = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT";
            base.AddHeader("x-ms-version",Version);
            base.AddHeader("x-ms-date", dateString);
            AddAuthorization("GET");
            string ret = base.Get();
            clearHeaders();
            return ret;
        }

        [SqlMethod()]
        public new string Post(string body)
        {
            base.AddHeader("x-ms-version", Version);
            AddDateHeader();
            AddAuthorization("POST");
            string ret =  base.Post(body);
            clearHeaders();
            return ret;
        }

        [SqlMethod()]
        public new string Delete()
        {
            base.AddHeader("x-ms-version", Version);
            AddDateHeader();
            AddAuthorization("DELETE");
            string ret = base.Delete();
            clearHeaders();
            return ret;
        }    

        public override string ToString()
        {
            return base.ToString();
        }

        public static TableStorage Null
        {
            get
            {
                return RestWebService.GetNullValue<TableStorage>();
            }
        }

        [SqlMethod(OnNullCall = false)]
        public static TableStorage Parse(SqlString s)
        {
            return RestWebService.Parse<TableStorage>(s);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
            w.Write(AccountKey);
            w.Write(Version);
            w.Write(insertBody.ToString());
            w.Write(RowKeyAdded);
            w.Write(PartitionKeyAdded);
        }

        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);
            AccountKey = r.ReadString();
            Version = r.ReadString();
            insertBody = new StringBuilder(r.ReadString());
            RowKeyAdded = r.ReadBoolean();
            PartitionKeyAdded = r.ReadBoolean();
        }

        public override bool IsEqual(RestWebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            TableStorage tbl = (TableStorage)obj;
            if (this.AccountKey != tbl.AccountKey) return false;
            if (this.Version != tbl.Version) return false;
            if (this.PartitionKeyAdded != tbl.PartitionKeyAdded) return false;
            if (this.RowKeyAdded != RowKeyAdded) return false;
            if (this.insertBody.ToString() != tbl.insertBody.ToString()) return false;
            return base.IsEqual(obj);
        }
        #endregion

        #region Table storage specific methods

        [SqlMethod(OnNullCall = false)]
        public TableStorage SetAccountKey(SqlString Key)
        {
            AccountKey = Key.Value;
            return this;
        }

        /// <summary>
        /// Creates table with given name
        /// </summary>
        /// <param name="name">Table name</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public TableStorage CreateTable(SqlString name)
        {
            base.AddHeader("Content-Type", "application/json");
            base.AddHeader("Accept", "application/json;odata=nometadata");
            string body = String.Format("{{ \"TableName\":\"{0}\" }}", name.ToString());


            string oldPath = urlPath;
            string newPath = "/Tables";

            swapPaths(oldPath, newPath);
            string ret = Post(body);
            swapPaths(newPath, oldPath);
            return this;

        }

        /// <summary>
        /// Returns list of all tables for the given account
        /// </summary>
        /// <returns>Json formatted list of tables</returns>
        [SqlMethod(OnNullCall = false)]
        public SqlString ListTables()
        {
            base.AddHeader("Accept", "application/json;odata=nometadata");

            string oldPath = urlPath;
            string newPath = "/Tables";

            swapPaths(oldPath, newPath);
            string ret = Get();
            swapPaths(newPath, oldPath);
            return ret;
        }

        /// <summary>
        /// Deletes table with the given name
        /// </summary>
        /// <param name="name">Table name</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public TableStorage DeleteTable(SqlString name)
        {
            base.AddHeader("Content-Type", "application/atom+xml");


            string oldPath = urlPath;
            string newPath = String.Format("/Tables('{0}')",name.ToString());

            swapPaths(oldPath, newPath);
            string ret = Delete();
            swapPaths(newPath, oldPath);
            return this;

        }

        /// <summary>
        /// Querying table storage should look like:
        /// 
        /// 				Table.Returns('a, b, c')
        ///                      .Take(10)
        ///                      .Filter('a ge 20')
		///					     .FromTable('TestTable')
		///					     .Get()
        /// 
        /// Where Table is TableStorage object
        /// 
        /// This method specifies which table is being queried
        /// </summary>
        /// <param name="name">Table name</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public TableStorage FromTable(SqlString name)
        {
            base.AddHeader("Accept", "application/json;odata=nometadata");

            string oldPath = urlPath;
            string newPath = String.Format("/{0}()",name.ToString());
            swapPaths(oldPath, newPath);
            return this;
        }


        private StringBuilder insertBody = new StringBuilder("{");
        private Boolean RowKeyAdded = false;
        private Boolean PartitionKeyAdded = false;

        /// <summary>
        /// Inserting into table storage should look like:
        /// 
        ///                 Table.Value('PartitionKey','"first_partition"')
        ///                      .KeyValue('45')
        ///                      .Value('IntegerProperty','7')
        ///                      .Value('StringProperty','"value"')
        ///                      .InsertInto('TestTable')
        /// 
        /// Where Table is TableStorage object
        /// 
        /// This method specifies the table in which we insert values and it perform insert.
        /// </summary>
        /// <param name="name">Table name</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public TableStorage InsertInto(SqlString name)
        {
            base.AddHeader("Accept", "application/json;odata=nometadata");
            base.AddHeader("Content-Type", "application/json");

            if (!PartitionKeyAdded)
            {
                Value("PartitionKey", name.ToString());
            }
            if (!RowKeyAdded)
            {
                KeyValue(DateTime.UtcNow.ToString("R"));
            }

            insertBody.Append("}");
            string body = insertBody.ToString();
            insertBody.Clear().Append("{");

            string oldPath = urlPath;
            string newPath = String.Format("/{0}", name.ToString());

            swapPaths(oldPath, newPath);
            Post(body);
            swapPaths(newPath, oldPath);

            RowKeyAdded = false;
            PartitionKeyAdded = false;

            return this;
        }

        /// <summary>
        /// Adds one more value to current entity that is going to be inserted
        /// with InsertInto method into table for current account.
        /// </summary>
        /// <param name="property">Name of a property</param>
        /// <param name="value">Value of the property, string type</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public TableStorage Value(SqlString property, SqlString value)
        {
            if (property == "PartitionKey" && PartitionKeyAdded)
            {
                return this;
            }
            else if (property == "PartitionKey")
            {
                PartitionKeyAdded = true;
            }
            if (insertBody.Length != 1)
            {
                insertBody.Append(",");
            }    
            insertBody.Append("\"" + property + "\":\"" + value+"\"");
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public TableStorage IntValue(SqlString property, SqlInt32 value)
        {
            if (property == "PartitionKey")
            {
                return this;
            }
            if (insertBody.Length != 1)
            {
                insertBody.Append(",");
            }
            insertBody.Append("\"" + property + "\":" + value.ToString());
            return this;
        }

        /// <summary>
        /// Adds value for "RowKey" property which is necessary for every entity
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [SqlMethod(OnNullCall = false)]
        public TableStorage KeyValue(SqlString value)
        {
            if (RowKeyAdded) return this;
            RowKeyAdded = true;
            if (insertBody.Length != 1)
            {
                insertBody.Append(",");
            }
            insertBody.Append("\"RowKey\":\"" + value+"\"");
            return this;
        }
        /// <summary>
        /// Method that prepare UDT for batch insert
        /// </summary>
        /// <param name="body">Json formatted batch</param>
        /// <param name="batchName">batch name</param>
        /// <returns></returns>
        [SqlMethod(OnNullCall = false)]
        public TableStorage BatchInsert(SqlString body, SqlString batchName)
        {
            base.AddHeader("Accept-Charset", "UTF-8");
            base.AddHeader("DataServiceVersion", "3.0;");
            base.AddHeader("MaxDataServiceVersion", "3.0;NetFx");
            base.AddHeader("Content-Type", "multipart/mixed; boundary="+batchName.ToString());

            string oldPath = urlPath;
            string newPath = "/$batch";

            swapPaths(oldPath, newPath);
            Post(body.ToString());
            swapPaths(newPath, oldPath);
            return this;
        }

        /// <summary>
        /// Checks whether table with given name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true or false value</returns>
        [SqlMethod(OnNullCall = false)]
        public SqlBoolean ExistsTable(SqlString name)
        {
            string lst = ListTables().ToString();
            return lst.Contains("\"TableName\":+\"" + name.ToString() + "\"");
        }
        #region Authorization
        private void AddAuthorization(string Method)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Method + "\n");

            if (this.header.ContainsKey("Content-MD5")) 
                sb.Append(header["Content-MD5"] + "\n");
            else 
                sb.Append("\n");

            if (this.header.ContainsKey("Content-Type"))
                sb.Append(header["Content-Type"] + "\n");
            else
                sb.Append("\n");

            sb.Append(header["x-ms-date"] + "\n");

            sb.Append(GenerateCanonicalizedResource());

            string signature = SignTheStringToSign(sb.ToString(),AccountKey);
            string AuthorizationHeader = "SharedKey " + GetAccountNameFromUri(new Uri(Url)) + ":" + signature;
            base.AddHeader("Authorization", AuthorizationHeader);
        }

        private string GetAccountNameFromUri(Uri uri)
        {
            int idx = uri.Host.IndexOf('.');
            if (idx == -1)
            {
                idx = uri.PathAndQuery.IndexOf('/') + 1;
                int iEnd = uri.PathAndQuery.IndexOfAny(new char[] { '?', '/' }, idx);
                if (iEnd == -1)
                    iEnd = uri.PathAndQuery.Length;
                return uri.PathAndQuery.Substring(idx, iEnd - idx);
            }
            else
                return uri.Host.Substring(0, idx);
        }

        private string GenerateCanonicalizedResource()
        {
            StringBuilder sb = new StringBuilder("/");
            Uri uri = new Uri(GetURL());
            sb.Append(GetAccountNameFromUri(uri));
			string path = uri.AbsolutePath;
			if(path == "") sb.Append("/");
			else sb.Append(path);

            if (this.persistantParameters.ContainsKey("comp"))
                sb.Append("?comp=" + this.persistantParameters["comp"]);

            return sb.ToString();
        }

        private static string SignTheStringToSign(string stringToSign, string sharedKey)
        {
            byte[] SignatureBytes = System.Text.Encoding.UTF8.GetBytes(stringToSign);
            System.Security.Cryptography.HMACSHA256 SHA256 = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(sharedKey));

            return Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes));
        }
        #endregion

        private void AddDateHeader()
        {
            string dateString = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT";
            base.AddHeader("x-ms-date", dateString);
        }

        private void clearHeaders()
        {
            base.header.Clear();
            base.AddHeader("x-ms-version", versionString);
        }

        private string urlPath
        {
            get
            {
                Uri myUri = new Uri(url.ToString());
                return myUri.AbsolutePath;
            }
        }

        private void swapPaths(string oldPath, string newPath)
        {
            if (oldPath != newPath)
            {
                url.Replace(oldPath, "", url.Length - oldPath.Length, oldPath.Length);
                url.Append(newPath);
            }
        }
        #endregion


        #region OData query parameters
        [SqlMethod(OnNullCall = false)]
        public TableStorage Filter(SqlString condition)
        {
            base.Filter(condition);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public TableStorage Take(SqlInt64 number)
        {
            base.Take(number);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public TableStorage Returns(SqlString properties)
        {
            base.Returns(properties);
            return this;
        }
        #endregion
    }
}
