using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.DebugMenu
{
    /// <summary>
    /// Class taken from MDFramework
    /// https://github.com/DoubleDeez/MDFramework
    /// </summary>
    public partial class OnScreenDebugInterface : Control
    {
        private RichTextLabel DisplayLabel;

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;
            AnchorTop = 0;
            AnchorLeft = 0;
            AnchorRight = 1;
            AnchorBottom = 1;
            OffsetBottom = 0;
            OffsetLeft = 0;
            OffsetRight = 0;
            OffsetTop = 0;
            CreateControls();
            Visible = true;
        }

        public override void _Process(double delta)
        {
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            DisplayLabel.Clear();
            DisplayLabel.PushTable(2);

            foreach (string Category in OnScreenDebugManager.DebugInfoCategoryMap.Keys)
            {
                if (OnScreenDebugManager.HiddenCategories.Contains(Category))
                {
                    continue;
                }

                AddCateogryTitle(Category);

                Dictionary<string, OnScreenDebugInfo> DebugInfoList = OnScreenDebugManager.DebugInfoCategoryMap[Category];

                foreach (string key in DebugInfoList.Keys)
                {
                    try
                    {
                        string text = DebugInfoList[key].InfoFunction.Invoke();
                        AddTextCell(key, DebugInfoList[key].Color);
                        AddTextCell(text, DebugInfoList[key].Color);
                    }
                    catch (Exception ex)
                    {
                        // Something went wrong
                        Logger.Error("Somethign went wrong when updating labels", ex);
                    }
                }

                AddEmptyLine();
            }

            DisplayLabel.Pop();
        }

        private void AddTextCell(string Text, Color Color)
        {
            DisplayLabel.PushCell();
            DisplayLabel.PushColor(Color);
            DisplayLabel.AppendText(Text);
            DisplayLabel.Pop();
            DisplayLabel.Pop();
        }

        private void AddEmptyLine()
        {
            DisplayLabel.PushCell();
            DisplayLabel.Pop();
            DisplayLabel.PushCell();
            DisplayLabel.Pop();
        }

        private void AddCateogryTitle(string Category)
        {
            DisplayLabel.PushCell();
            DisplayLabel.PushColor(Colors.White);
            DisplayLabel.PushBold();
            DisplayLabel.PushUnderline();
            DisplayLabel.AppendText(Category);
            DisplayLabel.Pop();
            DisplayLabel.Pop();
            DisplayLabel.Pop();
            DisplayLabel.Pop();

            DisplayLabel.PushCell();
            DisplayLabel.Pop();
        }

        // Creates the UI control for the debug screen
        private void CreateControls()
        {
            DisplayLabel = new RichTextLabel();
            DisplayLabel.Name = nameof(DisplayLabel);
            DisplayLabel.Text = "";
            DisplayLabel.MouseFilter = MouseFilterEnum.Ignore;
            DisplayLabel.CustomMinimumSize = GetViewport().GetVisibleRect().Size;
            if (DebugMenu.GetFont() != null)
            {
                DisplayLabel.AddThemeFontOverride("normal_font", DebugMenu.GetFont());
            }
            AddChild(DisplayLabel);
        }
    }
}
