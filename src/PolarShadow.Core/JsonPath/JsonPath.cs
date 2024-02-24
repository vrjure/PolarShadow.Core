using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PolarShadow.Core
{
    public static class JsonPath
    {
        public static T ReadValue<T>(this JsonElement root, string jsonPath)
        {
            var value = Read(root, jsonPath);
            return value.ValueKind switch
            {
                JsonValueKind.Object => JsonSerializer.Deserialize<T>(value, JsonOption.DefaultSerializer),
                JsonValueKind.Array => JsonSerializer.Deserialize<T>(value, JsonOption.DefaultSerializer),
                JsonValueKind.String => value.GetString() is T str ? str : default,
                JsonValueKind.Number => value.GetDecimal() is T dec ? dec : value.GetInt64() is T int64 ? int64: value.GetInt32() is T int32 ? int32 : value.GetInt16() is T int16 ? int16: default,
                JsonValueKind.True => value.GetBoolean() is T boolean ? boolean : default,
                JsonValueKind.False => value.GetBoolean() is T boolean? boolean : default,
                _ => default
            };
        }

        public static JsonElement Read(this JsonElement root, string jsonPath)
        {
            var reader = new JsonPathReader(jsonPath);
            if (!reader.Read() || reader.TokenType != JsonPathTokenType.Root) return default;
            return ReadRootNext(ref reader, root, root);
        }

        private static JsonElement ReadRootNext(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            if (!reader.Read()) return default;

            if (IsExpressionPartEndOrEnd(ref reader)) return current;

            return reader.TokenType switch
            {
                JsonPathTokenType.Child => ReadChildNext(ref reader, current, root),
                JsonPathTokenType.DeepScan => ReadDeepScanNext(ref reader, current, root),
                JsonPathTokenType.StartFilter => ReadStartFilterNext(ref reader, current, root),
                _ => default
            };
        }

        private static JsonElement ReadChildNext(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            if (!reader.Read()) return default;

            var value = reader.TokenType switch
            {
                JsonPathTokenType.PropertyName => ReadProperty(current, reader.Slice()),
                JsonPathTokenType.Wildcard => ReadAllChildValue(current),
                _ => default
            };
            return ReadPropertyNext(ref reader, value, root);
        }

        private static JsonElement ReadAllChildValue(JsonElement current)
        {
            using var ms = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(ms, JsonOption.DefaultWriteOption);
            jsonWriter.WriteStartArray();
            if (current.ValueKind == JsonValueKind.Object)
            {
                foreach (var item in current.EnumerateObject())
                {
                    item.Value.WriteTo(jsonWriter);
                }
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in current.EnumerateArray())
                {
                    item.WriteTo(jsonWriter);
                }
            }
            jsonWriter.WriteEndArray();
            jsonWriter.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            using var doc = JsonDocument.Parse(ms);
            return doc.RootElement.Clone();     
        }

        private static JsonElement ReadDeepScanNext(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            if (!reader.Read())
            {
                return default;
            }

            var value = reader.TokenType switch
            {
                JsonPathTokenType.PropertyName => DeepScanProperty(current, reader.Slice()),
                JsonPathTokenType.Wildcard => ReadAllValue(current),
                _ => default
            };

            return ReadDeepScanPropertyNext(ref reader, value, root);
        }

        private static JsonElement ReadAllValue(JsonElement current)
        {
            using var ms = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(ms, JsonOption.DefaultWriteOption);
            jsonWriter.WriteStartArray();
            if (current.ValueKind == JsonValueKind.Object)
            {
                foreach (var item in current.EnumerateObject())
                {
                    ReadAllValue(jsonWriter, item.Value);
                }
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in current.EnumerateArray())
                {
                    ReadAllValue(jsonWriter, item);
                }
            }
            jsonWriter.WriteEndArray();
            jsonWriter.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            using var doc = JsonDocument.Parse(ms);
            return doc.RootElement.Clone();
        }

        private static void ReadAllValue(Utf8JsonWriter jsonWriter, JsonElement current)
        {
            if (current.ValueKind == JsonValueKind.Object)
            {
                current.WriteTo(jsonWriter);
                foreach (var item in current.EnumerateObject())
                {
                    ReadAllValue(jsonWriter, item.Value);
                }
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                current.WriteTo(jsonWriter);
                foreach (var item in current.EnumerateArray())
                {
                    ReadAllValue(jsonWriter, item);
                }
            }
            else if (current.ValueKind != JsonValueKind.Undefined)
            {
                current.WriteTo(jsonWriter);
            }
        }

        private static JsonElement ReadProperty(JsonElement current, ReadOnlySpan<byte> propertyName)
        {
            if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(propertyName, out JsonElement resut))
            {
                return resut;
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                using var ms = new MemoryStream();
                using var jsonWriter = new Utf8JsonWriter(ms);
                jsonWriter.WriteStartArray();
                foreach (var item in current.EnumerateArray())
                {
                    var value = ReadProperty(item, propertyName);
                    if (value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var obj in value.EnumerateArray())
                        {
                            obj.WriteTo(jsonWriter);
                        }
                    }
                    else
                    {
                        value.WriteTo(jsonWriter);
                    }
                }
                jsonWriter.WriteEndArray();
                jsonWriter.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                using var doc = JsonDocument.Parse(ms);
                return doc.RootElement.Clone();
            }

            return default;
        }

        private static JsonElement ReadDeepScanPropertyNext(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            if (!reader.Read()) return current;

            if (IsExpressionPartEndOrEnd(ref reader)) return current;

            if (current.ValueKind == JsonValueKind.Array)
            {
                using var ms = new MemoryStream();
                using var jsonWriter = new Utf8JsonWriter(ms, JsonOption.DefaultWriteOption);
                jsonWriter.WriteStartArray();

                var state = reader.State;
                foreach (var item in current.EnumerateArray())
                {
                    reader.Reset(state);
                    var value = reader.TokenType switch
                    {
                        JsonPathTokenType.Child => ReadChildNext(ref reader, item, root),
                        JsonPathTokenType.DeepScan => ReadDeepScanNext(ref reader, item, root),
                        JsonPathTokenType.StartFilter => ReadStartFilterNext(ref reader, item, root),
                        _ => default
                    };

                    if (value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var child in value.EnumerateArray())
                        {
                            child.WriteTo(jsonWriter);
                        }
                    }
                    else if (value.ValueKind != JsonValueKind.Undefined)
                    {
                        value.WriteTo(jsonWriter);
                    }
                }

                jsonWriter.WriteEndArray();
                jsonWriter.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                using var doc = JsonDocument.Parse(ms);
                return doc.RootElement.Clone();
            }
            return reader.TokenType switch
            {
                JsonPathTokenType.Child => ReadChildNext(ref reader, current, root),
                JsonPathTokenType.DeepScan => ReadDeepScanNext(ref reader, current, root),
                JsonPathTokenType.StartFilter => ReadStartFilterNext(ref reader, current, root),
                _ => default
            };
        }

        private static JsonElement ReadPropertyNext(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            if (!reader.Read()) return current;

            if (IsExpressionPartEndOrEnd(ref reader)) return current;

            return reader.TokenType switch
            {
                JsonPathTokenType.Child => ReadChildNext(ref reader, current, root),
                JsonPathTokenType.DeepScan => ReadDeepScanNext(ref reader, current, root),
                JsonPathTokenType.StartFilter => ReadStartFilterNext(ref reader, current, root),
                _ => default
            };
        }

        private static JsonElement ReadStartFilterNext(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            if (!reader.Read()) return default;

            var result = reader.TokenType switch
            {
                JsonPathTokenType.StartExpression => ReadStartExpressionNext(ref reader, current, root),
                JsonPathTokenType.String => ReadStringFilterNext(ref reader, current, root),
                JsonPathTokenType.Number => ReadNumberNext(ref reader, current, root),
                JsonPathTokenType.Wildcard => ReadAllChildValue(current),
                _ => default
            };

            EndFilter(ref reader);
            return ReadEndFilterNext(ref reader, result, root);
        }

        private static void EndFilter(ref JsonPathReader reader)
        {
            if (reader.TokenType == JsonPathTokenType.EndFilter) return;
            else if (reader.Read() && reader.TokenType == JsonPathTokenType.EndFilter) return;

            throw new InvalidOperationException("Filter not end");
        }

        private static JsonElement ReadEndFilterNext(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            if (reader.IsCompleted || !reader.Read()) return current;

            if (current.ValueKind == JsonValueKind.Array)
            {
                using var ms = new MemoryStream();
                using var jsonWriter = new Utf8JsonWriter(ms, JsonOption.DefaultWriteOption);
                jsonWriter.WriteStartArray();
                foreach (var item in current.EnumerateArray())
                {
                    var value = reader.TokenType switch
                    {
                        JsonPathTokenType.Child => ReadChildNext(ref reader, current, root),
                        JsonPathTokenType.DeepScan => ReadDeepScanNext(ref reader, current, root),
                        JsonPathTokenType.StartFilter => ReadStartFilterNext(ref reader, item, root),
                        _ => default
                    };

                    if (value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var child in value.EnumerateArray())
                        {
                            child.WriteTo(jsonWriter);
                        }
                    }
                    else if(value.ValueKind != JsonValueKind.Undefined)
                    {
                        value.WriteTo(jsonWriter);
                    }
                }
                jsonWriter.WriteEndArray();
                jsonWriter.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                using var doc = JsonDocument.Parse(ms);
                return doc.RootElement.Clone();
            }

            return reader.TokenType switch
            {
                JsonPathTokenType.Child => ReadChildNext(ref reader, current, root),
                JsonPathTokenType.DeepScan => ReadDeepScanNext(ref reader, current, root),
                JsonPathTokenType.StartFilter => ReadStartFilterNext(ref reader, current, root),
                _ => default
            };
        }

        private static JsonElement ReadNumberNext(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            if (IsExpressionPartEndOrEnd(ref reader)) return current;

            if (reader.TokenType != JsonPathTokenType.Number
                || current.ValueKind != JsonValueKind.Array) return default;

            var arrayLenght = current.GetArrayLength();

            var firstIndex = reader.GetInt();
            if (firstIndex < 0)
            {
                firstIndex = arrayLenght + firstIndex;
            }
            var firstPro = current[firstIndex];

            using var ms = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(ms, JsonOption.DefaultWriteOption);
            jsonWriter.WriteStartArray();
            firstPro.WriteTo(jsonWriter);

            if (!reader.Read()) return default;
            if (reader.TokenType == JsonPathTokenType.Number)
            {
                var nextInex = reader.GetInt();
                if (nextInex < 0)
                {
                    nextInex = arrayLenght + nextInex;
                }
                var nextPro = current[nextInex];
                nextPro.WriteTo(jsonWriter);
                while (reader.Read())
                {
                    if (reader.TokenType != JsonPathTokenType.Number)
                    {
                        break;
                    }

                    var index = reader.GetInt();
                    if (index < 0)
                    {
                        index = arrayLenght + index;
                    }
                    var pro = current[index];
                    pro.WriteTo(jsonWriter);
                }
            }
            else if (reader.TokenType == JsonPathTokenType.Slice && reader.Read())
            {
                if (reader.TokenType == JsonPathTokenType.EndFilter)
                {
                    for (int i = firstIndex + 1; i < arrayLenght; i++)
                    {
                        var pro = current[i];
                        pro.WriteTo(jsonWriter);
                    }
                }
                else if (reader.TokenType == JsonPathTokenType.Number)
                {
                    var index = reader.GetInt();
                    index = index < 0 ? arrayLenght + index : index;
                    for (int i = firstIndex + 1; i <= index; i++)
                    {
                        var pro = current[i];
                        pro.WriteTo(jsonWriter);
                    }
                }
            }
            jsonWriter.WriteEndArray();
            jsonWriter.Flush();
            if (reader.TokenType != JsonPathTokenType.EndFilter) return default;

            ms.Seek(0, SeekOrigin.Begin);
            var doc = JsonDocument.Parse(ms);
            return doc.RootElement.Clone();
        }

        private static JsonElement ReadStringFilterNext(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            if (IsExpressionPartEndOrEnd(ref reader)) return current;

            if (reader.TokenType != JsonPathTokenType.String) return default;
            
            var proName = reader.GetString();
            var pro = current.GetProperty(proName);

            using var ms = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(ms, JsonOption.DefaultWriteOption);
            jsonWriter.WriteStartArray();

            pro.WriteTo(jsonWriter);
            while (reader.Read())
            {
                if (reader.TokenType != JsonPathTokenType.String)
                {
                    break;
                }
                var nextProName = reader.GetString();
                var nextPro = current.GetProperty(nextProName);
                nextPro.WriteTo(jsonWriter);
            }

            if (reader.TokenType != JsonPathTokenType.EndFilter)
            {
                return default;
            }

            jsonWriter.WriteEndArray();
            jsonWriter.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            using var doc = JsonDocument.Parse(ms);
            return doc.RootElement.Clone();
        }

        private static JsonElement ReadStartExpressionNext(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            if (!reader.Read()) return default;

            JsonElement result = default;
            if (current.ValueKind == JsonValueKind.Object)
            {
                result = CaculateExpression(ref reader, current, root) ? current : default;
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                using var ms = new MemoryStream();
                using var jsonWriter = new Utf8JsonWriter(ms, JsonOption.DefaultWriteOption);

                var state = reader.State;
                jsonWriter.WriteStartArray();
                foreach (var item in current.EnumerateArray())
                {
                    reader.Reset(state);
                    if (CaculateExpression(ref reader, item, root))
                    {
                        item.WriteTo(jsonWriter);
                    }
                }

                jsonWriter.WriteEndArray();
                jsonWriter.Flush();

                ms.Seek(0, SeekOrigin.Begin);

                using var doc = JsonDocument.Parse(ms);

                result = doc.RootElement.Clone();
            }

            EndExpression(ref reader);

            return result;
        }

        private static void EndExpression(ref JsonPathReader reader)
        {
            if (reader.TokenType == JsonPathTokenType.EndExpression) return;
            else if (reader.Read() && reader.TokenType == JsonPathTokenType.EndExpression) return;

            throw new InvalidOperationException("Expression not end");
        }

        private static bool CaculateExpression(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            var left = reader.TokenType switch
            {
                JsonPathTokenType.Current => new JsonPathValue(ReadCurrentNext(ref reader, current, root)),
                JsonPathTokenType.Root => new JsonPathValue(ReadRootNext(ref reader, root, root)),
                _ => default
            };

            if (left.ValueKind == JsonPathValueKind.Undefined) return false;

            var op = reader.TokenType;
            if (op == JsonPathTokenType.EndExpression) return left.IsTrue();
            else if (op == JsonPathTokenType.Empty) return left.Empty();

            if (reader.IsCompleted || !reader.Read()) return false;

            var right = reader.TokenType switch
            {
                JsonPathTokenType.Root => new JsonPathValue(ReadRootNext(ref reader, root, root)),
                JsonPathTokenType.Current => new JsonPathValue(ReadCurrentNext(ref reader, current, root)),
                JsonPathTokenType.Number => new JsonPathValue(reader.GetDecimal()),
                JsonPathTokenType.String => new JsonPathValue(reader.GetString()),
                JsonPathTokenType.Regex => new JsonPathValue(reader.GetString()),
                _ => default
            };

            return CaculateExpression(left, right, op);
        }

        private static JsonElement ReadCurrentNext(ref JsonPathReader reader, JsonElement current, JsonElement root)
        {
            if (!reader.Read()) return default;

            if (IsExpressionPartEndOrEnd(ref reader)) return current;

            return reader.TokenType switch
            {
                JsonPathTokenType.Child => ReadChildNext(ref reader, current, root),
                JsonPathTokenType.DeepScan => ReadDeepScanNext(ref reader, current, root),
                _ => default
            };
        }

        private static JsonElement DeepScanProperty(JsonElement current, ReadOnlySpan<byte> propertyName)
        {
            using var ms = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(ms, JsonOption.DefaultWriteOption);
            jsonWriter.WriteStartArray();
            DeepScanProperty(jsonWriter, current, propertyName);
            jsonWriter.WriteEndArray();
            jsonWriter.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            using var doc = JsonDocument.Parse(ms);
            return doc.RootElement.Clone();
        }

        private static void DeepScanProperty(Utf8JsonWriter jsonWriter, JsonElement current, ReadOnlySpan<byte> propertyName)
        {
            if (current.ValueKind == JsonValueKind.Object)
            {
                if (current.TryGetProperty(propertyName, out JsonElement result))
                {
                    result.WriteTo(jsonWriter);
                }
                else
                {
                    foreach (var item in current.EnumerateObject())
                    {
                        DeepScanProperty(jsonWriter, item.Value, propertyName);
                    }
                }
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in current.EnumerateArray())
                {
                    DeepScanProperty(jsonWriter, item, propertyName);
                }
            }
        }

        private static bool CaculateExpression(JsonPathValue left, JsonPathValue right, JsonPathTokenType op)
        {
            return op switch
            {
                JsonPathTokenType.In => left.In(right),
                JsonPathTokenType.NIn => !left.In(right),
                JsonPathTokenType.Subsetof => left.SubsetOf(right),
                JsonPathTokenType.Anyof => left.AnyOf(right),
                JsonPathTokenType.Noneof => !left.AnyOf(right),
                JsonPathTokenType.Size => left.Size(right),
                JsonPathTokenType.Equal => left == right,
                JsonPathTokenType.NotEqual => left != right,
                JsonPathTokenType.LessThan => left < right,
                JsonPathTokenType.LessThanOrEqual => left <= right,
                JsonPathTokenType.GreaterThan => left > right,
                JsonPathTokenType.GreaterThanOrEqual =>left >= right,
                JsonPathTokenType.Matches => left.Matches(right),
                _ => false
            };
        }

        private static bool IsExpressionPartEndOrEnd(ref JsonPathReader reader)
        {
            return reader.TokenType == JsonPathTokenType.In
                || reader.TokenType == JsonPathTokenType.NIn
                || reader.TokenType == JsonPathTokenType.Subsetof
                || reader.TokenType == JsonPathTokenType.Anyof
                || reader.TokenType == JsonPathTokenType.Noneof
                || reader.TokenType == JsonPathTokenType.Size
                || reader.TokenType == JsonPathTokenType.Equal
                || reader.TokenType == JsonPathTokenType.NotEqual
                || reader.TokenType == JsonPathTokenType.LessThan
                || reader.TokenType == JsonPathTokenType.LessThanOrEqual
                || reader.TokenType == JsonPathTokenType.GreaterThan
                || reader.TokenType == JsonPathTokenType.GreaterThanOrEqual
                || reader.TokenType == JsonPathTokenType.Matches
                || reader.TokenType == JsonPathTokenType.EndExpression
                || reader.TokenType == JsonPathTokenType.Empty;
        }
    }
}
