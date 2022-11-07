using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


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
                
                ICollection<ElementId> na = new List<ElementId>();
                na.Add(sheet.Id);
                ElementOwnerViewFilter bbf = new ElementOwnerViewFilter(sheet.Id);

                ExclusionFilter noView = new ExclusionFilter(na);
                LogicalAndFilter summFilter = new LogicalAndFilter(bbf, noView);


                FilteredElementCollector conduintsFilter = new FilteredElementCollector(doc, sheet.Id);
                FilteredElementCollector fittingsFilter = new FilteredElementCollector(doc, sheet.Id);

                FilteredElementCollector electricalFixturesFilter = new FilteredElementCollector(doc, sheet.Id);


                // electricalFixturesFilter = electricalFixturesFilter.OfClass(typeof(Family));
                //
                // // Get Element Id for family symbol which will be used to find family instances
                // var query = from element in electricalFixturesFilter
                //     where ((FamilyInstance)element).Symbol.Name.Equals("MM_SupportTrapeze")
                //     select element;
                // List<Element> famSyms = query.ToList<Element>();
                // ElementId symbolId = famSyms[0].Id;
                //
                // // Create a FamilyInstance filter with the FamilySymbol Id
                // FamilyInstanceFilter filter = new FamilyInstanceFilter(doc, symbolId);
                //
                // // Apply the filter to the elements in the active document
                // electricalFixturesFilter = new FilteredElementCollector(doc, sheet.Id);
                // ICollection<Element> familyInstances = electricalFixturesFilter.WherePasses(filter).ToElements();
                //
                //
                // var electricalFixturesElements = GetInstancesBySymbol(doc, sheet.Id,);
                IList<Element> efC = FindElement(doc, sheet.Id, typeof(FamilyInstance), "C");
                IList<Element> efD = FindElement(doc, sheet.Id, typeof(FamilyInstance), "D");
                IList<Element> efD2 = FindElement(doc, sheet.Id, typeof(FamilyInstance), "D2");
                IList<Element> efCi = FindElement(doc, sheet.Id, typeof(FamilyInstance), "Ci");
                
                



                /*#region Delete
                IList<ElementId> deletEl = new List<ElementId>();
                foreach (Element e in efC)
                {
                    deletEl.Add(e.Id);
                }

                foreach (Element e in efD)
                {
                    deletEl.Add(e.Id);
                }

                foreach (Element e in efD2)
                {
                    deletEl.Add(e.Id);
                }

                if (efCi != null)
                {
                    foreach (Element e in efCi)
                    {
                        deletEl.Add(e.Id);
                    }
                }
                

                using (Transaction t = new Transaction(doc, "Delete element"))
                {
                    t.Start();

                    foreach (ElementId id in deletEl)
                    {
                        doc.Delete(id);
                    }

                    t.Commit();
                }
                

                #endregion*/

                IList<Element> fittings = FindElement(doc, sheet.Id, typeof(FamilyInstance), "Standard");

                IList<Element> conduintsElements = conduintsFilter
                    .OfCategory(BuiltInCategory.OST_Conduit)
                    .WhereElementIsNotElementType()
                    .ToElements();
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

        static IEnumerable<string> GetFamilyNames(
            Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(ElementType))
                .Cast<ElementType>()
                .Select<ElementType, string>(a => a.FamilyName)
                .Distinct<string>();
        }
    }
}