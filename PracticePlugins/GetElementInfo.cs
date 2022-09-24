using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PracticePlugins
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class GetElementInfo : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get the DocumentUI to select element.
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;

            //Get the document to deal with elements
            Document document = uiDocument.Document;


            try
            {
                //Select item to get ist info.
                Reference selectedElement = uiDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (selectedElement != null)
                {
                    //Get the selected element by its reference.
                    Element element = document.GetElement(selectedElement.ElementId);

                    //Get the type of selected element.
                    ElementType elementType = document.GetElement(element.GetTypeId()) as ElementType;

                    //Show the element details in a task dialog.
                    TaskDialog.Show("Element Info",
                        $"Category Name: {elementType.Category.Name}\n" +
                        $"Element Id: {element.Id}\n" +
                        $"Family Name: {elementType.FamilyName}\n" +
                        $"Element Type: {elementType.Name}\n" +
                        $"Element Name: {element.Name}");

                    //Done!
                    return Result.Succeeded;
                }
                else
                {
                    //Not done!
                    return Result.Failed;
                }
            }
            catch(Exception e) //Catch the exception if exists.
            {
                //Assing the exception message to the failed message.
                message = e.Message;

                //Not done
                return Result.Failed;
            }
            
           

           
        }
    }
}
