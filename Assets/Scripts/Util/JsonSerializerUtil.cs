using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RamjetAnvil.Util {
    public class TypedJsonDeserializer {
        private readonly Type[] _types;

        public TypedJsonDeserializer(params Type[] types) {
            _types = types;
        }

        public object Deserialize(object o) {
            if (o is JObject) {
                foreach (var type in _types) {
                    try {
                        return ((JObject) o).ToObject(type);
                    }
                    catch (Exception) {}
                }
                throw new Exception("No types were provided for " + o);
            }
            if (o is string) {
                return TryParseEnum((string) o);
            }
            if (o is Int64 || o is Int32 || o is Int16) {
                return Convert.ToInt32(o);
            }
            return o;
            //throw new Exception("Cannot deserialize object: " + o);
        }

        private object TryParseEnum(string o) {
            foreach (var type in _types) {
                try {
                    return Enum.Parse(type, o);
                }
                catch (Exception) {
                    //Debug.LogException(e);
                }
            }
            return o;
        }
    }

    public static class JsonSerializerUtil {
        public static string Serialize2String(this JsonSerializer serializer, object value) {
            using (var writer = new StringWriter()) {
                serializer.Serialize(writer, value);
                writer.Flush();
                return writer.ToString();
            }
        }

        public static T DeserializeFromFile<T>(this JsonSerializer serializer, string filePath) {
            using (var fileReader = new FileStream(filePath, FileMode.Open))
            using (var jsonReader = new JsonTextReader(new StreamReader(fileReader))) {
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        public static void SerializeToFile<T>(this JsonSerializer serializer, string filePath, T o) {
            using (var fileReader = new FileStream(filePath, FileMode.Create))
            using (var jsonWriter = new JsonTextWriter(new StreamWriter(fileReader))) {
                serializer.Serialize(jsonWriter, o);
            }
        }

        public static T Deserialize<T>(this JsonSerializer serializer, string value) {
            using (var reader = new StringReader(value)) {
                using (var jsonReader = new JsonTextReader(reader)) {
                    return serializer.Deserialize<T>(jsonReader);
                }
            }
        }
    }
}