# goInk et StreamDeck

## Sommaire
- [English version](readme_StreamDeck_EN.md)
- [Pour les outils spécifiques au jeu de go](#pour-les-outils-spécifiques-au-jeu-de-go)
- [Tous les autres endpoints](#liste-des-autres-endpoints-rest-disponibles-résumé-fonctionnel-du-fichier-apirestcs)]
- [Prédéfinir vos gobans](#prédéfinir-vos-gobans)

Il est possible d'utiliser goInk avec des raccourcis clavier, mais également avec [Rest API](https://github.com/fakirsu/ppInk?tab=readme-ov-file#rest-api). Les deux solutions sont possibles avec un StreamDeck. A ma connaissance, il y a très peu de différences en termes de performances, consommation de ressource, fiabilité, ... 

Pour ma part j'utilise Rest API, car cela évite des conflits potentiels de raccourcis clavier avec d'autres applications.

Dans StreamlDeck, il suffit d'appeler une adresse URL précise : 

<img src="../images/rest_api.png" alt="Appel dans StreamDeck" width="500"/>

## Pour les outils spécifiques au jeu de go

- /RectTool  
  Description : Outil "Set goban" pour définir les dimensions du goban  
  "http://localhost:1234/RectTool"

- /NTag_Show_White  
  Description : Pierres numérotées (première pierre blanche)  
  "http://localhost:1234/NTag_Show_White"  

- /NTag_Show_Black  
  Description : Pierres numérotées (première pierre noire)  
  "http://localhost:1234/NTag_Show_Black"

- /NTag_Hide_White  
  Description : Ajout de pierres sans numéros (1ère blanche)  
  "http://localhost:1234/NTag_Hide_White"

- /NTag_Hide_Black  
  Description : Ajout de pierres sans numéros (1ère noire)  
  "http://localhost:1234/NTag_Hide_Black"

- /HandFilledWhite  
  Description : Ajout d'une zone blanche  
  "http://localhost:1234/HandFilledWhite"

- /HandFilledBlack  
  Description : Ajout d'une zone noire  
  "http://localhost:1234/HandFilledBlack"

- /HandFilledBlack  
  Description : Ajout d'une flèche 
  "http://localhost:1234/AddArrow"

- /LetterTag  
  Description : Ajout de lettres (A, B, C, ...)  
  "http://localhost:1234/LetterTag"

- /SquareTag  
  Description : Ajout de carrés  
  "http://localhost:1234/SquareTag"

- /TriangleTag  
  Description : Ajout de triangles  
  "http://localhost:1234/TriangleTag"

- /CircleTag  
  Description : Ajouts de cercles  
  "http://localhost:1234/CircleTag"

- /CrossTag  
  Description : Ajout de croix  
  "http://localhost:1234/CrossTag"

Remarques générales
- Remplacer http://localhost:1234 par l'URL de base configurée dans votre menu Options (onglet Général)
- Beaucoup d’actions ne fonctionneront pas si l'application n'est pas en mode inking (barre d'outils ouverte) : la réponse sera 409 "Not in Inking mode".


---

## Liste des autres endpoints REST disponibles (résumé fonctionnel du fichier APIRest.cs)
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



## Prédéfinir vos gobans
Si vous utilisez plusieurs goban de tailles différentes (OGS, KGS, FOX, ...), vous pouvez utiliser le StreamDeck pour "préenregistrer" les dimensions de chacun d'eux.

Je vous conseille l'utilisation de l'extension [BarRaider SuperMacro](https://marketplace.elgato.com/product/supermacro-62195fec-7bcb-403d-b650-c342e9dfec67)

Vous pourrez ensuite créer une multi-action dans StreamDeck, avec les actions suivantes :
  - Ouvrir la barre d'outils goInk (si pas déjà ouvert)
  - Attendre 500 ms (pour laisser le temps à goInk de s'ouvrir)
  - Appeler l'endpoint "http://localhost:1234/RectTool" pour activer l'outil "Set goban"
  - Attendre 200 ms
  - Fonction BarRaider avec le code suivant dans Short-Press-Macro :
	<pre><code>{{MSAVEPOS}}
	{{MOUSEPOS:30000,15000}}
	{{MLEFTDOWN}}
	{{PAUSE:50}}
	{{MOUSEPOS:50000,45000}}
	{{PAUSE:50}}
	{{MLEFTUP}}
	{{MLOADPOS}} </code></pre>
	
Les coordonnées (x,y) dans {{MOUSEPOS:x,y}} sont à adapter en fonction de la position de votre goban sur l'écran. Vous pouvez utiliser l'outil Windows "Capture d'écran et croquis" pour obtenir les coordonnées.
x et y doivent être entre 0 et 65535 (le point 65535,65535 est tout en bas à droite de votre écran)
Vous poouvez utiliser des outils tiers pour déterminer les coordonnées x,y de votre souris (par exemple VoiceAttack).









---
