using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.DebugMenu
{
    public class DebugMenuDialog : Control
    {
        public static DebugMenuDialog Instance;

        private Label Title;
        private GridContainer ControlGrid;
        private Button ButtonOk;
        private Panel ErrorPanel;
        private Label ErrorLabel;

        private int DialogId = 0;
        private Node Node;
        private List<DebugMenuEntry> ControlList;
        private string ValidationCallback = "";

        private Dictionary<string, Func<object>> ActiveControls = new Dictionary<string, Func<object>>();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Instance = this;
            Visible = false;

            Title = (Label)FindNode("Title");
            ControlGrid = (GridContainer)FindNode("ControlGrid");

            ErrorPanel = (Panel)FindNode("ErrorPanel");
            ErrorLabel = (Label)FindNode("ErrorMessage");
            ButtonOk = (Button)FindNode("BtnOk");
        }

        public static void ShowDialog(string title, Color titleColor, Node node,
                                      int dialogId, List<DebugMenuEntry> controlList)
        {
            Instance.Visible = true;
            Instance.Title.Text = title;
            Instance.Title.Modulate = titleColor;

            Instance.Node = node;
            Instance.DialogId = dialogId;
            Instance.ControlList = controlList;
            Instance.ValidationCallback = "";
            Instance.ActiveControls.Clear();

            Instance.BuildControlList();
        }

        private void BuildControlList()
        {
            ControlGrid.ClearNodeChildren();
            foreach (DebugMenuEntry entry in ControlList)
            {
                if (entry is DebugMenuDialogField field)
                {
                    AddControl(field);
                }
                else if (entry is DebugMenuDialogValidator validator)
                {
                    ValidationCallback = validator.ValidationCallback;
                }
            }

            Validate();
        }

        private void AddControl(DebugMenuDialogField entry)
        {
            AddTitle(entry.FieldName);
            switch (entry.FieldType)
            {
                case DebugMenuDialogField.FieldTypes.Text:
                    AddTextField(entry);
                    break;
                case DebugMenuDialogField.FieldTypes.Checkbox:
                    AddCheckboxField(entry);
                    break;
                case DebugMenuDialogField.FieldTypes.List:
                    AddListField(entry);
                    break;
            }
        }

        private void AddTextField(DebugMenuDialogField entry)
        {
            LineEdit edit = new LineEdit();
            edit.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            edit.Text = entry.GetInitialTextValue(Node, DialogId, entry.FieldName);
            edit.Connect("text_changed", this, nameof(TextChanged));
            ActiveControls.Add(entry.FieldName, () => edit.Text);
            ControlGrid.AddChild(edit);
        }

        private void TextChanged(string text)
        {
            Validate();
        }

        private void AddCheckboxField(DebugMenuDialogField entry)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            checkBox.Pressed = entry.GetInitialBoolValue(Node, DialogId, entry.FieldName);
            checkBox.Text = checkBox.Pressed.ToString();
            var array = new Godot.Collections.Array();
            array.Add(checkBox);
            checkBox.Connect("pressed", this, nameof(CheckboxChanged), array);
            ActiveControls.Add(entry.FieldName, () => checkBox.Pressed);
            ControlGrid.AddChild(checkBox);
        }

        private void CheckboxChanged(CheckBox checkBox)
        {
            checkBox.Text = checkBox.Pressed.ToString();
            Validate();
        }

        private void AddListField(DebugMenuDialogField entry)
        {
            MenuButton menu = new MenuButton();
            menu.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            menu.Text = entry.GetInitialTextValue(Node, DialogId, entry.FieldName);
            menu.Align = Button.TextAlign.Left;
            ActiveControls.Add(entry.FieldName, () => menu.Text);
            ControlGrid.AddChild(menu);

            // Create menu
            var popup = menu.GetPopup();
            foreach (string value in entry.GetListValues(Node, DialogId, entry.FieldName))
            {
                popup.AddItem(value);
            }
            var array = new Godot.Collections.Array();
            array.Add(menu);
            popup.Connect("id_pressed", this, nameof(PopupPressed), array);
        }

        private void PopupPressed(int id, MenuButton button)
        {
            button.Text = button.GetPopup().GetItemText(id);
            Validate();
        }

        private void AddTitle(string title)
        {
            // Add title
            Label lbl = new Label();
            lbl.Text = title;
            ControlGrid.AddChild(lbl);
        }

        private void ShowError(string message)
        {
            if (message == null || "".Equals(message))
            {
                ErrorPanel.Visible = false;
            }
            else
            {
                ErrorLabel.Text = message;
                ErrorPanel.Visible = true;
            }

            ButtonOk.Disabled = ErrorPanel.Visible;
        }

        /// <summary>
        /// Will validate the content and show error message if needed
        /// </summary>
        /// <returns>True if everything is ok, false if there is an error</returns>
        private bool Validate()
        {
            string message = "";

            if (ValidationCallback == null || "".Equals(ValidationCallback))
            {
                ShowError(message);
                return true;
            }

            Dictionary<string, object> values = new Dictionary<string, object>();
            foreach (string key in ActiveControls.Keys)
            {
                values.Add(key, ActiveControls[key].Invoke());
            }

            try
            {
                message = (string)Node.GetType().GetMethod(ValidationCallback).Invoke(Node, new object[] { DialogId, values });
            }
            catch (Exception ex)
            {
                message = $"Error during validation: '{ex.Message}'";
                Logger.Error("Error during validation", ex);
            }

            ShowError(message);
            return message == null || "".Equals(message);
        }

        private void OnOkPressed()
        {
            // Do validation
            if (!Validate())
            {
                return;
            }

            // Build list of values
            List<object> values = new List<object>();
            foreach (string key in ActiveControls.Keys)
            {
                values.Add(ActiveControls[key].Invoke());
            }
            if (DebugButtonMenu.DialogOkPressed(Node, DialogId, values.ToArray()))
            {
                Visible = false;
            }

        }

        private void OnCancelPressed()
        {
            Visible = false;
        }

    }
}