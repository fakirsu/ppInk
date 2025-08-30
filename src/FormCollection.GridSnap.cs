using System;
using System.Drawing;

namespace gInk
{
    // partial FormCollection : stockage et helpers pour la grille définie par l'utilisateur
    public partial class FormCollection
    {
        // zone définie par l'utilisateur (pixels écran)
        public Rectangle GridRect = Rectangle.Empty;
        public bool GridRectDefined = false;

        /// <summary>
        /// Définit la zone de la grille à partir d'un rectangle en pixels (écran / client).
        /// Appelle Normalize pour garantir des dimensions positives.
        /// </summary>
        public void SetGridFromRectangle(Rectangle r)
        {
            // normaliser : width/height positifs et position top-left correcte
            if (r.Width < 0)
            {
                r.X += r.Width;
                r.Width = -r.Width;
            }
            if (r.Height < 0)
            {
                r.Y += r.Height;
                r.Height = -r.Height;
            }

            // seuil minimum pour éviter captures accidentelles
            if (r.Width < 10 || r.Height < 10)
            {
                // trop petit -> ignorer
                return;
            }

            GridRect = r;
            GridRectDefined = true;


            // Propager dans Root pour persistance
            try
            {
                if (this.Root != null)
                {
                    this.Root.GridRect = GridRect;
                    this.Root.GridRectDefined = true;
                    // Sauvegarde silencieuse : écrire dans le config principal (config.ini)
                    // utilise le même fichier que ReadOptions a lu
                    try
                    {
                        this.Root.SaveOptions(Program.RunningFolder + "config.ini");
                    }
                    catch
                    {
                        // Ne pas bloquer l'UI si l'écriture échoue
                    }
                }
            }
            catch { }



            // forcer redraw si l'affichage est visible
            if (this.Root?.FormDisplay != null)
            {
                this.Root.UponAllDrawingUpdate = true;
            }
        }

        /// <summary>
        /// Supprime la définition utilisateur de la grille (retour au comportement par défaut).
        /// </summary>
        public void ClearGridDefinition()
        {
            GridRect = Rectangle.Empty;
            GridRectDefined = false;
            if (this.Root?.FormDisplay != null)
                this.Root.UponAllDrawingUpdate = true;
        }
    }
}