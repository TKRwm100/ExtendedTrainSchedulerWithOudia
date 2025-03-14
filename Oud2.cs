using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BveEx.Toukaitetudou.ExtendedTrainSchedulerWithOudia
{
    public class Loader
    {
        public Dictionary<string, List<string>> element;
        public Dictionary<string, List<Loader>> childs;
        public Loader()
        {
            element = new Dictionary<string, List<string>>();
            childs = new Dictionary<string, List<Loader>>();
        }
        public Loader(string filepath) : this()
        {
            if(!File.Exists(filepath))return;

            List<string> file = File.ReadAllLines(filepath, Encoding.GetEncoding("UTF-8")).ToList();

            int index = 0;
            Loader buf = new Loader(file, ref index);
            this.childs = buf.childs;
            this.element = buf.element;
            return;
        }
        Loader(List<string> file, ref int index) : this()
        {
            for (; index<file.Count;)
            {
                if (file[index][file[index].Length-1]!='.')
                {
                    int eqindex = file[index].IndexOf('=');
                    if (eqindex>=0)
                    {
                        if (element.ContainsKey(file[index].Substring(0, eqindex)))
                        {
                            element[file[index].Substring(0, eqindex)].AddRange(file[index].Substring(eqindex+1).Split(','));
                        }
                        else
                        {
                            element.Add(file[index].Substring(0, eqindex), new List<string>(file[index].Substring(eqindex+1).Split(',')));
                        }
                    }
                    ++index;
                }
                else if (file[index][0]!='.')
                {
                    string name = file[index].Split('.')[0];
                    ++index;
                    Loader buf = new Loader(file, ref index);
                    if (!childs.ContainsKey(name))
                    {
                        childs.Add(name, new List<Loader>());
                    }
                    this.childs[name].Add(buf);
                }
                else
                {
                    ++index;
                    return;
                }
            }
            return;
        }
    }
    public class Oud2
    {
        public class Font
        {
            public int PointTextHeight;
            public string Facename;
            public bool Bold;
            public bool Itaric;
            public Font(string src)
            {
                string[] buf = src.Split(';');
                Dictionary<string, string> buf2 = new Dictionary<string, string>();
                foreach (string elm in buf)
                {

                    int eqindex = elm.IndexOf('=');
                    if (eqindex>=0)
                    {
                        string[] buf3 = elm.Split('=');
                        buf2.Add(buf3[0], buf3[1]);
                    }
                }
                PointTextHeight=int.Parse(buf2[nameof(PointTextHeight)]);
                Facename=buf2[nameof(Facename)];
                if (buf2.ContainsKey(nameof(Bold)))
                    Bold=int.Parse(buf2[nameof(Bold)])!=0;
                if (buf2.ContainsKey(nameof(Itaric)))
                    Itaric=int.Parse(buf2[nameof(Itaric)])!=0;
            }
        }
        public class Color
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;
            public Color(UInt32 src)
            {
                R=(byte)(src%0x100);
                G=(byte)((src/0x100)%0x100);
                B=(byte)(((src/0x100)/0x100)%0x100);
                A=0;
            }
            static public implicit operator int(Color src)
            {
                return src.A*0x1000000+src.B*0x10000+src.G*0x100+src.R;
            }
        }
        public class Time
        {
            public TimeSpan time;
            public int toMilliSecconds()
            {
                return (time.Hours*3600+time.Minutes*60+time.Seconds)*1000;
            }

            public Time(int src)
            {
                if (src%10000==src)
                {
                    time=new TimeSpan(src/100, src%100, 0);
                }
                else
                {
                    time=new TimeSpan(src/10000, src%10000/100, src%100);
                }
            }
            static public implicit operator int(Time src)
            {
                if (src.time.Seconds!=0)
                {
                    return src.time.Hours*10000+src.time.Minutes*100+src.time.Seconds;
                }
                else
                {
                    return src.time.Hours*100+src.time.Minutes;
                }
            }
        }
        public class RosenClass
        {
            public class EkiClass
            {
                public class EkiTrack2ContClass
                {
                    public class EkiTrack2Class
                    {
                        public string TrackName;
                        public string TrackRyakusyou;

                        public EkiTrack2Class(Loader loader)
                        {
                            TrackName=loader.element[nameof(TrackName)][0];
                            TrackRyakusyou=loader.element[nameof(TrackRyakusyou)][0];
                        }
                    }
                    public List<EkiTrack2Class> EkiTrack2;

                    public EkiTrack2ContClass(Loader loader)
                    {
                        EkiTrack2 = new List<EkiTrack2Class>();
                        foreach (Loader loader1 in loader.childs[nameof(EkiTrack2)])
                        {
                            EkiTrack2.Add(new EkiTrack2Class(loader1));
                        }
                    }
                }
                public string Ekimei;
                public string Ekijikokukeisiki;
                public string Ekikibo;
                public int DownMain;
                public int UpMain;
                public int JikokuhyouTrackDisplayKudari;
                public int JikokuhyouTrackDisplayNobori;
                public int DiagramTrackDisplay;
                public EkiTrack2ContClass EkiTrack2Cont;
                public List<int> JikokuhyouJikokuDisplayKudari;
                public List<int> JikokuhyouJikokuDisplayNobori;
                public List<int> JikokuhyouSyubetsuChangeDisplayKudari;
                public List<int> JikokuhyouSyubetsuChangeDisplayNobori;
                public int DiagramColorNextEki;
                public List<int> JikokuhyouOuterDisplayKudari;
                public List<int> JikokuhyouOuterDisplayNobori;

                public EkiClass(Loader loader)
                {
                    Ekimei=loader.element[nameof(Ekimei)][0];
                    Ekijikokukeisiki=loader.element[nameof(Ekijikokukeisiki)][0];
                    Ekikibo=loader.element[nameof(Ekikibo)][0];
                    DownMain=int.Parse(loader.element[nameof(DownMain)][0]);
                    UpMain=int.Parse(loader.element[nameof(UpMain)][0]);
                    if (loader.element.ContainsKey(nameof(JikokuhyouTrackDisplayKudari)))
                        JikokuhyouTrackDisplayKudari=int.Parse(loader.element[nameof(JikokuhyouTrackDisplayKudari)][0]);
                    if (loader.element.ContainsKey(nameof(JikokuhyouTrackDisplayNobori)))
                        JikokuhyouTrackDisplayNobori=int.Parse(loader.element[nameof(JikokuhyouTrackDisplayNobori)][0]);

                    if (loader.element.ContainsKey(nameof(DiagramTrackDisplay)))
                        DiagramTrackDisplay=int.Parse(loader.element[nameof(DiagramTrackDisplay)][0]);

                    EkiTrack2Cont=new EkiTrack2ContClass(loader.childs[nameof(EkiTrack2Cont)][0]);
                    JikokuhyouJikokuDisplayKudari=new List<int>();
                    foreach (string buf in loader.element[nameof(JikokuhyouJikokuDisplayKudari)])
                    {
                        JikokuhyouJikokuDisplayKudari.Add(int.Parse(buf));
                    }
                    JikokuhyouJikokuDisplayNobori=new List<int>();
                    foreach (string buf in loader.element[nameof(JikokuhyouJikokuDisplayNobori)])
                    {
                        JikokuhyouJikokuDisplayNobori.Add(int.Parse(buf));
                    }
                    JikokuhyouSyubetsuChangeDisplayKudari=new List<int>();
                    foreach (string buf in loader.element[nameof(JikokuhyouSyubetsuChangeDisplayKudari)])
                    {
                        JikokuhyouSyubetsuChangeDisplayKudari.Add(int.Parse(buf));
                    }
                    JikokuhyouSyubetsuChangeDisplayNobori=new List<int>();
                    foreach (string buf in loader.element[nameof(JikokuhyouSyubetsuChangeDisplayNobori)])
                    {
                        JikokuhyouSyubetsuChangeDisplayNobori.Add(int.Parse(buf));
                    }
                    DiagramColorNextEki=int.Parse(loader.element[nameof(DiagramColorNextEki)][0]);

                    JikokuhyouOuterDisplayKudari=new List<int>();
                    foreach (string buf in loader.element[nameof(JikokuhyouOuterDisplayKudari)])
                    {
                        JikokuhyouOuterDisplayKudari.Add(int.Parse(buf));
                    }
                    JikokuhyouOuterDisplayNobori=new List<int>();
                    foreach (string buf in loader.element[nameof(JikokuhyouOuterDisplayNobori)])
                    {
                        JikokuhyouOuterDisplayNobori.Add(int.Parse(buf));
                    }
                }
            }
            public class RessyasyubetsuClass
            {
                public string Syubetsumei;
                public Color JikokuhyouMojiColor;
                public Color JikokuhyouFontIndex;
                public Color JikokuhyouBackColor;
                public Color DiagramSenColor;
                public string DiagramSenStyle;
                public string StopMarkDrawType;

                public RessyasyubetsuClass(Loader loader)
                {
                    Syubetsumei=loader.element[nameof(Syubetsumei)][0];
                    JikokuhyouMojiColor=new Color(uint.Parse(loader.element[nameof(JikokuhyouMojiColor)][0], System.Globalization.NumberStyles.HexNumber));
                    JikokuhyouFontIndex=new Color(uint.Parse(loader.element[nameof(JikokuhyouFontIndex)][0], System.Globalization.NumberStyles.HexNumber));
                    JikokuhyouBackColor=new Color(uint.Parse(loader.element[nameof(JikokuhyouBackColor)][0], System.Globalization.NumberStyles.HexNumber));
                    DiagramSenColor=new Color(uint.Parse(loader.element[nameof(DiagramSenColor)][0], System.Globalization.NumberStyles.HexNumber));
                    DiagramSenStyle=loader.element[nameof(DiagramSenStyle)][0];
                    StopMarkDrawType=loader.element[nameof(StopMarkDrawType)][0];
                }
            }
            public class DiaClass
            {
                public class NoboriKudari
                {
                    public class RessyaClass
                    {
                        public class EkijikokuClass
                        {
                            public int Ekiatsukai;
                            public Time arrivalTime;
                            public Time departureTime;
                            public int Track;
                            public EkijikokuClass(string src)
                            {
                                if (src==string.Empty)
                                {
                                    Ekiatsukai=0;
                                    return;
                                }
                                string[] buf = src.Split(';');
                                if (buf.Length==1)
                                {
                                    buf= buf[0].Split('$');
                                    Ekiatsukai=int.Parse(buf[0]);
                                    Track=int.Parse(buf[1]);
                                }
                                else
                                {
                                    Ekiatsukai=int.Parse(buf[0]);
                                    buf= buf[1].Split('$');
                                    Track=int.Parse(buf[1]);
                                    int index = buf[0].IndexOf('/');
                                    if (index != -1)
                                    {
                                        buf=buf[0].Split('/');
                                        arrivalTime=new Time(int.Parse(buf[0]));
                                        if (buf[1]!="")
                                            departureTime=new Time(int.Parse(buf[1]));
                                    }
                                    else
                                    {
                                        departureTime= new Time(int.Parse(buf[0]));
                                    }
                                }
                            }
                        }
                        public class OperationBeforeClass
                        {
                            public virtual List<string> OperationNumber { get => null; }
                            public class OperationBeforeUnparallelled : OperationBeforeClass
                            {
                                Time UnparallelledTime;
                                string UnparallelledCode;
                                string[] OperationNumbers;
                                public override List<string> OperationNumber { get => OperationNumbers.ToList(); }
                                public OperationBeforeUnparallelled(OperationBeforeClass src, string element) : base(src)
                                {
                                    string[] buf = element.Split('$');
                                    UnparallelledTime=new Time(int.Parse(buf[0]));
                                    buf = buf[1].Split('/');
                                    UnparallelledCode=buf[0];
                                    if(buf.Length>1)
                                    OperationNumbers = buf[1].Split(';');
                                }
                            }
                            public class OperationBeforeConection : OperationBeforeClass
                            {
                                Time DepTime;
                                string[] OperationNumbers;
                                public override List<string> OperationNumber { get => OperationNumbers.ToList(); }
                                public OperationBeforeConection(OperationBeforeClass src, string element) : base(src)
                                {
                                    string[] buf = element.Split('$');
                                    int deptimeint;
                                    if(int.TryParse(buf[0], out deptimeint))
                                    DepTime=new Time(deptimeint);
                                    if(buf.Length>1)
                                    OperationNumbers = buf[1].Split(';');
                                }
                            }
                            protected OperationBeforeClass(OperationBeforeClass src) { }
                            private OperationBeforeClass() { }
                            static public OperationBeforeClass CreateOperationBeforeClass(Loader loader,int ekicount)
                            {
                                List<string> ops;
                                OperationBeforeClass rt= new OperationBeforeClass();
                                for (int i = 0; i<ekicount; ++i)
                                {
                                    if (loader.element.TryGetValue($"Operation{i}B",out ops))
                                    {
                                        string[] buf= ops[0].Split('/');
                                        int opcode;
                                        string element=buf.Length>1? buf[1] :string.Empty;
                                        for(int j=2;j<buf.Length; ++j)
                                        {
                                            element+="/"+buf[j];
                                        }
                                        if (int.TryParse(buf[0], out opcode))
                                        {
                                            switch (opcode)
                                            {
                                                case 3:
                                                    {
                                                        return new OperationBeforeUnparallelled(rt, element);
                                                    }
                                                case 5:
                                                    {
                                                        return new OperationBeforeConection(rt,element);
                                                    }
                                            }
                                        }

                                        break;
                                    }
                                }
                                return rt;
                            }
                        }
                        public class OperationAfterClass
                        {
                            public class OperationAfterUnparallelled : OperationAfterClass
                            {
                                Time UnparallelledTime;
                                string UnparallelledCode;
                                string[] OperationNumbers;
                                public OperationAfterUnparallelled(OperationAfterClass src, string element) : base(src)
                                {
                                    string[] buf = element.Split('$');
                                    UnparallelledTime=new Time(int.Parse(buf[0]));
                                    buf = buf[1].Split('/');
                                    UnparallelledCode=buf[0];
                                    if (buf.Length>1)
                                        OperationNumbers = buf[1].Split(';');
                                }
                            }
                            public class OperationAfterConection : OperationAfterClass
                            {
                                Time DepTime;
                                int conectType;
                                public OperationAfterConection(OperationAfterClass src, string element) : base(src)
                                {
                                    string[] buf = element.Split('$');
                                    int deptimeint;
                                    if(int.TryParse(buf[0],out deptimeint))
                                    DepTime=new Time(deptimeint);
                                    if (buf.Length>2)
                                        int.TryParse(buf[1], out conectType); 
                                    return;
                                }
                            }
                            protected OperationAfterClass(OperationAfterClass src) { }
                            private OperationAfterClass() { }
                            static public OperationAfterClass CreateOperationAfterClass(Loader loader,int ekicount)
                            {
                                List<string> ops;
                                OperationAfterClass rt= new OperationAfterClass();
                                for (int i = 0; i<ekicount; ++i)
                                {
                                    if (loader.element.TryGetValue($"Operation{i}A",out ops))
                                    {
                                        string[] buf= ops[0].Split('/');
                                        int opcode; 
                                        string element = buf.Length>1 ? buf[1] : string.Empty;
                                        for (int j = 2; j<buf.Length; ++j)
                                        {
                                            element+="/"+buf[j];
                                        }
                                        if (int.TryParse(buf[0], out opcode))
                                        {
                                            switch (opcode)
                                            {
                                                case 3:
                                                    {
                                                        return new OperationAfterUnparallelled(rt, element);
                                                    }
                                                case 5:
                                                    {
                                                        return new OperationAfterConection(rt, element);
                                                    }
                                            }
                                        }

                                        break;
                                    }
                                }
                                return rt;
                            }
                        }


                        public string Houkou;
                        public int Syubetsu;
                        public string Ressyabangou;
                        public string Ressyamei;
                        public string Gousuu;
                        public List<EkijikokuClass> EkiJikoku;
                        public OperationBeforeClass OperationB;
                        public OperationAfterClass OperationA;
                        public string Bikou;

                        public int from;
                        public int to;

                        public RessyaClass(Loader loader)
                        {
                            Houkou=loader.element[nameof(Houkou)][0];
                            Syubetsu=int.Parse(loader.element[nameof(Syubetsu)][0]);
                            Ressyabangou=loader.element[nameof(Ressyabangou)][0];
                            if (loader.element.ContainsKey(nameof(Ressyamei)))
                                Ressyamei=loader.element[nameof(Ressyamei)][0];
                            if (loader.element.ContainsKey(nameof(Gousuu)))
                                Gousuu=loader.element[nameof(Gousuu)][0];
                            EkiJikoku=new List<EkijikokuClass>();
                            foreach (string buf in loader.element[nameof(EkiJikoku)])
                            {
                                EkiJikoku.Add(new EkijikokuClass(buf));
                            }
                            OperationB=OperationBeforeClass.CreateOperationBeforeClass(loader, EkiJikoku.Count);
                            OperationA=OperationAfterClass.CreateOperationAfterClass(loader,  EkiJikoku.Count);
                            if (loader.element.ContainsKey(nameof(Bikou)))
                                Bikou=loader.element[nameof(Bikou)][0];

                            for(int i = 0;i< EkiJikoku.Count;++i)
                            {
                                if(EkiJikoku[i].Ekiatsukai!=0)
                                {
                                    from=i;
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            to=EkiJikoku.Count-1;
                        }
                    }
                    public List<RessyaClass> Ressya;

                    public NoboriKudari(Loader loader)
                    {
                        Ressya=new List<RessyaClass>();
                        foreach (Loader loader1 in loader.childs[nameof(Ressya)])
                        {
                            Ressya.Add(new RessyaClass(loader1));
                        }
                        return;
                    }
                }
                public string DiaName;
                public int MainBackColorIndex;
                public int SubBackColorIndex;
                public int BackPatternIndex;
                public NoboriKudari Kudari;
                public NoboriKudari Nobori;

                public DiaClass(Loader loader)
                {
                    DiaName=loader.element[nameof(DiaName)][0];
                    MainBackColorIndex=int.Parse(loader.element[nameof(MainBackColorIndex)][0]);
                    SubBackColorIndex=int.Parse(loader.element[nameof(SubBackColorIndex)][0]);
                    BackPatternIndex=int.Parse(loader.element[nameof(BackPatternIndex)][0]);
                    Kudari=new NoboriKudari(loader.childs[nameof(Kudari)][0]);
                    Nobori=new NoboriKudari(loader.childs[nameof(Nobori)][0]);
                }
            }
            public string Rosenmei;
            public string KudariDiaAlias;
            public string NoboriDiaAlias;
            public List<EkiClass> Eki;
            public List<RessyasyubetsuClass> Ressyasyubetsu;
            public List<DiaClass> Dia;
            public int KitenJikoku;
            public int DiagramDgrYZahyouKyoriDefault;
            public int OperationCrossKitenJikoku;
            public int KijunDiaIndex;
            public string Comment;

            public RosenClass(Loader loader)
            {
                Rosenmei=loader.element[nameof(Rosenmei)][0];
                KudariDiaAlias=loader.element[nameof(KudariDiaAlias)][0];
                NoboriDiaAlias=loader.element[nameof(NoboriDiaAlias)][0];
                Eki=new List<EkiClass>();
                foreach (Loader loader1 in loader.childs[nameof(Eki)])
                {
                    Eki.Add(new EkiClass(loader1));
                }
                Ressyasyubetsu=new List<RessyasyubetsuClass>();
                foreach (Loader loader1 in loader.childs[nameof(Ressyasyubetsu)])
                {
                    Ressyasyubetsu.Add(new RessyasyubetsuClass(loader1));
                }
                Dia=new List<DiaClass>();
                foreach (Loader loader1 in loader.childs[nameof(Dia)])
                {
                    Dia.Add(new DiaClass(loader1));
                }
                KitenJikoku=int.Parse(loader.element[nameof(KitenJikoku)][0]);
                DiagramDgrYZahyouKyoriDefault=int.Parse(loader.element[nameof(DiagramDgrYZahyouKyoriDefault)][0]);
                if(loader.element.ContainsKey(nameof(OperationCrossKitenJikoku)))
                OperationCrossKitenJikoku=int.Parse(loader.element[nameof(OperationCrossKitenJikoku)][0]);
                if(loader.element.ContainsKey(nameof(KijunDiaIndex)))
                KijunDiaIndex=int.Parse(loader.element[nameof(KijunDiaIndex)][0]);
                if(loader.element.ContainsKey(nameof(Comment)))
                Comment=loader.element[nameof(Comment)][0];
            }
        }
        public class DispPropClass
        {
            public List<Font> JikokuhyouFont;
            public Font JikokuhyouVFont;
            public Font DiaEkimeiFont;
            public Font DiaJikokuFont;
            public Font DiaRessyaFont;
            public Font OperationTableFont;
            public Font AllOperationTableJikokuFont;
            public Font CommentFont;
            public Color DiaMojiColor;
            public List<Color> DiaBackColor;
            public Color DiaRessyaColor;
            public Color DiaJikuColor;
            public List<Color> JikokuhyouBackColor;
            public Color StdOpeTimeLowerColor;
            public Color StdOpeTimeHigherColor;
            public Color StdOpeTimeUndefColor;
            public Color StdOpeTimeIllegalColor;
            public Color OperationStringColor;
            public Color OperationGridColor;
            public int EkimeiLength;
            public int JikokuhyouRessyaWidth;
            public int AnySecondIncDec1;
            public int AnySecondIncDec2;
            public int DisplayRessyamei;
            public int DisplayOuterTerminalEkimeiOriginSide;
            public int DisplayOuterTerminalEkimeiTerminalSide;
            public int DiagramDisplayOuterTerminal;
            public int SecondRoundChaku;
            public int SecondRoundHatsu;
            public int Display2400;
            public int OperationNumberRows;
            public int DisplayInOutLinkCode;

            public DispPropClass(Loader loader)
            {
                JikokuhyouFont=new List<Font>();
                foreach (string buf in loader.element[nameof(JikokuhyouFont)])
                {
                    JikokuhyouFont.Add(new Font(buf));
                }
                JikokuhyouVFont=new Font(loader.element[nameof(JikokuhyouVFont)][0]);
                DiaEkimeiFont=new Font(loader.element[nameof(DiaEkimeiFont)][0]);
                DiaJikokuFont=new Font(loader.element[nameof(DiaJikokuFont)][0]);
                DiaRessyaFont=new Font(loader.element[nameof(DiaRessyaFont)][0]);
                OperationTableFont=new Font(loader.element[nameof(OperationTableFont)][0]);
                AllOperationTableJikokuFont=new Font(loader.element[nameof(AllOperationTableJikokuFont)][0]);
                CommentFont=new Font(loader.element[nameof(CommentFont)][0]);
                DiaMojiColor=new Color(uint.Parse(loader.element[nameof(DiaMojiColor)][0]));
                DiaBackColor=new List<Color>();
                foreach (string buf in loader.element[nameof(DiaBackColor)])
                {
                    DiaBackColor.Add(new Color(uint.Parse(buf, System.Globalization.NumberStyles.HexNumber)));
                }
                DiaRessyaColor=new Color(uint.Parse(loader.element[nameof(DiaRessyaColor)][0], System.Globalization.NumberStyles.HexNumber));
                DiaJikuColor=new Color(uint.Parse(loader.element[nameof(DiaJikuColor)][0], System.Globalization.NumberStyles.HexNumber));
                JikokuhyouBackColor=new List<Color>();
                foreach (string buf in loader.element[nameof(JikokuhyouBackColor)])
                {
                    JikokuhyouBackColor.Add(new Color(uint.Parse(buf, System.Globalization.NumberStyles.HexNumber)));
                }
                StdOpeTimeLowerColor=new Color(uint.Parse(loader.element[nameof(StdOpeTimeLowerColor)][0], System.Globalization.NumberStyles.HexNumber));
                StdOpeTimeHigherColor=new Color(uint.Parse(loader.element[nameof(StdOpeTimeHigherColor)][0], System.Globalization.NumberStyles.HexNumber));
                StdOpeTimeUndefColor=new Color(uint.Parse(loader.element[nameof(StdOpeTimeUndefColor)][0], System.Globalization.NumberStyles.HexNumber));
                StdOpeTimeIllegalColor=new Color(uint.Parse(loader.element[nameof(StdOpeTimeIllegalColor)][0], System.Globalization.NumberStyles.HexNumber));
                OperationStringColor=new Color(uint.Parse(loader.element[nameof(OperationStringColor)][0], System.Globalization.NumberStyles.HexNumber));
                OperationGridColor=new Color(uint.Parse(loader.element[nameof(OperationGridColor)][0], System.Globalization.NumberStyles.HexNumber));
                EkimeiLength=int.Parse(loader.element[nameof(EkimeiLength)][0]);
                JikokuhyouRessyaWidth=int.Parse(loader.element[nameof(JikokuhyouRessyaWidth)][0]);
                AnySecondIncDec1=int.Parse(loader.element[nameof(AnySecondIncDec1)][0]);
                AnySecondIncDec2=int.Parse(loader.element[nameof(AnySecondIncDec2)][0]);
                DisplayRessyamei=int.Parse(loader.element[nameof(DisplayRessyamei)][0]);
                DisplayOuterTerminalEkimeiOriginSide=int.Parse(loader.element[nameof(DisplayOuterTerminalEkimeiOriginSide)][0]);
                DisplayOuterTerminalEkimeiTerminalSide=int.Parse(loader.element[nameof(DisplayOuterTerminalEkimeiTerminalSide)][0]);
                DiagramDisplayOuterTerminal=int.Parse(loader.element[nameof(DiagramDisplayOuterTerminal)][0]);
                SecondRoundChaku=int.Parse(loader.element[nameof(SecondRoundChaku)][0]);
                SecondRoundHatsu=int.Parse(loader.element[nameof(SecondRoundHatsu)][0]);
                Display2400=int.Parse(loader.element[nameof(Display2400)][0]);
                OperationNumberRows=int.Parse(loader.element[nameof(OperationNumberRows)][0]);
                DisplayInOutLinkCode=int.Parse(loader.element[nameof(DisplayInOutLinkCode)][0]);
            }
        }
        public class WindowPlacementClass
        {
            public class ChildWindowClass
            {
                public int WindowType;
                public int DiaIndex;
                public int XPos;
                public int YPos;
                public int XSize;
                public int YSize;

                public ChildWindowClass(Loader loader)
                {
                    WindowType=int.Parse(loader.element[nameof(WindowType)][0]);
                    DiaIndex=int.Parse(loader.element[nameof(DiaIndex)][0]);
                    XPos=int.Parse(loader.element[nameof(XPos)][0]);
                    YPos=int.Parse(loader.element[nameof(YPos)][0]);
                    XSize=int.Parse(loader.element[nameof(XSize)][0]);
                    YSize=int.Parse(loader.element[nameof(YSize)][0]);
                }
            }
            public int RosenViewWidth;
            public List<ChildWindowClass> ChildWindow;

            public WindowPlacementClass(Loader loader)
            {
                RosenViewWidth=int.Parse(loader.element[nameof(RosenViewWidth)][0]);
                ChildWindow = new List<ChildWindowClass>();
                if(loader.childs.ContainsKey(nameof(ChildWindow)))
                foreach (Loader loader1 in loader.childs[nameof(ChildWindow)])
                {
                    ChildWindow.Add(new ChildWindowClass(loader1));
                }
            }
        }
        public string FileType;
        public RosenClass Rosen;
        public DispPropClass DispProp;
        public WindowPlacementClass WindowPlacement;
        public string FileTypeAppComment;

        public Oud2(Loader loader)
        {
            if (!loader.element.ContainsKey(nameof(FileType)))
            {
                FileType=null;
                return; 
            }
            FileType=loader.element[nameof(FileType)][0];
            Rosen=new RosenClass(loader.childs[nameof(Rosen)][0]);
            DispProp=new DispPropClass(loader.childs[nameof(DispProp)][0]);
            if(loader.childs.ContainsKey(nameof(WindowPlacement)))
            WindowPlacement=new WindowPlacementClass(loader.childs[nameof(WindowPlacement)][0]);
            FileTypeAppComment=loader.element[nameof(FileTypeAppComment)][0];
        }
    }
}
