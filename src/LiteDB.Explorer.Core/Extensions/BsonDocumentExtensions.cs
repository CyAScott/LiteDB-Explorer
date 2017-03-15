using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LiteDB.Explorer.Core.Extensions
{
    /// <summary>
    /// The format to use when converting BSON to string.
    /// </summary>
    [Flags]
    public enum BsonToStringFormat
    {
        /// <summary>
        /// Bson friendly means that function names are used to represent the data types that are not supported with Json.
        /// </summary>
        BsonFriendly = 1,

        /// <summary>
        /// Json friendly means that objects are used to represent the data types that are not supported with Json.
        /// </summary>
        JsonFriendly = 2,

        /// <summary>
        /// If the resulting text should be indented.
        /// </summary>
        Indent = 4
    }
    /// <summary>
    /// Extension methods for BSON documents.
    /// </summary>
    public static class BsonDocumentExtensions
    {
        private static int indexOf(this string text, Func<char, bool> match, Func<char, bool> errorMatch, int index)
        {
            for (; index < text.Length; index++)
            {
                if (match(text[index]))
                {
                    return index;
                }

                if (errorMatch(text[index]))
                {
                    throw new ArgumentException($"Error parsing '{text.Substring(index, 1).EscapeStringAsJson()}' at " +
                                                $"({text.Take(index).Count(c => c == '\n')}, {index - text.LastIndexOf('\n')}).");
                }
            }

            throw new ArgumentException("Invalid format.");
        }

        //bson to string
        private static string valueToText(this BsonArray value, BsonToStringFormat format, string leftPad = null)
        {
            if (value.Count == 0)
            {
                return "[]";
            }

            var builder = new StringBuilder();
            var isFirstElement = true;
            
            if (format.HasFlag(BsonToStringFormat.Indent))
            {
                builder.AppendLine("[");
            }
            else
            {
                builder.Append("[");
            }

            foreach (var item in value)
            {
                if (isFirstElement)
                {
                    isFirstElement = false;
                }
                else if (format.HasFlag(BsonToStringFormat.Indent))
                {
                    builder.AppendLine(",");
                }
                else
                {
                    builder.Append(", ");
                }

                builder.Append(format.HasFlag(BsonToStringFormat.Indent) ?
                    $"{leftPad}\t{item.valueToText(format, $"{leftPad}\t")}" :
                    item.valueToText(format));
            }
            
            if (format.HasFlag(BsonToStringFormat.Indent) && !isFirstElement)
            {
                builder.AppendLine();
            }

            builder.Append(format.HasFlag(BsonToStringFormat.Indent) ? $"{leftPad}]" : "]");

            return builder.ToString();
        }
        private static string valueToText(this BsonValue value, BsonToStringFormat format, string leftPad = null)
        {
            switch (value.Type)
            {
                case BsonType.Null:
                    return "null";
                case BsonType.Int32:
                    return format.HasFlag(BsonToStringFormat.JsonFriendly) ? value.AsInt32.ToString() : $"Int({value.AsInt32})";
                case BsonType.Int64:
                    return format.HasFlag(BsonToStringFormat.JsonFriendly) ? value.AsInt64.ToString() : $"Long({value.AsInt64})";
                case BsonType.Double:
                    return format.HasFlag(BsonToStringFormat.JsonFriendly) ? value.AsDouble.ToString("0.0################", NumberFormatInfo.InvariantInfo) : $"Double({value.AsDouble.ToString("0.0################", NumberFormatInfo.InvariantInfo)})";
                case BsonType.Decimal:
                    return format.HasFlag(BsonToStringFormat.JsonFriendly) ? value.AsDecimal.ToString("0.0################", NumberFormatInfo.InvariantInfo) : $"Decimal({value.AsDecimal.ToString("0.0################", NumberFormatInfo.InvariantInfo)})";
                case BsonType.String:
                    return $"\"{value.AsString.EscapeStringAsJson()}\"";
                case BsonType.Document:
                    return value.AsDocument.ToText(format, leftPad);
                case BsonType.Array:
                    return value.AsArray.valueToText(format, leftPad);
                case BsonType.Binary:
                    return format.HasFlag(BsonToStringFormat.JsonFriendly) ? $"{{\"$binary\" : \"{Convert.ToBase64String(value.AsBinary).EscapeStringAsJson()}\"}}" : $"Binary(\"{Convert.ToBase64String(value.AsBinary).EscapeStringAsJson()}\")";
                case BsonType.ObjectId:
                    return format.HasFlag(BsonToStringFormat.JsonFriendly) ? $"{{\"$oid\" : \"{value.AsObjectId}\"}}" : $"ObjectId(\"{value.AsObjectId}\")";
                case BsonType.Guid:
                    return format.HasFlag(BsonToStringFormat.JsonFriendly) ? $"{{\"$guid\" : \"{value.AsGuid}\"}}" : $"Guid(\"{value.AsGuid}\")";
                case BsonType.Boolean:
                    return value.AsBoolean ? "true" : "false";
                case BsonType.DateTime:
                    return format.HasFlag(BsonToStringFormat.JsonFriendly) ? $"\"{value.AsDateTime:yyyy-MM-ddTHH:mm:ss.fffZ}\"" : $"ISODate(\"{value.AsDateTime:O}\")";
                case BsonType.MaxValue:
                    return format.HasFlag(BsonToStringFormat.JsonFriendly) ? "{\"$maxValue\" : \"\"}" : "MaxValue()";
                case BsonType.MinValue:
                    return format.HasFlag(BsonToStringFormat.JsonFriendly) ? "{\"$minValue\" : \"\"}" : "MinValue()";
            }

            throw new NotSupportedException();
        }

        //string to bson
        private static List<object> parseArray(this string text, ref int index)
        {
            var array = new List<object>();

            //find start of object
            index = text.indexOf(c => c == '[', c => !char.IsWhiteSpace(c), index) + 1;

            index = text.indexOf(c => !char.IsWhiteSpace(c), c => false, index);
            if (text[index] == ']')
            {
                index++;
                return array;
            }

            while (index != -1)
            {
                //read value
                var value = parseValue(text, ref index);
                array.Add(value);

                //read comma or end of array
                index = text.indexOf(c => c == ',' || c == ']', c => c != ',' && c != ']' && !char.IsWhiteSpace(c), index);
                if (text[index] == ']')
                {
                    index++;
                    return array;
                }
                index++;

                index = text.indexOf(c => !char.IsWhiteSpace(c), c => false, index);
            }

            throw new ArgumentException($"Error parsing array at ({text.Take(index).Count(c => c == '\n')}, {index - text.LastIndexOf('\n')}).");
        }
        private static object documentToType(Dictionary<string, object> doc)
        {
            if (doc.Count == 1)
            {
                var pair = doc.First();

                if (pair.Key == "$binary")
                {
                    return Convert.FromBase64String(Convert.ToString(pair.Value));
                }

                if (pair.Key == "$oid")
                {
                    return new ObjectId(Convert.ToString(pair.Value));
                }

                if (pair.Key == "$guid")
                {
                    return new Guid(Convert.ToString(pair.Value));
                }

                if (pair.Key == "$date")
                {
                    return DateTime.Parse(Convert.ToString(pair.Value));
                }

                if (pair.Key == "$numberLong")
                {
                    return long.Parse(Convert.ToString(pair.Value));
                }

                if (pair.Key == "$numberDecimal")
                {
                    return decimal.Parse(Convert.ToString(pair.Value));
                }

                if (pair.Key == "$minValue")
                {
                    return BsonValue.MinValue;
                }

                if (pair.Key == "$maxValue")
                {
                    return BsonValue.MaxValue;
                }
            }

            return doc;
        }
        private static object parseDocument(this string text, ref int index)
        {
            var bson = new Dictionary<string, object>();

            //find start of object
            index = text.indexOf(c => c == '{', c => !char.IsWhiteSpace(c), index) + 1;

            index = text.indexOf(c => !char.IsWhiteSpace(c), c => false, index);
            if (text[index] == '}')
            {
                index++;
                return documentToType(bson);
            }

            while (index != -1)
            {
                //read key
                string key;
                if (text[index] == '\"' || text[index] == '\'')
                {
                    key = text.parseString(ref index);
                    index++;
                }
                else
                {
                    key = text.Substring(index, text.IndexOf(":", index, StringComparison.OrdinalIgnoreCase) - index).TrimEnd();
                    index += key.Length;
                }
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException($"Error parsing key at ({text.Take(index).Count(c => c == '\n')}, {index - text.LastIndexOf('\n')}).");
                }
                index = text.indexOf(c => c == ':', c => c != ':' && !char.IsWhiteSpace(c), index) + 1;
                index = text.indexOf(c => !char.IsWhiteSpace(c), c => false, index);

                //read value
                var value = parseValue(text, ref index);

                //set value
                var keyPath = key.Split('.');
                var doc = bson;
                foreach (var keyItem in keyPath.Take(keyPath.Length - 1))
                {
                    object keyValue;
                    if (doc.TryGetValue(keyItem, out keyValue))
                    {
                        doc = (Dictionary<string, object>)keyValue;
                    }
                    else
                    {
                        var newDoc = new Dictionary<string, object>();
                        doc[keyItem] = newDoc;
                        doc = newDoc;
                    }
                }
                doc[keyPath.Last()] = value;

                //read comma or end of object
                index = text.indexOf(c => c == ',' || c == '}', c => c != ',' && c != '}' && !char.IsWhiteSpace(c), index);
                if (text[index] == '}')
                {
                    index++;
                    return documentToType(bson);
                }
                index++;

                index = text.indexOf(c => !char.IsWhiteSpace(c), c => false, index);
            }

            throw new ArgumentException($"Error parsing object at ({text.Take(index).Count(c => c == '\n')}, {index - text.LastIndexOf('\n')}).");
        }
        private static object parseValue(this string text, ref int index)
        {
            var startText = text.Substring(index);

            if (text[index] == '\"' || text[index] == '\'')
            {
                var valueAsString = text.parseString(ref index);
                index++;
                
                //if (Regex.IsMatch(startText, @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$", RegexOptions.IgnoreCase))
                //{
                //    return DateTime.Parse(startText, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                //}

                return valueAsString;
            }

            if (text[index] == '[')
            {
                return parseArray(text, ref index);
            }

            if (text[index] == '{')
            {
                return parseDocument(text, ref index);
            }

            if (startText.StartsWith("null", StringComparison.OrdinalIgnoreCase))
            {
                index += 4;
                return null;
            }

            if (startText.StartsWith("false", StringComparison.OrdinalIgnoreCase))
            {
                index += 5;
                return false;
            }

            if (startText.StartsWith("true", StringComparison.OrdinalIgnoreCase))
            {
                index += 4;
                return true;
            }

            if (Regex.IsMatch(startText, @"^Int\s*\(\s*[-+]?\d+\s*\)", RegexOptions.IgnoreCase))
            {
                index += startText.IndexOf(')') + 1;

                var number = Regex.Match(startText, @"\(.*\)").ToString().Trim('(', ')');

                return int.Parse(number);
            }

            if (Regex.IsMatch(startText, @"^Double\s*\(\s*[-+]?(\d+([,.]\d*)?|[,.]\d+)\s*\)", RegexOptions.IgnoreCase))
            {
                index += startText.IndexOf(')') + 1;

                var number = Regex.Match(startText, @"\(.*\)").ToString().Trim('(', ')');

                return double.Parse(number);
            }

            if (Regex.IsMatch(startText, @"^Long\s*\(\s*[-+]?\d+\s*\)", RegexOptions.IgnoreCase))
            {
                index += startText.IndexOf(')') + 1;

                var number = Regex.Match(startText, @"\(.*\)").ToString().Trim('(', ')');

                return long.Parse(number);
            }

            if (Regex.IsMatch(startText, @"^Decimal\s*\(\s*[-+]?(\d+([,.]\d*)?|[,.]\d+)\s*\)", RegexOptions.IgnoreCase))
            {
                index += startText.IndexOf(')') + 1;

                var number = Regex.Match(startText, @"\(.*\)").ToString().Trim('(', ')');

                return decimal.Parse(number, NumberStyles.Float);
            }

            if (Regex.IsMatch(startText, @"^Binary\s*\(\s*""(?:[a-z\d+/]{4})*(?:[a-z\d+/]{2}==|[a-z\d+/]{3}=)?""\s*\)", RegexOptions.IgnoreCase))
            {
                index += startText.IndexOf(')') + 1;

                var binary = Regex.Match(startText, @"\(.*\)").ToString().Trim('(', ')').Trim().Trim('"');

                return Convert.FromBase64String(binary);
            }

            if (Regex.IsMatch(startText, @"^ObjectId\s*\(\s*""[a-f\d]{24}""\s*\)", RegexOptions.IgnoreCase))
            {
                index += startText.IndexOf(')') + 1;

                var id = Regex.Match(startText, @"\(.*\)").ToString().Trim('(', ')').Trim().Trim('"');

                return new ObjectId(id);
            }

            if (Regex.IsMatch(startText, @"^Guid\s*\(\s*""[a-f\d]{8}[-]?([a-f\d]{4}[-]?){3}[a-f\d]{12}""\s*\)", RegexOptions.IgnoreCase))
            {
                index += startText.IndexOf(')') + 1;

                var id = Regex.Match(startText, @"\(.*\)").ToString().Trim('(', ')').Trim().Trim('"');

                return new Guid(id);
            }

            if (Regex.IsMatch(startText, @"^ISODate\s*\(\s*""\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{7}Z""\s*\)", RegexOptions.IgnoreCase))
            {
                index += startText.IndexOf(')') + 1;

                var dateTime = Regex.Match(startText, @"\(.*\)").ToString().Trim('(', ')').Trim().Trim('"');

                return DateTime.Parse(dateTime, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            }

            if (Regex.IsMatch(startText, @"^MaxValue\s*\(\s*\)", RegexOptions.IgnoreCase))
            {
                index += startText.IndexOf(')') + 1;

                return BsonValue.MaxValue;
            }

            if (Regex.IsMatch(startText, @"^MinValue\s*\(\s*\)", RegexOptions.IgnoreCase))
            {
                index += startText.IndexOf(')') + 1;

                return BsonValue.MinValue;
            }

            if (Regex.IsMatch(startText, @"^[-+]?\d+([\s,\}\]]|$)", RegexOptions.IgnoreCase))
            {
                var number = Regex.Match(startText, @"^[-+]?\d+([\s,\}\]]|$)").ToString();

                if (!char.IsDigit(number[number.Length - 1]))
                {
                    number = number.Substring(0, number.Length - 1);
                }

                index += number.Length;

                int returnInt;
                if (int.TryParse(number, out returnInt))
                {
                    return returnInt;
                }

                long returnLong;
                if (long.TryParse(number, out returnLong))
                {
                    return returnLong;
                }

                decimal returnDecimal;
                if (decimal.TryParse(number, out returnDecimal))
                {
                    return returnDecimal;
                }

                double returnDouble;
                if (double.TryParse(number, out returnDouble))
                {
                    return returnDouble;
                }
            }

            if (Regex.IsMatch(startText, @"^[-+]?(\d+(\.\d*)?|\.\d+)([\s,\}\]]|$)", RegexOptions.IgnoreCase))
            {
                var number = Regex.Match(startText, @"^[-+]?(\d+(\.\d*)?|\.\d+)([\s,\}\]]|$)").ToString();

                if (number[number.Length - 1] != '.' || !char.IsDigit(number[number.Length - 1]))
                {
                    number = number.Substring(0, number.Length - 1);
                }

                index += number.Length;

                decimal returnDecimal;
                double returnDouble;

                if (decimal.TryParse(number, out returnDecimal))
                {
                    if (double.TryParse(number, out returnDouble) &&
                        returnDecimal.ToString("0.0################", NumberFormatInfo.InvariantInfo) ==
                        returnDouble.ToString("0.0################", NumberFormatInfo.InvariantInfo))
                    {
                        return returnDouble;
                    }
                    return returnDecimal;
                }

                if (double.TryParse(number, out returnDouble))
                {
                    return returnDouble;
                }
            }
            
            throw new ArgumentException($"Error parsing BSON value at ({text.Take(index).Count(c => c == '\n')}, {index - text.LastIndexOf('\n')}).");
        }
        private static string parseString(this string text, ref int index)
        {
            var builder = new StringBuilder();
            var endChar = text[index];

            try
            {
                for (index++; index < text.Length; index++)
                {
                    if (text[index] == endChar)
                    {
                        return builder.ToString();
                    }

                    if (text[index] == '\\')
                    {
                        index++;

                        switch (text[index])
                        {
                            default:
                                throw new IndexOutOfRangeException();
                            case 'b':
                                builder.Append('\b');
                                break;
                            case 'f':
                                builder.Append('\f');
                                break;
                            case 'n':
                                builder.Append('\n');
                                break;
                            case 'r':
                                builder.Append('\r');
                                break;
                            case 't':
                                builder.Append('\t');
                                break;
                            case '\"':
                                builder.Append('\"');
                                break;
                            case '\'':
                                builder.Append('\'');
                                break;
                            case '\\':
                                builder.Append('\\');
                                break;
                            case 'u':
                                builder.Append((char)Convert.ToInt32(text.Substring(index + 1, 4), 16));
                                index += 4;
                                break;
                        }
                    }
                    else
                    {
                        builder.Append(text[index]);
                    }
                }

                throw new IndexOutOfRangeException();
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentException($"Error parsing string at ({text.Take(index).Count(c => c == '\n')}, {index - text.LastIndexOf('\n')}).");
            }
        }
        
        /// <summary>
        /// Parses a BSON document from a string.
        /// </summary>
        public static BsonDocument ParseBsonDocument(this string text)
        {
            var index = 0;
            
            return (BsonDocument)parseDocument(text, ref index).ToBsonValue();
        }

        /// <summary>
        /// Converts an object to a BSON value.
        /// </summary>
        public static BsonValue ToBsonValue(this object value)
        {
            if (value == null)
            {
                return BsonValue.Null;
            }

            var array = value as List<object>;
            if (array != null)
            {
                return new BsonArray(array.Select(ToBsonValue));
            }

            var doc = value as Dictionary<string, object>;
            if (doc != null)
            {
                var returnBsonDoc = new BsonDocument();

                foreach (var pair in doc)
                {
                    returnBsonDoc[pair.Key] = pair.Value.ToBsonValue();
                }

                return returnBsonDoc;
            }

            var binary = value as byte[];
            if (binary != null)
            {
                return new BsonValue(binary);
            }

            var objectId = value as ObjectId;
            if (objectId != null)
            {
                return new BsonValue(objectId);
            }

            var text = value as string;
            if (text != null)
            {
                return new BsonValue(text);
            }

            if (value is DateTime)
            {
                return new BsonValue((DateTime)value);
            }

            if (value is Guid)
            {
                return new BsonValue((Guid)value);
            }

            if (value is bool)
            {
                return new BsonValue((bool)value);
            }

            if (value is decimal)
            {
                return new BsonValue((decimal)value);
            }

            if (value is double)
            {
                return new BsonValue((double)value);
            }

            if (value is int)
            {
                return new BsonValue((int)value);
            }

            if (value is long)
            {
                return new BsonValue((long)value);
            }

            return (BsonValue)value;
        }

        /// <summary>
        /// Parses a BSON document from a string.
        /// </summary>
        public static Dictionary<string, object> ParseDocument(this string text)
        {
            var index = 0;

            return (Dictionary<string, object>)parseDocument(text, ref index);
        }

        /// <summary>
        /// Escapes a string in JSON format.
        /// </summary>
        public static string EscapeStringAsJson(this string text)
        {
            var builder = new StringBuilder();

            foreach (var c in text)
            {
                switch (c)
                {
                    case '\b':
                        builder.Append(@"\b");
                        break;
                    case '\f':
                        builder.Append(@"\f");
                        break;
                    case '\n':
                        builder.Append(@"\n");
                        break;
                    case '\r':
                        builder.Append(@"\r");
                        break;
                    case '\t':
                        builder.Append(@"\t");
                        break;
                    case '\"':
                        builder.Append(@"\""");
                        break;
                    case '\\':
                        builder.Append(@"\\");
                        break;
                    default:
                        if (c < ' ')
                        {
                            builder.Append($@"\u{(int)c:x4}");
                        }
                        else
                        {
                            builder.Append(c);
                        }
                        break;
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Converts the BSON document to an indented formatted string.
        /// </summary>
        public static string ToText(this BsonDocument doc, BsonToStringFormat format = BsonToStringFormat.BsonFriendly | BsonToStringFormat.Indent, string leftPad = null)
        {
            if (doc.Count == 0)
            {
                return "{}";
            }

            var builder = new StringBuilder();
            var isFirstProperty = true;

            if (format.HasFlag(BsonToStringFormat.Indent))
            {
                builder.AppendLine("{");
            }
            else
            {
                builder.Append("{");
            }

            foreach (var property in doc)
            {
                if (isFirstProperty)
                {
                    isFirstProperty = false;
                }
                else if (format.HasFlag(BsonToStringFormat.Indent))
                {
                    builder.AppendLine(",");
                }
                else
                {
                    builder.Append(", ");
                }

                var key = $"\"{property.Key.EscapeStringAsJson()}\"";

                builder.Append(format.HasFlag(BsonToStringFormat.Indent) ? 
                    $"{leftPad}\t{key} : {property.Value.valueToText(format, $"{leftPad}\t")}" : 
                    $"{key} : {property.Value.valueToText(format)}");
            }

            if (format.HasFlag(BsonToStringFormat.Indent) && !isFirstProperty)
            {
                builder.AppendLine();
            }

            builder.Append(format.HasFlag(BsonToStringFormat.Indent) ? $"{leftPad}}}" : "}");

            return builder.ToString();
        }
    }
}
