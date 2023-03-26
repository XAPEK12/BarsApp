using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace BarsApp
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {            
            RibbonPanel panel = ribbonPanel(a); // Method to add Tab and Panel             
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location; // Reflection to look for this assembly path 

            // Создаем кнопки
            PushButton wallButton = panel.AddItem(new PushButtonData
                ("WallButton", "Стены", thisAssemblyPath, "BarsApp.CWallButton")) 
                as PushButton;
            PushButton columnButton = panel.AddItem(new PushButtonData
                ("ColumnButton", "Колонны", thisAssemblyPath, "BarsApp.CColumnButton"))
                as PushButton;
            PushButton openingsButton = panel.AddItem(new PushButtonData
                ("OpeningsButton", "Отверстия", thisAssemblyPath, "BarsApp.COpeningsButton"))
                as PushButton;

            // Добавляем подсказки
            wallButton.ToolTip = "Заполнить параметры Разм.ОтметкаНиза\\Разм.ОтметкаВерха для СТЕН";
            columnButton.ToolTip = "Заполнить параметры Разм.ОтметкаНиза\\Разм.ОтметкаВерха для КОЛОНН";
            openingsButton.ToolTip = "Заполнить параметры Разм.ОтметкаНиза\\Разм.ОтметкаВерха для ОТВЕРСТИЙ";

            //Вставляем картинки на кнопки
            var globePath = System.IO.Path.Combine
                (System.IO.Path.GetDirectoryName
                (System.Reflection.Assembly.GetExecutingAssembly()
                .Location), "Images", "BARSImg.png"); // Reflection of path to image 
            Uri uriImage = new Uri(globePath);            
            BitmapImage largeImage = new BitmapImage(uriImage); // Apply image to bitmap            
            wallButton.LargeImage = largeImage; // Apply image to button 
            columnButton.LargeImage = largeImage;
            openingsButton.LargeImage = largeImage;
                        
            a.ApplicationClosing += a_ApplicationClosing;

            //Set Application to Idling
            a.Idling += a_Idling;

            return Result.Succeeded;
        }

        //*****************************a_Idling()*****************************
        void a_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {

        }

        //*****************************a_ApplicationClosing()*****************************
        void a_ApplicationClosing(object sender, Autodesk.Revit.UI.Events.ApplicationClosingEventArgs e)
        {
            throw new NotImplementedException();
        }

        //*****************************ribbonPanel()*****************************
        public RibbonPanel ribbonPanel(UIControlledApplication a)
        {
            string tab = "BARS"; // Tab name            
            RibbonPanel ribbonPanel = null; // Empty ribbon panel 

            // Try to create ribbon tab. 
            try
            {
                a.CreateRibbonTab(tab);
            }
            catch { }

            // Try to create ribbon panel.
            try
            {
                RibbonPanel wallPanel = a.CreateRibbonPanel(tab, "Отметка верха\\низа");                
            }
            catch { }

            // Search existing tab for your panel.
            List<RibbonPanel> panels = a.GetRibbonPanels(tab);
            foreach (RibbonPanel p in panels)
            {
                if (p.Name == "Отметка верха\\низа")
                {
                    ribbonPanel = p;
                }
            }
            
            return ribbonPanel; //return panel 
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
