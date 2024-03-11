using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using GodotCSharpToolkit.Extensions;
using GodotCSharpToolkit.Misc;
using ScriptSystem.Data;

namespace GodotCSharpToolkit.Editor
{
    public partial class IconSelectionDialog : ColorRect
    {
        private const string PREF_SIZE = "icon_select_size_";
        private const string PREF_POS = "icon_select_pos_";
        private const string ICON_SCENE = "res://GodotCSharpToolkit/DataEditor/Dialogs/IconSelectionDialog/IS_IconButton.tscn";

        private GridContainer Grid;
        AdvancedDialogWindowInput WindowDialog;
        private IDataEditor Editor;
        private Button BtnOk;
        private Button BtnCancel;
        private Button BtnPrev;
        private Button BtnNext;
        private Label LblCount;
        private LineEdit TxtSearch;
        private IEditorDataParent Data;
        private IIconProvider Provider;
        private PackedScene IconScene;
        private TextureRect ImgPreview;

        private int Page = 1;

        private string SelectedIcon = "";
        private Button OldButton = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Grid = FindChild("IconGrid") as GridContainer;


            WindowDialog = FindChild("Dialog") as AdvancedDialogWindowInput;
            WindowDialog.OnCancelPressed += OnCancelPressed;
            WindowDialog.OnOkPressed += OnOkPressed;
            WindowDialog.Connect("close_requested", new Callable(this, nameof(OnCancelPressed)));
            WindowDialog.Connect("size_changed", new Callable(this, nameof(OnSizeChanged)));

            // Buttons
            BtnOk = FindChild("BtnOk") as Button;
            BtnCancel = FindChild("BtnCancel") as Button;
            BtnOk.Connect("pressed", new Callable(this, nameof(OnOkPressed)));
            BtnCancel.Connect("pressed", new Callable(this, nameof(OnCancelPressed)));

            BtnNext = FindChild("BtnNext") as Button;
            BtnPrev = FindChild("BtnPrev") as Button;
            BtnNext.Connect("pressed", new Callable(this, nameof(NextPage)));
            BtnPrev.Connect("pressed", new Callable(this, nameof(PrevPage)));
            LblCount = FindChild("LblCount") as Label;

            TxtSearch = FindChild("TxtSearch") as LineEdit;
            TxtSearch.Connect("text_changed", new Callable(this, nameof(SearchTextChanged)));

            ImgPreview = FindChild("ImgPreview") as TextureRect;

            IconScene = ResourceLoader.Load(ICON_SCENE) as PackedScene;

            WindowDialog.Popup();
            LoadPrefs();
            Provider.Search(null);
            Page = 1;
            Load();
            UpdatePreview();
        }

        private void SearchTextChanged(string text)
        {
            Provider.Search(text);
            Page = 1;
            Load();
        }

        private void OnSizeChanged()
        {
            var cols = (WindowDialog.Size.X - 40) / 72;
            Grid.Columns = cols;
        }

        private void NextPage()
        {
            if (Page < Provider.GetPageCount())
            {
                Page++;
                Load();
            }
        }
        private void PrevPage()
        {
            if (Page > 1)
            {
                Page--;
                Load();
            }
        }

        private void ButtonPressed(string key, Button btn)
        {
            if (OldButton != null && OldButton != btn)
            {
                OldButton.ButtonPressed = false;
            }
            if (OldButton == btn)
            {
                SelectedIcon = "";
                OldButton = null;
                UpdatePreview();
                return;
            }
            OldButton = btn;
            SelectedIcon = key;
            UpdatePreview();
        }

        private void Load()
        {
            OldButton = null;
            Grid.ClearChildren();
            var page = Provider.GetPage(Page);
            foreach (var key in page.Keys)
            {
                var btn = IconScene.Instantiate() as Button;
                btn.Icon = page[key];
                if (SelectedIcon == key)
                {
                    btn.ButtonPressed = true;
                    OldButton = btn;
                }
                btn.Connect("pressed", Callable.From(() => ButtonPressed(key, btn)));
                Grid.AddChild(btn);
            }

            LblCount.Text = $"({Page} / {Provider.GetPageCount()})";
        }

        public void Init(IDataEditor editor, IEditorDataParent editorDataParent, IIconProvider provider)
        {
            Data = editorDataParent;
            Editor = editor;
            Provider = provider;
            SelectedIcon = Data.EditorData.Icon;
        }

        private void UpdatePreview()
        {
            if (!SelectedIcon.IsNullOrEmpty())
            {
                ImgPreview.Texture = Provider.GetIcon(SelectedIcon);
            }
            else
            {
                ImgPreview.Texture = null;
            }
        }

        private void OnCancelPressed()
        {
            SavePrefs();
            QueueFree();
        }

        private void OnOkPressed()
        {
            if (BtnOk.Disabled) { return; }
            if (Data != null && Data.EditorData.Icon != SelectedIcon)
            {
                Data.EditorData.Icon = SelectedIcon;
                ((JsonDefWithName)Data).IsModified = true;
            }
            SavePrefs();
            QueueFree();
            Editor.RefreshActiveEditor();
        }

        private void SavePrefs()
        {
            if (Editor != null)
            {
                Editor.Preferences.SetValue(PREF_SIZE, GD.VarToStr(WindowDialog.Size));
                Editor.Preferences.SetValue(PREF_POS, GD.VarToStr(WindowDialog.Position));
            }
        }

        private void LoadPrefs()
        {
            if (Editor != null)
            {
                var size = Editor.Preferences.GetValue(PREF_SIZE, GD.VarToStr(WindowDialog.Size));
                var pos = Editor.Preferences.GetValue(PREF_POS, GD.VarToStr(WindowDialog.Position));

                WindowDialog.Size = ((Vector2)GD.StrToVar(size)).ToVector2I();
                WindowDialog.Position = ((Vector2)GD.StrToVar(pos)).ToVector2I();

                if (WindowDialog.Position.Y <= 10)
                {
                    WindowDialog.Position = new Vector2I(WindowDialog.Position.X, 10);
                }
            }
        }
    }
}