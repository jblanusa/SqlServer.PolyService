using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServer.PolyService
{
    //Modified from https://sqlservertoazure.codeplex.com/
    public class BlobAuthorisation
    {
        public static string GetAuthorisationHeader(System.Net.HttpWebRequest req, string sharedKey)
        {
            String AuthorizationHeader = "SharedKey " + GetAccountNameFromUri(req.RequestUri) + ":" + GenerateAzureSignatureFromSharedKey(req, sharedKey);

            req.Headers.Add("Authorization", AuthorizationHeader);

            return AuthorizationHeader;
        }

        public static string SignTheStringToSign(string stringToSign, string sharedKey)
        {
            byte[] SignatureBytes = System.Text.Encoding.UTF8.GetBytes(stringToSign);
            System.Security.Cryptography.HMACSHA256 SHA256 = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(sharedKey));

            return Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes));
        }

        public static string GenerateAzureSignatureFromSharedKey(System.Net.HttpWebRequest req, string sharedKey)
        {
            StringBuilder sb = new StringBuilder();

            string CanonicalizedResource = GenerateCanonicalizedResource(req.RequestUri);
            string CanonicalizedHeader = GenerateCanonicalizedHeader(req);


            #region Method (aka VERB)
            sb.Append(req.Method + "\n");
            #endregion

            #region Content-Encoding
            if (req.Headers.AllKeys.FirstOrDefault(item => item == "Content-Encoding") != null)
                sb.Append(req.Headers["Content-Encoding"] + "\n");
            else
                sb.Append("\n");
            #endregion

            #region Content-Language
            if (req.Headers.AllKeys.FirstOrDefault(item => item == "Content-Language") != null)
                sb.Append(req.Headers["Content-Language"] + "\n");
            else
                sb.Append("\n");
            #endregion

            #region Content-Length
            if (((req.Method == "GET") || (req.Method == "HEAD")) && (req.ContentLength == 0))
                sb.Append("\n");

            else if ((req.Method == "DELETE") && (req.ContentLength < 0))
                sb.Append("\n");
            else
                sb.Append(req.ContentLength + "\n");
            #endregion

            #region Content-MD5
            if (req.Headers.AllKeys.FirstOrDefault(item => item == "Content-MD5") != null)
                sb.Append(req.Headers["Content-MD5"] + "\n");
            else
                sb.Append("\n");
            #endregion

            #region Content-Type
            if (!string.IsNullOrEmpty(req.ContentType))
                sb.Append(req.ContentType + "\n");
            else
                sb.Append("\n");
            #endregion

            #region Date
            if (req.Date != DateTime.MinValue)
                sb.Append(req.Date.ToString("R") + "\n");
            else
                sb.Append("\n");
            #endregion

            #region If-Modified-Since
            if (req.IfModifiedSince != DateTime.MinValue)
                sb.Append(req.IfModifiedSince.ToString("R") + "\n");
            else
                sb.Append("\n");
            #endregion

            #region If-Match
            if (req.Headers.AllKeys.FirstOrDefault(item => item == "If-Match") != null)
                sb.Append(req.Headers["If-Match"] + "\n");
            else
                sb.Append("\n");
            #endregion

            #region If-None-Match
            if (req.Headers.AllKeys.FirstOrDefault(item => item == "If-None-Match") != null)
                sb.Append(req.Headers["If-None-Match"] + "\n");
            else
                sb.Append("\n");
            #endregion

            #region If-Unmodified-Since
            if (req.Headers.AllKeys.FirstOrDefault(item => item == "If-Unmodified-Since") != null)
                sb.Append(req.Headers["If-Unmodified-Since"] + "\n");
            else
                sb.Append("\n");
            #endregion

            #region Range
            if (req.Headers.AllKeys.FirstOrDefault(item => item == "Range") != null)
                sb.Append(req.Headers["Range"] + "\n");
            else
                sb.Append("\n");
            #endregion

            sb.Append(CanonicalizedHeader);
            sb.Append(CanonicalizedResource);

            string signature = sb.ToString();

            return SignTheStringToSign(signature, sharedKey);
        }

        public static string GetAccountNameFromUri(Uri uri)
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

        public static string GenerateCanonicalizedHeader(System.Net.HttpWebRequest req)
        {
            StringBuilder sb = new StringBuilder();
            string[] strHeadersSorted = req.Headers.AllKeys.AsParallel().Where(item => item.StartsWith("x-ms-")).OrderBy(item => item).ToArray();

            foreach (string sHeader in strHeadersSorted)
            {
                sb.AppendFormat(
                    "{0:S}:{1:S}\n",
                    sHeader.ToLowerInvariant(),
                    req.Headers[sHeader].Trim()
                    );
            }

            return sb.ToString();
        }

        public static string GenerateCanonicalizedResource(string uri)
        {
            return GenerateCanonicalizedResource(new Uri(uri));
        }

        public static string GenerateCanonicalizedResource(Uri uri)
        {
            StringBuilder sb = new StringBuilder("/");

            sb.AppendFormat("{0:S}", GetAccountNameFromUri(uri));

            string[] tokens = uri.AbsolutePath.Split(new char[] { '/' });
            bool fAdded = false;

            for (int i = 0; i < tokens.Length; i++)
            {
                if (!string.IsNullOrEmpty(tokens[i]))
                {
                    sb.AppendFormat("/{0:S}", tokens[i]);
                    fAdded = true;
                }
            }
            if (!fAdded)
                sb.Append("/");

            Dictionary<string, List<string>> dParams = new Dictionary<string, List<string>>();

            if (!string.IsNullOrEmpty(uri.Query))
            {
                foreach (string sParam in uri.Query.Substring(1).Split(new char[] { '&' }))
                {
                    int idx = sParam.IndexOf('=');
                    string sKey = Uri.UnescapeDataString(sParam.Substring(0, idx)).ToLowerInvariant();
                    string sValue = Uri.UnescapeDataString(sParam.Substring(idx + 1, sParam.Length - idx - 1));

                    if (!dParams.ContainsKey(sKey))
                    {
                        if ((sKey == "nextpartitionkey") || (sKey == "nextrowkey"))
                            continue;
                        dParams[sKey] = new List<string>();
                    }
                    dParams[sKey].Add(sValue);
                }
            }

            KeyValuePair<string, List<string>>[] kvpairs = dParams.AsParallel().OrderBy(item => item.Key).ToArray();

            if (kvpairs.Length > 0)
                sb.Append("\n");

            for (int iPair = 0; iPair < kvpairs.Length; iPair++)
            {
                StringBuilder sbParam = new StringBuilder();
                sbParam.AppendFormat("{0:S}:", kvpairs[iPair].Key);
                string[] vals = kvpairs[iPair].Value.AsParallel().OrderBy(item => item).ToArray();
                for (int i = 0; i < vals.Length; i++)
                {
                    sbParam.Append(vals[i]);
                    if (i + 1 < vals.Length)
                        sbParam.Append(",");
                }

                if ((iPair + 1) < kvpairs.Length)
                    sbParam.Append("\n");

                sb.Append(sbParam);
            }

            return sb.ToString();
        }

    }
}
