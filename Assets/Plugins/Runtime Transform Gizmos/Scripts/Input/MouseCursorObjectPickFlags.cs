using System;
namespace RTEditor
{
    [Flags]
    public enum MouseCursorObjectPickFlags
    {
        None = 0,
        ObjectBox = 0x1,
        ObjectMesh = 0x2,
        ObjectSprite = 0x4,
        ObjectTerrain = 0x8
    }
}