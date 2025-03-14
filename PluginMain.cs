using BveEx.Extensions.MapStatements;
using BveEx.PluginHost.Plugins;
using BveEx.PluginHost.Plugins.Extensions;
using BveTypes.ClassWrappers;
using BveTypes.ClassWrappers.Extensions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnembeddedResources;
using static BveEx.Toukaitetudou.ExtendedTrainSchedulerWithOudia.Oud2.RosenClass.DiaClass.NoboriKudari;
using static BveEx.Toukaitetudou.ExtendedTrainSchedulerWithOudia.Oud2.RosenClass.DiaClass.NoboriKudari.RessyaClass;
namespace BveEx.Toukaitetudou.ExtendedTrainSchedulerWithOudia
{
    [Plugin(PluginType.MapPlugin, "2.0.50314.1")]
    public class PluginMain : AssemblyPluginBase,IExtension
    {
        private class ResourceSet
        {
            private readonly ResourceLocalizer Localizer = ResourceLocalizer.FromResX(Path.Combine(BaseDirectory, $"Resources\\{nameof(ExtendedTrainSchedulerWithOudia)}.resx"));

            [ResourceStringHolder(nameof(Localizer))] public Resource<string> XmlSchemaValidation { get; private set; }

            [ResourceStringHolder(nameof(Localizer))] public Resource<string> OperationNumberDuplication { get; private set; }
            [ResourceStringHolder(nameof(Localizer))] public Resource<string> OudFileFormat {  get; private set; }
            [ResourceStringHolder(nameof(Localizer))] public Resource<string> OperationNumberNotDefined {  get; private set; }
            [ResourceStringHolder(nameof(Localizer))] public Resource<string> OudFileIsNotFound { get; private set; }
            public ResourceSet()
            {
                ResourceLoader.LoadAndSetAll(this);
            }
        }
        static string BaseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static string xmlPath = Path.Combine(BaseDirectory, $"{nameof(BveEx.Toukaitetudou.ExtendedTrainSchedulerWithOudia)}.Config.xml");
        private static readonly Lazy<ResourceSet> Resources = new Lazy<ResourceSet>();
        IStatementSet statements;
        Oud2 oud2;
            ExtendedTrainSchedulerWithOudiaConfig ExtendedTrainScedulerWithOudiaConfig=null;
        Dictionary<string, Oud2.RosenClass.DiaClass> Dias;
        public PluginMain(PluginBuilder builder) : base(builder)
        {
            {
                XmlSchemaSet SchemaSet = new XmlSchemaSet();
                using (Stream schemaStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(ExtendedTrainSchedulerWithOudia)}.{nameof(ExtendedTrainSchedulerWithOudia)}.Config.xsd"))
                {
                    var rt = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                    XmlSchema schema = XmlSchema.Read(schemaStream, SchemaValidation);
                    SchemaSet.Add(schema);
                }
                XDocument doc = XDocument.Load(xmlPath, LoadOptions.SetLineInfo);

                doc.Validate(SchemaSet, DocumentValidation);

            }
            using (StreamReader sr = new StreamReader(xmlPath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(ExtendedTrainSchedulerWithOudiaConfig));
                try
                {
                    ExtendedTrainScedulerWithOudiaConfig = (ExtendedTrainSchedulerWithOudiaConfig)xs.Deserialize(sr);
                }
                catch (Exception e)
                {
                    BveHacker.LoadingProgressForm.ThrowError(e.Message, Name, 0, 0);
                    return;
                }
            }
            string vopnum = ExtendedTrainScedulerWithOudiaConfig.Config.VehicleOperationNumber;
            foreach (Operation operation in ExtendedTrainScedulerWithOudiaConfig.Operation)
            {
                if (operation.OperationNumber==vopnum)
                {
                    BveHacker.LoadingProgressForm.ThrowError(Resources.Value.OperationNumberDuplication.Value, Name, 0, 0);
                }
            }
            Loader Loader = new Loader(Path.Combine(BaseDirectory, ExtendedTrainScedulerWithOudiaConfig.Config.Oud2FilePath));
            oud2 = new Oud2(Loader);
            if (oud2.FileType is null) 
            {
                BveHacker.LoadingProgressForm.ThrowError(Resources.Value.OudFileIsNotFound.Value, Name, 0, 0);
                oud2=null;
                return;
            }
            if (!permissionOudiaVersion.Contains(oud2.FileType))
            {
                BveHacker.LoadingProgressForm.ThrowError(Resources.Value.OudFileFormat.Value, Name, 0, 0);
                oud2=null;
                return;
            }
            Dias= oud2.Rosen.Dia.ToDictionary(x => x.DiaName, x => x);
            statements=Extensions.GetExtension<IStatementSet>();
            statements.LoadingCompleted+=Statements_LoadingCompleted;
        }
        private static readonly List<string> permissionOudiaVersion = new List<string>() {
            "OuDiaSecond.1.10",
            "OuDiaSecond.1.11",
            "OuDiaSecond.1.12",
            "OuDiaSecond.1.13",
            "OuDiaSecond.1.14",
            "OuDiaSecond.1.15",
        };

        private static void SchemaValidation(object sender, ValidationEventArgs e) => throw  new FormatException(Resources.Value.XmlSchemaValidation.Value, e.Exception);

        private static void DocumentValidation(object sender, ValidationEventArgs e) => throw e.Exception;


        string outdirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "output", nameof(Toukaitetudou), nameof(ExtendedTrainSchedulerWithOudia));

        class TrainData
        {
            public RessyaClass ressya;
            public Bound bound;
            public string operationNumber;
            public double minLocation=double.MaxValue;
            public double maxLocation=double.MinValue;
            public int cars;
        }
        class thisStatement
        {
            public enum StatementType
            {
                Stop,
                Pass,
                Accelerate_FromHere, 
                Accelerate_ToHere
            }
            public StatementType Type;
            public Statement Statement;
            public MapStatementCreateer Createer =new MapStatementCreateer();
            public bool IsEnableLocation(TrainData trainData)
            {
                return trainData.minLocation<=Statement.Source.Location&&Statement.Source.Location<=trainData.maxLocation;
            }
        }
        bool IsEnableStop(Statement statement, TrainData trainData)
        {
            if(trainData.operationNumber==ExtendedTrainScedulerWithOudiaConfig.Config.VehicleOperationNumber)return false;
            if (!statement.IsUserStatement(nameof(Toukaitetudou), ClauseFilter.Element(nameof(BveEx.Toukaitetudou.ExtendedTrainSchedulerWithOudia), 0), ClauseFilter.Element("Track", 1), ClauseFilter.Function("Stop",2))) return false;
            if (statement.Source.Clauses[5].Args.Count<2)return false;
            WrappedList<MapStatementClause> clauses = statement.Source.Clauses;
            if (clauses[4].Keys[0] as string==trainData.bound.ToString())
            {
                if (
                !clauses.Where(x => x.Name=="Series").Select(x => x.Args.Contains(operations[trainData.operationNumber].Series)).Append(true).Contains(false)&&
                !clauses.Where(x => x.Name=="IgnoreSeries").Select(x => !x.Args.Contains(operations[trainData.operationNumber].Series)).Append(true).Contains(false)
                )
                {
                    return true;
                }
            }
            return false;
        }
        Statement GetEnableStop( TrainData trainData,string stationName,params Statement[] statements)
        {
            IEnumerable<Statement> buf= statements.Where(x => IsEnableStop(x, trainData)).Where(x => x.Source.Clauses[5].Args[0] as string==stationName).OrderBy(x => (int)x.Source.Clauses[5].Args[1]).Where(x => (int)x.Source.Clauses[5].Args[1]>=trainData.cars);
           return buf.Where(x => x.Source.Clauses[5].Args[1]==buf.FirstOrDefault().Source.Clauses[5].Args[1]).OrderBy(x=>x.Source.Location).FirstOrDefault();
        }
        Dictionary<string, Operation> operations;

        private void Statements_LoadingCompleted(object sender, EventArgs e)
        {
            if(oud2 is null)return;

            Dictionary<string, int> stations = new Dictionary<string, int>();
            for (int i = 0; i<oud2.Rosen.Eki.Count; ++i)
            {
                stations[oud2.Rosen.Eki[i].Ekimei]=i;
            }
            List<thisStatement> thisStatements = new List<thisStatement>();
            List<KeyValuePair<Statement, MapStatementCreateer>> mapstatements = statements.ToDictionary(x => x, x => new MapStatementCreateer()).OrderBy(x => x.Key.Source.Location).ToList();
            MapStatementCreateer mapStatementCreator = new MapStatementCreateer();
            operations= ExtendedTrainScedulerWithOudiaConfig.Operation.ToDictionary(x => x.OperationNumber, x => x);
            List<TrainData> trains = Dias[ExtendedTrainScedulerWithOudiaConfig.Config.TimeTableName].Kudari.Ressya.AsEnumerable().Select(x => new TrainData() { ressya= x, bound=Bound.OutBound, operationNumber=x.OperationB.OperationNumber[0], cars=int.Parse(operations.GetValueSafe(x.OperationB.OperationNumber[0])?.Cars ??"0") }).ToList();
            trains.AddRange(Dias[ExtendedTrainScedulerWithOudiaConfig.Config.TimeTableName].Nobori.Ressya.AsEnumerable().Select(x => new TrainData() { ressya= x, bound=Bound.InBound, operationNumber=x.OperationB.OperationNumber[0], cars=int.Parse(operations.GetValueSafe(x.OperationB.OperationNumber[0])?.Cars ??"0") }));
            foreach (TrainData trainData in trains)
            {
                if (trainData.operationNumber==ExtendedTrainScedulerWithOudiaConfig.Config.VehicleOperationNumber) continue;
                if (!IsOperationNumberDefined(trainData.operationNumber)) continue;

                RessyaClass ressya = trainData.ressya;

                if (!BveHacker.MapLoader.Map.TrainInfos.TryGetValue(ressya.Ressyabangou.ToTrainKey().ToLower(), out _)&&!(trainData.operationNumber is null))
                {
                    WrappedList<MapStatementClause> newMapStatementClauses = new WrappedList<MapStatementClause>();

                    MapStatementClause statement = new MapStatementClause("Train", 0, 0);
                    statement.Keys.Add($"{ressya.Ressyabangou.ToTrainKey()}");
                    newMapStatementClauses.Add(statement);
                    statement=new MapStatementClause("Load", 0, 0);
                    statement.Args.Add($"{Path.GetFullPath(Path.Combine(BaseDirectory, trainData.bound==Bound.InBound ? operations[trainData.operationNumber].InBoundTrainFilePath : operations[trainData.operationNumber].OutBoundTrainFilepath??operations[trainData.operationNumber].InBoundTrainFilePath))}");
                    statement.Args.Add($"1");
                    statement.Args.Add(trainData.bound==ExtendedTrainScedulerWithOudiaConfig.Config.VehicleDirection ? 1 : -1);
                    newMapStatementClauses.Add(statement);
                    if (ExtendedTrainScedulerWithOudiaConfig.Config.RealTimeUpdate)
                    {
                        BveHacker.MapLoader.Statements.Add(new MapStatement(0, newMapStatementClauses, BaseDirectory));
                        BveHacker.MapLoader.ParseStatement(newMapStatementClauses);
                    }
                    newMapStatementClauses[1].Args[0]=new Uri(outdirectory).MakeRelativeUri(new Uri(newMapStatementClauses[1].Args[0]as string)).ToString();
                    mapStatementCreator.Add(newMapStatementClauses);

                    newMapStatementClauses = new WrappedList<MapStatementClause>();
                    statement = new MapStatementClause("Train", 0, 0);
                    statement.Keys.Add($"{ressya.Ressyabangou.ToTrainKey()}");
                    newMapStatementClauses.Add(statement);
                    statement=new MapStatementClause("Stop", 0, 0);
                    statement.Args.Add(0);
                    statement.Args.Add(0);
                    statement.Args.Add(0);
                    statement.Args.Add(0);
                    newMapStatementClauses.Add(statement);
                    BveHacker.MapLoader.Statements.Add(new MapStatement(0, newMapStatementClauses, BveHacker.MapLoader.FilePath));
                    BveHacker.MapLoader.ParseStatement(newMapStatementClauses);
                    mapStatementCreator.Add(newMapStatementClauses);
                }
            }
            if (ExtendedTrainScedulerWithOudiaConfig.Config.UpdateOutputFile)
            {
                mapStatementCreator.Create(Path.Combine(outdirectory, $"extendedTrainLoad.map"));
            }
            Dictionary<string, List<EkijikokuClass>> timetables = new Dictionary<string, List<EkijikokuClass>>();
            foreach (TrainData trainData in trains)
            {
                string trainNumber = trainData.ressya.Ressyabangou.ToTrainKey();
                if (!timetables.ContainsKey(trainNumber))
                {
                    timetables[trainNumber]=new List<EkijikokuClass>(trainData.ressya.EkiJikoku);
                }
                else
                {
                    for (int i = 0; i<trainData.ressya.EkiJikoku.Count||i<timetables[trainNumber].Count; ++i)
                    {
                        if (!(i<timetables[trainNumber].Count))
                        {
                            timetables[trainNumber].Add(new EkijikokuClass(string.Empty));
                        }
                        if (timetables[trainNumber][i].Ekiatsukai==0)
                        {
                            timetables[trainNumber][i]= trainData.ressya.EkiJikoku[i];
                        }
                        else
                        {
                            if (timetables[trainNumber][i].arrivalTime is null)
                            {
                                timetables[trainNumber][i].arrivalTime=trainData.ressya.EkiJikoku[i].arrivalTime;
                            }
                            if (timetables[trainNumber][i].departureTime is null)
                            {
                                timetables[trainNumber][i].departureTime=trainData.ressya.EkiJikoku[i].departureTime;
                            }
                        }
                    }
                }
            }


            thisStatements.AddRange(statements.FindUserStatements(nameof(Toukaitetudou), ClauseFilter.Element(nameof(BveEx.Toukaitetudou.ExtendedTrainSchedulerWithOudia), 0), ClauseFilter.Element("Track", 1), ClauseFilter.Element("Accelerate", 0), ClauseFilter.Function("ToHere", 2)).Select(x => new thisStatement() { Type=thisStatement.StatementType.Accelerate_ToHere, Statement=x }));
            thisStatements.AddRange(statements.FindUserStatements(nameof(Toukaitetudou), ClauseFilter.Element(nameof(BveEx.Toukaitetudou.ExtendedTrainSchedulerWithOudia), 0), ClauseFilter.Element("Track", 1), ClauseFilter.Element("Accelerate", 0), ClauseFilter.Function("FromHere", 2)).Select(x => new thisStatement() { Type=thisStatement.StatementType.Accelerate_FromHere, Statement=x }));
            thisStatements.AddRange(statements.FindUserStatements(nameof(Toukaitetudou), ClauseFilter.Element(nameof(BveEx.Toukaitetudou.ExtendedTrainSchedulerWithOudia), 0), ClauseFilter.Element("Track", 1), ClauseFilter.Function("Stop",2)).Select(x => new thisStatement() { Type=thisStatement.StatementType.Stop, Statement=x }));

            foreach (TrainData trainData in trains)
            {
                RessyaClass ressya = trainData.ressya;

                string trainNumber = ressya.Ressyabangou.ToTrainKey();
                if (operations.ContainsKey(trainNumber))
                {
                    foreach (Statement statement in thisStatements.Where(x => x.Type==thisStatement.StatementType.Stop).Select(x => x.Statement))
                    {
                        if (IsEnableStop(statement, trainData)&&(timetables[trainNumber].ElementAtOrDefault(stations[statement.Source.Clauses[5].Args[0]as string])?.Ekiatsukai??0)!=0)
                        {
                            if (statement.Source.Location<trainData.minLocation) trainData.minLocation=statement.Source.Location;
                            if (trainData.maxLocation<statement.Source.Location) trainData.maxLocation=statement.Source.Location;
                        }
                    }
                }
            }
            List<string> loadedTrains = new List<string>();
            foreach (TrainData trainData in trains)
            {
                if (trainData.operationNumber==ExtendedTrainScedulerWithOudiaConfig.Config.VehicleOperationNumber) continue;
                RessyaClass ressya = trainData.ressya;

                string trainNumber = ressya.Ressyabangou.ToTrainKey();

                if (loadedTrains.Contains(trainNumber)) continue;
                loadedTrains.Add(trainNumber);

                for (int i = 0; i<thisStatements.Count; ++i)
                {
                    Statement statement = thisStatements[i].Statement;
                    mapStatementCreator = thisStatements[i].Createer;
                    if (thisStatements[i].IsEnableLocation(trainData))
                    {
                        WrappedList<MapStatementClause> clauses = statement.Source.Clauses;
                        switch (thisStatements[i].Type)
                        {
                            case thisStatement.StatementType.Accelerate_ToHere:
                                {
                                    if (clauses[4].Keys[0] as string==trainData.bound.ToString()&&
                                    !clauses.Where(x => x.Name=="Pass").Select(x => x.Args).OfType<string>().Select(x => timetables[trainData.ressya.Ressyabangou.ToTrainKey()][stations[x]].Ekiatsukai==2).Append(true).Contains(false)&&
                                    !clauses.Where(x => x.Name=="IgnorePass").Select(x => x.Args).OfType<string>().Select(x => timetables[trainData.ressya.Ressyabangou.ToTrainKey()][stations[x]].Ekiatsukai!=2).Append(true).Contains(false)&&
                                    !clauses.Where(x => x.Name=="Stop").Select(x => x.Args).OfType<string>().Select(x => timetables[trainData.ressya.Ressyabangou.ToTrainKey()][stations[x]].Ekiatsukai==1).Append(true).Contains(false)&&
                                    !clauses.Where(x => x.Name=="IgnoreStop").Select(x => x.Args).OfType<string>().Select(x => timetables[trainData.ressya.Ressyabangou.ToTrainKey()][stations[x]].Ekiatsukai!=1).Append(true).Contains(false)&&
                                    !clauses.Where(x => x.Name=="Series").Select(x => x.Args.Contains(operations[trainData.operationNumber].Series)).Append(true).Contains(false)&&
                                    !clauses.Where(x => x.Name=="IgnoreSeries").Select(x => !x.Args.Contains(operations[trainData.operationNumber].Series)).Append(true).Contains(false)
                                    )
                                    {
                                        WrappedList<MapStatementClause> newMapStatementClauses = new WrappedList<MapStatementClause>();
                                        newMapStatementClauses.Add(new MapStatementClause("BveEx", clauses[0].LineIndex, clauses[0].CharIndex));
                                        newMapStatementClauses.Add(new MapStatementClause("User", clauses[1].LineIndex, clauses[1].CharIndex));
                                        newMapStatementClauses.Add(new MapStatementClause("Automatic9045", clauses[2].LineIndex, clauses[2].CharIndex));
                                        newMapStatementClauses.Add(new MapStatementClause("ExtendedTrainScheduler", clauses[3].LineIndex, clauses[3].CharIndex));
                                        MapStatementClause state = new MapStatementClause("Train", clauses[4].LineIndex, clauses[4].CharIndex);
                                        state.Keys.Add(trainNumber);
                                        newMapStatementClauses.Add(state);
                                        newMapStatementClauses.Add(new MapStatementClause("Accelerate", clauses[5].LineIndex, clauses[5].CharIndex));
                                        state = new MapStatementClause("ToHere", clauses[6].LineIndex, clauses[6].CharIndex);
                                        state.Args.Add(clauses[6].Args[0]);
                                        state.Args.Add(clauses[6].Args[1]);
                                        newMapStatementClauses.Add(state);
                                        if (ExtendedTrainScedulerWithOudiaConfig.Config.RealTimeUpdate)
                                        {
                                            BveHacker.MapLoader.Statements.Add(new MapStatement(statement.Source.Location, newMapStatementClauses, statement.Source.FileName));
                                            BveHacker.MapLoader.ParseStatement(newMapStatementClauses);
                                        }
                                        mapStatementCreator.Add(newMapStatementClauses);
                                    }
                                    break;
                                }
                            case thisStatement.StatementType.Accelerate_FromHere:
                                {
                                    bool a = (!clauses.Where(x => x.Name=="Series").Select(x => x.Args.Contains(operations[trainData.operationNumber].Series)).Append(true).Contains(false));
                                    bool b = (!clauses.Where(x => x.Name=="Ignore").Select(x => !x.Args.Contains("Series")).Append(true).Contains(false));
                                    if (clauses[4].Keys[0] as string==trainData.bound.ToString()&&
                                    !clauses.Where(x => x.Name=="Pass").Select(x => x.Args).OfType<string>().Select(x => timetables[trainData.ressya.Ressyabangou.ToTrainKey()][stations[x]].Ekiatsukai==2).Append(true).Contains(false)&&
                                    !clauses.Where(x => x.Name=="IgnorePass").Select(x => x.Args).OfType<string>().Select(x => timetables[trainData.ressya.Ressyabangou.ToTrainKey()][stations[x]].Ekiatsukai!=2).Append(true).Contains(false)&&
                                    !clauses.Where(x => x.Name=="Stop").Select(x => x.Args).OfType<string>().Select(x => timetables[trainData.ressya.Ressyabangou.ToTrainKey()][stations[x]].Ekiatsukai==1).Append(true).Contains(false)&&
                                    !clauses.Where(x => x.Name=="IgnoreStop").Select(x => x.Args).OfType<string>().Select(x => timetables[trainData.ressya.Ressyabangou.ToTrainKey()][stations[x]].Ekiatsukai!=1).Append(true).Contains(false)&&
                                    !clauses.Where(x => x.Name=="Series").Select(x => x.Args.Contains(operations[trainData.operationNumber].Series)).Append(true).Contains(false)&&
                                    !clauses.Where(x => x.Name=="IgnoreSeries").Select(x => !x.Args.Contains(operations[trainData.operationNumber].Series)).Append(true).Contains(false)

                                    )
                                    {
                                        WrappedList<MapStatementClause> newMapStatementClauses = new WrappedList<MapStatementClause>();

                                        newMapStatementClauses.Add(new MapStatementClause("BveEx", clauses[0].LineIndex, clauses[0].CharIndex));
                                        newMapStatementClauses.Add(new MapStatementClause("User", clauses[1].LineIndex, clauses[1].CharIndex));
                                        newMapStatementClauses.Add(new MapStatementClause("Automatic9045", clauses[2].LineIndex, clauses[2].CharIndex));
                                        newMapStatementClauses.Add(new MapStatementClause("ExtendedTrainScheduler", clauses[3].LineIndex, clauses[3].CharIndex));
                                        MapStatementClause state = new MapStatementClause("Train", clauses[4].LineIndex, clauses[4].CharIndex);
                                        state.Keys.Add(trainNumber);
                                        newMapStatementClauses.Add(state);
                                        newMapStatementClauses.Add(new MapStatementClause("Accelerate", clauses[5].LineIndex, clauses[5].CharIndex));
                                        state = new MapStatementClause("FromHere", clauses[6].LineIndex, clauses[6].CharIndex);
                                        state.Args.Add(clauses[6].Args[0]);
                                        state.Args.Add(clauses[6].Args[1]);
                                        newMapStatementClauses.Add(state);
                                        if (ExtendedTrainScedulerWithOudiaConfig.Config.RealTimeUpdate)
                                        {

                                            BveHacker.MapLoader.Statements.Add(new MapStatement(statement.Source.Location, newMapStatementClauses, statement.Source.FileName));
                                            BveHacker.MapLoader.ParseStatement(newMapStatementClauses);
                                        }
                                        mapStatementCreator.Add(newMapStatementClauses);
                                    }
                                    break;
                                }
                            case thisStatement.StatementType.Pass:
                                {
                                    string stationname = clauses[5].Args[0]as string;

                                    if (timetables[trainData.ressya.Ressyabangou.ToTrainKey()][stations[stationname]].Ekiatsukai==2)
                                    {

                                    }
                                    break;
                                }
                        }
                    }

                }

                for (int i = 0; i<timetables[trainData.ressya.Ressyabangou.ToTrainKey()].Count; i++)
                {
                    if (timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].Ekiatsukai==1)
                    {
                        int ind = (trainData.bound==Bound.OutBound ? i : oud2.Rosen.Eki.Count-i-1)%oud2.Rosen.Eki.Count;
                        string stationName = oud2.Rosen.Eki[(trainData.bound==Bound.OutBound ? i : oud2.Rosen.Eki.Count-i-1)%oud2.Rosen.Eki.Count].Ekimei;
                        if(!IsOperationNumberDefined(trainData.operationNumber))continue;
                        Statement stop = GetEnableStop(trainData, stationName, thisStatements.Select(x => x.Statement).ToArray());
                        if (stop is null) continue;
                        _=ind;
                        var rt = BveHacker.MapLoader.Map.TrainInfos[trainData.ressya.Ressyabangou.ToTrainKey().ToLower()].GoToAndGetCurrent(BveHacker.MapLoader.CurrentLocation);
                        WrappedList<MapStatementClause> clauses = stop.Source.Clauses;
                        WrappedList<MapStatementClause> newMapStatementClauses = new WrappedList<MapStatementClause>();
                        mapStatementCreator=thisStatements.ToDictionary(x => x.Statement, x => x.Createer)[stop];

                        newMapStatementClauses.Add(new MapStatementClause("BveEx", clauses[0].LineIndex, clauses[0].CharIndex));
                        newMapStatementClauses.Add(new MapStatementClause("User", clauses[1].LineIndex, clauses[1].CharIndex));
                        newMapStatementClauses.Add(new MapStatementClause("Automatic9045", clauses[2].LineIndex, clauses[2].CharIndex));
                        newMapStatementClauses.Add(new MapStatementClause("ExtendedTrainScheduler", clauses[3].LineIndex, clauses[3].CharIndex));
                        MapStatementClause state = new MapStatementClause("Train", clauses[4].LineIndex, clauses[4].CharIndex);
                        state.Keys.Add(trainNumber);
                        newMapStatementClauses.Add(state);
                        if (!(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].departureTime is null))
                        {
                            if (!(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].arrivalTime is null))
                            {
                                newMapStatementClauses[2].Name ="Toukaitetudou";
                                state = new MapStatementClause("StopAtUntil", clauses[5].LineIndex, clauses[5].CharIndex);
                                state.Args.Add(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].arrivalTime?.time.ToString());
                                state.Args.Add(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].departureTime?.time.ToString());
                                state.Args.Add(7.2);
                                state.Args.Add(72);
                            }
                            else
                            {
                                state = new MapStatementClause("StopUntil", clauses[5].LineIndex, clauses[5].CharIndex);
                                state.Args.Add(0);
                                state.Args.Add(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].departureTime?.time.ToString());
                                state.Args.Add(7.2);
                                state.Args.Add(72);
                            }
                        }
                        else
                        {
                            if (!(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].arrivalTime is null))
                            {
                                newMapStatementClauses[2].Name ="Toukaitetudou";
                                state = new MapStatementClause("StopAt", clauses[5].LineIndex, clauses[5].CharIndex);
                                state.Args.Add(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].arrivalTime?.time.ToString());
                                state.Args.Add(0);
                                state.Args.Add(0);
                                state.Args.Add(0);
                            }
                            else
                            {
                                newMapStatementClauses=new WrappedList<MapStatementClause>() { newMapStatementClauses[4] };
                                state = new MapStatementClause("Stop", clauses[5].LineIndex, clauses[5].CharIndex);
                                state.Args.Add(7.2);
                                state.Args.Add(60);
                                state.Args.Add(7.2);
                                state.Args.Add(72);

                            }
                        }
                        newMapStatementClauses.Add(state);
                        if (ExtendedTrainScedulerWithOudiaConfig.Config.RealTimeUpdate)
                        {
                            BveHacker.MapLoader.Statements.Add(new MapStatement(stop.Source.Location, newMapStatementClauses, stop.Source.FileName));
                            BveHacker.MapLoader.ParseStatement(newMapStatementClauses);
                        }
                        mapStatementCreator.Add(newMapStatementClauses);
                    }
                    if (timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].Ekiatsukai==2)
                    {
                        if (!operations.ContainsKey(trainNumber)) continue;
                        
                        int ind = (trainData.bound==Bound.OutBound ? i : oud2.Rosen.Eki.Count-i-1)%oud2.Rosen.Eki.Count;
                        string stationName = oud2.Rosen.Eki[(trainData.bound==Bound.OutBound ? i : oud2.Rosen.Eki.Count-i-1)%oud2.Rosen.Eki.Count].Ekimei;
                        Statement stop = GetEnableStop(trainData, stationName, thisStatements.Select(x => x.Statement).ToArray());
                        if (stop is null) continue;
                        _=ind;
                        var rt = BveHacker.MapLoader.Map.TrainInfos[trainData.ressya.Ressyabangou.ToTrainKey().ToLower()].GoToAndGetCurrent(BveHacker.MapLoader.CurrentLocation);
                        WrappedList<MapStatementClause> clauses = stop.Source.Clauses;
                        WrappedList<MapStatementClause> newMapStatementClauses = new WrappedList<MapStatementClause>();
                        mapStatementCreator=thisStatements.ToDictionary(x => x.Statement, x => x.Createer)[stop];

                        newMapStatementClauses.Add(new MapStatementClause("BveEx", clauses[0].LineIndex, clauses[0].CharIndex));
                        newMapStatementClauses.Add(new MapStatementClause("User", clauses[1].LineIndex, clauses[1].CharIndex));
                        newMapStatementClauses.Add(new MapStatementClause("Automatic9045", clauses[2].LineIndex, clauses[2].CharIndex));
                        newMapStatementClauses.Add(new MapStatementClause("ExtendedTrainScheduler", clauses[3].LineIndex, clauses[3].CharIndex));
                        MapStatementClause state = new MapStatementClause("Train", clauses[4].LineIndex, clauses[4].CharIndex);
                        state.Keys.Add(trainNumber);
                        newMapStatementClauses.Add(state);
                        if (!(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].departureTime is null))
                        {
                            if (!(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].arrivalTime is null))
                            {
                                newMapStatementClauses[2].Name ="Toukaitetudou";
                                state = new MapStatementClause("StopAtUntil", clauses[5].LineIndex, clauses[5].CharIndex);
                                state.Args.Add(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].arrivalTime?.time.ToString());
                                state.Args.Add(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].departureTime?.time.ToString());
                                state.Args.Add(7.2);
                                state.Args.Add(72);
                            }
                            else
                            {
                                state = new MapStatementClause("StopUntil", clauses[5].LineIndex, clauses[5].CharIndex);
                                state.Args.Add(0);
                                state.Args.Add(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].departureTime?.time.ToString());
                                state.Args.Add(7.2);
                                state.Args.Add(72);
                            }
                        }
                        else
                        {
                            if (!(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].arrivalTime is null))
                            {
                                newMapStatementClauses[2].Name ="Toukaitetudou";
                                state = new MapStatementClause("StopAt", clauses[5].LineIndex, clauses[5].CharIndex);
                                state.Args.Add(timetables[trainData.ressya.Ressyabangou.ToTrainKey()][i].arrivalTime?.time.ToString());
                                state.Args.Add(0);
                                state.Args.Add(0);
                                state.Args.Add(0);
                            }
                            else
                            {
                                newMapStatementClauses=new WrappedList<MapStatementClause>() { newMapStatementClauses[4] };
                                state = new MapStatementClause("Stop", clauses[5].LineIndex, clauses[5].CharIndex);
                                state.Args.Add(7.2);
                                state.Args.Add(60);
                                state.Args.Add(7.2);
                                state.Args.Add(72);

                            }
                        }
                        newMapStatementClauses.Add(state);
                        if (ExtendedTrainScedulerWithOudiaConfig.Config.RealTimeUpdate)
                        {
                            BveHacker.MapLoader.Statements.Add(new MapStatement(stop.Source.Location, newMapStatementClauses, stop.Source.FileName));
                            BveHacker.MapLoader.ParseStatement(newMapStatementClauses);
                        }
                        mapStatementCreator.Add(newMapStatementClauses);
                    }
                }
            }
            if (ExtendedTrainScedulerWithOudiaConfig.Config.UpdateOutputFile)
            {
                thisStatements.Foreach(x => x.Createer.Create(Path.Combine(outdirectory, $"{Path.GetFileName(x.Statement.Source.FileName)}_{x.Statement.Source.Clauses[0].LineIndex}_{x.Statement.Source.Clauses[0].CharIndex}.map")));
            }
        }
        private bool IsOperationNumberDefined(string operationNumber)
        {
            if (!operations.ContainsKey(operationNumber))
            {
                BveHacker.LoadingProgressForm.ThrowError(Resources.Value.OperationNumberNotDefined.Value, Name, 0, 0);
                return false;
            }
            return true;
        }
        public override void Dispose()
        {
            if (statements != null) statements.LoadingCompleted-=Statements_LoadingCompleted;
        }
        public override void Tick(TimeSpan elapsed)
        {}
    }
}
