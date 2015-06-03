using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net;
using System.Text;

namespace SqlServer.PolyService
{
    public class RestWebService : INullable, IBinarySerialize
    {
        public RestWebService()
        {

        }

        protected StringBuilder url = null;
        protected Dictionary<string, string> header = new Dictionary<string, string>();
        protected Dictionary<string, string> persistantParameters = new Dictionary<string, string>(); 
        protected Dictionary<string, string> tempParameters = new Dictionary<string, string>();
 
        private string parameterString
        { 
        get
        {
            StringBuilder param = null;
            foreach (var par in persistantParameters)
            {
                if (param == null)
                {
                    param = new StringBuilder(par.Key + "=" + par.Value);
                }
                else
                {
                    param.AppendFormat("&{0}={1}", par.Key, par.Value);
                }
            }

            foreach (var par in tempParameters)
            {
                if (param == null)
                {
                    param = new StringBuilder(par.Key + "=" + par.Value);
                }
                else
                {
                    param.AppendFormat("&{0}={1}", par.Key, par.Value);
                }
            }
            return param.ToString();
        }
        }

        protected bool HasParameters
        {
            get
            {
                return ((this.persistantParameters.Count != 0) || (this.tempParameters.Count != 0));
            }
        }

        protected virtual void AddParameter(string parameter, string value)
        {
            if (this.tempParameters.ContainsKey(parameter))
            {
                this.tempParameters[parameter] = value;
            } 
            else 
            {
                this.tempParameters.Add(parameter, value);
            }
        }

        protected virtual void ClearParameters()
        {
            tempParameters.Clear();
        }

        protected virtual void AddPersistantParameter(string parameter, string value)
        {
            if (this.persistantParameters.ContainsKey(parameter))
            {
                this.persistantParameters[parameter] = value;
            }
            else
            {
                this.persistantParameters.Add(parameter, value);
            }
        }

        public virtual void AddHeader(string property, string value)
        {
            if (this.header.ContainsKey(property))
            {
                this.header[property] = value;
            }
            else
            {
                this.header.Add(property, value);
            }
        }

        public string Get()
        {
            if (this.IsNull)
            {
                return null;
            }
            else
            {
                var client = new System.Net.WebClient();
                foreach (var h in this.header) {
                    client.Headers.Add(h.Key, h.Value);
                }
                try
                {
                return client.DownloadString(Uri.EscapeUriString(GetURL()));
                }
                catch
                {
                    throw new Exception("Operation Failed");
                }
            }
        }

        public string Post(string body)
        {
            if (this.IsNull)

            {
                return null;
            }
            else
            {
                var client = new System.Net.WebClient();
                foreach(var h in this.header) {
                    client.Headers.Add(h.Key, h.Value);
                }
                try
                {
                return client.UploadString(Uri.EscapeUriString(GetURL()), body);
               }
                catch
                {
                    throw new Exception("Operation Failed");
                }
            }
        }

        public string Delete()
        {
            if (this.IsNull)
            {
                return null;
            }
            else
            {
                var client = new System.Net.WebClient();
                foreach (var h in this.header)
                {
                    client.Headers.Add(h.Key, h.Value);
                }
                try  
                {
                    return client.UploadString(Uri.EscapeUriString(GetURL()), "DELETE", "");
                }
                catch
                {
                    throw new Exception("Operation Failed");
                }
            }
        }

        public RestWebService From(SqlString url)
        {
            string urlVal = url.Value;
            string[] urlSplit = urlVal.Split('?');
            this.url = new StringBuilder(urlSplit[0]);
            if (urlSplit.Length == 1) return this;
            string[] param = urlSplit[1].Split('&');
            foreach (string s in param)
            {
                if (s.Length == 0) continue;
                string[] pair = s.Split('=');
                if (pair.Length != 2) continue;
                this.AddPersistantParameter(pair[0], pair[1]);
            }
            return this;
        }        

        public static T Parse<T>(SqlString s) where T: RestWebService, new()
        {
            T ws = new T();
            ws.From(s);
            return ws;
        }

        public static T GetNullValue<T>() where T: RestWebService, new()
        {
            return new T();
        }

        public bool IsNull
        {
            get
            {
                return this.url == null;
            }
        }

        public virtual void Read(System.IO.BinaryReader r)
        {
            this.url = new StringBuilder(r.ReadString());
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = r.ReadString();
                string value = r.ReadString();
                this.AddHeader(key,value);
            }
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = r.ReadString();
                string value = r.ReadString();
                this.AddPersistantParameter(key, value);
            }
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = r.ReadString();
                string value = r.ReadString();
                this.AddParameter(key, value);
            }
        }

        public virtual void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.url.ToString());
            w.Write(header.Count);
            foreach (var entry in header)
            {
                w.Write(entry.Key);
                w.Write(entry.Value);
            }
            w.Write(persistantParameters.Count);
            foreach (var entry in persistantParameters)
            {
                w.Write(entry.Key);
                w.Write(entry.Value);
            }
            w.Write(tempParameters.Count);
            foreach (var entry in tempParameters)
            {
                w.Write(entry.Key);
                w.Write(entry.Value);
            }
        }

        private static bool CompareDict(Dictionary<string, string> dict1, Dictionary<string, string> dict2)
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

        public virtual bool IsEqual(RestWebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (obj == null) return false;
            if (this.url.ToString() != obj.url.ToString()) return false;
            if (!RestWebService.CompareDict(this.header, obj.header)) return false;
            if (!RestWebService.CompareDict(this.persistantParameters, obj.persistantParameters)) return false;
            if (!RestWebService.CompareDict(this.tempParameters, obj.tempParameters)) return false;
            return true;
        }

        public override string ToString()
        {
            if (this.IsNull)
                return "NULL";
            else return GetURL();
            
        }

        public virtual string GetURL()
        {
            return this.url.ToString() +
                    (this.HasParameters ?
                    "?" + this.parameterString : "");
        }

    }
}
