using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;

/*
 * Todo:
 * - This is currently only really useful for managers (singletons). Instead, let users of
 * this class specify filenames and locations manually.
 * - Keeping track of settingsChanged here is stupid, it has nothing to do with serialization
 */
namespace RamjetAnvil.Settings
{
    public interface ISettingsSerializer
    {
        void Save(object settings, string path);
        object Load(Type type, string path);
    }

    public class XmlFileSerializer : ISettingsSerializer
    {
        public void Save(object settings, string path)
        {
            
        }

        public object Load(Type type, string path) {
            return null;
        }
    }

    public class PlayerPrefsSerializer : ISettingsSerializer
    {
        public void Save(object settings, string prefsKey)
        {
            System.Type type = settings.GetType();
            XmlSerializer serializer = new XmlSerializer(type);
            System.Text.StringBuilder xmlBuilder = new System.Text.StringBuilder();
            XmlWriter writer = XmlWriter.Create(xmlBuilder);
            serializer.Serialize(writer, settings);
            writer.Close();

            PlayerPrefs.SetString(prefsKey, xmlBuilder.ToString());
        }

        public object Load(Type type, string prefsKey)
        {
            XmlSerializer serializer = new XmlSerializer(type);

            object settings = null;

            try
            {
                string xmlString = PlayerPrefs.GetString(prefsKey);
                StringReader stringReader = new StringReader(xmlString);
                settings = serializer.Deserialize(stringReader);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(type.ToString() + " settings could not be loaded.\n" + e.ToString());
            }

            return settings;
        }
    }
}