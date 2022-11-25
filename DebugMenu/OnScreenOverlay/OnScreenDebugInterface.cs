using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.DebugMenu
{
    /// <summary>
    /// Class taken from MDFramework
    /// https://github.com/DoubleDeez/MDFramework
    /// </summary>
    public class OnScreenDebugInterface : Control
    {
        private RichTextLabel DisplayLabel;

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;
            AnchorTop = 0;
            AnchorLeft = 0;
            AnchorRight = 1;
            AnchorBottom = 1;
            MarginBottom = 0;
            MarginLeft = 0;
            MarginRight = 0;
            MarginTop = 0;
            CreateControls();
            Visible = true;
        }

        public override void _Process(float delta)
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
                        GD.Print(ex.ToString());
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
            DisplayLabel.AppendBbcode(Text);
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
            DisplayLabel.AppendBbcode(Category);
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
            DisplayLabel.RectMinSize = GetViewport().GetVisibleRect().Size;
            if (DebugMenu.GetFont() != null)
            {
                DisplayLabel.AddFontOverride("normal_font", DebugMenu.GetFont());
            }
            AddChild(DisplayLabel);
        }
    }
}
