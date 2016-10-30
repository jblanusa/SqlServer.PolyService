using Microsoft.SqlServer.Server;
using PolyService.Service;
using System.Data.SqlTypes;
using System.Text;

namespace PolyService.Azure
{
    /// <summary>
    /// User defined type that represents Text Analytics app by Azure Machine Learning
    /// </summary>
    [Microsoft.SqlServer.Server.SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1)]
    public class TextAnalytics : AzureMLApp
    {
        public TextAnalytics()
        {
            url = new StringBuilder("https://api.datamarket.azure.com/data.ashx/amla/text-analytics/v1");
        }

        /// <summary>
        /// The API returns a numeric score between 0 & 1. Scores close to 1 indicate positive sentiment, while scores 
        /// close to 0 indicate negative sentiment. Sentiment score is generated using classification techniques
        /// </summary>
        /// <param name="body">Text to be analyzed</param>
        /// <returns>Numeric value between 0 & 1</returns>
        [SqlMethod(OnNullCall = false)]
        public float GetSentiment(string body)
        {
            StringBuilder text = new StringBuilder(body);   
            text.Replace(' ', '+');
            this.url.Append("/GetSentiment");
            base.AddRequestParameter("Text",text.ToString());
            string result =  base.Get();
            base.ClearRequestParameters();
            this.url.Replace("/GetSentiment", "");
            string[] split = result.Split(':');
            return float.Parse(split[3].Replace("}",""));
        }

        /// <summary>
        /// The API returns a list of strings denoting the key talking points in the input text. 
        /// </summary>
        /// <param name="body">Text to be analyzed</param>
        /// <returns>List of key phrases</returns>
        [SqlMethod(OnNullCall = false)]
        [return: SqlFacet(MaxSize = -1)]
        public string GetKeyPhrases(string body)
        {
            StringBuilder text = new StringBuilder(body);
            text.Replace(' ', '+');
            this.url.Append("/GetKeyPhrases");
            base.AddRequestParameter("Text", text.ToString());
            string result = base.Get();
            base.ClearRequestParameters();
            this.url.Replace("/GetKeyPhrases", "");
            string[] split = result.Split(':');
            return split[3].Replace("[", "").Replace("]","").Replace("}","").Trim();
        }

        #region Common inherited methods
        /// <summary>
        /// Sets account key for this app
        /// </summary>
        /// <param name="aKey">Account Key</param>
        /// <returns>This object</returns>
        [SqlMethod(OnNullCall = false)]
        public new TextAnalytics SetAccountKey(SqlString aKey)
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
        public new TextAnalytics SetUserName(SqlString uname)
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
        public new TextAnalytics SetNamePass(SqlString uname, SqlString akey)
        {
            base.SetNamePass(uname, akey);
            return this;
        }

        [SqlMethod(OnNullCall = false)]
        public new static TextAnalytics Parse(SqlString s)
        {

            return new TextAnalytics();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static TextAnalytics Null
        {
            get
            {
                return RestWebService.GetNullValue<TextAnalytics>();
            }
        }
        #endregion
    }
}
