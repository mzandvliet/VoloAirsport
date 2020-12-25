using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;

namespace UnityExecutionOrder {

    public static class ExecutionOrder {

        public static IDictionary<Type, IList<Run.Before>> DependencyList(IEnumerable<Type> allTypes) {
            var typesWithDeps = allTypes
                .Select(type => new {type, dependencies = Dependencies(type)})
                .Where(typeWithDeps => typeWithDeps.dependencies.Any())
                .ToDictionary(typeWithDeps => typeWithDeps.type, typeWithDeps => typeWithDeps.dependencies.ToList());
            foreach (var type in typesWithDeps.Keys.ToList()) {
                var dependencies = typesWithDeps[type];
                var runAfterDeps = (from dependency in dependencies
                     where dependency is Run.After
                     select dependency as Run.After).ToList();
                foreach (var runAfterDep in runAfterDeps) {
                    if (!typesWithDeps.ContainsKey(runAfterDep.Type)) {
                        typesWithDeps.Add(runAfterDep.Type, new List<Run>());
                    }
                    typesWithDeps[runAfterDep.Type].Add(new Run.Before(type));
                    dependencies.Remove(runAfterDep);
                }
            }
            
            var depList = typesWithDeps.ToDictionary(
                kvPair => kvPair.Key,
                kvPair => {
                    var dependencies = kvPair.Value
                        .Select(a => a as Run.Before)
                        .Distinct()
                        .ToList();
                    return dependencies as IList<Run.Before>;
                });
            return new SortedDictionary<Type, IList<Run.Before>>(depList, TypeComparer.Default);
        }

        public static IList<Type> GetOrder(IDictionary<Type, IList<Run.Before>> typesWithDependencies) {
            var visitedTypes = new HashSet<Type>();
            var order = new List<Type>();
            Action<IEnumerable<Type>, Type> addType = null;
            addType = (dependencyTracker, type) => {
                if (dependencyTracker.Contains(type)) {
                    throw new Exception("Circular reference detected in dependency chain: " +
                                        dependencyTracker.JoinToString(" -> "));
                } 
                
                if (!visitedTypes.Contains(type)) {
                    visitedTypes.Add(type);

                    IList<Run.Before> dependencies;
                    if (!typesWithDependencies.TryGetValue(type, out dependencies)) {
                        dependencies = new List<Run.Before>();
                    }

                    if (dependencies.Count > 0) {
                        foreach (var dependency in dependencies) {
                            addType(dependencyTracker.Append(type), dependency.Type);
                        }

                        var index = order.Aggregate(order.Count - 1, (lowestIndex, t) => {
                            var typeIndex = order.IndexOf(t);
                            return typeIndex < lowestIndex ? typeIndex : lowestIndex;
                        });
                        order.Insert(index, type);
                    } else {
                        order.Add(type);
                    }
                }
            };

            foreach (var type in typesWithDependencies.Keys) {
                addType(Enumerable.Empty<Type>(), type);
            }
            return order.ToList();
        }

        public static IEnumerable<Run> Dependencies(Type type) {
            return type.GetCustomAttributes(typeof (Run), inherit: true).Cast<Run>().ToList();
        }

        public static IList<Type> DeserializeExecutionOrder(IDictionary<Type, MonoScript> monoScripts, string path) {
            try {
                var monoScriptsByString = monoScripts.ToDictionary(kvPair => kvPair.Key.FullName, kvPair => kvPair.Value);
                using (var fileReader = new FileStream(path, FileMode.Open))
                using (var streamReader = new StreamReader(fileReader, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))){
                    var serializer = new XmlSerializer(typeof (List<string>));
                    return (serializer.Deserialize(streamReader) as List<string>)
                        .Where(serializedType => monoScriptsByString.ContainsKey(serializedType))
                        .Select(serializedType => monoScriptsByString[serializedType].GetClass())
                        .ToList();
                }
            } catch (FileNotFoundException) {
                return new List<Type>();
            }
        }

        public static void SerializeExecutionOrder(string path, IList<Type> executionOrder) {
            using (var fileWriter = new FileStream(path, FileMode.OpenOrCreate))
            using (var streamWriter = new StreamWriter(fileWriter, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))){
                var serializer = new XmlSerializer(typeof (List<string>));
                serializer.Serialize(streamWriter, executionOrder.Select(type => type.FullName).ToList());    
            }
        }
    }
}
