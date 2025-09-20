using Microsoft.Ink;
using System;
using System.Globalization;

namespace gInk
{
    // Measurement functionality for FormCollection
    public partial class FormCollection
    {
        // Measure multiple strokes, combining selections and hovered stroke
        public string MeasureAllStrokes(Strokes selection, Strokes tempSel, Stroke hovered, bool returnJustValue = false)
        {
            try
            {
                double total = 0.0;
                if (selection != null)
                    foreach (Stroke s in selection) total += StrokeLength(s);
                if (tempSel != null)
                    foreach (Stroke s in tempSel) total += StrokeLength(s);
                if (hovered != null) total += StrokeLength(hovered);
                
                double conv = ConvertMeasureLength(total);
                
                if (returnJustValue)
                    return conv.ToString(MeasureNumberFormat);
                else
                    return string.Format(MeasureNumberFormat, Root.Local.FormatLength, conv, Root.Measure2Unit);
            }
            catch
            {
                return returnJustValue ? "0" : "";
            }
        }
    }
}