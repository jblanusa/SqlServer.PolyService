using Microsoft.SqlServer.Server;
using PolyService.Azure;
using PolyService.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net;
using System.Text;

namespace PolyService.Service
{
    /// <summary>
    /// Represents generic web service that can return informaiton using GET HTTP method.
    /// </summary>
    [SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class WebService : INullable, IBinarySerialize
    {
        /// <summary>
        /// Action that will be called when cookies are set in response.
        /// </summary>
        protected Action<string> SetCookieAction = null;

        /// <summary>
        /// Type of the data source (e.g. DocumentDB, AzureSearch, REST Service).
        /// </summary>
        public string DataSourceType { get; set; }

        public WebService()
        {

        }

        protected StringBuilder url = null;
        /// <summary>
        /// Collection of HTTP header parameters.
        /// </summary>
        protected Dictionary<string, string> header = new Dictionary<string, string>();
        /// <summary>
        /// Collection of URL parameters that will not be changed between two web service calls.
        /// </summary>
        protected Dictionary<string, string> urlParameters = new Dictionary<string, string>();
        /// <summary>
        /// Collection of URL parameters that will be cleaned after request execution.
        /// </summary>
        protected Dictionary<string, string> requestParameters = new Dictionary<string, string>();
 
        /// <summary>
        /// Returns set of URL parameters (both persistent/url and transient/request parameters)
        /// </summary>
        private string parameterString
        { 
            get
            {
                StringBuilder param = null;
                foreach (var par in urlParameters)
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

                foreach (var par in requestParameters)
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

        /// <summary>
        /// Checks is there at least one parameter in URL.
        /// </summary>
        protected bool HasParameters
        {
            get
            {
                return ((this.urlParameters.Count != 0) || (this.requestParameters.Count != 0));
            }
        }

        /// <summary>
        /// Adds a new request parameter.
        /// </summary>
        /// <param name="parameter">Name of parameter</param>
        /// <param name="value">Value of parameter.</param>
        protected virtual void AddRequestParameter(string parameter, string value)
        {
            if (this.requestParameters.ContainsKey(parameter))
            {
                this.requestParameters[parameter] = value;
            } 
            else 
            {
                this.requestParameters.Add(parameter, value);
            }
        }

        /// <summary>
        /// Deletes all request parameters.
        /// </summary>
        protected virtual void ClearRequestParameters()
        {
            requestParameters.Clear();
        }

        /// <summary>
        /// Add persistent URL parameter.
        /// </summary>
        /// <param name="parameter">Name of parameter</param>
        /// <param name="value">Value of parameter.</param>
        protected virtual void AddUrlParameter(string parameter, string value)
        {
            if (this.urlParameters.ContainsKey(parameter))
            {
                this.urlParameters[parameter] = value;
            }
            else
            {
                this.urlParameters.Add(parameter, value);
            }
        }

        /// <summary>
        /// Add new HTTP header.
        /// </summary>
        /// <param name="property">Name of the header parameter.</param>
        /// <param name="value">Value of header parameter.</param>
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

        /// <summary>
        /// Authorize request.
        /// </summary>
        protected virtual void Authorize() { }

        #region Methods exposed via T-SQL

        /// <summary>
        /// Add parameter in URL
        /// </summary>
        /// <param name="parameter">Name of parameter</param>
        /// <param name="value">Value of parameter.</param>
        /// <returns>WebService instance.</returns>
        [SqlMethod(OnNullCall = false)]
        public WebService Param(string parameter, string value)
        {
            this.AddRequestParameter(parameter, value);
            return this;
        }

        /// <summary>
        /// Get response from web service URL.
        /// </summary>
        /// <returns>Response body.</returns>
        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public virtual string Get()
        {
            return this.SendGetRequest();
        }

        /// <summary>
        /// Creates web service using the datasource.
        /// </summary>
        /// <param name="ds">Data source that contains definition of web service.</param>
        [SqlFunction()]
        public static WebService CreateWebService(DataSource ds)
        {
            return WebService.CreateService<WebService>(ds);
        }

        #endregion 

        /// <summary>
        /// Send HTTP request using GET method.
        /// </summary>
        /// <returns>Body of HTTP response.</returns>
        protected virtual string SendGetRequest()
        {
            return this.SendHttpRequest(this.GetURL(), "GET");
        }

        /// <summary>
        /// Generic function that sends HTTP request.
        /// </summary>
        /// <param name="uri">URL of web service.</param>
        /// <param name="method">HTTP method (e.g. GET, POST, DELETE).</param>
        /// <param name="body">Body of HTTP requests.</param>
        /// <returns>Response that is returned by web service.</returns>
        protected virtual string SendHttpRequest(string uri, string method, string body = null)
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
                    switch (method.ToLower())
                    {
                        case "get":
                            return client.DownloadString(Uri.EscapeUriString(uri));
                        case "post":
                            var response = client.UploadString(Uri.EscapeUriString(uri), body);
                            if (this.SetCookieAction != null)
                                this.SetCookieAction(client.ResponseHeaders["Set-Cookie"]);
                            return response;
                        case "put":
                            var putResponse = client.UploadString(Uri.EscapeUriString(uri), "PUT", body);
                            if (this.SetCookieAction != null)
                                this.SetCookieAction(client.ResponseHeaders["Set-Cookie"]);
                            return putResponse;
                        case "delete":
                            return client.UploadString(Uri.EscapeUriString(uri), "DELETE", body??String.Empty);
                        default:
                            throw new Exception("Invalid Http method - " + method);
                    }
                    
                }
                catch (Exception ex)
                {
                    throw new Exception("Operation Failed", ex);
                }
            }
        }

        /// <summary>
        /// Initialize web service instance using URL or "connection string".
        /// </summary>
        /// <param name="url">Definition of web service (e.g. URL).</param>
        /// <returns>Initialized WebService UTD.</returns>
        public WebService From(SqlString url)
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
                    this.AddUrlParameter(pair[0], pair[1]);
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

        public static T ParseLiteral<T>(SqlString s) where T: WebService, new()
        {
            T ws = new T();
            ws.From(s);
            return ws;
        }

        /// <summary>
        /// Generic method that creates service using the datasource.
        /// </summary>
        /// <param name="ds">Datasource that contians Uri, Username and password.</param>
        public static T CreateService<T>(DataSource ds) where T : WebService, new()
        {
            T ws = WebService.ParseLiteral<T>(new SqlString(ds.Uri));
            if (ws is IAzureService)
            {
                IAzureService aws = (IAzureService)ws;
                aws.Key = ds.Username;
            }

            ws.DataSourceType = ds.Provider;

            return ws;
        }

        #region Common SQL UTD methods.

        [SqlMethod(OnNullCall = false)]
        public static WebService Parse(SqlString s)
        {
            return WebService.ParseLiteral<WebService>(s);
        }

        public static T GetNullValue<T>() where T: WebService, new()
        {
            return new T();
        }

        public static WebService Null
        {
            get
            {
                return WebService.GetNullValue<WebService>();
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
                this.AddUrlParameter(key, value);
            }
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = r.ReadString();
                string value = r.ReadString();
                this.AddRequestParameter(key, value);
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
            w.Write(urlParameters.Count);
            foreach (var entry in urlParameters)
            {
                w.Write(entry.Key);
                w.Write(entry.Value);
            }
            w.Write(requestParameters.Count);
            foreach (var entry in requestParameters)
            {
                w.Write(entry.Key);
                w.Write(entry.Value);
            }
        }

        #endregion

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

        public virtual bool IsEqual(WebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            if (obj == null) return false;
            if (this.url.ToString() != obj.url.ToString()) return false;
            if (!WebService.CompareDict(this.header, obj.header)) return false;
            if (!WebService.CompareDict(this.urlParameters, obj.urlParameters)) return false;
            if (!WebService.CompareDict(this.requestParameters, obj.requestParameters)) return false;
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
