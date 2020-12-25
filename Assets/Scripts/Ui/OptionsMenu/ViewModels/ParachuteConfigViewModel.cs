using Assets.Scripts.OptionsMenu;
using UnityEngine;

namespace RamjetAnvil.Volo.Ui {
    public class ParachuteConfigViewModel {
        public readonly GuiComponentDescriptor.Range NumCells; // int
        public readonly GuiComponentDescriptor.Range PressureMultiplier;
        //Num Toggle Controlled Cells: 0 - floor(NumCells/2)
        public readonly GuiComponentDescriptor.Range NumToggleControlledCells; // int

        public ParachuteConfigViewModel(ITypedDataCursor<ParachuteConfig> parachuteConfig) {
            var c = parachuteConfig.Get;

            NumCells = new GuiComponentDescriptor.Range(
                "Number of cells",
                minValue: 3f,
                maxValue: 27f,
                updateValue: value => {
                    var config = c();
                    int val = (int) value;
                    val = (val%2 == 0) ? val + 1 : val; //Todo: respect min/max
                    config.NumCells = (int) val;
                    parachuteConfig.Set(config);
                },
                currentValue: () => c().NumCells,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(c().NumCells, str, decimalPlaces: 0);
                }, 
                stepSize: 2f);
            NumToggleControlledCells = new GuiComponentDescriptor.Range(
                "Number of braked cells",
                minValue: 0f,
                maxValue: 13f,
                updateValue: value => {
                    var config = c();
                    config.NumToggleControlledCells = (int) value;
                    parachuteConfig.Set(config);
                },
                currentValue: () => c().NumToggleControlledCells,
                updateDisplayValue: (descriptor, str) => {
                    descriptor.MaxValue = Mathf.Floor(c().NumCells / 2f);
                    GuiComponentDescriptor.DisplayNumber(c().NumToggleControlledCells, str, decimalPlaces: 0);
                }, 
                stepSize: 1f);
            PressureMultiplier = new GuiComponentDescriptor.Range(
                "Pressure multiplier",
                minValue: 0.5f,
                maxValue: 4.0f,
                updateValue: value => {
                    var config = c();
                    config.PressureMultiplier = value;
                    parachuteConfig.Set(config);
                },
                currentValue: () => c().PressureMultiplier,
                updateDisplayValue: (descriptor, str) => {
                    GuiComponentDescriptor.DisplayNumber(c().PressureMultiplier, str, decimalPlaces: 1, postFix: "×");
                }, 
                stepSize: 0.2f);
//            RearRiserPullMagnitude = new GuiComponentDescriptor.Range(
//                "Rear riser pull magnitude",
//                minValue: 0.01f,
//                maxValue: 0.1f,
//                updateValue: value => {
//                    var config = c();
//                    config.RearRiserPullMagnitude = value;
//                    parachuteConfig.Set(config);
//                },
//                currentValue: () => c().RearRiserPullMagnitude,
//                updateDisplayValue: (descriptor, str) => {
//                    GuiComponentDescriptor.DisplayNumber(c().RearRiserPullMagnitude, str, decimalPlaces: 2);
//                }, 
//                stepSize: 0.01f);
//            FrontRiserPullMagnitude = new GuiComponentDescriptor.Range(
//                "Front riser pull magnitude",
//                minValue: 0.01f,
//                maxValue: 0.2f,
//                updateValue: value => {
//                    var config = c();
//                    config.FrontRiserPullMagnitude = value;
//                    parachuteConfig.Set(config);
//                },
//                currentValue: () => c().FrontRiserPullMagnitude,
//                updateDisplayValue: (descriptor, str) => {
//                    GuiComponentDescriptor.DisplayNumber(c().FrontRiserPullMagnitude, str, decimalPlaces: 2);
//                }, 
//                stepSize: 0.01f);
        }
    }
}
