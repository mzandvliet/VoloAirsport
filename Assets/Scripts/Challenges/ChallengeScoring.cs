using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public static class ChallengeScoring {

        public class Score {
            public int Value;

            public Score(int value) {
                Value = value;
            }

            public override string ToString() {
                return string.Format("Score: {0}", Value);
            }
        }

        public static readonly Lazy<string> ScoreStoragePath = new Lazy<string>(() => {
            return Path.Combine(VoloAirsportFileStorage.StorageDir.Value, "ChallengeScores.json");
        });
        private static readonly JsonSerializer Json = new JsonSerializer();
        
        public static void WriteScores(Score score) {
            using (var fileStream = new FileStream(ScoreStoragePath.Value, FileMode.Create))
            using (var cryptoProvider = new DESCryptoServiceProvider().CreateEncryptor(rgbKey: Encoding.ASCII.GetBytes("A07cDjnN"), rgbIV: Encoding.ASCII.GetBytes("0MhNCX3445hDSJ1F")))
            using (var cryptoStream = new CryptoStream(fileStream, cryptoProvider, CryptoStreamMode.Write))

            // Allow json to stream its contents to the cryptostream
            using (var textWriter = new StreamWriter(cryptoStream))
            using (var jsonWriter = new JsonTextWriter(textWriter)) {
                Json.Serialize(jsonWriter, score);
            }
        }

        public static Score ReadScores() {
            using (var fileStream = new FileStream(ScoreStoragePath.Value, FileMode.Open))
            using (var cryptoProvider = new DESCryptoServiceProvider().CreateDecryptor(rgbKey: Encoding.ASCII.GetBytes("A07cDjnN"), rgbIV: Encoding.ASCII.GetBytes("0MhNCX3445hDSJ1F")))
            using (var cryptoStream = new CryptoStream(fileStream, cryptoProvider, CryptoStreamMode.Read))

            using (var textReader = new StreamReader(cryptoStream))
            using (var jsonReader = new JsonTextReader(textReader)) {
                return Json.Deserialize<Score>(jsonReader);
            }
        }
 
    }
}
