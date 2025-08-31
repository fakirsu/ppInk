using System;
using System.Drawing;
using System.Windows.Forms;

namespace gInk
{
    partial class FormCollection
    {
        public bool NumberTag_ShowNumber = true;
        public bool NumberTag_FirstIsWhite = false;
        private int NumberTag_Counter = 1;

        public Button btNTag_Show_White;
        public Button btNTag_Show_Black;
        public Button btNTag_Hide_White;
        public Button btNTag_Hide_Black;
        //private Button NumberTag_CurrentHighlighted;

        // --- API publique simple ---
        public void NumberTag_Reset() => NumberTag_Counter = 1;
        public void NumberTag_ToggleShowNumber() => NumberTag_ShowNumber = !NumberTag_ShowNumber;
        public void NumberTag_SetFirstIsWhite(bool firstWhite) => NumberTag_FirstIsWhite = firstWhite;
        public string NumberTag_GetAndIncrementText() { var s = NumberTag_Counter.ToString(); NumberTag_Counter++; return s; }

        // Chargement + rognage + MARGIN pour laisser de la place à la bordure de sélection
        private Bitmap LoadNumberTagIconTrimmed(string name, int dim)
        {
            Bitmap src = null;
            try { src = getImgFromDiskOrRes(name, ImageExts); }
            catch { return BuildFallback(dim); }
            if (src == null) return BuildFallback(dim);

            // Détection zone non transparente
            int minX = src.Width, minY = src.Height, maxX = -1, maxY = -1;
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    Color c = src.GetPixel(x, y);
                    if (c.A > 8)
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
                // Image totalement transparente -> fallback
                src.Dispose();
                return BuildFallback(dim);
            }

            int w = maxX - minX + 1;
            int h = maxY - minY + 1;
            Rectangle crop = new Rectangle(minX, minY, w, h);

            Bitmap cropped = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(cropped))
            {
                g.DrawImage(src, new Rectangle(0, 0, w, h), crop, GraphicsUnit.Pixel);
            }
            src.Dispose();

            // Margin pour que la bordure du bouton soit visible (surtout sur disque blanc)
            int margin = Math.Max(2, dim / 10); // ajustable
            float scale = Math.Min((float)(dim - 2 * margin) / w, (float)(dim - 2 * margin) / h);
            int dw = Math.Max(1, (int)Math.Round(w * scale));
            int dh = Math.Max(1, (int)Math.Round(h * scale));
            int ox = (dim - dw) / 2;
            int oy = (dim - dh) / 2;

            Bitmap dst = new Bitmap(dim, dim);
            using (Graphics g = Graphics.FromImage(dst))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.DrawImage(cropped, new Rectangle(ox, oy, dw, dh),
                    new Rectangle(0, 0, w, h), GraphicsUnit.Pixel);
            }
            cropped.Dispose();

            // Sanity check (éviter bitmap vide)
            if (!HasVisiblePixel(dst))
            {
                dst.Dispose();
                return BuildFallback(dim);
            }
            return dst;
        }

        private bool HasVisiblePixel(Bitmap bmp)
        {
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                    if (bmp.GetPixel(x, y).A > 16) return true;
            return false;
        }

        private Bitmap BuildFallback(int dim)
        {
            var b = new Bitmap(dim, dim);
            using (var g = Graphics.FromImage(b))
            {
                g.Clear(Color.Transparent);
                using (var pen = new Pen(Color.Red, Math.Max(1, dim / 8)))
                    g.DrawEllipse(pen, Math.Max(1, dim / 8), Math.Max(1, dim / 8),
                        dim - 2 * Math.Max(1, dim / 8) - 1, dim - 2 * Math.Max(1, dim / 8) - 1);
            }
            return b;
        }

        public Button CreateNumberTagButtons(int dim1s, int dim2s, Button anchor)
        {
            // Nettoyage précédent (ne pas disposer des images utilisées par d'autres encore en peinture)
            RemoveOldNumberTagButtons();

            var cfg = new (bool show, bool firstWhite, string icon)[]
            {
                (true,  true,  "tool_numb_fillW"),
                (true,  false, "tool_numb_fillB"),
                (false, true,  "tool_numb_fillWfalse"),
                (false, false, "tool_numb_fillBfalse")
            };

            if (anchor == null)
                anchor = btArrow;

            int spacing = 0;
            try
            {
                if (btRect != null && btHand != null && btRect.Visible && btHand.Visible)
                    spacing = btRect.Left - (btHand.Left + btHand.Width);
                if (spacing <= 0) spacing = Math.Max(2, dim1s / 6);
            }
            catch { spacing = Math.Max(2, dim1s / 6); }

            btNTag_Show_White = BuildNTagButton("btNTag_0", cfg[0], dim1s);
            btNTag_Show_Black = BuildNTagButton("btNTag_1", cfg[1], dim1s);
            btNTag_Hide_White = BuildNTagButton("btNTag_2", cfg[2], dim1s);
            btNTag_Hide_Black = BuildNTagButton("btNTag_3", cfg[3], dim1s);

            // Position rangée haute
            SetButtonPosition(anchor, btNTag_Show_White, spacing);
            SetButtonPosition(btNTag_Show_White, btNTag_Show_Black, spacing);
            // Rangée basse (utilise helper vertical relatif)
            SetSmallButtonNext(btNTag_Show_White, btNTag_Hide_White, dim2s);
            SetSmallButtonNext(btNTag_Show_Black, btNTag_Hide_Black, dim2s);

            UpdateNumberTagButtonBorders();
            //UpdateNumberTagButtonBorders();
            InvalidateNumberTagButtons(); // s’assure que l’anneau apparaît immédiatement
            AdjustToolbarSize();

            return (Root.ToolbarOrientation <= Orientation.Horizontal) ? btNTag_Show_Black : btNTag_Hide_White;
        }

        private Button BuildNTagButton(string name, (bool show, bool firstWhite, string icon) cfg, int dim)
        {
            var b = new Button
            {
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Width = dim,
                Height = dim,
                Visible = true,
                Name = name,
                BackgroundImageLayout = ImageLayout.Stretch,
                TabStop = false,
                Tag = Tuple.Create(cfg.show, cfg.firstWhite)
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.BorderColor = Color.Orange; // Couleur de sélection
            try { b.BackgroundImage = LoadNumberTagIconTrimmed(cfg.icon, dim); } catch { }
            try { toolTip.SetToolTip(b, Root?.Local?.ButtonNameNumb ?? "Number tag"); } catch { }
            b.Click += NumberTagBtn_Click;
            b.MouseDown += btAllButtons_MouseDown;
            b.MouseUp += btAllButtons_MouseUp;
            b.MouseMove += gpButtons_MouseMove;
            b.ContextMenu = new ContextMenu();
            b.ContextMenu.Popup += btAllButtons_RightClick;
            b.Paint += NumberTagButton_Paint;
            gpButtons.Controls.Add(b);
            b.BringToFront();
            return b;
        }

        private void RemoveOldNumberTagButtons()
        {
            try
            {
                foreach (Control c in gpButtons.Controls)
                {
                    if (c is Button btn && btn.Name.StartsWith("btNTag_"))
                    {
                        try
                        {
                            var img = btn.BackgroundImage;
                            btn.BackgroundImage = null;
                            img?.Dispose();
                        }
                        catch { }
                        try { btn.Paint -= NumberTagButton_Paint; } catch { }
                        btn.Dispose();
                    }
                }
            }
            catch { }
        }

        private void NumberTagBtn_Click(object sender, EventArgs e)
        {
            var b = sender as Button;
            if (b == null) return;
            var tup = b.Tag as Tuple<bool, bool>;
            if (tup == null) return;

            // Nettoyage encre (comportement précédent conservé)
            try { btClear_Click(null, null); } catch { }

            NumberTag_ShowNumber = tup.Item1;
            NumberTag_FirstIsWhite = tup.Item2;
            NumberTag_Reset();
            Root.FilledSelected = NumberTag_FirstIsWhite ? Filling.WhiteFilled : Filling.BlackFilled;

            SelectTool(Tools.NumberTag, Root.FilledSelected);
            UpdateNumberTagButtonBorders();
            InvalidateNumberTagButtons();
            Root.UponButtonsUpdate |= 0x2;
        }

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

        //private void InvalidateNumberTagButtons()
        //{
        //    btNTag_Show_White?.Invalidate();
        //    btNTag_Show_Black?.Invalidate();
        //    btNTag_Hide_White?.Invalidate();
        //    btNTag_Hide_Black?.Invalidate();
        //}

        //private void UpdateNumberTagButtonBorders()
        //{
        //    // On désactive toute bordure "Flat" (on passe par Paint uniquement)
        //    try { btNTag_Show_White?.FlatAppearance?.SetBorderSizeSafely(0); } catch { }
        //    try { btNTag_Show_Black?.FlatAppearance?.SetBorderSizeSafely(0); } catch { }
        //    try { btNTag_Hide_White?.FlatAppearance?.SetBorderSizeSafely(0); } catch { }
        //    try { btNTag_Hide_Black?.FlatAppearance?.SetBorderSizeSafely(0); } catch { }

        //    Button previous = NumberTag_CurrentHighlighted;
        //    NumberTag_CurrentHighlighted = null;

        //    if (Root == null || Root.ToolSelected != Tools.NumberTag)
        //    {
        //        // Plus de surbrillance : on redessine juste pour effacer l’anneau éventuel
        //        if (previous != null) InvalidateNumberTagButtons();
        //        return;
        //    }

        //    Button target =
        //        (NumberTag_ShowNumber && NumberTag_FirstIsWhite) ? btNTag_Show_White :
        //        (NumberTag_ShowNumber && !NumberTag_FirstIsWhite) ? btNTag_Show_Black :
        //        (!NumberTag_ShowNumber && NumberTag_FirstIsWhite) ? btNTag_Hide_White :
        //        btNTag_Hide_Black;

        //    NumberTag_CurrentHighlighted = target;

        //    if (previous != target)
        //    {
        //        // On redessine les 4 (faible coût, simplifie)
        //        InvalidateNumberTagButtons();
        //    }
        //}

        //private void NumberTagButton_Paint(object sender, PaintEventArgs e)
        //{
        //    var btn = sender as Button;
        //    if (btn == null) return;

        //    // Ne dessine que si c'est le bouton actuellement surligné
        //    if (btn != NumberTag_CurrentHighlighted) return;
        //    if (Root == null || Root.ToolSelected != Tools.NumberTag) return;

        //    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        //    bool isWhiteVariant = (btn == btNTag_Show_White) || (btn == btNTag_Hide_White);
        //    Color ring = isWhiteVariant ? Color.DarkOrange : Color.Orange;
        //    int thick = Math.Max(2, btn.Width / 10);

        //    var r = new Rectangle(thick / 2, thick / 2, btn.Width - thick - 1, btn.Height - thick - 1);
        //    using (var pen = new Pen(ring, thick))
        //    {
        //        pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
        //        e.Graphics.DrawEllipse(pen, r); // ellipse = anneau circulaire (plus harmonieux pour pastilles)
        //    }
        //}


        private void InvalidateNumberTagButtons()
        {
            btNTag_Show_White?.Invalidate();
            btNTag_Show_Black?.Invalidate();
            btNTag_Hide_White?.Invalidate();
            btNTag_Hide_Black?.Invalidate();
        }

        private void UpdateNumberTagButtonBorders()
        {
            // Plus de bordures Flat ; tout se fait dans Paint.
            if (Root == null) return;

            if (Root.ToolSelected == Tools.NumberTag)
                InvalidateNumberTagButtons();
            else
            {
                // Effacer éventuels restes : simple inval global
                InvalidateNumberTagButtons();
            }
        }

        private void NumberTagButton_Paint(object sender, PaintEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            if (Root == null) return;
            if (Root.ToolSelected != Tools.NumberTag) return;

            var tup = btn.Tag as Tuple<bool, bool>;
            if (tup == null) return;

            // Ce bouton est-il la variante sélectionnée ?
            if (tup.Item1 != NumberTag_ShowNumber || tup.Item2 != NumberTag_FirstIsWhite)
                return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            bool isWhiteVariant = tup.Item2; // firstWhite==true => variante blanche
            Color ring = isWhiteVariant ? Color.DarkOrange : Color.Orange;
            int thick = Math.Max(3, btn.Width / 9); // anneau un peu plus épais

            var r = new Rectangle(thick / 2, thick / 2,
                                  btn.Width - thick - 1,
                                  btn.Height - thick - 1);
            using (var pen = new Pen(ring, thick))
            {
                pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                e.Graphics.DrawEllipse(pen, r);
            }
        }

    }

    // Classe d'extension de niveau namespace pour FlatButtonAppearance (doit être en dehors de la classe partielle)
    static class ButtonAppearanceExtensions
    {
        public static void SetBorderSizeSafely(this FlatButtonAppearance appearance, int size)
        {
            try
            {
                if (appearance != null)
                    appearance.BorderSize = size;
            }
            catch { }
        }
    }
}