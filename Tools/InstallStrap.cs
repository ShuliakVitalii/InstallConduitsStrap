﻿using System;
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
    public class InstallStrap : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;


            try
            {
                /*var sheet = new FilteredElementCollector(doc).OfClass(typeof(View)).Where(a => a.Name == "Test").First();
                uidoc.ActiveView = sheet as View;*/

                var sheet = uidoc.ActiveView;


                FilteredElementCollector conduintsFilter = new FilteredElementCollector(doc, sheet.Id);


                IList<Element> efCs = FindElement(doc, sheet.Id, typeof(FamilyInstance), "MM_StrutClearance");

                using (TransactionGroup transactionGroup = new TransactionGroup(doc, "Using plagin Install Strap"))
                {
                    transactionGroup.Start();

                    // #region Delete
                    //
                    // IList<Element> fittings = FindElement(doc, sheet.Id, typeof(FamilyInstance), "Standard");
                    //
                    // IList<ElementId> deletEl = new List<ElementId>();
                    // if (fittings != null)
                    // {
                    //     foreach (Element e in fittings)
                    //     {
                    //         deletEl.Add(e.Id);
                    //     }
                    // }
                    //
                    // using (Transaction t = new Transaction(doc, "Delete element"))
                    // {
                    //     t.Start();
                    //
                    //     foreach (ElementId id in deletEl)
                    //     {
                    //         doc.Delete(id);
                    //     }
                    //
                    //     t.Commit();
                    // }
                    //
                    // #endregion


                    IList<Element> conduintsElements = conduintsFilter
                        .OfCategory(BuiltInCategory.OST_Conduit)
                        .WhereElementIsNotElementType()
                        .ToElements();

                    #region Create

                    XYZ x = null;
                    XYZ y = null;
                    double pointX = 0;
                    double pointY = 0;
                    double degree = 0;
                    foreach (var element in efCs)
                    {
                        LocationPoint locationPoint = element.Location as LocationPoint;

                        // var name = element.Name;
                        pointX = locationPoint.Point.X;
                        pointY = locationPoint.Point.Y;
                        var pointZ = locationPoint.Point.Z;

                        degree = locationPoint.Rotation;
                        if (degree < 2.00)
                        {
                            degree *= 2;
                        }


                        foreach (var element2 in conduintsElements)
                        {
                            LocationCurve curve = element2.Location as LocationCurve;

                            x = curve.Curve.GetEndPoint(0);
                            y = curve.Curve.GetEndPoint(1);
                            //MM_StrutClearance

                            #region Diameter definition

                            string _parameterSize =
                                element2.LookupParameter("Diameter(Trade Size)").AsValueString();
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

                                    double rotationCellValue = degree + 90 * Math.PI / 180;
                                    string symbolName = "MM_Conduit_Strap";

                                    FilteredElementCollector allFamilySymbols = new FilteredElementCollector(doc)
                                        .OfClass(typeof(ElementType));
                                    // FamilySymbol selectedFamilySymbol = allFamilySymbols
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
                                            elementType,
                                            famStructuralType);
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