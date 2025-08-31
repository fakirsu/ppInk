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
    }
}