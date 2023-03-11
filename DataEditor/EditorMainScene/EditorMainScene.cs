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
        private Control EditorArea;
        public EditorPrefsExtended Preferences { get; private set; } = new EditorPrefsExtended();
        public EditorTreeView Tree { get; private set; }
        public EditorToolbar Toolbar { get; private set; }
        public PopupMenu PopupMenu { get; private set; }
        public Dictionary<string, Action> PopupMenuDelegates = new Dictionary<string, Action>();

        private IDataEditorContent ActiveEditor = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Tree = FindNode("EditorTreeView") as EditorTreeView;
            EditorArea = FindNode("EditorArea") as Control;
            Toolbar = FindNode("Toolbar") as EditorToolbar;
            PopupMenu = FindNode("PopupMenu") as PopupMenu;
            PopupMenu.Connect("id_pressed", this, nameof(OnPopupMenuPressed));

            Tree.Init(this);
            Toolbar.Init(this);

            // TODO: Remove this
            //GetTree().SetScreenStretch(SceneTree.StretchMode.Mode2d, SceneTree.StretchAspect.Expand, new Vector2(1440f, 900f));
            //OS.WindowSize = new Vector2(1440f, 900f);
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
            if (askForSave && Tree.HasUnsavedChanges())
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
            if (Tree.HasUnsavedChanges())
            {
                ShowConfirmDialog("You have unsaved changes, would you like to save before you exit?", shouldSave => { Close(shouldSave); });
            }
            else
            {
                Close(false);
            }
        }

        public void Close(bool save)
        {
            if (save) { Tree.Save(); }
            GetParent().RemoveChild(this);
        }

        public void Save()
        {
            Tree.Save();
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
            Tree.RefreshTree(true, uniqueId);
            Toolbar.Init(this);

        }
    }
}