using Godot;
using System;
using System.Collections.Generic;
using GodotCSharpToolkit.Misc;
using GodotCSharpToolkit.Extensions;

namespace CSharpDataEditorDll
{
    /// <summary>
    /// Defines this as a list of options based on a json file
    /// Just inherit and implement the required methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public abstract class CSDOListRendererJsonBase : CSDOList
    {
        private Dictionary<string, string> ListWithColor = new Dictionary<string, string>();
        public CSDOListRendererJsonBase(bool sortList = true) : base(true, sortList)
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

        protected abstract void FillList(CSDataObject dataObject);

        protected abstract string GetColor(string name);

        protected void FillListInternal<T>(List<T> defs, string scriptTypeName, bool addEmpty = false) where T : IJsonDefWithName
        {
            if (defs.Count == 0)
            {
                ListWithColor.Add("- Could not find any definitions -", nameof(Colors.Red));
                return;
            }

            if (addEmpty)
            {
                ListWithColor.Add(" ", GetColor(" "));
            }
            foreach (T def in defs)
            {
                ListWithColor.Add(def.GetName(), GetColor(def.GetName()));
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

        public static List<U> LoadFromFile<T, U>(string path) where T : IJsonFile<U> where U : IJsonDefWithName
        {
            List<U> returnList = new List<U>();
            try
            {
                List<T> files = Utils.LoadAllJsonFilesInFolder<T>(path, false);
                foreach (T def in files)
                {
                    returnList.AddRange(def.GetValues());
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.Write(ex);
            }
            return returnList;
        }
    }
}