# goInk et StreamDeck

Il est possible d'utiliser goInk avec des raccourcis clavier, mais également avec [Rest API](https://github.com/fakirsu/ppInk?tab=readme-ov-file#rest-api). Les deux solutions sont possibles avec un StreamDeck. A ma connaissance, il y a très peu de différences en termes de performances, consommation de ressource, fiabilité, ... 

Pour ma part j'utilise Rest API, car cela évite des conflits potentiels de raccourcis clavier avec d'autres applications.

Dans StreamlDeck, il suffit d'appeler une adresse URL précise : 
<img src="src/images/rest_api.png" alt="Appel dans StreamDeck" width="500"/>

## Nouveaux endpoints REST (ajoutés)
Les endpoints suivants ont été ajoutés au serveur REST de goInk. Tous utilisent GET et requièrent normalement que l'application soit en mode "inking" (sinon réponse 409).

- /NTag_Show_White
  - Description : sélectionne l'outil "Number tag" avec affichage des numéros et première pierre blanche.
  - Paramètre optionnel : C=true pour forcer le nettoyage des tracés existants avant application.
  - Exemple curl :
    curl "http://localhost:1234/NTag_Show_White"
    curl "http://localhost:1234/NTag_Show_White?C=true"
  - Exemple StreamDeck : Action HTTP GET -> URL = http://localhost:1234/NTag_Show_White

- /NTag_Show_Black
  - Description : "Number tag" avec affichage des numéros et première pierre noire.
  - Exemple : curl "http://localhost:1234/NTag_Show_Black"

- /NTag_Hide_White
  - Description : "Number tag" sans numéros (pastille seule), première pierre blanche.
  - Exemple : curl "http://localhost:1234/NTag_Hide_White"

- /NTag_Hide_Black
  - Description : "Number tag" sans numéros, première pierre noire.
  - Exemple : curl "http://localhost:1234/NTag_Hide_Black"

- /HandFilledWhite
  - Description : active l'outil de zone à main levée remplie en blanc.
  - Exemple : curl "http://localhost:1234/HandFilledWhite"
  - StreamDeck : HTTP GET -> URL = http://localhost:1234/HandFilledWhite

- /HandFilledBlack
  - Description : active l'outil de zone à main levée remplie en noir.
  - Exemple : curl "http://localhost:1234/HandFilledBlack"

- /LetterTag
  - Description : active l'outil Lettres (A, B, C, ...).
  - Exemple : curl "http://localhost:1234/LetterTag"

- /SquareTag
  - Description : active l'outil Carré.
  - Exemple : curl "http://localhost:1234/SquareTag"

- /TriangleTag
  - Description : active l'outil Triangle.
  - Exemple : curl "http://localhost:1234/TriangleTag"

- /CircleTag
  - Description : active l'outil Cercle.
  - Exemple : curl "http://localhost:1234/CircleTag"

- /CrossTag
  - Description : active l'outil Croix.
  - Exemple : curl "http://localhost:1234/CrossTag"

Remarques générales
- Remplacer http://localhost:1234 par l'URL de base configurée dans votre __APIRestUrl__ (voir options ou APIRest.GetAddress()).
- StreamDeck : créez une action HTTP GET pointant sur l'URL correspondante. Pour indiquer un effet visuel sur le bouton (état), vous pouvez appeler périodiquement /CurrentTool ou /Inking et parser le JSON renvoyé.
- Beaucoup d’actions ne fonctionneront pas si l'application n'est pas en mode inking : la réponse sera 409 "Not in Inking mode".
- Pour /NTag_* : ajoutez ?C=true si vous voulez forcer l'effacement des tracés existants (comme le comportement "clear" optionnel).

---
