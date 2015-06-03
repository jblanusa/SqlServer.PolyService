using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;
using System.Text;

namespace SqlServer.PolyService
{
    [Serializable]
    [Microsoft.SqlServer.Server.SqlUserDefinedAggregate(
    Format.UserDefined, /// Binary Serialization because of StringBuilder
    IsInvariantToOrder = false, /// order changes the result
    IsInvariantToNulls = true,  /// nulls don't change the result
    IsInvariantToDuplicates = false, /// duplicates change the result
    MaxByteSize = -1
)]
    public class CreateCSV : IBinarySerialize
    {
        /// <summary>
        /// The variable that holds the intermediate result of the concatenation
        /// </summary>
        private StringBuilder intermediateResult;

        /// <summary>
        /// Initialize the internal data structures
        /// </summary>
        public void Init()
        {
            this.intermediateResult = new StringBuilder();
        }

        /// <summary>
        /// Accumulate the next value, not if the value is null
        /// </summary>
        /// <param name="value"></param>
        public void Accumulate(SqlString value)
        {
            if (value.IsNull)
            {
                return;
            }

            this.intermediateResult.Append(value.Value).Append(',');
        }

        /// <summary>
        /// Merge the partially computed aggregate with this aggregate.
        /// </summary>
        /// <param name="other"></param>
        public void Merge(CreateCSV other)
        {
            this.intermediateResult.Append(other.intermediateResult);
        }

        /// <summary>
        /// Called at the end of aggregation, to return the results of the aggregation.
        /// </summary>
        /// <returns></returns>
        public SqlString Terminate()
        {
            string output = string.Empty;
            //delete the trailing comma, if any
            if (this.intermediateResult != null
                && this.intermediateResult.Length > 0)
            {
                output = this.intermediateResult.ToString(0, this.intermediateResult.Length - 1);
            }

            return new SqlString(output);
        }

        public void Read(System.IO.BinaryReader r)
        {
            intermediateResult = new StringBuilder(r.ReadString());
        }

        public void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.intermediateResult.ToString());
        }
    }
}
