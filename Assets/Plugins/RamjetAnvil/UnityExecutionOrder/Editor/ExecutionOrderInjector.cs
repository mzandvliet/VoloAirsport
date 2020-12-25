using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityExecutionOrder {

    [InitializeOnLoad]
    public class ExecutionOrderInjector {

        private const string ExecutionOrderPath = "./.execution_order_cache";
        private const int StartOrderIndex = 100;

        static ExecutionOrderInjector() {
            // TODO Maybe use Assembly class instead as it is more reliable
            var monoScripts = MonoImporter.GetAllRuntimeMonoScripts()
                .Where(script => script.GetClass() != null)
                .Distinct(MonoScriptComparer.Default)
                .ToDictionary(script => script.GetClass());
            var executionOrder = ExecutionOrder.GetOrder(ExecutionOrder.DependencyList(monoScripts.Keys));

            var serializedExecutionOrder = ExecutionOrder.DeserializeExecutionOrder(monoScripts, ExecutionOrderPath);

            if (!executionOrder.SequenceEqual(serializedExecutionOrder)) {
                Debug.Log("UnityExecutionOrder - Setting script execution order: " + executionOrder.JoinToString(" -> "));

                for (int i = 0; i < executionOrder.Count; i++) {
                    var scriptType = executionOrder[i];
                    MonoImporter.SetExecutionOrder(monoScripts[scriptType], order: StartOrderIndex + i);
                }

                ExecutionOrder.SerializeExecutionOrder(ExecutionOrderPath, executionOrder);
            }
        }
    }
}
