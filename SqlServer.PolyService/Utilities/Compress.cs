using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SqlServer.PolyService
{
    public static class Compress
    {

        static readonly UnicodeEncoding UnicodeEncoder = new UnicodeEncoding();


        /// <summary>
        /// Compressing the data
        /// </summary>
        [Microsoft.SqlServer.Server.SqlFunction(IsDeterministic = true, IsPrecise = true,
                DataAccess = DataAccessKind.None)]
        public static SqlBytes TextCompress(SqlString input)
        {

            if (input.IsNull || input.Value.Length == 0) return SqlBytes.Null;

            SqlBytes ret = SqlBytes.Null;

            using (MemoryStream result = new MemoryStream())
            {
                using (DeflateStream deflateStream =
                    new DeflateStream(result, CompressionMode.Compress, true))
                {
                    var bytes = input.GetUnicodeBytes();
                    deflateStream.Write(bytes, 0, bytes.Length);
                    deflateStream.Flush();
                    deflateStream.Close();
                }
                ret = new SqlBytes(result.ToArray());
            }
            return ret;
        }

        /// <summary>
        /// Decompressing the data
        /// </summary>
        [Microsoft.SqlServer.Server.SqlFunction(IsDeterministic = true, IsPrecise = true,
                DataAccess = DataAccessKind.None)]
        public static SqlString TextDecompress(SqlBytes input)
        {
            if (input.IsNull) return SqlString.Null;

            if (input.Length == 1 && input.Value[0] == 0) return new SqlString("");

            const int bufcnt = 50000;
            byte[] buf = new byte[bufcnt];

            using (MemoryStream result = new MemoryStream())
            {
                using (DeflateStream deflateStream =
                    new DeflateStream(input.Stream, CompressionMode.Decompress, true))
                {
                    int bytesRead;
                    while ((bytesRead = deflateStream.Read(buf, 0, bufcnt)) > 0)
                        result.Write(buf, 0, bytesRead);
                }
                return new SqlString(UnicodeEncoder.GetString(result.ToArray()));
            }
        }

    }
}
