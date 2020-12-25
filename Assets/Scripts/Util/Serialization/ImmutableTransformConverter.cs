using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Volo.Util.Serialization
{
    public class ImmutableTransformConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var transform = (ImmutableTransform)value;
            serializer.Serialize(writer, SerializeTransform(transform));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var transform = serializer.Deserialize<SerializableTransform>(reader);
            return FromSerializedTransform(transform);
        }

        public override bool CanConvert(Type objectType) {
            return typeof(ImmutableTransform).IsAssignableFrom(objectType);
        }

        private static SerializableTransform SerializeTransform(ImmutableTransform t) {
            return new SerializableTransform {
                Position = SerializeVector3(t.Position),
                Rotation = SerializeVector3(t.Rotation.eulerAngles),
                Scale = SerializeVector3(t.Scale)
            };
        }

        private static ImmutableTransform FromSerializedTransform(SerializableTransform t) {
            return new ImmutableTransform(
                position: DeserializeVector3(t.Position),
                rotation: Quaternion.Euler(DeserializeVector3(t.Rotation)),
                scale: DeserializeVector3(t.Scale));
        }

        private static float[] SerializeVector3(Vector3 v) {
            return new [] {v.x, v.y, v.z};
        }

        private static Vector3 DeserializeVector3(float[] v) {
            return new Vector3(x: v[0], y: v[1], z: v[2]);
        }
    }

    public struct SerializableTransform {
        public float[] Position { get; set; }
        public float[] Rotation { get; set; }
        public float[] Scale { get; set; }
    }

}
