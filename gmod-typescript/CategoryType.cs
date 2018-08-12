using System;
namespace gmod_typescript
{
    [Flags]
    public enum CategoryType
    {
        None = 0x0,
        Class = 0x1,
        Library = 0x2,
        Global = 0x4,
        Interface = 0x8
    }
}
