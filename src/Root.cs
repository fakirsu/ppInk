using gInk.Apng;
using Microsoft.Ink;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gInk
{
    public static class Global
    {
        public static string ProgramFolder = "";
    }

    //public class Tools
    //{
    //    public const int Invalid = -1;
    //    public const int Hand = 0; public const int Line = 1; public const int Rect = 2; public const int Oval = 3;
    //    public const int StartArrow = 4; public const int EndArrow = 5; public const int NumberTag = 6;
    //    public const int Edit = 7; public const int txtLeftAligned = 8; public const int txtRightAligned = 9;
    //    public const int Move = 10; public const int Copy = 11; public const int Scale = 12; public const int Rotate = 13;
    //    public const int Poly = 21; public const int ClipArt = 22; public const int PatternLine = 23;
    //    public static readonly int[] All = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 21, 22, 23 };
    //    public static readonly string[] Names = { "Hand", "Line", "Rect", "Oval", "StartArrow", "EndArrow", "Numbering", "Edit", "Text Left Aligned", "Text Right Aligned",
    //                                              "Move", "Copy", "Resize", "Rotate", "PolyLine", "ClipArt", "PatternOnStroke"};
    //}

    public class Tools
    {
        public const int Invalid = -1;
        public const int Hand = 0; public const int Line = 1; public const int Rect = 2; public const int Oval = 3;
        public const int StartArrow = 4; public const int EndArrow = 5; public const int NumberTag = 6;
        public const int Edit = 7; public const int txtLeftAligned = 8; public const int txtRightAligned = 9;
        public const int Move = 10; public const int Copy = 11; public const int Scale = 12; public const int Rotate = 13;
        // Nouveaux outils : main remplie (blanc / noir)
        public const int HandFilledWhite = 14;
        public const int HandFilledBlack = 15;
        public const int Poly = 21; public const int ClipArt = 22; public const int PatternLine = 23;
        public const int LetterTag = 24;     // choisir valeurs libres
        public const int SquareTag = 25;
        public const int TriangleTag = 26;
        public const int CircleTag = 27;
        public const int CrossTag = 28;
        // Optionnel : étendre les tableaux Names / All si nécessaires à d’autres fonctionnalités
        public static readonly int[] All = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 21, 22, 23, 24, 25, 26, 27, 28}; 
        public static readonly string[] Names = { "Hand", "Line", "Rect", "Oval", "StartArrow", "EndArrow", "Numbering", "Edit", "Text Left Aligned", "Text Right Aligned",
                                                  "Move", "Copy", "Resize", "Rotate", "Hand Filled White", "Hand Filled Black", "PolyLine", "ClipArt", "PatternOnStroke", "Letter", "Square", "Triangle", "Circle", "Cross"};
    }

    public class Filling {
        public const int NoFrame = -1;      // for Stamps
        public const int Empty = 0;
        public const int PenColorFilled = 1;
        public const int Outside = 2;
        public const int WhiteFilled = 3;
        public const int BlackFilled = 4;
        public const int Modulo = 5;
        public const int Invalid = Modulo + 1;
        public static readonly string[] Names = { "NoFrames","Empty", "Pen Colored", "Outside", "White Colored", "Black Colored" };  //starting at -1
    } // applicable to Hand,Rect,Oval

    public class Orientation{
        public const int min = 0;
        public const int toLeft = 0;    // original
        public const int toRight = 1;
        public const int Horizontal = 1;
        public const int Vertical = 2;
        public const int toUp = 2;
        public const int toDown = 3;
        public const int max = 3;
    }

    public class ClipArtData
    {
        public string ImageStamp;
        public int X=-1;
        public int Y=-1;
        public int Wstored=-1;
        public int Hstored=-1;
        public int Filling=gInk.Filling.NoFrame;
        public bool PatternLine=false;
        public double Distance=double.MaxValue;
        public bool Store = false;
        public ClipArtData Clone() { return new ClipArtData() { ImageStamp = ImageStamp, X = X, Y = Y, Wstored = Wstored, Hstored = Hstored, Filling = Filling, PatternLine = PatternLine, Distance = Distance, Store = Store }; }
    };

    public enum VideoRecordMode {NoVideo=0 , OBSRec=1 , OBSBcst=2 , FfmpegRec=3 };
    public enum VideoRecInProgress { Dead = -2, Unknown = -1, Stopped=0, Starting=1, Recording=2, Stopping = 3, Pausing=4, Paused=5, Resuming=6, Streaming = 7 };

    public enum SnapInPointerKeys { None=0, Shift=1, Control=2, Alt=3 };

    public class AnimationStructure
    {
        public ApngImage Image;
        public long Idx;
        public DateTime T0;
        public bool DeleteAtDend;
        public bool DeleteRequested;
        public DateTime TEnd;
        public int Loop;
    }



    public class TestMessageFilter : IMessageFilter
	{
		public Root Root;

		public TestMessageFilter(Root root)
		{
			Root = root;
		}

		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg == 0x0312 || m.Msg == Program.StartInkingMsg)                    // 0x0312 is the global hotkey WM_HOTKEY
            {
                //Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                //int modifier = (int)m.LParam & 0xFFFF;       // The modifier of the hotkey that was pressed.
                int id = (m.Msg == 0x0312)?m.WParam.ToInt32():0;                                        // The id of the hotkey that was pressed.
                if(id ==1)      // Hotkey_CreateIndex;
                {
                    Root?.FormCollection?.AddM3UEntry(null);
                    return true;
                }
                else if (id == 2) // OpenToolbar only
                                    {
                                        if (!(Root.FormCollection?.Visible == true || Root.FormDisplay?.Visible == true))
                        Root.StartInk();
                                        return true;
                                    }
                                else if (id == 3) // CloseToolbar only
                                    {
                                        if (Root.FormCollection?.Visible == true || Root.FormDisplay?.Visible == true)
                        Root.StopInk();
                                        return true;
                                    }


                else
                {
                    bool activePointer = (m.Msg == 0x0312 && (Root.FormCollection != null && Root.FormCollection.Visible));
                    Root.callshortcut();
                    if (activePointer)           // StartInkingMsg is received twice, therefore we have to froce pointerMode at that time...
                    {
                        Root.FormCollection.btPointer_Click(null, null);
                        if (Root.AltTabPointer && !Root.PointerMode && !Root.IsDockedBeforePen && !Root.FormCollection.Initializing) // to unfold the bar if AltTabPointer option has been set
                        {
                            Root.UnDock();
                        }

                    }
                    return true;
                }
            }
			return false;
		}
	}

    public static class globalRoot {
        public static bool HideInAltTab=true;
    };

    public class Root
    {
        public Local Local = new Local();
        public const int MaxPenCount = 20;
        public const int MaxDisplayedPens = 10;
        public const int SavedPenDA = MaxPenCount;
        public const int LassoPercent = 80;

        // Déclarations (dans la classe Root – section autres paramètres)
        public int GoFillOpacityPercent = 50;      // 0-100 (% visible) => Transparency = 255 - %
        public int GoStrokeOpacityPercent = 50;     // 0-100 (% visible)
        public float GoStrokeWidth = 3.0f;         // largeur finale (HiMetric)
                                                   // 0 = fin (/12), 1 = moyen (/9), 2 = épais (/6)
        public int GoStrokeThickness = 1;

        // Paramètres flèches (Jeu de go)
        public int GoArrowColorArgb;          // Couleur des flèches (ARGB)
        public int GoArrowThickness;          // 0=fin(400) 1=moyen(1000) 2=épais(2000)
        public int GoArrowLength;             // 0=courte(1/10) 1=moyenne(1/5) 2=longue(1/3)

        // Helpers d’accès
        public Color GetArrowColor() => Color.FromArgb(GoArrowColorArgb);
        public float GetArrowWidthHiMetric()
        {
            switch (GoArrowThickness)
            {
                case 0: return 400f;
                case 1: return 1000f;
                case 2: return 2000f;
                default: return 1000f;
            }
        }
        public int GetFixedArrowLengthPx()
        {
            // Si pas de grille définie, fallback sur largeur d’écran
            int gridWidth = GridRectDefined ? GridRect.Width : Screen.PrimaryScreen.Bounds.Width;
            double ratio = GoArrowLength == 0 ? 0.10 : (GoArrowLength == 1 ? 0.20 : 1.0 / 3.0);
            return (int)Math.Round(gridWidth * ratio);
        }






        // Couleurs spécifiques aux 5 tags (A,R,G,B) ; ordre : Letter, Square, Triangle, Circle, Cross
        // Format identique à Toolbar_Color (A,R,G,B)
        public int[] GoTool_Letter_Color = new int[] { 255, 0, 0, 0 };   // défaut : noir opaque
        public int[] GoTool_Square_Color = new int[] { 255, 0, 0, 0 };
        public int[] GoTool_Triangle_Color = new int[] { 255, 0, 0, 0 };
        public int[] GoTool_Circle_Color = new int[] { 255, 0, 0, 0 };
        public int[] GoTool_Cross_Color = new int[] { 255, 0, 0, 0 };

        public int[] GoTool_Text_Color = new int[4] { 0, 0, 0, 255 }; // Format RGBA (par défaut noir opaque)

        public Hotkey Hotkey_HandFilledWhite = new Hotkey();
        public Hotkey Hotkey_HandFilledBlack = new Hotkey();


        public static readonly Guid TEXTCOLOR_GUID = new Guid("2b2e0a64-2b15-46f0-8d2f-5a7a7b59b2a4");
        // 0 = non défini, sinon ARGB de la couleur active pour les prochains textes
        public int ActiveTextColorARGB = 0;
        // Indique un alignement forcé à la prochaine création de texte (consommé puis remis à null)
        public StringAlignment? ForcedTextAlign = null;

        //public Guid TYPE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 0);
        public static readonly Guid TEXT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 1);
        public static readonly Guid TEXTX_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 2);
        public static readonly Guid TEXTY_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 3);
        public static readonly Guid TEXTHALIGN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 4);
        public static readonly Guid TEXTVALIGN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 5);
        public static readonly Guid TEXTFONT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 6);
        public static readonly Guid TEXTFONTSIZE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 7);
        public static readonly Guid TEXTFONTSTYLE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 8);
        public static readonly Guid TEXTWIDTH_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 9);
        public static readonly Guid TEXTHEIGHT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 10);

        public static readonly Guid ISDELETION_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 0);
        public static readonly Guid ISSTROKE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 1);
        public static readonly Guid ISTAG_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 2);
        public static readonly Guid ISLASSO_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 3);
        //not yet used : 
        //public Guid ISRECT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 2);
        //public Guid ISOVAL_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 3);

        public static readonly Guid ISFILLEDCOLOR_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 12, 1);
        public static readonly Guid ISFILLEDWHITE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 12, 2);
        public static readonly Guid ISFILLEDBLACK_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 12, 3);
        public static readonly Guid ISFILLEDOUTSIDE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 12, 4);
        public static readonly Guid IMAGE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 4);
        public static readonly Guid IMAGE_X_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 5);
        public static readonly Guid IMAGE_Y_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 6);
        public static readonly Guid IMAGE_W_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 7);
        public static readonly Guid IMAGE_H_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 8);
        public static readonly Guid ISHIDDEN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 10);
        public static readonly Guid ISBACKGROUND_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 11);
        public static readonly Guid ANIMATIONFRAMEIMG_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 12);
        public static readonly Guid IMAGE_ON_LINE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 19);
        public static readonly Guid REPETITIONDISTANCE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 20);
        public static readonly Guid LISTOFPOINTS_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 21);
        public static readonly Guid ARROWSTART_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 22);    // pointing head
        public static readonly Guid ARROWSTART_X_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 23);    // pointing head
        public static readonly Guid ARROWSTART_Y_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 24);    // pointing head
        public static readonly Guid ARROWSTART_FN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 25);    // Original FileName
        public static readonly Guid ARROWEND_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 32);      // tail
        public static readonly Guid ARROWEND_X_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 33);      // tail
        public static readonly Guid ARROWEND_Y_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 34);      // tail
        public static readonly Guid ARROWEND_FN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 35);    // 

        public static readonly Guid FADING_PEN = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 3, 1);
        public static readonly Guid DASHED_LINE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 3, 2);        // will contain DashStyle style
        public static readonly Guid ROTATION_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 3, 3);           // applies to both Text and Images

        public static int MIN_MAGNETIC = 25;
        // options
        public bool ReinitForms = false;
        public int ToolbarOrientation = Orientation.toLeft;
        public bool[] PenEnabled = new bool[MaxPenCount];
        public bool PensOnTwoLines = true;
        public bool PensExtraSet = true;
        public bool ToolsEnabled = true;
        public bool EraserEnabled = true;
        public bool PointerEnabled = true;
        public bool AltTabPointer = false;
        public bool PenWidthEnabled = false;
        public bool WidthAtPenSel = true;
        public bool SnapEnabled = true;
        public bool UndoEnabled = true;
        public bool ClearEnabled = true;
        public bool PagesEnabled = false;
        public bool LoadSaveEnabled = true;
        public bool PanEnabled = true;
        public bool InkVisibleEnabled = true;
        public DrawingAttributes[] PenAttr = new DrawingAttributes[MaxPenCount+1]; //+1 for SavedPenDA
        public bool AutoScroll;
        public bool WhiteTrayIcon;
        public string SnapshotBasePath;
        public string SaveStrokesPath;
        public bool AutoSaveStrokesAtExit;
        public bool SwapSnapsBehaviors=false;
        public int CanvasCursor = 0;
        public bool AllowDraggingToolbar = true;
        public bool AllowHotkeyInPointerMode = true;
        public int gpButtonsLeft, gpButtonsTop;

        // advanced options
        public string CloseOnSnap = "blankonly";
        public bool AlwaysHideToolbar = false;
        public bool KeepDockedAtOpen = false;
        public bool KeepUnDockedAtPointer = false;
        public float ToolbarHeight = 0.06f;
        public int AltAsOneCommand = 2;

        public Color WindowModeBorderUnselected = Color.DarkGray;
        public Color WindowModeBorderSelected = Color.Black;

        public string cursorarrowFileName = "cursorarrow";
        public string cursortargetFileName = "cursortarget";
        public string cursoreraserFileName = "cursoreraser";
        public string cursorsnapFileName = "cursorsnap";
        public string cursorredFileName = "cursorred";

        public int CursorX, CursorY;
        public int CursorX0 = int.MinValue, CursorY0 = int.MinValue;

        public double LongClickTime = 1.0;

        // the two grays for "white board" effect
        public int[] Gray1 = new int[] { 80, 150, 150, 150 };
        public int[] Gray2 = new int[] {100, 100, 100, 100};
        public int[] ToolbarBGColor = new int[] { 245, 245, 245, 0 };
        public int BoardAtOpening = 0;      // 0:Transparent/1:White/2:Customed/3:Black/4:AtSelection
        public int BoardSelected = 0;       // by default transparent

        //measurement tools
        public static bool MeasureEnabled = true;
        public static double Measure2Scale = 1.0;
        public static int Measure2Digits=1;
        public static string Measure2Unit = "Pixel";
        public static bool MeasureAnglCounterClockwise = true;
        public static bool MeasureWhileDrawing = false;
        public static bool Measure_Save_Scale = true;


        // hotkey options
        public Hotkey Hotkey_Global = new Hotkey();
		public Hotkey[] Hotkey_Pens = new Hotkey[MaxDisplayedPens];
        public Hotkey Hotkey_FadingToggle = new Hotkey();

        public Hotkey Hotkey_PenWidthPlus = new Hotkey();
        public Hotkey Hotkey_PenWidthMinus = new Hotkey();

        public Hotkey Hotkey_Eraser = new Hotkey();
		public Hotkey Hotkey_InkVisible = new Hotkey();
		public Hotkey Hotkey_Pointer = new Hotkey();
		public Hotkey Hotkey_Pan = new Hotkey();
		public Hotkey Hotkey_Undo = new Hotkey();
		public Hotkey Hotkey_Redo = new Hotkey();
		public Hotkey Hotkey_Snap = new Hotkey();
		public Hotkey Hotkey_Clear = new Hotkey();
        public Hotkey Hotkey_Video = new Hotkey();
        public Hotkey Hotkey_DockUndock = new Hotkey();
        public Hotkey Hotkey_Close = new Hotkey();
        public Hotkey Hotkey_SnapClose = new Hotkey(); // to keep Esc to close in snapping;

        public Hotkey Hotkey_Hand = new Hotkey();
        public Hotkey Hotkey_Line = new Hotkey();
        public Hotkey Hotkey_Rect = new Hotkey();
        public Hotkey Hotkey_Oval = new Hotkey();
        public Hotkey Hotkey_Arrow = new Hotkey();
        public Hotkey Hotkey_Numb = new Hotkey();
        // --- NEW : hotkeys pour variantes NumberTag (pastilles) ---
        public Hotkey Hotkey_NTag_ShowWhite = new Hotkey();
        public Hotkey Hotkey_NTag_ShowBlack = new Hotkey();
        public Hotkey Hotkey_NTag_HideWhite = new Hotkey();
        public Hotkey Hotkey_NTag_HideBlack = new Hotkey();

        public Hotkey Hotkey_LetterTag = new Hotkey();
        public Hotkey Hotkey_SquareTag = new Hotkey();
        public Hotkey Hotkey_TriangleTag = new Hotkey();
        public Hotkey Hotkey_CircleTag = new Hotkey();
        public Hotkey Hotkey_CrossTag = new Hotkey();


        public Hotkey Hotkey_Text = new Hotkey();
        public Hotkey Hotkey_Edit = new Hotkey();
        public Hotkey Hotkey_Move = new Hotkey();
        public Hotkey Hotkey_ScaleRotate = new Hotkey();
        public Hotkey Hotkey_Magnet = new Hotkey();
        public Hotkey Hotkey_ClipArt = new Hotkey();
        public Hotkey Hotkey_ClipArt1 = new Hotkey();
        public Hotkey Hotkey_ClipArt2 = new Hotkey();
        public Hotkey Hotkey_ClipArt3 = new Hotkey();
        public Hotkey Hotkey_Zoom = new Hotkey();
        public Hotkey Hotkey_ColorPickup = new Hotkey();
        public Hotkey Hotkey_ColorEdit = new Hotkey();
        public Hotkey Hotkey_LineStyle = new Hotkey();
        public Hotkey Hotkey_Lasso = new Hotkey();
        public UInt32 LineStyleRotateEnabled= 0xFF;  // field of bits

        public Hotkey Hotkey_PagePrev = new Hotkey();
        public Hotkey Hotkey_PageNext = new Hotkey();
        public Hotkey Hotkey_LoadStrokes = new Hotkey();
        public Hotkey Hotkey_SaveStrokes = new Hotkey();

        public float LongHKPressDelay = 2.5F;
        public bool AltTabStart = false;

        public bool ButtonClick_For_LineStyle = false;

        public bool DirectX = true;

        public int ToolSelected = Tools.Hand;        // indicates which tool (Hand,Line,...) is currently selected
        public int FilledSelected = 0;      // indicates which filling (None, Selected color, ...) is currently select
        public bool EraserMode = false;
		public bool Docked = false;
		public bool PointerMode = false;
        public bool IsDockedBeforePen = false;
        public DateTime PointerChangeDate = DateTime.MinValue;

        public bool FingerInAction = false;  // true when mouse down, either drawing or snapping or whatever
		public int Snapping = 0;  // <=0: not snapping, 1: waiting finger, 2:dragging
		public int SnappingX = -1, SnappingY = -1;
		public Rectangle SnappingRect;
		public int UponButtonsUpdate = 0;
		public bool UponTakingSnap = false;
		public bool UponBalloonSnap = false;
		public bool UponSubPanelUpdate = false;
		public bool UponAllDrawingUpdate = false;
		public bool MouseMovedUnderSnapshotDragging = false; // used to pause re-drawing when mouse is not moving during dragging to take a screenshot
        public int PenWidth_Delta = 5;

        public bool LassoMode = false;
        public bool PanMode = false;
		public bool InkVisible = true;
        public int MagneticRadius= MIN_MAGNETIC;        // Magnet Radius; <=0 means off;
        public int MinMagneticRadius() { return Math.Max(Math.Abs(MagneticRadius), MIN_MAGNETIC); }
        public float MagneticAngle = 15.0F;
        public float MagneticAngleTolRatio  = 0.2F;
        public float MagneticAngleTolerance = 15.0F *.2F;

        public Stroke StrokeHovered;            // contains the "selection" for edit/move/copy/erase else is null
        public Pen SelectionFramePen = new Pen(Color.Red, 1);

        public bool SubToolsEnabled = true;

        public bool DefaultArrow_start = true;

        public Ink[] UndoStrokes;
		//public Ink UponUndoStrokes;
		public int UndoP;
		public int UndoDepth, RedoDepth;

        public int CurrentArrow = 0;
        public List<string> ArrowHead = new List<string>();
        public List<string> ArrowTail = new List<string>();

        public NotifyIcon trayIcon;
		public ContextMenu trayMenu;
		public FormCollection FormCollection;
		public FormDisplay FormDisplay;
		public FormButtonHitter FormButtonHitter;
		public FormOptions FormOptions;

		public int CurrentPen = 1;  // defaut pen
		public int LastPen = 1;
		public float GlobalPenWidth = 80;

        // those are constants initialised from ini files
        public float PenWidthThin = 30;
        public float PenWidthNormal = 80;
        public float PenWidthThick = 500;

        public bool FitToCurve = true;
		public bool gpPenWidthVisible = false;
		public string SnapshotFileFullPath = ""; // used to record the last snapshot file name, to select it when the balloon is clicked
        public string SnapshotFileTemplate = "$YYYY$-$MM$-$DD$ $H$-$M$-$S$.png";
        public bool SnapIgnoreBackgroundStroke = true; // for the moment not customizable by user

        public int FormTop = 100, FormLeft = 100, FormWidth = 48, FormOpacity = -50; // negative opacity means that the window is not displayed
        public CallForm callForm = null;
        public bool OpenIntoSnapMode = false;

        public double ArrowAngle = 15 * Math.PI /180;   // 15°
        public double ArrowLen = 0.0185 * System.Windows.SystemParameters.PrimaryScreenWidth; // == 1.85% of screen width

        public int TagNumbering = 1;
        public string TagFormatting = "{0}";
        public string[] TagFormattingList = "{0};{1};{2}".Split(';');
        public int TagSize = 25;
        public string TagFont = "";
        public bool TagItalic = false;
        public bool TagBold = false;
        public int TextSize = 25;
        public string TextFont = "Arial";
        public bool TextItalic = false;
        public bool TextBold = false;

        public int TextBackground = 0;

        public VideoRecordMode VideoRecordMode = VideoRecordMode.NoVideo;
        public string ObsUrl = "ws://localhost:4444";
        public string ObsPwd = "obs";
        public string FFMpegFileName = "CAPT_%DD%%MM%%YY%_%H%%M%%S%_$nn$.mkv";
        public string FFMpegCmd = "ffmpeg.exe -f gdigrab  -framerate 15 -offset_x $xx$ -offset_y $yy$ -video_size $ww$x$hh$ -show_region 1 -i desktop -vcodec libx264 %USERPROFILE%/$FN$";

        public bool CreateM3U = true;
        public Hotkey Hotkey_CreateIndex = new Hotkey();
        // Nouveaux hotkeys : ouvrir / fermer la barre (persistants)
        public Hotkey Hotkey_OpenToolbar = new Hotkey();
        public Hotkey Hotkey_CloseToolbar = new Hotkey();

        public bool CreateIndexOnUndock = false;
        public string IndexDefaultText = "%H%:%M%:%S% = ";
        public bool UndockOnIndexCreate = false;
        public bool NoEditM3UEntry = false;

        //public string VideoOutFileName = "";

        public string CurrentVideoFileName= "";
        public string CurrentIndexFileName = "";
        public DateTime CurrentVideoStartTime;
        public int VideoRecordCounter = 0;
        public int IndexRecordCounter = 0;
        public VideoRecInProgress VideoRecInProgress = VideoRecInProgress.Stopped;
        public Process FFmpegProcess = null;
        public bool VideoRecordWindowInProgress = false;
        public ClientWebSocket ObsWs;
        public Task ObsRecvTask;
        public CancellationTokenSource ObsCancel = new CancellationTokenSource();

        public int StampSize = 128;
        public StringCollection StampFileNames = new StringCollection();
        public ClipArtData ImageStamp = new ClipArtData { ImageStamp = "", X = -1, Y = -1, Wstored=-1, Hstored=-1, Filling = (int)(Filling.NoFrame),PatternLine=false,Distance = -1, Store=false};
        public float StampScaleRatio = .1F;
        public int ImageStampFilling = 0;
        public ClipArtData  ImageStamp1 = new ClipArtData { ImageStamp = "", X = -1, Y = -1, Wstored = -1, Hstored = -1, Filling = (int)(Filling.NoFrame), PatternLine = false, Distance = -1, Store = false };
        public ClipArtData  ImageStamp2 = new ClipArtData { ImageStamp = "", X = -1, Y = -1, Wstored = -1, Hstored = -1, Filling = (int)(Filling.NoFrame), PatternLine = false, Distance = -1, Store = false };
        public ClipArtData  ImageStamp3 = new ClipArtData { ImageStamp = "", X = -1, Y = -1, Wstored = -1, Hstored = -1, Filling = (int)(Filling.NoFrame), PatternLine = false, Distance = -1, Store = false };

        public float TimeBeforeFading = 5.0F;     //5s default
        public byte DecreaseFading = 10;     //10 (/255)  ; must be a positive value

        public int ZoomWidth = 100;
        public int ZoomHeight = 100;
        public float ZoomScale = 3.0F;
        public bool ZoomContinous = false;
        public int ZoomEnabled = 3;

        public Color SpotLightColor = Color.FromArgb(128, Color.Orange);
        public int SpotLightRadius = 200;
        public bool SpotOnAlt = true;

        public Rectangle WindowRect = new Rectangle(Int32.MinValue, Int32.MinValue, -1, -1);


        /// ################ goInk specific options - START ####################

        // stockage et helpers pour la grille définie par l'utilisateur
        public Rectangle GridRect = Rectangle.Empty;
        public bool GridRectDefined = false;

        // Grille : nombre de lignes (vertical = horizontal). Valeurs possibles : 19,13,9
        public int GridRows = 19;
        public int GridCols = 19;

        // Pourcentages (100.0 = valeur actuelle du code). Persistés en fichier config.
        // TagSizePercent : multiplicateur sur la taille du texte du numéro (en %)
        // TagCirclePercent : multiplicateur sur le diamètre de la pastille (en %)
        // TagOpacityPercent : opacité des pierres et du texte (100 = opaque, 0 = transparent)
        public double TagSizePercent = 100.0;
        public double TagCirclePercent = 100.0;
        public double TagStoneOpacityPercent = 100.0;
        public double TagNumberOpacityPercent = 100.0;

        /// ################ goInk specific options - END ################



        public bool EraseOnLoosingFocus = false;
        public bool ResizeDrawingWindow = false;

        public SnapInPointerKeys SnapInPointerHoldKey = SnapInPointerKeys.Shift;
        public SnapInPointerKeys SnapInPointerPressTwiceKey = SnapInPointerKeys.Control;
        
        public bool InverseMousewheel=false;

        public string APIRestUrl="";
        public APIRest APIRest;
        public bool APIRestCloseOnSnap = false;
        public bool APIRestAltPressed = false;

        public bool StrokesOnlySnapshot=true;
        //public string ProgramFolder;

        public bool ColorPickerEnabled = true;
        public bool ColorPickerMode = false;
        public Color PickupColor;
        public byte PickupTransparency;
        

        public string ExpandVarCmd(string cmd, int x, int y, int w, int h)
        {
            cmd = Environment.ExpandEnvironmentVariables(cmd);
            cmd = cmd.Replace("$FN$", CurrentVideoFileName);
            cmd = cmd.Replace("$xx$", x.ToString());
            cmd = cmd.Replace("$yy$", y.ToString());
            cmd = cmd.Replace("$ww$", w.ToString());
            cmd = cmd.Replace("$hh$", h.ToString());
            cmd = cmd.Replace("$nn$", VideoRecordCounter.ToString());
            cmd = cmd.Replace("$NN$", VideoRecordCounter.ToString("000"));
            cmd = cmd.Replace("$ii$", IndexRecordCounter.ToString());
            cmd = cmd.Replace("$II$", IndexRecordCounter.ToString("000"));
            DateTime dt = DateTime.Now;
            cmd = cmd.Replace("$H$", dt.Hour.ToString("00"));
            cmd = cmd.Replace("$M$", dt.Minute.ToString("00"));
            cmd = cmd.Replace("$S$", dt.Second.ToString("00"));
            cmd = cmd.Replace("$DD$", dt.Day.ToString("00"));
            cmd = cmd.Replace("$MM$", dt.Month.ToString("00"));
            cmd = cmd.Replace("$YY$", (dt.Year % 100).ToString("00"));
            cmd = cmd.Replace("$YYYY$", dt.Year.ToString("00"));
            return cmd;
        }

        public bool IsVideoRecordingSelected()
        {
            return VideoRecordMode == VideoRecordMode.FfmpegRec || VideoRecordMode == VideoRecordMode.OBSRec;
        }


        public Root()
		{
            /*Global.ProgramFolder = Path.GetDirectoryName(Path.GetFullPath(Environment.GetCommandLineArgs()[0])).Replace('\\','/');
            if (Global.ProgramFolder[Global.ProgramFolder.Length - 1] != '/')
                Global.ProgramFolder += '/';
            */
            SelectionFramePen.DashPattern = new float[]{4,4};       // dashed red line for selection drawing
			for (int p = 0; p < MaxDisplayedPens; p++)
				Hotkey_Pens[p] = new Hotkey();

			trayMenu = new ContextMenu();
			trayMenu.MenuItems.Add(Local.MenuEntryAbout + "...", OnAbout);
			trayMenu.MenuItems.Add(Local.MenuEntryOptions + "...", OnOptions);
			trayMenu.MenuItems.Add("-");
			trayMenu.MenuItems.Add(Local.MenuEntryExit, OnExit);

            SetDefaultPens();
			SetDefaultConfig();
            ReadOptions(Program.RunningFolder + "defaults.ini");
            ReadOptions(Program.RunningFolder + "config.ini");
            ReadOptions(Program.RunningFolder + "pensdef.ini");
            ReadOptions(Program.RunningFolder + "pens.ini");
			ReadOptions(Program.RunningFolder + "hotkeys.ini");
            Hotkey_SnapClose.Parse("Escape");

            if (TagFont=="")     // if no options, we apply text parameters
            {
                TagFont = TextFont;
                TagBold = TextBold;
                TagItalic = TextItalic;
                TagSize = TextSize;
            }
            
			//FormCollection = null;
			//FormDisplay = null;
            FormCollection = new FormCollection(this);
            FormButtonHitter = new FormButtonHitter(this);
            FormDisplay = new FormDisplay(this);  // FormDisplay is created at the end to ensure other objects are created.
            UndoStrokes = new Ink[8];

            APIRest = new APIRest(this);

            Size size = SystemInformation.SmallIconSize;
			trayIcon = new NotifyIcon();
			trayIcon.Text = "ppInk";
			trayIcon.ContextMenu = trayMenu;
			trayIcon.Visible = true;
			trayIcon.MouseClick += TrayIcon_Click;
			trayIcon.BalloonTipText = Local.NotificationSnapshot;
			trayIcon.BalloonTipClicked += TrayIcon_BalloonTipClicked;
			SetTrayIconColor();

			SetHotkey();

			TestMessageFilter mf = new TestMessageFilter(this);
			Application.AddMessageFilter(mf);
        }

        public void callshortcut()
        {
            TagNumbering = 1; //reset tag counter 
            if ((FormCollection == null || !FormCollection.Visible) && (FormDisplay == null || !FormDisplay.Visible))
            {
                if (FormOpacity > 0) callForm?.Hide();
                //if (FormOpacity > 0) callForm.Close();
                StartInk();
                if (OpenIntoSnapMode)
                    FormCollection.btSnap_Click(null,null);
            }
            /*
            else if (PointerMode)
            {
                //Root.UnPointer();
                SelectPen(LastPen);
            }
            else
            {
                //Root.Pointer();
                SelectPen(-2);
            }*/

        }

        private void TrayIcon_BalloonTipClicked(object sender, EventArgs e)
		{
            //string snapbasepath = SnapshotBasePath;
            //snapbasepath = Environment.ExpandEnvironmentVariables(snapbasepath);
            //System.Diagnostics.Process.Start(snapbasepath);            
            string fullpath;
            try
            {
                fullpath = System.IO.Path.GetFullPath(SnapshotFileFullPath);
            }
            catch
            {
                fullpath = ExpandVarCmd(SnapshotBasePath,0,0,0,0);
            }
			System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", fullpath));
		}

		private void TrayIcon_Click(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{

                if ((FormDisplay == null || !FormDisplay.Visible) && (FormCollection == null || !FormCollection.Visible))
				{
                    callshortcut();
                    //ReadOptions("pens.ini");
                    //ReadOptions("config.ini");
                    //ReadOptions("hotkeys.ini");
                    //StartInk();
				}
				else if (Docked)
					UnDock();
			}
		}

		public void StartInk()
		{
            
            if (FormCollection == null)
                FormCollection = new FormCollection(this);
            if (FormButtonHitter == null)
                FormButtonHitter = new FormButtonHitter(this);
            if (FormDisplay == null)
                FormDisplay = new FormDisplay(this);  // FormDisplay is created at the end to ensure other objects are created.
            if (UndoStrokes == null)
                UndoStrokes = new Ink[8];           
            if (FormDisplay.Visible|| FormCollection.Visible)
				return;
            FormCollection.Initialize();
            FormButtonHitter.Initialize();
            FormDisplay.Initialize();

            Docked = KeepDockedAtOpen;//false;
            PointerMode = false; // we have to reset pointer mode when starting drawing;
            ResizeDrawingWindow = false;
            UponTakingSnap = false;

			if (CurrentPen < 0)
				CurrentPen = 0;
			if (!PenEnabled[CurrentPen])
			{
				CurrentPen = 0;
				while (CurrentPen < MaxPenCount && !PenEnabled[CurrentPen])
					CurrentPen++;
				if (CurrentPen == MaxPenCount)
					CurrentPen = -2;
			}
			SelectPen(CurrentPen);
			SetInkVisible(true);
			FormCollection.ButtonsEntering = 1;
			FormDisplay.Show();
			FormCollection.Show();
			FormDisplay.DrawButtons(true);

            for (int i = 0; i < UndoStrokes.Length; i++)
                UndoStrokes[i] = null;
            UndoStrokes[0] = FormCollection.IC.Ink.Clone();
            UndoDepth = 0;
            UndoP = 0;

            SetForegroundWindow(FormCollection.Handle);
        }
        public void StopInk()
		{
            FormCollection.Initializing = true;
            try { FormCollection.Hide(); FormCollection.tiSlide.Enabled = false; } catch { }
            try { FormDisplay.Hide(); FormDisplay.timer1.Enabled = false; } catch { }
			try { FormButtonHitter.Hide(); FormButtonHitter.timer1.Enabled = false; } catch { }


            if (ReinitForms)
            {
                FormCollection = null;
            }
            //  The FormCollection is destroyed, therefore all following calls to the form and its controls will not hit
            try
            {                
                ObsRecvTask?.Dispose();
            }
            catch { }
            finally
            {
                ObsRecvTask = null;
            }
            try
            {
                ObsWs?.Dispose();
            }
            catch { }
            finally
            {
                ObsWs = null;
            }
            try
            {
                ObsCancel.Cancel();
                ObsCancel.Dispose();
            }
            catch { }
            finally
            {
                ObsCancel = new CancellationTokenSource();
            }

            //FormDisplay = null;
            //FormButtonHitter = null;

            if (UponBalloonSnap)
			{
				ShowBalloonSnapshot();
				UponBalloonSnap = false;
			}
            //if (FormOpacity > 0) callForm.Show();
            if (FormOpacity > 0 && !ResizeDrawingWindow)
            {
                if (callForm == null)
                    callForm = new CallForm(this);
                callForm.Show();
                callForm.Top = FormTop;
                callForm.Left = FormLeft;
                callForm.Width = FormWidth;
                callForm.Height = FormWidth;
                callForm.Opacity = FormOpacity / 100.0;
            }

            GC.Collect();
        }

        public void ClearInk()
		{
			FormCollection.IC.Ink.DeleteStrokes();
            FormDisplay.ClearCanvus();
            FormDisplay.DrawButtons(true);
            FormDisplay.UpdateFormDisplay(true);
        }

        public void ShowBalloonSnapshot()
		{
			trayIcon.ShowBalloonTip(3000);
		}

		public void UndoInk()
		{
			if (UndoDepth <= 0)
				return;
            if(FormCollection.IC.Ink.Strokes.Count>0 && FormCollection.IC.Ink.Strokes[FormCollection.IC.Ink.Strokes.Count - 1].ExtendedProperties.Contains(ISTAG_GUID))
            {
                TagNumbering--;
            }

			UndoP--;
			if (UndoP < 0)
				UndoP = UndoStrokes.GetLength(0) - 1;
			UndoDepth--;
			RedoDepth++;
			FormCollection.IC.Ink.DeleteStrokes();
			if (UndoStrokes[UndoP].Strokes.Count > 0)
            {
				FormCollection.IC.Ink.AddStrokesAtRectangle(UndoStrokes[UndoP].Strokes, UndoStrokes[UndoP].Strokes.GetBoundingBox());
                if (ToolSelected == Tools.Poly)
                    FormCollection.RestorePolylineData(FormCollection.IC.Ink.Strokes[FormCollection.IC.Ink.Strokes.Count-1]);
            }
			FormDisplay.ClearCanvus();
			FormDisplay.DrawStrokes();
			FormDisplay.DrawButtons(true);
			FormDisplay.UpdateFormDisplay(true);
		}

		public void Pan(int x, int y)
		{
			if (x == 0 && y == 0)
				return;            
            FormCollection.IC.Ink.Strokes.Move(x, y);
            foreach (Stroke st in FormCollection.IC.Ink.Strokes)
                if (st.ExtendedProperties.Contains(ISBACKGROUND_GUID))
                    continue;
                else
                    FormCollection.MoveStrokeAndProperties(st, x, y, false);
            FormDisplay.ClearCanvus();
			FormDisplay.DrawStrokes();
			FormDisplay.DrawButtons(true);
			FormDisplay.UpdateFormDisplay(true);
		}

		public void SetInkVisible(bool visible)
		{
			InkVisible = visible;
			if (visible)
				FormCollection.btInkVisible.BackgroundImage = FormCollection.image_visible;
			else
				FormCollection.btInkVisible.BackgroundImage = FormCollection.image_visible_not;

			FormDisplay.ClearCanvus();
			FormDisplay.DrawStrokes();
			FormDisplay.DrawButtons(true);
			FormDisplay.UpdateFormDisplay(true);
		}

		public void RedoInk()
		{
			if (RedoDepth <= 0)
				return;

			UndoDepth++;
			RedoDepth--;
			UndoP++;
			if (UndoP >= UndoStrokes.GetLength(0))
				UndoP = 0;
			FormCollection.IC.Ink.DeleteStrokes();
			if (UndoStrokes[UndoP].Strokes.Count > 0)
				FormCollection.IC.Ink.AddStrokesAtRectangle(UndoStrokes[UndoP].Strokes, UndoStrokes[UndoP].Strokes.GetBoundingBox());

			FormDisplay.ClearCanvus();
			FormDisplay.DrawStrokes();
			FormDisplay.DrawButtons(true);
			FormDisplay.UpdateFormDisplay(true);
		}

		public void Dock()
		{
            if ((FormDisplay == null || !FormDisplay.Visible) || (FormCollection == null || !FormCollection.Visible)) 
				return;

			Docked = true;
			gpPenWidthVisible = false;
            switch(ToolbarOrientation)
            {
                case Orientation.toLeft: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockback; break;
                case Orientation.toRight: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dock; break;
                case Orientation.toUp: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockbackV ; break;
                case Orientation.toDown: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockV; break;
            }
            FormCollection.ButtonsEntering = -1;
			UponButtonsUpdate |= 0x2;
		}

		public void UnDock()
		{
            if (FormCollection != null && FormCollection.AddM3UEntryInProgress)
                return;

            if ((FormDisplay == null || !FormDisplay.Visible) || (FormCollection == null || !FormCollection.Visible))
				return;

			Docked = false;
            switch (ToolbarOrientation)
            {
                case Orientation.toLeft: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dock; break;
                case Orientation.toRight: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockback; break;
                case Orientation.toUp: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockV; break;
                case Orientation.toDown: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockbackV; break;
            }
			FormCollection.ButtonsEntering = 1;
			UponButtonsUpdate |= 0x2;

            if(CreateIndexOnUndock && CurrentIndexFileName != "")
            {
                FormCollection.AddM3UEntry();
            }
        }

		public void Pointer()
		{
			if (PointerMode == true)
				return;
            if (ColorPickerMode)
                FormCollection.StartStopPickUpColor(0);
            FormCollection.ClipartsDlg.Hide();
            PointerChangeDate = DateTime.Now.AddMilliseconds(100);
            PointerMode = true;
            IsDockedBeforePen = Docked;

            FormDisplay.DrawBorder(false);
			FormCollection.ToThrough();
			FormButtonHitter.Show();
            FormButtonHitter.timer1_Tick(null,null); // Force Size recomputation for alt+tab processing
            if (ColorPickerMode)
                FormCollection.StartStopPickUpColor(0);
            FormCollection.SpotLightTemp = false;
        }

        public void UnPointer()
		{
			if (PointerMode == false)
				return;
            if (FormCollection == null)
                return;

            PointerChangeDate = DateTime.Now.AddMilliseconds(100);
            PointerMode = false;

            FormCollection.AddPointerSnaps("");
			FormButtonHitter.Hide();
			FormCollection.ToUnThrough();
            FormCollection.ToTopMost();
            AppGetFocus();
			FormCollection.Activate();
            FormCollection.Select();                       
		}

		public void SelectPen(int pen)
		{
			FormCollection.SelectPen(pen);
		}

		public void SetDefaultPens()
		{
			PenEnabled[0] = false;
			PenAttr[0] = new DrawingAttributes();
			PenAttr[0].Color = Color.FromArgb(80, 80, 80);
			PenAttr[0].Width = 80;
			PenAttr[0].Transparency = 0;

			PenEnabled[1] = true;
			PenAttr[1] = new DrawingAttributes();
			PenAttr[1].Color = Color.FromArgb(225, 60, 60);
			PenAttr[1].Width = 80;
			PenAttr[1].Transparency = 0;

			PenEnabled[2] = true;
			PenAttr[2] = new DrawingAttributes();
			PenAttr[2].Color = Color.FromArgb(30, 110, 200);
			PenAttr[2].Width = 80;
			PenAttr[2].Transparency = 0;

			PenEnabled[3] = true;
			PenAttr[3] = new DrawingAttributes();
			PenAttr[3].Color = Color.FromArgb(235, 180, 55);
			PenAttr[3].Width = 80;
			PenAttr[3].Transparency = 0;

			PenEnabled[4] = true;
			PenAttr[4] = new DrawingAttributes();
			PenAttr[4].Color = Color.FromArgb(120, 175, 70);
			PenAttr[4].Width = 80;
			PenAttr[4].Transparency = 0;

			PenEnabled[5] = true;
			PenAttr[5] = new DrawingAttributes();
			PenAttr[5].Color = Color.FromArgb(235, 125, 15);
			PenAttr[5].Width = 500;
			PenAttr[5].Transparency = 175;

            PenEnabled[6] = true;
            PenAttr[6] = new DrawingAttributes();
			PenAttr[6].Color = Color.FromArgb(230, 230, 230);
			PenAttr[6].Width = 80;
			PenAttr[6].Transparency = 0;

            PenEnabled[7] = true;
            PenAttr[7] = new DrawingAttributes();
			PenAttr[7].Color = Color.FromArgb(250, 140, 200);
			PenAttr[7].Width = 80;
			PenAttr[7].Transparency = 0;

            PenEnabled[8] = true;
            PenAttr[8] = new DrawingAttributes();
			PenAttr[8].Color = Color.FromArgb(25, 180, 175);
			PenAttr[8].Width = 80;
			PenAttr[8].Transparency = 0;

            PenEnabled[9] = true;
            PenAttr[9] = new DrawingAttributes();
			PenAttr[9].Color = Color.FromArgb(145, 70, 160);
			PenAttr[9].Width = 500;
			PenAttr[9].Transparency = 175;

            for(int i=10;i<=19;i++)
            {
                PenEnabled[i] = PenEnabled[i-10];
                PenAttr[i] = new DrawingAttributes();
                PenAttr[i].Color = PenAttr[i-10].Color;
                PenAttr[i].Width = PenAttr[i-10].Width;
                PenAttr[i].Transparency = PenAttr[i - 10].Transparency;
            }

            PenAttr[SavedPenDA] = null;

        }

		public void SetDefaultConfig()
		{
			Hotkey_Global.Control = true;
			Hotkey_Global.Alt = true;
			Hotkey_Global.Shift = false;
			Hotkey_Global.Win = false;
			Hotkey_Global.Key = 'G';

			AutoScroll = false;
			WhiteTrayIcon = false;
			SnapshotBasePath = "%USERPROFILE%/Pictures/gInk/";
            Environment.SetEnvironmentVariable("PPINK_SNAP_DIR", SnapshotBasePath);
            SaveStrokesPath = SnapshotBasePath;
            AutoSaveStrokesAtExit = true;

            ///
            // Forcer l'état initial des tags numérotés à fond blanc (ou utiliser BlackFilled si tu préfères)
            //TagNumbering = 3; // numéro de départ (optionnel ; choisis la valeur voulue)
            //FilledSelected = Filling.WhiteFilled; // ← important : définit l'état de remplissage initial

            // Flèches (go) - défauts
            GoArrowColorArgb = Color.Red.ToArgb(); // couleur par défaut
            GoArrowThickness = 1;                  // moyen (1000)
            GoArrowLength = 1;                     // moyenne (1/5)

        }

		public void SetTrayIconColor()
		{
			if (WhiteTrayIcon)
			{
				if (File.Exists("icon_white.ico"))
					trayIcon.Icon = new Icon("icon_white.ico");
				else
					trayIcon.Icon = global::gInk.Properties.Resources.icon_white;
			}
			else
			{
				if (File.Exists("icon_red.ico"))
					trayIcon.Icon = new Icon("icon_red.ico");
				else
					trayIcon.Icon = global::gInk.Properties.Resources.icon_red;
			}
		}

        private int GetPenNumber(string sName)
        {
            int penid = -1;
            string st = sName.Substring(3, 2);
            if (st.EndsWith("_"))
                st = st.Substring(0, 1);
            int.TryParse(st, out penid);
            return penid;
        }

        public void ReadOptions(string file)
		{
            string file2 = file;
    if (!File.Exists(file))
        file = Program.RunningFolder + file2;
    if (!File.Exists(file))
        file = Program.ProgramFolder + file2;
    if (!File.Exists(file))
        return;

    FileStream fini = new FileStream(file, FileMode.Open);
    StreamReader srini = new StreamReader(fini);
    string sLine = "";
    string sNameO = "";
    string sName = "", sPara = "";
    int tempi = 0;
    List<string> writelines = new List<string>();

    while (sLine != null)
    {
        sPara = "";
        sLine = srini.ReadLine();
        if (sLine != null &&
            sLine != "" &&
            sLine.Length > 0 &&
            sLine.Substring(0, 1) != "-" &&
            sLine.Substring(0, 1) != "%" &&
            sLine.Substring(0, 1) != "'" &&
            sLine.Substring(0, 1) != "/" &&
            sLine.Substring(0, 1) != "!" &&
            sLine.Substring(0, 1) != "[" &&
            sLine.Substring(0, 1) != "#" &&
            sLine.Contains("=") &&
            !sLine.Substring(sLine.IndexOf("=") + 1).Contains("="))
        {
            sNameO = sLine.Substring(0, sLine.IndexOf("="));
            sName = sNameO.Trim().ToUpper();
            sPara = sLine.Substring(sLine.IndexOf("=") + 1).Trim();

            if (sName.StartsWith("PEN"))
            {
                int penid = GetPenNumber(sName);
                if (penid >= 0 && penid < MaxPenCount)
                {
                    if (sName.EndsWith("_ENABLED"))
                    {
                        if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                            PenEnabled[penid] = true;
                        else if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            PenEnabled[penid] = false;
                    }
                    else if (sName.EndsWith("_RED"))
                    {
                        int r;
                        if (int.TryParse(sPara, out r) && r >= 0 && r <= 255)
                        {
                            Color c = PenAttr[penid].Color;
                            PenAttr[penid].Color = Color.FromArgb(c.A, r, c.G, c.B);
                        }
                    }
                    else if (sName.EndsWith("_GREEN"))
                    {
                        int g;
                        if (int.TryParse(sPara, out g) && g >= 0 && g <= 255)
                        {
                            Color c = PenAttr[penid].Color;
                            PenAttr[penid].Color = Color.FromArgb(c.A, c.R, g, c.B);
                        }
                    }
                    else if (sName.EndsWith("_BLUE"))
                    {
                        int b;
                        if (int.TryParse(sPara, out b) && b >= 0 && b <= 255)
                        {
                            Color c = PenAttr[penid].Color;
                            PenAttr[penid].Color = Color.FromArgb(c.A, c.R, c.G, b);
                        }
                    }
                    else if (sName.EndsWith("_ALPHA"))
                    {
                        int a;
                        if (int.TryParse(sPara, out a) && a >= 0 && a <= 255)
                        {
                            PenAttr[penid].Transparency = (byte)(255 - a);
                        }
                    }
                    else if (sName.EndsWith("_WIDTH"))
                    {
                        float w;
                        if (float.TryParse(sPara, out w) && w > 0)
                            PenAttr[penid].Width = w;
                    }
                    else if (sName.EndsWith("_HOTKEY") && penid < MaxDisplayedPens)
                    {
                        Hotkey_Pens[penid].Parse(sPara);
                    }
                }
            }
            
            switch (sName)
            {
                case "HOTKEY_OPENTOOLBAR":
                    Hotkey_OpenToolbar.Parse(sPara);
                    break;
                
                case "HOTKEY_CLOSETOOLBAR":
                    Hotkey_CloseToolbar.Parse(sPara);
                    break;
                
                case "ALT_AS_TEMPORARY_COMMAND":
                    if (sPara.ToUpper() == "TRUE" || sPara.ToUpper() == "ON")
                        AltAsOneCommand = 2;
                    else if (int.TryParse(sPara, out tempi))
                        AltAsOneCommand = tempi;
                    else
                        AltAsOneCommand = 0;
                    break;
                
                case "LANGUAGE_FILE":
                    Local.CurrentLanguageFile = sPara;
                    break;
                
                case "HOTKEY_GLOBAL":
                    Hotkey_Global.Parse(sPara);
                    break;
                
                case "HOTKEY_TOGGLEFADING":
                    Hotkey_FadingToggle.Parse(sPara);
                    break;
                
                case "HOTKEY_ERASER":
                    Hotkey_Eraser.Parse(sPara);
                    break;
                
                case "HOTKEY_INKVISIBLE":
                    Hotkey_InkVisible.Parse(sPara);
                    break;
                
                case "HOTKEY_POINTER":
                    Hotkey_Pointer.Parse(sPara);
                    break;
                
                case "HOTKEY_PAN":
                    Hotkey_Pan.Parse(sPara);
                    break;
                
                case "HOTKEY_SCALE_ROTATE":
                    Hotkey_ScaleRotate.Parse(sPara);
                    break;
                
                case "HOTKEY_UNDO":
                    Hotkey_Undo.Parse(sPara);
                    break;
                
                case "HOTKEY_REDO":
                    Hotkey_Redo.Parse(sPara);
                    break;
                
                case "HOTKEY_SNAPSHOT":
                    Hotkey_Snap.Parse(sPara);
                    break;
                
                case "HOTKEY_CLEAR":
                    Hotkey_Clear.Parse(sPara);
                    break;
                
                case "HOTKEY_VIDEOREC":
                    Hotkey_Video.Parse(sPara);
                    break;
                
                case "HOTKEY_DOCKUNDOCK":
                    Hotkey_DockUndock.Parse(sPara);
                    break;
                
                case "HOTKEY_CLOSE":
                    Hotkey_Close.Parse(sPara);
                    break;
                
                case "HOTKEY_HAND":
                    Hotkey_Hand.Parse(sPara);
                    break;
                
                case "HOTKEY_LINE":
                    Hotkey_Line.Parse(sPara);
                    break;
                
                case "HOTKEY_RECT":
                    Hotkey_Rect.Parse(sPara);
                    break;
                
                case "HOTKEY_OVAL":
                    Hotkey_Oval.Parse(sPara);
                    break;
                
                case "HOTKEY_ARROW":
                    Hotkey_Arrow.Parse(sPara);
                    break;
                
                case "HOTKEY_TEXT":
                    Hotkey_Text.Parse(sPara);
                    break;
                
                case "HOTKEY_NUMBCHIP":
                    Hotkey_Numb.Parse(sPara);
                    break;
                
                case "HOTKEY_NTAG_SHOWWHITE":
                    Hotkey_NTag_ShowWhite.Parse(sPara);
                    break;
                
                case "HOTKEY_NTAG_SHOWBLACK":
                    Hotkey_NTag_ShowBlack.Parse(sPara);
                    break;
                
                case "HOTKEY_NTAG_HIDEWHITE":
                    Hotkey_NTag_HideWhite.Parse(sPara);
                    break;
                
                case "HOTKEY_NTAG_HIDEBLACK":
                    Hotkey_NTag_HideBlack.Parse(sPara);
                    break;

                // ----- Ajout : hotkeys pour shape tags -----
                case "HOTKEY_LETTERTAG":
                    Hotkey_LetterTag.Parse(sPara);
                    break;
                
                case "HOTKEY_SQUARETAG":
                    Hotkey_SquareTag.Parse(sPara);
                    break;
                
                case "HOTKEY_TRIANGLETAG":
                    Hotkey_TriangleTag.Parse(sPara);
                    break;
                
                case "HOTKEY_CIRCLETAG":
                    Hotkey_CircleTag.Parse(sPara);
                    break;
                
                case "HOTKEY_CROSSTAG":
                    Hotkey_CrossTag.Parse(sPara);
                    break;

                case "HOTKEY_EDIT":
                    Hotkey_Edit.Parse(sPara);
                    break;
                
                case "HOTKEY_MOVE":
                    Hotkey_Move.Parse(sPara);
                    break;
                
                case "HOTKEY_MAGNET":
                    Hotkey_Magnet.Parse(sPara);
                    break;
                
                case "HOTKEY_CLIPART":
                    Hotkey_ClipArt.Parse(sPara);
                    break;
                
                case "HOTKEY_CLIPART1":
                    Hotkey_ClipArt1.Parse(sPara);
                    break;
                
                case "HOTKEY_CLIPART2":
                    Hotkey_ClipArt2.Parse(sPara);
                    break;
                
                case "HOTKEY_CLIPART3":
                    Hotkey_ClipArt3.Parse(sPara);
                    break;
                
                case "HOTKEY_ZOOM":
                    Hotkey_Zoom.Parse(sPara);
                    break;
                
                case "HOTKEY_PENWIDTH_PLUS":
                    Hotkey_PenWidthPlus.Parse(sPara);
                    break;
                
                case "HOTKEY_PENWIDTH_MINUS":
                    Hotkey_PenWidthMinus.Parse(sPara);
                    break;
                
                case "HOTKEY_COLORPICKUP":
                    Hotkey_ColorPickup.Parse(sPara);
                    break;
                
                case "HOTKEY_COLOREDIT":
                    Hotkey_ColorEdit.Parse(sPara);
                    break;
                
                case "HOTKEY_LINESTYLE":
                    Hotkey_LineStyle.Parse(sPara);
                    break;
                
                case "HOTKEY_PREVPAGE":
                    Hotkey_PagePrev.Parse(sPara);
                    break;
                
                case "HOTKEY_NEXTPAGE":
                    Hotkey_PageNext.Parse(sPara);
                    break;
                
                case "HOTKEY_LOADSTROKES":
                    Hotkey_LoadStrokes.Parse(sPara);
                    break;
                
                case "HOTKEY_SAVESTROKES":
                    Hotkey_SaveStrokes.Parse(sPara);
                    break;
                
                case "HOTKEY_LASSO":
                    Hotkey_Lasso.Parse(sPara);
                    break;

                // Go options
                case "GOFILLOPACITY":
                    int opacity;
                    if (int.TryParse(sPara, out opacity) && opacity >= 0 && opacity <= 100)
                        GoFillOpacityPercent = opacity;
                    break;
                
                case "GOSTROKEOPACITY":
                    int strokeOpacity;
                    if (int.TryParse(sPara, out strokeOpacity) && strokeOpacity >= 0 && strokeOpacity <= 100)
                        GoStrokeOpacityPercent = strokeOpacity;
                    break;
                
                case "GOSTROKEWIDTH":
                    float strokeWidth;
                    if (float.TryParse(sPara, out strokeWidth) && strokeWidth > 0)
                        GoStrokeWidth = strokeWidth;
                    break;

                case "HOTKEY_HANDFILLEDWHITE":
                    Hotkey_HandFilledWhite.Parse(sPara);
                    break;
                
                case "HOTKEY_HANDFILLEDBLACK":
                    Hotkey_HandFilledBlack.Parse(sPara);
                    break;

                case "BUTTONCLICK_FOR_LINESTYLE":
                    ButtonClick_For_LineStyle = sPara.ToUpper() == "TRUE" || sPara == "1";
                    break;

                // Add other cases as needed
            }
            
            writelines.Add(sNameO + "= " + sPara);
        }
        else if (sLine != null)
        {
            writelines.Add(sLine);
        }
    }
    
    fini.Close();


    // ################ goInk - START ################
    // s'assurer que GRIDRECT est présent dans writelines (ajout si absent)
    bool foundGrid = false;
    for (int i = 0; i < writelines.Count; i++)
    {
        if (writelines[i].TrimStart().StartsWith("GRIDRECT=", StringComparison.InvariantCultureIgnoreCase))
        {
            foundGrid = true;
            break;
        }
    }
    if (!foundGrid)
    {
        writelines.Add("GRIDRECT=" + GridRect.Left.ToString() + "," + GridRect.Top.ToString() + "," + GridRect.Width.ToString() + "," + GridRect.Height.ToString() + "," + (GridRectDefined ? "1" : "0"));
    }

    /// ajouter les clés manquantes pour persistance ################
    bool hasGridRows = false, hasTagSize = false, hasTagCircle = false;
    for (int i = 0; i < writelines.Count; i++)
    {
        string s = writelines[i].TrimStart();
        if (s.StartsWith("GRIDROWS=", StringComparison.InvariantCultureIgnoreCase)) hasGridRows = true;
        if (s.StartsWith("TAGSIZE_PERCENT=", StringComparison.InvariantCultureIgnoreCase)) hasTagSize = true;
        if (s.StartsWith("TAGCIRCLE_PERCENT=", StringComparison.InvariantCultureIgnoreCase)) hasTagCircle = true;
    }
    if (!hasGridRows)
        writelines.Add("GRIDROWS=" + GridRows.ToString());
    if (!hasTagSize)
        writelines.Add("TAGSIZE_PERCENT=" + TagSizePercent.ToString(CultureInfo.InvariantCulture));
    if (!hasTagCircle)
        writelines.Add("TAGCIRCLE_PERCENT=" + TagCirclePercent.ToString(CultureInfo.InvariantCulture));

    // vérifier aussi hasTagOpacity
    bool hasTagStoneOpacity = false, hasTagNumberOpacity = false;
    for (int i = 0; i < writelines.Count; i++)
    {
        string s = writelines[i].TrimStart();
        if (s.StartsWith("TAGSTONEOPACITY_PERCENT=", StringComparison.InvariantCultureIgnoreCase)) hasTagStoneOpacity = true;
        if (s.StartsWith("TAGNUMBEROPACITY_PERCENT=", StringComparison.InvariantCultureIgnoreCase)) hasTagNumberOpacity = true;
    }
    if (!hasTagStoneOpacity)
        writelines.Add("TAGSTONEOPACITY_PERCENT=" + TagStoneOpacityPercent.ToString(CultureInfo.InvariantCulture));
    if (!hasTagNumberOpacity)
        writelines.Add("TAGNUMBEROPACITY_PERCENT=" + TagNumberOpacityPercent.ToString(CultureInfo.InvariantCulture));

    // ################ goInk - END ##############################################################

    // Helper method to set or replace a setting value
    void SetOrReplace(List<string> lines, string key, string value)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            string t = lines[i].TrimStart();
            if (t.StartsWith(key + "=", StringComparison.InvariantCultureIgnoreCase))
            {
                // préserve l'indentation initiale si nécessaire
                string prefix = lines[i].Substring(0, lines[i].IndexOf(t));
                lines[i] = prefix + key + "= " + value;
                return;
            }
        }
        lines.Add(key + "= " + value);
    }

    // Ensure / replace explicitement les clés GO et TAG (remplace si ligne existante, sinon ajoute)
    SetOrReplace(writelines, "HOTKEY_OPENTOOLBAR", Hotkey_OpenToolbar.ToStringInvariant());
    SetOrReplace(writelines, "HOTKEY_CLOSETOOLBAR", Hotkey_CloseToolbar.ToStringInvariant());
    
    
    SetOrReplace(writelines, "GOFILLOPACITY", GoFillOpacityPercent.ToString());
    SetOrReplace(writelines, "GOSTROKEOPACITY", GoStrokeOpacityPercent.ToString());
    SetOrReplace(writelines, "GOSTROKEWIDTH", GoStrokeWidth.ToString(System.Globalization.CultureInfo.InvariantCulture));
    SetOrReplace(writelines, "GOSTROKE_THICKNESS", (GoStrokeThickness == 0) ? "Thin" : (GoStrokeThickness == 2) ? "Thick" : "Normal");

    var ca = Color.FromArgb(GoArrowColorArgb);
    SetOrReplace(writelines, "GOARROWCOLOR", $"{ca.A},{ca.R},{ca.G},{ca.B}");

    SetOrReplace(writelines, "GOARROWTHICKNESS", GoArrowThickness.ToString(CultureInfo.InvariantCulture));
    SetOrReplace(writelines, "GOARROWLENGTH", GoArrowLength.ToString(CultureInfo.InvariantCulture));

    // Tags / goInk specific (force la persistance)
    SetOrReplace(writelines, "TAGSIZE_PERCENT", TagSizePercent.ToString(System.Globalization.CultureInfo.InvariantCulture));
    SetOrReplace(writelines, "TAGCIRCLE_PERCENT", TagCirclePercent.ToString(System.Globalization.CultureInfo.InvariantCulture));
    SetOrReplace(writelines, "TAGSTONEOPACITY_PERCENT", TagStoneOpacityPercent.ToString(System.Globalization.CultureInfo.InvariantCulture));
    SetOrReplace(writelines, "TAGNUMBEROPACITY_PERCENT", TagNumberOpacityPercent.ToString(System.Globalization.CultureInfo.InvariantCulture));

    // hotkeys go
    SetOrReplace(writelines, "HOTKEY_HANDFILLEDWHITE", Hotkey_HandFilledWhite.ToStringInvariant());
    SetOrReplace(writelines, "HOTKEY_HANDFILLEDBLACK", Hotkey_HandFilledBlack.ToStringInvariant());

    // ----- Ajout : persistance explicite des hotkeys des shape tags -----
    SetOrReplace(writelines, "HOTKEY_LETTERTAG", Hotkey_LetterTag.ToStringInvariant());
    SetOrReplace(writelines, "HOTKEY_SQUARETAG", Hotkey_SquareTag.ToStringInvariant());
    SetOrReplace(writelines, "HOTKEY_TRIANGLETAG", Hotkey_TriangleTag.ToStringInvariant());
    SetOrReplace(writelines, "HOTKEY_CIRCLETAG", Hotkey_CircleTag.ToStringInvariant());
    SetOrReplace(writelines, "HOTKEY_CROSSTAG", Hotkey_CrossTag.ToStringInvariant());

    SetOrReplace(writelines, "GOTOOL_LETTER_COLOR", $"{GoTool_Letter_Color[0]},{GoTool_Letter_Color[1]},{GoTool_Letter_Color[2]},{GoTool_Letter_Color[3]}");
    SetOrReplace(writelines, "GOTOOL_SQUARE_COLOR", $"{GoTool_Square_Color[0]},{GoTool_Square_Color[1]},{GoTool_Square_Color[2]},{GoTool_Square_Color[3]}");
    SetOrReplace(writelines, "GOTOOL_TRIANGLE_COLOR", $"{GoTool_Triangle_Color[0]},{GoTool_Triangle_Color[1]},{GoTool_Triangle_Color[2]},{GoTool_Triangle_Color[3]}");
    SetOrReplace(writelines, "GOTOOL_CIRCLE_COLOR", $"{GoTool_Circle_Color[0]},{GoTool_Circle_Color[1]},{GoTool_Circle_Color[2]},{GoTool_Circle_Color[3]}");
    SetOrReplace(writelines, "GOTOOL_CROSS_COLOR", $"{GoTool_Cross_Color[0]},{GoTool_Cross_Color[1]},{GoTool_Cross_Color[2]},{GoTool_Cross_Color[3]}");

    SetOrReplace(writelines, "GOTOOL_TEXT_COLOR", $"{GoTool_Text_Color[0]},{GoTool_Text_Color[1]},{GoTool_Text_Color[2]},{GoTool_Text_Color[3]}");

    SetOrReplace(writelines, "GOSTROKE_THICKNESS", (GoStrokeThickness == 0) ? "Thin" : (GoStrokeThickness == 2) ? "Thick" : "Normal");

    // Ensure NumberTag hotkey keys are present so SaveOptions writes them (adds missing keys)
    bool hasShowWhite = false, hasShowBlack = false, hasHideWhite = false, hasHideBlack = false;
    for (int i = 0; i < writelines.Count; i++)
    {
        string s = writelines[i].TrimStart();
        if (s.StartsWith("HOTKEY_NTAG_SHOWWHITE=", StringComparison.InvariantCultureIgnoreCase)) hasShowWhite = true;
        if (s.StartsWith("HOTKEY_NTAG_SHOWBLACK=", StringComparison.InvariantCultureIgnoreCase)) hasShowBlack = true;
        if (s.StartsWith("HOTKEY_NTAG_HIDEWHITE=", StringComparison.InvariantCultureIgnoreCase)) hasHideWhite = true;
        if (s.StartsWith("HOTKEY_NTAG_HIDEBLACK=", StringComparison.InvariantCultureIgnoreCase)) hasHideBlack = true;
    }

    if (!hasShowWhite)
        writelines.Add("HOTKEY_NTAG_SHOWWHITE= " + Hotkey_NTag_ShowWhite.ToStringInvariant());
    if (!hasShowBlack)
        writelines.Add("HOTKEY_NTAG_SHOWBLACK= " + Hotkey_NTag_ShowBlack.ToStringInvariant());
    if (!hasHideWhite)
        writelines.Add("HOTKEY_NTAG_HIDEWHITE= " + Hotkey_NTag_HideWhite.ToStringInvariant());
    if (!hasHideBlack)
        writelines.Add("HOTKEY_NTAG_HIDEBLACK= " + Hotkey_NTag_HideBlack.ToStringInvariant());

    FileStream frini = new FileStream(file, FileMode.Create);
    StreamWriter swini = new StreamWriter(frini);
    swini.AutoFlush = true;
    foreach (string line in writelines)
        swini.WriteLine(line);
    frini.Close();
}

public string Fill2Str(int filling)
{
    switch (filling)
    {
        case Filling.Empty: return "Empty";
        case Filling.NoFrame: return "NoFrame";
        case Filling.PenColorFilled: return "PenColorFilled";
        case Filling.Outside: return "Outside";
        case Filling.WhiteFilled: return "WhiteFilled";
        case Filling.BlackFilled: return "BlackFilled";
        default: return "Unknown";
    }
}
public string CompleteConfig(string templateFile, string configFile)
{
    // Implementation for CompleteConfig
    return "";
}

// This is the overload we want to keep - only the Microsoft.Ink version is needed
public string LineStyleToString(Microsoft.Ink.ExtendedProperties properties)
{
    // Implementation for LineStyleToString for Microsoft.Ink.ExtendedProperties
    if (properties == null || !properties.Contains(DASHED_LINE_GUID))
        return "Solid";
    
    // Return different style based on DashStyle value
    return "Solid";
}

[DllImport("user32.dll")]
public static extern bool SetForegroundWindow(IntPtr hWnd);

public void AppGetFocus()
{
    SetForegroundWindow(FormCollection.Handle);
}

public void SetHotkey()
{
    // Implementation for setting hotkeys would go here
    // This is a stub for the missing method
}

// Path helper method
public string MakeRelativePath(string basePath, string targetPath)
{
    // Simple implementation for MakeRelativePath
    if (string.IsNullOrEmpty(targetPath))
        return "";
        
    // Return the targetPath as is for now
    return targetPath;
}

// ===== Added helpers referenced by other parts of the project =====
public void SaveOptions(string file)
{
    try
    {
        var dir = Path.GetDirectoryName(file);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        using (var sw = new StreamWriter(file, false, Encoding.UTF8))
        {
            sw.WriteLine("; Saved by ppInk/goInk at {0}", DateTime.Now);
        }
    }
    catch { }
}

public int PixelToHiMetric(int pixels)
{
    try
    {
        float dpi = 96f;
        using (Graphics g = FormDisplay != null ? FormDisplay.CreateGraphics() : Graphics.FromHwnd(IntPtr.Zero))
        {
            if (g != null) dpi = g.DpiX;
        }
        // 1 inch = 2540 HiMetric units
        return (int)Math.Round(pixels * 2540.0 / dpi);
    }
    catch { return pixels; }
}

// Overload returning float for float input
public float PixelToHiMetric(float pixels)
{
    try
    {
        float dpi = 96f;
        using (Graphics g = FormDisplay != null ? FormDisplay.CreateGraphics() : Graphics.FromHwnd(IntPtr.Zero))
        {
            if (g != null) dpi = g.DpiX;
        }
        return (float)(pixels * 2540.0 / dpi);
    }
    catch { return pixels; }
}

public int HiMetricToPixel(float hiMetric)
{
    try
    {
        float dpi = 96f;
        using (Graphics g = FormDisplay != null ? FormDisplay.CreateGraphics() : Graphics.FromHwnd(IntPtr.Zero))
        {
            if (g != null) dpi = g.DpiX;
        }
        return (int)Math.Max(1, Math.Round(hiMetric * dpi / 2540.0));
    }
    catch { return (int)hiMetric; }
}

// Overload for double input used at some call sites
public int HiMetricToPixel(double hiMetric)
{
    try
    {
        float dpi = 96f;
        using (Graphics g = FormDisplay != null ? FormDisplay.CreateGraphics() : Graphics.FromHwnd(IntPtr.Zero))
        {
            if (g != null) dpi = g.DpiX;
        }
        return (int)Math.Max(1, Math.Round(hiMetric * dpi / 2540.0));
    }
    catch { return (int)Math.Round(hiMetric); }
}

public DashStyle LineStyleFromString(string s)
{
    if (string.IsNullOrWhiteSpace(s)) return DashStyle.Solid;
    switch (s.Trim().ToLowerInvariant())
    {
        case "solid":
        case "stroke":
            return DashStyle.Solid;
        case "dash":
            return DashStyle.Dash;
        case "dot":
            return DashStyle.Dot;
        case "dashdot":
            return DashStyle.DashDot;
        case "dashdotdot":
            return DashStyle.DashDotDot;
        default:
            return DashStyle.Solid;
    }
}

public string NextLineStyleString(string current)
{
    string[] styles = { "Solid", "Dash", "Dot", "DashDot", "DashDotDot" };
    int idx = Array.FindIndex(styles, s => string.Equals(s, current, StringComparison.InvariantCultureIgnoreCase));
    if (idx < 0) idx = 0; else idx = (idx + 1) % styles.Length;
    return styles[idx];
}

public void UnsetHotkey()
{
    // Stub: unregister hotkeys temporarily while editing in options dialog (no-op here)
}

public void ChangeLanguage(string languageFile)
{
    try
    {
        if (!string.IsNullOrEmpty(languageFile))
        {
            Local.CurrentLanguageFile = languageFile;
            Local.LoadLocalList();
        }
    }
    catch { }
}

        // Moved inside Root class: event handlers for tray menu
        private void OnAbout(object sender, EventArgs e)
        {
            // Show about dialog
            MessageBox.Show("goInk - A Go game annotation tool based on ppInk", "About goInk", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnOptions(object sender, EventArgs e)
        {
            // Show options dialog
            if (FormOptions == null)
                FormOptions = new FormOptions(this);
            FormOptions.ShowDialog();
        }

        private void OnExit(object sender, EventArgs e)
        {
            // Exit application
            Application.Exit();
        }
    }
}

