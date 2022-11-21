using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using InstallConduitsStrap.Common;


namespace InstallConduitsStrap.Tools
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class NewInstall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            try
            {
                var sheet = uidoc.ActiveView;
                FilteredElementCollector conduintsFilter = new FilteredElementCollector(doc, sheet.Id);
                IList<Element> strutElements = FindElement(doc, sheet.Id, typeof(FamilyInstance), "MM_StrutClearance");
                IList<Element> conduintsElements = conduintsFilter.OfCategory(BuiltInCategory.OST_Conduit)
                    .WhereElementIsNotElementType().ToElements();

                using (TransactionGroup transactionGroup = new TransactionGroup(doc, "Using plagin Install Strap"))
                {
                    transactionGroup.Start();

                    XYZ x, y;
                    double pointX, pointY, pointZ, degree;

                    #region Create

                    foreach (var strut in strutElements)
                    {
                        LocationPoint locationPoint = strut.Location as LocationPoint;

                        pointX = locationPoint.Point.X;
                        pointY = locationPoint.Point.Y;
                        pointZ = locationPoint.Point.Z;

                        foreach (var conduints in conduintsElements)
                        {
                            LocationCurve curve = conduints.Location as LocationCurve;

                            x = curve.Curve.GetEndPoint(0);
                            y = curve.Curve.GetEndPoint(1);

                            Line line = curve.Curve as Line;
                            if (line.Direction.X < -1)
                            {
                                degree = Math.Acos(line.Direction.X);
                            }
                            else
                            {
                                degree = Math.Asin(line.Direction.X) + 90 * Math.PI / 180;
                            }


                            #region Diameter definition

                            string _parameterSize =
                                conduints.LookupParameter("Diameter(Trade Size)").AsValueString();
                            string sizeFitting = "";
                            if (_parameterSize == "3\"")
                            {
                                sizeFitting = "1 1/2\"";
                            }
                            else if (_parameterSize == "4\"")
                            {
                                sizeFitting = "2\"";
                            }

                            #endregion

                            double pointXstate = 0;
                            double pointYstate = 0;
                            double pointZstate = 0;

                            bool positionXBool = pointX > x.X && pointX < y.X;
                            bool positionXBool_n = pointX < x.X && pointX > y.X;

                            bool positionYBool = pointY > x.Y && pointY < y.Y;
                            bool positionYBool_n = pointY < x.Y && pointY > y.Y;


                            bool positionZ = x.Z - pointZ < 0.19 && x.Z - pointZ > 0.14;


                            if (positionXBool || positionYBool || positionXBool_n || positionYBool_n)
                            {
                                if (Math.Abs(pointZ - x.Z) < 0.19 && Math.Abs(pointZ - x.Z) > 0.14 &&
                                    (Math.Abs(pointX - x.X) < 4 || Math.Abs(pointY - x.Y) < 4))
                                {
                                    if (Math.Abs(x.X - y.X) < 0.2)
                                    {
                                        pointXstate = x.X;
                                        pointYstate = pointY;
                                        pointZstate = x.Z;
                                    }
                                    else if (Math.Abs(x.Y - y.Y) < 0.2)
                                    {
                                        pointXstate = pointX;
                                        pointYstate = x.Y;
                                        pointZstate = x.Z;
                                    }

                                    //double rotationCellValue = degree + 90 * Math.PI / 180;

                                    double rotationCellValue = degree;


                                    string symbolName = "MM_Conduit_Strap";

                                    FilteredElementCollector allFamilySymbols = new FilteredElementCollector(doc)
                                        .OfClass(typeof(ElementType));

                                    var elementType = allFamilySymbols
                                            .OfCategory(BuiltInCategory.OST_ConduitFitting)
                                            .Cast<ElementType>()
                                            .FirstOrDefault<ElementType>(a => a.FamilyName == symbolName) as
                                        FamilySymbol;

                                    //Place a Family
                                    FamilyInstance newFamilyInstance;
                                    XYZ placementPoint;

                                    using (Transaction tl = new Transaction(doc, "Place an instance"))
                                    {
                                        tl.Start("EXEMPLE");
                                        FailureHandlingOptions options = tl.GetFailureHandlingOptions();
                                        MyPreProcessor preproccessor = new MyPreProcessor();
                                        options.SetClearAfterRollback(true);
                                        options.SetFailuresPreprocessor(preproccessor);
                                        tl.SetFailureHandlingOptions(options);

                                        if (null == elementType)
                                        {
                                            RevitMessage.Display("Something wrong. Family was not loaded!",
                                                WindowType.Error);
                                            return Result.Cancelled;
                                        }

                                        if (!elementType.IsActive)
                                        {
                                            elementType.Activate();
                                            doc.Regenerate();
                                        }


                                        placementPoint = new XYZ(pointXstate, pointYstate, pointZstate);

                                        StructuralType famStructuralType = StructuralType.NonStructural;

                                        newFamilyInstance = doc.Create.NewFamilyInstance(placementPoint,
                                            elementType, famStructuralType);
                                        newFamilyInstance.LookupParameter("Nominal Radius")
                                            .SetValueString(sizeFitting);
                                        if (!positionZ)
                                        {
                                            newFamilyInstance.LookupParameter("TOP").Set(0);
                                        }

                                        if (rotationCellValue != 0)
                                        {
                                            XYZ direction = new XYZ(0, 0, 1);
                                            Line axis = Line.CreateUnbound(placementPoint, direction);

                                            newFamilyInstance.Location.Rotate(axis, rotationCellValue);
                                            doc.Regenerate();
                                        }

                                        tl.Commit(options);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    transactionGroup.Assimilate();
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception a)
            {
                message = a.Message;
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }


        public static XYZ LinePlaneIntersection(Line line, Plane plane, out double lineParameter)
        {
            XYZ planePoint = plane.Origin;
            XYZ planeNormal = plane.Normal;
            XYZ linePoint = line.GetEndPoint(0);

            XYZ lineDirection = (line.GetEndPoint(1)
                                 - linePoint).Normalize();

            // Is the line parallel to the plane, i.e.,
            // perpendicular to the plane normal?

            if (IsZero(planeNormal.DotProduct(lineDirection)))
            {
                lineParameter = double.NaN;
                return null;
            }

            lineParameter = (planeNormal.DotProduct(planePoint)
                             - planeNormal.DotProduct(linePoint))
                            / planeNormal.DotProduct(lineDirection);

            return linePoint + lineParameter * lineDirection;
        }

        public const double _eps = 1.0e-9;

        public static bool IsZero(double a, double tolerance = _eps)
        {
            return tolerance > Math.Abs(a);
        }

        public static IList<Element> FindElement(Document doc, ElementId sheetId, Type targetType, string targetName)
        {
            // Get the elements of the given class 

            FilteredElementCollector collector
                = new FilteredElementCollector(doc, sheetId);

            collector.WherePasses(
                new ElementClassFilter(targetType));

            // Parse the collection for the 
            // given name using LINQ query.

            IEnumerable<Element> targetElems =
                from element in collector
                where element.Name.Equals(targetName)
                select element;

            IList<Element> elems = targetElems.ToList();

            if (elems.Count > 0)
            {
                // We should have only one with the given name.
                return elems;
            }

            // Cannot find it.

            return null;
        }
    }
}