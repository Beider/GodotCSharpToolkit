using Godot;
using System;
using System.Collections.Generic;

namespace CSharpDataEditorDll
{
    /// <summary>
    /// Defines this as a list of options based on a class that is inherited from
    /// Just inherit and implement the required methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public abstract class CSDOListRendererClassBase : CSDOList
    {
        private Dictionary<string, string> ListWithColor = new Dictionary<string, string>();
        public CSDOListRendererClassBase(bool sortList = true) : base(true, sortList)
        {

        }

        public override string[] GetList(CSDataObject dataObject)
        {
            RefreshList(dataObject);
            return (new List<string>(ListWithColor.Keys)).ToArray();
        }

        private void RefreshList(CSDataObject dataObject)
        {
            ListWithColor.Clear();
            try
            {
                FillList(dataObject);
            }
            catch (Exception ex)
            {
                System.Console.Error.Write(ex);
                ListWithColor.Add(ex.Message, nameof(Colors.Red));
            }
        }

        protected abstract Type GetBaseType();

        protected virtual string GetColor(string name)
        {
            return nameof(Colors.SkyBlue);
        }

        protected void FillList(CSDataObject dataObject)
        {
            foreach (Type type in dataObject.Factory.GetAssembly().GetTypes())
            {
                if (!type.IsAbstract && GetBaseType().IsAssignableFrom(type))
                {
                    ListWithColor.Add(type.Name, GetColor(type.Name));
                }
            }

            if (ListWithColor.Count == 0)
            {
                ListWithColor.Add("- Could not find any definitions -", nameof(Colors.Red));
                return;
            }
        }

        public override string GetColor(string value, CSDataObject dataObject)
        {
            if (ListWithColor.Count == 0)
            {
                RefreshList(dataObject);
            }
            if (ListWithColor.ContainsKey(value))
            {
                return ListWithColor[value];
            }
            return nameof(Colors.Red);
        }
    }
}