using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RamjetAnvil.Settings
{
    public class EditorPrefsSerializer : ISettingsSerializer
    {
        public void Save(object settings, string prefsKey)
        {
            System.Type type = settings.GetType();
            XmlSerializer serializer = new XmlSerializer(type);
            System.Text.StringBuilder xmlBuilder = new System.Text.StringBuilder();
            XmlWriter writer = XmlWriter.Create(xmlBuilder);
            serializer.Serialize(writer, settings);
            writer.Close();

            EditorPrefs.SetString(prefsKey, xmlBuilder.ToString());
        }

        public object Load(Type type, string prefsKey)
        {
            XmlSerializer serializer = new XmlSerializer(type);

            object settings = null;

            try
            {
                string xmlString = EditorPrefs.GetString(prefsKey);
                StringReader stringReader = new StringReader(xmlString);
                settings = serializer.Deserialize(stringReader);
            }
            catch (System.Exception e) // Todo: sigh, be more specific
            {
                Debug.LogWarning(type.ToString() + " settings could not be loaded.\n" + e.Message);
            }

            return settings;
        }
    }
}