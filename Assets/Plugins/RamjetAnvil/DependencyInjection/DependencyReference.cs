namespace RamjetAnvil.DependencyInjection
{
    public struct DependencyReference {
        public readonly string Name;
        public readonly object Instance;

        public DependencyReference(string name, object instance) {
            Name = name;
            Instance = instance;
        }
    }
}
