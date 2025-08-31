using System;
using System.Drawing;
using System.Windows.Forms;

namespace gInk
{
    partial class FormCollection
    {
        // Variables d'état (non persistées)
        // true = afficher le numéro à l'intérieur de la pastille
        public bool NumberTag_ShowNumber = true;
        // true = première pastille blanche, false = première pastille noire
        public bool NumberTag_FirstIsWhite = false;

        // compteur courant (commence à 1)
        private int NumberTag_Counter = 1;

        // Buttons supplémentaires (4 combinaisons)
        public Button btNTag_Show_White;
        public Button btNTag_Show_Black;
        public Button btNTag_Hide_White;
        public Button btNTag_Hide_Black;

        // Réinitialiser (à appeler lors d'un reclique sur l'outil)
        public void NumberTag_Reset()
        {
            NumberTag_Counter = 1;
        }

        // Basculer l'affichage du numéro (ex: hotkey ou menu)
        public void NumberTag_ToggleShowNumber()
        {
            NumberTag_ShowNumber = !NumberTag_ShowNumber;
        }

        // Setter rapide pour la couleur de départ
        public void NumberTag_SetFirstIsWhite(bool firstWhite)
        {
            NumberTag_FirstIsWhite = firstWhite;
        }

        // Récupère et incrémente le texte numérique courant
        public string NumberTag_GetAndIncrementText()
        {
            var s = NumberTag_Counter.ToString();
            NumberTag_Counter++;
            return s;
        }

        //// Rogne les bords totalement transparents et recentre dans un carré dim×dim
        //private Bitmap LoadNumberTagIconTrimmed(string name, int dim)
        //{
        //    Bitmap src = null;
        //    try { src = getImgFromDiskOrRes(name, ImageExts); }
        //    catch { return new Bitmap(dim, dim); }

        //    int minX = src.Width, minY = src.Height, maxX = -1, maxY = -1;
        //    for (int y = 0; y < src.Height; y++)
        //    {
        //        for (int x = 0; x < src.Width; x++)
        //        {
        //            Color c = src.GetPixel(x, y);
        //            if (c.A > 8)
        //            {
        //                if (x < minX) minX = x;
        //                if (y < minY) minY = y;
        //                if (x > maxX) maxX = x;
        //                if (y > maxY) maxY = y;
        //            }
        //        }
        //    }
        //    if (maxX < 0)
        //    {
        //        Bitmap empty = new Bitmap(dim, dim);
        //        src.Dispose();
        //        return empty;
        //    }
        //    int w = maxX - minX + 1;
        //    int h = maxY - minY + 1;
        //    Rectangle crop = new Rectangle(minX, minY, w, h);
        //    Bitmap cropped = new Bitmap(w, h);
        //    using (Graphics g = Graphics.FromImage(cropped))
        //    {
        //        g.DrawImage(src, new Rectangle(0, 0, w, h), crop, GraphicsUnit.Pixel);
        //    }
        //    src.Dispose();

        //    Bitmap dst = new Bitmap(dim, dim);
        //    using (Graphics g = Graphics.FromImage(dst))
        //    {
        //        g.Clear(Color.Transparent);
        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        //        float scale = Math.Min((float)dim / w, (float)dim / h);
        //        int dw = (int)Math.Round(w * scale);
        //        int dh = (int)Math.Round(h * scale);
        //        int ox = (dim - dw) / 2;
        //        int oy = (dim - dh) / 2;
        //        g.DrawImage(cropped, new Rectangle(ox, oy, dw, dh), new Rectangle(0, 0, w, h), GraphicsUnit.Pixel);
        //    }
        //    cropped.Dispose();
        //    return dst;
        //}

        // Rogne les bords, puis normalise le diamètre visible pour assurer une taille identique entre variantes
        private Bitmap LoadNumberTagIconTrimmed(string name, int dim)
        {
            const int AlphaThreshold = 16;                 // seuil minimal pour considérer le pixel "non vide"
            const float NormalizedContentRatio = 0.90f;    // pourcentage du côté dim occupé par le disque (adapter si nécessaire)
            const bool DebugFrame = false;                 // true => dessine un cadre vert (diagnostic)

            Bitmap src = null;
            try { src = getImgFromDiskOrRes(name, ImageExts); }
            catch { return new Bitmap(dim, dim); }

            int minX = src.Width, minY = src.Height, maxX = -1, maxY = -1;
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    var c = src.GetPixel(x, y);
                    if (c.A > AlphaThreshold)
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX < 0)
            {
                // Icône vide
                Bitmap empty = new Bitmap(dim, dim);
                src.Dispose();
                return empty;
            }

            int w = maxX - minX + 1;
            int h = maxY - minY + 1;
            Rectangle crop = new Rectangle(minX, minY, w, h);

            // Extraction
            Bitmap cropped = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(cropped))
            {
                g.DrawImage(src, new Rectangle(0, 0, w, h), crop, GraphicsUnit.Pixel);
            }
            src.Dispose();

            // Normalisation du diamètre : on part de la plus grande dimension
            int contentSize = Math.Max(w, h);
            int targetContent = Math.Max(1, (int)Math.Round(dim * NormalizedContentRatio));
            float scale = targetContent / (float)contentSize;

            int dw = (int)Math.Round(w * scale);
            int dh = (int)Math.Round(h * scale);

            // Centrage
            int ox = (dim - dw) / 2;
            int oy = (dim - dh) / 2;

            Bitmap dst = new Bitmap(dim, dim);
            using (Graphics g = Graphics.FromImage(dst))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                g.DrawImage(cropped, new Rectangle(ox, oy, dw, dh), new Rectangle(0, 0, w, h), GraphicsUnit.Pixel);

                if (DebugFrame)
                {
                    using (var pen = new Pen(Color.Lime, 1))
                        g.DrawRectangle(pen, 0, 0, dim - 1, dim - 1);
                }
            }

            cropped.Dispose();
            return dst;
        }


        public Button CreateNumberTagButtons(int dim1s, int dim2s, Button anchor)
        {
            // 1. Masquer / nettoyer l’ancien bouton unique
            try
            {
                if (btNumb != null)
                {
                    btNumb.Visible = false;
                    try { btNumb.BackgroundImage?.Dispose(); } catch { }
                    btNumb.BackgroundImage = null;
                }
            }
            catch { }

            // 2. Supprimer d’éventuels boutons déjà créés (ré‑initialisation / réorientation)
            foreach (Control c in gpButtons.Controls)
            {
                if (c is Button && c.Name.StartsWith("btNTag_"))
                    c.Dispose();
            }
            btNTag_Show_White = btNTag_Show_Black = btNTag_Hide_White = btNTag_Hide_Black = null;

            // 3. Configuration des 4 variantes
            var cfg = new (bool show, bool firstWhite, string icon, string tip)[]
            {
        (true,  true,  "tool_numb_fillW",      Root?.Local?.ButtonNameNumb ?? "Number tag (Show / White)"),
        (true,  false, "tool_numb_fillB",      Root?.Local?.ButtonNameNumb ?? "Number tag (Show / Black)"),
        (false, true,  "tool_numb_fillWfalse", Root?.Local?.ButtonNameNumb ?? "Number tag (Hide / White)"),
        (false, false, "tool_numb_fillBfalse", Root?.Local?.ButtonNameNumb ?? "Number tag (Hide / Black)")
            };

            if (anchor == null)
                anchor = btArrow;

            // 4. Déduire l’espacement horizontal (si possible depuis deux petits boutons déjà posés)
            int spacing = 0;
            try
            {
                if (btRect != null && btHand != null && btRect.Visible && btHand.Visible)
                {
                    spacing = btRect.Left - (btHand.Left + btHand.Width);
                }
                if (spacing <= 0)
                    spacing = Math.Max(2, dim1s / 6);
            }
            catch { spacing = Math.Max(2, dim1s / 6); }

            // 5. Création des 4 boutons :
            //    Rangée haute : show/white puis show/black (après anchor)
            //    Rangée basse : hide/white sous show/white ; hide/black sous show/black
            Button bShowWhite = new Button
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Width = dim1s,
                Height = dim1s,
                Visible = true,
                Name = "btNTag_0",
                BackgroundImageLayout = ImageLayout.Stretch,
                TabStop = false
            };
            bShowWhite.FlatAppearance.BorderSize = 0;

            try { bShowWhite.BackgroundImage = LoadNumberTagIconTrimmed(cfg[0].icon, dim1s); } catch { }
            bShowWhite.Tag = Tuple.Create(cfg[0].show, cfg[0].firstWhite);
            try { toolTip.SetToolTip(bShowWhite, cfg[0].tip + " (1)"); } catch { }
            bShowWhite.Click += NumberTagBtn_Click;
            bShowWhite.MouseDown += btAllButtons_MouseDown;
            bShowWhite.MouseUp += btAllButtons_MouseUp;
            bShowWhite.MouseMove += gpButtons_MouseMove;
            bShowWhite.ContextMenu = new ContextMenu();
            bShowWhite.ContextMenu.Popup += btAllButtons_RightClick;
            gpButtons.Controls.Add(bShowWhite);
            gpButtons.Controls.SetChildIndex(bShowWhite, 0);
            bShowWhite.BringToFront();
            btNTag_Show_White = bShowWhite;

            // show / black (top-right)
            Button bShowBlack = new Button
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Width = dim1s,
                Height = dim1s,
                Visible = true,
                Name = "btNTag_1",
                BackgroundImageLayout = ImageLayout.Stretch,
                TabStop = false
            };
            bShowBlack.FlatAppearance.BorderSize = 0;
            try { bShowBlack.BackgroundImage = LoadNumberTagIconTrimmed(cfg[1].icon, dim1s); } catch { }
            bShowBlack.Tag = Tuple.Create(cfg[1].show, cfg[1].firstWhite);
            try { toolTip.SetToolTip(bShowBlack, cfg[1].tip + " (2)"); } catch { }
            bShowBlack.Click += NumberTagBtn_Click;
            bShowBlack.MouseDown += btAllButtons_MouseDown;
            bShowBlack.MouseUp += btAllButtons_MouseUp;
            bShowBlack.MouseMove += gpButtons_MouseMove;
            bShowBlack.ContextMenu = new ContextMenu();
            bShowBlack.ContextMenu.Popup += btAllButtons_RightClick;
            gpButtons.Controls.Add(bShowBlack);
            bShowBlack.BringToFront();
            btNTag_Show_Black = bShowBlack;

            // Positionner les deux premiers avec les helpers standards
            SetButtonPosition(anchor, bShowWhite, spacing);
            SetButtonPosition(bShowWhite, bShowBlack, spacing);

            // hide / white (bottom-left)
            Button bHideWhite = new Button
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Width = dim1s,
                Height = dim1s,
                Visible = true,
                Name = "btNTag_2",
                BackgroundImageLayout = ImageLayout.Stretch,
                TabStop = false
            };
            bHideWhite.FlatAppearance.BorderSize = 0;
            try { bHideWhite.BackgroundImage = LoadNumberTagIconTrimmed(cfg[2].icon, dim1s); } catch { }
            bHideWhite.Tag = Tuple.Create(cfg[2].show, cfg[2].firstWhite);
            try { toolTip.SetToolTip(bHideWhite, cfg[2].tip + " (3)"); } catch { }
            bHideWhite.Click += NumberTagBtn_Click;
            bHideWhite.MouseDown += btAllButtons_MouseDown;
            bHideWhite.MouseUp += btAllButtons_MouseUp;
            bHideWhite.MouseMove += gpButtons_MouseMove;
            bHideWhite.ContextMenu = new ContextMenu();
            bHideWhite.ContextMenu.Popup += btAllButtons_RightClick;
            gpButtons.Controls.Add(bHideWhite);
            bHideWhite.BringToFront();
            btNTag_Hide_White = bHideWhite;

            // hide / black (bottom-right)
            Button bHideBlack = new Button
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Width = dim1s,
                Height = dim1s,
                Visible = true,
                Name = "btNTag_3",
                BackgroundImageLayout = ImageLayout.Stretch,
                TabStop = false
            };
            bHideBlack.FlatAppearance.BorderSize = 0;
            try { bHideBlack.BackgroundImage = LoadNumberTagIconTrimmed(cfg[3].icon, dim1s); } catch { }
            bHideBlack.Tag = Tuple.Create(cfg[3].show, cfg[3].firstWhite);
            try { toolTip.SetToolTip(bHideBlack, cfg[3].tip + " (4)"); } catch { }
            bHideBlack.Click += NumberTagBtn_Click;
            bHideBlack.MouseDown += btAllButtons_MouseDown;
            bHideBlack.MouseUp += btAllButtons_MouseUp;
            bHideBlack.MouseMove += gpButtons_MouseMove;
            bHideBlack.ContextMenu = new ContextMenu();
            bHideBlack.ContextMenu.Popup += btAllButtons_RightClick;
            gpButtons.Controls.Add(bHideBlack);
            bHideBlack.BringToFront();
            btNTag_Hide_Black = bHideBlack;

            // Empilement rangée basse via helper (mêmes offsets que le reste de la barre)
            SetSmallButtonNext(bShowWhite, bHideWhite, dim2s);
            SetSmallButtonNext(bShowBlack, bHideBlack, dim2s);

            // Visuel sélection
            UpdateNumberTagButtonBorders();

            // Ajuster panel (si nouvelle largeur)
            AdjustToolbarSize();

            // Bouton de référence de la suite : on retourne le top-right (bShowBlack)
            if (Root.ToolbarOrientation <= Orientation.Horizontal)
                return bShowBlack;
            else
                return bHideWhite;
        }

        // Click handler commun
        private void NumberTagBtn_Click(object sender, EventArgs e)
        {
            var b = sender as Button;
            if (b == null) return;
            var tup = b.Tag as Tuple<bool, bool>;
            if (tup == null) return;

            // Effacement automatique à la sélection d'une variante number-tag
            try
            {
                btClear_Click(null, null);
            }
            catch
            {
                // silent fallback si pb
            }

            NumberTag_ShowNumber = tup.Item1;
            NumberTag_FirstIsWhite = tup.Item2;

            // reset compteur sur sélection explicite
            NumberTag_Reset();

            // set current filling selon la préférence
            Root.FilledSelected = NumberTag_FirstIsWhite ? Filling.WhiteFilled : Filling.BlackFilled;

            // feedback visuel
            UpdateNumberTagButtonBorders();

            // sélectionner l'outil NumberTag explicitement
            SelectTool(Tools.NumberTag, Root.FilledSelected);

            Root.UponButtonsUpdate |= 0x2;
        }


        // Efface toutes les bordures des 4 boutons de pastilles
        private void ClearNumberTagButtonBorders()
        {
            void clr(Button b)
            {
                if (b == null) return;
                b.FlatAppearance.BorderSize = 0;
            }
            clr(btNTag_Show_White);
            clr(btNTag_Show_Black);
            clr(btNTag_Hide_White);
            clr(btNTag_Hide_Black);
        }


        //// Met à jour l'apparence (bordure) des 4 boutons pour montrer la sélection active
        //private void UpdateNumberTagButtonBorders()
        //{
        //    foreach (Control c in gpButtons.Controls)
        //    {
        //        if (!(c is Button)) continue;
        //        if (!c.Name.StartsWith("btNTag_")) continue;
        //        (c as Button).FlatAppearance.BorderSize = 0;
        //    }

        //    string matchName;
        //    if (NumberTag_ShowNumber && NumberTag_FirstIsWhite) matchName = "btNTag_0";
        //    else if (NumberTag_ShowNumber && !NumberTag_FirstIsWhite) matchName = "btNTag_1";
        //    else if (!NumberTag_ShowNumber && NumberTag_FirstIsWhite) matchName = "btNTag_2";
        //    else matchName = "btNTag_3";

        //    try
        //    {
        //        Control c = gpButtons.Controls[matchName];
        //        if (c is Button)
        //            (c as Button).FlatAppearance.BorderSize = 3;
        //    }
        //    catch { }
        //}


        // Met à jour l'apparence (bordure) des 4 boutons pour montrer la sélection active (uniquement si l'outil NumberTag est actif)
        private void UpdateNumberTagButtonBorders()
        {
            if (Root == null || Root.ToolSelected != Tools.NumberTag)
            {
                // Si on n'est plus dans l'outil NumberTag, on nettoie tout
                ClearNumberTagButtonBorders();
                return;
            }

            ClearNumberTagButtonBorders();

            string matchName;
            if (NumberTag_ShowNumber && NumberTag_FirstIsWhite) matchName = "btNTag_0";
            else if (NumberTag_ShowNumber && !NumberTag_FirstIsWhite) matchName = "btNTag_1";
            else if (!NumberTag_ShowNumber && NumberTag_FirstIsWhite) matchName = "btNTag_2";
            else matchName = "btNTag_3";

            try
            {
                var c = gpButtons.Controls[matchName] as Button;
                if (c != null)
                {
                    c.FlatAppearance.BorderSize = 3;
                    // Optionnel : couleur de bordure personnalisée
                    c.FlatAppearance.BorderColor = Color.Orange; // Ajuster si besoin
                }
            }
            catch { }
        }

    }
}