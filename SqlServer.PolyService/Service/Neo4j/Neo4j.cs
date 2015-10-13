using Microsoft.SqlServer.Server;
using PolyService.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;

namespace PolyService.Service
{
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class Neo4j : RestWebService, IQueryableService, ILogin
    {
        public Neo4j()
        {
            base.AddHeader("Accept", "application/json; charset=UTF-8");
            base.AddHeader("Content-Type", "application/json");
        }

        private string accountKey = "";
        private string username = "";

        private StringBuilder query = new StringBuilder("");
        private int formatCount = 0;
        private int relationCount = 0;
        private Dictionary<int, string> formatArgs = new Dictionary<int, string>();
        private  Dictionary<int, string> relationArgs = new Dictionary<int, string>();

        /// <summary>
        /// Username used to access Neo4j service
        /// </summary>
        public string Password
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

        /// <summary>
        /// Pasword used to access Neo4j service
        /// </summary>
        public string Username
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


        /// <summary>
        /// Test Property
        /// </summary>
        public string queryBody 
        {
            get
            {
                FormatQuery();
                return query.ToString();
            }
        }

        #region Common inherited methods
        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public new string Get()
        {
            return base.SendGetRequest();
        }

        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public string Post(string body)
        {
            
            return base.SendPostRequest(body);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static new Neo4j Null
        {
            get
            {
                return RestWebService.GetNullValue<Neo4j>();
            }
        }

        [SqlMethod(OnNullCall = false)]
        public static new Neo4j Parse(SqlString s)
        {
            return RestWebService.ParseLiteral<Neo4j>(s);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
            w.Write(Password);
            w.Write(Username);
            w.Write(query.ToString());
            w.Write(formatCount);
            w.Write(formatArgs.Count);
            foreach (var entry in formatArgs)
            {
                w.Write(entry.Key);
                w.Write(entry.Value);
            }
            w.Write(relationCount);
            w.Write(relationArgs.Count);
            foreach (var entry in relationArgs)
            {
                w.Write(entry.Key);
                w.Write(entry.Value);
            }
            w.Write(patternInd);
            w.Write((int)lastOp);
        }

        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);
            Password = r.ReadString();
            Username = r.ReadString();
            query = new StringBuilder(r.ReadString());
            formatCount = r.ReadInt32();
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int key = r.ReadInt32();
                string value = r.ReadString();
                formatArgs.Add(key,value);
            }
            relationCount = r.ReadInt32();
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int key = r.ReadInt32();
                string value = r.ReadString();
                relationArgs.Add(key, value);
            }
            patternInd = r.ReadInt32();
            lastOp = (operations)r.ReadInt32();
        }

        private static bool CompareDict(Dictionary<int, string> dict1, Dictionary<int, string> dict2)
        {
            if (dict1 == dict2) return true;
            if ((dict1 == null) || (dict2 == null)) return false;
            if (dict1.Count != dict2.Count) return false;

            var valueComparer = EqualityComparer<string>.Default;

            foreach (var kvp in dict1)
            {
                string value2;
                if (!dict2.TryGetValue(kvp.Key, out value2)) return false;
                if (!valueComparer.Equals(kvp.Value, value2)) return false;
            }
            return true;
        }

        public override bool IsEqual(WebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Neo4j nfj = (Neo4j)obj;
            if (this.Password != nfj.Password) return false;
            if (this.Username != nfj.Username) return false;
            if (!this.query.ToString().Equals(nfj.query.ToString())) return false;
            if (this.lastOp != nfj.lastOp) return false;
            if (this.formatCount != nfj.formatCount) return false;
            if (!Neo4j.CompareDict(formatArgs, nfj.formatArgs)) return false;
            if (this.relationCount != nfj.relationCount) return false;
            if (!Neo4j.CompareDict(relationArgs, nfj.relationArgs)) return false;
            return base.IsEqual(nfj);
        }
        #endregion

        #region Neo4j specific methods

        private SqlChars SendQuery(string query)
        {
            string form = "{{\"statements\" : [ {{\"statement\":\"{0}\"}} ]}}";
            var jsonQuery = string.Format(form, query);
            char[] chars = Post(jsonQuery).ToCharArray();
            return new SqlChars(chars);
        }

        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public SqlChars ExecuteQuery(SqlString query)
        {
            return this.SendQuery(this.stringEscape(query.Value));
        }


        [SqlMethod(OnNullCall = false)]
        public Neo4j SetAccountKey(SqlString aKey)
        {
            Password = aKey.Value;
            return this;
        }


        [SqlMethod(OnNullCall = false)]
        public Neo4j SetUserName(SqlString uname)
        {
            Username = uname.Value;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j SetNamePass(SqlString uname, SqlString akey)
        {
            Username = uname.Value;
            Password = akey.Value;
            return this;
        }
        protected virtual string CreateAuthenticationHeader(string username, string accountKey)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(username + ":" + accountKey);
            return "Basic " + Convert.ToBase64String(byteArray);
        }
        #endregion

        #region Query builder

        private enum operations
        {
            NOP,MATCH, OPTIONALMATCH, PATTERNMATCH, CREATE, RETURN, DELETE, REMOVE, ORDERBY, WITH, LIMIT, SKIP, WHERE, UNION
        }

        private operations lastOp = operations.NOP;

        private void Clause(string name, string field, operations op)
        {
            if (lastOp == op)
            {
                query.Append("," + field);
            }
            else
            {
                query.Append(" "+name + " " + field);
            }
            lastOp = op;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Match(SqlString name)
        {
            if (name.ToString() != "")
            {
                Clause("MATCH", "(" + name.ToString() + formatString + ")", operations.MATCH);
            }
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j OptionalMatch(SqlString name)
        {
            Clause("OPTIONAL MATCH", "(" + name.ToString() + formatString + ")",operations.OPTIONALMATCH);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Creates(SqlString name)
        {
            Clause("CREATE", "(" + name.ToString() + formatString + ")", operations.CREATE);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Returns(SqlString name)
        {
            Clause("RETURN", name.ToString(),operations.RETURN);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Deletes(SqlString name)
        {
            Clause("DELETE", name.ToString(),operations.DELETE);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Removes(SqlString name)
        {
            Clause("REMOVE", name.ToString(),operations.REMOVE);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j OrderBy(SqlString name)
        {
            Clause("ORDER BY", name.ToString(),operations.ORDERBY);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j WithVar(SqlString name)
        {
            Clause("WITH", name.ToString(),operations.WITH);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Limit(SqlInt32 lim)
        {
            query.Append(" LIMIT " + lim);
            lastOp = operations.LIMIT;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Skips(SqlInt32 sk)
        {
            query.Append(" SKIP " + sk);
            lastOp = operations.SKIP;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Condition(SqlString name)
        {
            query.Append(" WHERE " + name.ToString());
            lastOp = operations.WHERE;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Unions()
        {
            query.Append(" UNION ");
            lastOp = operations.UNION;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j UnionAll()
        {
            query.Append(" UNION ALL ");
            lastOp = operations.UNION;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j InnerJoin(SqlString name)
        {
            query.Append("-" + relationString + "-(" + name.ToString() + formatString + ")");
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j JoinFrom(SqlString name)
        {
            query.Append("<-" + relationString + "-(" + name.ToString() + formatString + ")");
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j JoinTo(SqlString name)
        {
            query.Append("-" + relationString + "->(" + name.ToString() + formatString + ")");
            return this;
        }

        public Neo4j OnRelation(SqlString name)
        {
            relationArgs.Add(relationCount - 1, "[" + name.ToString() + formatString + "]");
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Filter(SqlString name)
        {
            AddFormatArgument(formatCount - 1, name.ToString());
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j WithProperty(SqlString key,SqlString value)
        {
            AddFormatArgument(formatCount - 1, key.ToString() + ":\'" + stringEscape(value.ToString())+"\'");
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public SqlChars Run()
        {
            FormatQuery();
            SqlChars ret = this.SendQuery(query.ToString());
            ResetQuery();
            return ret;
        }

        #region pattern builder

        private int patternInd = -1;

        /// <summary>
        /// Format of a pattern is (@m) <-- () -[@r]-> (@n)
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        [SqlMethod(OnNullCall = false)]
        public Neo4j MatchPattern(SqlString pat)
        {
            patternInd = query.Length;
            Clause("MATCH",pat.ToString(),operations.MATCH);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Node(SqlString node)
        {
            query.Replace("@@type", "",patternInd,query.Length-patternInd-1);
            query.Replace("@" + node.ToString(), node.ToString() + "@@type" + formatString, patternInd, query.Length - patternInd-1);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j Relationship(SqlString rel)
        {
            query.Replace("@@type", "", patternInd, query.Length - patternInd-1);
            query.Replace("@" + rel.ToString(), rel.ToString() + "@@type" + formatString, patternInd, query.Length - patternInd-1);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public Neo4j AsType(SqlString type)
        {
            query.Replace("@@type", ":" + type.ToString(), patternInd, query.Length - patternInd-1);
            return this;
        }
        #endregion

        //private
        private void ResetQuery()
        {
            query.Clear();
            lastOp = operations.NOP;
            formatCount = 0;
            formatArgs.Clear();
            relationCount = 0;
            relationArgs.Clear();
        }

        private string formatString
        {
            get { return "{" + formatCount++ + "}"; }
        }

        private string relationString
        {
            get { return "[" + relationCount++ + "]"; }
        }

        public void FormatQuery()
        {
            for (int i = 0; i < relationCount; i++)
            {
                string val = "";
                if (relationArgs.TryGetValue(i, out val))
                {
                    query.Replace("[" + i + "]", val);
                }
                else
                {
                    query.Replace("[" + i + "]", "");
                }
            }
            for (int i = 0; i < formatCount; i++)
            {
                string val = "";
                if (formatArgs.TryGetValue(i, out val))
                {
                    query.Replace("{" + i + "}", "{" + val + "}");
                }
                else
                {
                    query.Replace("{" + i + "}", "");
                }
            }
            query.Replace("@@type", "");
        }

        private string stringEscape(string s)
        {
            return s.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\\", "\\\\");
        }

        private void AddFormatArgument(int key, string value)
        {
            if (this.formatArgs.ContainsKey(key))
            {
                this.formatArgs[key] += ", " + value;
            }
            else
            {
                this.formatArgs.Add(key, value);
            }
        }
        #endregion
    }
}
