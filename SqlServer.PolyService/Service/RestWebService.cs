﻿using Microsoft.SqlServer.Server;
using PolyService.Azure;
using PolyService.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;

namespace PolyService.Service
{
    [SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class RestWebService : INullable, IBinarySerialize
    {
        protected Action<string> SetCookieAction = null;
        /// <summary>
        /// Type of the data source (e.g. DocumentDb, AzureSearch, REST Service).
        /// </summary>
        public string DataSourceType { get; set; }

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

        [SqlMethod(OnNullCall = false)]
        public new RestWebService Param(string parameter, string value)
        {
            this.AddParameter(parameter, value);
            return this;
        }


        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
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
                    try
                    {
                        client.Headers.Add(h.Key, h.Value);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Cannot add header '{0}' with value '{1}'.", h.Key, h.Value),ex);
                    }
                }
                try
                {
                    return client.DownloadString(Uri.EscapeUriString(GetURL()));
                }
                catch (Exception ex)
                {
                    throw new Exception("Operation Failed", ex);
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
                    try
                    {
                        client.Headers.Add(h.Key, h.Value);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Cannot add header '{0}' with value '{1}'.", h.Key, h.Value), ex);
                    }
                }
                try
                {
                    var response = client.UploadString(Uri.EscapeUriString(GetURL()), body);
                    if (this.SetCookieAction != null)
                        this.SetCookieAction(client.ResponseHeaders["Set-Cookie"]);
                    return response;
                }
                catch (Exception ex)
                {
                    throw new Exception("Operation Failed", ex);
                }
            }
        }

        public string Delete(string url)
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
                    return client.UploadString(Uri.EscapeUriString(url), "DELETE", "");
                }
                catch
                {
                    throw new Exception("Operation Failed");
                }
            }
        }


        public string Delete()
        {
            return this.Delete(this.GetURL());
        }

        public RestWebService From(SqlString url)
        {
            if (Uri.IsWellFormedUriString(url.Value, UriKind.Absolute))
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
            else
            {
                var ds = DataSource.Parse(url);
                this.url = new StringBuilder(ds.Uri);
                this.DataSourceType = ds.Provider;

                if (this is IAzureService)
                {
                    IAzureService aws = (IAzureService)this;
                    aws.Key = ds.Username;
                }

                if (this is ILogin)
                {
                    ILogin l = (ILogin)this;
                    l.Username = ds.Username;
                    l.Password = ds.Password;
                }

                return this;
            }
        }        

        public static T ParseLiteral<T>(SqlString s) where T: RestWebService, new()
        {
            T ws = new T();
            ws.From(s);
            return ws;
        }

        [SqlMethod(OnNullCall = false)]
        public static RestWebService Parse(SqlString s)
        {
            return RestWebService.ParseLiteral<RestWebService>(s);
        }

        public static T GetNullValue<T>() where T: RestWebService, new()
        {
            return new T();
        }

        public static RestWebService Null
        {
            get
            {
                return RestWebService.GetNullValue<RestWebService>();
            }
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

        /// <summary>
        /// Generic method that creates service using the datasource.
        /// </summary>
        /// <param name="ds">Datasource that contians Uri, Username and password.</param>
        public static T CreateService<T>(DataSource ds) where T : RestWebService, new()
        {
            T ws = RestWebService.ParseLiteral<T>(new SqlString(ds.Uri));
            if (ws is IAzureService)
            {
                IAzureService aws = (IAzureService)ws;
                aws.Key = ds.Username;
            }

            ws.DataSourceType = ds.Provider;

            return ws;
        }

        /// <summary>
        /// Creates REST service using the datasource.
        /// </summary>
        /// <param name="ds"></param>
        [SqlFunction()]
        public static RestWebService CreateRestService(DataSource ds)
        {
            return RestWebService.CreateService<RestWebService>(ds);
        }
    }
}
