using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.Editor
{
    public partial class SearchWindow : Panel
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
            SearchInput = FindChild("SearchInput") as LineEdit;
            SearchButton = FindChild("SearchBtn") as Button;
            ClearButton = FindChild("ClearBtn") as Button;
            ChkExact = FindChild("ChkExact") as Button;
            GridButtons = FindChild("GridButtons") as GridContainer;
            ClearButton.Visible = false;
            GridButtons.Columns = 2;

            SearchButton.Connect("pressed", new Callable(this, nameof(OnSearchPressed)));
            ClearButton.Connect("pressed", new Callable(this, nameof(OnClearPressed)));
            SearchInput.Connect("text_submitted", new Callable(this, nameof(OnEnterPressed)));
        }

        private void OnEnterPressed(string query)
        {
            bool exact = ChkExact.ButtonPressed;
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
            bool exact = ChkExact.ButtonPressed;
            Search(query, exact);
        }

        public void Search(string query, bool exactMatch = false)
        {
            SearchInput.Text = query;
            ChkExact.ButtonPressed = exactMatch;
            ClearButton.Visible = true;
            GridButtons.Columns = 3;
            MainScene.Tree.SetFilter(query, exactMatch);
        }
    }
}