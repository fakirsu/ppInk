using System;
using System.Drawing;
using System.Windows.Forms;

namespace gInk
{
    /// <summary>
    /// Helper statique pour calculer le point d'intersection le plus proche
    /// d'une grille définie dans le quart supérieur gauche de l'écran.
    /// </summary>
    public static class GridSnap
    {
        /// <summary>
        /// Retourne le point d'intersection de la grille (rows x cols) le plus proche de (x,y).
        /// Par défaut la grille couvre le quart supérieur gauche de l'écran principal.
        /// </summary>
        /// <param name="root">instance Root (non utilisée ici mais pratique pour extensions futures)</param>
        /// <param name="x">coordonnée X d'origine (clic)</param>
        /// <param name="y">coordonnée Y d'origine (clic)</param>
        /// <param name="rows">nombre de lignes d'intersections verticales (default 19)</param>
        /// <param name="cols">nombre de colonnes d'intersections horizontales (default 19)</param>
        /// <returns>Point où placer la pastille</returns>
        public static Point SnapNumberTagPoint(Root root, int x, int y, int rows = 19, int cols = 19)
        {
            if (rows < 2 || cols < 2)
                return new Point(x, y);

            // Région : quart supérieur gauche de l'écran principal
            Rectangle screen = Screen.PrimaryScreen.Bounds;
            int gridLeft = screen.Left;
            int gridTop = screen.Top;
            int gridWidth = Math.Max(1, screen.Width / 2);
            int gridHeight = Math.Max(1, screen.Height / 2);

            // Pas entre intersections (on divise par (count - 1) pour inclure les bords)
            double stepX = (double)gridWidth / (cols - 1);
            double stepY = (double)gridHeight / (rows - 1);

            // Si le clic est hors de la zone de la grille, on le laisse quand même "snapper"
            // vers l'intersection la plus proche (possible extension : clamp à la zone).
            double bestDistSq = double.MaxValue;
            Point best = new Point(x, y);

            for (int j = 0; j < rows; j++)
            {
                int py = gridTop + (int)Math.Round(j * stepY);
                for (int i = 0; i < cols; i++)
                {
                    int px = gridLeft + (int)Math.Round(i * stepX);
                    double dx = px - x;
                    double dy = py - y;
                    double d2 = dx * dx + dy * dy;
                    if (d2 < bestDistSq)
                    {
                        bestDistSq = d2;
                        best = new Point(px, py);
                    }
                }
            }

            return best;
        }
    }
}