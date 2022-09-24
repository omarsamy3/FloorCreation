using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PracticePlugins.Revit_Helper
{
    public static class CurveHelper
    {
        //The inch value.
         const double Inch = 1 / 12;

        //The tolerance to check the equality.
        //const double Tolerence = Inch / 16;
        const double Tolerence = 0.005;


        //Swap two lists
        static void swap<T>(IList<T> List, int index1, int index2)
        {
            T temp = List[index1];
            List[index1] = List[index2];
            List[index2] = temp;
        }

        //Swap two curves and reverse it.
        static void SwapAndReverse(IList<Curve> curves, int index1, int index2)
        {
            Curve temp = curves[index1];
            curves[index1] = CreateReversedCurve(curves[index2]);
            curves[index2] = temp;
        }

        //Check if the selected element is supported to be curve.
        static bool IsSupported(Curve curve)
        {
            return curve is Line || curve is Arc;
        }

        //Get reversed curve 
        static Curve CreateReversedCurve(Curve original)
        {
            //Check if the curve is supported 
            if (!IsSupported(original))
            {
                throw new NotImplementedException("CreateReversedCurve for type" + original.GetType().Name);
            }
            
                //If the curve is line
                if(original is Line)
                {
                    return Line.CreateBound(original.GetEndPoint(1), original.GetEndPoint(0));
                }

                //If the curve is Arc
                else if(original is Arc)
                {
                    return Arc.Create(original.GetEndPoint(1), original.GetEndPoint(0), original.Evaluate(0.5, true));
                }
                //If there is not selected curves.
                else
                {
                    throw new Exception("CreateReversedCurve - Unreachable");
                }
            
        }
        static void SortCurvesWithOffsetsContiguous(IList<Curve> curves, IList<double> offsets)
        {
            //The number of curves.
            int n = curves.Count;

            //Walk throug the curves to match up the curves in correct ordering and direction.
            for (int i = 0; i < n - 1; ++i)
            {
                //The curve to check with.
                Curve curve = curves[i];

                //Get the end point of the check curve.
                XYZ endPoint = curve.GetEndPoint(1);

                //The point to use in the check loop
                XYZ p;

                //Flag to stop if the curve found
                bool found = false;

                //The check curve number.
                int j = i + 1;

                //Find the curve with start point equal to end point of the current curve.
                while(!found && j < n)
                {
                    //Get the start point of the next curve.
                    p = curves[j].GetEndPoint(0);
                    var re = p.DistanceTo(endPoint);
                    if (p.DistanceTo(endPoint) <= Tolerence) //Contiguous
                    {
                        if(i + 1 != j) //Ordering Issue >> swap
                        {
                            //Swap curves
                            swap(curves, i + 1, j);

                            //Swap offsets
                            swap(offsets, i + 1, j);
                        }

                        found = true; //The curve is found, continue and loop.
                    }

                    if (!found) //Not found, check the endpoint 
                    {
                        p = curves[j].GetEndPoint(1);

                        if (p.DistanceTo(endPoint) <= Tolerence) //Contiguous but different direction.
                        {
                            if(i + 1 == j) //contiguous but reversed
                            {
                                curves[i + 1] = CreateReversedCurve(curves[j]);
                            }
                            else //Direction and ordering issues.
                            {
                                //Swap and reverse the next curve
                                SwapAndReverse(curves, i + 1, j);

                                //Swap offsets
                                swap(offsets, i + 1, j);
                            }
                            
                            found = true; //Curve found and solve the problem
                        }
                    }
                    //Increate the counter >> go to the next curve.
                    j++;    
                }

                if (!found) // curve not found yet! there is an error!
                {
                    throw new Exception("SortCurvesContiguous:"
                      + " non-contiguous input curves");
                }
            }
        }
        public static (List<Curve> curves , List<double> offsets) GetContiguousCurvesWithOffsets(IList<Curve> OriginalCurves, IList<double> OriginalOffsets)
        {
            //Create a new list of curves to sort and deal with it.
            List<Curve> NewCurves = new List<Curve>(OriginalCurves);

            //Create a new list of offsets for the new curves list.
            List<double> NewOffsets = new List<double>(OriginalOffsets);

            //Sort the the curves to return them contiguous.
            SortCurvesWithOffsetsContiguous(NewCurves, NewOffsets);

            //Return the tuple of curves and their curves.
            return (NewCurves, NewOffsets);
        }
    }
}
