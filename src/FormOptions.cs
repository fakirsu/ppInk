using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gInk
{
    public partial class FormOptions : Form
    {
        public Root Root;

        Label[] lbPens = new Label[Root.MaxPenCount];
        CheckBox[] cbPens = new CheckBox[Root.MaxPenCount];
        PictureBox[] pboxPens = new PictureBox[Root.MaxPenCount];
        ComboBox[] comboPensAlpha = new ComboBox[Root.MaxPenCount];
        ComboBox[] comboPensWidth = new ComboBox[Root.MaxPenCount];
        Panel[] comboPensLineStyle = new Panel[Root.MaxPenCount];
        CheckBox[] comboPensFading = new CheckBox[Root.MaxPenCount];

        Label[] lbHotkeyPens = new Label[Root.MaxPenCount];
        HotkeyInputBox[] hiPens = new HotkeyInputBox[Root.MaxPenCount];

        Bitmap[] ToolBarOrientationIcons = {
            gInk.Properties.Resources.toolbar2Left,
            gInk.Properties.Resources.toolbar2Right,
            gInk.Properties.Resources.toolbar2Up,
            gInk.Properties.Resources.toolbar2Down
        };

        // ################ goInk - START ####################
        private TabPage tabPageGridTags;
        private TabPage tabPageGoHotkeys;
        private Label lblHK_ShowWhite;
        private Label lblHK_ShowBlack;
        private Label lblHK_HideWhite;
        private Label lblHK_HideBlack;
        private HotkeyInputBox hiHK_ShowWhite;
        private HotkeyInputBox hiHK_ShowBlack;

        private HotkeyInputBox hiHK_HideWhite;
        private HotkeyInputBox hiHK_HideBlack;

        //Pour la couleur du texte
        private Button btnGoTextColor;
        private Label lblGoTextColor;

        // Ajout : champs Hotkeys pour les nouveaux tags GO
        private Label lblHK_LetterTag;
        private Label lblHK_SquareTag;
        private Label lblHK_TriangleTag;
        private Label lblHK_CircleTag;
        private Label lblHK_CrossTag;
        private HotkeyInputBox hiHK_LetterTag;
        private HotkeyInputBox hiHK_SquareTag;
        private HotkeyInputBox hiHK_TriangleTag;
        private HotkeyInputBox hiHK_CircleTag;
        private HotkeyInputBox hiHK_CrossTag;

        // --- Nouveaux raccourcis en tête de l’onglet Go ---
        private Label lblHK_OpenToolbar;
        private HotkeyInputBox hiHK_OpenToolbar;
        private Label lblHK_CloseToolbar;
        private HotkeyInputBox hiHK_CloseToolbar;


        private Label lblTagCirclePerc;
        private NumericUpDown nudTagCirclePerc;
        private Label lblTagSizePerc;
        private NumericUpDown nudTagSizePerc;
        private Label lblGridType;
        private ComboBox cbGridType;
        private Label lblTagOpacityPerc;
        private NumericUpDown nudTagOpacityPerc;
        private Label lblTagNumberOpacityPerc;

        // --- INSÉRER ) ---
        private Label lblHK_HandWhite;
        private Label lblHK_HandBlack;
        private HotkeyInputBox hiHK_HandWhite;
        private HotkeyInputBox hiHK_HandBlack;

        private Label lblGoFillOpacity;
        private NumericUpDown nudGoFillOpacity;
        private Label lblGoStrokeOpacity;
        private NumericUpDown nudGoStrokeOpacity;
        private Label lblGoStrokeWidth;
        private NumericUpDown nudGoStrokeWidth;

        private Label lblGoStrokeThickness;
        private ComboBox cbGoStrokeThickness;

        // en haut de FormOptions (déclarations)
        private Button btnGoColor_Letter;
        private Button btnGoColor_Square;
        private Button btnGoColor_Triangle;
        private Button btnGoColor_Circle;
        private Button btnGoColor_Cross;



        private NumericUpDown nudTagNumberOpacityPerc;

        // Déclarations de champs FormOptions (en haut du fichier)
        private Button btnGoArrowColor;
        private ComboBox cbGoArrowThickness;
        private ComboBox cbGoArrowLength;
        private Label lblGoArrowColor;
        private Label lblGoArrowThickness;
        private Label lblGoArrowLength;

        // ################ goInk - END ####################

        public FormOptions(Root root)
        {
            Root = root;
            InitializeComponent();

            // ##### goInk - START #####
            tabPageGridTags = new TabPage { Text = "Jeu de go" };
            tabPageGoHotkeys = new TabPage { Text = "Jeu de go - Raccourcis" };

            int baseLeft = 12;
            int col2 = 260;
            int lineH = 30;
            int top0 = 16;

            lblHK_ShowWhite = new Label { Left = baseLeft, Top = top0, AutoSize = true, Text = "Numérotée (1 blanche)" };
            hiHK_ShowWhite = new HotkeyInputBox { Left = col2, Top = top0 - 3, Width = 180 };
            lblHK_ShowBlack = new Label { Left = baseLeft, Top = top0 + lineH, AutoSize = true, Text = "Numérotée (1 noire)" };
            hiHK_ShowBlack = new HotkeyInputBox { Left = col2, Top = top0 + lineH - 3, Width = 180 };
            lblHK_HideWhite = new Label { Left = baseLeft, Top = top0 + 2 * lineH, AutoSize = true, Text = "Vide (1 blanche)" };
            hiHK_HideWhite = new HotkeyInputBox { Left = col2, Top = top0 + 2 * lineH - 3, Width = 180 };
            lblHK_HideBlack = new Label { Left = baseLeft, Top = top0 + 3 * lineH, AutoSize = true, Text = "Vide (1 noire)" };
            hiHK_HideBlack = new HotkeyInputBox { Left = col2, Top = top0 + 3 * lineH - 3, Width = 180 };

            hiHK_ShowWhite.OnHotkeyChanged += hi_OnHotkeyChanged;
            hiHK_ShowBlack.OnHotkeyChanged += hi_OnHotkeyChanged;
            hiHK_HideWhite.OnHotkeyChanged += hi_OnHotkeyChanged;
            hiHK_HideBlack.OnHotkeyChanged += hi_OnHotkeyChanged;

            //tabPageGoHotkeys.Controls.AddRange(new Control[]


            // --- INSÉRER dans le constructeur FormOptions(Root root), après la création des hiHK_Show... et l'ajout à tabPageGoHotkeys ---
            int yExtra = lblHK_HideBlack.Bottom + 12;
            lblHK_HandWhite = new Label { Left = baseLeft, Top = yExtra, AutoSize = true, Text = "Main — Rempli blanc (hotkey)" };
            hiHK_HandWhite = new HotkeyInputBox { Left = col2, Top = yExtra - 3, Width = 180 };
            yExtra += lineH;
            lblHK_HandBlack = new Label { Left = baseLeft, Top = yExtra, AutoSize = true, Text = "Main — Rempli noir (hotkey)" };
            hiHK_HandBlack = new HotkeyInputBox { Left = col2, Top = yExtra - 3, Width = 180 };





            hiHK_HandWhite.OnHotkeyChanged += (s, e) =>
            {
                try { Root.Hotkey_HandFilledWhite = hiHK_HandWhite.Hotkey; } catch { }
                try { Root.SetHotkey(); } catch { }
                try { Root.SaveOptions(Program.RunningFolder + "hotkeys.ini"); } catch { }
                // Conflict visual update (reuse existing helper)
                hi_OnHotkeyChanged(s, e);
            };
            hiHK_HandBlack.OnHotkeyChanged += (s, e) =>
            {
                try { Root.Hotkey_HandFilledBlack = hiHK_HandBlack.Hotkey; } catch { }
                try { Root.SetHotkey(); } catch { }
                try { Root.SaveOptions(Program.RunningFolder + "hotkeys.ini"); } catch { }
                hi_OnHotkeyChanged(s, e);
            };



            // --- Création contrôles Hotkey nouveaux tags Go (à insérer dans le constructeur, après hiHK_HandBlack)
            int y = lblHK_HandBlack.Bottom + 12;
            int col = 260;
            int lh = 28;

            lblHK_LetterTag = new Label { Left = 12, Top = y, AutoSize = true, Text = "Tag Lettre" };
            hiHK_LetterTag = new HotkeyInputBox { Left = col, Top = y - 3, Width = 180 };
            y += lh;

            lblHK_SquareTag = new Label { Left = 12, Top = y, AutoSize = true, Text = "Tag Carré" };
            hiHK_SquareTag = new HotkeyInputBox { Left = col, Top = y - 3, Width = 180 };
            y += lh;

            lblHK_TriangleTag = new Label { Left = 12, Top = y, AutoSize = true, Text = "Tag Triangle" };
            hiHK_TriangleTag = new HotkeyInputBox { Left = col, Top = y - 3, Width = 180 };
            y += lh;

            lblHK_CircleTag = new Label { Left = 12, Top = y, AutoSize = true, Text = "Tag Cercle" };
            hiHK_CircleTag = new HotkeyInputBox { Left = col, Top = y - 3, Width = 180 };
            y += lh;

            lblHK_CrossTag = new Label { Left = 12, Top = y, AutoSize = true, Text = "Tag Croix" };
            hiHK_CrossTag = new HotkeyInputBox { Left = col, Top = y - 3, Width = 180 };
            y += lh;

            // hookup events
            hiHK_LetterTag.OnHotkeyChanged += (s, e) => { try { Root.Hotkey_LetterTag = hiHK_LetterTag.Hotkey; Root.SetHotkey(); Root.SaveOptions(Program.RunningFolder + "hotkeys.ini"); } catch { } hi_OnHotkeyChanged(s, e); };
            hiHK_SquareTag.OnHotkeyChanged += (s, e) => { try { Root.Hotkey_SquareTag = hiHK_SquareTag.Hotkey; Root.SetHotkey(); Root.SaveOptions(Program.RunningFolder + "hotkeys.ini"); } catch { } hi_OnHotkeyChanged(s, e); };
            hiHK_TriangleTag.OnHotkeyChanged += (s, e) => { try { Root.Hotkey_TriangleTag = hiHK_TriangleTag.Hotkey; Root.SetHotkey(); Root.SaveOptions(Program.RunningFolder + "hotkeys.ini"); } catch { } hi_OnHotkeyChanged(s, e); };
            hiHK_CircleTag.OnHotkeyChanged += (s, e) => { try { Root.Hotkey_CircleTag = hiHK_CircleTag.Hotkey; Root.SetHotkey(); Root.SaveOptions(Program.RunningFolder + "hotkeys.ini"); } catch { } hi_OnHotkeyChanged(s, e); };
            hiHK_CrossTag.OnHotkeyChanged += (s, e) => { try { Root.Hotkey_CrossTag = hiHK_CrossTag.Hotkey; Root.SetHotkey(); Root.SaveOptions(Program.RunningFolder + "hotkeys.ini"); } catch { } hi_OnHotkeyChanged(s, e); };

            // add to tab
            tabPageGoHotkeys.Controls.AddRange(new Control[]
            {
    lblHK_LetterTag, hiHK_LetterTag,
    lblHK_SquareTag, hiHK_SquareTag,
    lblHK_TriangleTag, hiHK_TriangleTag,
    lblHK_CircleTag, hiHK_CircleTag,
    lblHK_CrossTag, hiHK_CrossTag
            });




            // après avoir créé tous les HotkeyInputBox (hiHK_ShowWhite, hiHK_ShowBlack, hiHK_HideWhite, hiHK_HideBlack, hiHK_HandWhite, hiHK_HandBlack)
            tabPageGoHotkeys.Controls.AddRange(new Control[]
            {
    lblHK_ShowWhite, hiHK_ShowWhite,
    lblHK_ShowBlack, hiHK_ShowBlack,
    lblHK_HideWhite, hiHK_HideWhite,
    lblHK_HideBlack, hiHK_HideBlack,
    lblHK_HandWhite, hiHK_HandWhite,
    lblHK_HandBlack, hiHK_HandBlack
            });


            try
            {
                if (!VideoTabCtrl.TabPages.Contains(tabPageGoHotkeys))
                    VideoTabCtrl.TabPages.Add(tabPageGoHotkeys);
            }
            catch { }

            lblTagCirclePerc = new Label
            {
                //Text = "Diamètre des pierres (%) :",
                Text = Root.Local.OptionsTagCirclePerc ?? "Diamètre des pierres (%) :",
                AutoSize = true,
                Left = 12,
                Top = 16
            };
            nudTagCirclePerc = new NumericUpDown
            {
                Minimum = 10,
                Maximum = 300,
                DecimalPlaces = 1,
                Increment = 1,
                Left = 220,
                Top = lblTagCirclePerc.Top - 3,
                Width = 90,
                Value = (decimal)Root.TagCirclePercent
            };

            lblTagSizePerc = new Label
            {
                //Text = "Taille des numéros (%) :",
                Text = Root.Local.OptionsTagSizePerc ?? "Taille des numéros (%) :",
                AutoSize = true,
                Left = 12,
                Top = lblTagCirclePerc.Bottom + 18
            };
            nudTagSizePerc = new NumericUpDown
            {
                Minimum = 10,
                Maximum = 500,
                DecimalPlaces = 1,
                Increment = 1,
                Left = 220,
                Top = lblTagSizePerc.Top - 3,
                Width = 90,
                Value = (decimal)Root.TagSizePercent
            };

            //lblTagOpacityPerc = new Label
            //{
            //    Text = "Opacité des pierres (%) :",
            //    AutoSize = true,
            //    Left = 12,
            //    Top = lblTagSizePerc.Bottom + 18
            //};
            lblTagOpacityPerc = new Label
            {
                //Text = Root.Local.OptionsTagOpacityPerc ?? "Opacité des pierres (%) :",
                Text = Root.Local.OptionsTagOpacityPerc ?? "Opacité des pierres (%) :",
                AutoSize = true,
                Left = 12,
                Top = lblTagSizePerc.Bottom + 18
            };


            nudTagOpacityPerc = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                DecimalPlaces = 0,
                Increment = 1,
                Left = 220,
                Top = lblTagOpacityPerc.Top - 3,
                Width = 90,
                Value = (decimal)Root.TagStoneOpacityPercent
            };

            lblTagNumberOpacityPerc = new Label
            {
                //Text = "Opacité des numéros (%) :",
                Text = Root.Local.OptionsTagNumberOpacityPerc ?? "Opacité des numéros (%) :",
                AutoSize = true,
                Left = 12,
                Top = lblTagOpacityPerc.Bottom + 18
            };
            //{
            //    Minimum = 0,
            //    Maximum = 100,
            //    DecimalPlaces = 0,
            //    Increment = 1,
            //    Left = 220,
            //    Top = lblTagNumberOpacityPerc.Top - 3,
            //    Width = 90,
            //    Value = (decimal)Root.TagNumberOpacityPercent
            //};

            nudTagNumberOpacityPerc = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                DecimalPlaces = 0,
                Increment = 1,
                Left = 220,
                Top = lblTagNumberOpacityPerc.Top - 3,
                Width = 90,
                Value = (decimal)Root.TagNumberOpacityPercent
            };

            lblGridType = new Label
            {
                Text = "Type de goban :",
                AutoSize = true,
                Left = 12,
                Top = nudTagNumberOpacityPerc.Bottom + 20
            };
            cbGridType = new ComboBox
            {
                Left = 220,
                Top = lblGridType.Top - 3,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbGridType.Items.AddRange(new object[] { "19 x 19", "13 x 13", "9 x 9" });
            if (Root.GridRows == 13) cbGridType.SelectedIndex = 1;
            else if (Root.GridRows == 9) cbGridType.SelectedIndex = 2;
            else cbGridType.SelectedIndex = 0;

            //nudTagCirclePerc.ValueChanged += (s, e) => Root.TagCirclePercent = (double)nudTagCirclePerc.Value;
            //nudTagSizePerc.ValueChanged += (s, e) => Root.TagSizePercent = (double)nudTagSizePerc.Value;
            //nudTagOpacityPerc.ValueChanged += (s, e) => Root.TagStoneOpacityPercent = (double)nudTagOpacityPerc.Value;
            //nudTagNumberOpacityPerc.ValueChanged += (s, e) => Root.TagNumberOpacityPercent = (double)nudTagNumberOpacityPerc.Value;
            //cbGridType.SelectedIndexChanged += (s, e) =>
            //{
            //    int v = (cbGridType.SelectedIndex == 0) ? 19 : (cbGridType.SelectedIndex == 1 ? 13 : 9);
            //    Root.GridRows = v;
            //    Root.GridCols = v;
            //};

            nudTagCirclePerc.ValueChanged += (s, e) =>
            {
                Root.TagCirclePercent = (double)nudTagCirclePerc.Value;
                ScheduleConfigSave();
            };
            nudTagSizePerc.ValueChanged += (s, e) =>
            {
                Root.TagSizePercent = (double)nudTagSizePerc.Value;
                ScheduleConfigSave();
            };
            nudTagOpacityPerc.ValueChanged += (s, e) =>
            {
                Root.TagStoneOpacityPercent = (double)nudTagOpacityPerc.Value;
                ScheduleConfigSave();
            };
            nudTagNumberOpacityPerc.ValueChanged += (s, e) =>
            {
                Root.TagNumberOpacityPercent = (double)nudTagNumberOpacityPerc.Value;
                ScheduleConfigSave();
            };
            cbGridType.SelectedIndexChanged += (s, e) =>
            {
                int v = (cbGridType.SelectedIndex == 0) ? 19 : (cbGridType.SelectedIndex == 1 ? 13 : 9);
                Root.GridRows = v;
                Root.GridCols = v;
                ScheduleConfigSave();
            };


            tabPageGridTags.Controls.Add(lblTagCirclePerc);
            tabPageGridTags.Controls.Add(nudTagCirclePerc);
            tabPageGridTags.Controls.Add(lblTagSizePerc);
            tabPageGridTags.Controls.Add(nudTagSizePerc);
            tabPageGridTags.Controls.Add(lblTagOpacityPerc);
            tabPageGridTags.Controls.Add(nudTagOpacityPerc);
            tabPageGridTags.Controls.Add(lblTagNumberOpacityPerc);
            tabPageGridTags.Controls.Add(nudTagNumberOpacityPerc);
            tabPageGridTags.Controls.Add(lblGridType);
            // Go : contrôles de remplissage / contour / largeur
            lblGoFillOpacity = new Label { Text = "Opacité remplissage (%) :", AutoSize = true, Left = 12, Top = lblGridType.Bottom + 18 };
            nudGoFillOpacity = new NumericUpDown { Minimum = 0, Maximum = 100, DecimalPlaces = 0, Increment = 1, Left = 220, Top = lblGoFillOpacity.Top - 3, Width = 90, Value = (decimal)Root.GoFillOpacityPercent };
            nudGoFillOpacity.ValueChanged += (s, e) =>
            {
                Root.GoFillOpacityPercent = (int)nudGoFillOpacity.Value;
                try { Root.SaveOptions(Program.RunningFolder + "config.ini"); } catch { }
            };

            lblGoStrokeOpacity = new Label { Text = "Opacité contour (%) :", AutoSize = true, Left = 12, Top = lblGoFillOpacity.Bottom + 18 };
            nudGoStrokeOpacity = new NumericUpDown { Minimum = 0, Maximum = 100, DecimalPlaces = 0, Increment = 1, Left = 220, Top = lblGoStrokeOpacity.Top - 3, Width = 90, Value = (decimal)Root.GoStrokeOpacityPercent };
            nudGoStrokeOpacity.ValueChanged += (s, e) =>
            {
                Root.GoStrokeOpacityPercent = (int)nudGoStrokeOpacity.Value;
                try { Root.SaveOptions(Program.RunningFolder + "config.ini"); } catch { }
            };

            lblGoStrokeWidth = new Label { Text = "Épaisseur contour (HiMetric) :", AutoSize = true, Left = 12, Top = lblGoStrokeOpacity.Bottom + 18 };
            nudGoStrokeWidth = new NumericUpDown { Minimum = 1, Maximum = 3000, DecimalPlaces = 1, Increment = 1, Left = 220, Top = lblGoStrokeWidth.Top - 3, Width = 90, Value = (decimal)Root.GoStrokeWidth };
            nudGoStrokeWidth.ValueChanged += (s, e) =>
            {
                Root.GoStrokeWidth = (float)nudGoStrokeWidth.Value;
                try { Root.SaveOptions(Program.RunningFolder + "config.ini"); } catch { }
            };

            //lblGoStrokeThickness = new Label { Text = "Épaisseur contour :", AutoSize = true, Left = 12, Top = lblGoStrokeWidth.Bottom + 18 };
            //cbGoStrokeThickness = new ComboBox { Left = 220, Top = lblGoStrokeThickness.Top - 3, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            //cbGoStrokeThickness.Items.AddRange(new object[] { "Fin", "Moyen", "Épais" });
            //cbGoStrokeThickness.SelectedIndex = Math.Min(Math.Max(0, Root.GoStrokeThickness), 2);
            //cbGoStrokeThickness.SelectedIndexChanged += (s, e) =>
            //{
            //    Root.GoStrokeThickness = cbGoStrokeThickness.SelectedIndex;
            //    ScheduleConfigSave();
            //};
            //tabPageGridTags.Controls.Add(lblGoStrokeThickness);
            //tabPageGridTags.Controls.Add(cbGoStrokeThickness);

            lblGoStrokeThickness = new Label { Text = Root.Local.OptionsGoStrokeThickness ?? "Épaisseur contour :", AutoSize = true, Left = 12, Top = lblGoStrokeWidth.Bottom + 18 };
            cbGoStrokeThickness = new ComboBox { Left = 220, Top = lblGoStrokeThickness.Top - 3, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cbGoStrokeThickness.Items.AddRange(new object[] { Root.Local.OptionsPensThin ?? "Thin", Root.Local.OptionsPensNormal ?? "Normal", Root.Local.OptionsPensThick ?? "Thick" });
            cbGoStrokeThickness.SelectedIndex = Math.Min(Math.Max(0, Root.GoStrokeThickness), 2);
            cbGoStrokeThickness.SelectedIndexChanged += (s, e) =>
            {
                Root.GoStrokeThickness = cbGoStrokeThickness.SelectedIndex;
                ScheduleConfigSave();
            };
            tabPageGridTags.Controls.Add(lblGoStrokeThickness);
            tabPageGridTags.Controls.Add(cbGoStrokeThickness);



            // création boutons couleurs tags (petits carrés cliquables)
            int colorBtnLeft = cbGoStrokeThickness.Left + cbGoStrokeThickness.Width + 16;
            int colorTop = lblGoStrokeThickness.Top - 3;

            btnGoColor_Letter = new Button { Left = colorBtnLeft, Top = colorTop, Width = 24, Height = 24, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(Root.GoTool_Letter_Color[0], Root.GoTool_Letter_Color[1], Root.GoTool_Letter_Color[2], Root.GoTool_Letter_Color[3]) };
            btnGoColor_Square = new Button { Left = colorBtnLeft + 28, Top = colorTop, Width = 24, Height = 24, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(Root.GoTool_Square_Color[0], Root.GoTool_Square_Color[1], Root.GoTool_Square_Color[2], Root.GoTool_Square_Color[3]) };
            btnGoColor_Triangle = new Button { Left = colorBtnLeft + 56, Top = colorTop, Width = 24, Height = 24, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(Root.GoTool_Triangle_Color[0], Root.GoTool_Triangle_Color[1], Root.GoTool_Triangle_Color[2], Root.GoTool_Triangle_Color[3]) };
            btnGoColor_Circle = new Button { Left = colorBtnLeft + 84, Top = colorTop, Width = 24, Height = 24, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(Root.GoTool_Circle_Color[0], Root.GoTool_Circle_Color[1], Root.GoTool_Circle_Color[2], Root.GoTool_Circle_Color[3]) };
            btnGoColor_Cross = new Button { Left = colorBtnLeft + 112, Top = colorTop, Width = 24, Height = 24, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(Root.GoTool_Cross_Color[0], Root.GoTool_Cross_Color[1], Root.GoTool_Cross_Color[2], Root.GoTool_Cross_Color[3]) };

            // handlers (réutilise PenModifyDlg pour choisir couleur + transparence)
            btnGoColor_Letter.Click += (s, e) => EditGoTagColor("Letter", btnGoColor_Letter, Root.GoTool_Letter_Color);
            btnGoColor_Square.Click += (s, e) => EditGoTagColor("Square", btnGoColor_Square, Root.GoTool_Square_Color);
            btnGoColor_Triangle.Click += (s, e) => EditGoTagColor("Triangle", btnGoColor_Triangle, Root.GoTool_Triangle_Color);
            btnGoColor_Circle.Click += (s, e) => EditGoTagColor("Circle", btnGoColor_Circle, Root.GoTool_Circle_Color);
            btnGoColor_Cross.Click += (s, e) => EditGoTagColor("Cross", btnGoColor_Cross, Root.GoTool_Cross_Color);

            tabPageGridTags.Controls.AddRange(new Control[] { btnGoColor_Letter, btnGoColor_Square, btnGoColor_Triangle, btnGoColor_Circle, btnGoColor_Cross });



            tabPageGridTags.Controls.Add(lblGoFillOpacity);
            tabPageGridTags.Controls.Add(nudGoFillOpacity);
            tabPageGridTags.Controls.Add(lblGoStrokeOpacity);
            tabPageGridTags.Controls.Add(nudGoStrokeOpacity);
            tabPageGridTags.Controls.Add(lblGoStrokeWidth);
            tabPageGridTags.Controls.Add(nudGoStrokeWidth);


            tabPageGridTags.Controls.Add(cbGridType);

            try
            {
                if (!VideoTabCtrl.TabPages.Contains(tabPageGridTags))
                    VideoTabCtrl.TabPages.Add(tabPageGridTags);
            }
            catch { }
            // ##### goInk - END #####

            int offset = PenPanel.Left;
            PenPanel.Left = 0;
            PenPanel.Width += offset;

            for (int p = 0; p < Root.MaxPenCount; p++)
            {
                lbPens[p] = new Label();

                cbPens[p] = new CheckBox();
                cbPens[p].CheckedChanged += cbPens_CheckedChanged;

                pboxPens[p] = new PictureBox();
                pboxPens[p].Click += pboxPens_Click;

                comboPensAlpha[p] = new ComboBox();
                comboPensAlpha[p].TextChanged += comboPensAlpha_TextChanged;

                comboPensWidth[p] = new ComboBox();
                comboPensWidth[p].TextChanged += comboPensWidth_TextChanged;

                comboPensLineStyle[p] = new Panel();
                comboPensLineStyle[p].Click += comboPensLineStyle_Changed;

                comboPensFading[p] = new CheckBox();
                comboPensFading[p].CheckedChanged += comboPensFading_Changed;

                PenPanel.Controls.Add(lbPens[p]);
                PenPanel.Controls.Add(cbPens[p]);
                PenPanel.Controls.Add(pboxPens[p]);
                PenPanel.Controls.Add(comboPensAlpha[p]);
                PenPanel.Controls.Add(comboPensWidth[p]);
                PenPanel.Controls.Add(comboPensLineStyle[p]);
                PenPanel.Controls.Add(comboPensFading[p]);

                if (p >= Root.MaxDisplayedPens)
                    continue;

                lbHotkeyPens[p] = new Label();
                hiPens[p] = new HotkeyInputBox();
                hiPens[p].OnHotkeyChanged += hi_OnHotkeyChanged;
                tabPage3.Controls.Add(lbHotkeyPens[p]);
                tabPage3.Controls.Add(hiPens[p]);
            }
        }

        // --- Auto-save différée pour onglet Jeu de go ---
        private Timer _goDeferredSaveTimer;
        private void ScheduleConfigSave()
        {
            if (Root == null) return;
            if (_goDeferredSaveTimer == null)
            {
                _goDeferredSaveTimer = new Timer();
                _goDeferredSaveTimer.Interval = 500; // ms
                _goDeferredSaveTimer.Tick += (s, e) =>
                {
                    _goDeferredSaveTimer.Stop();
                    TrySaveGoConfig();
                };
            }
            _goDeferredSaveTimer.Stop();
            _goDeferredSaveTimer.Start();
        }
        private void TrySaveGoConfig()
        {
            try { Root.SaveOptions(Program.RunningFolder + "config.ini"); }
            catch { /* silencieux */ }
        }

        private void FormOptions_Load(object sender, EventArgs e)
        {
            ToolbarDwg.BackColor = Color.FromArgb(Root.ToolbarBGColor[0], Root.ToolbarBGColor[1], Root.ToolbarBGColor[2], Root.ToolbarBGColor[3]);
            ToolbarOrientationBtn.BackgroundImage = ToolBarOrientationIcons[Root.ToolbarOrientation];

            if (Root.StampFileNames.Count != Root.FormCollection.ClipartsDlg.ImageListViewer.Items.Count
                && MessageBox.Show(Root.Local.QuestionClipArtUpdate, "ppInk", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Root.StampFileNames.Clear();
                foreach (ListViewItem it in Root.FormCollection.ClipartsDlg.ImageListViewer.Items)
                    Root.StampFileNames.Add(it.ImageKey);
            }



            InitClipArtButtons();

            SubToolsBar_cb.Checked = Root.SubToolsEnabled;
            PensOnTwoLinesCb.Checked = Root.PensOnTwoLines;
            cbEraserEnabled.Checked = Root.EraserEnabled;
            cbPointerEnabled.Checked = Root.PointerEnabled;
            cbSnapEnabled.Checked = Root.SnapEnabled;
            cbUndoEnabled.Checked = Root.UndoEnabled;
            cbClearEnabled.Checked = Root.ClearEnabled;
            WidthAtPenSelCb.Checked = Root.WidthAtPenSel;
            cbWidthEnabled.Checked = Root.PenWidthEnabled;
            cbPanEnabled.Checked = Root.PanEnabled;
            cbInkVisibleEnabled.Checked = Root.InkVisibleEnabled;
            cbToolsEnabled.Checked = Root.ToolsEnabled;
            cbWhiteIcon.Checked = Root.WhiteTrayIcon;
            cbAllowDragging.Checked = Root.AllowDraggingToolbar;
            cbAllowHotkeyInPointer.Checked = Root.AllowHotkeyInPointerMode;
            cbPagesEnabled.Checked = Root.PagesEnabled;
            cbLoadSaveEnabled.Checked = Root.LoadSaveEnabled;
            ColorPickerEnaCb.Checked = Root.ColorPickerEnabled;
            SwapSnapsBehviorsCb.Checked = !Root.SwapSnapsBehaviors;
            AltTabActivateCb.Checked = Root.AltTabPointer;
            KeepUnfoldedPointerCb.Checked = Root.KeepUnDockedAtPointer;

            TextBackgroundLst.SelectedIndex = Root.TextBackground;

            MeasureEnabledCb.Checked = Root.MeasureEnabled;
            Measure2ScaleEd.Text = Root.Measure2Scale.ToString();
            Measure2DigEd.Text = Root.Measure2Digits.ToString();
            Measure2UnitEd.Text = Root.Measure2Unit;
            MeasureAngleCb.Checked = Root.MeasureAnglCounterClockwise;
            MeasureWhileDrawing.Checked = Root.MeasureWhileDrawing;

            APIRestEd.Text = Root.APIRestUrl;
            APIRestEd.BackColor = Root.APIRest.IsListening() ? Color.White : Color.Orange;

            ToolBarHeight.Text = string.Format("{0:F1}", Root.ToolbarHeight * 100);
            comboCanvasCursor.SelectedIndex = Root.CanvasCursor;

            BoardAtOpenCombo.SelectedIndex = Root.BoardAtOpening;
            BoardCustColorPnl.BackColor = Color.FromArgb(Root.Gray1[0], Root.Gray1[1], Root.Gray1[2], Root.Gray1[3]);

            tbSnapPath.Text = Root.SnapshotBasePath;
            tbSnapFileTemplate.Text = Root.SnapshotFileTemplate;
            OpenIntoSnapCb.Checked = Root.OpenIntoSnapMode;
            StartFoldedCb.Checked = Root.KeepDockedAtOpen;
            ShowFloatingWinCb.Checked = Root.FormOpacity > 0;
            ArrHdAperture.Text = (Root.ArrowAngle * 180.0 / Math.PI).ToString("#0", CultureInfo.InvariantCulture);
            ArrHdLength.Text = (Root.ArrowLen / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString("#0.0000", CultureInfo.InvariantCulture);
            Magnet_TB.Text = (Root.MagneticRadius / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString("#0.0000", CultureInfo.InvariantCulture);
            MagnetAngleEd.Text = Root.MagneticAngle.ToString(CultureInfo.InvariantCulture);
            DefArrStartCb.Checked = Root.DefaultArrow_start;

            ZoomEnabledCb.SelectedIndex = Root.ZoomEnabled;
            ZoomWidthEd.Text = Root.ZoomWidth.ToString();
            ZoomHeightEd.Text = Root.ZoomHeight.ToString();
            ZoomScaleEd.Text = Root.ZoomScale.ToString(CultureInfo.InvariantCulture);
            ZoomContinousCb.Checked = Root.ZoomContinous;

            SpotColorPnl.BackColor = Root.SpotLightColor;
            SpotOnAltCb.Checked = Root.SpotOnAlt;
            SpotRadTb.Text = (Root.SpotLightRadius / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString("#0.0", CultureInfo.InvariantCulture);

            CaptStrokesOnlyCb.Checked = Root.StrokesOnlySnapshot;

            InitDynamicPenLines();

            FadingTimeEd.Text = Root.TimeBeforeFading.ToString(CultureInfo.InvariantCulture);
            InverseWheelCb.Checked = Root.InverseMousewheel;
            FitToCurveEd.Checked = Root.FitToCurve;
            Click4StrokeCb.Checked = Root.ButtonClick_For_LineStyle;
            ExtraPensCb.Checked = Root.PensExtraSet;

            InitHotkeysPens();

            SetAltAsOneCommandState();

            SnapInPointerHoldCb.SelectedIndex = (int)Root.SnapInPointerHoldKey;
            SnapInPointerTwiceCb.SelectedIndex = (int)Root.SnapInPointerPressTwiceKey;

            AssignHotkeys();

            InitLineStyleRotateChecks();

            WsUrlTxt.Text = Root.ObsUrl;
            WsPwdTxt.Text = Root.ObsPwd;
            FfmegFileNameTxt.Text = Root.FFMpegFileName;
            FfmpegCmdTxt.Text = Root.FFMpegCmd;

            CreateM3u.Checked = Root.CreateM3U;
            M3UIndexOnUndockCb.Checked = Root.CreateIndexOnUndock;
            hiCreateM3UIndex.Hotkey = Root.Hotkey_CreateIndex;
            M3UIndexDefaultTxt.Text = Root.IndexDefaultText;
            UndockOnM3UIndexCb.Checked = Root.UndockOnIndexCreate;
            NoEditM3UIndexCb.Checked = Root.NoEditM3UEntry;



            //// --- Flèches (Jeu de go)
            //lblGoArrowColor = new Label { AutoSize = true, Text = Root.Local.GoOptionsArrowColor };
            //btnGoArrowColor = new Button { Width = 80 };
            //btnGoArrowColor.Click += (s, e2) =>
            //{
            //    using (var cd = new ColorDialog())
            //    {
            //        cd.Color = Root.GetArrowColor();
            //        if (cd.ShowDialog() == DialogResult.OK)
            //        {
            //            Root.GoArrowColorArgb = cd.Color.ToArgb();
            //            btnGoArrowColor.BackColor = cd.Color;
            //        }
            //    }
            //};

            //lblGoArrowThickness = new Label { AutoSize = true, Text = Root.Local.GoOptionsArrowThickness };
            //cbGoArrowThickness = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            //cbGoArrowThickness.Items.AddRange(Root.Local.GoOptionsArrowThicknessItems.Split(';'));
            //cbGoArrowThickness.SelectedIndex = Math.Max(0, Math.Min(2, Root.GoArrowThickness));
            //cbGoArrowThickness.SelectedIndexChanged += (s, e2) =>
            //{
            //    Root.GoArrowThickness = cbGoArrowThickness.SelectedIndex;
            //};

            //lblGoArrowLength = new Label { AutoSize = true, Text = Root.Local.GoOptionsArrowLength };
            //cbGoArrowLength = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            //cbGoArrowLength.Items.AddRange(Root.Local.GoOptionsArrowLengthItems.Split(';'));
            //cbGoArrowLength.SelectedIndex = Math.Max(0, Math.Min(2, Root.GoArrowLength));
            //cbGoArrowLength.SelectedIndexChanged += (s, e2) =>
            //{
            //    Root.GoArrowLength = cbGoArrowLength.SelectedIndex;
            //};

            //// Placement sommaire (adapter au layout existant)
            ////var top = /* calculez une Y libre après les autres options go */;
            //int top = lblGridType.Bottom + 18;
            //lblGoArrowColor.Left = 20; lblGoArrowColor.Top = top;
            //btnGoArrowColor.Left = 250; btnGoArrowColor.Top = top - 4;
            //btnGoArrowColor.BackColor = Root.GetArrowColor();

            //lblGoArrowThickness.Left = 20; lblGoArrowThickness.Top = top + 30;
            //cbGoArrowThickness.Left = 250; cbGoArrowThickness.Top = top + 26;

            //lblGoArrowLength.Left = 20; lblGoArrowLength.Top = top + 60;
            //cbGoArrowLength.Left = 250; cbGoArrowLength.Top = top + 56;

            //// Ajout sur le TabPage “Jeu de go”
            //tabPageGridTags.Controls.Add(lblGoArrowColor);
            //tabPageGridTags.Controls.Add(btnGoArrowColor);
            //tabPageGridTags.Controls.Add(lblGoArrowThickness);
            //tabPageGridTags.Controls.Add(cbGoArrowThickness);
            //tabPageGridTags.Controls.Add(lblGoArrowLength);
            //tabPageGridTags.Controls.Add(cbGoArrowLength);


            switch (Root.VideoRecordMode)
            {
                case VideoRecordMode.NoVideo: OptNoVideo.Checked = true; break;
                case VideoRecordMode.OBSRec: OptObsRecord.Checked = true; break;
                case VideoRecordMode.OBSBcst: OptObsBcast.Checked = true; break;
                case VideoRecordMode.FfmpegRec: OptFfmpeg.Checked = true; break;
                default: throw new Exception("unk video recording mode");
            }

            FormOptions_LocalReload();



            // <-- AJOUTER CETTE LIGNE ICI (initialise les boutons couleurs des tags)
            InitGoTagButtons();


            // --- Flèches (Jeu de go) : ajouté en fin d'onglet pour éviter recouvrement
            lblGoArrowColor = new Label { AutoSize = true, Text = Root.Local.GoOptionsArrowColor ?? "Arrow color :" };
            btnGoArrowColor = new Button { Width = 80 };
            //btnGoArrowColor.Click += (s, e2) =>
            //{
            //    using (var cd = new ColorDialog())
            //    {
            //        cd.Color = Root.GetArrowColor();
            //        if (cd.ShowDialog() == DialogResult.OK)
            //        {
            //            Root.GoArrowColorArgb = cd.Color.ToArgb();
            //            btnGoArrowColor.BackColor = cd.Color;
            //            ScheduleConfigSave();
            //        }
            //    }
            //};
            //btnGoArrowColor.Click += (s, e2) =>
            //{
            //    PenModifyDlg dlg = new PenModifyDlg(Root);
            //    // Préparer DrawingAttributes initial à partir de la couleur actuelle
            //    Color cur = Root.GetArrowColor();
            //    Microsoft.Ink.DrawingAttributes at = new Microsoft.Ink.DrawingAttributes
            //    {
            //        Transparency = (byte)(255 - cur.A),
            //        Color = Color.FromArgb(cur.A, cur.R, cur.G, cur.B),
            //        Width = 0
            //    };

            //    if (dlg.ModifyPen(ref at))
            //    {
            //        // at.Color.A contient l'alpha choisi ; on stocke l'ARGB
            //        int alpha = at.Color.A;
            //        Root.GoArrowColorArgb = Color.FromArgb(alpha, at.Color.R, at.Color.G, at.Color.B).ToArgb();

            //        // aperçu bouton (BackColor ignore l'alpha mais montre la couleur)
            //        try { btnGoArrowColor.BackColor = Color.FromArgb(alpha, at.Color.R, at.Color.G, at.Color.B); } catch { btnGoArrowColor.BackColor = at.Color; }

            //        ScheduleConfigSave();
            //    }
            //    dlg.Dispose();
            //};
            btnGoArrowColor.Click += (s, e2) =>
            {
                PenModifyDlg dlg = new PenModifyDlg(Root);
                // Préparer DrawingAttributes initial à partir de la couleur actuelle
                Color cur = Root.GetArrowColor();
                Microsoft.Ink.DrawingAttributes at = new Microsoft.Ink.DrawingAttributes
                {
                    // DrawingAttributes utilise Transparency = 255 - alpha
                    Transparency = (byte)(255 - cur.A),
                    // stocker la couleur RGB (l'alpha proviendra de Transparency)
                    Color = Color.FromArgb(cur.R, cur.G, cur.B),
                    // éviter Width = 0 (boîte de confirmation) -> mettre 1
                    Width = 1
                };

                if (dlg.ModifyPen(ref at))
                {
                    // Récupérer l'alpha depuis Transparency (convention : alpha = 255 - Transparency)
                    int alpha = 255 - at.Transparency;
                    Root.GoArrowColorArgb = Color.FromArgb(alpha, at.Color.R, at.Color.G, at.Color.B).ToArgb();

                    // aperçu bouton (BackColor ignore l'alpha mais montre la couleur)
                    try { btnGoArrowColor.BackColor = Color.FromArgb(alpha, at.Color.R, at.Color.G, at.Color.B); } catch { btnGoArrowColor.BackColor = at.Color; }

                    ScheduleConfigSave();
                }
                dlg.Dispose();
            };

            lblGoArrowThickness = new Label { AutoSize = true, Text = Root.Local.GoOptionsArrowThickness ?? "Arrow thickness :" };
            cbGoArrowThickness = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            var thicknessItems = (Root.Local.GoOptionsArrowThicknessItems ?? "Thin;Normal;Thick").Split(';');
            cbGoArrowThickness.Items.AddRange(thicknessItems);
            cbGoArrowThickness.SelectedIndex = Math.Max(0, Math.Min(thicknessItems.Length - 1, Root.GoArrowThickness));
            cbGoArrowThickness.SelectedIndexChanged += (s, e2) =>
            {
                Root.GoArrowThickness = cbGoArrowThickness.SelectedIndex;
                ScheduleConfigSave();
            };

            lblGoArrowLength = new Label { AutoSize = true, Text = Root.Local.GoOptionsArrowLength ?? "Arrow length :" };
            cbGoArrowLength = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            var lengthItems = (Root.Local.GoOptionsArrowLengthItems ?? "Short;Normal;Long").Split(';');
            cbGoArrowLength.Items.AddRange(lengthItems);
            cbGoArrowLength.SelectedIndex = Math.Max(0, Math.Min(lengthItems.Length - 1, Root.GoArrowLength));
            cbGoArrowLength.SelectedIndexChanged += (s, e2) =>
            {
                Root.GoArrowLength = cbGoArrowLength.SelectedIndex;
                ScheduleConfigSave();
            };



            // Couleur du texte (à ajouter après les boutons de couleur tags existants)
            lblGoTextColor = new Label { Text = Root.Local.GoOptionsTextColor ?? "Couleur du texte :", AutoSize = true, Left = 12, Top = lblGoStrokeThickness.Bottom + 18 };
            btnGoTextColor = new Button { Left = 220, Top = lblGoTextColor.Top - 3, Width = 80, Height = 24, FlatStyle = FlatStyle.Flat };
            btnGoTextColor.Click += (s, e2) => EditGoTagColor("Text", btnGoTextColor, Root.GoTool_Text_Color);
            tabPageGridTags.Controls.Add(lblGoTextColor);
            tabPageGridTags.Controls.Add(btnGoTextColor);



            // Placement sommaire — calcule une Y libre basée sur lblGridType
            // Remplacer cette ligne :
            //int topArrow = lblGridType.Bottom + 18;
            // Par cette ligne (garantit que les contrôles flèche viennent APRÈS les contrôles Go existants) :
            //int topArrow = Math.Max(lblGoStrokeThickness.Bottom, colorTop + 24) + 18; lblGoArrowColor.Left = 20; lblGoArrowColor.Top = topArrow;
            //int topArrow = Math.Max(lblGoStrokeThickness.Bottom, lblGoStrokeThickness.Top + 21) + 18;
            int topArrow = Math.Max(lblGoStrokeThickness.Bottom, lblGoTextColor.Bottom) + 18;
            lblGoArrowColor.Left = 20;
            lblGoArrowColor.Top = topArrow;

            btnGoArrowColor.Left = 250; btnGoArrowColor.Top = topArrow - 4;
            btnGoArrowColor.BackColor = Root.GetArrowColor();

            lblGoArrowThickness.Left = 20; lblGoArrowThickness.Top = topArrow + 30;
            cbGoArrowThickness.Left = 250; cbGoArrowThickness.Top = topArrow + 26;

            lblGoArrowLength.Left = 20; lblGoArrowLength.Top = topArrow + 60;
            cbGoArrowLength.Left = 250; cbGoArrowLength.Top = topArrow + 56;

            // Ajout des contrôles à la fin de tabPageGridTags
            tabPageGridTags.Controls.Add(lblGoArrowColor);
            tabPageGridTags.Controls.Add(btnGoArrowColor);
            tabPageGridTags.Controls.Add(lblGoArrowThickness);
            tabPageGridTags.Controls.Add(cbGoArrowThickness);
            tabPageGridTags.Controls.Add(lblGoArrowLength);
            tabPageGridTags.Controls.Add(cbGoArrowLength);

            // Synchronisation après reload (contrôles go)
            try
            {
                if (nudTagSizePerc != null)
                    nudTagSizePerc.Value = ClampDecimal(nudTagSizePerc, Root.TagSizePercent);
                if (nudTagCirclePerc != null)
                    nudTagCirclePerc.Value = ClampDecimal(nudTagCirclePerc, Root.TagCirclePercent);
                if (nudTagOpacityPerc != null)
                    nudTagOpacityPerc.Value = ClampDecimal(nudTagOpacityPerc, Root.TagStoneOpacityPercent);
                if (nudTagNumberOpacityPerc != null)
                    nudTagNumberOpacityPerc.Value = ClampDecimal(nudTagNumberOpacityPerc, Root.TagNumberOpacityPercent);

                if (nudGoFillOpacity != null) nudGoFillOpacity.Value = ClampDecimal(nudGoFillOpacity, Root.GoFillOpacityPercent);
                if (nudGoStrokeOpacity != null) nudGoStrokeOpacity.Value = ClampDecimal(nudGoStrokeOpacity, Root.GoStrokeOpacityPercent);
                if (nudGoStrokeWidth != null) nudGoStrokeWidth.Value = ClampDecimal(nudGoStrokeWidth, Root.GoStrokeWidth);

                if (cbGridType != null)
                {
                    if (Root.GridRows == 13) cbGridType.SelectedIndex = 1;
                    else if (Root.GridRows == 9) cbGridType.SelectedIndex = 2;
                    else cbGridType.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private decimal ClampDecimal(NumericUpDown nud, double value)
        {
            double v = Math.Min((double)nud.Maximum, Math.Max((double)nud.Minimum, value));
            return (decimal)v;
        }

        private void InitClipArtButtons()
        {
            Clip1Btn.BackColor = ToolbarDwg.BackColor;
            TryLoadClipArtButton(Clip1Btn, Root.ImageStamp1.ImageStamp, "Stamp1");
            Clip2Btn.BackColor = ToolbarDwg.BackColor;
            TryLoadClipArtButton(Clip2Btn, Root.ImageStamp2.ImageStamp, "Stamp2");
            Clip3Btn.BackColor = ToolbarDwg.BackColor;
            TryLoadClipArtButton(Clip3Btn, Root.ImageStamp3.ImageStamp, "Stamp3");
        }

        private void TryLoadClipArtButton(Button btn, string filename, string tag)
        {
            try
            {
                if (btn.BackgroundImage != null)
                    btn.BackgroundImage.Dispose();
                btn.BackgroundImage = FormCollection.getImgFromDiskOrRes(filename);
            }
            catch
            {
                Program.WriteErrorLog($"File {tag} found but can not be loaded:{filename} \n");
                btn.BackgroundImage = FormCollection.getImgFromDiskOrRes("unknown");
            }
        }

        private void InitDynamicPenLines()
        {
            for (int p = 0; p < Root.MaxPenCount; p++)
            {
                int top = lbPens0.Top - PenPanel.Top + p * (lbPens1.Top - lbPens0.Top);

                lbPens[p].Left = lbPens0.Left;
                lbPens[p].Top = top;

                cbPens[p].Left = lbcbPens.Left + 10;
                cbPens[p].Width = 25;
                cbPens[p].Top = top - 5;
                cbPens[p].Checked = Root.PenEnabled[p];

                pboxPens[p].Left = lbpboxPens.Left + 10;
                pboxPens[p].Top = top;
                pboxPens[p].Width = 15;
                pboxPens[p].Height = 15;
                pboxPens[p].BackColor = Root.PenAttr[p].Color;

                comboPensAlpha[p].Left = lbcomboPensAlpha.Left;
                comboPensAlpha[p].Top = top - 2;
                comboPensAlpha[p].Width = 60;
                comboPensAlpha[p].Text = (255 - Root.PenAttr[p].Transparency).ToString();

                comboPensWidth[p].Left = lbcomboPensWidth.Left;
                comboPensWidth[p].Top = top - 2;
                comboPensWidth[p].Width = 60;
                comboPensWidth[p].Text = ((int)Root.PenAttr[p].Width).ToString();

                comboPensLineStyle[p].Left = lbLineStyle.Left + 5;
                comboPensLineStyle[p].Top = top - 2;
                comboPensLineStyle[p].Height = comboPensWidth[p].Height;
                comboPensLineStyle[p].Width = comboPensWidth[p].Height * 2;
                comboPensLineStyle[p].BackgroundImageLayout = ImageLayout.Stretch;
                comboPensLineStyle[p].BackgroundImage = FormCollection.getImgFromDiskOrRes("DashStyle" + Root.LineStyleToString(Root.PenAttr[p].ExtendedProperties));
                comboPensLineStyle[p].Tag = p;

                comboPensFading[p].Left = lbcomboPensFading.Left + 10;
                comboPensFading[p].Top = top - 2;
                comboPensFading[p].Width = 20;
                comboPensFading[p].Checked = Root.PenAttr[p].ExtendedProperties.Contains(Root.FADING_PEN);
            }
        }

        private void InitHotkeysPens()
        {
            for (int p = 0; p < Root.MaxDisplayedPens; p++)
            {
                lbHotkeyPens[p].Left = lbHkColorEdit.Left;
                lbHotkeyPens[p].Width = 80;
                lbHotkeyPens[p].Top = p * (lbHkEraser.Top - lbHkLasso.Top) + lbHkLasso.Top;

                hiPens[p].Hotkey = Root.Hotkey_Pens[p];
                hiPens[p].Left = hiColorEdit.Left;
                hiPens[p].Width = hiColorEdit.Width;
                hiPens[p].Top = p * (hiEraser.Top - hiLasso.Top) + hiLasso.Top;
            }
        }

        private void SetAltAsOneCommandState()
        {
            switch (Root.AltAsOneCommand)
            {
                case 2: AltAsOneCommandCb.CheckState = CheckState.Checked; break;
                case 1: AltAsOneCommandCb.CheckState = CheckState.Indeterminate; break;
                default: AltAsOneCommandCb.CheckState = CheckState.Unchecked; break;
            }
        }

        private void AssignHotkeys()
        {
            hiGlobal.Hotkey = Root.Hotkey_Global;
            hiFadingToggle.Hotkey = Root.Hotkey_FadingToggle;
            hiEraser.Hotkey = Root.Hotkey_Eraser;
            hiPan.Hotkey = Root.Hotkey_Pan;
            hiInkVisible.Hotkey = Root.Hotkey_InkVisible;
            hiScaleRotate.Hotkey = Root.Hotkey_ScaleRotate;
            hiSnapshot.Hotkey = Root.Hotkey_Snap;
            hiUndo.Hotkey = Root.Hotkey_Undo;
            hiRedo.Hotkey = Root.Hotkey_Redo;
            hiClear.Hotkey = Root.Hotkey_Clear;
            hiVideo.Hotkey = Root.Hotkey_Video;
            hiDockUndock.Hotkey = Root.Hotkey_DockUndock;
            hiClose.Hotkey = Root.Hotkey_Close;

            hiToolHand.Hotkey = Root.Hotkey_Hand;
            hiToolLine.Hotkey = Root.Hotkey_Line;
            hiToolRect.Hotkey = Root.Hotkey_Rect;
            hiToolOval.Hotkey = Root.Hotkey_Oval;
            hiToolArrow.Hotkey = Root.Hotkey_Arrow;
            hiToolNumb.Hotkey = Root.Hotkey_Numb;
            hiHK_ShowWhite.Hotkey = Root.Hotkey_NTag_ShowWhite;
            hiHK_ShowBlack.Hotkey = Root.Hotkey_NTag_ShowBlack;
            hiHK_HideWhite.Hotkey = Root.Hotkey_NTag_HideWhite;
            hiHK_HideBlack.Hotkey = Root.Hotkey_NTag_HideBlack;
            hiHK_HandWhite.Hotkey = Root.Hotkey_HandFilledWhite;
            hiHK_HandBlack.Hotkey = Root.Hotkey_HandFilledBlack;

            // ajouter dans AssignHotkeys()
            hiHK_LetterTag.Hotkey = Root.Hotkey_LetterTag;
            hiHK_SquareTag.Hotkey = Root.Hotkey_SquareTag;
            hiHK_TriangleTag.Hotkey = Root.Hotkey_TriangleTag;
            hiHK_CircleTag.Hotkey = Root.Hotkey_CircleTag;
            hiHK_CrossTag.Hotkey = Root.Hotkey_CrossTag;

            HiToolText.Hotkey = Root.Hotkey_Text;
            hiToolEdit.Hotkey = Root.Hotkey_Edit;
            hiToolMagnet.Hotkey = Root.Hotkey_Magnet;
            hiToolClipArt.Hotkey = Root.Hotkey_ClipArt;
            hiToolClipArt1.Hotkey = Root.Hotkey_ClipArt1;
            hiToolClipArt2.Hotkey = Root.Hotkey_ClipArt2;
            hiToolClipArt3.Hotkey = Root.Hotkey_ClipArt3;
            hiPrevPage.Hotkey = Root.Hotkey_PagePrev;
            hiNextPage.Hotkey = Root.Hotkey_PageNext;
            hiLoadStrokes.Hotkey = Root.Hotkey_LoadStrokes;
            hiSaveStrokes.Hotkey = Root.Hotkey_SaveStrokes;
            hiZoom.Hotkey = Root.Hotkey_Zoom;
            hiPenWidthPlus.Hotkey = Root.Hotkey_PenWidthPlus;
            hiPenWidthMinus.Hotkey = Root.Hotkey_PenWidthMinus;
            hiColorPickup.Hotkey = Root.Hotkey_ColorPickup;
            hiColorEdit.Hotkey = Root.Hotkey_ColorEdit;
            hiLineStyle.Hotkey = Root.Hotkey_LineStyle;
            hiLasso.Hotkey = Root.Hotkey_Lasso;
        }

        private void InitLineStyleRotateChecks()
        {
            CbHKRot_Stroke.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_Stroke.Tag)) != 0;
            CbHKRot_Solid.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_Solid.Tag)) != 0;
            CbHKRot_Dash.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_Dash.Tag)) != 0;
            CbHKRot_Dot.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_Dot.Tag)) != 0;
            CbHKRot_DashDot.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_DashDot.Tag)) != 0;
            CbHKRot_DashDotDot.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_DashDotDot.Tag)) != 0;
        }

        private void FormOptions_Shown(object sender, EventArgs e)
        {
            FormOptions_Load(sender, e);
        }

        private void FormOptions_LocalReload()
        {
            string shortTxt(string sin)
            {
                int i = sin.IndexOf("(");
                if (i < 0) i = sin.Length;
                return sin.Substring(0, i);
            }

            // Onglets principaux (indices initiaux supposés stables)
            if (VideoTabCtrl.TabPages.Count > 0) VideoTabCtrl.TabPages[0].Text = Root.Local.OptionsTabGeneral;
            if (VideoTabCtrl.TabPages.Count > 1) VideoTabCtrl.TabPages[1].Text = Root.Local.OptionsTabPens;
            if (VideoTabCtrl.TabPages.Count > 2) VideoTabCtrl.TabPages[2].Text = Root.Local.OptionsTabHotkeys;

            // Onglet go hotkeys
            if (tabPageGoHotkeys != null && !string.IsNullOrEmpty(Root.Local.OptionsTabGoHotkeys))
                tabPageGoHotkeys.Text = Root.Local.OptionsTabGoHotkeys;

            // Labels go hotkeys
            lblHK_ShowWhite.Text = Root.Local.OptionsGoHotkeys_ShowWhite ?? lblHK_ShowWhite.Text;
            lblHK_ShowBlack.Text = Root.Local.OptionsGoHotkeys_ShowBlack ?? lblHK_ShowBlack.Text;
            lblHK_HideWhite.Text = Root.Local.OptionsGoHotkeys_HideWhite ?? lblHK_HideWhite.Text;
            lblHK_HideBlack.Text = Root.Local.OptionsGoHotkeys_HideBlack ?? lblHK_HideBlack.Text;

            lblHK_HandWhite.Text = Root.Local.OptionsGoHotkeys_HandWhite ?? lblHK_HandWhite.Text;
            lblHK_HandBlack.Text = Root.Local.OptionsGoHotkeys_HandBlack ?? lblHK_HandBlack.Text;

            lblGoFillOpacity.Text = Root.Local.OptionsGoFillOpacity ?? lblGoFillOpacity.Text;
            lblGoStrokeOpacity.Text = Root.Local.OptionsGoStrokeOpacity ?? lblGoStrokeOpacity.Text;
            lblGoStrokeWidth.Text = Root.Local.OptionsGoStrokeWidth ?? lblGoStrokeWidth.Text;

            // Ajouter dans FormOptions_LocalReload() pour afficher les labels localisés
            lblHK_LetterTag.Text = Root.Local.ButtonNameLetterTag ?? lblHK_LetterTag.Text;
            lblHK_SquareTag.Text = Root.Local.ButtonNameSquareTag ?? lblHK_SquareTag.Text;
            lblHK_TriangleTag.Text = Root.Local.ButtonNameTriangleTag ?? lblHK_TriangleTag.Text;
            lblHK_CircleTag.Text = Root.Local.ButtonNameCircleTag ?? lblHK_CircleTag.Text;
            lblHK_CrossTag.Text = Root.Local.ButtonNameCrossTag ?? lblHK_CrossTag.Text;


            this.Text = Root.Local.MenuEntryOptions + " - ppInk";
            SubToolsBar_cb.Text = Root.Local.SubToolsBarCbText;
            PensOnTwoLinesCb.Text = Root.Local.OptionPensOnTwoLinesCb;
            ToolBarColorLbl.Text = Root.Local.OptionsGeneralToolBarColorText;
            ClipartsSelBtn.Text = shortTxt(Root.Local.ButtonNameClipArt);
            AltTabActivateCb.Text = Root.Local.OptionsGeneralAltTabActivateText;
            KeepUnfoldedPointerCb.Text = Root.Local.OptionsGeneralUnDockedInPointerText;
            lblToolbarHeight.Text = Root.Local.OptionsGeneralToolbarHeight;
            lbLanguage.Text = Root.Local.OptionsGeneralLanguage;
            lbCanvascursor.Text = Root.Local.OptionsGeneralCanvascursor;
            lbSnapshotsavepath.Text = Root.Local.OptionsGeneralSnapshotsavepath;
            OpenIntoSnapCb.Text = Root.Local.OptionsGeneralOpenIntoSnapMode;
            StartFoldedCb.Text = Root.Local.OptionGeneralStartFolded;
            cbWhiteIcon.Text = Root.Local.OptionsGeneralWhitetrayicon;
            cbAllowDragging.Text = Root.Local.OptionsGeneralAllowdragging;
            APIRestLbl.Text = Root.Local.OptionsGeneralAPIRest;
            ShowFloatingWinCb.Text = Root.Local.OptionsGeneralShowFloatingWindow;
            SaveWindowPosBtn.Text = Root.Local.OptionsGeneralSaveFloatingWindowPos;
            ArrwGrp.Text = Root.Local.OptionsGeneralArrowHead;
            ArrHdAptLbl.Text = Root.Local.OptionsGeneralArrowHeadApt;
            ArrHdLenLbl.Text = Root.Local.OptionsGeneralArrowHeadLen;
            NewArrowEditBtn.Text = shortTxt(Root.Local.ButtonNameArrow);
            DefTxtLbl.Text = shortTxt(Root.Local.ButtonNameText) + " - " + Root.Local.OptionsGeneralDefaultTextLbl;
            DefaultFontBtn.Text = Root.Local.OptionsGeneralDefaultTextBtn;
            DefTagLbl.Text = shortTxt(Root.Local.ButtonNameNumb) + " - " + Root.Local.OptionsGeneralDefaultTextLbl;
            TagFontBtn.Text = Root.Local.OptionsGeneralDefaultTextBtn;
            DefArrStartCb.Text = Root.Local.OptionsGeneralDefaultArrHdBtn;
            MagnetLbl.Text = Root.Local.OptionsGeneralMagnetLbl;
            SaveConfigBtn.Text = Root.Local.OptionsGeneralSaveConfigToFile;
            CaptStrokesOnlyCb.Text = Root.Local.OptionsCaptureStrokesOnly;
            ColorPickerEnaCb.Text = Root.Local.OptionsHotkeysColorPicker;
            SwapSnapsBehviorsCb.Text = Root.Local.OptionsSwapSnapshotBehavior;

            MeasurementBox.Text = Root.Local.OptionMeasureGroup;
            Measuse1Lbl.Text = Root.Local.OptionMeasureLenLabel;
            MeasureAngleCb.Text = Root.Local.OptionMeasureAngle;
            MeasureWhileDrawing.Text = Root.Local.OptionMeasureWhileDrawing;
            lbNote.Text = Root.Local.OptionsGeneralNotePenwidth;

            // ZoomEnabledCb
            ZoomEnabledCb.Items.Clear();
            foreach (var s in Root.Local.OptionsZoomEnabled.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                ZoomEnabledCb.Items.Add(s.Trim());
            if (ZoomEnabledCb.Items.Count > 0)
            {
                if (Root.ZoomEnabled >= 0 && Root.ZoomEnabled < ZoomEnabledCb.Items.Count)
                    ZoomEnabledCb.SelectedIndex = Root.ZoomEnabled;
                else ZoomEnabledCb.SelectedIndex = 0;
            }

            // TextBackgroundLst
            TextBackgroundLst.Items.Clear();
            foreach (var s in Root.Local.TextFramingText.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                TextBackgroundLst.Items.Add(s.Trim());
            if (TextBackgroundLst.Items.Count > 0)
            {
                if (Root.TextBackground >= 0 && Root.TextBackground < TextBackgroundLst.Items.Count)
                    TextBackgroundLst.SelectedIndex = Root.TextBackground;
                else TextBackgroundLst.SelectedIndex = 0;
            }

            ZoomBox.Text = Root.Local.ButtonNameZoom;
            ZoomDimLbl.Text = Root.Local.OptionsZoomDim;
            ZoomScaleLbl.Text = Root.Local.OptionsZoomScale;
            ZoomContinousCb.Text = Root.Local.OptionsZoomContinous;

            SpotLightBox.Text = Root.Local.OptionsSpotLightBox;
            SpotOnAltCb.Text = Root.Local.OptionsSpotOnAlt;
            SpotRadLbl.Text = Root.Local.OptionsSpotLightRadius;

            ActivateDbgWinBtn.Text = Root.Local.ButtonActivateDebug;

            SnapInPointerGrp.Text = Root.Local.OptionsHotKeySnapInPointerGrp;
            SnapInPointerLbl.Text = Root.Local.OptionsHotKeySnapInPointerLbl;

            // SnapInPointer comboboxes
            SnapInPointerHoldCb.Items.Clear();
            SnapInPointerTwiceCb.Items.Clear();
            foreach (var s in Root.Local.OptionsHotKeySnapInPointerKeys.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var t = s.Trim();
                SnapInPointerHoldCb.Items.Add(t);
                SnapInPointerTwiceCb.Items.Add(t);
            }
            if (SnapInPointerHoldCb.Items.Count > 0)
            {
                int idxHold = (int)Root.SnapInPointerHoldKey;
                SnapInPointerHoldCb.SelectedIndex = (idxHold >= 0 && idxHold < SnapInPointerHoldCb.Items.Count) ? idxHold : 0;
                int idxTwice = (int)Root.SnapInPointerPressTwiceKey;
                SnapInPointerTwiceCb.SelectedIndex = (idxTwice >= 0 && idxTwice < SnapInPointerTwiceCb.Items.Count) ? idxTwice : 0;
            }

            ExtraPensCb.Text = Root.Local.OptionHotkeysExtraPens;
            AltAsOneCommandCb.Text = Root.Local.OptionsHotKeyAltAsOneCommand;
            lbHkFadingToggle.Text = Root.Local.ButtonNameToogle;
            lbHkClear.Text = shortTxt(Root.Local.ButtonNameClear);
            lbHkVideo.Text = shortTxt(Root.Local.ButtonNameVideo);
            lbHkEraser.Text = shortTxt(Root.Local.ButtonNameErasor);
            lbHkInkVisible.Text = shortTxt(Root.Local.ButtonNameInkVisible);
            lbHkPan.Text = shortTxt(Root.Local.ButtonNamePan);
            lbHkScaleRotate.Text = shortTxt(Root.Local.ButtonNameScaleRotate);
            lbHkRedo.Text = shortTxt(Root.Local.ButtonNameRedo);
            lbHkSnapshot.Text = shortTxt(Root.Local.ButtonNameSnapshot);
            lbHkUndo.Text = shortTxt(Root.Local.ButtonNameUndo);
            lbHkDockUndock.Text = shortTxt(Root.Local.ButtonNameDock);
            lbHkClose.Text = Root.Local.ButtonNameClose;

            lbHkHand.Text = shortTxt(Root.Local.ButtonNameHand);
            lbHkLine.Text = shortTxt(Root.Local.ButtonNameLine);
            lbHkRect.Text = shortTxt(Root.Local.ButtonNameRect);
            lbHkOval.Text = shortTxt(Root.Local.ButtonNameOval);
            lbHkArrow.Text = shortTxt(Root.Local.ButtonNameArrow);
            lbHkNumb.Text = shortTxt(Root.Local.ButtonNameNumb);
            lbHkText.Text = shortTxt(Root.Local.ButtonNameText);
            lbHkEdit.Text = shortTxt(Root.Local.ButtonNameEdit);
            lbHkMagn.Text = shortTxt(Root.Local.ButtonNameMagn);
            lbHkClipart.Text = shortTxt(Root.Local.ButtonNameClipArt);
            lbHkClipart1.Text = shortTxt(Root.Local.ButtonNameClipArt) + " 1";
            lbHkClipart2.Text = shortTxt(Root.Local.ButtonNameClipArt) + " 2";
            lbHkClipart3.Text = shortTxt(Root.Local.ButtonNameClipArt) + " 3";
            lbHkZoom.Text = shortTxt(Root.Local.ButtonNameZoom);
            lbHkPrevPage.Text = shortTxt(Root.Local.PagePrevHK);
            lbHkNextPage.Text = shortTxt(Root.Local.PageNextHK);
            lbHkLoadStrokes.Text = shortTxt(Root.Local.LoadStroke);
            lbHkSaveStrokes.Text = shortTxt(Root.Local.SaveStroke);
            lbHkLineStyle.Text = Root.Local.OptionsLineStyle;
            lbHkPenWidthPlus.Text = Root.Local.OptionsHotkeysPenWidthPlus;
            lbHkPenWidthMinus.Text = Root.Local.OptionsHotkeysPenWidthMinus;
            lbHkColorPickup.Text = Root.Local.OptionsHotkeysColorPicker;
            lbHkColorEdit.Text = Root.Local.OptionsHotkeysColorEdit;
            lbHkLasso.Text = shortTxt(Root.Local.ButtonNameLasso);
            lbGlobalHotkey.Text = Root.Local.OptionsHotkeysglobal;
            cbAllowHotkeyInPointer.Text = Root.Local.OptionsHotkeysEnableinpointer;

            foreach (Control ct in tabPage3.Controls)
                if (ct is HotkeyInputBox h) h.UpdateText();
            foreach (Control ct in tabPageGoHotkeys.Controls)
                if (ct is HotkeyInputBox h2) h2.UpdateText();

            // Canvas cursor
            comboCanvasCursor.Items.Clear();
            comboCanvasCursor.Items.Add(Root.Local.OptionsGeneralCanvascursorArrow);
            comboCanvasCursor.Items.Add(Root.Local.OptionsGeneralCanvascursorPentip);
            if (comboCanvasCursor.Items.Count > Root.CanvasCursor && Root.CanvasCursor >= 0)
                comboCanvasCursor.SelectedIndex = Root.CanvasCursor;
            else if (comboCanvasCursor.Items.Count > 0)
                comboCanvasCursor.SelectedIndex = 0;

            BoardBx.Text = Root.Local.OptionsGeneralBoardBox;
            BoardAtOpenLbl.Text = Root.Local.OptionsGeneralBoardAtOpenLbl;
            BoardCustColorLbl.Text = Root.Local.OptionsGeneralBoardCustColorLbl;

            BoardAtOpenCombo.Items.Clear();
            BoardAtOpenCombo.Items.Add(Root.Local.BoardTransparent);
            BoardAtOpenCombo.Items.Add(Root.Local.BoardWhite);
            BoardAtOpenCombo.Items.Add(Root.Local.BoardGray);
            BoardAtOpenCombo.Items.Add(Root.Local.BoardBlack);
            BoardAtOpenCombo.Items.Add(Root.Local.BoardLast);
            if (BoardAtOpenCombo.Items.Count > Root.BoardAtOpening && Root.BoardAtOpening >= 0)
                BoardAtOpenCombo.SelectedIndex = Root.BoardAtOpening;
            else if (BoardAtOpenCombo.Items.Count > 0)
                BoardAtOpenCombo.SelectedIndex = 0;

            VideoTab.Text = Root.Local.VideoTab;
            OptNoVideo.Text = Root.Local.OptNoVideo;
            OptObsRecord.Text = Root.Local.OptObsRecord;
            OptObsBcast.Text = Root.Local.OptObsBcast;
            LblWsUrl.Text = Root.Local.LblWsUrl;
            LblWsPwd.Text = Root.Local.LblWsPwd;
            LblObsNote.Text = Root.Local.LblObsNote;
            OptFfmpeg.Text = Root.Local.OptFfmpeg;
            LblFfmpegCmd.Text = Root.Local.LblFfmpegCmd;
            LblFfmpegNote.Text = Root.Local.LblFfmpegNote;

            CreateM3u.Text = Root.Local.CreateM3UGroup;
            M3UIndexOnUndockCb.Text = Root.Local.CreateIndexOnUndock;
            LblIndexHotKey.Text = Root.Local.LblM3UIndexHotKey;
            M3UIndexDefTextLbl.Text = Root.Local.M3UIndexDefaultText;
            UndockOnM3UIndexCb.Text = Root.Local.UndockOnM3UIndexCreate;
            NoEditM3UIndexCb.Text = Root.Local.NoEditM3UIndex;

            for (int p = 0; p < Root.MaxPenCount; p++)
            {
                comboPensAlpha[p].Items.Clear();
                comboPensWidth[p].Items.Clear();
                comboPensAlpha[p].Items.AddRange(new object[] { Root.Local.OptionsPensPencil, Root.Local.OptionsPensHighlighter });
                comboPensWidth[p].Items.AddRange(new object[] { Root.Local.OptionsPensThin, Root.Local.OptionsPensNormal, Root.Local.OptionsPensThick });
                lbPens[p].Text = Root.Local.ButtonNamePen[p];
                if (p >= Root.MaxDisplayedPens)
                    continue;
                lbHotkeyPens[p].Text = Root.Local.ButtonNamePen[p];
            }
            lbcbPens.Text = Root.Local.OptionsPensShow;
            lbpboxPens.Text = Root.Local.OptionsPensColor;
            lbcomboPensAlpha.Text = Root.Local.OptionsPensAlpha;
            lbcomboPensWidth.Text = Root.Local.OptionsPensWidth;
            lbcomboPensFading.Text = Root.Local.OptionsPensFading;
            WidthAtPenSelCb.Text = Root.Local.OptionsPensWidthAtSelection;
            InverseWheelCb.Text = Root.InverseMousewheel ? Root.Local.OptionsInverseMouseWheelChecked : Root.Local.OptionsInverseMouseWheel;
            FitToCurveEd.Text = Root.Local.OptionsFitToCurve;
            lbLineStyle.Text = Root.Local.OptionsLineStyle;
            Click4StrokeCb.Text = Root.Local.OptionsClick4Stroke;

            // Langues
            comboLanguage.Items.Clear();
            List<string> langs = Root.Local.GetLanguagenames();
            foreach (string ln in langs)
                comboLanguage.Items.Add(ln);
            string currentLn = Root.Local.GetLanguagenameByFilename(Root.Local.CurrentLanguageFile);
            if (comboLanguage.Items.Contains(currentLn))
                comboLanguage.SelectedIndex = comboLanguage.Items.IndexOf(currentLn);



            // <-- AJOUTER CETTE LIGNE ICI (réinitialise l'affichage des boutons de couleur après rechargement local)
            InitGoTagButtons();
           
        
        
        }




        private void comboPensAlpha_TextChanged(object sender, EventArgs e)
        {
            for (int p = 0; p < Root.MaxPenCount; p++)
                if (sender == comboPensAlpha[p])
                {
                    if (byte.TryParse(comboPensAlpha[p].Text, out byte o) && o <= 255)
                    {
                        Root.PenAttr[p].Transparency = (byte)(255 - o);
                        comboPensAlpha[p].BackColor = Color.White;
                    }
                    else
                        comboPensAlpha[p].BackColor = Color.IndianRed;
                }
        }

        private void comboPensWidth_TextChanged(object sender, EventArgs e)
        {
            for (int p = 0; p < Root.MaxPenCount; p++)
                if (sender == comboPensWidth[p])
                {
                    if (float.TryParse(comboPensWidth[p].Text, out float o) && o > 0 && o <= 3000)
                    {
                        Root.PenAttr[p].Width = o;
                        comboPensWidth[p].BackColor = Color.White;
                    }
                    else
                        comboPensWidth[p].BackColor = Color.IndianRed;
                }
        }

        private void comboPensFading_Changed(object sender, EventArgs e)
        {
            for (int p = 0; p < Root.MaxPenCount; p++)
                if (sender == comboPensFading[p])
                {
                    if (comboPensFading[p].Checked)
                        Root.PenAttr[p].ExtendedProperties.Add(Root.FADING_PEN, Root.TimeBeforeFading);
                    else
                        try { Root.PenAttr[p].ExtendedProperties.Remove(Root.FADING_PEN); } catch { }
                }
        }

        private void pboxPens_Click(object sender, EventArgs e)
        {
            for (int p = 0; p < Root.MaxPenCount; p++)
                if (sender == pboxPens[p])
                {
                    PenModifyDlg dlg = new PenModifyDlg(Root);
                    if (dlg.ModifyPen(ref Root.PenAttr[p]))
                    {
                        pboxPens[p].BackColor = Color.FromArgb(255, Root.PenAttr[p].Color);
                        comboPensAlpha[p].Text = (255 - Root.PenAttr[p].Transparency).ToString();
                        comboPensWidth[p].Text = ((int)Root.PenAttr[p].Width).ToString();
                        comboPensFading[p].Checked = Root.PenAttr[p].ExtendedProperties.Contains(Root.FADING_PEN);
                        comboPensLineStyle[p].BackgroundImage = FormCollection.getImgFromDiskOrRes("DashStyle" + Root.LineStyleToString(Root.PenAttr[p].ExtendedProperties));
                    }
                    dlg.Dispose();
                }
        }

        private void cbPens_CheckedChanged(object sender, EventArgs e)
        {
            for (int p = 0; p < Root.MaxPenCount; p++)
                if (sender == cbPens[p])
                    Root.PenEnabled[p] = cbPens[p].Checked;
        }

        private void FormOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                ActiveControl?.SelectNextControl(ActiveControl, true, true, false, true);
                ActiveControl?.SelectNextControl(ActiveControl, false, true, false, true);
            }
            catch { }
            try { Root.SetHotkey(); } catch { }
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                try { TrySaveGoConfig(); } catch { }
                Hide();
            }
            GC.Collect();
        }

        private void cbWidthEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Root.PenWidthEnabled = cbWidthEnabled.Checked;
            lbNote.ForeColor = Color.Red;
        }

        private void cbEraserEnabled_CheckedChanged(object sender, EventArgs e) => Root.EraserEnabled = cbEraserEnabled.Checked;
        private void cbPointerEnabled_CheckedChanged(object sender, EventArgs e) => Root.PointerEnabled = cbPointerEnabled.Checked;
        private void cbSnapEnabled_CheckedChanged(object sender, EventArgs e) => Root.SnapEnabled = cbSnapEnabled.Checked;
        private void cbUndoEnabled_CheckedChanged(object sender, EventArgs e) => Root.UndoEnabled = cbUndoEnabled.Checked;
        private void cbClearEnabled_CheckedChanged(object sender, EventArgs e) => Root.ClearEnabled = cbClearEnabled.Checked;
        private void cbPanEnabled_CheckedChanged(object sender, EventArgs e) => Root.PanEnabled = cbPanEnabled.Checked;
        private void cbInkVisibleEnabled_CheckedChanged(object sender, EventArgs e) => Root.InkVisibleEnabled = cbInkVisibleEnabled.Checked;
        private void cbWhiteIcon_CheckedChanged(object sender, EventArgs e) { Root.WhiteTrayIcon = cbWhiteIcon.Checked; Root.SetTrayIconColor(); }

        private void btSnapPath_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog { SelectedPath = Root.SnapshotBasePath };
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
            {
                tbSnapPath.Text = NormalizeDir(folderBrowserDialog1.SelectedPath);
                Root.SnapshotBasePath = tbSnapPath.Text;
            }
        }

        private string NormalizeDir(string path)
        {
            path = path.Replace('\\', '/');
            if (!path.EndsWith("/")) path += '/';
            return path;
        }

        private void tbSnapPath_ModifiedChanged(object sender, EventArgs e)
        {
            tbSnapPath.Text = NormalizeDir(folderBrowserDialog1.SelectedPath);
            Root.SnapshotBasePath = tbSnapPath.Text;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int p = 0; p < Root.MaxPenCount; p++)
            {
                if (comboPensWidth[p].Text == Root.Local.OptionsPensThin)
                    comboPensWidth[p].Text = Root.PenWidthThin.ToString();
                else if (comboPensWidth[p].Text == Root.Local.OptionsPensNormal)
                    comboPensWidth[p].Text = Root.PenWidthNormal.ToString();
                else if (comboPensWidth[p].Text == Root.Local.OptionsPensThick)
                    comboPensWidth[p].Text = Root.PenWidthThick.ToString();

                if (comboPensAlpha[p].Text == Root.Local.OptionsPensPencil)
                    comboPensAlpha[p].Text = "255";
                else if (comboPensAlpha[p].Text == Root.Local.OptionsPensHighlighter)
                    comboPensAlpha[p].Text = "80";
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) => Root.CanvasCursor = comboCanvasCursor.SelectedIndex;
        private void cbAllowDragging_CheckedChanged(object sender, EventArgs e) => Root.AllowDraggingToolbar = cbAllowDragging.Checked;
        private void cbToolsEnabled_CheckedChanged(object sender, EventArgs e) => Root.ToolsEnabled = cbToolsEnabled.Checked;

        private void SaveWindowPosBtn_Click(object sender, EventArgs e)
        {
            if (Root.callForm != null)
            {
                Root.FormTop = Root.callForm.Top;
                Root.FormLeft = Root.callForm.Left;
            }
        }

        private void SaveConfigBtn_Click(object sender, EventArgs e)
        {
            string config_ini, st;
            using (var f = new StreamReader(Program.RunningFolder + "config.ini"))
                config_ini = f.ReadToEnd();
            using (var f = new StreamReader(Program.RunningFolder + "defaults.ini"))
                st = Root.CompleteConfig(f.ReadToEnd(), config_ini);
            if (st != "")
                using (var f = new StreamWriter(Program.RunningFolder + "config.ini"))
                    f.Write(config_ini + "\n" + st);

            using (var f = new StreamReader(Program.RunningFolder + "pens.ini"))
                config_ini = f.ReadToEnd();
            using (var f = new StreamReader(Program.RunningFolder + "pensdef.ini"))
                st = Root.CompleteConfig(f.ReadToEnd(), config_ini);
            if (st != "")
                using (var f = new StreamWriter(Program.RunningFolder + "pens.ini"))
                    f.Write(config_ini + "\n" + st);

            Root.SaveOptions(Program.RunningFolder + "pens.ini");
            Root.SaveOptions(Program.RunningFolder + "config.ini");
            Root.SaveOptions(Program.RunningFolder + "hotkeys.ini");
        }

        private void Float_Validating(object sender, CancelEventArgs e)
        {
            if (float.TryParse(((TextBox)sender).Text.Replace(",", "."), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _))
                ((TextBox)sender).BackColor = Color.White;
            else
            {
                e.Cancel = true;
                ((TextBox)sender).BackColor = Color.Orange;
                ((TextBox)sender).Select();
            }
        }

        private void DefaultFontBtn_Click(object sender, EventArgs e)
        {
            FontDlg.Font = new Font(Root.TextFont, Root.TextSize,
                (Root.TextItalic ? FontStyle.Italic : FontStyle.Regular) | (Root.TextBold ? FontStyle.Bold : FontStyle.Regular));
            if (FontDlg.ShowDialog() == DialogResult.OK)
            {
                Root.TextFont = FontDlg.Font.Name;
                Root.TextItalic = FontDlg.Font.Italic;
                Root.TextBold = FontDlg.Font.Bold;
                Root.TextSize = (int)FontDlg.Font.Size;
            }
        }

        private void TagFontBtn_Click(object sender, EventArgs e)
        {
            FontDlg.Font = new Font(Root.TagFont, Root.TagSize,
                (Root.TagItalic ? FontStyle.Italic : FontStyle.Regular) | (Root.TagBold ? FontStyle.Bold : FontStyle.Regular));
            if (FontDlg.ShowDialog() == DialogResult.OK)
            {
                Root.TagFont = FontDlg.Font.Name;
                Root.TagItalic = FontDlg.Font.Italic;
                Root.TagBold = FontDlg.Font.Bold;
                Root.TagSize = (int)FontDlg.Font.Size;
            }
        }

        private void ArrHdAperture_Validated(object sender, EventArgs e) =>
            Root.ArrowAngle = float.Parse(ArrHdAperture.Text.Replace(",", "."), CultureInfo.InvariantCulture) / 180.0 * Math.PI;

        private void ArrHdLength_Validated(object sender, EventArgs e) =>
            Root.ArrowLen = float.Parse(ArrHdLength.Text.Replace(",", "."), CultureInfo.InvariantCulture) / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth;

        private void ShowFloatingWinCb_Click(object sender, EventArgs e)
        {
            Root.FormOpacity = (((CheckBox)sender).Checked ? 1 : -1) * Math.Abs(Root.FormOpacity);
            if (((CheckBox)sender).Checked && (Root.FormTop <= Screen.PrimaryScreen.WorkingArea.Top || Root.FormTop >= Screen.PrimaryScreen.WorkingArea.Bottom ||
                Root.FormLeft <= Screen.PrimaryScreen.WorkingArea.Left || Root.FormLeft >= Screen.PrimaryScreen.WorkingArea.Right))
            {
                Root.FormTop = Screen.PrimaryScreen.WorkingArea.Top + 100;
                Root.FormLeft = Screen.PrimaryScreen.WorkingArea.Left + 100;
            }
            if (Root.FormOpacity > 0)
            {
                if (Root.callForm == null)
                    Root.callForm = new CallForm(Root);
                Root.callForm.Show();
                Root.callForm.Top = Root.FormTop;
                Root.callForm.Left = Root.FormLeft;
                Root.callForm.Width = Root.FormWidth;
                Root.callForm.Height = Root.FormWidth;
                Root.callForm.Opacity = Root.FormOpacity / 100.0;
            }
            else
                Root.callForm?.Hide();
        }

        private void Magnet_TB_Validated(object sender, EventArgs e)
        {
            float f = float.Parse(Magnet_TB.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            Root.MagneticRadius = (int)Math.Round(f / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth);
            if (Root.MagneticRadius == 0 && f != 0.0)
            {
                Root.MagneticRadius = Math.Sign(f);
                Magnet_TB.Text = (Root.MagneticRadius / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString("#0.0000", CultureInfo.InvariantCulture);
            }
        }

        private void DefArrStartCb_CheckedChanged(object sender, EventArgs e) =>
            Root.DefaultArrow_start = DefArrStartCb.Checked;

        private void OpenIntoSnapCb_CheckedChanged(object sender, EventArgs e) =>
            Root.OpenIntoSnapMode = OpenIntoSnapCb.Checked;

        private void WidthAtPenSelCb_CheckedChanged(object sender, EventArgs e) =>
            Root.WidthAtPenSel = WidthAtPenSelCb.Checked;

        private void ToolBarHeight_Validated(object sender, EventArgs e) =>
            Root.ToolbarHeight = float.Parse(ToolBarHeight.Text.Replace(",", "."), CultureInfo.InvariantCulture) / 100;

        private void ValidateOnEnter(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                SelectNextControl(ActiveControl, true, true, true, true);
                (sender as TextBox)?.Select();
                e.Handled = true;
            }
        }

        private void BoardAtOpenCombo_SelectedIndexChanged(object sender, EventArgs e) =>
            Root.BoardAtOpening = BoardAtOpenCombo.SelectedIndex;

        private void BoardCustColorPnl_Click(object sender, EventArgs e)
        {
            PenModifyDlg dlg = new PenModifyDlg(Root)
            {
                Text = Root.Local.BoardCustColorModifyTitle
            };
            Microsoft.Ink.DrawingAttributes at = new Microsoft.Ink.DrawingAttributes
            {
                Transparency = (byte)(255 - Root.Gray1[0]),
                Color = Color.FromArgb(Root.Gray1[0], Root.Gray1[1], Root.Gray1[2], Root.Gray1[3]),
                Width = 0
            };
            if (dlg.ModifyPen(ref at))
            {
                Root.Gray1[0] = 255 - at.Transparency;
                Root.Gray1[1] = at.Color.R;
                Root.Gray1[2] = at.Color.G;
                Root.Gray1[3] = at.Color.B;
                BoardCustColorPnl.BackColor = Color.FromArgb(Root.Gray1[0], at.Color);
            }
            dlg.Dispose();
        }

        //private void EditGoTagColor(string name, Button btn, int[] colorArr)
        //{
        //    try
        //    {
        //        // Prépare les attributs initiaux en réutilisant la convention existante :
        //        // colorArr = { A, R, G, B } ; DrawingAttributes.Transparency = 255 - A
        //        PenModifyDlg dlg = new PenModifyDlg(Root);
        //        Microsoft.Ink.DrawingAttributes at = new Microsoft.Ink.DrawingAttributes
        //        {
        //            Transparency = (byte)(255 - (colorArr.Length > 0 ? colorArr[0] : 255)),
        //            Color = Color.FromArgb(
        //                (colorArr.Length > 0 ? colorArr[0] : 255),
        //                (colorArr.Length > 1 ? colorArr[1] : 0),
        //                (colorArr.Length > 2 ? colorArr[2] : 0),
        //                (colorArr.Length > 3 ? colorArr[3] : 0)
        //            ),
        //            Width = 0
        //        };

        //        if (dlg.ModifyPen(ref at))
        //        {
        //            // Stocke la couleur choisie dans le tableau (A,R,G,B)
        //            if (colorArr.Length >= 4)
        //            {
        //                colorArr[0] = 255 - at.Transparency;
        //                colorArr[1] = at.Color.R;
        //                colorArr[2] = at.Color.G;
        //                colorArr[3] = at.Color.B;
        //            }

        //            // Mise à jour visuelle (aperçu — BackColor ignore l'alpha)
        //            try
        //            {
        //                btn.BackColor = Color.FromArgb(
        //                    (colorArr.Length > 0 ? colorArr[0] : 255),
        //                    (colorArr.Length > 1 ? colorArr[1] : 0),
        //                    (colorArr.Length > 2 ? colorArr[2] : 0),
        //                    (colorArr.Length > 3 ? colorArr[3] : 0)
        //                );
        //            }
        //            catch { }

        //            // Sauvegarde différée
        //            ScheduleConfigSave();
        //        }
        //        dlg.Dispose();
        //    }
        //    catch { }
        //}

        private void EditGoTagColor(string name, Button btn, int[] colorArr)
        {
            try
            {
                // Prépare les attributs initiaux en réutilisant la convention existante :
                // colorArr = { A, R, G, B } ; DrawingAttributes.Transparency = 255 - A
                PenModifyDlg dlg = new PenModifyDlg(Root);
                Microsoft.Ink.DrawingAttributes at = new Microsoft.Ink.DrawingAttributes
                {
                    Transparency = (byte)(255 - (colorArr.Length > 0 ? colorArr[0] : 255)),
                    Color = Color.FromArgb(
                        (colorArr.Length > 0 ? colorArr[0] : 255),
                        (colorArr.Length > 1 ? colorArr[1] : 0),
                        (colorArr.Length > 2 ? colorArr[2] : 0),
                        (colorArr.Length > 3 ? colorArr[3] : 0)
                    ),
                    // <-- éviter la largeur 0 (message de confirmation). On met 1 par défaut.
                    Width = 1
                };

                if (dlg.ModifyPen(ref at))
                {
                    // Stocke la couleur choisie dans le tableau (A,R,G,B)
                    if (colorArr.Length >= 4)
                    {
                        colorArr[0] = 255 - at.Transparency;
                        colorArr[1] = at.Color.R;
                        colorArr[2] = at.Color.G;
                        colorArr[3] = at.Color.B;
                    }

                    // Mise à jour visuelle (aperçu — BackColor ignore l'alpha)
                    try
                    {
                        btn.BackColor = Color.FromArgb(
                            (colorArr.Length > 0 ? colorArr[0] : 255),
                            (colorArr.Length > 1 ? colorArr[1] : 0),
                            (colorArr.Length > 2 ? colorArr[2] : 0),
                            (colorArr.Length > 3 ? colorArr[3] : 0)
                        );
                    }
                    catch { }

                    // Sauvegarde différée
                    ScheduleConfigSave();
                }
                dlg.Dispose();
            }
            catch { }
        }



        //private void InitGoTagButtons()
        //{



        //    try
        //    {
        //        Action<Button, int[]> initBtn = (btn, arr) =>
        //        {
        //            try
        //            {
        //                if (btn == null || arr == null || arr.Length < 4) return;
        //                // BackColor n'affiche pas l'alpha, mais donne un aperçu de la couleur
        //                btn.BackColor = Color.FromArgb(arr[0], arr[1], arr[2], arr[3]);
        //            }
        //            catch { }
        //        };

        //        initBtn(btnGoColor_Letter, Root?.GoTool_Letter_Color);
        //        initBtn(btnGoColor_Square, Root?.GoTool_Square_Color);
        //        initBtn(btnGoColor_Triangle, Root?.GoTool_Triangle_Color);
        //        initBtn(btnGoColor_Circle, Root?.GoTool_Circle_Color);
        //        initBtn(btnGoColor_Cross, Root?.GoTool_Cross_Color);
        //    }
        //    catch { }

        //    initBtn(btnGoTextColor, Root?.GoTool_Text_Color);

        //}

        private void InitGoTagButtons()
        {
            try
            {
                Action<Button, int[]> initBtn = (btn, arr) =>
                {
                    try
                    {
                        if (btn == null || arr == null || arr.Length < 4) return;
                        // BackColor n'affiche pas l'alpha, mais donne un aperçu de la couleur
                        btn.BackColor = Color.FromArgb(arr[0], arr[1], arr[2], arr[3]);
                    }
                    catch { }
                };

                initBtn(btnGoColor_Letter, Root?.GoTool_Letter_Color);
                initBtn(btnGoColor_Square, Root?.GoTool_Square_Color);
                initBtn(btnGoColor_Triangle, Root?.GoTool_Triangle_Color);
                initBtn(btnGoColor_Circle, Root?.GoTool_Circle_Color);
                initBtn(btnGoColor_Cross, Root?.GoTool_Cross_Color);

                // initialiser aussi le bouton de couleur du texte ici (évite erreur de portée de initBtn)
                initBtn(btnGoTextColor, Root?.GoTool_Text_Color);
            }
            catch { }
        }

        private void WsUrlTxt_TextChanged(object sender, EventArgs e) => Root.ObsUrl = WsUrlTxt.Text;
        private void WsPwdTxt_TextChanged(object sender, EventArgs e) => Root.ObsPwd = WsPwdTxt.Text;
        private void FfmpegCmdTxt_TextChanged(object sender, EventArgs e) => Root.FFMpegCmd = FfmpegCmdTxt.Text;

        private void VideoOption_Changed(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
                Root.VideoRecordMode = (VideoRecordMode)int.Parse((string)(sender as Control).Tag);
            M3UOptions.Enabled = Root.IsVideoRecordingSelected() && Root.CreateM3U;
            Root.UnsetHotkey();
            Root.SetHotkey();
        }

        private void ToolbarDwg_Click(object sender, EventArgs e)
        {
            PenModifyDlg dlg = new PenModifyDlg(Root) { Text = "" };
            Microsoft.Ink.DrawingAttributes at = new Microsoft.Ink.DrawingAttributes
            {
                Transparency = (byte)(255 - Root.ToolbarBGColor[0]),
                Color = Color.FromArgb(Root.ToolbarBGColor[0], Root.ToolbarBGColor[1], Root.ToolbarBGColor[2], Root.ToolbarBGColor[3]),
                Width = 0
            };
            if (dlg.ModifyPen(ref at))
            {
                Root.ToolbarBGColor[0] = 255 - at.Transparency;
                Root.ToolbarBGColor[1] = at.Color.R;
                Root.ToolbarBGColor[2] = at.Color.G;
                Root.ToolbarBGColor[3] = at.Color.B;
                ToolbarDwg.BackColor = Color.FromArgb(Root.ToolbarBGColor[0], at.Color);
                Clip1Btn.BackColor = ToolbarDwg.BackColor;
                Clip2Btn.BackColor = ToolbarDwg.BackColor;
                Clip3Btn.BackColor = ToolbarDwg.BackColor;
            }
            dlg.Dispose();
        }

        private void AltTabActivateCb_CheckedChanged(object sender, EventArgs e) =>
            Root.AltTabPointer = AltTabActivateCb.Checked;

        private void ClipartsSelBtn_Click(object sender, EventArgs e)
        {
            ImageLister dlg = new ImageLister(Root)
            {
                FromClpBtn = { Visible = false },
                InsertBtn = { Text = Root.Local.ButtonOkText },
                AutoCloseCb = { Visible = false }
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Root.ImageStampFilling = dlg.ImageStampFilling;
                Root.StampFileNames.Clear();
                foreach (ListViewItem it in dlg.ImageListViewer.Items)
                    Root.StampFileNames.Add(it.ImageKey);
                Root.FormCollection.ClipartsDlg.Initialize();
            }
            dlg.Dispose();
        }

        private void ClipBtn_Click(object sender, EventArgs e)
        {
            ImageLister dlg = new ImageLister(Root);
            dlg.FromClpBtn.Visible = false;
            dlg.LoadImageBtn.Visible = false;
            dlg.DelBtn.Visible = false;
            dlg.FillingCombo.Visible = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ((Button)sender).BackgroundImage = FormCollection.getImgFromDiskOrRes(dlg.ImageStamp);
                string tag = (string)((Control)sender).Tag;
                if (tag == "1") Root.ImageStamp1.ImageStamp = dlg.ImageStamp;
                else if (tag == "2") Root.ImageStamp2.ImageStamp = dlg.ImageStamp;
                else if (tag == "3") Root.ImageStamp3.ImageStamp = dlg.ImageStamp;
            }
            dlg.Dispose();
        }

        private void cbLoadSaveEnabled_CheckedChanged(object sender, EventArgs e) =>
            Root.LoadSaveEnabled = cbLoadSaveEnabled.Checked;

        private void ToolbarOrientationBtn_Click(object sender, EventArgs e)
        {
            Root.ToolbarOrientation++;
            if (Root.ToolbarOrientation > Orientation.max)
                Root.ToolbarOrientation = Orientation.min;
            ToolbarOrientationBtn.BackgroundImage = ToolBarOrientationIcons[Root.ToolbarOrientation];
        }

        private void FadingTimeEd_Validating(object sender, CancelEventArgs e)
        {
            if (float.TryParse(FadingTimeEd.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float f) && f >= 0)
            {
                Root.TimeBeforeFading = f;
                FadingTimeEd.BackColor = SystemColors.Window;
                for (int i = 0; i < Root.MaxPenCount; i++)
                    if (Root.PenAttr[i].ExtendedProperties.Contains(Root.FADING_PEN))
                        Root.PenAttr[i].ExtendedProperties.Add(Root.FADING_PEN, Root.TimeBeforeFading);
            }
            else
            {
                FadingTimeEd.BackColor = Color.Orange;
                e.Cancel = true;
            }
        }

        private void ZoomWidthEd_Validating(object sender, CancelEventArgs e)
        {
            if (int.TryParse(ZoomWidthEd.Text, out Root.ZoomWidth))
                ZoomWidthEd.BackColor = SystemColors.Window;
            else
            {
                ZoomWidthEd.BackColor = Color.Orange;
                e.Cancel = true;
            }
        }

        private void ZoomHeightEd_Validating(object sender, CancelEventArgs e)
        {
            if (int.TryParse(ZoomHeightEd.Text, out Root.ZoomHeight))
                ZoomHeightEd.BackColor = SystemColors.Window;
            else
            {
                ZoomHeightEd.BackColor = Color.Orange;
                e.Cancel = true;
            }
        }

        private void ZoomScaleEd_Validating(object sender, CancelEventArgs e)
        {
            if (float.TryParse(ZoomScaleEd.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out Root.ZoomScale))
                ZoomScaleEd.BackColor = SystemColors.Window;
            else
            {
                ZoomScaleEd.BackColor = Color.Orange;
                e.Cancel = true;
            }
        }

        private void ZoomContinousCb_CheckedChanged(object sender, EventArgs e) =>
            Root.ZoomContinous = ZoomContinousCb.Checked;

        private void ZoomEnabledCb_SelectedIndexChanged(object sender, EventArgs e) =>
            Root.ZoomEnabled = ZoomEnabledCb.SelectedIndex;

        private void hiGlobal_Enter(object sender, EventArgs e) => Root.UnsetHotkey();
        private void hiGlobal_Leave(object sender, EventArgs e) => Root.SetHotkey();
        private void cbAllowHotkeyInPointer_CheckedChanged(object sender, EventArgs e) => Root.AllowHotkeyInPointerMode = cbAllowHotkeyInPointer.Checked;

        private void hi_OnHotkeyChanged(object sender, EventArgs e)
        {
            var boxes = new List<HotkeyInputBox>();
            foreach (Control ct in tabPage3.Controls)
                if (ct is HotkeyInputBox h) boxes.Add(h);
            if (tabPageGoHotkeys != null)
                foreach (Control ct in tabPageGoHotkeys.Controls)
                    if (ct is HotkeyInputBox h2) boxes.Add(h2);

            foreach (var h in boxes) h.ExternalConflictFlag = false;

            for (int i = 0; i < boxes.Count; i++)
            {
                var hi_i = boxes[i];
                if (hi_i?.Hotkey == null) continue;
                for (int j = 0; j < boxes.Count; j++)
                {
                    if (i == j) continue;
                    var hi_j = boxes[j];
                    if (hi_j?.Hotkey == null) continue;
                    try
                    {
                        if (hi_i.Hotkey.ConflictWith(hi_j.Hotkey))
                        {
                            hi_i.ExternalConflictFlag = true;
                            break;
                        }
                    }
                    catch { }
                }
            }
            foreach (var h in boxes) h.UpdateText();
        }

        private void comboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboLanguage.Text != Root.Local.GetLanguagenameByFilename(Root.Local.CurrentLanguageFile))
            {
                Root.ChangeLanguage(Root.Local.GetFilenameByLanguagename(comboLanguage.Text));
                FormOptions_LocalReload();
            }
            string local, st;
            using (var f = new StreamReader("lang/" + Root.Local.CurrentLanguageFile + ".txt"))
                local = f.ReadToEnd();
            using (var f = new StreamReader("lang/en-us.txt"))
                st = Root.CompleteConfig(f.ReadToEnd(), "\n" + local);
            if (st != "" && MessageBox.Show("The translation file seems to not include all the required translation. Do you want to add the missing entries ?", Root.Local.CurrentLanguageFile, MessageBoxButtons.YesNo) == DialogResult.Yes)
                using (var f = new StreamWriter("lang/" + Root.Local.CurrentLanguageFile + ".txt"))
                    f.Write(local + "\n" + st);
        }

        private void InverseWheelCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.InverseMousewheel = InverseWheelCb.Checked;
            InverseWheelCb.Text = Root.InverseMousewheel ? Root.Local.OptionsInverseMouseWheelChecked : Root.Local.OptionsInverseMouseWheel;
        }

        private void SnapInPointerKeysChanged(object sender, EventArgs e)
        {
            if (SnapInPointerHoldCb.SelectedIndex >= 0)
                Root.SnapInPointerHoldKey = (SnapInPointerKeys)(SnapInPointerHoldCb.SelectedIndex);
            if (SnapInPointerTwiceCb.SelectedIndex >= 0)
                Root.SnapInPointerPressTwiceKey = (SnapInPointerKeys)(SnapInPointerTwiceCb.SelectedIndex);
        }

        private void SubToolsBar_cb_CheckedChanged(object sender, EventArgs e) =>
            Root.SubToolsEnabled = SubToolsBar_cb.Checked;

        private void FormOptions_FormClosed(object sender, FormClosedEventArgs e) => GC.Collect();

        private void ActivateDbgWinBtn_Click(object sender, EventArgs e)
        {
            Program.ShowWindow(Program.GetConsoleWindow(), 1);
            Console.WriteLine("Debug Window activated");
        }

        private void APIRestEd_Validating(object sender, CancelEventArgs e)
        {
            if (Root.APIRest.ChangeAddress(APIRestEd.Text))
            {
                e.Cancel = false;
                APIRestEd.BackColor = Color.White;
                Root.APIRestUrl = APIRestEd.Text;
            }
            else
            {
                e.Cancel = true;
                APIRestEd.BackColor = Color.Orange;
                Root.APIRest.ChangeAddress(Root.APIRestUrl);
            }
        }

        private void APIRestEd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                SelectNextControl(ActiveControl, true, true, true, true);
                (sender as TextBox)?.Select();
                e.Handled = true;
            }
        }

        private void FitToCurveEd_CheckedChanged(object sender, EventArgs e) =>
            Root.FitToCurve = FitToCurveEd.Checked;

        private void CaptStrokesOnlyCb_CheckedChanged(object sender, EventArgs e) =>
            Root.StrokesOnlySnapshot = CaptStrokesOnlyCb.Checked;

        private void PensOnTwoLinesCb_CheckedChanged(object sender, EventArgs e) =>
            Root.PensOnTwoLines = PensOnTwoLinesCb.Checked;

        private void MeasureEnabledCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.MeasureEnabled = MeasureEnabledCb.Checked;
            if (!Root.MeasureEnabled)
                Root.MeasureWhileDrawing = false;
            MeasurementBox.Enabled = Root.MeasureEnabled;
        }

        private void Measure2ScaleEd_Validated(object sender, EventArgs e) =>
            Root.Measure2Scale = double.Parse(Measure2ScaleEd.Text, CultureInfo.InvariantCulture);

        private void Measure2DigEd_Validated(object sender, EventArgs e) =>
            Root.Measure2Digits = int.Parse(Measure2DigEd.Text);

        private void Measure2DigEd_Validating(object sender, CancelEventArgs e)
        {
            if (int.TryParse(Measure2DigEd.Text, out int d) && d >= 0 && d <= 9)
                Measure2DigEd.BackColor = Color.White;
            else
            {
                e.Cancel = true;
                Measure2DigEd.BackColor = Color.Orange;
                Measure2DigEd.Select();
            }
        }

        private void Measure2UnitEd_TextChanged(object sender, EventArgs e) =>
            Root.Measure2Unit = Measure2UnitEd.Text;

        private void MeasureAngleCb_CheckedChanged(object sender, EventArgs e) =>
            Root.MeasureAnglCounterClockwise = MeasureAngleCb.Checked;

        private void ColorPickerEnaCb_CheckedChanged(object sender, EventArgs e) =>
            Root.ColorPickerEnabled = ColorPickerEnaCb.Checked;

        private void comboPensLineStyle_Changed(object sender, EventArgs e)
        {
            Panel p = (Panel)sender;
            string s = Root.NextLineStyleString(Root.LineStyleToString(Root.PenAttr[(int)p.Tag].ExtendedProperties));
            p.BackgroundImage = FormCollection.getImgFromDiskOrRes("DashStyle" + s);
            DashStyle ds = Root.LineStyleFromString(s);
            if (ds == DashStyle.Custom)
                try { Root.PenAttr[(int)p.Tag].ExtendedProperties.Remove(Root.DASHED_LINE_GUID); } catch { }
            else
                Root.PenAttr[(int)p.Tag].ExtendedProperties.Add(Root.DASHED_LINE_GUID, ds);
        }

        private void SwapSnapBehaviorsCb_CheckedChanged(object sender, EventArgs e) =>
            Root.SwapSnapsBehaviors = !SwapSnapsBehviorsCb.Checked;

        private void AltAsOneCommandCb_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                AltAsOneCommandCb.CheckState = CheckState.Indeterminate;
        }

        private void AltAsOneCommandCb_CheckStateChanged(object sender, EventArgs e)
        {
            switch (AltAsOneCommandCb.CheckState)
            {
                case CheckState.Checked: Root.AltAsOneCommand = 2; break;
                case CheckState.Indeterminate: Root.AltAsOneCommand = 1; break;
                default: Root.AltAsOneCommand = 0; break;
            }
        }

        private void AltAsOneCommandCb_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                AltAsOneCommandCb.CheckState = CheckState.Indeterminate;
        }

        private void CbHKRot_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
                Root.LineStyleRotateEnabled |= Convert.ToUInt32((int)(cb.Tag));
            else
                Root.LineStyleRotateEnabled &= 0xFF ^ Convert.ToUInt32((int)(cb.Tag));
        }

        private void Click4StrokeCb_CheckedChanged(object sender, EventArgs e) =>
            Root.ButtonClick_For_LineStyle = Click4StrokeCb.Checked;

        private void SpotColorPnl_Click(object sender, EventArgs e)
        {
            PenModifyDlg dlg = new PenModifyDlg(Root) { Text = "" };
            Microsoft.Ink.DrawingAttributes at = new Microsoft.Ink.DrawingAttributes
            {
                Transparency = (byte)(255 - Root.SpotLightColor.A),
                Color = Color.FromArgb(Root.SpotLightColor.A, Root.SpotLightColor.R, Root.SpotLightColor.G, Root.SpotLightColor.B),
                Width = 0
            };
            if (dlg.ModifyPen(ref at))
            {
                Root.SpotLightColor = Color.FromArgb(255 - at.Transparency, at.Color);
                SpotColorPnl.BackColor = Root.SpotLightColor;
            }
            dlg.Dispose();
        }

        private void SpotOnAltCb_CheckedChanged(object sender, EventArgs e) =>
            Root.SpotOnAlt = SpotOnAltCb.Checked;

        private void SpotRadTb_Validated(object sender, EventArgs e) =>
            Root.SpotLightRadius = (int)(float.Parse(SpotRadTb.Text, CultureInfo.InvariantCulture) / 100.0F * System.Windows.SystemParameters.PrimaryScreenWidth);

        private void NewArrowEditBtn_Click(object sender, EventArgs e)
        {
            ArrowSelDlg dlg = new ArrowSelDlg(Root);
            dlg.ShowDialog();
        }

        private void MagnetAngleEd_Validated(object sender, EventArgs e)
        {
            Root.MagneticAngle = float.Parse(MagnetAngleEd.Text, CultureInfo.InvariantCulture);
            Root.MagneticAngleTolerance = Root.MagneticAngle * (Root.MagneticAngleTolRatio);
        }

        private void IndexOnDockUndockCb_CheckedChanged(object sender, EventArgs e) =>
            Root.CreateIndexOnUndock = M3UIndexOnUndockCb.Checked;

        private void FfmegFileNameTxt_Validated(object sender, EventArgs e) =>
            Root.FFMpegFileName = FfmegFileNameTxt.Text;

        private void IndexDefaultTxt_Validated(object sender, EventArgs e) =>
            Root.IndexDefaultText = M3UIndexDefaultTxt.Text;

        private void IndexDefaultTxt_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Root != null)
                    Root.IndexDefaultText = M3UIndexDefaultTxt.Text;
            }
            catch
            {
                // Défensif : ne pas casser l'UI si Root n'est pas encore initialisé
            }
        }

        private void CreateM3u_CheckedChanged(object sender, EventArgs e)
        {
            Root.CreateM3U = CreateM3u.Checked;
            M3UOptions.Enabled = Root.IsVideoRecordingSelected() && Root.CreateM3U;
        }

        private void UndockOnIndexCb_CheckedChanged(object sender, EventArgs e) =>
            Root.UndockOnIndexCreate = UndockOnM3UIndexCb.Checked;

        private void NoEditM3Cb_CheckedChanged(object sender, EventArgs e) =>
            Root.NoEditM3UEntry = NoEditM3UIndexCb.Checked;

        private void tbSnapFileTemplate_TextChanged(object sender, EventArgs e)
        {
            string s = Root.ExpandVarCmd(tbSnapFileTemplate.Text, 0, 0, 0, 0);
            tbSnapFileTemplate.BackColor = (s.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) ? Color.Orange : Color.White;
            toolTip.SetToolTip(tbSnapFileTemplate, "ex: " + s);
        }

        private void tbSnapFileTemplate_Validating(object sender, CancelEventArgs e)
        {
            if (Root.ExpandVarCmd(tbSnapFileTemplate.Text, 0, 0, 0, 0).IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                tbSnapFileTemplate.BackColor = Color.Orange;
                e.Cancel = true;
            }
            else
            {
                tbSnapFileTemplate.BackColor = Color.White;
            }
        }

        private void tbSnapFileTemplate_Validated(object sender, EventArgs e) =>
            Root.SnapshotFileTemplate = tbSnapFileTemplate.Text;

        private void tbSnapPath_TextChanged(object sender, EventArgs e)
        {
            string s = Root.ExpandVarCmd(tbSnapPath.Text, 0, 0, 0, 0).Replace("/", "\\");
            tbSnapPath.BackColor = (s.IndexOfAny(Path.GetInvalidPathChars()) >= 0) ? Color.Orange : Color.White;
            toolTip.SetToolTip(tbSnapPath, "ex: " + s);
        }

        private void StartFoldedCb_CheckedChanged(object sender, EventArgs e) =>
            Root.KeepDockedAtOpen = StartFoldedCb.Checked;

        private void KeepUnfoldedPointerCb_CheckedChanged(object sender, EventArgs e) =>
            Root.KeepUnDockedAtPointer = KeepUnfoldedPointerCb.Checked;

        private void ExtraPensCb_CheckedChanged(object sender, EventArgs e) =>
            Root.PensExtraSet = ExtraPensCb.Checked;

        private void VideoTabCtrl_Selecting(object sender, TabControlCancelEventArgs e) =>
            Cursor.Current = Cursors.WaitCursor;

        private void VideoTabCtrl_SelectedIndexChanged(object sender, EventArgs e) =>
            Cursor.Current = Cursors.Default;

        private void cbPagesEnabled_CheckedChanged(object sender, EventArgs e) =>
            Root.PagesEnabled = ((CheckBox)sender).Checked;

        private void MeasureWhileDrawing_CheckedChanged(object sender, EventArgs e) =>
            Root.MeasureWhileDrawing = ((CheckBox)sender).Checked;

        private void TextBackgroundCb_Click(object sender, EventArgs e) { }

        private void TextBackgroundLst_SelectedIndexChanged(object sender, EventArgs e) =>
            Root.TextBackground = TextBackgroundLst.SelectedIndex;
    }
}