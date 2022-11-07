using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using adWin = Autodesk.Windows;
using Autodesk.Revit.UI;
using System.Linq;
using System.Collections.Generic;

namespace InstallConduitsStrap.RevitUI
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RibbonPanelOverrides
    {
        public string TabName { get;}
        /// <summary>
        /// Class to override panels ribbon Tab.
        /// </summary>
        /// <param name="tabName">Name of the custom Tab on ribbon which panels need to override.</param>
        internal RibbonPanelOverrides(string tabName)
        {
            TabName = tabName;
        }

        /// <summary>
        /// Overrides the Panel Title background with solid color.
        /// </summary>
        internal void SolidColorPanelTitleOverride()
        {
            adWin.RibbonControl revitRibbon = adWin.ComponentManager.Ribbon;
            adWin.RibbonTab tab = revitRibbon.Tabs.FirstOrDefault(x => x.Name == TabName);

            adWin.RibbonPanelCollection ribbonPanels = tab.Panels;


            ribbonPanels.FirstOrDefault(x => x.Source.Name == "Install")
                .CustomPanelTitleBarBackground = new SolidColorBrush(Colors.DarkOliveGreen);
            ribbonPanels.FirstOrDefault(x => x.Source.Name == "Install")
                .CustomPanelTitleBarBackground.Opacity = 0.2;

        }
    }
}