using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zhai.Renamer.Models;

namespace Zhai.Renamer.Core
{
    internal class ModifierManager
    {
        internal List<RenameModifier> ModifierSettings { get; private set; }

        private IList<RenameNode> RenameNodeList { get; }

        private IList<RenameModifier> ModifierList { get; }

        internal ModifierManager(IList<RenameNode> renameNodes, IList<RenameModifier> modifiers)
        {
            RenameNodeList = renameNodes ?? new List<RenameNode>();

            ModifierList = modifiers ?? new List<RenameModifier>();

            InitModifierSettings();
        }

        private void InitModifierSettings()
        {
            // Initialize Modifier Settings
            ModifierSettings = new List<RenameModifier>();

            foreach (var item in Enum.GetValues(typeof(ModifierKind)))
            {
                var modifierItem = new RenameModifier((ModifierKind)item);

                modifierItem.RenamerModifierUpdated += (sender, e) =>
                {
                    PreviewModifier();
                };

                ModifierSettings.Add(modifierItem);
            }

            InitModifierTimer();
        }

        internal void ResetModifierSettings()
        {
            foreach (var modifier in ModifierSettings)
            {
                modifier.IsUsed = false;
                modifier.FirstArgument = null;
                modifier.SecondArgument = null;
            }
        }

        internal void AttachModifierSettings(IList<RenameModifier> modifiers)
        {
            foreach (var modifier in ModifierSettings)
            {
                if (modifier.IsUsed)
                {
                    modifiers.Add(new RenameModifier(modifier.ModifierKind, modifier.FirstArgument, modifier.SecondArgument));
                }
            }
        }

        #region Preview Modifiers

        private System.Timers.Timer modifierTimer;

        private CancellationTokenSource modifierTimerTokenSource;

        private void InitModifierTimer()
        {
            modifierTimer = new System.Timers.Timer(500);

            modifierTimer.Elapsed += (sender, e) =>
            {
                if (this.modifierTimerTokenSource != null)
                {
                    this.modifierTimerTokenSource.Cancel();
                    this.modifierTimerTokenSource = null;
                }

                var localTokenSource = new CancellationTokenSource();

                this.modifierTimerTokenSource = localTokenSource;

                Task.Factory.StartNew(() =>
                {
                    var copyModifiers = new List<RenameModifier>(ModifierList);

                    AttachModifierSettings(copyModifiers);

                    ApplyModifier(copyModifiers, localTokenSource);

                }, this.modifierTimerTokenSource.Token).Wait();

                modifierTimer.Enabled = false;

                ModifierPreviewed?.Invoke(this, null);
            };
        }

        private void ApplyModifier(IList<RenameModifier> modifiers, CancellationTokenSource localTokenSource)
        {
            foreach (var modifier in modifiers)
            {
                modifier.Reset();
            }

            try
            {
                for (int i = 0; i < RenameNodeList.Count; i++)
                {
                    localTokenSource.Token.ThrowIfCancellationRequested();

                    var renameNode = RenameNodeList[i];

                    renameNode.Reset();

                    foreach (var modifier in modifiers)
                    {
                        if (!modifier.TryApply(renameNode, i, RenameNodeList.Count))
                        {
                            return;
                        }
                    }
                }
            }
            finally
            {
                if (this.modifierTimerTokenSource == localTokenSource)
                {
                    this.modifierTimerTokenSource = null;
                }

                localTokenSource.Dispose();
            }
        }

        internal void PreviewModifier()
        {
            if (!modifierTimer.Enabled)
            {
                modifierTimer.Enabled = true;
            }
            else
            {
                modifierTimer.Enabled = false;
                modifierTimer.Enabled = true;
            }

            ModifierPreviewing?.Invoke(this, null);
        }


        #endregion

        #region Event

        public event EventHandler ModifierPreviewing;

        public event EventHandler ModifierPreviewed;

        #endregion
    }
}
