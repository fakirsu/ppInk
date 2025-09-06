
# goInk, outils pour commenter des parties de jeu de go
#### goInk est un outil pour faciliter les commentaires de parties de jeu de go. 
goInk est un fork de [ppInk](https://github.com/pubpub-zz/ppInk), lui-même fork de [ggInk](https://github.com/geovens/gInk). Grand merci à eux. 

## Fonctionnalités pour le jeu de go
Ne sont décrits ici que les ajouts spécifiques au jeu de go, les autres fonctionnalités de ppInk sont documentées sur https://github.com/pubpub-zz/ppInk/

Pour les besoins spécifiques du jeu de go, nous avons créé les outils suivants : 
#### - ajout d'une séquence de pierres (aléternées noire/blanc), avec ou sans numéros. 
#### - ajout de zones (blanches ou noires) dessinées à main levées
#### - ajout de lettres (A, B, C, ...) et de symboles (triangle, carré, croix, cercle) sur le goban)

Ces outils sont paramétrables (couleur, opacité, taille, ...) dans le menu option accessible avec un clic droit sur l'icône de la barre des tâches.

<img src="src/images/outils.png" alt="Les outils" width="200"/>
<img src="src/images/zones.png" alt="Les zones" height="150"/>

## Important : définissez les dimensions de votre goban avant d'utiliser les outils
L'utilisateur doit définir la dimension du goban, en utilisant l'outil "set goban" dans la barre d'outils : <img src="src/images/outil_set_goban.png" alt="Outil Set goban" height="50"/>, puis dessiner un rectangle aux dimensions du goban (cliquer sur l'intersection en haut à gauche, maintenir le clic jusqu'à l'intersection en bas à droite, puis relâcher). 

Si le goban n'est pas un 19x19, penser à changer dans le menu options : <img src="src/images/taille_goban.png" alt="Option taille goban" width="200"/>


## Utilisation
Les outils sont accessibles par raccourcis clavier, qui peuvent être appelés depuis un StreamDeck). 
Les raccourcis sont également paramétrables dans le menu options.

## Disclaimer
Ce fork a été exclusivement réalisé avec l'aide d'une IA, car je ne suis pas du tout développeur de code. 

## Problèmes connus
Parfois, certains changements dans le menu Options nécessitent de cliquer sur "Sauver configuration" dans le premier onglet du menu Options, faute de quoi ils ne sont pas enregistrés. 

Parfois, les changements du menu Options ne sont pris en compte qu'après avoir rouvert/refermé la barre d'outils. 

Les outils Lettres, Carré, Cercle, Triangle et Croix n'ont pas de bouton dans la barre d'outils : ils ne peuvent être appelés que par raccourci clavier (je n'ai pas réussi à ajouter les boutons dans la barre d'outils)

Selon l'orientation de la barre d'outils, elle peut être tronquée. 

D'une manière générale, goInk est davantage conçu pour fonctionner avec des raccourcis clavier que via la barre d'outils. 



