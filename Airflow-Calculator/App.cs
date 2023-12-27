using Autodesk.Revit.UI;
using Revit_Sandbox.Commands;
using System.Reflection;

namespace Revit_Sandbox
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab("BIMLOGIQ");
            var calculatorPanel = application.CreateRibbonPanel("BIMLOGIQ", "Calculators");
            calculatorPanel.AddItem(new PushButtonData("CalculateAirflowButton", "Calculate Airflow", Assembly.GetExecutingAssembly().Location, typeof(CalculateAirflowCommand).FullName));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // Nothing to do for the time being
            return Result.Succeeded;
        }
    }
}
