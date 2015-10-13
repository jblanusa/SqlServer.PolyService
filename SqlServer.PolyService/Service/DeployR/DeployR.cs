using Microsoft.SqlServer.Server;
using PolyService.Common;
using System.Data.SqlTypes;
using System.Text;

namespace PolyService.Service
{
    /// <summary>
    /// Work in progress.
    /// </summary>
    [SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class DeployR : RestWebService, ILogin
    {
        public DeployR()
        {
            base.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");
            base.SetCookieAction = delegate( string cookie) {
                /*
                if (cookie == null)
                    return;
                var startIndex = cookie.IndexOf("JSSESSIONID=")+  "JSSESSIONID=".Length;
                this.cookie = cookie.Substring(
                    startIndex: startIndex,
                    length: cookie.IndexOf(";", startIndex)  - startIndex);

                startIndex = 0;
                 * */
            };
        }

        string project = "";
        string cookie = "";
        string inputs = "";
        string robjects = "";

        public string Username { get; set; }
        public string Password { get; set; }

        [SqlMethod(OnNullCall = false)]
        public DeployR Project(SqlString project)
        {
            this.project = project.Value;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public DeployR SetCookie(SqlString cookie)
        {
            this.cookie = cookie.Value;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public DeployR Inputs(SqlString inputs)
        {
            this.inputs = inputs.Value;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public DeployR SelectFields(SqlString fields)
        {
            this.robjects = fields.Value;
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public DeployR RObjects(SqlString robjects)
        {
            return this.SelectFields(robjects);
        }

        /// <summary>
        /// Executed R code block.
        /// <see cref="http://deployr.revolutionanalytics.com/documents/dev/api-doc/guide/single.html#projectexecutecode"/>
        /// </summary>
        /// <returns></returns>
        [SqlMethod(OnNullCall = false)]
        public SqlChars Login()
        {
            var body = new StringBuilder("format=json&");
            body.AppendFormat("username={0}&password={1}", this.Username, this.Password);
            var url = base.url.ToString();
            try
            {
                base.url = new StringBuilder(url.Substring(0, url.IndexOf("/deployr/r/")) + "/deployr/r/user/login");
                return new SqlChars(base.SendPostRequest(body.ToString()).ToCharArray());
            }
            catch
            {
                throw;
            }
            finally
            {
                base.url = new StringBuilder(url);
            }
        }

        /// <summary>
        /// Executed R code block.
        /// <see cref="http://deployr.revolutionanalytics.com/documents/dev/api-doc/guide/single.html#projectexecutecode"/>
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [SqlMethod(OnNullCall = false)]
        public SqlChars ExecuteQuery(SqlString query)
        {
            this.AddHeader("Cookie", "JSESSIONID=" + this.cookie);
            var body = new StringBuilder("format=json&");
            body.AppendFormat("project={0}&code={1}", this.project, System.Uri.EscapeDataString(query.Value));
            if (!string.IsNullOrEmpty(inputs))
            {
                body.AppendFormat("&inputs={0}", this.inputs);
            }
            if (!string.IsNullOrEmpty(robjects))
            {
                body.AppendFormat("&robjects={0}", this.robjects);
            }
            return new SqlChars(base.SendPostRequest(body.ToString()).ToCharArray());
        }

        /// <summary>
        /// Executed R code block.
        /// <see cref="http://deployr.revolutionanalytics.com/documents/dev/api-doc/guide/single.html#projectexecutecode"/>
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        [SqlMethod(OnNullCall = false)]
        public SqlChars ExecuteScript(SqlString script)
        {
            //this.AddHeader("Cookie", "JSESSIONID=2700D5199AA22A680E8DDB6F1B90EB69");
            this.AddHeader("Cookie", "JSESSIONID=" + this.cookie);//radi sa B20B7E8D0AEDB8EE69BF561D6984E63E
            var body = new StringBuilder("format=json&");
            body.AppendFormat("project={0}&filename={1}&author=admin&directory=root", this.project, System.Uri.EscapeDataString(script.Value));
            if (!string.IsNullOrEmpty(inputs))
            {
                body.AppendFormat("&inputs={0}", this.inputs);
            }
            if (!string.IsNullOrEmpty(robjects))
            {
                body.AppendFormat("&robjects={0}", this.robjects);
            }
            return new SqlChars(base.SendPostRequest(body.ToString()).ToCharArray());
        }

        #region Common inherited methods

        [SqlMethod(OnNullCall = false)]
        public new DeployR AddHeader(string property, string value)
        {
            base.AddHeader(property, value);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new DeployR Param(string parameter, string value)
        {
            base.AddRequestParameter(parameter, value);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public new string Get()
        {
            string data = base.SendGetRequest();
            base.ClearRequestParameters();
            return data;
        }

        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public new string Post(string body)
        {
            string data = base.SendPostRequest(body);
            base.ClearRequestParameters();
            return data;
        }



        public override string ToString()
        {
            return base.ToString();
        }

        public static DeployR Null
        {
            get
            {
                return RestWebService.GetNullValue<DeployR>();
            }
        }

        [SqlMethod(OnNullCall = false)]
        public static DeployR Parse(SqlString s)
        {
            return RestWebService.ParseLiteral<DeployR>(s);
        }

        public override string GetURL()
        {
            return base.GetURL();
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.project);
            w.Write(this.cookie);
            w.Write(this.inputs);
            w.Write(this.robjects);
            base.Write(w);
        }

        public override void Read(System.IO.BinaryReader r)
        {
            this.project = r.ReadString();
            this.cookie  = r.ReadString();
            this.inputs = r.ReadString();;
            this.robjects = r.ReadString();;
            base.Read(r);

        }

        public override bool IsEqual(WebService obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            DeployR odt = (DeployR)obj;
            return base.IsEqual(obj);
        }

        #endregion

    }
}
