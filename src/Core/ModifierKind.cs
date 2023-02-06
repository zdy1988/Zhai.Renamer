namespace Zhai.Renamer.Core
{
    //Each modifier has 3 parts (in this file)
    //1 enum, 1 condition inside the constructor and 1 condition inside the switch
    //If some operation like file listing is needed, another condition must be created at the end of the constructor (like AppendFromTextFile)
    public enum ModifierKind
    {
        Clear,

        AppendBefore,
        AppendAfter,
        AppendAtPosition,

        CapitalizeEachWord,
        UpperCase,
        LowerCase,
        SentenceCase,

        AddNumbering,
        NumberByDirectories,
        AddMultipleNumbering,
        SwapOrder,

        KeepNumeric,
        KeepAlphanumeric,
        RemoveInvalidCharacters,

        PreserveFromLeft,
        PreserveFromRight,
        TrimFromLeft,
        TrimFromRight,

        Substring,
        RemoveSubstring,

        Regex,
        RegexReplace,
        ReplaceString,
        ReplaceCaseInsensitive,

        RemoveTimeString,
        FormatTimeString,
        KeepTimeString,

        AppendFromDirectory,
        AppendFromTextFile,

        ParentDirectory,
        OriginalFileName,
        AddExtension,
        RemoveExtension,

        AppendCountingFileQuantityAfter,
        AppendCountingFileQuantity,
        AppendCountingCreationTimeAfter,
        AppendCountingCreationTime,
        AppendCountingModifiedTimeAfter,
        AppendCountingModifiedTime,
    }
}
