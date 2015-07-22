using Microsoft.SqlServer.Server;
using PolyService.Service;
using System.Data.SqlTypes;
using System.Text;

namespace PolyService.Azure
{
    /// <summary>
    /// User defined type that represents Binary Classifier app by Azure Machine Learning
    /// </summary>
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class BinaryClassifier : AzureMLApp
    {
        public BinaryClassifier()
        {
            base.url = new StringBuilder("https://api.datamarket.azure.com/aml_labs/log_regression/v1/Score");
        }

        public string Url
        {
            get
            {
                return base.GetURL();
            }

        }

        public static BinaryClassifier Null
        {
            get
            {
                return RestWebService.GetNullValue<BinaryClassifier>();
            }
        }

        /// <summary>
        /// Uploading content to Azure Machine Learning
        /// </summary>
        /// <param name="body">json formatted string</param>
        /// <returns>web response as json string</returns>
        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public string Classify(string body)
        {
            return base.Post("{\"value\":\"" + body + "\"}");
        }

        #region Common inherited methods
        /// <summary>
        /// Sets account key for this app
        /// </summary>
        /// <param name="aKey">Account Key</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public BinaryClassifier SetAccountKey(SqlString aKey)
        {
            base.SetAccountKey(aKey);
            return this;
        }

        /// <summary>
        /// Sets user name for this app
        /// </summary>
        /// <param name="uname">User Name</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public BinaryClassifier SetUserName(SqlString uname)
        {
            base.SetUserName(uname);
            return this;
        }

        /// <summary>
        /// Sets both user name and account key for this app
        /// </summary>
        /// <param name="uname">User Name</param>
        /// <param name="akey">Account Key</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public BinaryClassifier SetNamePass(SqlString uname, SqlString akey)
        {
            base.SetNamePass(uname, akey);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public static BinaryClassifier Parse(SqlString s)
        {

            return new BinaryClassifier();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion
    }
}
