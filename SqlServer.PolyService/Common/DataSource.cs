using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;

namespace PolyService.Common
{
    [SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class DataSource : INullable, IBinarySerialize
    {
        #region Properties

        /// <summary>
        /// Unified Resource Identifier of data source.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// USername used to access data source.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password used to access data source.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Type of the data source (e.g. DocumentDb, AzureSearch, REST Service).
        /// </summary>
        public string Provider { get; set; }

        #endregion

        [SqlMethod(OnNullCall = false)]
        public static DataSource Parse(SqlString s)
        {
            const string dsToken = "data source=";
            const string unToken = "username=";
            const string pwdToken = "password=";
            const string prvToken = "provider=";

            DataSource res = new DataSource();

            var tokens = s.Value.Split(";".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i+=1)
            {
                if (tokens[i].ToLower().StartsWith(dsToken))
                {
                    res.Uri = tokens[i].Substring(dsToken.Length);
                } else
                if (tokens[i].ToLower().StartsWith(unToken))
                {
                    res.Username = tokens[i].Substring(unToken.Length);
                } else
                if (tokens[i].ToLower().StartsWith(pwdToken))
                {
                    res.Password = tokens[i].Substring(pwdToken.Length);
                } else
                if (tokens[i].ToLower().StartsWith(prvToken))
                {
                    res.Provider = tokens[i].Substring(prvToken.Length);
                }
                else
                {
                    throw new ArgumentException(string.Format("Property '{0}' is not supported", tokens[i]));
                }
            }

            return res;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// NULL value.
        /// </summary>
        public static DataSource Null
        {
            get
            {
                return new DataSource();
            }
        }


        public bool IsNull
        {
            get
            {
                return this.Uri == null;
            }
        }

        /// <summary>
        /// Serialize object.
        /// </summary>
        /// <param name="w"></param>
        public void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Uri);
            w.Write(this.Username??string.Empty);
            w.Write(this.Password??string.Empty);
            w.Write(this.Provider??string.Empty);
        }

        /// <summary>
        /// Deserialize object.
        /// </summary>
        /// <param name="r"></param>
        public void Read(System.IO.BinaryReader r)
        {
            this.Uri = r.ReadString();
            this.Username = r.ReadString();
            this.Password = r.ReadString();
            this.Provider = r.ReadString();
        }
    
    }
}
