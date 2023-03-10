using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GodotCSharpToolkit.Editor
{
    public interface IAbstractJsonEditor
    {
        /// <summary>
        /// Check if the given name is unique
        /// </summary>
        bool IsNameUnique(string name, JsonDefWithName original);
    }
}