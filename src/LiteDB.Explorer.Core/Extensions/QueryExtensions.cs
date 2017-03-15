using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB.Explorer.Core.Extensions
{
    /// <summary>
    /// Extension methods for queries.  
    /// </summary>
    public static class QueryExtensions
    {
        /// <summary>
        /// Ands the query with another query.
        /// </summary>
        public static Query And(this Query query, Query andThis)
        {
            return query != null && andThis != null ? Query.And(query, andThis) : query ?? andThis;
        }

        /// <summary>
        /// Compares a field using the $between operator.
        /// </summary>
        public static Query Between(this string field, object value)
        {
            var betweenArray = value as List<object>;
            if (betweenArray?.Count != 2)
            {
                throw new InvalidCastException("The $between query operator should be formated like: { $between : [value1, value2] }");
            }

            return Query.Between(field, betweenArray.First().ToBsonValue(), betweenArray.Last().ToBsonValue());
        }

        /// <summary>
        /// Compares a field using the $in operator.
        /// </summary>
        public static Query In(this string field, object value)
        {
            var array = value as List<object>;
            if (array == null || array.Count == 0)
            {
                throw new InvalidCastException("The $in query operator should be formated like: { $in : [value1, ..., valueN] }");
            }
            
            return Query.In(field, new BsonArray(array.Select(element => element.ToBsonValue())));
        }

        /// <summary>
        /// Compares a field using the $ne operator.
        /// </summary>
        public static Query NotEqual(this string field, object value)
        {
            var valueAsDoc = value as Dictionary<string, object>;
            if (valueAsDoc != null)
            {
                return Query.EQ(field, BsonValue.Null)
                    .Or(Query.Not(valueAsDoc.ToQuery(field) ?? Query.EQ(field, new BsonDocument())));
            }
            return Query.Not(Query.EQ(field, value.ToBsonValue()));
        }

        /// <summary>
        /// Compares a field using the $or operator.
        /// </summary>
        public static Query Or(this string field, object value)
        {
            var array = value as List<object>;
            if (array == null || array.Count == 0)
            {
                throw new InvalidCastException("The $or query operator should be formated like: { $or : [value1, ..., valueN] }");
            }

            Query returnValue = null;

            foreach (var orValue in array)
            {
                var valueAsDoc = orValue as Dictionary<string, object>;
                if (valueAsDoc != null)
                {
                    returnValue = returnValue.Or(Query.Not(Query.EQ(field, BsonValue.Null))
                        .And(valueAsDoc.ToQuery(field) ?? Query.EQ(field, new BsonDocument())));
                }
                else
                {
                    returnValue = returnValue.Or(Query.EQ(field, orValue.ToBsonValue()));
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Ors the query with another query.
        /// </summary>
        public static Query Or(this Query query, Query orThis)
        {
            return query != null && orThis != null ? Query.Or(query, orThis) : query ?? orThis;
        }
        
        /// <summary>
        /// Converts a document to a query query.
        /// </summary>
        public static Query ToQuery(this Dictionary<string, object> doc, string field = null)
        {
            Query returnValue = null;

            foreach (var pair in doc)
            {
                var path = field == null ? pair.Key : $"{field}.{pair.Key}";

                var valueAsDoc = pair.Value as Dictionary<string, object>;
                if (valueAsDoc != null)
                {
                    var operatorPair = valueAsDoc.FirstOrDefault();
                    if (valueAsDoc.Count == 1 && operatorPair.Key.StartsWith("$"))
                    {
                        switch (operatorPair.Key)
                        {
                            case "$between":
                                returnValue = returnValue.And(path.Between(operatorPair.Value));
                                break;
                            case "$contains":
                                returnValue = returnValue.And(Query.Contains(path, Convert.ToString(operatorPair.Value)));
                                break;
                            case "$gt":
                                returnValue = returnValue.And(Query.GT(path, operatorPair.Value.ToBsonValue()));
                                break;
                            case "$gte":
                                returnValue = returnValue.And(Query.GTE(path, operatorPair.Value.ToBsonValue()));
                                break;
                            case "$in":
                                returnValue = returnValue.And(path.In(operatorPair.Value));
                                break;
                            case "$lt":
                                returnValue = returnValue.And(Query.LT(path, operatorPair.Value.ToBsonValue()));
                                break;
                            case "$lte":
                                returnValue = returnValue.And(Query.LTE(path, operatorPair.Value.ToBsonValue()));
                                break;
                            case "$like":
                                returnValue = returnValue.And(Query.StartsWith(path, Convert.ToString(operatorPair.Value)));
                                break;
                            case "$ne":
                                returnValue = returnValue.And(path.NotEqual(operatorPair.Value));
                                break;
                            case "$or":
                                returnValue = returnValue.And(path.Or(operatorPair.Value));
                                break;
                            default:
                                throw new InvalidCastException($"There is no query function for {operatorPair.Key}.");
                        }
                    }
                    else
                    {
                        returnValue = returnValue.And(valueAsDoc.ToQuery(path) ?? Query.EQ(path, new BsonDocument()));
                    }
                }
                else
                {
                    returnValue = returnValue.And(Query.EQ(path, pair.Value.ToBsonValue()));
                }
            }

            return returnValue;
        }
    }
}
