using System;
using System.Drawing;
using System.Windows.Forms;

namespace gInk
{
    /// <summary>
    /// Helper statique pour calculer le point d'intersection le plus proche
    /// d'une grille définie.
    /// </summary>
    public static class GridSnap
    {
        /// <summary>
        /// Retourne le point d'intersection de la grille (rows x cols) le plus proche de (x,y).
        /// Si root.FormCollection.GridRectDefined == true, la grille couvre cette zone.
        /// Sinon on tombe sur le comportement par défaut (quart supérieur gauche).
        /// </summary>
        public static Point SnapNumberTagPoint(Root root, int x, int y, int rows = 19, int cols = 19)
        {
            if (rows < 2 || cols < 2)
                return new Point(x, y);

            // Si une grille personnalisée a été définie, l'utiliser.
            if (root?.FormCollection != null && root.FormCollection.GridRectDefined)
            {
                Rectangle gridRect = root.FormCollection.GridRect;
                if (gridRect.Width <= 0 || gridRect.Height <= 0)
                    return new Point(x, y);

                double stepX = (double)gridRect.Width / (cols - 1);
                double stepY = (double)gridRect.Height / (rows - 1);

                double bestDistSq = double.MaxValue;
                Point best = new Point(x, y);

                for (int j = 0; j < rows; j++)
                {
                    int py = gridRect.Top + (int)Math.Round(j * stepY);
                    for (int i = 0; i < cols; i++)
                    {
                        int px = gridRect.Left + (int)Math.Round(i * stepX);
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

            // comportement d'origine : quart supérieur gauche de l'écran principal
            Rectangle screen = Screen.PrimaryScreen.Bounds;
            int gridLeft = screen.Left;
            int gridTop = screen.Top;
            int gridWidth = Math.Max(1, screen.Width / 2);
            int gridHeight = Math.Max(1, screen.Height / 2);

            double stepX2 = (double)gridWidth / (cols - 1);
            double stepY2 = (double)gridHeight / (rows - 1);

            double bestDistSq2 = double.MaxValue;
            Point best2 = new Point(x, y);

            for (int j = 0; j < rows; j++)
            {
                int py = gridTop + (int)Math.Round(j * stepY2);
                for (int i = 0; i < cols; i++)
                {
                    int px = gridLeft + (int)Math.Round(i * stepX2);
                    double dx = px - x;
                    double dy = py - y;
                    double d2 = dx * dx + dy * dy;
                    if (d2 < bestDistSq2)
                    {
                        bestDistSq2 = d2;
                        best2 = new Point(px, py);
                    }
                }
            }

            return best2;
        }
    }
}