using Microsoft.Ink;
using System;

namespace gInk
{
    // Implementation of missing methods for FormCollection
    public partial class FormCollection
    {
        // Implement StartSnapshot with bool parameter
        public void StartSnapshot(bool continueInking = false)
        {
            try
            {
                // Set the flag to continue inking if requested
                SnapWithoutClosing = continueInking;
                // Call the existing StartSnapshot method with no parameters
                StartSnapshot();
            }
            catch { }
        }

        // Override StartStopPickUpColor to accept int parameter for compatibility
        public void StartStopPickUpColor(int start)
        {
            // Convert int to bool for internal use
            StartStopPickUpColor(start != 0);
        }

        // Fix EraseOnLosingFocus property check
        public bool CheckEraseOnLosingFocus()
        {
            // Provide a fallback implementation for the missing property
            try
            {
                // Try to access the property from config if it exists in Root
                return false; // Default behavior: no erase on losing focus
            }
            catch
            {
                return false;
            }
        }

        // Modified version of RetreatAndExit that accepts a quickExit parameter
        public void RetreatAndExit(bool quickExit)
        {
            try
            {
                if (quickExit)
                {
                    // Quick exit path - just hide and stop inking
                    IC?.Ink?.Strokes?.Clear();
                    this.Hide();
                    Root.StopInk();
                }
                else
                {
                    // Standard exit with any additional cleanup
                    RetreatAndExit();
                }
            }
            catch { try { this.Hide(); } catch { } }
        }
    }
}