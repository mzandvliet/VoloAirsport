using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using InControl;
using RamjetAnvil.DependencyInjection;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.Input;
using RamjetAnvil.Volo.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace RamjetAnvil.Volo.UI {

    public class QuickControlOverview : MonoBehaviour {

        private static readonly IList<object> RenderableActions = new object[] {
            new InputBindingId(InputBindingGroup.Wingsuit, WingsuitAction.Respawn),
            new InputBindingId(InputBindingGroup.Wingsuit, WingsuitAction.PitchUp),
            new InputBindingId(InputBindingGroup.Wingsuit, WingsuitAction.RollLeft),
            new InputBindingId(InputBindingGroup.Wingsuit, WingsuitAction.YawLeft),
            new InputBindingId(InputBindingGroup.Wingsuit, WingsuitAction.UnfoldParachute),
            new InputBindingId(InputBindingGroup.Parachute, ParachuteAction.PullLeftLines),
            new InputBindingId(InputBindingGroup.Parachute, ParachuteAction.PullRightLines),
            new InputBindingId(InputBindingGroup.Parachute, ParachuteAction.HoldFrontLines),
            new InputBindingId(InputBindingGroup.Parachute, ParachuteAction.HoldRearLines),
            new InputBindingId(InputBindingGroup.Menu, MenuAction.Pause)
        };

        [Dependency, SerializeField] private InputMappingsViewModel _inputMappingsViewModel;
        [SerializeField] private Text _overviewText;

        private MutableString _overviewStr;

        void Awake() {
            _overviewStr = new MutableString(1024);
            var renderableBindings = _inputMappingsViewModel
                .InputMappings
                .Select(bindings => {
                    return bindings
                        .Where(binding => RenderableActions.Contains(binding.Id))
                        .OrderBy(binding => RenderableActions.IndexOf(binding.Id));
                });

            renderableBindings.Subscribe(bindings => {
                _overviewStr.Clear();
                foreach (var binding in bindings) {
                    _overviewStr
                        .Append("<i>")
                        .Append(binding.Group)
                        .Append("</i>")
                        .Append(" - ")
                        .Append(binding.Name)
                        .Append(": <b>")
                        .Append(binding.Binding)
                        .Append("</b>")
                        .Append(Environment.NewLine);
                }
                _overviewText.SetMutableString(_overviewStr);
            });
        }


    }
}
