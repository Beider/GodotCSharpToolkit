using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{
    public class SearchWindow : Panel
    {
        private LineEdit SearchInput;
        private Button SearchButton;
        private Button ClearButton;
        private Button ChkExact;
        private GridContainer GridButtons;

        public EditorMainScene MainScene;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            SearchInput = FindNode("SearchInput") as LineEdit;
            SearchButton = FindNode("SearchBtn") as Button;
            ClearButton = FindNode("ClearBtn") as Button;
            ChkExact = FindNode("ChkExact") as Button;
            GridButtons = FindNode("GridButtons") as GridContainer;
            ClearButton.Visible = false;
            GridButtons.Columns = 2;

            SearchButton.Connect("pressed", this, nameof(OnSearchPressed));
            ClearButton.Connect("pressed", this, nameof(OnClearPressed));
            SearchInput.Connect("text_entered", this, nameof(OnEnterPressed));
        }

        private void OnEnterPressed(string query)
        {
            bool exact = ChkExact.Pressed;
            Search(query, exact);
        }

        private void OnClearPressed()
        {
            ClearButton.Visible = false;
            GridButtons.Columns = 2;
            MainScene.Tree.ClearFilter();
            SearchInput.Text = "";
            MainScene.Tree.RefreshTree(false);
        }

        private void OnSearchPressed()
        {
            var query = SearchInput.Text;
            bool exact = ChkExact.Pressed;
            Search(query, exact);
        }

        public void Search(string query, bool exactMatch = false)
        {
            var result = new List<string>();
            ClearButton.Visible = true;
            GridButtons.Columns = 3;

            var results = MainScene.Search(query, exactMatch);

            foreach (var jDef in results)
            {
                result.Add(jDef.GetUniqueId());
            }

            MainScene.Tree.SetFilter(result);
            MainScene.Tree.RefreshTree(false);
        }
    }
}