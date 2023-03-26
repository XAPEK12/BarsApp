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
    class COpeningsButton : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            #region Переменные
           
            List<Element> opns = null;
            ICollection<ElementId> selected = uidoc.Selection.GetElementIds();
            IList<ElementId> idsToSelect = new List<ElementId>();

            string parameterBaseElevation = "Рзм.ОтметкаНиза";
            string parameterTopElevation = "Рзм.ОтметкаВерха";
            string parameterCenterElevation = "Рзм.ОтметкаЦентра";
            int count = 0;
            int errorCount = 0;
            double divider = 304.8;
            #endregion

            #region Собираем элементы

            if (selected.Count > 0) //Если пользователь выбрал
            {
                opns = new FilteredElementCollector(doc, selected)
                    .OfCategory(BuiltInCategory.OST_GenericModel)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .ToList();
            }

            else // Если ничего не выбрано
            {
                opns = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_GenericModel)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .ToList();
            }


            #endregion

            #region Вычисления
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Openings elevation");
                foreach (Element opn in opns)
                {
                    if (((opn as FamilyInstance).Symbol.FamilyName.ToLower().Contains("про")
                        || (opn as FamilyInstance).Symbol.FamilyName.ToLower().Contains("отв")
                        || (opn as FamilyInstance).Symbol.FamilyName.ToLower().Contains("две"))
                       & (opn as FamilyInstance).Symbol.FamilyName.ToLower().Contains("плит") == false)
                    {
                        try
                        {
                            ElementId baseLevelId = opn.LevelId;
                            Level baseLevel = doc.GetElement(baseLevelId) as Level;
                            double baseLevelHeigth = 0;
                            double baseOffset = 0;
                            double height = 0;

                            UnitFormatUtils.TryParse(doc.GetUnits(), UnitType.UT_Number,
                                                     baseLevel.LookupParameter("Фасад").AsValueString(), out baseLevelHeigth);
                            UnitFormatUtils.TryParse(doc.GetUnits(), UnitType.UT_Number,
                                                     opn.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM).AsValueString(), out baseOffset);
                            UnitFormatUtils.TryParse(doc.GetUnits(), UnitType.UT_Number,
                                                     opn.LookupParameter("Высота").AsValueString(), out height);

                            double baseElev = (baseLevelHeigth + baseOffset) / 1000;
                            double topElev = (baseLevelHeigth + baseOffset + height) / 1000;
                            double centerElev = (baseLevelHeigth + baseOffset + height) / 2 / 1000;

                            opn.LookupParameter(parameterBaseElevation).Set(baseElev / divider);
                            opn.LookupParameter(parameterTopElevation).Set(topElev / divider);
                            opn.LookupParameter(parameterCenterElevation).Set(centerElev / divider);
                            count++;
                        }
                        #endregion

                        #region выделяем ошибки
                        catch
                        {
                            errorCount++;
                            idsToSelect.Add(opn.Id);
                        }
                    }
                }
                uidoc.Selection.SetElementIds(idsToSelect);
                t.Commit();
            }
            #endregion

            #region Диалоговые окна
            TaskDialog.Show("Отметки отверстий", "Обработано отверстий: " + count + ", ошибок: " + errorCount);
            if (idsToSelect.Count > 0) { TaskDialog.Show("Отметки отверстий", "Выделены необработанные отверстия"); }
            #endregion


            return Result.Succeeded;

        }
    }
}
