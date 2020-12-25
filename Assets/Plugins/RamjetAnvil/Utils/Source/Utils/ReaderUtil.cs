using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Unity.Utility {
    public static class ReaderUtil {
        /// <summary>
        /// Falls back to the second reader if creating the first one gives a file not found exception.
        /// </summary>
        /// <returns>Either of the two readers</returns>
        public static T FallbackReader<T>(params Func<T>[] readers) {
            foreach (var reader in readers) {
                try {
                    return reader();
                } catch (Exception e) {
                    Debug.LogWarning("Read failed, trying alternative. Reason: " + e.Message);
                }
                
            }
            throw new Exception("No suitable reader found.");
        }
    }
}
