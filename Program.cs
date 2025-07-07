using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PiVisionJsonGenerator.Models;

namespace PiVisionJsonGenerator
{
    internal class Program
    {
        // Adresse fixe du serveur PI
        const string PiServer = "10.110.1.40?268c8fff-12ed-4ab9-ae7d-6f71ef4bdfac";

        // Dossiers d'entrée/sortie
        static string baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FolderJson");
        static string outputFolder = @"C:\\Users\\lwene\\Documents\\Drehu Data Solution\\02 - CLIENTS\\00 - SLN\\03 - SLN - Réalisation de Projets\\01 - Lot 4 - Import Automatique  des Graphes IP21 SGSP\\PI Vision - OutPut Files\";

        // Fichier JSON principale
        static string dataPath = Path.Combine(baseFolder, "01 - Graphiques_Regroupes_Par_Vue.json");

        static void Main(string[] args)
        {
            // Chargement des données depuis fichier JSON
            List<Vue> vues = JsonConvert.DeserializeObject<List<Vue>>(File.ReadAllText(dataPath)); ;

            // Préparation du dossier pour enregistrer les fichier .pdx et les dossiers contenant les fichiers JSON
            string outputRoot = Path.Combine(outputFolder, "pdix_output");
            Directory.CreateDirectory(outputRoot);

            // Traitement des 5 premières vues
            int idStart = 50291;
           // var listNo_Vue= vues.Select(s=>s.NO_VUE).ToList();
           // string outputPathList = Path.Combine(outputFolder, "Liste_No_Vue.txt");
           // File.WriteAllLines(outputPathList, listNo_Vue);
            foreach (Vue vue in vues)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Traitement : {vue.NO_VUE}");
                Console.ResetColor();

                TraitementVue(vue, idStart++, outputRoot);
            }

            Console.WriteLine("Les fichiers .pdix ont été générés !");
            Console.WriteLine("Appuie sur une touche pour fermer...");
            Console.ReadKey();
        }


        // Traitement d'une vue : création du modèle, des symboles, et des fichiers
        static void TraitementVue(Vue vue, int id, string outputRoot)
        {
            DisplayModel display = InitialiserDisplay(vue, id);
            int k = 0;int j = 0;
           
                foreach (Graphique graphique in vue.GRAPHIQUES)
                {
                    Symbol symbol = ConstruireSymbol(graphique, vue.TYPE_GRAPHIQUE, vue.DISPOSITION, k++);
               
                    // Ajout des courbes dans chaque graphique
                    foreach (Courbe courbe in graphique.COURBES.OrderBy(c => c.ORDRE))
                    {
                        AjouterCourbe(symbol, courbe, display, vue.DISPOSITION, j++);
                    }
                                       
                    display.Symbols.Add(symbol);
                }
                if (vue.TYPE_GRAPHIQUE == "XY")
                {
                    string title = vue.GRAPHIQUES.Select(s => s.TITRE_GRAPH).FirstOrDefault();
                    display.Symbols.Add(new Symbol
                    {
                        Name = "Symbole" + k,
                        SymbolType = "statictext",
                        Configuration = new Configuration
                        {
                            Top = 14,
                            Left = 846,
                            Height = 35,
                            SetTextFromLink = false,
                            StaticText = $"<span style=\"font-weight: bold;\">{title}</span>",                           
                            LinkURL = "",
                            Fill = "rgba(255,255,255,0)",
                            Stroke = "#000000",
                            Rotation = 0,
                            FontName = "Arial",
                            FontSize = 18,
                            UseIntegerFontSize = false,
                            Width = 471,
                            Align = true,
                            BackgroundColor= "transparent"
                            
                        }

                    });
                    display.StartTime = "*-2d";
                }
            // Sauvegarde en fichiers + archive
           // SauvegarderVue(display, vue.NO_VUE, outputRoot);
            SauvegarderVue(display, vue.NO_VUE, vue.DOSSIER_PIVISION,outputRoot);
        }
        // Création du symbole (graphique) avec sa configuration
        static Symbol ConstruireSymbol(Graphique graphique, string typeGraphique, string disposition, int index)
        {
            Symbol symbol = new Symbol();
            symbol.Name = "Symbol" + index;
           
            if (typeGraphique !="XY")
             { 
                symbol.SymbolType = "trend";
                symbol.DataSources = new List<string>();
                symbol.Configuration = new Configuration
                {
                    DataShape = "Trend",
                    TrendConfig = new TrendConfig
                    {
                        ValueScale = new ValueScale { Axis = false, TickMarks = true, bands=false, Padding = 2 },
                        TimeScale = new TimeScale { Axis = true, TickMarks = true },
                        Padding = 2,
                        NowPosition = true,
                        LegendWidth = 320
                    },
                    MultipleScales = true,
                    ValueScaleSetting = new ValueScaleSetting { MinType = 0, MaxType = 0 },                   
                    NowPosition = true,
                    TraceSettings = new List<TraceSetting>(),
                    CursorDragValues = true,                    
                    Title = graphique.TITRE_GRAPH,
                    Description = true
                };
            }
            else
            {                
                symbol.SymbolType = "xyplot";
                symbol.Configuration = new Configuration
                {
                    DataShape = "XY",
                    DataSettings = new List<DataSetting>(),                   
                };
            }
            symbol.Configuration.FontName = "Arial";
            symbol.Configuration.FontSize = 9;
            symbol.DataSources = new List<string>();
            
            // Positionnement du symbole selon la disposition
            AppliquerDisposition(symbol, disposition, typeGraphique, index);
            return symbol;
        }

// Initialisation du modèle Display pour une vue
static DisplayModel InitialiserDisplay(Vue vue, int id)
        {
            return new DisplayModel
            {
                Id = id,
                Name = vue.NO_VUE,
                Symbols = new List<Symbol>(),
                AttachmentIds = new List<string>(),
                DisplayProperties = new DisplayProperties
                {
                    BackgroundColor = "#ffffff",
                    ShowAssetPaths = true,
                    FitAll = true,
                    Calculations = new List<Calculation>()
                },
                
                ProductVersion = "3.10"
            };
        }

    
        // Applique la taille et position du graphique en fonction de la disposition
        static void AppliquerDisposition(Symbol symbol, string disposition, string typeGraphique,int index)
        {
            var top = new List<int>(); 
            var left = new List<int>();

            if (typeGraphique !="XY")
            {
                top= GetTop(disposition);
                left= GetLeft(disposition);
                if (disposition.StartsWith("1C"))
                {
                    symbol.Configuration.Width = 2500;
                    symbol.Configuration.Left = 6;
                }
                else if (disposition.StartsWith("2C"))
                {
                    symbol.Configuration.Width = 1232;
                    symbol.Configuration.Left = (left != null && index < left.Count) ? left[index] : 6;
                }
                else if (disposition.StartsWith("3C"))
                {
                    symbol.Configuration.Width = 812;
                    symbol.Configuration.Left = (left != null && index < left.Count) ? left[index] : 6;
                }
                symbol.Configuration.Top = top != null && index < top.Count ? top[index] : 100;
            }
            else
            {
                top = GetTopXY(disposition);
                left = GetLeftXY(disposition);
                symbol.Configuration.Width = 1116;
                symbol.Configuration.Top = top[index];
                symbol.Configuration.Left = left[index];
            }
            
                       

            switch (disposition)
            {
                case "1C-5L":
                case "2C-5L":
                    symbol.Configuration.Height = 200;
                    break;
                case "1C-4L":
                case "2C-4L":
                    symbol.Configuration.Height = 250;
                    break;
                case "1C-3L":
                case "2C-3L":
                    symbol.Configuration.Height = 340;
                    break;
                case "1C-2L":
                case "2C-2L":
                    symbol.Configuration.Height = 520;
                    break;
                case "1C-1L":
                    symbol.Configuration.Height = 690;
                    symbol.Configuration.Top = 100;
                    break;
                case "3C-1L":
                    symbol.Configuration.Height = 690;
                    break;
                case "2C-2L-3":
                    symbol.Configuration.Height = 480;
                    break;
                case "2C-1L-2":
                    symbol.Configuration.Height = 580;
                    break;
                default:
                    symbol.Configuration.Height = 690;
                    symbol.Configuration.Width = 2500;
                    symbol.Configuration.Left = 6;
                    break;
            }
        }


        // Ajoute une courbe au symbole et éventuellement au modèle
        static void AjouterCourbe(Symbol symbol, Courbe courbe, DisplayModel display, string disposition, int index)
        {
            if(courbe.EXPORT !=false)
            {

            string style = null;
            if (courbe.TRAIT == "Dot") style = "2.5,2";
            else if (courbe.TRAIT == "Dash") style = "6,2";

            TraceSetting trace = new TraceSetting();
            DataSetting data = new DataSetting();
            trace.ValueScaleSetting = new ValueScaleSetting();
            if (symbol.SymbolType == "trend")
            {
                trace.Hidden = !(courbe.VISIBLE == true);
                trace.NameType = "D";
                trace.StrokeStyle = style;
                trace.StrokeWidth = courbe.EPAISSEURPIVISION;
                trace.Color = courbe.COULEUR;
                trace.ValueScaleSetting.MinType = 2;
                trace.ValueScaleSetting.MaxType = 2;
                trace.ValueScaleSetting.MinValue = Convert.ToInt32(courbe.MIN);
                trace.ValueScaleSetting.MaxValue = Convert.ToInt32(courbe.MAX);
                symbol.Configuration.TraceSettings.Add(trace);
            }
            else
            {
               // symbolType = "PlotXY"
               if (disposition == "2C-2L-3" && (index == 0 || index==3||index==6))
               {
                   data.Interval = 172;
                   data.IntervalType = "s";
                   data.IsX = true;
               }
               else if (disposition == "2C-2L-2" && (index == 0 || index == 2))
               {
                   data.Interval = 172;
                   data.IntervalType = "s";
                   data.IsX = true;
               }
               else
               {
                   data.ShowCorrelationLine = true;
                   data.ShowCorrelationCoefficient = true;
                   data.Color = courbe.COULEUR;
                   data.MarkerStyle = (index%2 ==0)?"diamond_hollow": "circle_hollow";
                   data.EndingMarkerColor = courbe.COULEUR;
                    data.Interval = 172;
                   data.IntervalType = "s";
               }                
                symbol.Configuration.DataSettings.Add(data);
            }

            if (courbe.EXPRESSION == true)
            {
                Calculation calc = new Calculation
                {
                    Name = courbe.TITRE_EXPRESSIONPIVISION ,
                    Expression = courbe.TAGPIVISION,
                    Server = PiServer,
                    SyncTime = "00:00:00",
                    ConversionFactor = "1d",
                    Stepped = courbe.STEPPED == true
                };
                string calcType = "";
                string period = "";               
                switch (courbe.TYPE_ECHANTILLONNAGE)
                {
                /* case "Avg":
                        calcType = "Average";
                        calc.IntervalMode = "Custom";
                        calc.CalcInterval = "30s";
                        calc.RefreshInterval = "30s";
                        break;      */                
                    case "Interpolated":  
                        calc.IntervalMode = "Custom";
                        calc.CalcInterval = "20m";
                        calc.RefreshInterval = "20m";
                        break;
                        
                    default: 
                        calc.IntervalMode = "Auto";
                        calcType = "Value";
                        break;
                }

                display.DisplayProperties.Calculations.Add(calc);
                symbol.DataSources.Add("calc:" + calc.Name + '.' + calcType);
            }
            else
            {
                symbol.DataSources.Add("pi:\\\\" + PiServer + "\\" + courbe.TAGPIVISION);
            }
            }
        }

        // Sauvegarde du fichier JSON, metadata et génération du .pdix
       /* static void SauvegarderVue(DisplayModel display, string vueName, string outputRoot)
        {
            string vueFolder = Path.Combine(outputRoot, vueName);
            Directory.CreateDirectory(vueFolder);

            string displayPath = Path.Combine(vueFolder, "display_json");
            string typeGraphique = display.Symbols.Select(s => s.SymbolType).FirstOrDefault();
            if (typeGraphique == "trend")
                File.WriteAllText(displayPath, JsonUtils.SerializeWithIgnoredProperties(display, new List<string> {

         "FormatType","Intervals","ShowTitle","TitlePosition","TitleColor","ShowXAxisLabel","XAxisLabelType","XAxisCustomLabel","ShowYAxisLabel","YAxisLabelType",
          "YAxisCustomLabel","ShowEngineeringUnits","ShowGrid","GridColor","ShowLegend","LegendPosition","LegendColor","ScaleColor","ScaleFormat","NumberFormat","NumberDecimals",
          "NumberThousands","ShowLine","XScaleMin","XScaleMax","YScaleMin","YScaleMax","DataSettings","zoomToggle","MultiScale"
          }));
            else
                File.WriteAllText(displayPath, JsonUtils.SerializeWithIgnoredProperties(display, new List<string>()));

            string metadataSrc = Path.Combine(baseFolder, "metadata_json");
            string metadataDest = Path.Combine(vueFolder, "metadata_json");
            File.Copy(metadataSrc, metadataDest, true);

            string zipPath = Path.Combine(outputRoot, vueName + ".zip");
            if (File.Exists(zipPath)) File.Delete(zipPath);

            using (FileStream zipStream = new FileStream(zipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(displayPath, "display_json");
                archive.CreateEntryFromFile(metadataDest, "metadata_json");
            }

            string pdixPath = Path.Combine(outputRoot, vueName + ".pdix");
            if (File.Exists(pdixPath)) File.Delete(pdixPath);
            File.Move(zipPath, pdixPath);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("→ " + vueName + ".pdix généré !");
            Console.ResetColor();
        }*/
         static void SauvegarderVue(DisplayModel display, string vueName, string dossierVision, string outputRoot)
         {
             // 📁 Créer le sous-dossier temporaire pour les fichiers d'une vue
             string vueFolder = Path.Combine(outputRoot, vueName);
             Directory.CreateDirectory(vueFolder);

             // 📄 Génération du display_json avec sérialisation conditionnelle
             string displayPath = Path.Combine(vueFolder, "display_json");
             string typeGraphique = display.Symbols.Select(s => s.SymbolType).FirstOrDefault();

             if (typeGraphique == "trend")
             {
                 File.WriteAllText(displayPath, JsonUtils.SerializeWithIgnoredProperties(display, new List<string>
         {
             "FormatType","Intervals","ShowTitle","TitlePosition","TitleColor","ShowXAxisLabel","XAxisLabelType","XAxisCustomLabel",
             "ShowYAxisLabel","YAxisLabelType","YAxisCustomLabel","ShowEngineeringUnits","ShowGrid","GridColor","ShowLegend",
             "LegendPosition","LegendColor","ScaleColor","ScaleFormat","NumberFormat","NumberDecimals","NumberThousands",
             "ShowLine","XScaleMin","XScaleMax","YScaleMin","YScaleMax","DataSettings","zoomToggle","MultiScale"
         }));
             }
             else
             {
                 File.WriteAllText(displayPath, JsonUtils.SerializeWithIgnoredProperties(display, new List<string>()));
             }

             // 📎 Copier le fichier metadata_json
             string metadataSrc = Path.Combine(baseFolder, "metadata_json");
             string metadataDest = Path.Combine(vueFolder, "metadata_json");
             File.Copy(metadataSrc, metadataDest, true);

             // 🗜 Créer le .zip contenant les deux fichiers
             string zipPath = Path.Combine(outputRoot, vueName + ".zip");
             if (File.Exists(zipPath)) File.Delete(zipPath);

             using (FileStream zipStream = new FileStream(zipPath, FileMode.Create))
             using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
             {
                 archive.CreateEntryFromFile(displayPath, "display_json");
                 archive.CreateEntryFromFile(metadataDest, "metadata_json");
             }

             // 📁 Créer le dossierVision une seule fois s'il n'existe pas
             string dossierVisionPath = Path.Combine(outputRoot, dossierVision);
             if (!Directory.Exists(dossierVisionPath))
             {
                 Directory.CreateDirectory(dossierVisionPath);
             }

             // 📦 Renommer le .zip en .pdix et déplacer dans le dossierVision
             string pdixPath = Path.Combine(dossierVisionPath, vueName + ".pdix");
             if (File.Exists(pdixPath)) File.Delete(pdixPath);
             File.Move(zipPath, pdixPath);

             Console.ForegroundColor = ConsoleColor.Green;
             Console.WriteLine("→ " + vueName + ".pdix généré dans dossier : " + dossierVision);
             Console.ResetColor();
         }


        // Gestion du parametre "Left" : Position à gauche horizentalement du graphe en fonction du paramètre "Disposition"      
        static List<int> GetLeft(string disposition)
        {
            switch (disposition)
            {
                case "2C-5L": return new List<int> { 6, 1268, 6, 1268, 6, 1268, 6, 1268, 6, 1268 };
                case "2C-4L": return new List<int> { 6, 1268, 6, 1268, 6, 1268, 6, 1268 };
                case "2C-3L": return new List<int> { 6, 1268, 6, 1268, 6, 1268 };
                case "2C-2L": return new List<int> { 6, 1268, 6, 1268 };
                case "3C-1L": return new List<int> { 6, 850, 1694 };
                default: return null;
            }
        }
        // Gestion du parametre "Left" : Position à gauche horizentalement du graphe en fonction du paramètre "Disposition"  pour les graphes XY
        static List<int> GetLeftXY(string disposition)
        {
            switch (disposition)
            {
                case "2C-2L-3": return new List<int> { 0, 1138, 543 };
                default: return new List<int> { 0, 1138 };
            }
        }
        // Gestion du parametre "Top" : Position du Haut verticalement du graphe en fonction du paramètre "Disposition"
        static List<int> GetTop(string disposition)
        {
            switch (disposition)
            {
                case "1C-5L": return new List<int> { 29, 249, 469, 689, 909 };
                case "1C-4L": return new List<int> { 29, 299, 569, 839 };
                case "1C-3L": return new List<int> { 29, 389, 749 };
                case "1C-2L": return new List<int> { 29, 569 };
                case "1C-1L": return new List<int> { 100 };
                case "2C-5L": return new List<int> { 29, 29, 249, 249, 469, 469, 689, 689, 909, 909 };
                case "2C-4L": return new List<int> { 29, 29, 299, 299, 569, 569, 839, 839 };
                case "2C-3L": return new List<int> { 29, 29, 389, 389, 749, 749 };
                case "2C-2L": return new List<int> { 29, 29, 569, 569 };
                case "3C-1L": return new List<int> { 100, 100, 100 };
                default: return null;
            }
        }
        // Gestion du parametre "Top" : Position du Haut verticalement du graphe en fonction du paramètre "Disposition" pour les graphes XY
        static List<int> GetTopXY(string disposition)
        {
            switch (disposition)
            {
                case "2C-2L-3": return new List<int> { 53, 53,568 };                
                default: return new List<int> { 120, 120 };
            }
        }
        public class IgnorePropertiesResolver : DefaultContractResolver
        {
            private readonly HashSet<string> _propsToIgnore;

            public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
            {
                _propsToIgnore = new HashSet<string>(propNamesToIgnore);
            }

            protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);

                if (_propsToIgnore.Contains(prop.PropertyName))
                {
                    prop.ShouldSerialize = instance => false;
                }

                return prop;
            }
        }

        public static class JsonUtils
        {
            public static string SerializeWithIgnoredProperties(object obj, List<string> propertiesToIgnore)
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new IgnorePropertiesResolver(propertiesToIgnore),
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                return JsonConvert.SerializeObject(obj, settings);
            }
        }
    }
}
