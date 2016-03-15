using System;

namespace RowShareTool.Model
{
    [Flags]
    public enum ListAccessMode
    {

        Unspecified = 0,

        PublicRead = 1,

        PublicWrite = 2,

        PublicWriteOwnedOnly = 4,

        PublicNone = 8,

        PublicReadOwnedOnly = 16,

        PublicReadWriteOwnedOnly = 32,
    }
}
