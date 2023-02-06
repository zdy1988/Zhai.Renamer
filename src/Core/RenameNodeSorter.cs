using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Zhai.Renamer.Models;

namespace Zhai.Renamer.Core
{
    internal enum RenamerNodeSortKind
    {
        Name,

        Extension,

        Size,

        CreationTime,

        ModifiedTime,
    }

    internal class RenameNodeSorter : IComparer<RenameNode>
    {
        private readonly bool isAsc = true;

        private readonly RenamerNodeSortKind kind = RenamerNodeSortKind.Name;

        public RenameNodeSorter(bool isAsc = true)
        {
            this.isAsc = isAsc;
        }

        public RenameNodeSorter(RenamerNodeSortKind kind, bool isAsc = true)
            : this(isAsc)
        {
            this.kind = kind;
        }

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string param1, string param2);

        public int Compare(RenameNode x, RenameNode y)
        {
            if (null == x && null == y)
            {
                return 0;
            }
            if (null == x)
            {
                return -1;
            }
            if (null == y)
            {
                return 1;
            }

            if (isAsc)
            {         
                return kind switch
                {
                    RenamerNodeSortKind.Name => StrCmpLogicalW(x.OriginalName, y.OriginalName),
                    RenamerNodeSortKind.Size => x.Size.CompareTo(y.Size),
                    RenamerNodeSortKind.Extension => x.Extension.CompareTo(y.Extension),
                    RenamerNodeSortKind.CreationTime => x.AllCreationTime.Any() ? x.SelectedCreationTime.CompareTo(y.SelectedCreationTime) : x.CreationTime.CompareTo(y.CreationTime),
                    RenamerNodeSortKind.ModifiedTime => x.AllModifiedTime.Any() ? x.SelectedModifiedTime.CompareTo(y.SelectedModifiedTime) : x.ModifiedName.CompareTo(y.ModifiedName),
                    _ => StrCmpLogicalW(x.OriginalName, y.OriginalName),
                };
            }
            else
            {
                return kind switch
                {
                    RenamerNodeSortKind.Name => StrCmpLogicalW(y.OriginalName, x.OriginalName),
                    RenamerNodeSortKind.Extension => y.Extension.CompareTo(x.Extension),
                    RenamerNodeSortKind.Size => y.Size.CompareTo(x.Size),
                    RenamerNodeSortKind.CreationTime => y.AllCreationTime.Any() ? y.SelectedCreationTime.CompareTo(x.SelectedCreationTime) : y.CreationTime.CompareTo(x.CreationTime),
                    RenamerNodeSortKind.ModifiedTime => y.AllModifiedTime.Any() ? y.SelectedModifiedTime.CompareTo(x.SelectedModifiedTime) : y.ModifiedName.CompareTo(x.ModifiedName),
                    _ => StrCmpLogicalW(y.OriginalName, x.OriginalName),
                };
            }
        }
    }
}
