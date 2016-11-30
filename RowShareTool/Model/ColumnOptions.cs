using System;

namespace RowShareTool.Model
{
    [Flags]
    public enum ColumnOptions
    {

        None = 0,

        IsMandatory = 1,

        IsComputed = 2,

        IsHyperlink = 4,

        IsCurrency = 8,

        IsRichText = 16,

        IsLookupMultiValued = 32,

        IsUnique = 64,

        IsEmail = 128,

        IsFrequent = 256,

        IsForOrganizations = 512,

        IsImage = 1024,

        IsHidden = 2048,

        DoNotShowThousandsSeparator = 4096,

        IsPercentage = 8192,

        IsTask = 16384,

        IsVote = 32768,

        IsUserHidden = 65536,

        IsUserReadOnly = 131072,
    }
}
