using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RamjetAnvil.Impero.Util;
using UnityEngine;

namespace RamjetAnvil.Volo {

    public class LanguageTable {
        private readonly IImmutableDictionary<string, string> _table;
        private readonly Func<string, string> _asFunc; 

        public LanguageTable(IImmutableDictionary<string, string> table) {
            _table = table;
            _asFunc = key => {
                string value;
                if (_table.TryGetValue(key, out value)) {
                    return value;
                }
                return key;
            };
        }

        public IImmutableDictionary<string, string> Table {
            get { return _table; }
        }

        public Func<string, string> AsFunc {
            get { return _asFunc; }
        }
    }

    public class LanguageMetaInfo {
        private readonly CultureCode _cultureCode;
        private readonly string _name;
        private readonly string _englishName;
        private readonly CultureInfo _cultureInfo;

        public LanguageMetaInfo(CultureCode cultureCode, string name, string englishName, CultureInfo cultureInfo) {
            _cultureCode = cultureCode;
            _name = name;
            _englishName = englishName;
            _cultureInfo = cultureInfo;
        }

        public CultureCode CultureCode {
            get { return _cultureCode; }
        }

        public string Name {
            get { return _name; }
        }

        public string EnglishName {
            get { return _englishName; }
        }

        public CultureInfo CultureInfo {
            get { return _cultureInfo; }
        }
    }

    public class Languages {
        private readonly IImmutableDictionary<CultureCode, LanguageTable> _languageTables;
        private readonly IImmutableDictionary<CultureCode, LanguageMetaInfo> _metaInfo;
        private readonly List<CultureCode> _cultureCodes; 

        public Languages(IImmutableDictionary<string, LanguageTable> table) {
            var availableLanguages = new Dictionary<CultureCode, LanguageMetaInfo>();
            var languageTables = new Dictionary<CultureCode, LanguageTable>();
            _cultureCodes = new List<CultureCode>();
            foreach (var kvPair in table) {
                var languageCode = kvPair.Key;
                var languageTable = kvPair.Value;
                var cultureCode = new CultureCode(languageCode, languageTable.Table["culture"]);
                availableLanguages[cultureCode] = new LanguageMetaInfo(
                    cultureCode, 
                    languageTable.Table["language_name"], 
                    languageTable.Table["english_language_name"],
                    new CultureInfo(languageCode + "-" + languageTable.Table["culture"]));
                languageTables[cultureCode] = languageTable;
                _cultureCodes.Add(cultureCode);
            }
            _languageTables = languageTables.ToImmutableDictionary();
            _metaInfo = availableLanguages.ToImmutableDictionary();
            _cultureCodes.Sort();
        }

        public IImmutableDictionary<CultureCode, LanguageTable> LanguageTables {
            get { return _languageTables; }
        }

        public IImmutableDictionary<CultureCode, LanguageMetaInfo> MetaInfo {
            get { return _metaInfo; }
        }

        public IList<CultureCode> CultureCodes {
            get { return _cultureCodes; }
        }
    }

    public struct CultureCode : IEquatable<CultureCode>, IComparable<CultureCode> {
        public readonly string LanguageCode;
        public readonly string RegionCode;

        public CultureCode(string languageCode, string regionCode) {
            LanguageCode = languageCode;
            RegionCode = regionCode;
        }

        public bool Equals(CultureCode other) {
            return string.Equals(LanguageCode, other.LanguageCode) && string.Equals(RegionCode, other.RegionCode);
        }

        public int CompareTo(CultureCode other) {
            return String.Compare(ToString(), other.ToString(), StringComparison.Ordinal);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CultureCode && Equals((CultureCode) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((LanguageCode != null ? LanguageCode.GetHashCode() : 0) * 397) ^ (RegionCode != null ? RegionCode.GetHashCode() : 0);
            }
        }

        public static bool operator ==(CultureCode left, CultureCode right) {
            return left.Equals(right);
        }

        public static bool operator !=(CultureCode left, CultureCode right) {
            return !left.Equals(right);
        }

        public override string ToString() {
            return LanguageCode + "-" + RegionCode;
        }

        public static CultureCode FromString(string culture) {
            var parts = culture.Split('-');
            return new CultureCode(languageCode: parts[0], regionCode: parts[1]);
        }
    }

    public class LanguageSettings {

        public static readonly Lazy<string> LanguagesPath = new Lazy<string>(() => 
            Path.Combine(Application.streamingAssetsPath, "languages.json"));

        public static readonly Lazy<JsonSerializer> JsonSerializer = new Lazy<JsonSerializer>(() => new JsonSerializer());

        public static Languages ReadLanguages() {
            using (var s = new StreamReader(LanguagesPath.Value, Encoding.UTF8))
            using (var reader = new JsonTextReader(s)) {
                var langs = JsonSerializer.Value.Deserialize<IImmutableDictionary<string, IImmutableDictionary<string, string>>>(reader);
                return new Languages(langs.ChangeValues(langDict => new LanguageTable(langDict)));
            }
        }

    }
}
