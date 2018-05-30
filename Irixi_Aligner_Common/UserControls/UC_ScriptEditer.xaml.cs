using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScintillaNET;
using ScintillaNET.WPF;
using ScintillaNET_FindReplaceDialog;
using SN=ScintillaNET;
using Irixi_Aligner_Common.Configuration.ScriptCfg;
using Irixi_Aligner_Common.Classes;
using GalaSoft.MvvmLight.Messaging;
using System.Threading;
using GalaSoft.MvvmLight.Command;
using Irixi_Aligner_Common.Configuration.Common;
using Irixi_Aligner_Common.ViewModel;
using Irixi_Aligner_Common.Classes.BaseClass;
using Irixi_Aligner_Common.MotionControllers.Base;
using Irixi_Aligner_Common.Equipments.Base;
using GalaSoft.MvvmLight.Ioc;
using System.IO;
using System.Windows.Forms;

namespace Irixi_Aligner_Common.UserControls
{
    /// <summary>
    /// UC_ScriptEditer.xaml 的交互逻辑
    /// </summary>
    

    public partial class UC_ScriptEditer : System.Windows.Controls.UserControl
    {
        #region private for Sci
        private FindReplace MyFindReplace = new FindReplace();
        private FuncManager Funcmanager = null;
        private ObservableCollectionEx<LogicalAxis> LogicAxisList = null;
        private ObservableCollectionEx<InstrumentBase> InstrumentList = null;
        private StringBuilder sbAxisStructPromt = new StringBuilder();
        private StringBuilder sbInstrumentStructPromt = new StringBuilder();
        private StringBuilder sbEnumPromt1 = new StringBuilder();
        private Dictionary<string,StringBuilder> sbEnumPromt2Dic = new Dictionary<string, StringBuilder>();
        private bool bNeedSaved=true;
        

        private StringBuilder sbWord1 = new StringBuilder();
        private Dictionary<string, string> strCateDic = new Dictionary<string, string>();
        private Dictionary<string, List<String>> rawDataDic = new Dictionary<string, List<String>>();
        private List<Node> _treeViewItemSource = new List<Node>();
        private const string NEW_DOCUMENT_TEXT = "Untitled";
        private const int LINE_NUMBERS_MARGIN_WIDTH = 30; // TODO - don't hardcode this
        private delegate void ErrorHappened(Exception e);
        private Thread ScriptThread = null;
        /// <summary>
        /// the background color of the text area
        /// </summary>
        private const int BACK_COLOR = 0x2A211C;

        /// <summary>
        /// default text color of the text area
        /// </summary>
        private const int FORE_COLOR = 0xB7B7B7;

        /// <summary>
        /// change this to whatever margin you want the line numbers to show in
        /// </summary>
        private const int NUMBER_MARGIN = 1;

        /// <summary>
        /// change this to whatever margin you want the bookmarks/breakpoints to show in
        /// </summary>
        private const int BOOKMARK_MARGIN = 2;

        private const int BOOKMARK_MARKER = 2;
        private int _zoomLevel = 8;
        /// <summary>
        /// change this to whatever margin you want the code folding tree (+/-) to show in
        /// </summary>
        private const int FOLDING_MARGIN = 3;
        private const bool CODEFOLDING_CIRCULAR = false;
        private string strPromot = "";
        private int nStartPos = -1;
        private int nEndPos = -1;
        private string strFinalScript = "";
        #endregion


        #region private for Luawrapper
        private LuaWrapper lua = new LuaWrapper();

        #endregion
        public UC_ScriptEditer()
        {
            InitializeComponent();
            SetScintillaToCurrentOptions(TextArea);
            StrFilePath = "";
            Messenger.Default.Register<string>(this,"ScriptStart",m => {
                {
                    //Task task = new Task(new Action(StartScript));
                    //task.Start();
                    //May be a bug ,I don't know why now,must reset the debughook everytime.


                    SystemService service = SimpleIoc.Default.GetInstance<SystemService>();
                    if (service.ScriptState == ScriptState.PAUSE)
                        service.Resume();
                    else
                    {
                        lua.lua.DebugHook -= Lua_DebugHook;
                        lua.lua.DebugHook += Lua_DebugHook;

                        if (ScriptThread == null || ScriptThread.ThreadState != ThreadState.Running)
                        {
                            ScriptThread = new Thread(new ThreadStart(StartScript));
                            ScriptThread.Start();
                        }
                    } 
                }
            });
            Messenger.Default.Register<string>(this, "ScriptStop", m => {
                {
                    if(ScriptThread!=null)
                        ScriptThread.Abort();
                }
            });
        }
        ~UC_ScriptEditer()
        {
            lua.lua.DebugHook -= Lua_DebugHook;
            Messenger.Default.Unregister<string>("ScriptStart");
            Messenger.Default.Unregister<string>("ScriptStop");
            lua.CloseLua();
        }
        private void Lua_DebugHook(object sender, NLua.Event.DebugHookEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {   
                TextArea.MarkerDeleteAll(BOOKMARK_MARKER);
                var line = TextArea.Lines[e.LuaDebug.currentline - 1];
                Console.WriteLine(e.LuaDebug.currentline - 1);
                line.MarkerAdd(BOOKMARK_MARKER);
                
            });
        }
        private void SetKeyWords(ScintillaWPF ScintillaNet)
        {
            ScintillaNet.CharAdded += ScintillaNet_CharAdded;
            ScintillaNet.Delete += ScintillaNet_Delete;

            //For AutoComplete
            foreach (var it in rawDataDic)
            {
                sbWord1.Append(it.Key);
                sbWord1.Append(" ");
                strCateDic.Add(it.Key, "");
                foreach (var funcName in it.Value)
                {
                    sbWord1.Append(it.Key);
                    sbWord1.Append(".");
                    sbWord1.Append(funcName);
                    sbWord1.Append(" ");
                    sbWord1.Append(funcName);
                    sbWord1.Append(" ");
                    strCateDic[it.Key] += funcName;
                    strCateDic[it.Key] += " ";            
                }
            }
 
            //Axis
            LogicAxisList = (DataContext as ViewModelLocator).Service.LogicalAxisCollection;
            InstrumentList = (DataContext as ViewModelLocator).Service.MeasurementInstrumentCollection;
            foreach (var axis in LogicAxisList)
            {
                string strAxisPromt = axis.ToString().Replace(" ", "").Replace("*", "").Replace("@", "_").ToUpper();
                sbWord1.Append(strAxisPromt);
                sbWord1.Append(" ");
                sbWord1.Append("AXIS.");
                sbWord1.Append(strAxisPromt);
                sbWord1.Append(" ");

                sbAxisStructPromt.Append(strAxisPromt);
                sbAxisStructPromt.Append(" ");
            }
            sbWord1.Append("AXIS");
            sbWord1.Append(" ");

            //Instrument
            foreach (var instrument in InstrumentList)
            {
                string strInstrumentPromt = instrument.Config.Caption.Replace(" ", "_").ToUpper();
                sbWord1.Append(strInstrumentPromt);
                sbWord1.Append(" ");
                sbWord1.Append("INST.");
                sbWord1.Append(strInstrumentPromt);
                sbWord1.Append(" ");

                sbInstrumentStructPromt.Append(strInstrumentPromt);
                sbInstrumentStructPromt.Append(" ");
            }
            sbWord1.Append("INST");
            sbWord1.Append(" ");

            //Enum
            List<KeyValuePair<string,List<KeyValuePair<string,int>>>> enumInfos = (DataContext as ViewModelLocator).Service.EnumInfos["ENUM"];
            sbWord1.Append("ENUM");
            sbWord1.Append(" ");
  
            foreach (var kp in enumInfos)
            {
                sbWord1.Append("ENUM.");
                sbWord1.Append(kp.Key);
                sbWord1.Append(" ");
                sbWord1.Append(kp.Key);
                sbWord1.Append(" ");

                sbEnumPromt1.Append(kp.Key);
                sbEnumPromt1.Append(" ");

                StringBuilder sbEnumPromt2 = new StringBuilder();
                foreach (var it in kp.Value)
                {
                    sbWord1.Append("ENUM.");
                    sbWord1.Append(kp.Key);
                    sbWord1.Append(".");
                    sbWord1.Append(it.Key);
                    sbWord1.Append(" ");
                    sbWord1.Append(it.Key);
                    sbWord1.Append(" ");

                    sbEnumPromt2.Append(it.Key);
                    sbEnumPromt2.Append(" ");
                }
                sbEnumPromt2Dic.Add(kp.Key, sbEnumPromt2);
            }              
            
            //string sss = sbWord1.ToString();
            ScintillaNet.SetKeywords(1, sbWord1.ToString());
        }

        private void InitBookmarkMargin(ScintillaWPF ScintillaNet)
        {
            //TextArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));

            var margin = ScintillaNet.Margins[BOOKMARK_MARGIN];
            margin.Width = 20;
            margin.Sensitive = true;
            margin.Type = MarginType.Symbol;
            margin.Mask = (1 << BOOKMARK_MARKER);
            //margin.Cursor = MarginCursor.Arrow;

            var marker = ScintillaNet.Markers[BOOKMARK_MARKER];
            marker.Symbol = MarkerSymbol.Arrows;
            //marker.SetBackColor(IntToColor(0xFF003B));
            //marker.SetForeColor(IntToColor(0x000000));

            marker.SetBackColor(IntToColor(0xFF003B));
            marker.SetForeColor(IntToColor(0x00FF00));

            marker.SetAlpha(100);
        }
        private void InitCodeFolding(ScintillaWPF ScintillaNet)
        {
            ScintillaNet.SetFoldMarginColor(true, IntToMediaColor(BACK_COLOR));
            ScintillaNet.SetFoldMarginHighlightColor(true, IntToMediaColor(BACK_COLOR));

            // Enable code folding
            ScintillaNet.SetProperty("fold", "1");
            ScintillaNet.SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            ScintillaNet.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
            ScintillaNet.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
            ScintillaNet.Margins[FOLDING_MARGIN].Sensitive = true;
            ScintillaNet.Margins[FOLDING_MARGIN].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                ScintillaNet.Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
                ScintillaNet.Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
            }

            // Configure folding markers with respective symbols
            ScintillaNet.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
            ScintillaNet.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
            ScintillaNet.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
            ScintillaNet.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            ScintillaNet.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
            ScintillaNet.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            ScintillaNet.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            ScintillaNet.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
        }
        private void InitColors(ScintillaWPF ScintillaNet)
        {
            ScintillaNet.CaretForeColor = Colors.White;
            ScintillaNet.SetSelectionBackColor(true, IntToMediaColor(0x114D9C));

            //FindReplace.Indicator.ForeColor = System.Drawing.Color.DarkOrange;
        }
        private void InitNumberMargin(ScintillaWPF ScintillaNet)
        {
            ScintillaNet.Styles[ScintillaNET.Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
            ScintillaNet.Styles[ScintillaNET.Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
            ScintillaNet.Styles[ScintillaNET.Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
            ScintillaNet.Styles[ScintillaNET.Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

            var nums = ScintillaNet.Margins[NUMBER_MARGIN];
            nums.Width = LINE_NUMBERS_MARGIN_WIDTH;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;
            ScintillaNet.MarginClick += TextArea_MarginClick;
        }
        private void InitSyntaxColoring(ScintillaWPF ScintillaNet)
        {
            // Configure the default style
            ScintillaNet.StyleResetDefault();
            ScintillaNet.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            ScintillaNet.Styles[ScintillaNET.Style.Default].Size = 10;
            ScintillaNet.Styles[ScintillaNET.Style.Default].BackColor = IntToColor(0x212121);
            ScintillaNet.Styles[ScintillaNET.Style.Default].ForeColor = IntToColor(0xFFFFFF);
            ScintillaNet.StyleClearAll();

            // Configure the Lua lexer styles
            ScintillaNet.Styles[ScintillaNET.Style.Lua.Identifier].ForeColor = IntToColor(0xD0DAE2);
            ScintillaNet.Styles[ScintillaNET.Style.Lua.Comment].ForeColor = IntToColor(0xBD758B);
            ScintillaNet.Styles[ScintillaNET.Style.Lua.CommentLine].ForeColor = IntToColor(0x40BF57);
            ScintillaNet.Styles[ScintillaNET.Style.Lua.CommentDoc].ForeColor = IntToColor(0x2FAE35);
            ScintillaNet.Styles[ScintillaNET.Style.Lua.Number].ForeColor = IntToColor(0xFFFF00);
            ScintillaNet.Styles[ScintillaNET.Style.Lua.String].ForeColor = IntToColor(0xFFFF00);
            ScintillaNet.Styles[ScintillaNET.Style.Lua.Character].ForeColor = IntToColor(0xE95454);
            ScintillaNet.Styles[ScintillaNET.Style.Lua.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
            ScintillaNet.Styles[ScintillaNET.Style.Lua.Operator].ForeColor = IntToColor(0xE0E0E0);
            ScintillaNet.Styles[ScintillaNET.Style.Lua.Word].ForeColor = IntToColor(0x48A8EE);
            ScintillaNet.Styles[ScintillaNET.Style.Lua.Word2].ForeColor = IntToColor(0xF98906);


            ScintillaNet.Lexer = Lexer.Lua;
            ScintillaNet.SetKeywords(0, "and or not function table nil for while do break in return until goto repeat true false if then else elseif local end");
        }
        private void SetScintillaToCurrentOptions(ScintillaWPF ScintillaNet)
        {
            ScintillaNet.KeyDown += ScintillaNet_KeyDown;

            // INITIAL VIEW CONFIG
            ScintillaNet.WrapMode = WrapMode.None;
            ScintillaNet.IndentationGuides = IndentView.LookBoth;

            // STYLING
            InitColors(ScintillaNet);
            InitSyntaxColoring(ScintillaNet);

            // NUMBER MARGIN
            InitNumberMargin(ScintillaNet);

            // BOOKMARK MARGIN
            InitBookmarkMargin(ScintillaNet);

            // CODE FOLDING MARGIN
            InitCodeFolding(ScintillaNet);

            // DRAG DROP
            // TODO - Enable InitDragDropFile
            //InitDragDropFile();

            // INIT HOTKEYS
            // TODO - Enable InitHotkeys
            //InitHotkeys(ScintillaNet);

            // Turn on line numbers?
            //if (lineNumbersMenuItem.IsChecked)
            //    TextArea.Margins[NUMBER_MARGIN].Width = LINE_NUMBERS_MARGIN_WIDTH;
            //else
            //    TextArea.Margins[NUMBER_MARGIN].Width = 0;

            //// Turn on white space?
            //if (whitespaceMenuItem.IsChecked)
            //    TextArea.ViewWhitespace = WhitespaceMode.VisibleAlways;
            //else
            //    TextArea.ViewWhitespace = WhitespaceMode.Invisible;

            //// Turn on word wrap?
            //if (wordWrapMenuItem.IsChecked)
            //    TextArea.WrapMode = WrapMode.Word;
            //else
            //    TextArea.WrapMode = WrapMode.None;

            //// Show EOL?
            //TextArea.ViewEol = endOfLineMenuItem.IsChecked;

            // Set the zoom
            TextArea.Zoom = _zoomLevel;
        }
        private static Color IntToMediaColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }
        private static System.Drawing.Color IntToColor(int rgb)
        {
            return System.Drawing.Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }
        private void ScintillaNet_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == System.Windows.Forms.Keys.F)
            {
                MyFindReplace.ShowFind();
                e.SuppressKeyPress = true;
            }
            else if (e.Shift && e.KeyCode == System.Windows.Forms.Keys.F3)
            {
                MyFindReplace.Window.FindPrevious();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.F3)
            {
                MyFindReplace.Window.FindNext();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.H)
            {
                MyFindReplace.ShowReplace();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.I)
            {
                MyFindReplace.ShowIncrementalSearch();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.G)
            {
                GoTo MyGoTo = new GoTo((Scintilla)sender);
                MyGoTo.ShowGoToDialog();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.S)
            {

            }
        }
        private void ScintillaNet_CharAdded(object sender, CharAddedEventArgs e)
        {
            ScriptHelpMgr.Instance.bCompileError = true;
            bNeedSaved = true;
            int nPos = TextArea.CurrentPosition;
            //AXIS
            if (e.Char == '.')
            {
                string sWord = TextArea.GetWordFromPosition(nPos - 1);
                if (sWord.Contains("AXIS"))
                    TextArea.AutoCShow(0, sbAxisStructPromt.ToString());
                else if (sWord.Contains("INST"))
                    TextArea.AutoCShow(0, sbInstrumentStructPromt.ToString());
                else if (sWord.Contains("ENUM"))
                    TextArea.AutoCShow(0, sbEnumPromt1.ToString());
                else if (sbEnumPromt1.ToString().Contains(sWord) && sbEnumPromt2Dic.Keys.Contains(sWord))
                    TextArea.AutoCShow(0, sbEnumPromt2Dic[sWord].ToString());
                else
                {
                    foreach (var it in rawDataDic)
                    {
                        if (it.Key == sWord)
                        {
                            TextArea.AutoCShow(0, strCateDic[sWord]);   //strCateDic : Cate-----"func1 func2 func3"
                            break;
                        }
                    }
                }
            }
            else if (e.Char == '(')
            {
                string sWord = TextArea.GetWordFromPosition(nPos - 1);
                foreach (var it in Funcmanager.Funcs)
                {
                    if (it.FunctionName == sWord)
                    {
                        strPromot = it.Prompt;
                        TextArea.CallTipShow(nPos, strPromot);
                        break;
                    }
                }
                nStartPos = strPromot.IndexOf('(');
                nEndPos = strPromot.IndexOf(',') == -1 ? strPromot.IndexOf(')') : strPromot.IndexOf(',');
                TextArea.CallTipSetHlt(nStartPos, nEndPos);
            }
            else if (e.Char == ')')
            {
                TextArea.CallTipCancel();
                strPromot = "";
                nEndPos = 0;
            }
            else if (e.Char == ',')
            {
                int pos1 = nEndPos;
                if (pos1 < strPromot.Length)
                {
                    int pos2 = strPromot.IndexOf(',', pos1 + 1) == -1 ? strPromot.IndexOf(')') : strPromot.IndexOf(',', pos1 + 1);
                    TextArea.CallTipShow(nPos, strPromot);
                    TextArea.CallTipSetHlt(pos1, pos2);
                    nEndPos = pos2;
                }
            }
        }

        private void ScintillaNet_Delete(object sender, ModificationEventArgs e)
        {
            ScriptHelpMgr.Instance.bCompileError = true;
            bNeedSaved = true;
        }
        private void TextArea_MarginClick(object sender, MarginClickEventArgs e)
        {
            if (e.Margin == BOOKMARK_MARGIN)
            {
                // Do we have a marker for this line?
                const uint mask = (1 << BOOKMARK_MARKER);
                var line = TextArea.Lines[TextArea.LineFromPosition(e.Position)];
                if ((line.MarkerGet() & mask) > 0)
                {
                    // Remove existing bookmark
                    line.MarkerDelete(BOOKMARK_MARKER);
                }
                else
                {
                    // Add bookmark
                    line.MarkerAdd(BOOKMARK_MARKER);
                }
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Funcmanager = (DataContext as ViewModelLocator).Configuration.FuncManager;
            InitTreeView();
            ScriptHelpMgr.Instance.InitAllInstrumentAndAxis();
            SetKeyWords(TextArea);
        }
        public void InitTreeView()
        {
            //Funcs
            foreach (var it in Funcmanager.Funcs)       //Level 1
            {
                if (!rawDataDic.Keys.Contains(it.Category))
                    rawDataDic.Add(it.Category, new List<string>() { it.FunctionName });
                else
                    rawDataDic[it.Category].Add(it.FunctionName);
            }


            //Enum
            List<KeyValuePair<string, List<KeyValuePair<string, int>>>> enumInfos = (DataContext as ViewModelLocator).Service.EnumInfos["ENUM"];
            List<Node> N2 = new List<Node>();
            foreach (var kp in enumInfos) 
            {
                List<Node> N3 = new List<Node>();
                foreach (var kp1 in kp.Value)   //Dic[]
                {          
                    N3.Add(new Node() { Name=kp1.Key,ToolTip= string.Format("{0}:  Dec  {1:D} ,   Hex  0x{2:X}",kp1.Key,kp1.Value,kp1.Value) });                   
                }
                N2.Add(new Node() { Name = kp.Key, NodeList = N3 ,ToolTip= kp.Key });              
            }

            //AXIS  &  INST & Enum
            foreach (var dic in rawDataDic)         
            {
                List<Node> AxisInstRoot = new List<Node>();
                foreach (string str in dic.Value)
                {
                    AxisInstRoot.Add(new Node() { Name = str, ToolTip = (from func in Funcmanager.Funcs where func.FunctionName == str select func).ElementAt(0).Prompt });
                }
                _treeViewItemSource.Add(new Node()
                {
                    Name = dic.Key,
                    NodeList = AxisInstRoot,
                    ToolTip = string.Format(dic.Key)
                });
            }
            _treeViewItemSource.Add(new Node() { Name = "ENUM", NodeList = N2 ,ToolTip= "ENUM"});
 
            TreeView1.ItemsSource = TreeViewItemSource;
        }
        public List<Node> TreeViewItemSource
        {
            get { return _treeViewItemSource; }
            set { _treeViewItemSource = value; }
        }
        //RunScript
        private void StartScript()
        {
            string strScript = "";
            this.Dispatcher.Invoke(() =>
            {
                strScript = TextArea.Text;
            });
            try
            {
                SimpleIoc.Default.GetInstance<SystemService>().ScriptState = ScriptState.BUSY;
                if (ScriptHelpMgr.Instance.bCompileError)
                    CompileScriptFile();
                if(!ScriptHelpMgr.Instance.bCompileError)
                    lua.DoString(strFinalScript);
                               
            }
            catch (Exception e)
            {
                Messenger.Default.Send<string>(string.Format("LuaError : {0}", e.Message), "ScriptCompileError");
            }
            finally
            {
                Messenger.Default.Send<string>("","ScriptFinish");
            }
        }
        //Compile
        private void btn_cmp_Click(object sender, RoutedEventArgs e)
        { 
            CompileScriptFile();
        }
        private bool CompileScriptFile()
        {
            try
            {
                ScriptHelpMgr.Instance.bCompile = true;
                ScriptHelpMgr.Instance.bCompileError = false;
                System.Windows.Application.Current.Dispatcher.Invoke(() => strFinalScript = TextArea.Text.Replace("Axis.", "").Replace("IO.", "").Replace("System.", "").Replace("Equipment.", "").Replace("ENUM.",""));
                lua.DoString(strFinalScript);
                if (!ScriptHelpMgr.Instance.bCompileError)
                {
                    Messenger.Default.Send<string>("CompileOk", "ScriptCompileOk");
                    return true;
                }
                else
                {
                    Messenger.Default.Send<string>(string.Format("LuaCompileError : {0}", ScriptHelpMgr.Instance.lastUnhandledError), "ScriptCompileError");
                    return false;
                }
            }
            catch (Exception ex)
            {               
                Messenger.Default.Send<string>(string.Format("LuaCompileError: Source: {0}, {1},", ex.Source, ex.Message), "ScriptCompileError");
                return false;
            }
            finally
            {
                ScriptHelpMgr.Instance.bCompile = false;
            }

        }
        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            ScriptThread.Abort();     
        }
        private void btn_run_Click(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send<string>("","ScriptStart");
        }
        private void btn_CloseScript_Click(object sender, RoutedEventArgs e)
        {
            if (bNeedSaved)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("The file is modified, do you want to save it before close the file?", "GPAS",MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    if (OnClickSaveFile())
                    {
                        TextArea.Text = "";
                        StrFilePath = "";
                        bNeedSaved = false;
                    }
                }
                else if(result==MessageBoxResult.No)
                    TextArea.Text = "";
            }
            else
                TextArea.Text = "";
        }

        private void btn_OpenScript_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "请选择要打开的文件";
            ofd.Multiselect = true;
            ofd.InitialDirectory = @"C:\";
            ofd.Filter = "文本文件 | *.fc";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string path = ofd.FileName;
                if (path == "")
                    return;
                else
                    StrFilePath = path;
            }
            else
                return;
            using (FileStream fsRead = new FileStream(StrFilePath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                byte[] buffer = new byte[1024 * 1024 * 5];
                int r = fsRead.Read(buffer, 0, buffer.Length);
                TextArea.Text = Encoding.Default.GetString(buffer, 0, r);
            }
        }

        private void btn_SaveScript_Click(object sender, RoutedEventArgs e)
        {
            if (StrFilePath == null || StrFilePath.Trim() == "")
                OnClickSaveFile();
            else
            {
                using (FileStream fsWrite = new FileStream(StrFilePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = Encoding.Default.GetBytes(TextArea.Text);
                    fsWrite.Write(buffer, 0, buffer.Length);
                    System.Windows.MessageBox.Show(string.Format("Save file {0} success",StrFilePath));
                    bNeedSaved = false;
                }
            }
        }

        private void btn_SaveAsScript_Click(object sender, RoutedEventArgs e)
        {
            OnClickSaveFile();
        }

        private bool OnClickSaveFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "";
            sfd.InitialDirectory = @"C:\";
            sfd.Filter = "文本文件| *.fc";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string path = sfd.FileName;
                if (path == "")
                    return false;
                else
                    StrFilePath = path;
            }
            else
                return false;
            using (FileStream fsWrite = new FileStream(StrFilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                byte[] buffer = Encoding.Default.GetBytes(TextArea.Text);
                fsWrite.Write(buffer, 0, buffer.Length);
                System.Windows.MessageBox.Show(string.Format("Save file {0} success", StrFilePath));
                bNeedSaved = false;
                return true;
            }
        }
        public string StrFilePath       //for Binding the panel title
        {
            set { SetValue(FilePathDependency, value); }
            get { return GetValue(FilePathDependency).ToString(); }
        }
        public static DependencyProperty FilePathDependency = DependencyProperty.Register("StrFilePath", typeof(string), typeof(UC_ScriptEditer));
    }
    public class Node
    {
        public string Name { set; get; }
        public string ToolTip { set; get; }
        public List<Node> NodeList { set; get; }
    }
   

}
