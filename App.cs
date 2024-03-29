﻿using System.Reflection;
using Autodesk.Revit.UI;
using InstallConduitsStrap.RevitUI;

namespace InstallConduitsStrap
{
    public class App : IExternalApplication
    {
        private string path = Assembly.GetExecutingAssembly().Location;

        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Install Strap";
            application.CreateRibbonTab(tabName);

            RibbonPanel InstallStrap = application.CreateRibbonPanel(tabName, "Install");

            #region Button

            PushButtonData Install = new PushButtonData(nameof(Tools.InstallStrap),
                "Install Conduits \n Strap", path,
                typeof(Tools.InstallStrap).FullName)
            {
                LargeImage = Common.IconManager.GetIcon("install_32px.png"),
                Image = Common.IconManager.GetIcon("install_16px.png")
            };

            #endregion

            RibbonPanelOverrides ribbonPanelOverrides = new RibbonPanelOverrides(tabName);
            ribbonPanelOverrides.SolidColorPanelTitleOverride();

            InstallStrap.AddItem(Install);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}