using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NftIndexer
{


    public class BigIntegerJsonConverter : JsonConverter<BigInteger>
    {
        public override BigInteger Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            BigInteger result;
            BigInteger.TryParse(reader.GetString(), out result);
            return result;
        }

        public override void Write(Utf8JsonWriter writer,
            BigInteger bigIntegerValue,
            JsonSerializerOptions options)
        {
            string value = "0";
            try
            {
                value = bigIntegerValue.ToString();
            }
            catch (Exception)
            {
            }
            writer.WriteStringValue(value);
        }

    }
}

