using Zhai.Renamer.Model;

namespace Zhai.Renamer.Core
{
    internal static class ModifierExplainer
    {
        internal static string Explain(ModifierKind modifierKind)
        {
            return modifierKind switch
            {
                ModifierKind.Clear => "清空名称的所有字符",
                ModifierKind.AppendBefore => "在名称前追加 {0}",
                ModifierKind.AppendAfter => "在名称后追加 {0}",
                ModifierKind.AppendAtPosition => "在名称位置 {1} 处追加 {0}",
                ModifierKind.CapitalizeEachWord => "将名称中的英文字符按单词转化为大写",
                ModifierKind.UpperCase => "将名称中的英文字符转化为大写",
                ModifierKind.LowerCase => "将名称中的英文字符转化为小写",
                ModifierKind.SentenceCase => "将名称中的英文字符转化为驼峰形式",

                ModifierKind.AddNumbering => "在名称位置 {0} 处增加编号",
                ModifierKind.NumberByDirectories => "在名称位置 {0} 处增加编号 （基于文件夹）",
                ModifierKind.AddMultipleNumbering => "在名称位置 {0} 处增加 {1} 个编号",
                ModifierKind.SwapOrder => "在名称位置 {0} 处增加编号（基偶行交换）",

                ModifierKind.KeepNumeric => "只保留名称中的数字字符",
                ModifierKind.KeepAlphanumeric => "只保留名称中的字母、数字和空格符",
                ModifierKind.RemoveInvalidCharacters => "去掉除空格、句点(.)、at符号(@)和连字符(-)以外的标点字符",
                ModifierKind.PreserveFromLeft => "保留名称左边 {0} 个字符",
                ModifierKind.PreserveFromRight => "保留名称右边 {0} 个字符",
                ModifierKind.TrimFromLeft => "去掉名称左边 {0} 个字符",
                ModifierKind.TrimFromRight => "去掉名称右边 {0} 个字符",
                ModifierKind.Substring => "从名称位置 {0} 处保留 {1} 个字符",
                ModifierKind.RemoveSubstring => "从位置 {0} 处去掉 {1} 个字符",

                ModifierKind.Regex => "只保留匹配 {0} 的名称",
                ModifierKind.RegexReplace => "将名称匹配 {0} 的字符替换为 {1}",
                ModifierKind.ReplaceString => "将名称中的 {0} 替换为 {1}",
                ModifierKind.ReplaceCaseInsensitive => "将名称中的 {0} （忽略大小写）替换为 {1}",

                ModifierKind.RemoveTimeString => "去掉名称中的日期字符串",
                ModifierKind.FormatTimeString => "使用 {0} 格式化名称中的日期字符串",
                ModifierKind.KeepTimeString => "提取名称中的日期字符串后附加到位置 {0} 处",

                ModifierKind.AppendFromDirectory => "在名称位置 {1} 处追加指定文件夹 {0} 中的文件名称",
                ModifierKind.AppendFromTextFile => "在名称位置 {1} 处追加指定文本文件 {0} 中的字符",
                ModifierKind.ParentDirectory => "在名称位置 {0} 处追加其所在目录名称",
                ModifierKind.OriginalFileName => "在名称位置 {0} 处追加原始文件名称",
                ModifierKind.AddExtension => "在名称后追加文件后缀名",
                ModifierKind.RemoveExtension => "去掉名称中的文件后缀名",

                ModifierKind.AppendCountingFileQuantityAfter => "在名称后追加 [文件数量]",
                ModifierKind.AppendCountingFileQuantity => "在名称位置 {1} 处追加 [文件数量]",
                ModifierKind.AppendCountingCreationTimeAfter => "在名称后追加 [创建时间]",
                ModifierKind.AppendCountingCreationTime => "在名称位置 {1} 处追加 [创建时间]",
                ModifierKind.AppendCountingModifiedTimeAfter => "在名称后追加 [修改时间]",
                ModifierKind.AppendCountingModifiedTime => "在名称位置 {1} 处追加 [修改时间]",

                _ => "",
            };
        }
    }
}
