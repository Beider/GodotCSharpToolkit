using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{
    /// <summary>
    /// Used to quickly build data editors for your games
    /// </summary>
    public partial class EditorMainScene : Control, IDataEditor
    {
        private const int PREF_SAVE_INTERVAL = 5;

        /// <summary>
        /// Called when data is saved
        /// </summary>
        public event Action OnDataSaved = delegate { };
        public event Action OnDataRefreshed = delegate { };
        public event Func<bool> HasUnsavedChanges = null;

        /// <summary>
        /// Called to open search dialog
        /// </summary>
        public event Action<IDataEditor> OnOpenSearchDialog = delegate { };

        /// <summary>
        /// Sent when editor is about to be closed
        /// Return true to free editor, false if you dont want editor to be free
        /// </summary>
        public Func<IDataEditor, bool> OnEditorCloseRequest;

        /// <summary>
        /// Called when a module is pressed
        /// </summary>
        public event Action<string, DelegateEditorTreeItem, IDataEditor> OnModuleTreeItemPressed = delegate { };

        private Control EditorArea;
        private HSplitContainer EditorTreeSplit;
        public EditorPrefsExtended Preferences { get; private set; } = new EditorPrefsExtended();
        private Timer SaveTimer;
        public EditorTreeView Tree { get; private set; }
        public EditorToolbar Toolbar { get; private set; }
        public PopupMenu PopupMenu { get; private set; }
        public Dictionary<string, Action> PopupMenuDelegates = new Dictionary<string, Action>();

        private Control CurrentDialog = null;

        private IDataEditorContent ActiveEditor = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Tree = FindNode("EditorTreeView") as EditorTreeView;
            EditorArea = FindNode("EditorArea") as Control;
            Toolbar = FindNode("Toolbar") as EditorToolbar;
            PopupMenu = FindNode("PopupMenu") as PopupMenu;
            EditorTreeSplit = FindNode("EditorTreeSplit") as HSplitContainer;
            PopupMenu.Connect("id_pressed", this, nameof(OnPopupMenuPressed));

            Tree.Init(this);
            Toolbar.Init(this);

            EditorTreeSplit.SplitOffset = Preferences.SettingEditorSplitOffset;
            EditorTreeSplit.Connect("dragged", this, nameof(SplitDragged));

            // Start preferences save timer
            SaveTimer = new Timer();
            SaveTimer.OneShot = false;
            SaveTimer.WaitTime = PREF_SAVE_INTERVAL;
            SaveTimer.Connect("timeout", this, nameof(SavePreferences));
            AddChild(SaveTimer);
            SaveTimer.Start();
        }

        private void SplitDragged(int offset)
        {
            Preferences.SettingEditorSplitOffset = offset;
        }

        private void SavePreferences()
        {
            Preferences.Save();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (CtrlOrCmd(keyEvent) && keyEvent.Scancode == (int)KeyList.S)
                {
                    Save();
                }
                else if (CtrlOrCmd(keyEvent) && keyEvent.Scancode == (int)KeyList.N)
                {
                    Toolbar.OnNewModPressed();
                }
                else if (CtrlOrCmd(keyEvent) && keyEvent.Scancode == (int)KeyList.R)
                {
                    Toolbar.OnRefreshPressed();
                }
                else if (keyEvent.Scancode == (int)KeyList.F1)
                {
                    Toolbar.OnSortPressed();
                }
                else if (keyEvent.Scancode == (int)KeyList.F2)
                {
                    Toolbar.OnToggleNamesPressed();
                }
                else if (keyEvent.Scancode == (int)KeyList.F3)
                {
                    Toolbar.OnLocalOnlyPressed();
                }
            }
        }

        private bool CtrlOrCmd(InputEventKey keyEvent)
        {
            return keyEvent.Control || keyEvent.Command;
        }

        public void Refresh(bool askForSave = true)
        {
            if (askForSave && _HasUnsavedChanges())
            {
                ShowConfirmDialog("You have unsaved changes, would you like to save before you refresh?", shouldSave => { if (shouldSave) { Save(); } else { _Refresh(); } });
            }
            else
            {
                _Refresh();
            }
        }

        public void Close()
        {
            if (_HasUnsavedChanges())
            {
                ShowConfirmDialog("You have unsaved changes, would you like to save before you exit?", shouldSave => { Close(shouldSave); });
            }
            else
            {
                Close(false);
            }
        }

        private bool _HasUnsavedChanges()
        {
            return Tree.HasUnsavedChanges() || (HasUnsavedChanges != null && HasUnsavedChanges());
        }

        public void Close(bool save)
        {
            if (save) { Save(); }
            if (OnEditorCloseRequest == null || OnEditorCloseRequest(this))
            {
                GetParent().RemoveChild(this);
            }
        }

        public void Save()
        {
            if (CurrentDialog != null && IsInstanceValid(CurrentDialog) && CurrentDialog.Visible)
            {
                return;
            }
            Tree.Save();
            OnDataSaved();
            _Refresh();
        }

        private void _Refresh()
        {
            if (CurrentDialog != null && IsInstanceValid(CurrentDialog) && CurrentDialog.Visible)
            {
                return;
            }
            string uniqueId = "";
            if (ActiveEditor != null && IsInstanceValid(((Control)ActiveEditor)))
            {
                uniqueId = ActiveEditor.GetUniqueId();
                ActiveEditor.QueueFree();
            }
            OnDataRefreshed();
            Tree.RefreshTree(true, uniqueId);
            Toolbar.Init(this);
        }

        public void NotifyOpenSearchDialog()
        {
            OnOpenSearchDialog(this);
        }

        public void NotifyOnModuleTreeItemPressed(string name, DelegateEditorTreeItem item)
        {
            OnModuleTreeItemPressed(name, item, this);
        }
    }
}