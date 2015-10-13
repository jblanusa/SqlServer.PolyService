using Microsoft.SqlServer.Server;
using PolyService.Azure;
using PolyService.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;

namespace PolyService.Service
{
    [SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class RestWebService : WebService
    {

        public RestWebService()
        {

        }

        #region Methods exposed via T-SQL surface area.

        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public override string Get()
        {
            return base.SendGetRequest();
        }

        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public SqlChars Post(SqlChars body)
        {
            return new SqlChars(this.SendPostRequest(body.ToSqlString().Value).ToCharArray());
        }

        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public SqlChars Put(SqlChars body)
        {
            return new SqlChars(this.SendPostRequest(body.ToSqlString().Value).ToCharArray());
        }


        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public string Delete()
        {
            return this.SendDeleteRequest(this.GetURL());
        }

        /// <summary>
        /// Add new header value.
        /// </summary>
        /// <param name="property">Name of the HTTP header parameter.</param>
        /// <param name="value">Value of the HTTP header parameter.</param>
        /// <returns>RestWebService instance configured with new header parameter.</returns>
        [SqlMethod(OnNullCall = false)]
        public new RestWebService AddHeader(string property, string value)
        {
            base.AddHeader(property, value);
            return this;
        }

        /// <summary>
        /// Add new URL parameter.
        /// </summary>
        /// <param name="parameter">Name of the parameter.</param>
        /// <param name="value">Value of the  parameter.</param>
        /// <returns>RestWebService instance configured with new parameter.</returns>
        [SqlMethod(OnNullCall = false)]
        public new RestWebService Param(string parameter, string value)
        {
            base.AddRequestParameter(parameter, value);
            return this;
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

        #endregion

        /// <summary>
        /// Send HTTP request using POST method.
        /// </summary>
        /// <returns>Body of HTTP response.</returns>
        protected string SendPostRequest(string body)
        {
            return base.SendHttpRequest(this.GetURL(), "POST", body);
        }

        /// <summary>
        /// Send HTTP request using DELETE method.
        /// </summary>
        /// <returns>Body of HTTP response.</returns>
        public string SendDeleteRequest(string url)
        {
            return base.SendHttpRequest(url, "DELETE");
        }

        /// <summary>
        /// Send HTTP request using PUT method.
        /// </summary>
        /// <returns>Body of HTTP response.</returns>
        public string SendPutRequest(string url, string body)
        {
            return base.SendHttpRequest(url, "PUT", body);
        }


        #region CLR UDT implementation

        [SqlMethod(OnNullCall = false)]
        public static new RestWebService Parse(SqlString s)
        {
            return WebService.ParseLiteral<RestWebService>(s);
        }

        public static new RestWebService Null
        {
            get
            {
                return RestWebService.GetNullValue<RestWebService>();
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion
    }
}
