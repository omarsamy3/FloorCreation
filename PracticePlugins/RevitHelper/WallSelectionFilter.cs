using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracticePlugins.Revit_Helper
{
    public class WallSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return (BuiltInCategory)GetCategoryIdAsInteger(elem) == BuiltInCategory.OST_Walls; //Allow Selection only if walls.
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }

        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1; //return -1 if null;
        }
    }
}
