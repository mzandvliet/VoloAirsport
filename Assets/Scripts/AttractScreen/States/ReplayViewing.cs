using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RamjetAnvil.Coroutine;
using RamjetAnvil.StateMachine;

namespace RamjetAnvil.Volo.States {

    public class ReplayViewing : State {
        public ReplayViewing(IStateMachine machine) : base(machine) {}

        IEnumerator<WaitCommand> OnEnter() {
            // Enable the replay pilot
            // Switch to active replay pilot mount
            // Activate the replay viewer menu
            yield return WaitCommand.DontWait;
        }

        IEnumerator<WaitCommand> OnExit() {
            // Disable the replay pilot (or at least make it invisible)
            // De-activate the replay viewer menu
            yield return WaitCommand.DontWait;
        }
    }
}
