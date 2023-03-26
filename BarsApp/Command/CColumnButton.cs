using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using System.Text;

namespace BarsApp
{
    [Transaction(TransactionMode.Manual)]
    class CColumnButton : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Document doc = this.ActiveUIDocument.Document;

            #region Переменные
            
            List<Element> cols = null;
            ICollection<ElementId> selected = uidoc.Selection.GetElementIds();
            IList<ElementId> idsToSelect = new List<ElementId>();

            string parameterBaseElevation = "Рзм.ОтметкаНиза";
            string parameterTopElevation = "Рзм.ОтметкаВерха";
            int count = 0;
            int errorCount = 0;
            #endregion

            #region Собираем элементы

            if (selected.Count > 0) //Если пользователь выбрал
            {
                cols = new FilteredElementCollector(doc, selected)
                    .OfCategory(BuiltInCategory.OST_StructuralColumns)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .ToList();
            }

            else // Если ничего не выбрано
            {
                cols = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_StructuralColumns)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .ToList();
            }


            #endregion


            using (Transaction t = new Transaction(doc))
            {
                t.Start("Colums elevation");
                foreach (Element col in cols)
                {
                    try
                    {
                        ElementId baseLevelId = col.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM).AsElementId();
                        Level baseLevel = doc.GetElement(baseLevelId) as Level;
                        double baseLevelHeigth = baseLevel.Elevation;
                        double baseOffset = col.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM).AsDouble();
                        double baseElev = (baseLevelHeigth + baseOffset) / 1000;

                        ElementId topLevelId = col.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId();
                        Level topLevel = doc.GetElement(topLevelId) as Level;
                        double topLevelHeigth = topLevel.Elevation;
                        double topOffset = col.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                        double topElev = (topLevelHeigth + topOffset) / 1000;

                        col.LookupParameter(parameterBaseElevation).Set(baseElev);
                        col.LookupParameter(parameterTopElevation).Set(topElev);
                        count++;
                    }
                    catch
                    {
                        errorCount++;
                        idsToSelect.Add(col.Id);

                    }
                }
                uidoc.Selection.SetElementIds(idsToSelect);
                t.Commit();
            }

            TaskDialog.Show("Отметки колонн", "Обработано колонн: " + count + ", ошибок: " + errorCount);
            if (idsToSelect.Count > 0) { TaskDialog.Show("Отметки колонн", "Выделены необработанные колонны"); }

        
            return Result.Succeeded;
        }
    }
}
