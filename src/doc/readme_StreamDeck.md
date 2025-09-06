# goInk et StreamDeck

Il est possible d'utiliser goInk avec des raccourcis clavier, mais également avec [Rest API](https://github.com/fakirsu/ppInk?tab=readme-ov-file#rest-api). Les deux solutions sont possibles avec un StreamDeck. A ma connaissance, il y a très peu de différences en termes de performances, consommation de ressource, fiabilité, ... 

Pour ma part j'utilise Rest API, car cela évite des conflits potentiels de raccourcis clavier avec d'autres applications.

Dans StreamlDeck, il suffit d'appeler une adresse URL précise : 

<img src="../images/rest_api.png" alt="Appel dans StreamDeck" width="500"/>

## Pour les outils spécifiques au jeu de go

- /NTag_Show_White  
  Description (fr.txt) : Pierres numérotées (première pierre blanche)  
  Exemple : curl "http://localhost:1234/NTag_Show_White"  
  StreamDeck : HTTP GET -> URL = http://localhost:1234/NTag_Show_White

- /NTag_Show_Black  
  Description (fr.txt) : Pierres numérotées (première pierre noire)  
  Exemple : curl "http://localhost:1234/NTag_Show_Black"

- /NTag_Hide_White  
  Description (fr.txt) : (Ajout de pierres) sans numéros (1ère blanche)  
  Exemple : curl "http://localhost:1234/NTag_Hide_White"

- /NTag_Hide_Black  
  Description (fr.txt) : (Ajout de pierres) sans numéros (1ère noire)  
  Exemple : curl "http://localhost:1234/NTag_Hide_Black"

- /HandFilledWhite  
  Description (fr.txt) : Ajout d'une zone blanche  
  Exemple : curl "http://localhost:1234/HandFilledWhite"

- /HandFilledBlack  
  Description (fr.txt) : Ajout d'une zone noire  
  Exemple : curl "http://localhost:1234/HandFilledBlack"

- /LetterTag  
  Description (fr.txt) : Ajout de lettres (A, B, C, ...)  
  Exemple : curl "http://localhost:1234/LetterTag"

- /SquareTag  
  Description (fr.txt) : Ajout de carrés  
  Exemple : curl "http://localhost:1234/SquareTag"

- /TriangleTag  
  Description (fr.txt) : Ajout de triangles  
  Exemple : curl "http://localhost:1234/TriangleTag"

- /CircleTag  
  Description (fr.txt) : Ajouts de cercles  
  Exemple : curl "http://localhost:1234/CircleTag"

- /CrossTag  
  Description (fr.txt) : Ajout de croix  
  Exemple : curl "http://localhost:1234/CrossTag"

Notes :
- Pour les /NTag_* vous pouvez ajouter ?C=true pour forcer le nettoyage des tracés existants avant application (paramètre C repris dans l'implémentation REST).
- Ces descriptions proviennent des clés de langue dans ppInk/lang/fr.txt (OptionsTagOpacityPerc, OptionsGoStrokeWidth, etc.) et servent à documenter l'usage / but des endpoints.

---

## Liste exhaustive des autres endpoints REST disponibles (résumé fonctionnel)
Tous les endpoints acceptent GET ; la plupart exigent que l'application soit en mode inking (sinon réponse 409).

- /Inking
  - params : S=true|false
  - action : démarrer / arrêter le mode inking
  - réponse : { "Started": true|false }

- /PenDef
  - params : P (pen -1..9), R,G,B (0-255), T (transp 0-255), W (width float), F (fading true|false|float), L (line style), Browse
  - action : définir propriétés du stylo
  - réponse : JSON avec état du stylo

- /ToggleFading
  - params : P (pen -1..9)
  - action : basculer fading pour le stylo

- /NextLineStyle
  - params : P (pen -1..9)
  - action : changer style de ligne du stylo

- /CurrentPen
  - params : P (0..9)
  - action : sélectionner stylo courant
  - réponse : { "Pen": n }

- /CurrentTool
  - params : T (tool id), optional F (filling), A (arrow), W,H (size), D (distance), I (image path)
  - action : sélectionner outil (ex. ClipArt, PatternLine, texte, formes)
  - réponse : JSON décrivant l'outil courant

- /EnlargePen
  - params : D (int delta)
  - action : modifier largeur stylo
  - réponse : { "Width": <valeur> }

- /SetTagNumber
  - params : V (string)
  - action : définir numéro de tag

- /Magnet
  - params : M=true|false
  - action : activer/désactiver magnétisme

- /VisibleInk
  - params : V=true|false
  - action : montrer/cacher l'encre

- /Fold
  - params : F=true|false
  - action : dock/undock la barre
  - réponse : { "Folded": true|false }

- /LoadStrokes
  - params : F = filename ou "-" (UI)
  - action : charger tracés
  - réponse : { "OK": true } ou 500

- /SaveStrokes
  - params : F = filename ou "-" (UI)
  - action : sauvegarder tracés
  - réponse : { "OK": true }

- /ClearScreen
  - params : B = tr|wh|cu|bk|me
  - action : effacer l'écran (choix du fond)

- /Resize
  - params : K (scale float)
  - action : redimensionner sélection (doit y avoir sélection)

- /Rotate
  - params : A (angle float)
  - action : tourner sélection

- /Magnify
  - params : Z = no|dyn|capt|spot
  - action : gérer la loupe / spot / capture

- /GetSelection
  - flags : presence de C et/ou L
  - action : retourner info sur sélection / hovered
  - réponse : JSON Type=Selection|Hovered, Count, TotalLength

- /Snapshot
  - params : A = out|end|cont
  - action : lancer capture (option close on snap)

- /ArrowCursor
  - params : A=true|false
  - action : forcer curseur alternative (alt-like)

- /PickupColor
  - params : P=true|false
  - action : démarrer/appliquer/cancel pipette couleur
  - réponse : { "PickupMode": ... , "Red":..., ... }

- /ChangePage
  - params : N = -1|0|1 (ou absent)
  - action : page prev/next / obtenir num page
  - réponse : { "PageNumer": n, "TotalPages": m }

- autres chemins non implémentés -> 404

Remarques générales
- Remplacer http://localhost:7799 par l'URL de base configurée dans votre __APIRestUrl__ (voir options ou APIRest.GetAddress()).
- StreamDeck : créez une action HTTP GET pointant sur l'URL correspondante. Pour indiquer un effet visuel sur le bouton (état), vous pouvez appeler périodiquement /CurrentTool ou /Inking et parser le JSON renvoyé.
- Beaucoup d’actions ne fonctionneront pas si l'application n'est pas en mode inking : la réponse sera 409 "Not in Inking mode".
- Pour /NTag_* : ajoutez ?C=true si vous voulez forcer l'effacement des tracés existants (comme le comportement "clear" optionnel).

---
