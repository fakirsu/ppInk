using System;
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

        // Crée et positionne les 4 boutons number-tag.
        // dim1s : taille des petites icônes ; dim2s : espacement utilisé (sera passé depuis Initialize)
        // anchor : bouton après lequel positionner la première pastille (généralement btArrow)
        // Renvoie le dernier bouton créé (à utiliser comme 'prev' pour le placement du bouton suivant).
        public Button CreateNumberTagButtons(int dim1s, int dim2s, Button anchor)
        {
            // masquer le bouton legacy (designer) et libérer son image si présent
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

            // configuration pour les 4 boutons : (showNumber, firstIsWhite, iconName, tooltip)
            // Utilise les images "*false" pour les variantes sans numéro (show==false)
            var cfg = new (bool show, bool firstWhite, string icon, string tip)[]
            {
                // show = true  -> icônes avec numéro (existantes)
                (true,  true,  "tool_numb_fillW", Root?.Local?.ButtonNameNumb ?? "Numbered chip (Show / White)"),
                (true,  false, "tool_numb_fillB", Root?.Local?.ButtonNameNumb ?? "Numbered chip (Show / Black)"),
                // show = false -> icônes sans numéro (fichiers ajoutés : tool_numb_fillWfalse.png / tool_numb_fillBfalse.png)
                (false, true,  "tool_numb_fillWfalse", Root?.Local?.ButtonNameNumb ?? "Numbered chip (Hide / White)"),
                (false, false, "tool_numb_fillBfalse", Root?.Local?.ButtonNameNumb ?? "Numbered chip (Hide / Black)")
            };

            Button prevBtn = anchor;
            Button last = null;
            for (int i = 0; i < cfg.Length; i++)
            {
                Button b = new Button
                {
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.Transparent,
                    Width = dim1s,
                    Height = dim1s,
                    Visible = true,
                    Name = "btNTag_" + i.ToString(),
                    BackgroundImageLayout = ImageLayout.Stretch,
                    TabStop = false
                };
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;

                // safe load of image (fallback)
                try
                {
                    b.BackgroundImage = getImgFromDiskOrRes(cfg[i].icon, ImageExts);
                }
                catch
                {
                    // fallback : si la variante "false" n'existe pas, utiliser la version normale correspondante
                    try
                    {
                        if (cfg[i].icon.EndsWith("false"))
                        {
                            string fallback = cfg[i].icon.Substring(0, cfg[i].icon.Length - "false".Length);
                            b.BackgroundImage = getImgFromDiskOrRes(fallback, ImageExts);
                        }
                        else
                        {
                            b.BackgroundImage = getImgFromDiskOrRes("tool_numb_fillW", ImageExts);
                        }
                    }
                    catch
                    {
                        try { b.BackgroundImage = getImgFromDiskOrRes("tool_numb_fillW", ImageExts); } catch { }
                    }
                }

                b.Tag = Tuple.Create(cfg[i].show, cfg[i].firstWhite);
                try
                {
                    toolTip.SetToolTip(b, cfg[i].tip + " (" + (i + 1).ToString() + ")");
                }
                catch { }

                // handlers identiques aux autres boutons
                b.Click += NumberTagBtn_Click;
                b.MouseDown += new MouseEventHandler(this.btAllButtons_MouseDown);
                b.MouseUp += new MouseEventHandler(this.btAllButtons_MouseUp);
                b.MouseMove += new MouseEventHandler(this.gpButtons_MouseMove);
                b.ContextMenu = new ContextMenu();
                b.ContextMenu.Popup += new EventHandler(this.btAllButtons_RightClick);

                // add to panel and position horizontally after anchor (garde la ligne principale)
                gpButtons.Controls.Add(b);
                SetButtonPosition(prevBtn, b, dim2s);
                b.BringToFront();
                prevBtn = b;
                last = b;

                // keep references
                switch (i)
                {
                    case 0: btNTag_Show_White = b; break;
                    case 1: btNTag_Show_Black = b; break;
                    case 2: btNTag_Hide_White = b; break;
                    case 3: btNTag_Hide_Black = b; break;
                }
            }

            // Visuel : marquer le bouton actif
            UpdateNumberTagButtonBorders();

            return last;
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

        // Met à jour l'apparence (bordure) des 4 boutons pour montrer la sélection active
        private void UpdateNumberTagButtonBorders()
        {
            foreach (Control c in gpButtons.Controls)
            {
                if (!(c is Button)) continue;
                if (!c.Name.StartsWith("btNTag_")) continue;
                (c as Button).FlatAppearance.BorderSize = 0;
            }

            string matchName;
            if (NumberTag_ShowNumber && NumberTag_FirstIsWhite) matchName = "btNTag_0";
            else if (NumberTag_ShowNumber && !NumberTag_FirstIsWhite) matchName = "btNTag_1";
            else if (!NumberTag_ShowNumber && NumberTag_FirstIsWhite) matchName = "btNTag_2";
            else matchName = "btNTag_3";

            try
            {
                Control c = gpButtons.Controls[matchName];
                if (c is Button)
                    (c as Button).FlatAppearance.BorderSize = 3;
            }
            catch { }
        }
    }
}