using System.Collections.Generic;
using RamjetAnvil.Coroutine;

public interface IModule {
    IEnumerator<WaitCommand> Load();

    IEnumerator<WaitCommand> Run();
}
