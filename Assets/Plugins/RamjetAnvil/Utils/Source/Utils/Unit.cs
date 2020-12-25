namespace RamjetAnvil.Unity.Utility
{
    /// <summary>
    /// The unit value can be used as a return type like void but unlike void it can be used in a first-class way
    /// e.g. unit can be referenced.
    /// </summary>
    public class Unit
    {
        public static readonly Unit Default = new Unit();

        private Unit()
        {
        }
    }
}
