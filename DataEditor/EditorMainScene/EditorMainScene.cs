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
        public event Action<int, string, string, Color> OnEditorOpened = delegate { };
        public event Func<bool> HasUnsavedChanges = null;

        /// <summary>
        /// Request the opening of the editor
        /// First is unique id, second is type
        /// </summary>
        public event Action<string, int> OnOpenEditorRequest = delegate { };

        /// <summary>
        /// Called to open dialogs
        /// </summary>
        public event Action<IDataEditor> OnOpenBrowseDialog = delegate { };
        public Func<string, bool, List<JsonDefWithName>> OnSearch = null;

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

        public EditorRecentTracker EditorRecentTracker;

        public SearchWindow SearchWindow { get; private set; }

        private Control CurrentDialog = null;

        private DocumentPanel DocumentPanel;
        private Control MainArea;

        private IDataEditorContent ActiveEditor = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Tree = FindChild("EditorTreeView") as EditorTreeView;
            EditorArea = FindChild("EditorArea") as Control;
            Toolbar = FindChild("Toolbar") as EditorToolbar;

            Toolbar.OnToggleHelpVisible += ToggleHelpVisible;

            SearchWindow = FindChild("SearchWindow") as SearchWindow;
            EditorTreeSplit = FindChild("EditorTreeSplit") as HSplitContainer;

            EditorRecentTracker = FindChild("EditorRecentTracker") as EditorRecentTracker;
            EditorRecentTracker.SetEditor(this);
            EditorRecentTracker.OnOpenEditorRequest += OnOpenEditorRequest;

            DocumentPanel = FindChild("DocumentPanel") as DocumentPanel;
            DocumentPanel.HideHelp();

            MainArea = FindChild("MainArea") as Control;

            Tree.Visible = true;
            SearchWindow.Visible = true;
            SearchWindow.MainScene = this;

            Tree.Init(this);
            Toolbar.Init(this);

            EditorTreeSplit.SplitOffset = Preferences.SettingEditorSplitOffset;
            EditorTreeSplit.Connect("dragged", new Callable(this, nameof(SplitDragged)));

            // Start preferences save timer
            SaveTimer = new Timer();
            SaveTimer.OneShot = false;
            SaveTimer.WaitTime = PREF_SAVE_INTERVAL;
            SaveTimer.Connect("timeout", new Callable(this, nameof(SavePreferences)));
            AddChild(SaveTimer);
            SaveTimer.Start();
        }

        private void ToggleHelpVisible()
        {
            DocumentPanel.ToggleVisible();
            MainArea.Visible = !DocumentPanel.Visible;
        }

        public void ShowHelpForItem(string item)
        {
            ToggleHelpVisible();
            DocumentPanel.Search(item);
        }

        public override void _ExitTree()
        {
            SavePreferences();
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
                // Do not allow shortcuts during dialogs
                if (CurrentDialog != null && IsInstanceValid(CurrentDialog) && CurrentDialog.Visible)
                {
                    return;
                }
                if (keyEvent.IsCommandOrControlPressed() && keyEvent.Keycode == Key.S)
                {
                    Save();
                }
                else if (keyEvent.IsCommandOrControlPressed() && keyEvent.Keycode == Key.N)
                {
                    Toolbar.OnNewModPressed();
                }
                else if (keyEvent.IsCommandOrControlPressed() && keyEvent.Keycode == Key.B)
                {
                    Toolbar.OnBrowsePressed();
                }
                else if (keyEvent.IsCommandOrControlPressed() && keyEvent.Keycode == Key.R)
                {
                    Toolbar.OnRefreshPressed();
                }
                else if (keyEvent.Keycode == Key.F1)
                {
                    ToggleHelpVisible();
                }
                else if (keyEvent.Keycode == Key.F4)
                {
                    Toolbar.OnSortPressed();
                }
                else if (keyEvent.Keycode == Key.F5)
                {
                    Toolbar.OnLocalOnlyPressed();
                }
            }
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
            if (DocumentPanel.Visible)
            {
                ToggleHelpVisible();
                return;
            }
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
            Tree.Save();
            OnDataSaved();
            _Refresh();
        }

        private void _Refresh()
        {
            string uniqueId = "";
            if (ActiveEditor != null && IsInstanceValid(((Control)ActiveEditor)))
            {
                uniqueId = ActiveEditor.GetUniqueId();
                ActiveEditor.QueueFree();
            }
            OnDataRefreshed();
            Tree.RefreshTree(true);
            Toolbar.Init(this);
        }

        public List<JsonDefWithName> Search(string query, bool exactMatch)
        {
            if (OnSearch == null) { return new List<JsonDefWithName>(); }
            return OnSearch(query, exactMatch);
        }

        public void NotifyOpenBrowseDialog()
        {
            OnOpenBrowseDialog(this);
        }

        public void OpenFolderManager()
        {
            if (_HasUnsavedChanges())
            {
                ShowConfirmDialog("You have unsaved changes, would you like to save first?", shouldSave =>
                {
                    if (shouldSave)
                    {
                        Save();
                        _OpenFolderManager();
                    }
                    else
                    {
                        _Refresh();
                        _OpenFolderManager();
                    }
                });
            }
            else
            {
                _OpenFolderManager();
            }
        }

        private void _OpenFolderManager()
        {
            ShowFolderManagerDialog();
        }

        public void NotifyOnModuleTreeItemPressed(string name, DelegateEditorTreeItem item)
        {
            OnModuleTreeItemPressed(name, item, this);
        }
    }
}