using Microsoft.Ink;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
//using System.Windows.Controls;
using System.Windows.Forms;

namespace gInk
{
    using ListPoint = List<Point>;

    public partial class FormCollection : Form
    {
        [Flags, Serializable]
        public enum RegisterTouchFlags
        {
            TWF_NONE = 0x00000000,
            TWF_FINETOUCH = 0x00000001, //Specifies that hWnd prefers noncoalesced touch input.
            TWF_WANTPALM = 0x00000002 //Setting this flag disables palm rejection which reduces delays for getting WM_TOUCH messages.
        }
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool RegisterTouchWindow(IntPtr hWnd, RegisterTouchFlags flags);

        // to load correctly customed cursor file
        static class MyNativeMethods
        {
            public static System.Windows.Forms.Cursor LoadCustomCursor(string path)
            {
                IntPtr hCurs = LoadCursorFromFile(path);
                if (hCurs == IntPtr.Zero) throw new Win32Exception();
                var curs = new System.Windows.Forms.Cursor(hCurs);
                // Note: force the cursor to own the handle so it gets released properly
                //var fi = typeof(System.Windows.Forms.Cursor).GetField("ownHandle", BindingFlags.NonPublic | BindingFlags.Instance);
                //fi.SetValue(curs, true);
                return curs;
            }
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern IntPtr LoadCursorFromFile(string path);
        }

        public Dictionary<int, AnimationStructure> Animations = new Dictionary<int, AnimationStructure>();
        public int AniPoolIdx;

        // Button/Tooblar
        const double NormSizePercent = 0.96;
        const double SmallSizePercent = 0.47;
        const double TopPercent = 0.02;
        const double SmallButtonNext = 0.98 - .47;
        const double InterButtonGap = .02;

        // hotkeys
        const int VK_SHIFT = 0x10;
        const int VK_CONTROL = 0x11;
        const int VK_MENU = 0x12;
        const int VK_LCONTROL = 0xA2;
        const int VK_RCONTROL = 0xA3;
        const int VK_LSHIFT = 0xA0;
        const int VK_RSHIFT = 0xA1;
        const int VK_LMENU = 0xA4;
        const int VK_RMENU = 0xA5;
        const int VK_LWIN = 0x5B;
        const int VK_RWIN = 0x5C;
        private PenModifyDlg PenModifyDlg;
        public Root Root;
        public InkOverlay IC;

        public Button[] btPen;
        public Bitmap image_dock, image_dockback;
        public Bitmap image_pointer, image_pointer_act;
        public Bitmap image_eraser_act, image_eraser;
        public Bitmap image_visible_not, image_visible;
        public Bitmap image_lasso_act, image_lasso;
        public System.Windows.Forms.Cursor cursorred, cursortarget, cursorsnap, cursorerase;
        public System.Windows.Forms.Cursor cursortip;
        public System.Windows.Forms.Cursor tempArrowCursor = null;
        public bool Initializing;

        public bool? oldShiftPensExtra = null;
        public int FirstPenDisplayed;

        public DateTime MouseTimeDown;
        public object MouseDownButtonObject;
        public int ButtonsEntering = 0;  // -1 = exiting
        public int gpButtonsLeft, gpButtonsTop, gpButtonsWidth, gpButtonsHeight; // the default location, fixed
        public Size VisibleToolbar = new Size();

        public bool gpPenWidth_MouseOn = false;
        public int gpSubTools_MouseOn = 0;

        public int PrimaryLeft, PrimaryTop;

        public int LastPenSelected = 0;
        public int SavedTool = -1;
        public int SavedFilled = -1;
        public int SavedPen = -1;
        private int PolyLineLastX = Int32.MinValue;
        private int PolyLineLastY = Int32.MinValue;
        private Stroke PolyLineInProgress = null;
        private bool FromHandToLineOnShift = false;

        public bool SnapWithoutClosing = false;

        // we have local variables for font to have an session limited default font characteristics
        public int TextSize = 25;
        public string TextFont = "Arial";
        public bool TextItalic = false;
        public bool TextBold = false;
        public int TagSize = 16;
        public string TagFont = "Arial";
        public bool TagItalic = false;
        public bool TagBold = false;

        private bool SetWindowInputRectFlag = false;

        public ImageLister ClipartsDlg;
        private Object btClipSel;

        private List<Stroke> FadingList = new List<Stroke>();

        public ZoomForm ZoomForm = new ZoomForm();
        private Bitmap ZoomImage, ZoomImage2;
        int ZoomFormRePosX;
        int ZoomFormRePosY;
        string ZoomSaveStroke;
        public MouseButtons CurrentMouseButton = MouseButtons.None;

        public string SaveStrokeFile;
        public List<string> PointerModeSnaps = new List<string>();

        public Button[] Btn_SubTools;

        public ToolTip MetricToolTip = new ToolTip();

        public Strokes StrokesSelection, InprogressSelection;
        public bool AppendToSelection;

        public Stroke LineForPatterns = null;
        public int PatternLineSteps = -1;          //0 = getSize ; 1 = getDistance; 2 = getStroke
        public Bitmap PatternImage = null;
        public bool RotatingOnLine = false;
        public ListPoint PatternPoints = new List<Point>();
        public List<ListPoint> StoredPatternPoints = new List<ListPoint>();
        public int PatternLastPtIndex = -1;
        public double PatternLastPtRemain = 0;
        public double PatternDist = double.MaxValue;

        public List<Bitmap> StoredArrowImages = new List<Bitmap>();

        public int PageIndex = 0;
        public int PageMax = 0;

        public static double Measure2Scale = Root.Measure2Scale;

        // === NOUVEAUX OUTILS TAGS ===

        // Compteur de lettres (A,B,...Z,AA,AB...)
        private int LetterTag_Counter = 0;

        private string GetNextLetterTag()
        {
            int n = LetterTag_Counter++;
            string s = "";
            do
            {
                int r = n % 26;
                s = (char)('A' + r) + s;
                n = n / 26 - 1;
            } while (n >= 0);
            return s;
        }

        private char ShapeGlyphForTool(int tool)
        {
            switch (tool)
            {
                // utiliser des glyphes "large" là où possible pour un rendu plus visible
                case Tools.SquareTag: return '⬜'; // U+2B1C WHITE LARGE SQUARE (plus grand que U+25A1)
                case Tools.TriangleTag: return '△'; // U+25B3 WHITE UP-POINTING TRIANGLE (creux)
                case Tools.CircleTag: return '⚪'; // U+26AA MEDIUM WHITE CIRCLE (souvent plus lisible que U+25CB)
                // la croix : plusieurs options, le rendu dépend des polices installées -> peut varier d'un poste à l'autre
                case Tools.CrossTag: return '✖'; // U+2716 HEAVY MULTIPLICATION X (généralement visible)
                default: return '?';
            }
        }

        private bool IsFixedShapeTool(int tool)
        {
            return tool == Tools.SquareTag ||
                   tool == Tools.TriangleTag ||
                   tool == Tools.CircleTag ||
                   tool == Tools.CrossTag;
        }

        private bool IsNewTagTool(int tool)
        {
            return tool == Tools.LetterTag || IsFixedShapeTool(tool);
        }

        private Stroke AddShapeTagStroke(int xCenter, int yCenter, string txt)
        {
            // comportement historique : conserve l'ancienne taille basée sur TagSize
            int diameter = Math.Max(10, (int)Math.Round(TagSize * 0.8));
            return AddShapeTagStroke(xCenter, yCenter, txt, diameter, null);
        }

        private Stroke AddShapeTagStroke(int xCenter, int yCenter, string txt, int diameterPx, Stroke st = null)
        {
            // clamp minimal
            int diameter = Math.Max(6, diameterPx);
            int half = Math.Max(1, diameter / 2);

            // largeur de trait exprimée en pixels (logique UI)
            float basePenWidthPx = Math.Max(1.0f, diameter / 4f);
            const float ShapeStrokeMultiplier = 3.0f;
            float shapePenWidthPx = basePenWidthPx * ShapeStrokeMultiplier;

            // Conversion pixels -> HiMetric (DrawingAttributes.Width attend HiMetric).
            // Application du facteur selon l'option utilisateur (0=fin,1=moyen,2=épais).
            float shapePenWidthHiMetric;
            try
            {
                shapePenWidthHiMetric = Root.PixelToHiMetric(shapePenWidthPx);
            }
            catch
            {
                shapePenWidthHiMetric = (float)(shapePenWidthPx / 0.037795280352161f);
            }

            // appliquer la division choisie par l'utilisateur (/12, /9, /6)
            float div;
            try
            {
                int mode = Root.GoStrokeThickness; // 0=fin,1=moyen,2=épais
                if (mode == 0) div = 15f;
                else if (mode == 2) div = 6f;
                else div = 9f; // default = moyen
            }
            catch
            {
                div = 9f;
            }
            shapePenWidthHiMetric = shapePenWidthHiMetric / div;

            // Lettre : texte centré et agrandi (double de la taille précédente)
            if (Root.ToolSelected == Tools.LetterTag)
            {
                Stroke stTxt = AddTextStroke(xCenter, yCenter, xCenter, yCenter, txt, StringAlignment.Center, Filling.Empty);
                if (stTxt != null)
                {
                    try
                    {
                        double sizePct = (Root.TagSizePercent <= 0.0) ? 100.0 : Root.TagSizePercent;
                        double fontSize;
                        if (this.GridRectDefined && this.GridRect.Width > 0 && this.GridRect.Height > 0)
                            fontSize = Math.Max(6.0, diameter * 0.54 * (sizePct / 100.0));
                        else
                            fontSize = Math.Max(6.0, (double)TagSize * (sizePct / 100.0));

                        double maxFromCircle = Math.Max(6.0, diameter * 0.75);
                        if (fontSize > maxFromCircle) fontSize = maxFromCircle;

                        // ajustement demandé précédemment : facteur 1.20 sur base
                        fontSize = fontSize * 1.20;

                        stTxt.ExtendedProperties.Add(Root.TEXTFONT_GUID, TagFont);
                        stTxt.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, fontSize);
                        System.Drawing.FontStyle style = TagItalic ? System.Drawing.FontStyle.Italic : System.Drawing.FontStyle.Regular;
                        if (TagBold) style |= System.Drawing.FontStyle.Bold;
                        stTxt.ExtendedProperties.Add(Root.TEXTFONTSTYLE_GUID, style);

                        stTxt.ExtendedProperties.Add(Root.TEXTHALIGN_GUID, StringAlignment.Center);
                        stTxt.ExtendedProperties.Add(Root.TEXTVALIGN_GUID, StringAlignment.Center);

                        // appliquer couleur/opacité configurée pour le tag Lettre
                        ApplyGoTagColorToDrawingAttributes(stTxt.DrawingAttributes, Root.GoTool_Letter_Color);

                        try
                        {
                            stTxt.ExtendedProperties.Add(Root.ISTAG_GUID, true);
                            try
                            {
                                if (!st.ExtendedProperties.Contains(Root.TEXTCOLOR_GUID))
                                    st.ExtendedProperties.Add(Root.TEXTCOLOR_GUID, Color.FromArgb(255, 128, 128, 128).ToArgb());
                            }
                            catch { }
                        }
                        catch { }
                        ComputeTextBoxSize(ref stTxt);
                    }
                    catch { }
                }
                return stTxt;
            }

            // Pour les formes fixes, dessiner des contours vectoriels (taille réduite de 40% si demandé ailleurs)
            try
            {
                switch (Root.ToolSelected)
                {
                    case Tools.SquareTag:
                        {
                            int eHalf = Math.Max(1, (int)Math.Round(half * 0.60));
                            int left = xCenter - eHalf;
                            int top = yCenter - eHalf;
                            int right = xCenter + eHalf;
                            int bottom = yCenter + eHalf;

                            Stroke rect = AddRectStroke(left, top, right, bottom, Filling.Empty);
                            if (rect != null)
                            {
                                try
                                {
                                    ApplyGoTagColorToDrawingAttributes(rect.DrawingAttributes, Root.GoTool_Square_Color);
                                    rect.DrawingAttributes.Width = shapePenWidthHiMetric;
                                    setStrokeProperties(ref rect, Filling.Empty);
                                    rect.ExtendedProperties.Add(Root.ISTAG_GUID, true);
                                    try
                                    {
                                        if (!st.ExtendedProperties.Contains(Root.TEXTCOLOR_GUID))
                                            st.ExtendedProperties.Add(Root.TEXTCOLOR_GUID, Color.FromArgb(255, 128, 128, 128).ToArgb());
                                    }
                                    catch { }
                                }
                                catch { }
                            }
                            return rect;
                        }

                    case Tools.CircleTag:
                        {
                            int eHalf = Math.Max(1, (int)Math.Round(half * 0.60));
                            Stroke circ = AddEllipseStroke(xCenter, yCenter, xCenter + eHalf, yCenter + eHalf, Filling.Empty);
                            if (circ != null)
                            {
                                try
                                {
                                    ApplyGoTagColorToDrawingAttributes(circ.DrawingAttributes, Root.GoTool_Circle_Color);
                                    circ.DrawingAttributes.Width = shapePenWidthHiMetric;
                                    setStrokeProperties(ref circ, Filling.Empty);
                                    circ.ExtendedProperties.Add(Root.ISTAG_GUID, true);
                                    try
                                    {
                                        if (!st.ExtendedProperties.Contains(Root.TEXTCOLOR_GUID))
                                            st.ExtendedProperties.Add(Root.TEXTCOLOR_GUID, Color.FromArgb(255, 128, 128, 128).ToArgb());
                                    }
                                    catch { }

                                    try
                                    {
                                        if (!st.ExtendedProperties.Contains(Root.TEXTCOLOR_GUID))
                                            st.ExtendedProperties.Add(Root.TEXTCOLOR_GUID, Color.FromArgb(255, 128, 128, 128).ToArgb());
                                    }
                                    catch { }
                                }
                                catch { }
                            }
                            return circ;
                        }

                    case Tools.TriangleTag:
                        {
                            int eHalf = Math.Max(1, (int)Math.Round(half * 0.60));
                            int shift = (int)Math.Round((double)eHalf / 3.0); // recentrage centroïde
                            Point pTop = new Point(xCenter, yCenter - eHalf - shift);
                            Point pBL = new Point(xCenter - eHalf, yCenter + eHalf - shift);
                            Point pBR = new Point(xCenter + eHalf, yCenter + eHalf - shift);
                            Point[] pts = new Point[] { pTop, pBR, pBL, pTop };

                            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts);

                            Stroke st2 = IC.Ink.CreateStroke(pts);
                            st2.DrawingAttributes = IC.DefaultDrawingAttributes.Clone();
                            st2.DrawingAttributes.AntiAliased = true;
                            st2.DrawingAttributes.FitToCurve = false;

                            ApplyGoTagColorToDrawingAttributes(st2.DrawingAttributes, Root.GoTool_Triangle_Color);
                            st2.DrawingAttributes.Width = shapePenWidthHiMetric;
                            setStrokeProperties(ref st2, Filling.Empty);
                            try
                            {
                                st2.ExtendedProperties.Add(Root.ISTAG_GUID, true);
                                try
                                {
                                    if (!st2.ExtendedProperties.Contains(Root.TEXTCOLOR_GUID))
                                        st2.ExtendedProperties.Add(Root.TEXTCOLOR_GUID, Color.FromArgb(255, 128, 128, 128).ToArgb());
                                }
                                catch { }
                            }
                            catch { }

                            IC.Ink.Strokes.Add(st2);
                            if (st2.ExtendedProperties.Contains(Root.FADING_PEN)) FadingList.Add(st2);
                            return st2;
                        }

                    case Tools.CrossTag:
                        {
                            double scale = 0.60;
                            int halfCross = Math.Max(1, (int)Math.Round(half * scale));

                            int x0 = xCenter - halfCross;
                            int y0 = yCenter - halfCross;
                            int x1 = xCenter + halfCross;
                            int y1 = yCenter + halfCross;

                            Stroke s1 = AddLineStroke(x0, y0, x1, y1);
                            Stroke s2 = AddLineStroke(x0, y1, x1, y0);
                            if (s1 != null)
                            {
                                try
                                {
                                    ApplyGoTagColorToDrawingAttributes(s1.DrawingAttributes, Root.GoTool_Cross_Color);
                                    s1.DrawingAttributes.Width = shapePenWidthHiMetric;
                                    setStrokeProperties(ref s1, Filling.Empty);
                                    s1.ExtendedProperties.Add(Root.ISTAG_GUID, true);
                                    try
                                    {
                                        if (!st.ExtendedProperties.Contains(Root.TEXTCOLOR_GUID))
                                            st.ExtendedProperties.Add(Root.TEXTCOLOR_GUID, Color.FromArgb(255, 128, 128, 128).ToArgb());
                                    }
                                    catch { }
                                }
                                catch { }
                            }
                            if (s2 != null)
                            {
                                try
                                {
                                    ApplyGoTagColorToDrawingAttributes(s2.DrawingAttributes, Root.GoTool_Cross_Color);
                                    s2.DrawingAttributes.Width = shapePenWidthHiMetric;
                                    setStrokeProperties(ref s2, Filling.Empty);
                                    s2.ExtendedProperties.Add(Root.ISTAG_GUID, true);
                                    try
                                    {
                                        if (!st.ExtendedProperties.Contains(Root.TEXTCOLOR_GUID))
                                            st.ExtendedProperties.Add(Root.TEXTCOLOR_GUID, Color.FromArgb(255, 128, 128, 128).ToArgb());
                                    }
                                    catch { }
                                }
                                catch { }
                            }
                            return s1 ?? s2;
                        }

                    default:
                        {
                            Stroke stTxt = AddTextStroke(xCenter, yCenter, xCenter, yCenter, txt, StringAlignment.Center, Filling.Empty);
                            if (stTxt != null)
                            {
                                try
                                {
                                    stTxt.DrawingAttributes.Color = Color.Black;
                                    stTxt.ExtendedProperties.Add(Root.ISTAG_GUID, true);
                                    try
                                    {
                                        if (!st.ExtendedProperties.Contains(Root.TEXTCOLOR_GUID))
                                            st.ExtendedProperties.Add(Root.TEXTCOLOR_GUID, Color.FromArgb(255, 128, 128, 128).ToArgb());
                                    }
                                    catch { }

                                    double sizePct = (Root.TagSizePercent <= 0.0) ? 100.0 : Root.TagSizePercent;
                                    double fontSize = Math.Max(6.0, (double)TagSize * (sizePct / 100.0)) * 1.20;
                                    stTxt.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, fontSize);
                                    stTxt.ExtendedProperties.Add(Root.TEXTHALIGN_GUID, StringAlignment.Center);
                                    stTxt.ExtendedProperties.Add(Root.TEXTVALIGN_GUID, StringAlignment.Center);
                                    ComputeTextBoxSize(ref stTxt);
                                }
                                catch { }
                            }
                            return stTxt;
                        }
                }
            }
            catch
            {
                try
                {
                    Stroke stTxt = AddTextStroke(xCenter, yCenter, xCenter, yCenter, txt, StringAlignment.Center, Filling.Empty);
                    if (stTxt != null)
                    {
                        stTxt.DrawingAttributes.Color = Color.Black;
                        stTxt.ExtendedProperties.Add(Root.ISTAG_GUID, true);
                        try
                        {
                            if (!st.ExtendedProperties.Contains(Root.TEXTCOLOR_GUID))
                                st.ExtendedProperties.Add(Root.TEXTCOLOR_GUID, Color.FromArgb(255, 128, 128, 128).ToArgb());
                        }
                        catch { }

                        ComputeTextBoxSize(ref stTxt);
                    }
                    return stTxt;
                }
                catch { return null; }
            }
        }

        // Gestion du clic sur les nouveaux boutons
        private void NewTagTool_Click(object sender, EventArgs e)
        {
            if (sender == btLetter) SelectTool(Tools.LetterTag);
            else if (sender == btSquare) SelectTool(Tools.SquareTag);
            else if (sender == btTriangle) SelectTool(Tools.TriangleTag);
            else if (sender == btCircle) SelectTool(Tools.CircleTag);
            else if (sender == btCross) SelectTool(Tools.CrossTag);
        }

        // http://www.csharp411.com/hide-form-from-alttab/
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr hObject);

            //[DllImport("kernel32.dll")]
            //public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

            [DllImport("kernel32.dll")]
            public static extern uint SuspendThread(IntPtr hThread);

            [DllImport("kernel32.dll")]
            public static extern uint ResumeThread(IntPtr hThread);
        }

        public static System.Windows.Forms.Cursor getCursFromDiskOrRes(string name, System.Windows.Forms.Cursor nocurs)
        {
            string filename;
            string[] namesize = name.Split('%');
            float scale = 1.0F;
            System.Windows.Forms.Cursor curs = null;
            Bitmap bmp = null;
            int[] cursorsize = new int[2];
            int[] hotSpot = new int[2];
            if (namesize.Length > 1)
                scale = float.Parse(namesize[1], CultureInfo.InvariantCulture);
            try
            {
                {
                    string[] exts = { ".cur", ".ico" };
                    foreach (string ext in exts)
                    {
                        filename = Program.RunningFolder + namesize[0] + ext;
                        if (File.Exists(filename))
                            try
                            {
                                curs = new System.Windows.Forms.Cursor(filename);
                                hotSpot[0] = curs.HotSpot.X; hotSpot[1] = curs.HotSpot.Y;
                                // required to handle every cursor size such as 128x128
                                bmp = new Bitmap(filename);
                                cursorsize[0] = bmp.Width; cursorsize[1] = bmp.Height;
                                break;
                            }
                            catch (Exception e)
                            {
                                Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:\n{1}\n", filename, e));
                            }
                    }
                }
                {
                    string[] exts = { ".ani" };
                    foreach (string ext in exts)
                    {
                        filename = Program.RunningFolder + namesize[0] + ext;
                        if (File.Exists(filename))
                            try
                            {
                                curs = new System.Windows.Forms.Cursor(filename);
                                return curs;
                            }
                            catch (Exception e)
                            {
                                Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:\n{1}\n", filename, e));
                            }
                    }
                }
                {
                    string[] exts = { ".bmp", ".png", ".tif", ".jpg", ".jpeg" };
                    foreach (string ext in exts)
                    {
                        filename = Program.RunningFolder + namesize[0] + ext;
                        if (File.Exists(filename))
                            try
                            {
                                bmp = new Bitmap(filename);
                                cursorsize[0] = bmp.Width; cursorsize[1] = bmp.Height;
                                hotSpot[0] = bmp.Width / 2; hotSpot[1] = bmp.Height / 2;
                                try
                                {
                                    string fn1 = Path.GetFileNameWithoutExtension(namesize[0]);
                                    fn1 = fn1.Split('@')[1];
                                    string[] lst = fn1.Split('.');
                                    int dx = int.Parse(lst[0]);
                                    int dy = int.Parse(lst[1]);
                                    hotSpot[0] = dx;
                                    hotSpot[1] = dy;
                                }
                                catch
                                {
                                    ;
                                }
                            }
                            catch (Exception e)
                            {
                                Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:\n{1}\n", filename, e));
                            }
                    }
                }
            }
            catch
            {
                return nocurs;
            }
            if (bmp == null)
            {
                curs = new System.Windows.Forms.Cursor(((System.Drawing.Icon)Properties.Resources.ResourceManager.GetObject(namesize[0])).Handle);
                cursorsize[0] = 128; cursorsize[1] = 128;
                hotSpot[0] = (int)(cursorsize[0] * (1.0 * curs.HotSpot.X) / curs.Size.Width); hotSpot[1] = (int)(cursorsize[1] * (1.0 * curs.HotSpot.Y) / curs.Size.Height);
                if (hotSpot[0] >= cursorsize[0] || hotSpot[1] >= cursorsize[1])
                {
                    hotSpot[0] = cursorsize[0] / 2;
                    hotSpot[1] = cursorsize[1] / 2;
                }
                bmp = new Bitmap(cursorsize[0], cursorsize[1], PixelFormat.Format32bppArgb);
                Graphics mg = Graphics.FromImage(bmp);
                curs.DrawStretched(mg, new Rectangle(0, 0, bmp.Width, bmp.Height));
                mg.Dispose();
            }
            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                bmp.MakeTransparent(bmp.GetPixel(0, 0));
            }
            Bitmap imgout = new Bitmap((int)Math.Round(cursorsize[0] * scale), (int)Math.Round(cursorsize[1] * scale), PixelFormat.Format32bppArgb);
            Graphics myGraphics = Graphics.FromImage(imgout);
            myGraphics.DrawImage(bmp, 0, 0, imgout.Width, imgout.Height);
            myGraphics.Dispose();
            return CreateCursorFromBitmap(imgout, (int)Math.Round(hotSpot[0] * scale), (int)Math.Round(hotSpot[1] * scale));
        }

        string[] ImageExts = { ".png" };

        public static Bitmap getImgFromDiskOrRes(string name, string[] exts = null)
        {
            string filename;
            if (Path.HasExtension(name))
                exts = new string[] { "" };
            else if (exts == null)
            {
                exts = new string[] { ".png", ".jpg", ".jpeg" };
            }
            foreach (string ext in exts)
            {
                if (Path.IsPathRooted(name))
                    filename = name + ext;
                else
                    filename = Program.RunningFolder + name + ext;
                if (File.Exists(filename))
                    try
                    {
                        return new Bitmap(filename);
                    }
                    catch (Exception e)
                    {
                        Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:{1} \n", filename, e));
                        return getImgFromDiskOrRes("unknown");
                    }
            }
            try
            {
                return new Bitmap((Bitmap)Properties.Resources.ResourceManager.GetObject(name));
            }
            catch
            {
                return getImgFromDiskOrRes("unknown");
            }
        }
        private double WidthForHalfDiag = 18.0 * global::gInk.Properties.Resources._null.Width * Math.Sqrt(2) / 2.0 / 300.0;

        private Bitmap BuildArrowBtn(string head, string tail, Color col)
        {
            Bitmap b = new Bitmap(global::gInk.Properties.Resources._null);
            Graphics g = Graphics.FromImage(b);

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            int i, j;
            Bitmap b1 = PrepareArrowBitmap(head, col, 0, (int)Math.Round(WidthForHalfDiag), (float)(-225.0 * Math.PI / 180.0), out i);

            //g.DrawImage(b1, new Rectangle(b.Width / 2, 0, b.Width / 2, b.Height / 2), 0, b1.Height / 2, b1.Width / 2, b1.Height / 2, GraphicsUnit.Pixel);
            //b1.Dispose();
            //b1 = PrepareArrowBitmap(tail, col, 0, (int)Math.Round(WidthForHalfDiag), (float)(-45.0 * Math.PI / 180.0), out j);
            //g.DrawImage(b1, new Rectangle(0, b.Height / 2, b.Width / 2, b.Height / 2), b1.Width / 2, 0, b1.Width / 2, b1.Height / 2, GraphicsUnit.Pixel);
            //Pen p = new Pen(col, 2);
            //g.DrawLine(p, (1F - .25F * i / 150.0F) * b.Width, (.25F * i / 150.0F) * b.Height, (.25F * j / 150.0F) * b.Width, (1F - .25F * j / 150.0F) * b.Height);

            g.DrawImage(b1, new Rectangle((int)(.375 * b.Width), 0, (int)(.75 * b.Width), (int)(.75 * b.Height)), 0, (int)(.375 * b1.Height), (int)(.75 * b1.Width), (int)(.75 * b1.Height), GraphicsUnit.Pixel);
            b1.Dispose();

            b1 = PrepareArrowBitmap(tail, col, 0, (int)Math.Round(WidthForHalfDiag), (float)(-45.0 * Math.PI / 180.0), out j);
            g.DrawImage(b1, new Rectangle(0, (int)(.375 * b.Height), (int)(.75 * b.Width), (int)(.75 * b.Height)), (int)(.375 * b1.Width), 0, (int)(.75 * b1.Width), (int)(.75 * b1.Height), GraphicsUnit.Pixel);

            Pen p = new Pen(col, 2);
            g.DrawLine(p,
                (1F - .25F * i / 150.0F) * b.Width, (.25F * i / 150.0F) * b.Height,
                (.25F * j / 150.0F) * b.Width, (1F - .25F * j / 150.0F) * b.Height);

            b1.Dispose();
            g.Dispose();
            return b;
        }

        private void SetButtonPosition(Button previous, Button current, int spacing, int Orient = -1)
        {
            if (Orient < Orientation.min)
                Orient = Root.ToolbarOrientation;

            if (Orient == Orientation.toLeft)
            {
                current.Left = previous.Left + previous.Width + spacing;
                current.Top = previous.Top;
            }
            else if (Orient == Orientation.toRight)
            {
                current.Left = previous.Left - spacing - current.Width;
                current.Top = previous.Top;
            }
            else if (Orient == Orientation.toDown)
            {
                current.Left = previous.Left;
                current.Top = previous.Top - spacing - current.Height;
            }
            else if (Orient == Orientation.toUp)
            {
                current.Left = previous.Left;
                current.Top = previous.Top + previous.Height + spacing;
            }
        }

        private void SetSmallButtonNext(Button previous, Button current, int incr, int Orient = -1)
        {
            if (Orient < Orientation.min)
                Orient = Root.ToolbarOrientation;

            if (Orient <= Orientation.Horizontal)
            {
                current.Left = previous.Left;
                current.Top = previous.Top + incr;
            }
            else
            {
                current.Left = previous.Left + incr;
                current.Top = previous.Top;
            }
        }

        public Bitmap buildPenIcon(Color col, int transparency, bool Sel, bool Fading, string LineStyle = "Stroke", float width = 100.0F)
        {
            Bitmap fg, img, Overlay;
            ImageAttributes imageAttributes = new ImageAttributes();
            bool Highlighter = transparency >= 100;
            bool Large = width >= (Root.PenWidthNormal + Root.PenWidthThick) / 2;

            float[][] colorMatrixElements = {
                new float[] { col.R / 255.0f, 0, 0, 0, 0 },
                new float[] { 0, col.G / 255.0f, 0, 0, 0 },
                new float[] { 0, 0, col.B / 255.0f, 0, 0 },
                new float[] { 0, 0, 0, (255 - transparency) / 255.0f, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            };
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            img = getImgFromDiskOrRes((Highlighter ? "Lpen" : (Large ? "PRpen" : "pen")) + (Sel ? "S" : "") + "_bg", ImageExts);
            fg = getImgFromDiskOrRes((Highlighter ? "Lpen" : (Large ? "PRpen" : "pen")) + (Sel ? "S" : "") + "_col", ImageExts);

            Graphics g = Graphics.FromImage(img);
            g.DrawImage(fg, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imageAttributes);

            Overlay = getImgFromDiskOrRes("fadingTag", ImageExts);
            if (Fading)
                g.DrawImage(Overlay, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);

            Overlay.Dispose();
            Overlay = getImgFromDiskOrRes(LineStyle + "LSTag", ImageExts);
            g.DrawImage(Overlay, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            Overlay.Dispose();
            fg.Dispose();
            return img;
        }

        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        /// Create a cursor from a bitmap, with the hot spot in the middle
        public static System.Windows.Forms.Cursor CreateCursorFromBitmap(Bitmap bmp, int hotX = -1, int hotY = -1)
        {
            IntPtr ptr = (bmp).GetHicon();
            IconInfo tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = hotX >= 0 ? hotX : (bmp.Width / 2);
            tmp.yHotspot = hotY >= 0 ? hotY : (bmp.Height / 2);
            tmp.fIcon = false;
            ptr = CreateIconIndirect(ref tmp);
            System.Windows.Forms.Cursor cu = new System.Windows.Forms.Cursor(ptr);
            cu.Tag = 2;
            return cu;
        }

        public Bitmap buildColorPicker(Color col, int transparency)
        {
            Bitmap img, dest;
            ImageAttributes imageAttributes = new ImageAttributes();

            float[][] colorMatrixElements = {
                new float[] { col.R / 255.0f, 0, 0, 0, 0 },
                new float[] { 0, col.G / 255.0f, 0, 0, 0 },
                new float[] { 0, 0, col.B / 255.0f, 0, 0 },
                new float[] { 0, 0, 0, (255 - transparency) / 255.0f, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            };
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            img = getImgFromDiskOrRes("picker", ImageExts);
            dest = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppPArgb);
            Graphics g = Graphics.FromImage(dest);
            g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imageAttributes);
            img.Dispose();
            return dest;
        }

        public FormCollection(Root root)
        {
            Root = root;

            /* // Kept for debug if required
            using (StreamWriter sw = File.AppendText("LogKey.txt"))
                sw.WriteLine("Start inking");
            */

            //Console.WriteLine("A=" + (DateTime.Now.Ticks/1e7).ToString());
            InitializeComponent();

            //Console.WriteLine("B=" + (DateTime.Now.Ticks/1e7).ToString());
            ClipartsDlg = new ImageLister(Root);
            Initializing = true;

            int nbPen = 0;
            for (int b = 0; b < Root.MaxDisplayedPens; b++)
                if (Root.PenEnabled[b])
                    nbPen++;
            btPen = new Button[Root.MaxPenCount];

            for (int b = 0; b < Root.MaxDisplayedPens; b++)
            {
                btPen[b] = new Button();
                btPen[b].Name = string.Format("pen{0}", b);
                btPen[b].FlatAppearance.BorderSize = 0;
                btPen[b].FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
                btPen[b].FlatStyle = System.Windows.Forms.FlatStyle.Flat;

                btPen[b].ContextMenu = new ContextMenu();
                btPen[b].ContextMenu.Popup += new System.EventHandler(btColor_Click);
                btPen[b].Click += new System.EventHandler(btColor_Click);

                btPen[b].BackColor = System.Drawing.Color.Transparent;
                btPen[b].BackgroundImageLayout = ImageLayout.Stretch;
                this.toolTip.SetToolTip(this.btPen[b], Root.Local.ButtonNamePen[b] + " (" + Root.Hotkey_Pens[b].ToString() + ")");

                btPen[b].MouseDown += gpButtons_MouseDown;
                btPen[b].MouseMove += gpButtons_MouseMove;
                btPen[b].MouseUp += gpButtons_MouseUp;

                gpButtons.Controls.Add(btPen[b]);
            }

            IC = new InkOverlay(this.Handle);
            Console.WriteLine("Module of IC " + IC.GetType().Module.FullyQualifiedName);
            IC.CollectionMode = CollectionMode.InkOnly;
            IC.AutoRedraw = false;
            IC.DynamicRendering = false;
            IC.EraserMode = InkOverlayEraserMode.StrokeErase;
            IC.CursorInRange += IC_CursorInRange;
            IC.MouseDown += IC_MouseDown;
            IC.MouseMove += IC_MouseMove;
            IC.MouseUp += IC_MouseUp;
            IC.CursorDown += IC_CursorDown;
            IC.MouseWheel += IC_MouseWheel;
            IC.Stroke += IC_Stroke;

            StrokesSelection = IC.Ink.CreateStrokes();

            foreach (Control ct in gpButtons.Controls)
            {
                if (ct.GetType() == typeof(Button))
                {
                    ct.MouseDown += new MouseEventHandler(this.btAllButtons_MouseDown);
                    ct.MouseUp += new MouseEventHandler(this.btAllButtons_MouseUp);
                    ct.ContextMenu = new ContextMenu();
                    ct.ContextMenu.Popup += new EventHandler(this.btAllButtons_RightClick);
                }
            }
            PenModifyDlg = new PenModifyDlg(Root); // It seems to be a little long to build so we prepare it.

            Btn_SubTools = new Button[] { Btn_SubTool0, Btn_SubTool1, Btn_SubTool2, Btn_SubTool3, Btn_SubTool4, Btn_SubTool5, Btn_SubTool6, Btn_SubTool7 };

            ClipartsDlg.Initialize();
            Initialize();
        }

        public double ConvertMeasureLength(double hl)
        {
            return hl * 0.037795280352161 * Measure2Scale;
        }

        private string MemoHintClose;
        private string MemoHintDock;

        // Calcule dynamiquement les dimensions de la barre d'outils
        private void ComputeToolbarDimensions(
            out int dim,
            out int dim1,
            out int dim1s,
            out int dim2,
            out int dim2s,
            out int dim3,
            out int dim4,
            out int dim4s)
        {
            // Liste des outils à ne pas afficher dans la barre d'outils
            int[] hiddenTools =
            {
                Tools.LetterTag,   // Masquer "Ajout de lettres"
                Tools.SquareTag,   // Masquer "Ajout de carrés"
                Tools.TriangleTag, // Masquer "Ajout de triangles"
                Tools.CircleTag,   // Masquer "Ajout de cercles"
                Tools.CrossTag     // Masquer "Ajout de croix"
            };

            // DPI scale safe try
            float dpiScale = 1.0f;
            try
            {
                using (Graphics g = this.CreateGraphics())
                    dpiScale = g.DpiY / 96f;
            }
            catch
            {
                dpiScale = 1.0f;
            }

            // taille de référence basée sur la hauteur de l'écran et l'option ToolbarHeight
            dim = (int)Math.Round(Screen.PrimaryScreen.Bounds.Height * Root.ToolbarHeight * dpiScale);
            dim = Math.Max(40, dim); // minimum raisonnable

            dim1 = (int)(dim * NormSizePercent);
            dim1s = (int)(dim * SmallSizePercent);
            dim2 = (int)(dim * TopPercent);
            dim2s = (int)(dim * SmallButtonNext);
            dim3 = Math.Max(1, (int)Math.Round(dim * InterButtonGap));
            dim4 = dim1 + dim3;
            dim4s = dim1s + dim3;

            // Estimation de l'espace nécessaire selon le contenu actif (nombre de stylos, boutons activés)
            int nbPen = 0;
            for (int b = 0; b < Root.MaxDisplayedPens; b++)
                if (Root.PenEnabled[b]) nbPen++;

            int penSec = Root.PensOnTwoLines ? ((int)Math.Ceiling(nbPen / 2.0) * dim4s) : (nbPen * dim4);

            int contentLength = (int)((dim1 * .5 + dim3) + (penSec
                + (Root.PensExtraSet ? (dim4 / 6) : 0)
                + (Root.ToolsEnabled ? (6 * dim4s) : 0)
                + (Root.EraserEnabled ? dim4 : 0)
                + (Root.PanEnabled ? 2 * dim4s : 0)
                + (Root.PointerEnabled ? dim4 : 0)
                + (Root.PenWidthEnabled ? dim4 : 0)
                + (Root.InkVisibleEnabled ? dim4 : 0)
                + (Root.ZoomEnabled > 0 ? dim4s : 0)
                + (Root.SnapEnabled ? dim4 : 0)
                + (Root.UndoEnabled ? dim4 : 0)
                + (Root.ClearEnabled ? dim4 : 0)
                + (Root.PagesEnabled ? dim4s : 0)
                + (Root.LoadSaveEnabled ? dim4s : 0)
                + ((Root.VideoRecordMode != VideoRecordMode.NoVideo) ? dim4 : 0)
                + dim1));

            // limite pour ne pas dépasser la zone utile écran
            // -> utiliser le bureau virtuel (tous écrans) pour autoriser une barre plus large sur multi‑moniteurs
            int maxMain = (Root.ToolbarOrientation <= Orientation.Horizontal)
                ? SystemInformation.VirtualScreen.Width - 40
                : SystemInformation.VirtualScreen.Height - 40;

            if (contentLength > maxMain && maxMain > 0)
            {
                float scale = (float)maxMain / contentLength;
                dim1 = Math.Max(12, (int)(dim1 * scale));
                dim1s = Math.Max(10, (int)(dim1s * scale));
                dim3 = Math.Max(1, (int)(dim3 * scale));
                dim4 = dim1 + dim3;
                dim4s = dim1s + dim3;
            }

            // garanties minimales
            dim = Math.Max(32, dim);
            dim1 = Math.Max(12, dim1);
            dim1s = Math.Max(10, dim1s);
        }

        // Ajuste la taille réelle de la barre après création/positionnement de tous les boutons.
        // Corrige la troncation quand le calcul théorique sous-estime la largeur/hauteur.
        private void AdjustToolbarSize()
        {
            if (gpButtons == null) return;
            int maxRight = 0, maxBottom = 0;
            foreach (Control c in gpButtons.Controls)
            {
                if (!c.Visible) continue;
                if (c.Right > maxRight) maxRight = c.Right;
                if (c.Bottom > maxBottom) maxBottom = c.Bottom;
            }
            const int margin = 2; // petite marge de respiration

            if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                int neededWidth = maxRight + margin;
                if (neededWidth > gpButtons.Width)
                    gpButtons.Width = neededWidth;
                int neededHeight = maxBottom + margin;
                if (neededHeight > gpButtons.Height)
                    gpButtons.Height = neededHeight;
            }
            else
            {
                int neededHeight = maxBottom + margin;
                if (neededHeight > gpButtons.Height)
                    gpButtons.Height = neededHeight;
                int neededWidth = maxRight + margin;
                if (neededWidth > gpButtons.Width)
                    gpButtons.Width = neededWidth;
            }
        }

        // Recalage post-création : ajuste la taille réelle et synchronise les variables internes.
        private void FixToolbarSizeForNumberTag()
        {
            if (gpButtons == null) return;

            int maxRight = 0, maxBottom = 0;
            foreach (Control c in gpButtons.Controls)
            {
                if (!c.Visible) continue;
                if (c.Right > maxRight) maxRight = c.Right;
                if (c.Bottom > maxBottom) maxBottom = c.Bottom;
            }
            const int margin = 2;

            if (maxRight + margin > gpButtons.Width)
                gpButtons.Width = maxRight + margin;
            if (maxBottom + margin > gpButtons.Height)
                gpButtons.Height = maxBottom + margin;

            // Synchronise les valeurs utilisées par l'animation / pliage
            gpButtonsWidth = gpButtons.Width;
            gpButtonsHeight = gpButtons.Height;
            VisibleToolbar.Width = gpButtonsWidth;
            VisibleToolbar.Height = gpButtonsHeight;
        }
        public void Initialize()
        {
            // accélère/ralentit l'animation : plus petit => plus rapide
            try { tiSlide.Interval = 0; } catch { }

            if (Root.FormOptions?.Visible ?? false)
            {
                // this is to validate the active field if the options are open. Not the best solution but nothing else found 
                Root.FormOptions.Close();
                Root.FormOptions.Show();
            }

            Console.WriteLine("A=" + (DateTime.Now.Ticks / 1e7).ToString());

            MeasureNumberFormat = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
            MeasureNumberFormat.NumberDecimalDigits = Root.Measure2Digits;
            Measure2Scale = Root.Measure2Scale;

            for (int i = 0; i < StoredArrowImages.Count; i++)
                try { StoredArrowImages[i].Dispose(); } catch { }
            StoredArrowImages.Clear();

            Root.Snapping = 0;
            Root.ColorPickerMode = false;
            Root.PenAttr[Root.SavedPenDA] = null;
            StrokesSelection.Clear();
            FadingList.Clear();

            Animations.Clear();
            AniPoolIdx = 0;

            if (Root.WindowRect.Width <= 0 || Root.WindowRect.Height <= 0)
            {
                this.Left = SystemInformation.VirtualScreen.Left;
                this.Top = SystemInformation.VirtualScreen.Top;
                this.Width = SystemInformation.VirtualScreen.Width;
                this.Height = SystemInformation.VirtualScreen.Height - 2;
                PrimaryLeft = Screen.PrimaryScreen.Bounds.Left - SystemInformation.VirtualScreen.Left;
                PrimaryTop = Screen.PrimaryScreen.Bounds.Top - SystemInformation.VirtualScreen.Top;
            }
            else // window mode
            {
                this.Left = Math.Min(Math.Max(SystemInformation.VirtualScreen.Left, Root.WindowRect.Left), SystemInformation.VirtualScreen.Right - Root.WindowRect.Width);
                this.Top = Math.Min(Math.Max(SystemInformation.VirtualScreen.Top, Root.WindowRect.Top), SystemInformation.VirtualScreen.Bottom - Root.WindowRect.Height);
                this.Width = Root.WindowRect.Width;
                this.Height = Root.WindowRect.Height;
                PrimaryLeft = 0; // top corner: Screen.PrimaryScreen.Bounds.Left - SystemInformation.VirtualScreen.Left;
                PrimaryTop = 0;  //             Screen.PrimaryScreen.Bounds.Top - SystemInformation.VirtualScreen.Top;
            }

            try { ZoomImage?.Dispose(); }
            catch { }
            finally { ZoomImage = new Bitmap(Root.ZoomWidth, Root.ZoomHeight); }

            try { ZoomImage2?.Dispose(); }
            catch { }
            finally { ZoomImage2 = new Bitmap(Root.ZoomWidth, Root.ZoomHeight); }

            ZoomForm.pictureBox1.BackgroundImage = ZoomImage;
            ZoomForm.pictureBox2.BackgroundImage = ZoomImage2;
            ZoomFormRePosX = ZoomImage.Width / 2;
            ZoomFormRePosY = ZoomImage.Height / 2;
            ZoomSaveStroke = Path.GetFullPath(Environment.ExpandEnvironmentVariables("%temp%/ZoomSave.strokes.txt")).Replace('\\', '/');

            // loading default params
            TextFont = Root.TextFont;
            TextBold = Root.TagBold;
            TextItalic = Root.TagItalic;
            TextSize = Root.TextSize;
            TagFont = Root.TagFont;
            TagBold = Root.TagBold;
            TagItalic = Root.TagItalic;
            TagSize = Root.TagSize;

            gpButtons.BackColor = Color.FromArgb(Root.ToolbarBGColor[0], Root.ToolbarBGColor[1], Root.ToolbarBGColor[2], Root.ToolbarBGColor[3]);
            gpPenWidth.BackColor = Color.FromArgb(Root.ToolbarBGColor[0], Root.ToolbarBGColor[1], Root.ToolbarBGColor[2], Root.ToolbarBGColor[3]);
            gpSubTools.BackColor = Color.FromArgb(Root.ToolbarBGColor[0], Root.ToolbarBGColor[1], Root.ToolbarBGColor[2], Root.ToolbarBGColor[3]);

            longClickTimer.Interval = (int)(Root.LongClickTime * 1000 + 100);

            int nbPen = 0;
            for (int b = 0; b < Root.MaxDisplayedPens; b++)
                if (Root.PenEnabled[b])
                    nbPen++;

            FirstPenDisplayed = 0;
            while (!Root.PenEnabled[FirstPenDisplayed])
                FirstPenDisplayed++;
            oldShiftPensExtra = null;

            // set dimensions and positions (remplacé par calcul dynamique)
            int dim, dim1, dim1s, dim2, dim2s, dim3, dim4, dim4s;
            ComputeToolbarDimensions(out dim, out dim1, out dim1s, out dim2, out dim2s, out dim3, out dim4, out dim4s);

            int penSec = Root.PensOnTwoLines ? ((int)Math.Ceiling(nbPen / 2.0) * dim4s) : (nbPen * dim4);
            if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                gpButtons.Height = dim;
                gpButtons.Width =
                    (int)((dim1 * .5 + dim3) +
                          (penSec + (Root.PensExtraSet ? (dim4 / 6) : 0) + (Root.ToolsEnabled ? (6 * dim4s) : 0) + (Root.EraserEnabled ? dim4 : 0) + (Root.PanEnabled ? 2 * dim4s : 0)
                           + (Root.PointerEnabled ? dim4 : 0) + (Root.PenWidthEnabled ? dim4 : 0) + (Root.InkVisibleEnabled ? dim4 : 0) + (Root.ZoomEnabled > 0 ? dim4s : 0)
                           + (Root.SnapEnabled ? dim4 : 0) + (Root.UndoEnabled ? dim4 : 0) + (Root.ClearEnabled ? dim4 : 0)
                           + (Root.PagesEnabled ? dim4s : 0) + (Root.LoadSaveEnabled ? dim4s : 0)
                           + ((Root.VideoRecordMode != VideoRecordMode.NoVideo) ? dim4 : 0)
                           + dim1));
            }
            else // Vertical
            {
                gpButtons.Width = dim;
                gpButtons.Height =
                    (int)((dim1 * .5 + dim3) +
                          (penSec + (Root.PensExtraSet ? (dim4 / 6) : 0) + (Root.ToolsEnabled ? (6 * dim4s) : 0) + (Root.EraserEnabled ? dim4 : 0) + (Root.PanEnabled ? 2 * dim4s : 0)
                           + (Root.PointerEnabled ? dim4 : 0) + (Root.PenWidthEnabled ? dim4 : 0) + (Root.InkVisibleEnabled ? dim4 : 0) + (Root.ZoomEnabled > 0 ? dim4s : 0)
                           + (Root.SnapEnabled ? dim4 : 0) + (Root.UndoEnabled ? dim4 : 0) + (Root.ClearEnabled ? dim4 : 0)
                           + (Root.PagesEnabled ? dim4s : 0) + (Root.LoadSaveEnabled ? dim4s : 0)
                           + ((Root.VideoRecordMode != VideoRecordMode.NoVideo) ? dim4 : 0)
                           + dim1));
            }

            if (Root.ToolbarOrientation == Orientation.toLeft)
            {
                btDock.Height = dim1;
                btDock.Width = dim1 / 2;
                btDock.BackgroundImage = getImgFromDiskOrRes(Root.Docked ? "dockback" : "dock");
                btDock.Top = dim2;
                btDock.Left = 0;
            }
            else if (Root.ToolbarOrientation == Orientation.toRight)
            {
                btDock.Height = dim1;
                btDock.Width = dim1 / 2;
                btDock.BackgroundImage = getImgFromDiskOrRes(!Root.Docked ? "dockback" : "dock");
                btDock.Top = dim2;
                btDock.Left = gpButtons.Width - btDock.Width;
            }
            else if (Root.ToolbarOrientation == Orientation.toDown)
            {
                btDock.Width = dim1;
                btDock.Height = dim1 / 2;
                btDock.BackgroundImage = getImgFromDiskOrRes(!Root.Docked ? "dockbackV" : "dockV");
                btDock.Top = gpButtons.Height - btDock.Height;
                btDock.Left = dim2;
            }
            else if (Root.ToolbarOrientation == Orientation.toUp)
            {
                btDock.Width = dim1;
                btDock.Height = dim1 / 2;
                btDock.BackgroundImage = getImgFromDiskOrRes(Root.Docked ? "dockbackV" : "dockV");
                btDock.Top = 0;
                btDock.Left = dim2;
            }

            Button prev = btDock;
            bool NextBelow = false;

            for (int b = 0; b < Root.MaxDisplayedPens; b++)
            {
                if (Root.PenEnabled[b])
                {
                    if (Root.PensOnTwoLines)
                    {
                        btPen[b].Width = dim1s;
                        btPen[b].Height = dim1s;

                        if (NextBelow)
                        {
                            SetSmallButtonNext(prev, btPen[b], dim2s);
                            NextBelow = false;
                        }
                        else
                        {
                            SetButtonPosition(prev, btPen[b], dim3);
                            prev = btPen[b];
                            NextBelow = true;
                        }
                    }
                    else
                    {
                        btPen[b].Width = dim1;
                        btPen[b].Height = dim1;

                        SetButtonPosition(prev, btPen[b], dim3);
                        prev = btPen[b];
                    }

                    toolTip.SetToolTip(btPen[b], Root.Local.ButtonNamePen[b] + " (" + Root.Hotkey_Pens[b].ToString() + ")");
                    btPen[b].Visible = true;
                }
                else
                {
                    btPen[b].Visible = false;
                }
            }

            if (Root.PensExtraSet)
            {
                btExtraPens.Visible = true;
                if (Root.ToolbarOrientation == Orientation.toUp || Root.ToolbarOrientation == Orientation.toDown)
                {
                    btExtraPens.Height = dim1 / 6;
                    btExtraPens.Width = dim1;
                    btExtraPens.BackgroundImage = getImgFromDiskOrRes("ExtraPensV");
                }
                else
                {
                    btExtraPens.Height = dim1;
                    btExtraPens.Width = dim1 / 6;
                    btExtraPens.BackgroundImage = getImgFromDiskOrRes("ExtraPens");
                }
                SetButtonPosition(prev, btExtraPens, dim3);
                prev = btExtraPens;
            }
            else
            {
                btExtraPens.Visible = false;
            }

            if (Root.ToolsEnabled)
            {
                // background images loaded/applied in SelectTool
                btHand.Height = dim1s;
                btHand.Width = dim1s;
                btHand.Visible = true;
                SetButtonPosition(prev, btHand, dim3);
                prev = btHand;

                // sécurise l'ajout / configuration de btHandWhite / btHandBlack
                bool createdWhite = false;
                if (btHandWhite == null)
                {
                    btHandWhite = new Button();
                    createdWhite = true;
                }
                btHandWhite.Name = "btHandWhite";
                btHandWhite.FlatAppearance.BorderSize = 0;
                btHandWhite.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
                btHandWhite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                btHandWhite.BackColor = System.Drawing.Color.Transparent;
                btHandWhite.BackgroundImageLayout = ImageLayout.Stretch;
                btHandWhite.Height = dim1s;
                btHandWhite.Width = dim1s;
                btHandWhite.Visible = true;
                btHandWhite.BackgroundImage = getImgFromDiskOrRes("tool_hand_filledW", ImageExts);
                SetButtonPosition(prev, btHandWhite, dim3);
                btHandWhite.Click -= btTool_Click; btHandWhite.Click += btTool_Click;
                btHandWhite.MouseDown -= btAllButtons_MouseDown; btHandWhite.MouseDown += btAllButtons_MouseDown;
                btHandWhite.MouseUp -= btAllButtons_MouseUp; btHandWhite.MouseUp += btAllButtons_MouseUp;
                btHandWhite.MouseMove -= gpButtons_MouseMove; btHandWhite.MouseMove += gpButtons_MouseMove;
                if (btHandWhite.ContextMenu == null) btHandWhite.ContextMenu = new ContextMenu();
                btHandWhite.ContextMenu.Popup -= btAllButtons_RightClick; btHandWhite.ContextMenu.Popup += btAllButtons_RightClick;
                this.toolTip.SetToolTip(this.btHandWhite, Root.Local.ButtonNameHandWhite ?? (Root.Local.ButtonNameHand + " — White"));
                if (createdWhite) gpButtons.Controls.Add(btHandWhite);
                btHandWhite.BringToFront();

                bool createdBlack = false;
                if (btHandBlack == null)
                {
                    btHandBlack = new Button();
                    createdBlack = true;
                }
                btHandBlack.Name = "btHandBlack";
                btHandBlack.FlatAppearance.BorderSize = 0;
                btHandBlack.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
                btHandBlack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                btHandBlack.BackColor = System.Drawing.Color.Transparent;
                btHandBlack.BackgroundImageLayout = ImageLayout.Stretch;
                btHandBlack.Height = dim1s;
                btHandBlack.Width = dim1s;
                btHandBlack.Visible = true;
                btHandBlack.BackgroundImage = getImgFromDiskOrRes("tool_hand_filledB", ImageExts);
                SetSmallButtonNext(btHandWhite, btHandBlack, dim2s);
                btHandBlack.Click -= btTool_Click; btHandBlack.Click += btTool_Click;
                btHandBlack.MouseDown -= btAllButtons_MouseDown; btHandBlack.MouseDown += btAllButtons_MouseDown;
                btHandBlack.MouseUp -= btAllButtons_MouseUp; btHandBlack.MouseUp += btAllButtons_MouseUp;
                btHandBlack.MouseMove -= gpButtons_MouseMove; btHandBlack.MouseMove += gpButtons_MouseMove;
                if (btHandBlack.ContextMenu == null) btHandBlack.ContextMenu = new ContextMenu();
                btHandBlack.ContextMenu.Popup -= btAllButtons_RightClick; btHandBlack.ContextMenu.Popup += btAllButtons_RightClick;
                this.toolTip.SetToolTip(this.btHandBlack, Root.Local.ButtonNameHandBlack ?? (Root.Local.ButtonNameHand + " — Black"));
                if (createdBlack) gpButtons.Controls.Add(btHandBlack);
                btHandWhite.BringToFront();
                btHandBlack.BringToFront();

                if (gpButtons.Width < btHandWhite.Right + dim1s)
                    gpButtons.Width = btHandWhite.Right + dim1s + dim3;
                if (gpButtons.Height < btHandBlack.Bottom + dim3)
                    gpButtons.Height = btHandBlack.Bottom + dim3;

                btHandWhite.BringToFront();
                btHandBlack.BringToFront();

                // IMPORTANT : on conserve la colonne “main” alignée sur le bouton du HAUT
                prev = btHandWhite;

                this.toolTip.SetToolTip(this.btLetter, Root.Local.ButtonNameLetterTag);
                this.toolTip.SetToolTip(this.btSquare, Root.Local.ButtonNameSquareTag);
                this.toolTip.SetToolTip(this.btTriangle, Root.Local.ButtonNameTriangleTag);
                this.toolTip.SetToolTip(this.btCircle, Root.Local.ButtonNameCircleTag);
                this.toolTip.SetToolTip(this.btCross, Root.Local.ButtonNameCrossTag);

                btLine.Height = dim1s;
                btLine.Width = dim1s;
                btLine.Visible = true;
                SetButtonPosition(prev, btLine, dim3);
                btHandBlack.BringToFront();
                btHandWhite.BringToFront();

                btRect.Height = dim1s;
                btRect.Width = dim1s;
                btRect.Visible = true;
                SetButtonPosition(prev, btRect, dim3);

                btOval.Height = dim1s;
                btOval.Width = dim1s;
                btOval.Visible = true;
                SetSmallButtonNext(btRect, btOval, dim2s);

                btArrow.Height = dim1s;
                btArrow.Width = dim1s;
                btArrow.Visible = true;
                SetButtonPosition(btRect, btArrow, dim3);

                // 4 boutons NumberTag (2x2) — utilise une routine existante
                Button lastNumBtn = CreateNumberTagButtons(dim1s, dim2s, btArrow);

                // Masquer explicitement l’ancien btNumb
                try
                {
                    btNumb.Visible = false;
                    btNumb.BackgroundImage?.Dispose();
                    btNumb.BackgroundImage = null;
                }
                catch { }

                // Placer btText après la série NumberTag
                btText.Height = dim1s;
                btText.Width = dim1s;
                btText.Visible = true;
                SetButtonPosition(lastNumBtn, btText, dim3);

                // Nouveaux outils tags
                btLetter.Width = dim1s; btLetter.Height = dim1s; btLetter.Visible = true;
                SetButtonPosition(btText, btLetter, dim3);

                btSquare.Width = dim1s; btSquare.Height = dim1s; btSquare.Visible = true;
                SetSmallButtonNext(btLetter, btSquare, dim2s);

                btTriangle.Width = dim1s; btTriangle.Height = dim1s; btTriangle.Visible = true;
                SetButtonPosition(btLetter, btTriangle, dim3);

                btCircle.Width = dim1s; btCircle.Height = dim1s; btCircle.Visible = true;
                SetSmallButtonNext(btTriangle, btCircle, dim2s);

                btCross.Width = dim1s; btCross.Height = dim1s; btCross.Visible = true;
                SetButtonPosition(btTriangle, btCross, dim3);

                // Positionnement "fallback"
                try
                {
                    int spacing = 6;
                    int bw = (btNumb != null) ? btNumb.Width : 46;
                    int bh = (btNumb != null) ? btNumb.Height : 46;
                    int top = (btNumb != null) ? btNumb.Top : 3;
                    int left = (btNumb != null) ? btNumb.Right + spacing : (btText != null ? btText.Left - (bw + spacing) : 580);

                    btLetter.Size = new Size(bw, bh);
                    btLetter.Left = left;
                    btLetter.Top = top;
                    btLetter.Visible = true;
                    toolTip.SetToolTip(btLetter, Root.Local?.ButtonNameLetterTag ?? "Letter");

                    btSquare.Size = new Size(bw, bh);
                    btSquare.Left = btLetter.Right + spacing;
                    btSquare.Top = top;
                    btSquare.Visible = true;
                    toolTip.SetToolTip(btSquare, Root.Local?.ButtonNameSquareTag ?? "Square");

                    btTriangle.Size = new Size(bw, bh);
                    btTriangle.Left = btSquare.Right + spacing;
                    btTriangle.Top = top;
                    btTriangle.Visible = true;
                    toolTip.SetToolTip(btTriangle, Root.Local?.ButtonNameTriangleTag ?? "Triangle");

                    btCircle.Size = new Size(bw, bh);
                    btCircle.Left = btTriangle.Right + spacing;
                    btCircle.Top = top;
                    btCircle.Visible = true;
                    toolTip.SetToolTip(btCircle, Root.Local?.ButtonNameCircleTag ?? "Circle");

                    btCross.Size = new Size(bw, bh);
                    btCross.Left = btCircle.Right + spacing;
                    btCross.Top = top;
                    btCross.Visible = true;
                    toolTip.SetToolTip(btCross, Root.Local?.ButtonNameCrossTag ?? "Cross");
                }
                catch { }

                btClipArt.Height = dim1s;
                btClipArt.Width = dim1s;
                btClipArt.Visible = true;
                btClipArt.Text = "";
                btClipArt.Font = new Font(btClipArt.Font.Name, dim1s * .5F, btClipArt.Font.Style);
                SetButtonPosition(btText, btClipArt, dim3);

                btClip1.Height = dim1s;
                btClip1.Width = dim1s;
                btClip1.Visible = true;
                btClip1.Text = "";
                btClip1.Font = btClipArt.Font;
                SetSmallButtonNext(btClipArt, btClip1, dim2s);
                try
                {
                    if ((btClip1.Tag as ClipArtData)?.ImageStamp != Root.ImageStamp1.ImageStamp)
                    {
                        btClip1.BackgroundImage.Dispose();
                        throw (new Exception("Renew button"));
                    }
                }
                catch
                {
                    btClip1.BackgroundImage = getImgFromDiskOrRes(Root.ImageStamp1.ImageStamp, ImageExts);
                    btClip1.Tag = Root.ImageStamp1.Clone();
                }

                btClip2.Height = dim1s;
                btClip2.Width = dim1s;
                btClip2.Visible = true;
                btClip2.Text = "";
                btClip2.Font = btClipArt.Font;
                SetButtonPosition(btClipArt, btClip2, dim3);
                try
                {
                    if ((btClip2.Tag as ClipArtData)?.ImageStamp != Root.ImageStamp2.ImageStamp)
                    {
                        btClip2.BackgroundImage.Dispose();
                        throw (new Exception("Renew button"));
                    }
                }
                catch
                {
                    btClip2.BackgroundImage = getImgFromDiskOrRes(Root.ImageStamp2.ImageStamp, ImageExts);
                    btClip2.Tag = Root.ImageStamp2.Clone();
                }

                btClip3.Height = dim1s;
                btClip3.Width = dim1s;
                btClip3.Visible = true;
                btClip3.Text = "";
                btClip3.Font = btClipArt.Font;
                SetSmallButtonNext(btClip2, btClip3, dim2s);
                try
                {
                    if ((btClip3.Tag as ClipArtData)?.ImageStamp != Root.ImageStamp3.ImageStamp)
                    {
                        btClip3.BackgroundImage.Dispose();
                        throw (new Exception("Renew button"));
                    }
                }
                catch
                {
                    btClip3.BackgroundImage = getImgFromDiskOrRes(Root.ImageStamp3.ImageStamp, ImageExts);
                    btClip3.Tag = Root.ImageStamp3.Clone();
                }

                prev = btClip2;
                AdjustToolbarSize(); // Recalage final après insertion des nouveaux outils + colonnes
            }
            else
            {
                btHand.Visible = false;
                btLine.Visible = false;
                btRect.Visible = false;
                btOval.Visible = false;
                btArrow.Visible = false;
                btNumb.Visible = false;
                btText.Visible = false;

                btClipArt.Visible = false;
                btClip1.Visible = false;
                btClip2.Visible = false;
                btClip3.Visible = false;
            }

            if (Root.EraserEnabled)
            {
                btEraser.Height = dim1;
                btEraser.Width = dim1;
                btEraser.Visible = true;
                image_eraser_act = getImgFromDiskOrRes("eraser_act", ImageExts);
                image_eraser = getImgFromDiskOrRes("eraser", ImageExts);
                btEraser.BackgroundImage = image_eraser;
                SetButtonPosition(prev, btEraser, dim3);
                prev = btEraser;
            }
            else
            {
                btEraser.Visible = false;
            }

            if (Root.PanEnabled)
            {
                btLasso.Height = dim1s;
                btLasso.Width = dim1s;
                btLasso.Visible = true;
                image_lasso_act = getImgFromDiskOrRes("lasso_act", ImageExts);
                image_lasso = getImgFromDiskOrRes("lasso", ImageExts);
                btLasso.BackgroundImage = image_lasso;
                SetButtonPosition(prev, btLasso, dim3);
                prev = btLasso;

                btPan.Height = dim1s;
                btPan.Width = dim1s;
                btPan.Visible = true;
                btPan.BackgroundImage = getImgFromDiskOrRes("pan", ImageExts);
                SetSmallButtonNext(prev, btPan, dim2s);

                btEdit.Height = dim1s;
                btEdit.Width = dim1s;
                btEdit.Visible = true;
                SetButtonPosition(prev, btEdit, dim3);
                prev = btEdit;

                btScaleRot.Height = dim1s;
                btScaleRot.Width = dim1s;
                btScaleRot.Visible = true;
                btScaleRot.BackgroundImage = getImgFromDiskOrRes("scale", ImageExts);
                SetSmallButtonNext(prev, btScaleRot, dim2s);
            }
            else
            {
                btLasso.Visible = false;
                btPan.Visible = false;
                btEdit.Visible = false;
                btScaleRot.Visible = false;
            }

            if (Root.PointerEnabled)
            {
                btPointer.Height = dim1;
                btPointer.Width = dim1;
                btPointer.Visible = true;
                image_pointer = getImgFromDiskOrRes("pointer", ImageExts);
                image_pointer_act = getImgFromDiskOrRes("pointer_act", ImageExts);
                SetButtonPosition(prev, btPointer, dim3);
                prev = btPointer;
            }
            else
            {
                btPointer.Visible = false;
            }

            if (Root.ZoomEnabled > 0)
            {
                btMagn.Height = dim1s;
                btMagn.Width = dim1s;
                btMagn.Visible = true;
                btMagn.BackgroundImage = getImgFromDiskOrRes((Root.MagneticRadius > 0) ? "Magnetic_act" : "Magnetic", ImageExts);
                SetButtonPosition(prev, btMagn, dim3);
                prev = btMagn;

                btZoom.Height = dim1s;
                btZoom.Width = dim1s;
                btZoom.Visible = true;
                btZoom.BackgroundImage = getImgFromDiskOrRes("Zoom", ImageExts);
                SetSmallButtonNext(btMagn, btZoom, dim2s);
                btZoom.Visible = true;
            }
            else
            {
                btMagn.Visible = false;
                btZoom.Visible = false;
            }

            // Grille 2x2 : PenWidth / Snap (haut), Undo / Clear (bas)
            var smallQuad = new List<(Button btn, bool enabled, Action prepare)>
            {
                (btPenWidth, Root.PenWidthEnabled, new Action(() =>
                {
                    btPenWidth.Height = dim1s; btPenWidth.Width = dim1s;
                    btPenWidth.BackgroundImage = getImgFromDiskOrRes("penwidth", ImageExts);
                })),
                (btSnap, Root.SnapEnabled, new Action(() =>
                {
                    btSnap.Height = dim1s; btSnap.Width = dim1s;
                    btSnap.BackgroundImage = getImgFromDiskOrRes("snap", ImageExts);
                })),
                (btUndo, Root.UndoEnabled, new Action(() =>
                {
                    btUndo.Height = dim1s; btUndo.Width = dim1s;
                    btUndo.BackgroundImage = getImgFromDiskOrRes("undo", ImageExts);
                })),
                (btClear, Root.ClearEnabled, new Action(() =>
                {
                    btClear.Height = dim1s; btClear.Width = dim1s;
                    btClear.BackgroundImage = getImgFromDiskOrRes("garbage", ImageExts);
                }))
            };

            var actives = smallQuad.Where(x => x.enabled).ToList();
            foreach (var x in smallQuad) x.btn.Visible = x.enabled;

            if (actives.Count > 0)
            {
                actives[0].prepare();
                SetButtonPosition(prev, actives[0].btn, dim3);

                if (actives.Count > 1)
                {
                    actives[1].prepare();
                    SetButtonPosition(actives[0].btn, actives[1].btn, dim3);
                }

                if (actives.Count > 2)
                {
                    actives[2].prepare();
                    SetSmallButtonNext(actives[0].btn, actives[2].btn, dim2s);
                }

                if (actives.Count > 3)
                {
                    actives[3].prepare();
                    var anchorTopRight = (actives.Count > 1) ? actives[1].btn : actives[0].btn;
                    SetSmallButtonNext(anchorTopRight, actives[3].btn, dim2s);
                }

                prev = (actives.Count > 1) ? actives[1].btn : actives[0].btn;
            }

            // InkVisible (placé après la grille 2x2)
            if (Root.InkVisibleEnabled)
            {
                btInkVisible.Visible = true;
                btInkVisible.Height = dim1;
                btInkVisible.Width = dim1;
                image_visible_not = getImgFromDiskOrRes("visible_not", ImageExts);
                image_visible = getImgFromDiskOrRes("visible", ImageExts);
                btInkVisible.BackgroundImage = image_visible;
                SetButtonPosition(prev, btInkVisible, dim3);
                prev = btInkVisible;
            }
            else
            {
                btInkVisible.Visible = false;
            }

            if (Root.PagesEnabled)
            {
                btPagePrev.Height = dim1s;
                btPagePrev.Width = dim1s;
                btPagePrev.Visible = true;
                btPagePrev.BackgroundImage = getImgFromDiskOrRes("PagePrev", ImageExts);
                SetButtonPosition(prev, btPagePrev, dim3);

                btPageNext.Height = dim1s;
                btPageNext.Width = dim1s;
                btPageNext.Visible = true;
                btPageNext.BackgroundImage = getImgFromDiskOrRes("PageNext", ImageExts);
                SetSmallButtonNext(btPagePrev, btPageNext, dim2s);
                prev = btPagePrev;
            }
            else
            {
                btPagePrev.Visible = false;
                btPageNext.Visible = false;
            }

            if (Root.LoadSaveEnabled)
            {
                btSave.Height = dim1s;
                btSave.Width = dim1s;
                btSave.Visible = true;
                btSave.BackgroundImage = getImgFromDiskOrRes("save", ImageExts);
                SetButtonPosition(prev, btSave, dim3);

                btLoad.Height = dim1s;
                btLoad.Width = dim1s;
                btLoad.Visible = true;
                btLoad.BackgroundImage = getImgFromDiskOrRes("open", ImageExts);
                SetSmallButtonNext(btSave, btLoad, dim2s);
                prev = btSave;
            }
            else
            {
                btSave.Visible = false;
                btLoad.Visible = false;
            }

            if (Root.VideoRecordMode != VideoRecordMode.NoVideo)
            {
                btVideo.Height = dim1;
                btVideo.Width = dim1;
                btVideo.Visible = true;
                SetButtonPosition(prev, btVideo, dim3);
                SetVidBgImage();

                if (Root.VideoRecordMode == VideoRecordMode.OBSBcst || Root.VideoRecordMode == VideoRecordMode.OBSRec)
                {
                    if (Root.ObsRecvTask == null || Root.ObsRecvTask.IsCompleted)
                    {
                        Root.VideoRecordWindowInProgress = true;
                        try { Root.ObsRecvTask.Dispose(); }
                        catch { }
                        finally { Root.ObsRecvTask = Task.Run(() => ReceiveObsMesgs(this)); }
                    }
                    while (Root.VideoRecordWindowInProgress) Task.Delay(50);
                    Task.Delay(100);
                    if (Root.VideoRecordMode == VideoRecordMode.OBSRec)
                        Task.Run(() => SendInWs(Root.ObsWs, "GetRecordingStatus", new CancellationToken()));
                    else
                        Task.Run(() => SendInWs(Root.ObsWs, "GetStreamingStatus", new CancellationToken()));
                }
                prev = btVideo;
            }
            else
            {
                btVideo.Visible = false;
            }

            btStop.Height = dim1;
            btStop.Width = dim1;
            btStop.BackgroundImage = getImgFromDiskOrRes("exit", ImageExts);
            SetButtonPosition(prev, btStop, dim3);

            AdjustToolbarSize();
            FixToolbarSizeForNumberTag();

            gpButtonsWidth = gpButtons.Width;
            gpButtonsHeight = gpButtons.Height;
            VisibleToolbar.Width = gpButtonsWidth;
            VisibleToolbar.Height = gpButtonsHeight;
            gpButtonsLeft = Root.gpButtonsLeft;
            gpButtonsTop = Root.gpButtonsTop;

            if (((true || Root.AllowDraggingToolbar) &&
                 (!(IsInsideVisibleScreen(gpButtonsLeft, gpButtonsTop) &&
                    IsInsideVisibleScreen(gpButtonsLeft + gpButtonsWidth, gpButtonsTop) &&
                    IsInsideVisibleScreen(gpButtonsLeft, gpButtonsTop + gpButtonsHeight) &&
                    IsInsideVisibleScreen(gpButtonsLeft + gpButtonsWidth, gpButtonsTop + gpButtonsHeight))
                   || (gpButtonsLeft == 0 && gpButtonsTop == 0)))
                || (!Root.AllowDraggingToolbar))
            {
                if (Root.WindowRect.Width <= 0 || Root.WindowRect.Height <= 0)
                {
                    var virt = SystemInformation.VirtualScreen;
                    switch (Root.ToolbarOrientation)
                    {
                        case Orientation.toLeft:
                            gpButtonsLeft = virt.Right - gpButtons.Width + PrimaryLeft;
                            gpButtonsTop = virt.Bottom - gpButtons.Height - 15 + PrimaryTop;
                            gpButtons.Left = gpButtonsLeft + gpButtons.Width;
                            gpButtons.Top = gpButtonsTop;
                            VisibleToolbar.Width = 0;
                            break;
                        case Orientation.toRight:
                            gpButtonsLeft = virt.Left + PrimaryLeft;
                            gpButtonsTop = virt.Bottom - gpButtons.Height - 15 + PrimaryTop;
                            gpButtons.Left = gpButtonsLeft;
                            gpButtons.Top = gpButtonsTop;
                            VisibleToolbar.Width = 0;
                            break;
                        case Orientation.toUp:
                            gpButtonsLeft = virt.Right - gpButtons.Width - 15 + PrimaryLeft;
                            gpButtonsTop = virt.Bottom - gpButtons.Height + PrimaryTop;
                            gpButtons.Left = gpButtonsLeft;
                            gpButtons.Top = gpButtonsTop + gpButtons.Height;
                            VisibleToolbar.Height = 0;
                            break;
                        case Orientation.toDown:
                            gpButtonsLeft = virt.Right - gpButtons.Width - 15 + PrimaryLeft;
                            gpButtonsTop = virt.Top + PrimaryTop;
                            gpButtons.Left = gpButtonsLeft;
                            gpButtons.Top = gpButtonsTop;
                            VisibleToolbar.Height = 0;
                            break;
                        default:
                            if (Root.ToolbarOrientation <= Orientation.Horizontal)
                            {
                                gpButtonsLeft = this.ClientRectangle.Right - gpButtons.Width;
                                gpButtonsTop = this.ClientRectangle.Bottom - gpButtons.Height;
                                gpButtons.Left = gpButtonsLeft + gpButtons.Width;
                                gpButtons.Top = gpButtonsTop;
                                VisibleToolbar.Width = 0;
                            }
                            else
                            {
                                gpButtonsLeft = this.ClientRectangle.Right - gpButtons.Width;
                                gpButtonsTop = this.ClientRectangle.Top;
                                gpButtons.Left = gpButtonsLeft;
                                gpButtons.Top = gpButtonsTop;
                                VisibleToolbar.Height = 0;
                            }
                            break;
                    }
                }
                else
                {
                    if (Root.ToolbarOrientation <= Orientation.Horizontal)
                    {
                        gpButtonsLeft = this.ClientRectangle.Right - gpButtons.Width;
                        gpButtonsTop = this.ClientRectangle.Bottom - gpButtons.Height;
                        gpButtons.Left = gpButtonsLeft + gpButtons.Width;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Width = 0;
                    }
                    else
                    {
                        gpButtonsLeft = this.ClientRectangle.Right - gpButtons.Width;
                        gpButtonsTop = this.ClientRectangle.Top;
                        gpButtons.Left = gpButtonsLeft;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Height = 0;
                    }
                }

                Root.gpButtonsLeft = gpButtonsLeft;
                Root.gpButtonsTop = gpButtonsTop;
            }
            else
            {
                switch (Root.ToolbarOrientation)
                {
                    case Orientation.toLeft:
                        gpButtons.Left = gpButtonsLeft + gpButtonsWidth;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Width = 0;
                        break;
                    case Orientation.toRight:
                        gpButtons.Left = gpButtonsLeft;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Width = 0;
                        break;
                    case Orientation.toUp:
                        gpButtons.Left = gpButtonsLeft + gpButtonsHeight;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Height = 0;
                        break;
                    case Orientation.toDown:
                        gpButtons.Left = gpButtonsLeft;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Height = 0;
                        break;
                }
            }

            pboxPenWidthIndicator.Top = 0;
            pboxPenWidthIndicator.Left = (int)Math.Sqrt(Root.GlobalPenWidth * 30.0F);
            gpPenWidth.Controls.Add(pboxPenWidthIndicator);

            tempArrowCursor = null;
            try { cursorred?.Dispose(); }
            catch { }
            finally { cursorred = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D); }

            try { cursortarget?.Dispose(); }
            catch { }
            finally { cursortarget = getCursFromDiskOrRes(Root.cursortargetFileName, System.Windows.Forms.Cursors.SizeNWSE); }

            try { cursorerase?.Dispose(); }
            catch { }
            finally { cursorerase = getCursFromDiskOrRes(Root.cursoreraserFileName, System.Windows.Forms.Cursors.No); }

            try { cursorsnap?.Dispose(); }
            catch { }
            finally { cursorsnap = getCursFromDiskOrRes(Root.cursorsnapFileName, System.Windows.Forms.Cursors.Cross); }

            IC.Ink.Strokes.Clear();
            IC.Enabled = true;

            LastTickTime = DateTime.Parse("1987-01-01");
            tiSlide.Enabled = true;

            MemoHintDock = Root.Local.ButtonNameDock + " (" + Root.Hotkey_DockUndock.ToString() + ")";
            this.toolTip.SetToolTip(this.btDock, MemoHintDock);
            this.toolTip.SetToolTip(this.btExtraPens, Root.Local.ExtraPensHint);
            this.toolTip.SetToolTip(this.btPenWidth, Root.Local.ButtonNamePenwidth);
            this.toolTip.SetToolTip(this.btEraser, Root.Local.ButtonNameErasor + " (" + Root.Hotkey_Eraser.ToString() + ")");
            this.toolTip.SetToolTip(this.btPan, Root.Local.ButtonNamePan + " (" + Root.Hotkey_Pan.ToString() + ")");
            this.toolTip.SetToolTip(this.btScaleRot, Root.Local.ButtonNameScaleRotate + " (" + Root.Hotkey_ScaleRotate.ToString() + ")");
            this.toolTip.SetToolTip(this.btPointer, Root.Local.ButtonNameMousePointer + " (" + Root.Hotkey_Global.ToString() + ")");
            this.toolTip.SetToolTip(this.btInkVisible, Root.Local.ButtonNameInkVisible + " (" + Root.Hotkey_InkVisible.ToString() + ")");
            this.toolTip.SetToolTip(this.btSnap, Root.Local.ButtonNameSnapshot + " (" + Root.Hotkey_Snap.ToString() + ")");
            this.toolTip.SetToolTip(this.btUndo, Root.Local.ButtonNameUndo + " (" + Root.Hotkey_Undo.ToString() + ")");
            this.toolTip.SetToolTip(this.btClear, Root.Local.ButtonNameClear + " (" + Root.Hotkey_Clear.ToString() + ")");
            this.toolTip.SetToolTip(this.btVideo, Root.Local.ButtonNameVideo + " (" + Root.Hotkey_Video.ToString() + ")");
            MemoHintClose = Root.Local.ButtonNameExit + " (" + Root.Hotkey_Close.ToString() + "/Alt+F4)";
            this.toolTip.SetToolTip(this.btStop, MemoHintClose);
            this.toolTip.SetToolTip(this.btHand, Root.Local.ButtonNameHand + " (" + Root.Hotkey_Hand.ToString() + ")");
            this.toolTip.SetToolTip(this.btLine, Root.Local.ButtonNameLine + " (" + Root.Hotkey_Line.ToString() + ")");
            this.toolTip.SetToolTip(this.btRect, Root.Local.ButtonNameRect + " (" + Root.Hotkey_Rect.ToString() + ")");
            this.toolTip.SetToolTip(this.btOval, Root.Local.ButtonNameOval + " (" + Root.Hotkey_Oval.ToString() + ")");
            this.toolTip.SetToolTip(this.btArrow, Root.Local.ButtonNameArrow + " (" + Root.Hotkey_Arrow.ToString() + ")");
            this.toolTip.SetToolTip(this.btNumb, Root.Local.ButtonNameNumb + " (" + Root.Hotkey_Numb.ToString() + ")");
            this.toolTip.SetToolTip(this.btText, Root.Local.ButtonNameText + " (" + Root.Hotkey_Text.ToString() + ")");
            this.toolTip.SetToolTip(this.btEdit, Root.Local.ButtonNameEdit + " (" + Root.Hotkey_Edit.ToString() + ")");
            this.toolTip.SetToolTip(this.btMagn, Root.Local.ButtonNameMagn + " (" + Root.Hotkey_Magnet.ToString() + ")");
            this.toolTip.SetToolTip(this.btZoom, Root.Local.ButtonNameZoom + " (" + Root.Hotkey_Zoom.ToString() + ")");
            this.toolTip.SetToolTip(this.btClipArt, Root.Local.ButtonNameClipArt + " (" + Root.Hotkey_ClipArt.ToString() + ")");
            this.toolTip.SetToolTip(this.btClip1, Root.Local.ButtonNameClipArt + "-1 (" + Root.Hotkey_ClipArt1.ToString() + ")");
            this.toolTip.SetToolTip(this.btClip2, Root.Local.ButtonNameClipArt + "-2 (" + Root.Hotkey_ClipArt2.ToString() + ")");
            this.toolTip.SetToolTip(this.btClip3, Root.Local.ButtonNameClipArt + "-3 (" + Root.Hotkey_ClipArt3.ToString() + ")");
            this.toolTip.SetToolTip(this.btPagePrev, string.Format(Root.Local.ButtonPageNextPrev, ""));
            this.toolTip.SetToolTip(this.btPageNext, string.Format(Root.Local.ButtonPageNextPrev, ""));
            this.toolTip.SetToolTip(this.btSave, string.Format(Root.Local.SaveStroke, ""));
            this.toolTip.SetToolTip(this.btLoad, string.Format(Root.Local.LoadStroke, ""));
            this.toolTip.SetToolTip(this.btLasso, Root.Local.ButtonNameLasso + " (" + Root.Hotkey_Lasso.ToString() + ")");

            if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                gpSubTools.Height = dim;
                gpSubTools.Width = dim1 * 8 + dim3 * 8 + dim1s;
            }
            else
            {
                gpSubTools.Width = dim;
                gpSubTools.Height = dim1 * 8 + dim3 * 8 + dim1s;
            }
            gpPenWidth.Height = dim;
            setPenWidthBarPosition();

            Btn_SubTool0.Height = dim1;
            Btn_SubTool0.Width = dim1;
            int o;
            if ((Root.ToolbarOrientation == Orientation.toLeft) || (Root.ToolbarOrientation == Orientation.toRight))
            {
                Btn_SubTool0.Top = dim2;
                Btn_SubTool0.Left = 0;
                o = Orientation.toLeft;
            }
            else
            {
                Btn_SubTool0.Top = 0;
                Btn_SubTool0.Left = dim2;
                o = Orientation.toUp;
            }
            prev = Btn_SubTool0;
            for (int i = 1; i < Btn_SubTools.Length; i++)
            {
                Btn_SubTools[i].Width = dim1;
                Btn_SubTools[i].Height = dim1;
                SetButtonPosition(prev, Btn_SubTools[i], dim3, o);
                prev = Btn_SubTools[i];
            }
            Btn_SubToolClose.Height = dim1s;
            Btn_SubToolClose.Width = dim1s;
            SetButtonPosition(prev, Btn_SubToolClose, dim3, o);
            Btn_SubToolPin.Height = dim1s;
            Btn_SubToolPin.Width = dim1s;
            SetSmallButtonNext(Btn_SubToolClose, Btn_SubToolPin, dim2s, o);

            ToTransparent();
            ToTopMost();
            StopAllZooms();
            Root.PointerMode = true; // will be set to false within SelectPen(0) below
            SelectPen(0);
            IC.DefaultDrawingAttributes.Width = Root.PenAttr[0].Width; //required to ensure width
            SelectTool(Tools.Hand, Filling.Empty); // Select Hand Drawing by Default

            SaveStrokeFile = "";

            PatternLineSteps = -1;
            LineForPatterns = null;
            PatternLastPtIndex = -1;
            PatternLastPtRemain = 0;
            PatternPoints.Clear();
            StoredPatternPoints.Clear();

            PageIndex = 0;
            PageMax = 0;

            // Si une grille est définie via REST/Options, l’appliquer
            try
            {
                if (Root != null && Root.GridRectDefined && Root.GridRect.Width > 0 && Root.GridRect.Height > 0)
                {
                    SetGridFromRectangle(Root.GridRect);
                }
            }
            catch { }

            Console.WriteLine("C=" + (DateTime.Now.Ticks / 1e7).ToString());
        }

        // Methods used in FormCollection.GridSnap.cs
        private int CursorX, CursorY;
        private int CursorX0, CursorY0;

        private void SetSubBarPosition(Panel Tb, Button RefButton)
        {
            if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                Tb.Left = gpButtonsLeft + RefButton.Left;
                Tb.Top = gpButtonsTop - Tb.Height - 10;
                if (!(IsInsideVisibleScreen(Tb.Left, Tb.Top) && IsInsideVisibleScreen(Tb.Right, Tb.Bottom)))
                    Tb.Top = gpButtonsTop + gpButtonsHeight + 10;
            }
            else
            {
                Tb.Top = gpButtonsTop + RefButton.Top;
                Tb.Left = gpButtonsLeft - Tb.Width - 10;
                if (!(IsInsideVisibleScreen(Tb.Left, Tb.Top) && IsInsideVisibleScreen(Tb.Right, Tb.Bottom)))
                    Tb.Left = gpButtonsLeft + gpButtonsWidth + 10;
            }
        }

        private void setPenWidthBarPosition()
        {
            SetSubBarPosition(gpPenWidth, btPenWidth);
        }

        private void setClipArtDlgPosition()
        {
            if (Root.Docked)
            {
                ClipartsDlg.Left = Screen.PrimaryScreen.Bounds.Right - ClipartsDlg.Width - 1;
                ClipartsDlg.Top = Screen.PrimaryScreen.Bounds.Bottom - ClipartsDlg.Height - 1;
            }
            else if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                ClipartsDlg.Left = gpButtons.Right - ClipartsDlg.Width - 1;
                ClipartsDlg.Top = gpButtons.Top - ClipartsDlg.Height - 1;
                if (!(IsInsideVisibleScreen(ClipartsDlg.Left, ClipartsDlg.Top) && IsInsideVisibleScreen(ClipartsDlg.Right, ClipartsDlg.Bottom)))
                    ClipartsDlg.Top = gpButtons.Bottom + 1;
            }
            else // vertical
            {
                ClipartsDlg.Left = gpButtons.Left - ClipartsDlg.Width - 1;
                ClipartsDlg.Top = gpButtons.Top + 1;
                if (!(IsInsideVisibleScreen(ClipartsDlg.Left, ClipartsDlg.Top) && IsInsideVisibleScreen(ClipartsDlg.Right, ClipartsDlg.Bottom)))
                    ClipartsDlg.Left = gpButtons.Right + 1;
            }
        }

        // I want to be able to use the space,escape,... I must not leave the application handle those and generate clicks...
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return true;
        }
        public void AltTabActivate()
        {
            if (Initializing)
            {
                Initializing = false;
                return;
            }
            if (ButtonsEntering != 0 || DateTime.Now <= Root.PointerChangeDate)
            {
                return;
            }

            if (Root.FormButtonHitter.Visible &&
                (Math.Min(Root.FormButtonHitter.Width, Root.FormButtonHitter.Height) <=
                 Math.Min(Root.FormCollection.btDock.Width, Root.FormCollection.btDock.Height) * 1.5))
            {
                SelectPen(LastPenSelected);
                SelectTool(SavedTool, SavedFilled);
                SavedTool = -1;
                SavedFilled = -1;
                Root.FormDisplay.DrawBorder(true);
                Root.UnDock();
                Root.UponAllDrawingUpdate = true;
                Root.UponButtonsUpdate |= 0x7;
            }
        }

        // http://www.csharp411.com/hide-form-from-alttab/
        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == 0x001C) // WM_ACTIVATEAPP : generated through alt+tab
            {
                if (Initializing || AddM3UEntryInProgress) // still initializing, ignore
                    return;

                if (Root.FormDisplay != null && Root.FormDisplay.Visible)
                {
                    Root.FormDisplay.DrawBorder(Root.FormDisplay.HasFocus());
                    Root.FormDisplay.UpdateFormDisplay(true);
                }
                if (Root.FormDisplay == null || !Root.FormDisplay.Visible)
                    return;

                if (!Root.AltTabPointer || DateTime.Now < Root.PointerChangeDate)
                    return;

                if (msg.WParam == IntPtr.Zero) // losing Focus
                {
                    if (CheckEraseOnLosingFocus())
                    {
                        Root.ClearInk();
                    }
                    Root.Snapping = 0;
                    Root.ColorPickerMode = false;
                    if (!Root.PointerMode)
                    {
                        SavedTool = Root.ToolSelected;
                        SavedFilled = Root.FilledSelected;

                        SelectPen(-2);
                        Root.Dock();
                    }
                    return;
                }
                else // getting Focus
                {
                    if (Root.PointerMode)
                        AltTabActivate();
                    return;
                }
            }
            base.WndProc(ref msg);
        }

        private void SetVidBgImage()
        {
            if (Root.VideoRecInProgress == VideoRecInProgress.Dead)
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidDead", ImageExts);
            if (Root.VideoRecInProgress == VideoRecInProgress.Stopped)
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidStop", ImageExts);
            else if (Root.VideoRecInProgress == VideoRecInProgress.Recording)
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidRecord", ImageExts);
            else if (Root.VideoRecInProgress == VideoRecInProgress.Streaming)
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidBroadcast", ImageExts);
            else if (Root.VideoRecInProgress == VideoRecInProgress.Paused)
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidPause", ImageExts);
            else
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidUnk", ImageExts);

            Root.UponButtonsUpdate |= 0x2;
        }

        private void IC_MouseWheel(object sender, CancelMouseEventArgs e)
        {
            if (Root.PointerMode) // Wheel shall not be taken into account in edit mode
                return;

            if (ZoomForm.Visible && ((GetKeyState(VK_CONTROL)) & 0x8000) != 0)
            {
                int t = Math.Sign(e.Delta);
                ZoomForm.Height += t * (int)(10.0F * Root.ZoomHeight / Root.ZoomWidth);
                ZoomForm.Width += t * 10;
                return;
            }

            if (Root.InverseMousewheel ^ ((GetKeyState(VK_SHIFT) & 0x8000) != 0))
            {
                int p = LastPenSelected + (e.Delta > 0 ? 1 : -1);
                if (p >= Root.MaxPenCount) p = 0;
                if (p < 0) p = Root.MaxPenCount - 1;
                while (!Root.PenEnabled[p])
                {
                    p += (e.Delta > 0 ? 1 : -1);
                    if (p >= Root.MaxPenCount) p = 0;
                    if (p < 0) p = Root.MaxPenCount - 1;
                }
                SelectPen(p);
                return;
            }
            else
            {
                if (Root.ColorPickerMode)
                {
                    int i = Root.PickupTransparency + (e.Delta > 0 ? 2 : -2);
                    Root.PickupTransparency = (byte)Math.Min(Math.Max(0, i), 255);
                    this.Cursor = CreateCursorFromBitmap(buildColorPicker(Root.PickupColor, Root.PickupTransparency));
                }
                else if (Root.ToolSelected == Tools.NumberTag)
                {
                    TagSize += (e.Delta > 0 ? 1 : -1);
                    TagSize = Math.Min(Math.Max(4, TagSize), 255);
                }
                else
                {
                    PenWidth_Change(e.Delta > 0 ? Root.PenWidth_Delta : -Root.PenWidth_Delta);
                }
                return;
            }
        }

        private bool AltKeyPressed()
        {
            return ((short)(GetKeyState(VK_LMENU) | GetKeyState(VK_RMENU)) & 0x8000) == 0x8000;
        }

        private void btAllButtons_MouseDown(object sender, MouseEventArgs e)
        {
            MouseTimeDown = DateTime.Now;
            MouseDownButtonObject = sender;
            longClickTimer.Start();
            longClickTimer.Tag = sender;
            gpButtons_MouseDown(sender, e);
        }

        private void btAllButtons_MouseUp(object sender, MouseEventArgs e)
        {
            MouseDownButtonObject = null;
            (sender as Button).RightToLeft = RightToLeft.No;
            longClickTimer.Stop();
            IsMovingToolbar = 0;
            gpButtons_MouseUp(sender, e);
        }

        private void btAllButtons_RightClick(object sender, EventArgs e)
        {
            MouseTimeDown = DateTime.FromBinary(0);
            MouseDownButtonObject = null;
            longClickTimer.Stop();
            sender = (sender as ContextMenu).SourceControl;
            (sender as Button).RightToLeft = RightToLeft.No;
            (sender as Button).PerformClick();
        }

        private void longClickTimer_Tick(object sender, EventArgs e)
        {
            Button bt = MouseDownButtonObject as Button;
            MouseDownButtonObject = null;
            longClickTimer.Stop();
            bt.RightToLeft = RightToLeft.Yes;
            bt.PerformClick();
            if (IsMovingToolbar < 2)
                IsMovingToolbar = 0;
        }

        private int getStrokeProperties(Stroke st)
        {
            if (st.ExtendedProperties.Contains(Root.ISFILLEDBLACK_GUID))
                return Filling.BlackFilled;
            else if (st.ExtendedProperties.Contains(Root.ISFILLEDWHITE_GUID))
                return Filling.WhiteFilled;
            else if (st.ExtendedProperties.Contains(Root.ISFILLEDOUTSIDE_GUID))
                return Filling.Outside;
            else if (st.ExtendedProperties.Contains(Root.ISFILLEDCOLOR_GUID))
                return Filling.PenColorFilled;
            else
                return Filling.Empty;
        }

        private void setStrokeProperties(ref Stroke st, int FilledSelected)
        {
            if (st == null) return;

            var props = st.ExtendedProperties;

            try {if (props.Contains(Root.ISSTROKE_GUID)) props.Remove(Root.ISSTROKE_GUID); } catch { }
            try { if (props.Contains(Root.ISFILLEDCOLOR_GUID)) props.Remove(Root.ISFILLEDCOLOR_GUID); } catch { }
            try { if (props.Contains(Root.ISFILLEDOUTSIDE_GUID)) props.Remove(Root.ISFILLEDOUTSIDE_GUID); } catch { }
            try { if (props.Contains(Root.ISFILLEDWHITE_GUID)) props.Remove(Root.ISFILLEDWHITE_GUID); } catch { }
            try { if (props.Contains(Root.ISFILLEDBLACK_GUID)) props.Remove(Root.ISFILLEDBLACK_GUID); } catch { }

            bool hasWidth = false;
            try { hasWidth = st.DrawingAttributes != null && st.DrawingAttributes.Width > 0; } catch { hasWidth = false; }
            if (FilledSelected != Filling.PenColorFilled && FilledSelected != Filling.Outside && hasWidth)
            {
                try { props.Add(Root.ISSTROKE_GUID, true); } catch { }
            }

            switch (FilledSelected)
            {
                case Filling.Empty:
                    break;
                case Filling.PenColorFilled:
                    try { props.Add(Root.ISFILLEDCOLOR_GUID, true); } catch { }
                    break;
                case Filling.WhiteFilled:
                    try { props.Add(Root.ISFILLEDWHITE_GUID, true); } catch { }
                    break;
                case Filling.BlackFilled:
                    try { props.Add(Root.ISFILLEDBLACK_GUID, true); } catch { }
                    break;
                case Filling.Outside:
                    try { props.Add(Root.ISFILLEDOUTSIDE_GUID, true); } catch { }
                    break;
                default:
                    break;
            }
        }

        private void ApplyGoTagColorToDrawingAttributes(DrawingAttributes da, int[] colorArr)
        {
            try
            {
                if (da == null || colorArr == null || colorArr.Length < 4) return;
                da.Color = Color.FromArgb(colorArr[0], colorArr[1], colorArr[2], colorArr[3]);
                da.Transparency = (byte)(255 - colorArr[0]);
            }
            catch { }
        }

        private void ApplyHandFilledStroke(Stroke st, Color color, int opacityPercent, float width, int filling)
        {
            if (st == null) return;
            try
            {
                try { if (st.ExtendedProperties.Contains(Root.ISHIDDEN_GUID)) st.ExtendedProperties.Remove(Root.ISHIDDEN_GUID); } catch { }
                st.DrawingAttributes.Color = color;
                int op = Math.Max(0, Math.Min(100, opacityPercent));
                st.DrawingAttributes.Transparency = (byte)(255 - (op * 255 / 100));
                st.DrawingAttributes.Width = width;
                setStrokeProperties(ref st, filling);
            }
            catch { }
            try { if (st.ExtendedProperties.Contains(Root.FADING_PEN)) FadingList.Add(st); } catch { }
        }

        int NB_ELLIPSE_PTS = 36 * 3;

        private Stroke AddEllipseStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, int FilledSelected)
        {
            int dX = CursorX - CursorX0;
            int dY = CursorY - CursorY0;

            int maxRadius = Math.Max(Math.Abs(dX), Math.Abs(dY));
            int estimated = (int)Math.Round(2.0 * Math.PI * Math.Max(1, maxRadius));
            int ptsCount = Math.Max(36, Math.Min(estimated, 720));
            Point[] pts = new Point[ptsCount + 1];

            double angleStep = 2.0 * Math.PI / ptsCount;
            double offset = ptsCount / 8.0;

            for (int i = 0; i <= ptsCount; i++)
            {
                double theta = angleStep * (i + offset);
                double fx = dX * Math.Cos(theta);
                double fy = dY * Math.Sin(theta);
                pts[i] = new Point(
                    CursorX0 + (int)Math.Round(fx),
                    CursorY0 + (int)Math.Round(fy));
            }

            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts);

            Stroke st = IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = IC.DefaultDrawingAttributes.Clone();
            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = false;

            try
            {
                if (FilledSelected == Filling.NoFrame)
                {
                    st.DrawingAttributes.Transparency = 255;
                }
                else if (FilledSelected == Filling.WhiteFilled)
                {
                    st.DrawingAttributes.Color = Color.White;
                    st.DrawingAttributes.Transparency = 128;
                }
                else if (FilledSelected == Filling.BlackFilled)
                {
                    st.DrawingAttributes.Color = Color.Black;
                    st.DrawingAttributes.Transparency = 128;
                }
            }
            catch { }

            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = Root.FitToCurve;
            setStrokeProperties(ref st, FilledSelected);

            IC.Ink.Strokes.Add(st);
            if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st);

            return st;
        }

        private Stroke AddRectStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, int FilledSelected)
        {
            Point[] pts = new Point[9];
            int i = 0;
            pts[i++] = new Point(CursorX0, CursorY0);
            pts[i++] = new Point(CursorX0, (CursorY0 + CursorY) / 2);
            pts[i++] = new Point(CursorX0, CursorY);
            pts[i++] = new Point((CursorX0 + CursorX) / 2, CursorY);
            pts[i++] = new Point(CursorX, CursorY);
            pts[i++] = new Point(CursorX, (CursorY0 + CursorY) / 2);
            pts[i++] = new Point(CursorX, CursorY0);
            pts[i++] = new Point((CursorX0 + CursorX) / 2, CursorY0);
            pts[i++] = new Point(CursorX0, CursorY0);

            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts);

            Stroke st = IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = IC.DefaultDrawingAttributes.Clone();
            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = false;

            try
            {
                if (FilledSelected == Filling.NoFrame)
                {
                    st.DrawingAttributes.Transparency = 255;
                }
                else if (FilledSelected == Filling.WhiteFilled)
                {
                    st.DrawingAttributes.Color = Color.White;
                    st.DrawingAttributes.Transparency = 128;
                }
                else if (FilledSelected == Filling.BlackFilled)
                {
                    st.DrawingAttributes.Color = Color.Black;
                    st.DrawingAttributes.Transparency = 128;
                }
            }
            catch { }

            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = false;
            setStrokeProperties(ref st, FilledSelected);

            IC.Ink.Strokes.Add(st);
            if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st);

            return st;
        }
        private Stroke AddImageStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, string fn, int Filling = -10)
        {
            Point org_sz;
            try
            {
                org_sz = ClipartsDlg.ImgSizes[ClipartsDlg.ImageListViewer.LargeImageList.Images.IndexOfKey(Root.ImageStamp.ImageStamp)];
            }
            catch
            {
                org_sz = new Point(128, 128); // fallback
            }

            if (Filling == -10)
                Filling = Root.ImageStamp.Filling;

            Stroke st = AddRectStroke(CursorX0, CursorY0, CursorX, CursorY, Filling);
            try
            {
                string fn1 = Path.GetFileNameWithoutExtension(fn);
                fn1 = fn1.Split('@')[1];
                string[] lst = fn1.Split('.');
                int dx = int.Parse(lst[0]);
                int dy = int.Parse(lst[1]);
                dx = (int)(dx * (CursorX - CursorX0) * 1.0 / org_sz.X);
                dy = (int)(dy * (CursorY - CursorY0) * 1.0 / org_sz.Y);
                CursorX -= dx;
                CursorX0 -= dx;
                CursorY -= dy;
                CursorY0 -= dy;
            }
            catch { /* ignore */ }

            st.ExtendedProperties.Add(Root.IMAGE_GUID, fn);
            st.ExtendedProperties.Add(Root.IMAGE_X_GUID, (double)CursorX0);
            st.ExtendedProperties.Add(Root.IMAGE_Y_GUID, (double)CursorY0);
            st.ExtendedProperties.Add(Root.IMAGE_W_GUID, (double)(CursorX - CursorX0));
            st.ExtendedProperties.Add(Root.IMAGE_H_GUID, (double)(CursorY - CursorY0));
            st.ExtendedProperties.Add(Root.ROTATION_GUID, 0.0D);

            if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st);

            if (ClipartsDlg.Animations.ContainsKey(fn))
            {
                AnimationStructure ani = buildAni(fn);
                Animations.Add(AniPoolIdx, ani);
                st.ExtendedProperties.Add(Root.ANIMATIONFRAMEIMG_GUID, AniPoolIdx);
                AniPoolIdx++;
            }
            return st;
        }

        private AnimationStructure buildAni(string fn)
        {
            AnimationStructure ani = new AnimationStructure();
            if (!ClipartsDlg.Animations.ContainsKey(fn))
                ClipartsDlg.LoadImage(fn);

            ani.Image = ClipartsDlg.Animations[fn];
            ani.Idx = 0;
            ani.DeleteRequested = false;
            ani.Loop = int.MaxValue;
            ani.TEnd = DateTime.MaxValue;

            double d;
            string s = Regex.Match(Path.GetFileNameWithoutExtension(fn), "\\[(.*)\\]$").Groups[1].Value;
            bool l = false;
            if (s.EndsWith("X", StringComparison.InvariantCultureIgnoreCase))
            {
                s = s.Remove(s.Length - 1);
                l = true;
            }
            if (double.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
            {
                ani.DeleteAtDend = d < 0;
                if (l)
                    ani.Loop = (int)Math.Abs(d * ani.Image.NumFrames - 1);
                else
                    ani.TEnd = DateTime.Now.AddSeconds(.1 + Math.Abs(d));
            }
            ani.T0 = DateTime.Now.AddSeconds(.1 + ani.Image.Frames[ani.Idx].GetDelay());
            return ani;
        }

        private Stroke AddLineStroke(int CursorX0, int CursorY0, int CursorX, int CursorY)
        {
            Point[] pts = new Point[2];
            pts[0] = new Point(CursorX0, CursorY0);
            pts[1] = new Point(CursorX, CursorY);

            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts);

            Stroke st = Root.FormCollection.IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = Root.FormCollection.IC.DefaultDrawingAttributes.Clone();
            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = false;
            setStrokeProperties(ref st, 0);

            Root.FormCollection.IC.Ink.Strokes.Add(st);
            if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st);

            return st;
        }

        private Stroke ExtendPolyLineStroke(Stroke st, int CursorX, int CursorY, int FilledSelected)
        {
            Point[] pts = st.GetPoints();
            Array.Resize(ref pts, pts.Length + 1);
            Point[] pts2 = new Point[1];
            pts2[0] = new Point(CursorX, CursorY);

            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts2);
            pts[pts.Length - 1] = new Point(pts2[0].X, pts2[0].Y);

            Stroke st1 = Root.FormCollection.IC.Ink.CreateStroke(pts);
            st1.DrawingAttributes = st.DrawingAttributes.Clone();
            st1.DrawingAttributes.AntiAliased = true;
            st1.DrawingAttributes.FitToCurve = false;
            setStrokeProperties(ref st1, FilledSelected);

            Root.FormCollection.IC.Ink.DeleteStroke(st);
            Root.FormCollection.IC.Ink.Strokes.Add(st1);
            if (st1.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st1);

            return st1;
        }

        public double ArrowVarLen()
        {
            return Root.ArrowLen * Math.Max(.5, Math.Pow(IC.DefaultDrawingAttributes.Width / Root.PenWidthNormal, .7));
        }

        public Bitmap PrepareArrowBitmap(string fn, Color col, int transparency, double PenWidth_p, float angle_r, out int conn_len)
        {
            string[] fn_size = fn.Split('%');
            float scale = 1.0F;
            if (fn_size.Length >= 2)
                scale = float.Parse(fn_size[1], CultureInfo.InvariantCulture);

            ImageAttributes imageAttributes = new ImageAttributes();
            Bitmap bmpi = getImgFromDiskOrRes(fn_size[0], ImageExts);

            conn_len = 0;
            int i = bmpi.Height / 2 + 1; // normally line 101
            while (conn_len < bmpi.Width && !bmpi.GetPixel(conn_len, i).ToArgb().Equals(Color.Blue.ToArgb()))
                conn_len++;
            if (conn_len == bmpi.Width)
                conn_len = bmpi.Width / 2;
            conn_len = bmpi.Width / 2 - conn_len;

            float[][] colorMatrixElements =
            {
                new float[] { col.R / 255.0f, 0, 0, 0, 0 },
                new float[] { 0, col.G / 255.0f, 0, 0, 0 },
                new float[] { 0, 0, col.B / 255.0f, 0, 0 },
                new float[] { 0, 0, 0, (255 - transparency) / 255.0f, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            };
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            float f = (float)(scale * PenWidth_p / 18.0F);
            float w = (float)(Math.Abs(Math.Cos(angle_r)) * f * bmpi.Width + Math.Abs(Math.Sin(angle_r)) * f * bmpi.Height);
            float h = (float)(Math.Abs(Math.Sin(angle_r)) * f * bmpi.Width + Math.Abs(Math.Cos(angle_r)) * f * bmpi.Height);
            conn_len = (int)Math.Round(conn_len * f, 0);

            Bitmap bmpo = new Bitmap((int)Math.Round(w, 0), (int)Math.Round(h, 0), PixelFormat.Format32bppPArgb);
            Graphics g = Graphics.FromImage(bmpo);

            g.TranslateTransform(-bmpi.Width / 2, -bmpi.Height / 2);
            g.ScaleTransform(f, f, MatrixOrder.Append);
            if (Path.GetFileName(fn)[0] != '!')
                g.RotateTransform(180 + angle_r / (float)Math.PI * 180.0F, MatrixOrder.Append);
            g.TranslateTransform(w / 2, h / 2, MatrixOrder.Append);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(bmpi, new Rectangle(0, 0, bmpi.Width, bmpi.Height), 0, 0, bmpi.Width, bmpi.Height, GraphicsUnit.Pixel, imageAttributes);

            g.Dispose();
            bmpi.Dispose();
            return bmpo;
        }

        private Stroke AddArrowStroke(int CursorX0, int CursorY0, int CursorX, int CursorY)
        {
            // Compute angle
            double theta = Math.Atan2(CursorY - CursorY0, CursorX - CursorX0);

            // Force attributes for arrows
            DrawingAttributes forcedDA = IC.DefaultDrawingAttributes.Clone();
            try
            {
                forcedDA.Color = Root.GetArrowColor();
                forcedDA.Transparency = (byte)(255 - forcedDA.Color.A);
                forcedDA.Width = Root.GetArrowWidthHiMetric();
            }
            catch
            {
                forcedDA.Color = Color.Red;
                forcedDA.Transparency = 0;
            }

            // Prepare bitmaps (visual cues)
            int connLenHead = 0, connLenTail = 0;
            bool isStartArrow = (Root.ToolSelected == Tools.StartArrow);

            Bitmap bmpHead = PrepareArrowBitmap(
                Root.ArrowHead[Root.CurrentArrow],
                forcedDA.Color,
                forcedDA.Transparency,
                Root.HiMetricToPixel(forcedDA.Width),
                (float)(isStartArrow ? Math.PI + theta : theta),
                out connLenHead);
            StoredArrowImages.Add(bmpHead);
            int idxHead = StoredArrowImages.Count - 1;

            Bitmap bmpTail = PrepareArrowBitmap(
                Root.ArrowTail[Root.CurrentArrow],
                forcedDA.Color,
                forcedDA.Transparency,
                Root.HiMetricToPixel(forcedDA.Width),
                (float)(isStartArrow ? theta : Math.PI + theta),
                out connLenTail);
            StoredArrowImages.Add(bmpTail);
            int idxTail = StoredArrowImages.Count - 1;

            Point[] pts = new Point[]
            {
                new Point(CursorX0, CursorY0),
                new Point(CursorX, CursorY)
            };
            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts);

            Stroke st = IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = forcedDA.Clone();
            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = false;
            setStrokeProperties(ref st, 0);

            if (isStartArrow)
            {
                st.ExtendedProperties.Add(Root.ARROWEND_GUID, idxHead);
                st.ExtendedProperties.Add(Root.ARROWEND_X_GUID, CursorX0);
                st.ExtendedProperties.Add(Root.ARROWEND_Y_GUID, CursorY0);
                st.ExtendedProperties.Add(Root.ARROWEND_FN_GUID, Root.ArrowHead[Root.CurrentArrow]);

                st.ExtendedProperties.Add(Root.ARROWSTART_GUID, idxTail);
                st.ExtendedProperties.Add(Root.ARROWSTART_X_GUID, CursorX);
                st.ExtendedProperties.Add(Root.ARROWSTART_Y_GUID, CursorY);
                st.ExtendedProperties.Add(Root.ARROWSTART_FN_GUID, Root.ArrowTail[Root.CurrentArrow]);
            }
            else
            {
                st.ExtendedProperties.Add(Root.ARROWEND_GUID, idxHead);
                st.ExtendedProperties.Add(Root.ARROWEND_X_GUID, CursorX);
                st.ExtendedProperties.Add(Root.ARROWEND_Y_GUID, CursorY);
                st.ExtendedProperties.Add(Root.ARROWEND_FN_GUID, Root.ArrowHead[Root.CurrentArrow]);

                st.ExtendedProperties.Add(Root.ARROWSTART_GUID, idxTail);
                st.ExtendedProperties.Add(Root.ARROWSTART_X_GUID, CursorX0);
                st.ExtendedProperties.Add(Root.ARROWSTART_Y_GUID, CursorY0);
                st.ExtendedProperties.Add(Root.ARROWSTART_FN_GUID, Root.ArrowTail[Root.CurrentArrow]);
            }

            IC.Ink.Strokes.Add(st);
            try { if (st.ExtendedProperties.Contains(Root.FADING_PEN)) FadingList.Add(st); } catch { }

            return st;
        }

        public Stroke AddArrowPublic(int CursorX0, int CursorY0, int CursorX, int CursorY, bool startArrow)
        {
            int prevTool = Root.ToolSelected;
            int prevFilled = Root.FilledSelected;
            try
            {
                SelectTool(startArrow ? Tools.StartArrow : Tools.EndArrow, -1);
                Stroke st = AddArrowStroke(CursorX0, CursorY0, CursorX, CursorY);
                Root.UponAllDrawingUpdate = true;
                Root.UponButtonsUpdate |= 0x2;
                return st;
            }
            catch
            {
                return null;
            }
            finally
            {
                try { SelectTool(prevTool, prevFilled); } catch { }
            }
        }

        private Stroke AddNumberTagStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, string txt)
        {
            int filling = (Root.FilledSelected == Filling.PenColorFilled) ? 0 : Root.FilledSelected;

            int baseDiameter;
            if (this.GridRectDefined && this.GridRect.Width > 0 && this.GridRect.Height > 0)
            {
                int rows = Math.Max(2, this.Root.GridRows);
                int cols = Math.Max(2, this.Root.GridCols);

                double stepX = (double)this.GridRect.Width / (cols - 1);
                double stepY = (double)this.GridRect.Height / (rows - 1);
                double cellStep = Math.Min(stepX, stepY);

                const double fillFactor = 0.85;
                const int paddingPx = 2;
                int cand = Math.Max(10, (int)Math.Round(cellStep * fillFactor) - paddingPx);
                int increased = (int)Math.Round(cand * 1.10);
                int maxAllowed = Math.Max(10, (int)Math.Round(cellStep) - paddingPx);
                baseDiameter = Math.Min(increased, maxAllowed);
                baseDiameter = Math.Max(baseDiameter, 10);
            }
            else
            {
                baseDiameter = (int)Math.Round(TagSize * 1.2);
                baseDiameter = Math.Max(baseDiameter, 10);
            }

            double circlePct = (Root.TagCirclePercent <= 0.0) ? 100.0 : Root.TagCirclePercent;
            int diameterPx = Math.Max(6, (int)Math.Round(baseDiameter * (circlePct / 100.0)));

            int half = Math.Max(1, diameterPx / 2);
            int left = CursorX0;
            int top = CursorY0;
            int right = CursorX0 + half;
            int bottom = CursorY0 + half;

            Stroke st = AddEllipseStroke(left, top, right, bottom, filling);

            try { st.ExtendedProperties.Remove(Root.ISSTROKE_GUID); } catch { }
            try { st.DrawingAttributes.Width = 0.0f; } catch { }

            try
            {
                double op = (Root.TagStoneOpacityPercent <= 0.0) ? 0.0 : Math.Max(0.0, Math.Min(100.0, Root.TagStoneOpacityPercent));
                byte transparencyByte = (byte)Math.Round(255.0 * (1.0 - op / 100.0));
                st.DrawingAttributes.Color = Color.FromArgb(128, 128, 128);
                st.DrawingAttributes.Transparency = transparencyByte;
            }
            catch { }

            st.ExtendedProperties.Add(Root.ISTAG_GUID, true);
            try
            {
                if (!st.ExtendedProperties.Contains(Root.TEXTCOLOR_GUID))
                    st.ExtendedProperties.Add(Root.TEXTCOLOR_GUID, Color.FromArgb(255, 128, 128, 128).ToArgb());
            }
            catch { }

            Point pt = new Point(CursorX0, CursorY0);
            try { IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pt); } catch { }

            if (NumberTag_ShowNumber && !string.IsNullOrEmpty(txt))
            {
                st.ExtendedProperties.Add(Root.TEXT_GUID, txt);
                st.ExtendedProperties.Add(Root.TEXTX_GUID, (double)pt.X);
                st.ExtendedProperties.Add(Root.TEXTY_GUID, (double)pt.Y);
                st.ExtendedProperties.Add(Root.TEXTHALIGN_GUID, StringAlignment.Center);
                st.ExtendedProperties.Add(Root.TEXTVALIGN_GUID, StringAlignment.Center);
                st.ExtendedProperties.Add(Root.TEXTFONT_GUID, TagFont);

                double sizePct = (Root.TagSizePercent <= 0.0) ? 100.0 : Root.TagSizePercent;
                double fontSize;
                if (this.GridRectDefined && this.GridRect.Width > 0 && this.GridRect.Height > 0)
                    fontSize = Math.Max(6.0, diameterPx * 0.54 * (sizePct / 100.0));
                else
                    fontSize = Math.Max(6.0, (double)TagSize * (sizePct / 100.0));

                double maxFromCircle = Math.Max(6.0, diameterPx * 0.75);
                if (fontSize > maxFromCircle) fontSize = maxFromCircle;

                st.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, fontSize);
                System.Drawing.FontStyle style = TagItalic ? System.Drawing.FontStyle.Italic : System.Drawing.FontStyle.Regular;
                st.ExtendedProperties.Add(Root.TEXTFONTSTYLE_GUID, style);
            }

            st.ExtendedProperties.Add(Root.ROTATION_GUID, 0.0);

            try { ComputeTextBoxSize(ref st); } catch { }
            try { if (st.ExtendedProperties.Contains(Root.FADING_PEN)) FadingList.Add(st); } catch { }

            return st;
        }

        double TextTheta = 0.0;

        private Stroke AddTextStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, string txt, StringAlignment Align, int fil_in = -1)
        {
            int offsetX = 0;
            int offsetY = 0;
            try
            {
                var style = (TextItalic ? FontStyle.Italic : FontStyle.Regular) | (TextBold ? FontStyle.Bold : FontStyle.Regular);
                using (var f = new Font(TextFont ?? Root.TextFont, (float)(TextSize > 0 ? TextSize : Root.TextSize), style))
                {
                    var stf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
                    SizeF measured = Root.FormDisplay.gOneStrokeCanvus.MeasureString(string.IsNullOrEmpty(txt) ? "Mg" : txt, f, new SizeF(2000f, 2000f), stf);
                    offsetY = -(int)Math.Round(measured.Height / 2.0f);
                    offsetX = (int)Math.Round(measured.Height / -2.0f);
                }
            }
            catch
            {
                offsetY = -6;
                offsetX = 4;
            }

            try
            {
                if (Root != null && Root.ToolSelected == Tools.LetterTag)
                {
                    offsetX = 0;
                    offsetY = 0;
                }
            }
            catch { }

            if (Root.ForcedTextAlign.HasValue)
            {
                Align = Root.ForcedTextAlign.Value;
                Root.ForcedTextAlign = null;
            }

            Point ptPixel = new Point(CursorX0 + offsetX, CursorY0 + offsetY);
            Point pt = ptPixel;
            try
            {
                IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pt);
            }
            catch
            {
                pt = new Point(CursorX0, CursorY0);
                try { IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pt); } catch { }
            }

            Point[] pts = new Point[9] { pt, pt, pt, pt, pt, pt, pt, pt, pt };

            Stroke st = IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = IC.DefaultDrawingAttributes.Clone();
            st.DrawingAttributes.Width = 100;
            st.DrawingAttributes.FitToCurve = false;

            try { st.ExtendedProperties.Add(Root.TEXT_GUID, txt ?? ""); } catch { }
            try { st.ExtendedProperties.Add(Root.TEXTX_GUID, (double)pt.X); } catch { }
            try { st.ExtendedProperties.Add(Root.TEXTY_GUID, (double)pt.Y); } catch { }
            try { st.ExtendedProperties.Add(Root.TEXTHALIGN_GUID, Align); } catch { }
            try { st.ExtendedProperties.Add(Root.TEXTVALIGN_GUID, StringAlignment.Near); } catch { }

            try { st.ExtendedProperties.Add(Root.TEXTFONT_GUID, TextFont ?? Root.TextFont); } catch { }
            try { st.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, (double)(TextSize > 0 ? TextSize : Root.TextSize)); } catch { }
            try { st.ExtendedProperties.Add(Root.TEXTFONTSTYLE_GUID, (TextItalic ? FontStyle.Italic : FontStyle.Regular) | (TextBold ? FontStyle.Bold : FontStyle.Regular)); } catch { }
            try { st.ExtendedProperties.Add(Root.ROTATION_GUID, TextTheta); } catch { }

            try
            {
                if (Root.ActiveTextColorARGB != 0 && !st.ExtendedProperties.Contains(Root.TEXTCOLOR_GUID))
                    st.ExtendedProperties.Add(Root.TEXTCOLOR_GUID, Root.ActiveTextColorARGB);
            }
            catch { }

            int fil;
            if (fil_in < 0)
                fil_in = Root.TextBackground;
            switch (fil_in / 2)
            {
                case 1:
                    fil = Filling.WhiteFilled;
                    break;
                case 2:
                    fil = Filling.BlackFilled;
                    break;
                default:
                    fil = Filling.Empty;
                    break;
            }
            setStrokeProperties(ref st, fil);
            try { st.ExtendedProperties.Remove(Root.ISSTROKE_GUID); } catch { }
            if ((fil_in % 2) == 1)
                try { st.ExtendedProperties.Add(Root.ISSTROKE_GUID, true); } catch { }

            try { IC.Ink.Strokes.Add(st); } catch { }
            try
            {
                if (st.ExtendedProperties.Contains(Root.FADING_PEN)) FadingList.Add(st);
            }
            catch { }

            return st;
        }

        // Texte : édition inline (dialog)
        bool TextEdited = false;

        private DialogResult ModifyTextInStroke(Stroke stk, string txt)
        {
            AllowInteractions(true);
            FormInput inp = new FormInput(Root.Local.DlgTextCaption, Root.Local.DlgTextLabel, txt, true, Root, stk);
            DialogResult ret = inp.ShowDialog();
            TextEdited = true;
            AllowInteractions(false);
            try { IC.Cursor = cursorred; }
            catch { IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D); }

            System.Windows.Forms.Cursor.Position = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            return ret;
        }

        private float NearestStroke(Point pt, bool ptInPixel, out Stroke minStroke, out float pos, bool Search4Text = true, bool butLast = false, bool Magnet = true)
        {
            if (ptInPixel)
                IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pt);

            float dst = 10000000000;
            float dst1 = dst;
            float pos1;
            pos = 0;
            minStroke = null;

            for (int i = IC.Ink.Strokes.Count - (butLast ? 2 : 1); i >= 0; i--)
            {
                Stroke st = IC.Ink.Strokes[i];
                if (st.ExtendedProperties.Contains(Root.ISDELETION_GUID))
                    continue;

                pos1 = st.NearestPoint(pt, out dst1);
                if ((dst1 < dst) && (!Search4Text || (st.ExtendedProperties.Contains(Root.TEXT_GUID))))
                {
                    dst = dst1;
                    minStroke = st;
                    pos = pos1;
                }
            }
            return dst;
        }

        private void MagneticEffect(int cursorX0, int cursorY0, ref int cursorX, ref int cursorY, bool Magnetic = false)
        {
            int dist(int x, int y)
            {
                if (x == int.MaxValue || y == int.MinValue)
                    return int.MaxValue;
                return x * x + y * y;
            }

            Stroke st;
            float pos;
            Point pt = new Point(int.MaxValue, int.MaxValue);
            int x2 = int.MaxValue, y2 = int.MaxValue;

            if ((Control.ModifierKeys & Keys.Control) != Keys.None && (Control.ModifierKeys & Keys.Shift) != Keys.None)
                return;

            if ((Control.ModifierKeys & Keys.Control) != Keys.None || (Control.ModifierKeys & Keys.Shift) != Keys.None)
                Magnetic = false;

            if ((Magnetic || (Control.ModifierKeys & Keys.Control) != Keys.None) &&
                (NearestStroke(new Point(cursorX, cursorY), true, out st, out pos, false, true) <
                 Root.PixelToHiMetric(Root.MinMagneticRadius())))
            {
                pt = st.GetPoint((int)Math.Round(pos));
                IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref pt);
            }

            if ((Magnetic || (ModifierKeys & Keys.Control) != Keys.None))
            {
                foreach (Stroke stk in IC.Ink.Strokes)
                {
                    if (stk.ExtendedProperties.Contains(Root.TEXTWIDTH_GUID))
                    {
                        int x0 = Root.HiMetricToPixel((int)(double)stk.ExtendedProperties[Root.TEXTX_GUID].Data);
                        int y0 = Root.HiMetricToPixel((int)(double)stk.ExtendedProperties[Root.TEXTY_GUID].Data);
                        int x1, y1;

                        if ((StringAlignment)stk.ExtendedProperties[Root.TEXTHALIGN_GUID].Data == StringAlignment.Near)
                            x1 = (int)(x0 + (double)(stk.ExtendedProperties[Root.TEXTWIDTH_GUID].Data));
                        else
                        {
                            x1 = x0;
                            x0 = (int)(x1 - (double)(stk.ExtendedProperties[Root.TEXTWIDTH_GUID].Data));
                        }

                        if ((StringAlignment)stk.ExtendedProperties[Root.TEXTVALIGN_GUID].Data == StringAlignment.Near)
                            y1 = (int)(y0 + (double)stk.ExtendedProperties[Root.TEXTHEIGHT_GUID].Data);
                        else
                        {
                            y1 = y0;
                            y0 = (int)(y1 - (double)stk.ExtendedProperties[Root.TEXTHEIGHT_GUID].Data);
                        }

                        if ((x0 - Root.MinMagneticRadius()) <= cursorX && cursorX <= (x1 + Root.MinMagneticRadius()) &&
                            (y0 - Root.MinMagneticRadius()) <= cursorY && cursorY <= (y1 + Root.MinMagneticRadius()))
                        {
                            int d = dist(cursorX - x0, cursorY - y0);
                            x2 = x0; y2 = y0;

                            int d1 = dist(cursorX - (x1 + x0) / 2, cursorY - y0);
                            if (d1 < d) { x2 = (x1 + x0) / 2; y2 = y0; d = d1; }

                            d1 = dist(cursorX - x1, cursorY - y0);
                            if (d1 < d) { x2 = x1; y2 = y0; d = d1; }

                            d1 = dist(cursorX - x1, cursorY - (y0 + y1) / 2);
                            if (d1 < d) { x2 = x1; y2 = (y0 + y1) / 2; d = d1; }

                            d1 = dist(cursorX - x1, cursorY - y1);
                            if (d1 < d) { x2 = x1; y2 = y1; d = d1; }

                            d1 = dist(cursorX - (x0 + x1) / 2, cursorY - y1);
                            if (d1 < d) { x2 = (x0 + x1) / 2; y2 = y1; d = d1; }

                            d1 = dist(cursorX - x0, cursorY - y1);
                            if (d1 < d) { x2 = x0; y2 = y1; d = d1; }

                            d1 = dist(cursorX - x0, cursorY - (y0 + y1) / 2);
                            if (d1 < d) { x2 = x0; y2 = (y0 + y1) / 2; d = d1; }

                            break;
                        }
                    }
                }
            }

            if (dist(pt.X - cursorX, pt.Y - cursorY) < dist(x2 - cursorX, y2 - cursorY))
            {
                x2 = pt.X;
                y2 = pt.Y;
            }

            if (x2 != int.MaxValue && y2 != int.MaxValue)
            {
                cursorX = x2;
                cursorY = y2;
                return;
            }

            double theta = Math.Atan2(cursorY - cursorY0, cursorX - cursorX0) * 180.0 / Math.PI;
            double theta2;
            if (theta < 0) theta = theta + 360.0;
            theta2 = (((theta + Root.MagneticAngle / 2.0F) % Root.MagneticAngle) - Root.MagneticAngle / 2.0F) % 360.0;
            if (theta2 < 0) theta2 += 360.0;

            if ((Magnetic || (ModifierKeys & Keys.Control) != Keys.None) &&
                (Math.Abs(theta2) < Root.MagneticAngleTolerance || Math.Abs(theta2) > (360.0 - Root.MagneticAngleTolerance)))
            {
                theta -= theta2;
                if ((Math.Abs(theta) < 45.0) || (Math.Abs(theta - 180.0) < 45.0) || (Math.Abs(theta + 180.0) < 45.0))
                    cursorY = (int)((cursorX - cursorX0) * Math.Tan(theta / 180.0 * Math.PI) + cursorY0);
                else
                    cursorX = (int)((cursorY - cursorY0) / Math.Tan(theta / 180.0 * Math.PI) + cursorX0);
            }
        }
        int TransformXc = int.MinValue;
        int TransformYc = int.MinValue;

        private void Scale(Strokes Sel, Stroke Hover, int Xc, int Yc, int X0, int Y0, int X, int Y)
        {
            if (Xc == int.MinValue || Xc == int.MaxValue)
            {
                if (Sel != null && Sel.Count > 0)
                {
                    Rectangle r = Sel.GetBoundingBox();
                    Xc = (r.Left + r.Right) / 2;
                    Yc = (r.Top + r.Bottom) / 2;
                }
                else if (Hover != null)
                {
                    Rectangle r = Hover.GetBoundingBox();
                    Xc = (r.Left + r.Right) / 2;
                    Yc = (r.Top + r.Bottom) / 2;
                }
                else
                    return;
            }

            double k = Math.Sqrt((X0 - Xc) * (X0 - Xc) + (Y0 - Yc) * (Y0 - Yc));
            k = Math.Sqrt((X - Xc) * (X - Xc) + (Y - Yc) * (Y - Yc)) / k;

            ScaleRotate(Sel, Hover, Xc, Yc, k, 0.0);
        }

        private void Rotate(Strokes Sel, Stroke Hover, int Xc, int Yc, int X0, int Y0, int X, int Y)
        {
            if (Xc == int.MinValue || Xc == int.MaxValue)
            {
                if (Sel != null && Sel.Count > 0)
                {
                    Rectangle r = Sel.GetBoundingBox();
                    Xc = (r.Left + r.Right) / 2;
                    Yc = (r.Top + r.Bottom) / 2;
                }
                else if (Hover != null)
                {
                    Rectangle r = Hover.GetBoundingBox();
                    Xc = (r.Left + r.Right) / 2;
                    Yc = (r.Top + r.Bottom) / 2;
                }
                else
                    return;
            }

            double alpha = Math.Atan2(Y - Yc, X - Xc) - Math.Atan2(Y0 - Yc, X0 - Xc);
            ScaleRotate(Sel, Hover, Xc, Yc, 1.0, alpha / Math.PI * 180.0);
        }

        public void ScaleRotate(Strokes Sel, Stroke Hover, int Xc, int Yc, double k, double deg, bool applyOnPen = true)
        {
            void ModifyProperties(Stroke s)
            {
                if (s.ExtendedProperties.Contains(Root.ISDELETION_GUID))
                {
                    try
                    {
                        if (s.ExtendedProperties.Contains(Root.ANIMATIONFRAMEIMG_GUID))
                            Animations.Remove((int)s.ExtendedProperties[Root.ANIMATIONFRAMEIMG_GUID].Data);
                    }
                    catch { }
                    IC.Ink.DeleteStroke(s);
                    return;
                }

                if (s.ExtendedProperties.Contains(Root.IMAGE_GUID))
                {
                    Point p = s.GetPoint(0);
                    double W, H, rot;
                    IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref p);
                    s.ExtendedProperties.Add(Root.IMAGE_X_GUID, (double)p.X);
                    s.ExtendedProperties.Add(Root.IMAGE_Y_GUID, (double)p.Y);

                    W = (double)(s.ExtendedProperties[Root.IMAGE_W_GUID].Data) * k;
                    H = (double)(s.ExtendedProperties[Root.IMAGE_H_GUID].Data) * k;
                    s.ExtendedProperties.Add(Root.IMAGE_W_GUID, W);
                    s.ExtendedProperties.Add(Root.IMAGE_H_GUID, H);

                    rot = (double)s.ExtendedProperties[Root.ROTATION_GUID].Data + deg;
                    s.ExtendedProperties.Add(Root.ROTATION_GUID, rot);

                    rot = rot * Math.PI / 180.0;
                    if (s.ExtendedProperties.Contains(Root.LISTOFPOINTS_GUID))
                    {
                        int i1 = 0;
                        double d1 = 0;
                        double d2 = (double)(s.ExtendedProperties[Root.REPETITIONDISTANCE_GUID].Data) * k;
                        s.ExtendedProperties.Add(Root.REPETITIONDISTANCE_GUID, d2);
                        ListPoint pts = getEquiPointsFromStroke(
                            s,
                            d2,
                            ref i1,
                            ref d1,
                            -(int)(W * Math.Cos(rot) - H * Math.Sin(rot)) / 2,
                            -(int)(W * Math.Sin(rot) + H * Math.Cos(rot)) / 2,
                            true);
                        StoredPatternPoints[(int)s.ExtendedProperties[Root.LISTOFPOINTS_GUID].Data].Clear();
                        StoredPatternPoints[(int)s.ExtendedProperties[Root.LISTOFPOINTS_GUID].Data].AddRange(pts);
                    }
                }

                if (s.ExtendedProperties.Contains(Root.TEXTFONT_GUID))
                {
                    Point p = s.GetPoint(0);
                    if (s.ExtendedProperties.Contains(Root.ISTAG_GUID))
                    {
                        int minX = p.X, maxX = p.X, minY = p.Y, maxY = p.Y;
                        foreach (Point pt in s.GetPoints())
                        {
                            if (pt.X < minX) minX = pt.X;
                            if (pt.Y < minY) minY = pt.Y;
                            if (pt.X > maxX) maxX = pt.X;
                            if (pt.Y > maxY) maxY = pt.Y;
                        }
                        p.X = (int)(minX + .5 * (maxX - minX));
                        p.Y = (int)(minY + .5 * (maxY - minY));
                    }
                    s.ExtendedProperties.Add(Root.TEXTX_GUID, (double)p.X);
                    s.ExtendedProperties.Add(Root.TEXTY_GUID, (double)p.Y);
                }
                if (s.ExtendedProperties.Contains(Root.ARROWSTART_GUID))
                {
                    double theta;
                    Point p = s.GetPoint(0);
                    Point p1 = s.GetPoint(1);
                    theta = Math.Atan2(p1.Y - p.Y, p1.X - p.X);
                    int i = (int)s.ExtendedProperties[Root.ARROWSTART_GUID].Data;
                    string fn = (string)s.ExtendedProperties[Root.ARROWSTART_FN_GUID].Data;
                    int l;
                    StoredArrowImages[i].Dispose();
                    double kk = Math.Max(1, s.DrawingAttributes.Width * 0.037795280352161);
                    StoredArrowImages[i] = PrepareArrowBitmap(fn, s.DrawingAttributes.Color, s.DrawingAttributes.Transparency, kk, (float)theta, out l);
                    kk = kk / 18.0f;
                    IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref p);
                    p.Offset((int)Math.Round(-kk * l * Math.Cos(theta)), (int)Math.Round(-kk * l * Math.Sin(theta)));
                    s.ExtendedProperties.Add(Root.ARROWSTART_X_GUID, (int)p.X);
                    s.ExtendedProperties.Add(Root.ARROWSTART_Y_GUID, (int)p.Y);
                }

                if (s.ExtendedProperties.Contains(Root.ARROWEND_GUID))
                {
                    double theta;
                    Point p = s.GetPoint(1);
                    Point p1 = s.GetPoint(0);
                    theta = Math.Atan2(p1.Y - p.Y, p1.X - p.X);
                    int i = (int)s.ExtendedProperties[Root.ARROWEND_GUID].Data;
                    string fn = (string)s.ExtendedProperties[Root.ARROWEND_FN_GUID].Data;
                    int l;
                    StoredArrowImages[i].Dispose();
                    double kk = Math.Max(1, s.DrawingAttributes.Width * 0.037795280352161);
                    StoredArrowImages[i] = PrepareArrowBitmap(fn, s.DrawingAttributes.Color, s.DrawingAttributes.Transparency, kk, (float)theta, out l);
                    kk = kk / 18.0f;
                    IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref p);
                    p.Offset((int)Math.Round(-kk * l * Math.Cos(theta)), (int)Math.Round(-kk * l * Math.Sin(theta)));
                    s.ExtendedProperties.Add(Root.ARROWEND_X_GUID, (int)p.X);
                    s.ExtendedProperties.Add(Root.ARROWEND_Y_GUID, (int)p.Y);
                }
            }

            if (k == 0)
                return;

            Matrix m = new Matrix(1, 0, 0, 1, 0, 0);
            m.Translate(+Xc, +Yc);
            m.Scale((float)k, (float)k);
            m.Rotate((float)deg);
            m.Translate(-Xc, -Yc);

            if (Sel != null && Sel.Count > 0)
            {
                if (double.IsNaN(Sel.GetBoundingBox().Width * k))
                    return;

                Sel.Transform(m, false);
                foreach (Stroke s in Sel)
                    ModifyProperties(s);
            }
            else if (Hover != null)
            {
                if (double.IsNaN(Hover.GetBoundingBox().Width * k))
                    return;

                Hover.Transform(m, false);
                ModifyProperties(Hover);
            }
        }

        private void mInkObject_StrokesDeleting(object sender, InkOverlayStrokesDeletingEventArgs e)
        {
            Console.WriteLine("deleting ");
        }

        int dbgcpt = 0;
        private Stroke currentStroke = null;
        private int HideMetricCountDown = 0;
        private void IC_Stroke(object sender, InkCollectorStrokeEventArgs e)
        {
            // Fix for the missing CursorX, CursorY, CursorX0, CursorY0 variables
            try
            {
                if (e.Stroke.ExtendedProperties.Contains(Root.IMAGE_GUID))
                {
                    // Use Root.CursorX/Y instead of local variables that don't exist
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_W_GUID, (double)(Root.CursorX - Root.CursorX0));
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_H_GUID, (double)(Root.CursorY - Root.CursorY0));
                }
            }
            catch { }

            if (e.Cursor.Inverted)
            {
                Console.WriteLine("del");
                e.Cancel = true;
                return;
            }

            // Determine current cursor position in client coordinates
            int ex, ey;
            try
            {
                Point scr = System.Windows.Forms.Cursor.Position;
                Point cli = Root?.FormDisplay != null ? Root.FormDisplay.PointToClient(scr) : this.PointToClient(scr);
                ex = cli.X; ey = cli.Y;
            }
            catch
            {
                ex = Root?.CursorX ?? 0; ey = Root?.CursorY ?? 0;
            }

            Rectangle r = e.Stroke.GetBoundingBox();
            bool HitTouch = Math.Max(r.Width, r.Height) < 2 * e.Stroke.DrawingAttributes.Width; // PenWidth extension by GetBoundingBox

            movedStroke = null; // reset the moving object
            Root.FingerInAction = false;

            try
            {
                if (e.Stroke.ExtendedProperties.Contains(Root.ISSTROKE_GUID))
                    e.Stroke.ExtendedProperties.Remove(Root.ISSTROKE_GUID);
            }
            catch { }

            if (ZoomCapturing)
            {
                IC.Ink.DeleteStroke(e.Stroke);
                if (HitTouch || ((Root.CursorX0 == Root.CursorX) && (Root.CursorY0 == Root.CursorY)))
                    return;
                else
                {
                    ZoomCapturing = false;
                    ZoomCaptured = true;
                }

                SaveStrokes(ZoomSaveStroke);
                Bitmap capt = new Bitmap(Math.Abs(Root.CursorX0 - Root.CursorX), Math.Abs(Root.CursorY0 - Root.CursorY));
                using (Graphics g = Graphics.FromImage(capt))
                {
                    Point p = PointToScreen(new Point(Math.Min(Root.CursorX0, Root.CursorX), Math.Min(Root.CursorY0, Root.CursorY)));
                    Size sz = new Size(capt.Width, capt.Height);
                    g.CopyFromScreen(p, Point.Empty, sz);
                    try { ClipartsDlg.Originals.Remove(Path.GetTempPath().Replace("\\", "/") + "_ZoomClip"); } catch { }
                    ClipartsDlg.Originals.Add(Path.GetTempPath().Replace("\\", "/") + "_ZoomClip", capt);

                    IC.Ink.Strokes.Clear();
                    Stroke st;
                    if (Root.WindowRect.Width > 0)
                    {
                        st = AddImageStroke(0, 0, Width, Height, Path.GetTempPath().Replace("\\", "/") + "_ZoomClip", Filling.NoFrame);
                    }
                    else
                    {
                        Screen scr = Screen.FromPoint(MousePosition);
                        st = AddImageStroke(scr.Bounds.Left, scr.Bounds.Top, scr.Bounds.Right, scr.Bounds.Bottom, Path.GetTempPath().Replace("\\", "/") + "_ZoomClip", Filling.NoFrame);
                    }
                    try { st.ExtendedProperties.Remove(Root.FADING_PEN); } catch { }
                    SetPenTipCursor();
                }
                return;
            }
            else if (Root.ToolSelected == Tools.Hand)
            {
                Console.WriteLine("Hand");
                Stroke st = e.Stroke;
                try
                {
                    if (e.Stroke.GetPoints().Length >= 3)
                        if (e.Stroke.GetPoint(0).Equals(e.Stroke.GetPoint(2)))
                            st.SetPoint(0, e.Stroke.GetPoint(1));
                }
                catch { }
                setStrokeProperties(ref st, Root.FilledSelected);
                if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                    FadingList.Add(st);
            }
            else if (Root.ToolSelected == Tools.HandFilledWhite)
            {
                Stroke st = e.Stroke;
                ApplyHandFilledStroke(st, Color.White, Root.GoStrokeOpacityPercent, Root.GoStrokeWidth, Filling.WhiteFilled);
            }
            else if (Root.ToolSelected == Tools.HandFilledBlack)
            {
                Stroke st = e.Stroke;
                ApplyHandFilledStroke(st, Color.Black, Root.GoStrokeOpacityPercent, Root.GoStrokeWidth, Filling.BlackFilled);
            }
            else if (Root.ToolSelected == Tools.PatternLine && PatternLineSteps == 2)
            {
                if (PatternPoints.Count == 0)
                {
                    IC.Ink.DeleteStroke(e.Stroke);
                }
                else
                {
                    try { e.Stroke.ExtendedProperties.Remove(Root.ISHIDDEN_GUID); } catch { }
                    e.Stroke.DrawingAttributes.Transparency = 255;
                    e.Stroke.ExtendedProperties.Add(Root.ISSTROKE_GUID, true);
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_GUID, Root.ImageStamp.ImageStamp);
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_X_GUID, (double)Root.CursorX0);
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_Y_GUID, (double)Root.CursorY0);
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_W_GUID, (double)(CursorX - CursorX0));
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_H_GUID, (double)(CursorY - CursorY0));
                    e.Stroke.ExtendedProperties.Add(Root.ROTATION_GUID, 0.0D);

                    try { if (e.Stroke.ExtendedProperties.Contains(Root.FADING_PEN)) FadingList.Add(e.Stroke); } catch { }

                    e.Stroke.ExtendedProperties.Add(Root.REPETITIONDISTANCE_GUID, PatternDist);
                    StoredPatternPoints.Add(new ListPoint(PatternPoints));
                    e.Stroke.ExtendedProperties.Add(Root.LISTOFPOINTS_GUID, StoredPatternPoints.Count - 1);

                    if (ClipartsDlg.Animations.ContainsKey(Root.ImageStamp.ImageStamp))
                    {
                        AnimationStructure ani = buildAni(Root.ImageStamp.ImageStamp);
                        Animations.Add(AniPoolIdx, ani);
                        e.Stroke.ExtendedProperties.Add(Root.ANIMATIONFRAMEIMG_GUID, AniPoolIdx);
                        AniPoolIdx++;
                    }
                    Root.ImageStamp.Store = false;
                }
                LineForPatterns = null;
                PatternPoints.Clear();
            }
            else
            {
                if (HitTouch)
                {
                    Point p = System.Windows.Forms.Cursor.Position;
                    p = Root.FormDisplay.PointToClient(p);
                    Root.CursorX0 = p.X;
                    Root.CursorY0 = p.Y;
                }

                if (Root.LassoMode)
                {
                    Point[] pts = e.Stroke.GetPoints();
                    if (pts.Length >= 3)
                    {
                        InprogressSelection = IC.Ink.HitTest(pts, Root.LassoPercent, out _);
                        InprogressSelection.Remove(e.Stroke);
                    }
                    Console.WriteLine("Lasso capt " + (InprogressSelection?.Count.ToString() ?? "0"));
                }

                IC.Ink.DeleteStroke(e.Stroke);

                if ((Root.ToolSelected == Tools.Line) && (!HitTouch))
                {
                    Console.WriteLine("Line");
                    AddLineStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY);
                }
                else if ((Root.ToolSelected == Tools.Rect) && (!HitTouch))
                {
                    Console.WriteLine("Rect");
                    if ((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2))
                        AddRectStroke(2 * Root.CursorX0 - Root.CursorX, 2 * Root.CursorY0 - Root.CursorY, Root.CursorX, Root.CursorY, Root.FilledSelected);
                    else
                        AddRectStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY, Root.FilledSelected);
                }
                else if (Root.ToolSelected == Tools.ClipArt || (Root.ToolSelected == Tools.PatternLine && PatternLineSteps == 0))
                {
                    Console.WriteLine("ClipArt");

                    int w = Root.ImageStamp.X > 0 ? Root.ImageStamp.X :
                        ClipartsDlg.ImgSizes[ClipartsDlg.ImageListViewer.LargeImageList.Images.IndexOfKey(Root.ImageStamp.ImageStamp)].X;
                    int h = Root.ImageStamp.Y > 0 ? Root.ImageStamp.Y :
                        ClipartsDlg.ImgSizes[ClipartsDlg.ImageListViewer.LargeImageList.Images.IndexOfKey(Root.ImageStamp.ImageStamp)].Y;

                    if (HitTouch || ((Root.CursorX0 == Root.CursorX) && (Root.CursorY0 == Root.CursorY)) || ((Root.CursorX0 == Int32.MinValue)))
                    {
                        Root.CursorX0 = Root.CursorX - (((int)CurrentMouseButton == 2 || CurrentMouseButton == MouseButtons.Right) ? (w / 2) : 0);
                        Root.CursorY0 = Root.CursorY - (((int)CurrentMouseButton == 2 || CurrentMouseButton == MouseButtons.Right) ? (h / 2) : 0);
                        Root.CursorX = Root.CursorX0 + w;
                        Root.CursorY = Root.CursorY0 + h;
                    }
                    else
                    {
                        if (Math.Abs((double)(Root.CursorX - Root.CursorX0) / (Root.CursorY - Root.CursorY0)) < Root.StampScaleRatio)
                            Root.CursorX = (int)(Root.CursorX0 + (double)(Root.CursorY - Root.CursorY0) / h * w);
                        else if (Math.Abs((double)(Root.CursorY - Root.CursorY0) / (Root.CursorX - Root.CursorX0)) < Root.StampScaleRatio)
                            Root.CursorY = (int)(Root.CursorY0 + (double)(Root.CursorX - Root.CursorX0) / w * h);

                        if ((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2))
                        {
                            Root.CursorX0 -= (Root.CursorX - Root.CursorX0) / 2;
                            Root.CursorY0 -= (Root.CursorY - Root.CursorY0) / 2;
                        }
                    }

                    if (Root.ToolSelected == Tools.ClipArt)
                    {
                        AddImageStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY, Root.ImageStamp.ImageStamp);
                    }
                    else if (Root.ToolSelected == Tools.PatternLine)
                    {
                        Root.ImageStamp.X = Math.Abs(Root.CursorX - Root.CursorX0);
                        Root.ImageStamp.Y = Math.Abs(Root.CursorY - Root.CursorY0);
                        if (Root.ImageStamp.Store)
                        {
                            Root.ImageStamp.Wstored = Root.ImageStamp.X;
                            Root.ImageStamp.Hstored = Root.ImageStamp.Y;
                        }
                        PatternLineSteps = 1; // nextStep
                        PatternImage?.Dispose();
                        PatternImage = new Bitmap(Root.ImageStamp.ImageStamp);
                        RotatingOnLine = Path.GetFileName(Root.ImageStamp.ImageStamp).StartsWith("~");
                        PatternPoints.Clear();
                    }
                }
                else if (Root.ToolSelected == Tools.PatternLine && PatternLineSteps == 1)
                {
                    Point p = new Point() { X = Root.CursorX0 - Root.CursorX, Y = Root.CursorY0 - Root.CursorY };
                    IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref p);
                    PatternDist = Math.Sqrt(p.X * p.X + p.Y * p.Y);
                    p.X = Root.ImageStamp.X;
                    p.Y = Root.ImageStamp.Y;
                    IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref p);
                    PatternDist = Math.Max(PatternDist, 0.5 * Math.Min(p.X, p.Y));
                    if (Root.ImageStamp.Store)
                        Root.ImageStamp.Distance = PatternDist;
                    PatternLineSteps = 2;
                    try { IC.Cursor = cursorred; }
                    catch { IC.Cursor = getCursFromDiskOrRes(Root.cursorredFileName, System.Windows.Forms.Cursors.NoMove2D); }
                }
                else if ((Root.ToolSelected == Tools.Oval) && !HitTouch)
                {
                    Console.WriteLine("Oval");
                    if ((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2))
                        AddEllipseStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY, Root.FilledSelected);
                    else
                        AddEllipseStroke((Root.CursorX0 + Root.CursorX) / 2, (Root.CursorY0 + Root.CursorY) / 2, Root.CursorX, Root.CursorY, Root.FilledSelected);
                }
                else if (((Root.ToolSelected == Tools.StartArrow) || (Root.ToolSelected == Tools.EndArrow)) && !HitTouch)
                {
                    Console.WriteLine("Arrow");
                    if ((((int)CurrentMouseButton == 2) || (CurrentMouseButton == MouseButtons.Right)) ^ (Root.ToolSelected == Tools.StartArrow))
                        AddArrowStroke(Root.CursorX, Root.CursorY, Root.CursorX0, Root.CursorY0);
                    else
                        AddArrowStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY);
                }
                else if (Root.ToolSelected == Tools.NumberTag)
                {
                    Point clientPt = new Point(Root.CursorX, Root.CursorY);
                    Point screenPt = Root.FormDisplay.PointToScreen(clientPt);

                    Point snapScreen = GridSnap.SnapNumberTagPoint(this.Root, screenPt.X, screenPt.Y, this.Root.GridRows, this.Root.GridCols);

                    bool ignoreClick = false;
                    if (this.GridRectDefined && this.GridRect.Width > 0 && this.GridRect.Height > 0)
                    {
                        int rows = Math.Max(2, this.Root.GridRows);
                        int cols = Math.Max(2, this.Root.GridCols);
                        double stepX = (double)this.GridRect.Width / (cols - 1);
                        double stepY = (double)this.GridRect.Height / (rows - 1);
                        double cellStep = Math.Min(stepX, stepY);
                        double allowedDist = 0.75 * cellStep;
                        double dx = screenPt.X - snapScreen.X;
                        double dy = screenPt.Y - snapScreen.Y;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        if (dist > allowedDist)
                            ignoreClick = true;
                    }

                    if (!ignoreClick)
                    {
                        Point snapClient = Root.FormDisplay.PointToClient(snapScreen);
                        string raw = NumberTag_GetAndIncrementText();
                        int val = 1; if (!int.TryParse(raw, out val)) val = 1;
                        string formattedTxt = NumberTag_ShowNumber
                            ? string.Format(Root.TagFormatting, val, (Char)(65 + (val - 1) % 26), (Char)(97 + (val - 1) % 26))
                            : "";
                        Stroke st = AddNumberTagStroke(snapClient.X, snapClient.Y, snapClient.X, snapClient.Y, formattedTxt);

                        if (Root.FilledSelected == Filling.WhiteFilled)
                            Root.FilledSelected = Filling.BlackFilled;
                        else if (Root.FilledSelected == Filling.BlackFilled)
                            Root.FilledSelected = Filling.WhiteFilled;
                        else
                            Root.FilledSelected = Filling.WhiteFilled;

                        Root.UponButtonsUpdate |= 0x2;
                    }
                }
                else if (IsNewTagTool(Root.ToolSelected))
                {
                    string txt;
                    if (Root.ToolSelected == Tools.LetterTag)
                        txt = GetNextLetterTag();
                    else
                        txt = ShapeGlyphForTool(Root.ToolSelected).ToString();

                    Point clientPt = new Point(Root.CursorX, Root.CursorY);
                    Point screenPt = Root.FormDisplay.PointToScreen(clientPt);
                    Point snapScreen = GridSnap.SnapNumberTagPoint(this.Root, screenPt.X, screenPt.Y, this.Root.GridRows, this.Root.GridCols);

                    bool ignoreClick = false;
                    if (this.GridRectDefined && this.GridRect.Width > 0 && this.GridRect.Height > 0)
                    {
                        int rows = Math.Max(2, this.Root.GridRows);
                        int cols = Math.Max(2, this.Root.GridCols);
                        double stepX = (double)this.GridRect.Width / (cols - 1);
                        double stepY = (double)this.GridRect.Height / (rows - 1);
                        double cellStep = Math.Min(stepX, stepY);
                        double allowedDist = 0.75 * cellStep;
                        double dx = screenPt.X - snapScreen.X;
                        double dy = screenPt.Y - snapScreen.Y;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        if (dist > allowedDist)
                            ignoreClick = true;
                    }

                    if (!ignoreClick)
                    {
                        Point snapClient = Root.FormDisplay.PointToClient(snapScreen);

                        int baseDiameter;
                        if (this.GridRectDefined && this.GridRect.Width > 0 && this.GridRect.Height > 0)
                        {
                            int rows = Math.Max(2, this.Root.GridRows);
                            int cols = Math.Max(2, this.Root.GridCols);
                            double stepX = (double)this.GridRect.Width / (cols - 1);
                            double stepY = (double)this.GridRect.Height / (rows - 1);
                            double cellStep = Math.Min(stepX, stepY);
                            double allowedDist = 0.75 * cellStep;
                            double dx = screenPt.X - snapScreen.X;
                            double dy = screenPt.Y - snapScreen.Y;
                            double dist = Math.Sqrt(dx * dx + dy * dy);
                            if (dist > allowedDist)
                                ignoreClick = true;
                        }

                        if (!ignoreClick)
                        {
                            int diameterPx = (int)Math.Round(TagSize * 0.8);
                            AddShapeTagStroke(snapClient.X, snapClient.Y, txt, diameterPx, null);
                            SaveUndoStrokes();
                        }
                    }
                }
                else if (Root.ToolSelected == Tools.Edit)
                {
                    float pos;
                    Stroke minStroke;
                    if (NearestStroke(new Point(Root.CursorX, Root.CursorY), true, out minStroke, out pos, false, false) <=
                        1 + Root.PixelToHiMetric(Root.MinMagneticRadius() / (Root.MagneticRadius >= 0 ^ ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0) ? 1 : 10)))
                    {
                        if (minStroke.ExtendedProperties.Contains(Root.TEXT_GUID))
                        {
                            ModifyTextInStroke(minStroke, (string)(minStroke.ExtendedProperties[Root.TEXT_GUID].Data));
                            SelectTool(Tools.Hand, Filling.Empty);
                            ComputeTextBoxSize(ref minStroke);
                        }
                        else
                        {
                            AllowInteractions(true);
                            DrawingAttributes da = minStroke.DrawingAttributes.Clone();
                            int fil = getStrokeProperties(minStroke);
                            if (PenModifyDlg.ModifyPenAndFilling(ref da, ref fil))
                            {
                                minStroke.DrawingAttributes = da.Clone();
                                setStrokeProperties(ref minStroke, fil);
                            }
                            if (minStroke.ExtendedProperties.Contains(Root.ARROWSTART_GUID))
                            {
                                ArrowSelDlg dlg = new ArrowSelDlg(Root);
                                dlg.Initialize(minStroke);
                                dlg.ShowDialog();
                            }
                            AllowInteractions(false);
                        }
                    }
                }
                else if ((Root.ToolSelected == Tools.txtLeftAligned) || (Root.ToolSelected == Tools.txtRightAligned))
                {
                    Stroke ms;
                    float pos1;
                    if (NearestStroke(new Point(ex, ey), true, out ms, out pos1, false, false) <=
                        1 + Root.PixelToHiMetric(Root.MinMagneticRadius() / (Root.MagneticRadius >= 0 ^ ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0) ? 1 : 10)))
                    {
                        Root.StrokeHovered = ms;
                        int i = (int)Math.Floor(pos1);
                        if (i == ms.PacketCount - 1) i--;
                        Point p = ms.GetPoint(i);
                        Point p1 = ms.GetPoint(i + 1);
                        TextTheta = (ms.PacketCount == 2 ? 180.0 : 0.0) + Math.Atan2(p1.Y - p.Y, p1.X - p.X) * 180.0 / Math.PI;
                        if (TextTheta >= 91.0 && TextTheta < 270.0)
                            TextTheta -= 180.0;
                    }
                    else
                    {
                        Root.StrokeHovered = null;
                        TextTheta = 0;
                    }
                }
                else if (Root.EraserMode || Root.ToolSelected == Tools.Edit || Root.ToolSelected == Tools.Move ||
                         Root.ToolSelected == Tools.Copy || Root.LassoMode ||
                         Root.ToolSelected == Tools.Scale || Root.ToolSelected == Tools.Rotate)
                {
                    float pos;
                    if (NearestStroke(new Point(ex, ey), true, out Root.StrokeHovered, out pos, false) >
                        1 + Root.PixelToHiMetric(Root.MinMagneticRadius() / (Root.MagneticRadius >= 0 ^ ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0) ? 1 : 10)))
                    {
                        Root.StrokeHovered = null;
                        MemoHintClose = Root.Local.ButtonNameExit + " (" + Root.Hotkey_Close.ToString() + "/Alt+F4)";
                        this.toolTip.SetToolTip(this.btStop, MemoHintClose);
                        MetricToolTip.Hide(this);
                        return;
                    }
                    else if (Root.StrokeHovered?.Id != SavHoveredForSelection?.Id)
                    {
                        if (Root.MeasureEnabled)
                            MetricToolTip.Show(MeasureStroke(Root.StrokeHovered)
                                + (Root.LassoMode ? ("\n" + MeasureAllStrokes(StrokesSelection, InprogressSelection, Root.StrokeHovered)) : ""), this, ex, ey - 80);
                        return;
                    }
                    else
                    {
                        Root.StrokeHovered = null;
                        MetricToolTip.Hide(this);
                        return;
                    }
                }
                else
                {
                    if (!Root.MeasureWhileDrawing)
                        MetricToolTip.Hide(this);
                    return;
                }
            }

            Root.CursorX = ex;
            Root.CursorY = ey;

            if (ZoomCapturing)
            {
                if (Root.WindowRect.Width > 0)
                    Root.CursorY = (int)(Root.CursorY0 + (Root.CursorX - Root.CursorX0) / (1.0 * Width / Height) *
                        Math.Sign(Root.CursorY - Root.CursorY0) * Math.Sign(Root.CursorX - Root.CursorX0));
                else
                    Root.CursorY = (int)(Root.CursorY0 + (Root.CursorX - Root.CursorX0) / ZoomScreenRatio *
                        Math.Sign(Root.CursorY - Root.CursorY0) * Math.Sign(Root.CursorX - Root.CursorX0));
            }
            else if (Root.ToolSelected != Tools.Hand)
            {
                MagneticEffect(Root.CursorX0, Root.CursorY0, ref Root.CursorX, ref Root.CursorY, Root.ToolSelected > Tools.Hand && Root.MagneticRadius > 0);
            }

            Point currentxy = new Point(ex, ey);
            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref currentxy);

            if (Root.Snapping == 2)
            {
                int left = Math.Min(Root.SnappingX, ex);
                int top = Math.Min(Root.SnappingY, ey);
                int width = Math.Abs(Root.SnappingX - ex);
                int height = Math.Abs(Root.SnappingY - ey);
                Root.SnappingRect = new Rectangle(left, top, width, height);

                if (LasteXY != currentxy)
                    Root.MouseMovedUnderSnapshotDragging = true;
            }
            else if (Root.PanMode && Root.FingerInAction)
            {
                Root.Pan(currentxy.X - LasteXY.X, currentxy.Y - LasteXY.Y);
            }
            else if ((Root.ToolSelected == Tools.Move) || (Root.ToolSelected == Tools.Copy))
            {
                if (StrokesSelection.Count > 0)
                {
                    try
                    {
                        StrokesSelection.Move(currentxy.X - LasteXY.X, currentxy.Y - LasteXY.Y);
                        foreach (Stroke s in StrokesSelection)
                            MoveStrokeAndProperties(s, currentxy.X - LasteXY.X, currentxy.Y - LasteXY.Y, false);
                    }
                    catch { }
                    Root.FormDisplay.ClearCanvus();
                    Root.FormDisplay.DrawStrokes();
                    Root.FormDisplay.UpdateFormDisplay(true);
                }
                else if (movedStroke != null)
                {
                    MoveStrokeAndProperties(movedStroke, currentxy.X - LasteXY.X, currentxy.Y - LasteXY.Y, true);
                    Root.FormDisplay.ClearCanvus();
                    Root.FormDisplay.DrawStrokes();
                    Root.FormDisplay.UpdateFormDisplay(true);
                }
            }

            if (Root.ToolSelected == Tools.Scale && TransformXc != int.MinValue &&
                ((((int)CurrentMouseButton == 1)) || (CurrentMouseButton == MouseButtons.Left)))
            {
                Scale(StrokesSelection, movedStroke, TransformXc, TransformYc, LasteXY.X, LasteXY.Y, currentxy.X, currentxy.Y);
                Root.UponAllDrawingUpdate = true;
            }

            if (Root.ToolSelected == Tools.Rotate && TransformXc != int.MinValue &&
                ((((int)CurrentMouseButton == 1)) || (CurrentMouseButton == MouseButtons.Left)))
            {
                Rotate(StrokesSelection, movedStroke, TransformXc, TransformYc, LasteXY.X, LasteXY.Y, currentxy.X, currentxy.Y);
                Root.UponAllDrawingUpdate = true;
            }

            if (currentStroke != null && Root.MeasureEnabled &&
                !(Root.ToolSelected == Tools.Hand || Root.ToolSelected == Tools.HandFilledWhite || Root.ToolSelected == Tools.HandFilledBlack))
            {
                if ((DateTime.Now.Ticks - lastHintDraw) > (200 * 10000))
                {
                    string str = "?????";
                    double dx = Root.CursorX0 == int.MinValue ? 0 : ConvertMeasureLength(Math.Abs(Root.PixelToHiMetric(Root.CursorX - Root.CursorX0)));
                    double dy = Root.CursorY0 == int.MinValue ? 0 : ConvertMeasureLength(Math.Abs(Root.PixelToHiMetric(Root.CursorY - Root.CursorY0)));

                    switch (Root.ToolSelected)
                    {
                        case Tools.Hand:
                        case Tools.HandFilledWhite:
                        case Tools.HandFilledBlack:
                            str = string.Format(MeasureNumberFormat, Root.Local.FormatLength,
                                ConvertMeasureLength(StrokeLength(currentStroke)), Root.Measure2Unit);
                            break;
                        case Tools.Line:
                        case Tools.EndArrow:
                        case Tools.StartArrow:
                            str = string.Format(MeasureNumberFormat, Root.Local.FormatLength,
                                Math.Sqrt(dx * dx + dy * dy), Root.Measure2Unit);
                            break;
                        case Tools.Rect:
                            str = string.Format(MeasureNumberFormat, Root.Local.FormatRectSize, dx, dy, Root.Measure2Unit);
                            break;
                        case Tools.Oval:
                            str = string.Format(MeasureNumberFormat, Root.Local.FormatEllipseSize, dx, dy, Root.Measure2Unit);
                            break;
                        case Tools.Poly:
                            str = string.Format(MeasureNumberFormat, Root.Local.FormatLength,
                                ConvertMeasureLength(StrokeLength(PolyLineInProgress)) + Math.Sqrt(dx * dx + dy * dy), Root.Measure2Unit);
                            break;
                    }

                    MetricToolTip.Show(str, this, ex, ey - 80);
                    lastHintDraw = DateTime.Now.Ticks;
                }
            }

            HideMetricCountDown = 3000 / tiSlide.Interval;
            LasteXY = currentxy;
        }
    }
}
