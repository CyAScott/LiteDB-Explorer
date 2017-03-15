using System;
using System.Text.RegularExpressions;
using LiteDB.Explorer.Core.Extensions;
using NUnit.Framework;

namespace LiteDB.Explorer.Tests
{
    public class BsonStringTests
    {
        private static BsonDocument getTestDocument(bool addSubDocument = true, bool includeDates = true)
        {
            const string allChars = "\0\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\v\u000C\r\u000E\u000F\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001A\u001B\u001C\u001D\u001E\u001F !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~€‚ƒ„…†‡ˆ‰Š‹ŒŽ‘’“”•–—˜™š›œžŸ ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";
            return new BsonDocument
            {
                {"Null", BsonValue.Null},
                {"Int32Min", int.MinValue},
                {"Int32Max", int.MaxValue},
                {"Int64Min", long.MinValue},
                {"Int64Max", long.MaxValue},
                {"DoubleMin", 3.14159265359},
                {"DecimalMin", decimal.MinValue},
                {"DecimalMax", decimal.MaxValue},
                {"Binary", Guid.Parse("16880f92-6092-43ec-b9cc-6def44f03dcc").ToByteArray()},
                {"String", allChars},
                {"Document", addSubDocument ? getTestDocument(false, includeDates) : new BsonDocument()},
                {"Array", !addSubDocument ? new BsonArray() : new BsonArray
                {
                    BsonValue.Null,
                    int.MinValue,
                    int.MaxValue,
                    long.MinValue,
                    long.MaxValue,
                    3.14159265359,
                    decimal.MinValue,
                    decimal.MaxValue,
                    Guid.Parse("16880f92-6092-43ec-b9cc-6def44f03dcc").ToByteArray(),
                    allChars,
                    getTestDocument(false, includeDates),
                    new BsonArray(),
                    new ObjectId("c969ce7c86ebf1670512579b"),
                    Guid.Parse("16880f92-6092-43ec-b9cc-6def44f03dcc"),
                    false,
                    true,
                    includeDates ? (BsonValue)DateTime.UtcNow : (BsonValue)DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                }},
                {"ObjectId", new ObjectId("c969ce7c86ebf1670512579b")},
                {"Guid", Guid.Parse("16880f92-6092-43ec-b9cc-6def44f03dcc")},
                {"BooleanFalse", false},
                {"BooleanTrue", true},
                {"DateTime", includeDates ? (BsonValue)DateTime.UtcNow : (BsonValue)DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}
            };
        }
        private static void assertAreEqual(BsonValue a, BsonValue b)
        {
            object bValue = null;
            switch (a.Type)
            {
                case BsonType.Decimal:
                    switch (b.Type)
                    {
                        case BsonType.Double:
                            bValue = Convert.ToDecimal(b.AsDouble);
                            break;
                        case BsonType.Int32:
                            bValue = Convert.ToDecimal(b.AsInt32);
                            break;
                        case BsonType.Int64:
                            bValue = Convert.ToDecimal(b.AsInt64);
                            break;
                        default:
                            Assert.AreEqual(a.Type, b.Type);
                            break;
                    }
                    break;
                case BsonType.Double:
                    switch (b.Type)
                    {
                        case BsonType.Decimal:
                            bValue = Convert.ToDouble(b.AsDecimal);
                            break;
                        case BsonType.Int32:
                            bValue = Convert.ToDouble(b.AsInt32);
                            break;
                        case BsonType.Int64:
                            bValue = Convert.ToDouble(b.AsInt64);
                            break;
                        default:
                            Assert.AreEqual(a.Type, b.Type);
                            break;
                    }
                    break;
                case BsonType.Int32:
                    switch (b.Type)
                    {
                        case BsonType.Decimal:
                            bValue = Convert.ToInt32(b.AsDecimal);
                            break;
                        case BsonType.Double:
                            bValue = Convert.ToInt32(b.AsDouble);
                            break;
                        case BsonType.Int64:
                            bValue = Convert.ToInt32(b.AsInt64);
                            break;
                        default:
                            Assert.AreEqual(a.Type, b.Type);
                            break;
                    }
                    break;
                case BsonType.Int64:
                    switch (b.Type)
                    {
                        case BsonType.Decimal:
                            bValue = Convert.ToInt64(b.AsDecimal);
                            break;
                        case BsonType.Double:
                            bValue = Convert.ToInt64(b.AsDouble);
                            break;
                        case BsonType.Int32:
                            bValue = Convert.ToInt64(b.AsInt32);
                            break;
                        default:
                            Assert.AreEqual(a.Type, b.Type);
                            break;
                    }
                    break;
                case BsonType.DateTime:
                    if (b.Type == BsonType.String)
                    {
                        var str = b.AsString;
                        Assert.IsTrue(Regex.IsMatch(str, @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$", RegexOptions.IgnoreCase));
                        bValue = DateTime.Parse(str);
                    }
                    else
                    {
                        Assert.AreEqual(a.Type, b.Type);
                    }
                    break;
                default:
                    Assert.AreEqual(a.Type, b.Type);
                    break;
            }

            switch (a.Type)
            {
                case BsonType.Int32:
                    Assert.AreEqual(a.AsInt32, bValue ?? b.AsInt32);
                    break;
                case BsonType.Int64:
                    Assert.AreEqual(a.AsInt64, bValue ?? b.AsInt64);
                    break;
                case BsonType.Double:
                    Assert.AreEqual(a.AsDouble, bValue ?? b.AsDouble);
                    break;
                case BsonType.Decimal:
                    Assert.AreEqual(a.AsDecimal, bValue ?? b.AsDecimal);
                    break;
                case BsonType.String:
                    Assert.AreEqual(a.AsString, b.AsString);
                    break;
                case BsonType.Document:
                    var aDoc = a.AsDocument;
                    var bDoc = b.AsDocument;

                    Assert.AreEqual(aDoc.Count, bDoc.Count);

                    foreach (var item in aDoc)
                    {
                        Assert.IsTrue(bDoc.ContainsKey(item.Key));

                        assertAreEqual(item.Value, bDoc[item.Key]);
                    }
                    break;
                case BsonType.Array:
                    var aArray = a.AsArray;
                    var bArray = b.AsArray;

                    Assert.AreEqual(aArray.Count, bArray.Count);

                    for (var index = 0; index < aArray.Count; index++)
                    {
                        assertAreEqual(aArray[index], bArray[index]);
                    }
                    break;
                case BsonType.Binary:
                    var aBinary = a.AsBinary;
                    var bBinary = b.AsBinary;

                    Assert.AreEqual(aBinary.Length, bBinary.Length);

                    for (var index = 0; index < aBinary.Length; index++)
                    {
                        Assert.AreEqual(aBinary[index], bBinary[index]);
                    }
                    break;
                case BsonType.ObjectId:
                    Assert.AreEqual(a.AsObjectId, b.AsObjectId);
                    break;
                case BsonType.Guid:
                    Assert.AreEqual(a.AsGuid, b.AsGuid);
                    break;
                case BsonType.Boolean:
                    Assert.AreEqual(a.AsBoolean, b.AsBoolean);
                    break;
                case BsonType.DateTime:
                    Assert.AreEqual(a.AsDateTime, bValue ?? b.AsDateTime);
                    break;
            }
        }
        
        [Test]
        public void TextParsing()
        {
            var doc = getTestDocument();

            var text = doc.ToText();
            Assert.IsNotNull(text);

            var parsedDoc = text.ParseBsonDocument();
            Assert.IsNotNull(parsedDoc);

            assertAreEqual(doc, parsedDoc);

            doc = getTestDocument(includeDates: false);

            text = doc.ToText(BsonToStringFormat.JsonFriendly);
            Assert.IsNotNull(text);

            parsedDoc = text.ParseBsonDocument();
            Assert.IsNotNull(parsedDoc);

            assertAreEqual(doc, parsedDoc);

            //test parsing dot notation
            text = "{\"test.1.2.3\" : 123, \"test.1.2.4\" : 456}";
            parsedDoc = text.ParseBsonDocument();
            Assert.IsNotNull(parsedDoc);
            Assert.AreEqual(parsedDoc.Count, 1);

            parsedDoc = parsedDoc["test"].AsDocument;
            Assert.IsNotNull(parsedDoc);
            Assert.AreEqual(parsedDoc.Count, 1);

            parsedDoc = parsedDoc["1"].AsDocument;
            Assert.IsNotNull(parsedDoc);
            Assert.AreEqual(parsedDoc.Count, 1);

            parsedDoc = parsedDoc["2"].AsDocument;
            Assert.IsNotNull(parsedDoc);
            Assert.AreEqual(parsedDoc.Count, 2);
            Assert.AreEqual(parsedDoc["3"].AsInt32, 123);
            Assert.AreEqual(parsedDoc["4"].AsInt32, 456);
        }
    }
}
