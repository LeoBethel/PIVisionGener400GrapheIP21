using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PiVisionJsonGenerator.Models
{
    public class Courbe
    {
        public string TAGPIVISION { get; set; }
        public bool? EXPRESSION { get; set; }
        public string TITRE_TAG { get; set; }
        public string TITRE_EXPRESSIONPIVISION { get; set; }
        
        public double? MIN { get; set; }
        public double? MAX { get; set; }
        public string COULEUR { get; set; }
        public int? EPAISSEUR { get; set; }
        public int? ORDRE { get; set; }
        public string TRAIT { get; set; }
        public string LEGENDE { get; set; }
        public string TYPE_ECHANTILLONNAGE { get; set; }
        public int? PERIODE { get; set; }
        public string PERIODE_UNIT { get; set; }
        public bool? STEPPED { get; set; }
        public bool? VISIBLE { get; set; }
        public string MAP { get; set; } = "";
        public bool EXPORT { get; set; }
        public decimal? EPAISSEURPIVISION
        {
            get
            {
                switch (EPAISSEUR)
                {
                    case 1: return Convert.ToDecimal(1.7);
                    case 2: return Convert.ToDecimal(2.2);
                    case 3: return Convert.ToDecimal(2.7);
                    case 4: return Convert.ToDecimal(3.2);
                    case 5: return Convert.ToDecimal(3.7);
                    case 6: return Convert.ToDecimal(4.2);
                    case 7: return Convert.ToDecimal(4.7);
                    default: return Convert.ToDecimal(1.7);
                }

            }
        }
    }

    public class Graphique
    {
        public int LIGNE { get; set; }
        public int COLONNE { get; set; }
        public string TITRE_GRAPH { get; set; }
        public List<Courbe> COURBES { get; set; }
    }

    public class Vue
    {
        public string NO_VUE { get; set; }
        public string DISPOSITION { get; set; }
        public string TYPE_GRAPHIQUE { get; set; }
        public string DOSSIER_PIVISION { get; set; }
        public List<Graphique> GRAPHIQUES { get; set; }
    }
    public class DisplayModel
    {
        public List<Symbol> Symbols { get; set; }
        public List<string> AttachmentIds { get; set; }
        public string RequestId { get; set; }
        public string StartTime { get; set; } = "*-8h";

        public string EndTime { get; set; } = "*";
        public string LegacyDisplay { get; set; }
        public string EventFramePath { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Owner { get; set; } = "OCEANIA\\ext-lwenehoua";
        public bool ReadOnly { get; set; } = false;
     
        public int Revision { get; set; } =  0;

        public DisplayProperties DisplayProperties { get; set; }
        public string ProductVersion { get; set; } = "3.10";
    }

    public class Symbol
    {
        public List<string> DataSources { get; set; }
        public string Name { get; set; }
        public string SymbolType { get; set; }
        public Configuration Configuration { get; set; }  // si tu veux le parser plus tard        

    }

    public class DisplayProperties
    {
        // Tu peux affiner selon le contenu réel de DisplayProperties
        public string BackgroundColor { get; set; } 
        public bool ShowAssetPaths { get; set; }
        public bool FitAll { get; set; }
        public List<Calculation> Calculations { get; set; }
    }

    public class Calculation
    {
        public string Name { get; set; }                  // Peut être null
        public string Description { get; set; }           // Peut être null
        public string Server { get; set; }                // Ex: "10.110.1.40?UUID"
        public string Expression { get; set; }            // Formule calculée
        public string IntervalMode { get; set; }          // "Auto", "Manual", etc.
        public string RefreshInterval { get; set; }       // Peut être null
        public string CalcInterval { get; set; }          // Peut être null 
        public string SyncTime { get; set; }              // Format "HH:mm:ss"
        public string ConversionFactor { get; set; }      // Ex: "1d"
        public bool Stepped { get; set; }                 // true/false
    }
   
    public class TrendConfig
    {
        public ValueScale ValueScale { get; set; }
        public TimeScale TimeScale { get; set; }
        public int Padding { get; set; }
        public bool NowPosition { get; set; }
       // public bool gridlines { get; set; } = true;
        
        public int LegendWidth { get; set; }
        public string BackgroundColor { get; set; }
    }

    public class ValueScale
    {
        public bool Axis { get; set; }
        public bool TickMarks { get; set; }
        public bool bands { get; set; }
        public int Padding { get; set; }
    }

    public class TimeScale
    {
        public bool Axis { get; set; }
        public bool TickMarks { get; set; }
    }

    public class ValueScaleSetting
    {
        public int MinType { get; set; }
        public int MaxType { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
    }

    public class TraceSetting
    {
        public bool Hidden { get; set; }
        public bool ShouldSerializeHidden()
        {
            return !Hidden;
        }
        public string NameType { get; set; }
        public string StrokeStyle { get; set; }
        public bool ShouldSerializeStrokeStyle()
        {
            return !string.IsNullOrWhiteSpace(StrokeStyle);
        }
        public string Color { get; set; }
        public decimal? StrokeWidth { get; set; }
        public ValueScaleSetting ValueScaleSetting { get; set; }
    }   
    public class Configuration
    {
        // Attribut communes
        public int Top { get; set; }
        public int Left { get; set; }
        public string DataShape { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }

        //Attribut specifique pour les graphes "Trend"
        public TrendConfig TrendConfig { get; set; }
        public bool MultipleScales { get; set; }
        public ValueScaleSetting ValueScaleSetting { get; set; }
        public int TimeScaleType { get; set; } = 1;
        public bool NowPosition { get; set; }
        public List<TraceSetting> TraceSettings { get; set; }
        public bool CursorDragValues { get; set; }      

        public string Title { get; set; }
        public bool Description { get; set; } 
        public string  TextColor { get; set; } = "#000000";
        // Attribut specifique pour les graphe "XYPlot"
        public string FormatType { get; set; }
        public int Intervals { get; set; } = 10000;
        public string BackgroundColor { get; set; } = "#ffffff";
        public bool ShowTitle { get; set; } = false;      
        public string TitlePosition { get; set; } = "top";
        public string TitleColor { get; set; } = "white";
        public bool ShowXAxisLabel { get; set; } = true;
        public string XAxisLabelType { get; set; } = "sourcedata";
        public string XAxisCustomLabel { get; set; } = "";
        public bool ShowYAxisLabel { get; set; } = true;
        public string YAxisLabelType { get; set; } = "sourcedata";
        public string YAxisCustomLabel { get; set; } = "";
        public bool ShowEngineeringUnits { get; set; } = true;
        public bool ShowGrid { get; set; } = true;
        public string GridColor { get; set; } = "#778899";
        public bool ShowLegend { get; set; } = true;
        public string LegendPosition { get; set; } = "right";
        public string LegendColor { get; set; } = "#000000";
        public string ScaleColor { get; set; } = "#000000";
        public string ScaleFormat { get; set; } = "autorange";
        public string NumberFormat { get; set; } = "Database";
        public int NumberDecimals { get; set; } = 2;
        public bool NumberThousands { get; set; } = true;
        public bool ShowLine { get; set; } = false;
        public double XScaleMin { get; set; } = 0;
        public double XScaleMax { get; set; } = 100;
        public double YScaleMin { get; set; } = 0;
        public double YScaleMax { get; set; } = 100;
        public List<DataSetting> DataSettings { get; set; }
        public bool zoomToggle { get; set; } = true;       
        public bool MultiScale { get; set; } = true;

        // Attributs spécifiques aux "statictext"
        public bool? SetTextFromLink { get; set; }
        public string StaticText { get; set; }
        public string LinkURL { get; set; }
        public string Fill { get; set; }
        public string Stroke { get; set; }
        public int? Rotation { get; set; }
        public bool? UseIntegerFontSize { get; set; }
        public double? WidthNullable { get; set; } // pour éviter conflit avec Width déjà défini
        public bool? Align { get; set; }
    }
    
    public class DataSetting
    {
        public int? Interval { get; set; }
        public string IntervalType { get; set; }
        public bool? IsX { get; set; }
        public bool? ShowCorrelationLine { get; set; }
        public bool? ShowCorrelationCoefficient { get; set; }
        public string Color { get; set; }
        public string MarkerStyle { get; set; }
        public string EndingMarkerColor { get; set; }
    }

}
