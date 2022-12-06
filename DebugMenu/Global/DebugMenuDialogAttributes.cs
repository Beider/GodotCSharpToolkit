using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using GodotCSharpToolkit.Logging;

namespace GodotCSharpToolkit.DebugMenu
{
    /// <summary>
    /// NOTE:
    /// When using dialogs the parameters you specify in the button will be passed along with the parameters from the dialog
    /// The button parameters comes first, then the dialog parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DebugMenuDialogField : DebugMenuEntry
    {
        public enum FieldTypes
        {
            Text,
            Checkbox,
            List
        }

        public readonly FieldTypes FieldType;
        public readonly string FieldName;
        public readonly string InitialValueCallback;
        public readonly string ListValuesCallback;

        /// <summary>
        /// Add a dialog field, will not do anything unless a button is marked with the same dialogId
        /// This can add either a checkbox or string field
        /// </summary>
        /// <param name="dialogId">The dialog this belongs to, should match some button on the same method. (Unique per class!)</param>
        /// <param name="fieldName">The name of this field, used for validation</param>
        /// <param name="initialValueCallback">Callback to get initial value, 
        /// if null or empty no initial value will be set (DebugButtonMenu.DebugDialogInitialValueStringCallback)</param>
        /// <param name="isCheckbox">If this is a checkbox set to true, if string false. The initial value callback must be of the correct type</param>
        public DebugMenuDialogField(int dialogId, string fieldName, string initialValueCallback, bool isCheckbox) :
            base("", "", "", false, dialogId)
        {
            this.FieldType = isCheckbox ? FieldTypes.Checkbox : FieldTypes.Text;
            this.FieldName = fieldName;
            this.InitialValueCallback = initialValueCallback;
        }

        /// <summary>
        /// Add a dialog field, will not do anything unless a button is marked with the same dialogId
        /// This will add a list field
        /// </summary>
        /// <param name="dialogId">The dialog this belongs to, should match some button on the same method. (Unique per class!)</param>
        /// <param name="fieldName">The name of this field, used for validation</param>
        /// <param name="initialValueCallback">Callback to get initial value, 
        /// if null or empty no initial value will be set (DebugButtonMenu.DebugDialogInitialValueStringCallback)</param>
        /// <param name="listValuesCallback">Callback used to get list values (DebugButtonMenu.DebugDialogListValuesCallback)</param>
        public DebugMenuDialogField(int dialogId, string fieldName, string initialValueCallback, string listValuesCallback) :
            base("", "", "", false, dialogId)
        {
            this.FieldType = FieldTypes.List;
            this.FieldName = fieldName;
            this.InitialValueCallback = initialValueCallback;
            this.ListValuesCallback = listValuesCallback;
        }

        public string GetInitialTextValue(Node node, int dialogId, string fieldName)
        {
            return GetValue<string>(node, dialogId, fieldName, InitialValueCallback);
        }

        public bool GetInitialBoolValue(Node node, int dialogId, string fieldName)
        {
            return GetValue<bool>(node, dialogId, fieldName, InitialValueCallback);
        }

        public List<String> GetListValues(Node node, int dialogId, string fieldName)
        {
            return GetValue<List<String>>(node, dialogId, fieldName, ListValuesCallback);
        }

        private T GetValue<T>(Node node, int dialogId, string fieldName, string callbackMethod)
        {
            T value = default(T);

            if (callbackMethod == null || "".Equals(callbackMethod))
            {
                return value;
            }

            try
            {
                value = (T)node.GetType().GetMethod(callbackMethod).Invoke(node, new object[] { dialogId, fieldName });
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get initial value", ex);
            }
            return value;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DebugMenuDialogValidator : DebugMenuEntry
    {
        public readonly string ValidationCallback;

        /// <summary>
        /// Adds a dialog validation method, must use the correct signature. See DebugButtonMenu.DebugDialogValidatorCallback
        /// </summary>
        /// <param name="dialogId">The dialog this belongs to, should match some button on the same method. (Unique per class!)</param>
        /// <param name="validationCallback">The name of this field, used for validation</param>
        /// <param name="initialValueCallback">Callback to get initial value, if null or empty no initial value will be set</param>
        /// <param name="isCheckbox">If this is a checkbox set to true, if string false. The initial value callback must be of the correct type</param>
        public DebugMenuDialogValidator(int dialogId, string validationCallback) :
            base("", "", "", false, dialogId)
        {
            this.ValidationCallback = validationCallback;
        }
    }
}