using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace Revit_Sandbox.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CalculateAirflowCommand : IExternalCommand
    {
        private double totalAirflow = 0;
        private List<ElementId> traversedElementIds = new List<ElementId>();
        private Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDocument = commandData.Application.ActiveUIDocument;
            var document = uiDocument.Document;
            doc = document;
            var elementId = uiDocument.Selection.PickObject(ObjectType.Element, "Please select an element to calculate total airflow.");
            if (elementId == null) return Result.Cancelled;

            var selectedElement = document.GetElement(elementId);

            CalculateTotalAirflow(selectedElement);

            TaskDialog.Show("Airflow", $"AirFlow: {totalAirflow}", TaskDialogCommonButtons.Ok);

            return Result.Succeeded;
        }

        private static ConnectorSet GetConnectors(Element element)
        {
            if (element is FamilyInstance fi && fi.MEPModel != null)
            {
                return fi.MEPModel.ConnectorManager?.Connectors;
            }
            else if (element is MEPCurve duct)
            {
                return duct.ConnectorManager?.Connectors;
            }

            return null;
        }

        private void CalculateTotalAirflow(Element element)
        {
            totalAirflow = 0;
            traversedElementIds = new List<ElementId>();

            CalculateAirflow(element);
        }

        private void CalculateAirflow(Element element)
        {
            var airflow = element.get_Parameter(BuiltInParameter.RBS_DUCT_FLOW_PARAM)?.AsDouble() ?? 0;
            var airflowInLiterPerSecond = UnitUtils.ConvertFromInternalUnits(airflow, UnitTypeId.LitersPerSecond);
            totalAirflow += Math.Round(airflowInLiterPerSecond);

            traversedElementIds.Add(element.Id);

            ConnectorSet connectorSet = GetConnectors(element);
            if (connectorSet != null)
            {
                foreach (Connector connector in connectorSet)
                {
                    foreach (Connector cRef in connector.AllRefs)
                    {
                        if (cRef.Owner?.Id != element.Id && cRef.Owner?.UniqueId != null && !traversedElementIds.Contains(cRef.Owner.Id))
                        {
                            // Continue calculating for the connected element
                            CalculateAirflow(doc.GetElement(cRef.Owner.UniqueId));
                        }
                    }
                }
            }
        }
    }
}
