using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;

namespace gInk
{
    public partial class FormInput : Form
    {
        Microsoft.Ink.Stroke stroke;
        Root Root;
        string Saved_Txt;
        Font Saved_Font;
        DrawingAttributes Saved_Da;
        bool Saved_Frame;
        bool Saved_White;
        bool Saved_Black;

        //public FormInput(string caption, string label, string txt, bool ML, gInk.Root rt = null, Microsoft.Ink.Stroke stk = null)
        //{
        //    InitializeComponent();

        //    // Rendre la fenêtre transparente
        //    this.BackColor = Color.FromArgb(1, 1, 1); // Couleur proche de noir mais pas exactement
        //    this.TransparencyKey = this.BackColor;
        //    this.FormBorderStyle = FormBorderStyle.None;

        //    // local
        //    this.btOK.Visible = false; // Cacher les boutons puisqu'on utilisera ENTRÉE
        //    this.btCancel.Visible = false;
        //    this.FontBtn.Visible = false;
        //    this.ColorBtn.Visible = false;
        //    this.boxingCb.Visible = false;
        //    this.captionLbl.Visible = false;

        //    Text = caption;
        //    if (ML)
        //    {
        //        InputML.Visible = true;
        //        InputML.BackColor = Color.FromArgb(10, 10, 10);
        //        InputML.ForeColor = Color.White;
        //        txt = txt.Replace("\r\n", "\n").Replace("\n", "\r\n");
        //        InputML.Text = txt;
        //        ActiveControl = InputML;
        //    }
        //    else
        //    {
        //        InputSL.Visible = true;
        //        InputSL.BackColor = Color.FromArgb(10, 10, 10);
        //        InputSL.ForeColor = Color.White;
        //        InputSL.Text = txt;
        //        ActiveControl = InputSL;
        //    }

        //    Root = rt;
        //    stroke = stk;
        //    if (stroke != null)
        //    {
        //        Saved_Txt = ML ? InputML.Text : InputSL.Text;
        //        Saved_Da = stk.DrawingAttributes.Clone();
        //        if (stk.ExtendedProperties.Contains(Root.TEXTFONT_GUID))
        //        {
        //            Saved_Font = new Font(
        //                (string)stk.ExtendedProperties[Root.TEXTFONT_GUID].Data,
        //                (float)(double)stk.ExtendedProperties[Root.TEXTFONTSIZE_GUID].Data,
        //                (System.Drawing.FontStyle)stk.ExtendedProperties[Root.TEXTFONTSTYLE_GUID].Data);
        //        }
        //        Saved_Frame = stk.ExtendedProperties.Contains(Root.ISSTROKE_GUID);
        //        Saved_White = stk.ExtendedProperties.Contains(Root.ISFILLEDWHITE_GUID);
        //        Saved_Black = stk.ExtendedProperties.Contains(Root.ISFILLEDBLACK_GUID);

        //        if (ML)
        //        {
        //            InputML.TextChanged += new System.EventHandler(this.InputML_TextChanged);
        //            InputML.SelectAll();
        //        }
        //        else
        //        {
        //            InputSL.TextChanged += new System.EventHandler(this.InputML_TextChanged);
        //            InputSL.SelectAll();
        //        }
        //    }

        //    // Ajuster la taille en fonction du contrôle visible
        //    if (ML)
        //    {
        //        this.Width = InputML.Width + 10;
        //        this.Height = InputML.Height + 10;
        //        InputML.Left = 5;
        //        InputML.Top = 5;
        //    }
        //    else
        //    {
        //        this.Width = InputSL.Width + 10;
        //        this.Height = InputSL.Height + 10;
        //        InputSL.Left = 5;
        //        InputSL.Top = 5;
        //    }
        //}

        public FormInput(string caption, string label, string txt, bool ML, gInk.Root rt = null, Microsoft.Ink.Stroke stk = null)
        {
            InitializeComponent();

            // Rendre la fenêtre transparente
            this.BackColor = Color.FromArgb(1, 1, 1); // Couleur proche de noir mais pas exactement
            this.TransparencyKey = this.BackColor;
            this.FormBorderStyle = FormBorderStyle.None;

            // local
            this.btOK.Visible = false; // Cacher les boutons puisqu'on utilisera ENTRÉE
            this.btCancel.Visible = false;
            this.FontBtn.Visible = false;
            this.ColorBtn.Visible = false;
            this.boxingCb.Visible = false;
            this.captionLbl.Visible = false;

            Text = caption;
            if (ML)
            {
                InputML.Visible = true;
                InputML.BackColor = Color.FromArgb(10, 10, 10);
                InputML.ForeColor = Color.White;
                if (stk == null)
                {
                    // nouvelle saisie : champ vide
                    InputML.Text = "";
                }
                else
                {
                    // édition d'une stroke existante : conserver le texte fourni
                    txt = txt.Replace("\r\n", "\n").Replace("\n", "\r\n");
                    InputML.Text = txt;
                }
                ActiveControl = InputML;
            }
            else
            {
                InputSL.Visible = true;
                InputSL.BackColor = Color.FromArgb(10, 10, 10);
                InputSL.ForeColor = Color.White;
                InputSL.Text = (stk == null) ? "" : txt;
                ActiveControl = InputSL;
            }

            Root = rt;
            stroke = stk;
            if (stroke != null)
            {
                Saved_Txt = ML ? InputML.Text : InputSL.Text;
                Saved_Da = stk.DrawingAttributes.Clone();
                if (stk.ExtendedProperties.Contains(Root.TEXTFONT_GUID))
                {
                    Saved_Font = new Font(
                        (string)stk.ExtendedProperties[Root.TEXTFONT_GUID].Data,
                        (float)(double)stk.ExtendedProperties[Root.TEXTFONTSIZE_GUID].Data,
                        (System.Drawing.FontStyle)stk.ExtendedProperties[Root.TEXTFONTSTYLE_GUID].Data);
                }
                Saved_Frame = stk.ExtendedProperties.Contains(Root.ISSTROKE_GUID);
                Saved_White = stk.ExtendedProperties.Contains(Root.ISFILLEDWHITE_GUID);
                Saved_Black = stk.ExtendedProperties.Contains(Root.ISFILLEDBLACK_GUID);

                if (ML)
                {
                    InputML.TextChanged += new System.EventHandler(this.InputML_TextChanged);
                    InputML.SelectAll();
                }
                else
                {
                    InputSL.TextChanged += new System.EventHandler(this.InputML_TextChanged);
                    InputSL.SelectAll();
                }
            }

            // Ajuster la taille en fonction du contrôle visible
            if (ML)
            {
                this.Width = InputML.Width + 10;
                this.Height = InputML.Height + 10;
                InputML.Left = 5;
                InputML.Top = 5;
            }
            else
            {
                this.Width = InputSL.Width + 10;
                this.Height = InputSL.Height + 10;
                InputSL.Left = 5;
                InputSL.Top = 5;
            }
        }


        public void TextIn(string txt)
        {
            if (InputML.Visible)
                InputML.Text = txt;
            else
                InputSL.Text = txt;
        }

        public string TextOut()
        {
            if (InputML.Visible)
                return InputML.Text;
            else
                return InputSL.Text;
        }

        // Ces méthodes doivent être conservées car elles sont référencées dans le Designer
        private void FontBtn_Click(object sender, EventArgs e)
        {
            // Méthode conservée mais vide car le bouton est caché
        }

        private void ColorBtn_Click(object sender, EventArgs e)
        {
            // Méthode conservée mais vide car le bouton est caché
        }

        private void boxingCb_TextChanged(object sender, EventArgs e)
        {
            // Méthode conservée mais vide car le combobox est caché
        }

        private void InputML_TextChanged(object sender, EventArgs e)
        {
            if (stroke == null) return;

            string t = ((TextBox)sender).Text;
            if (t.Length == 0) t = " ";
            stroke.ExtendedProperties.Remove(Root.TEXT_GUID);
            stroke.ExtendedProperties.Add(Root.TEXT_GUID, t);
            if (!stroke.ExtendedProperties.Contains(Root.ISTAG_GUID))
                Root.FormCollection.ComputeTextBoxSize(ref stroke);
            Root.FormDisplay.ClearCanvus();
            Root.FormDisplay.DrawStrokes();
            Root.FormDisplay.UpdateFormDisplay(true);
        }

        private void TB_CtrlAPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(1))
            {
                (sender as TextBox).SelectAll();
                e.Handled = true;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            if (stroke != null)
            {
                InputML.Text = Saved_Txt;
                if (Saved_Font != null)
                {
                    stroke.ExtendedProperties.Add(Root.TEXTFONT_GUID, Saved_Font.Name);
                    stroke.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, (double)Saved_Font.Size);
                    stroke.ExtendedProperties.Add(Root.TEXTFONTSTYLE_GUID, Saved_Font.Style);
                }
                Root.FormCollection.ComputeTextBoxSize(ref stroke);
                stroke.DrawingAttributes = Saved_Da;
                if (Saved_Frame)
                    stroke.ExtendedProperties.Add(Root.ISSTROKE_GUID, true);
                else
                    try { stroke.ExtendedProperties.Remove(Root.ISSTROKE_GUID); } catch { }

                if (Saved_Black)
                {
                    try { stroke.ExtendedProperties.Remove(Root.ISFILLEDWHITE_GUID); } catch { }
                    stroke.ExtendedProperties.Add(Root.ISFILLEDBLACK_GUID, true);
                }
                else if (Saved_White)
                {
                    try { stroke.ExtendedProperties.Remove(Root.ISFILLEDBLACK_GUID); } catch { }
                    stroke.ExtendedProperties.Add(Root.ISFILLEDWHITE_GUID, true);
                }
                else
                {
                    try { stroke.ExtendedProperties.Remove(Root.ISFILLEDBLACK_GUID); } catch { }
                    try { stroke.ExtendedProperties.Remove(Root.ISFILLEDWHITE_GUID); } catch { }
                }
            }
        }

        //// Modifier pour permettre la validation avec ENTRÉE simple (sans CTRL)
        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    if (keyData == Keys.Enter || keyData == Keys.Return)
        //    {
        //        // Pour TextBox multiline, on autorise les sauts de ligne sauf avec Shift
        //        if (InputML.Visible && InputML.Multiline && !ModifierKeys.HasFlag(Keys.Shift))
        //            return false;

        //        this.DialogResult = DialogResult.OK;
        //        this.Close();
        //        return true;
        //    }
        //    return base.ProcessCmdKey(ref msg, keyData);
        //}

        //// Garder l'ancienne méthode pour assurer la compatibilité
        //private void FormInput_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        //{
        //    if (e.Control && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return))
        //    {
        //        this.DialogResult = DialogResult.OK;
        //        this.Close();
        //    }
        //    else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
        //    {
        //        // Si c'est un TextBox multiline, ne pas intercepter ENTRÉE simple
        //        // pour permettre les sauts de ligne, sauf si la touche Shift est enfoncée
        //        if (sender is TextBox textBox && textBox.Multiline && !e.Shift)
        //        {
        //            return;
        //        }
        //        this.DialogResult = DialogResult.OK;
        //        this.Close();
        //    }
        //}

        // Modifier pour permettre la validation avec ENTRÉE simple (sans CTRL)
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter || keyData == Keys.Return)
            {
                // Si multiline et Shift enfoncé => insérer un saut de ligne
                if (InputML.Visible && InputML.Multiline && ModifierKeys.HasFlag(Keys.Shift))
                {
                    try
                    {
                        int sel = InputML.SelectionStart;
                        InputML.Text = InputML.Text.Insert(sel, Environment.NewLine);
                        InputML.SelectionStart = sel + Environment.NewLine.Length;
                    }
                    catch { }
                    return true;
                }

                // Valider la saisie sur ENTRÉE
                this.DialogResult = DialogResult.OK;
                this.Close();
                return true;
            }

            if (keyData == Keys.Escape)
            {
                // Annuler la saisie et déselectionner l'outil texte
                try { btCancel_Click(null, null); } catch { }
                try { this.DialogResult = DialogResult.Cancel; } catch { }
                try { this.Close(); } catch { }
                try { Root?.FormCollection?.SelectTool(Tools.Hand); } catch { }
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Garder l'ancienne méthode PreviewKeyDown mais la mettre en phase avec ProcessCmdKey (gestions rapides)
        private void FormInput_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                // Si multiline et SHIFT => insérer saut de ligne
                if (sender is TextBox textBox && textBox.Multiline && e.Shift)
                {
                    int sel = textBox.SelectionStart;
                    textBox.Text = textBox.Text.Insert(sel, Environment.NewLine);
                    textBox.SelectionStart = sel + Environment.NewLine.Length;
                    return;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                try { btCancel_Click(null, null); } catch { }
                try { this.DialogResult = DialogResult.Cancel; } catch { }
                try { this.Close(); } catch { }
                try { Root?.FormCollection?.SelectTool(Tools.Hand); } catch { }
            }
        }


    }
}