using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using PracticePlugins.Revit_Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracticePlugins
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateFloorFromWallsContiguous : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get the uiDocument to deal with UI.
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;

            //Get the document to deal with elements.
            Document document = uiDocument.Document;

            //Make an object from WallSelectionFilter Class to check only walls are selected
            WallSelectionFilter WallFilter = new WallSelectionFilter();

            //Get the elements selected from the user.
            IList<Reference> references = uiDocument.Selection.PickObjects
                (Autodesk.Revit.UI.Selection.ObjectType.Element,WallFilter);

            if (references != null && references.Count() >= 4)
            {
                //Make an array of selected walls curves.
                List<Curve> OriginalWallsCurves = new List<Curve>();

                //Initialize the list of original offsets to move wall curves.
                List<double> OriginalWallsCurveOffsets = new List<double>();

                //Initialize the level ids of the curves.
                List<ElementId> LevelIds = new List<ElementId>();


                //Loop to add the items to lists.
                foreach (var reference in references)
                {
                    //cast to wall element from the selected reference.
                    Wall wall = document.GetElement(reference) as Wall;

                    if (wall != null)
                    {
                        //Add the wall curve to the wallcurve list.
                        OriginalWallsCurves.Add((wall.Location as LocationCurve).Curve);

                        //Add the wall offset to the offset list.
                        OriginalWallsCurveOffsets.Add(wall.Width / 2);

                        //Add the wall level id to the level ids list.
                        LevelIds.Add(wall.LevelId);
                    }
                }

                //Use the wall if it is in the same level.
                if (LevelIds.All(levelId => levelId == LevelIds[0]))
                {
                    //Get the contiquouse curves after ordering.
                    var ContiguousCurvesWithOffsetsTuple = CurveHelper.GetContiguousCurvesWithOffsets(OriginalWallsCurves, OriginalWallsCurveOffsets);

                    //Make new list of curves to add wall curves
                    var ContiguousWallCurves = ContiguousCurvesWithOffsetsTuple.curves;

                    //Make new list of offsets
                    var ContiguousWallOffsets = ContiguousCurvesWithOffsetsTuple.offsets;

                    //Get the wall curve loop via offset to draw floor.
                    CurveLoop FloorCurves = CurveLoop.CreateViaOffset
                    (CurveLoop.Create(ContiguousWallCurves), ContiguousWallOffsets, new XYZ(0, 0, 1));

                    //Intialize the curve array
                    CurveArray WallCurveArray = new CurveArray();

                    //Loop to add the wall curves to the curve array
                    foreach (var curve in FloorCurves)
                    {
                        WallCurveArray.Append(curve);
                    }

                    //Get the level of drawing.
                    Level level = document.GetElement(LevelIds[0]) as Level;

                    //The floor type name 
                    string FloorTypeName = "Generic 300mm";

                    //Get the floor type
                    FloorType floorTpye = new FilteredElementCollector(document)
                        .OfClass(typeof(FloorType))
                        .First<Element>(e => e.Name.Equals(FloorTypeName)) as FloorType;

                    using (Transaction transaction = new Transaction(document, "Draw Floor From Walls"))
                    {
                        try
                        {
                            //Start Transaction
                            transaction.Start();

                            document.Create.NewFloor(WallCurveArray, floorTpye, level, false);

                            //End Transaction
                            transaction.Commit();
                        }
                        catch(Exception e)
                        {
                            //Assign the exception message to show if failed.
                            message = e.Message;
                            //Roll back all changes.
                            transaction.RollBack();
                            //Nothing done
                            return Result.Failed;
                        }
                    }
                }
                else return Result.Failed;
            }
            else return Result.Failed;


            return Result.Succeeded;
        }
    }
}
