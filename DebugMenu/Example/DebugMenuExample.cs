using Godot;
using System;
using System.Collections.Generic;

namespace GodotCSharpToolkit.DebugMenu
{
    /// <summary>
    /// To make example work load this as an autoload after the DebugMenu.
    /// <para>Also check out IDebugTool.cs for information on how to use debug tools</para>
    /// </summary>
    [DebugIncludeClass]
    public class DebugMenuExample : Node
    {
        private const string EXAMPLE_CATEGORY = "Examples";
        private const string EXAMPLE_CAT_SIMPLE = EXAMPLE_CATEGORY + " Simple";
        private const string EXAMPLE_CAT_TOGGLE = EXAMPLE_CATEGORY + " Toggle";
        private const string EXAMPLE_CAT_CALLBACK = EXAMPLE_CATEGORY + " Callbacks";
        private const string EXAMPLE_CAT_DIALOG = EXAMPLE_CATEGORY + " Dialog";

        #region DIALOG EXAMPLE

        public enum DialogExampleEnum
        {
            AllowTrue,
            AllowFalse,
            AllowBoth
        }

        /// <summary>
        /// When using dialogs the parameters set in the MenuEntry will be passed as the first arguments
        /// then the parameters of all values in the dialog will be passed after
        /// </summary>
        /// <param name="buttonName">This parameter is set in the menu entry for dialog 1, from dialog in dialog 2</param>
        /// <param name="textValue">The text value is taken from the dialog in both examples</param>
        [DebugCategoryColumn(EXAMPLE_CAT_DIALOG, 4)]
        [DebugMenuEntrySimple(EXAMPLE_CAT_DIALOG, "Open dialog (1)", nameof(Colors.Brown), false, 1, "Button01")]
        [DebugMenuDialogValidator(1, nameof(DialogValidatorExample))]
        [DebugMenuDialogField(1, "text_value", nameof(InitialStringValueExample), false)]
        [DebugMenuEntrySimple(EXAMPLE_CAT_DIALOG, "Open dialog (2)", nameof(Colors.Brown), false, 2)]
        [DebugMenuDialogField(2, "text_value1", null, false)]
        [DebugMenuDialogField(2, "text_value2", nameof(InitialStringValueExample), false)]
        public void OpenDialogExample(string buttonName, string textValue)
        {
            GD.Print($"DialogExample01: {buttonName}, {textValue}");
        }

        /// <summary>
        /// Dialog 01
        /// </summary>
        public string InitialStringValueExample(int dialogId, string fieldName)
        {
            return "Initial value";
        }

        /// <summary>
        /// Dialog 01
        /// </summary>
        public string DialogValidatorExample(int dialogId, Dictionary<string, object> values)
        {
            object value = values["text_value"];
            if (value == null || "".Equals(value.ToString()))
            {
                return "Empty is not allowed!";
            }

            if (value.ToString().Equals("Initial value"))
            {
                return "Please change the initial value";
            }

            return "";
        }

        [DebugMenuEntrySimple(EXAMPLE_CAT_DIALOG, "Open a different dialog", nameof(Colors.BurlyWood), false, 100)]
        [DebugMenuDialogValidator(100, nameof(DialogValidatorExample02))]
        [DebugMenuDialogField(100, "bool_value", nameof(InitialBoolValueExample), true)]
        [DebugMenuDialogField(100, "list_value", nameof(InitialStringValueListExample), nameof(ListValuesProviderExample))]
        public void OpenDialogExample2(bool boolValue, string listValue)
        {
            GD.Print($"DialogExample02: {boolValue}, {listValue}");
        }

        /// <summary>
        /// Dialog 02
        /// </summary>
        public bool InitialBoolValueExample(int dialogId, string fieldName)
        {
            return false;
        }

        /// <summary>
        /// Dialog 02
        /// </summary>
        public string InitialStringValueListExample(int dialogId, string fieldName)
        {
            return DialogExampleEnum.AllowBoth.ToString();
        }

        /// <summary>
        /// Dialog 02
        /// </summary>
        public List<String> ListValuesProviderExample(int dialogId, string fieldName)
        {
            var list = new List<String>();
            foreach (DialogExampleEnum value in Enum.GetValues(typeof(DialogExampleEnum)))
            {
                list.Add(value.ToString());
            }

            // You could sort here
            //list.Sort();

            return list;
        }

        /// <summary>
        /// Dialog 02
        /// </summary>
        public string DialogValidatorExample02(int dialogId, Dictionary<string, object> values)
        {
            bool boolValue = (bool)values["bool_value"];
            string listValue = (string)values["list_value"];
            DialogExampleEnum enumValue;
            if (!Enum.TryParse<DialogExampleEnum>(listValue, out enumValue))
            {
                return "Failed to parse enum value";
            }

            if (enumValue == DialogExampleEnum.AllowTrue && !boolValue)
            {
                return "False is currently not allowed!";
            }
            else if (enumValue == DialogExampleEnum.AllowFalse && boolValue)
            {
                return "True is currently not allowed!";
            }

            return "";
        }

        #endregion

        #region CALLBACK EXAMPLE

        enum CycleEnum
        {
            Value1,
            Value2,
            Value3
        }

        [OnScreenDebug(EXAMPLE_CATEGORY, "Cycle value", nameof(Colors.Yellow))]
        private CycleEnum CycleValue = CycleEnum.Value1;

        [DebugCategoryColumn(EXAMPLE_CAT_CALLBACK, 4)]
        [DebugMenuEntryCallback(EXAMPLE_CAT_CALLBACK, nameof(GetCycleText), nameof(GetCycleColor), false, 0)]
        [DebugMenuEntrySimple(EXAMPLE_CAT_CALLBACK, "Change cycle value", nameof(Colors.Yellow), false, 0)]
        public void ChangeCycleValue()
        {
            int val = (int)CycleValue;
            val++;
            if (val >= Enum.GetValues(typeof(CycleEnum)).Length)
            {
                val = 0;
            }
            CycleValue = (CycleEnum)val;
        }

        public string GetCycleText()
        {
            return $"Current value: {CycleValue.ToString()}";
        }

        public Color GetCycleColor()
        {
            switch (CycleValue)
            {
                case CycleEnum.Value1:
                    return Colors.Red;
                case CycleEnum.Value2:
                    return Colors.Green;
                case CycleEnum.Value3:
                    return Colors.Blue;
                default:
                    return Colors.Black;
            }
        }

        #endregion

        #region SIMPLE BUTTON EXAMPLE
        [DebugCategoryColumn(EXAMPLE_CAT_SIMPLE, 4)]
        [DebugMenuEntrySimple(EXAMPLE_CAT_SIMPLE, "Simple Button One (Close)", nameof(Colors.PaleTurquoise), true, 0, "First Value", "Second Value")]
        [DebugMenuEntrySimple(EXAMPLE_CAT_SIMPLE, "Simple Button Two (No close)", nameof(Colors.PaleTurquoise), false, 0, "Value1", "Value2")]
        public void SimpleButtonCall(string value1, string value2)
        {
            GD.Print($"Called: Value1({value1}), Value2({value2})");
        }

        #endregion

        #region TOGGLE ANNOTATION

        private bool ToggleValue = true;

        [DebugCategoryColumn(EXAMPLE_CAT_TOGGLE, 4)]
        [DebugMenuEntryToggle(EXAMPLE_CAT_TOGGLE, "Toggle Button Example", nameof(IsToggleActive))]
        public bool Toggle(bool value)
        {
            ToggleValue = value;
            return ToggleValue;
        }

        public bool IsToggleActive()
        {
            return ToggleValue;
        }

        #endregion
    }
}