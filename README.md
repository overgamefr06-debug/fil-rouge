#  VR Weather Plant (Fil Rouge Ortega UQAC)

Une expérience immersive en **Réalité Virtuelle (VR)** développée sur Unity. Le joueur incarne le « maître de la météo » et peut contrôler le climat autour de lui grâce à une interface holographique pour observer et interagir avec l'écosystème local. 

Au centre de l'expérience se trouve une **Plante Virtuelle** qui réagit visuellement, physiquement et auditivement aux changements de meteo.

---

##  Fonctionnalités Principales

*   ** Interface Utilisateur Holographique (UI World-Space)**
    *   Un panneau de contrôle virtuel interactif via Raycast Laser depuis les manettes VR.
    *   Affichage en temps réel de statistiques météorologiques dynamiques (Humidité, Températures).
    *   5 boutons pour invoquer instantanément différentes atmosphères.
*   **Plante Réactive & Interactive (XR Grab & Hover)**
    *   Saisie à la main (Grab) ou interaction de proximité (Hover) grâce au module XR Interaction Toolkit.
    *   La plante change de couleur selon le ratio d'humidité/soleil (vert clair joyeux au vert foncé orageux).
    *   *« Wind Sway »* physique : La racine de la plante se plie et lutte contre le vent lors des tempêtes.
*   **Design Sonore 3D Spatialisé**
    *   3 sources audio dispersées dans la scène avec positionnement spatial (Spatial Blend).
    *   Musiques et ambiances dynamiques (`Pluie.mp3`, `storm.mp3`, `sun & cloudy.mp3`) qui basculent avec des fondus enchaînés à chaque changement météo.
*   **Environnement Météo Poussé**
    *   **5 Climats distincts** : Soleilleux, Nuageux, Pluvieux, Orageux et Neigeux.
    *   **Matériaux Dynamiques** : Le sol se transforme automatiquement de l'herbe à la terre boueuse (pluie) ou à la neige éclatante en fonction du temps.
    *   **Skybox Actif** : Les nuages gonflent, s'assombrissent et empêchent mécaniquement la lumière du soleil (`DirectionalLight` shadow strength) de passer selon la météo.
    *   **Éclairs** : Les nuages flashent sous l'impact d'une lumière PointLight invisible cachée dans les cieux durant l'orage.

---

##  Stack Technique

*   **Moteur** : Unity Engine (Configuré avec Universal Render Pipeline - URP) 
*   **Réalité Virtuelle** : OpenXR + XR Interaction Toolkit (XR Rig, Locomotion, Teleportation Area)
*   **Éclairage (Lighting)** : Mixte (Directional, Point et Spot Lights) + Post-Processing (*Bloom, Vignette*)
*   **Langage** : C# (Scripts customs).

---

##  Contrôles (Casque VR)

*   **Déplacement** : Joypad gauche (Locomotion Continue) ou Joypad droit (Point & Teleport sur l'herbe).
*   **Interactions** : Lasers de ciblage (Gâchettes) pour cliquer sur les boutons Météo du panneau géant.
*   **Manipulation** : Bouton Grip (Poignée) pour agripper physiquement le corps 3D de la plante météo.

---

##  Architecture et Hiérarchie Unity

Le projet est organisé proprement dans la scène pour séparer chaque système. Voici l'arbre exact `SampleScene` :

```
SampleScene
├── ENVIRONMENT
│   └── Ground                        (Le sol sur lequel tu marches. Son matériau change selon la météo)
│
├── PLANT
│   └── WeatherPlant                  (La plante interactive)
│       └── PlantModel                (La géométrie 3D de la plante : tige, feuilles, fleur)
│
├── SYSTEMS
│   ├── WeatherSystem                 (L'objet central invisible qui contrôle la logique Météo)
│   └── WeatherEffectsController      (Le contrôleur visuel de la météo, parent des particules)
│       ├── LightningLight            (Lumière d'éclair pour les orages)
│       ├── SnowParticles             (Générateur de neige)
│       ├── RainParticles             (Générateur de pluie classique)
│       └── StormRainParticles        (Générateur de grosse pluie pour l'orage)
│
├── AUDIO
│   └── AmbientSoundManager           (Le gestionnaire des musiques de fond spatialisées)
│       ├── WeatherAudio              (Haut-parleur dynamique pour l'ambiance basique)
│       └── EnvironmentAudio          (Haut-parleur diffusant du vent subtil)
│
├── LIGHTING
│   ├── AmbientPointLight             (Lumière d'ambiance bleue/douce)
│   ├── PlantSpotLight                (Tour du projecteur qui éclaire uniquement la plante)
│   ├── Directional Light             (Le soleil principal du jeu générant les ombres)
│   └── Global Volume                 (L'objet qui applique le Post-Processing comme le "Bloom" ou "Vignette")
│
├── UI
│   └── WeatherInfoUI                 (Le grand écran de contrôle dans le monde 3D)
│       └── InfoPanel                 (Le composant interne contenant tous les Textes Canvas)
│
├── XR Origin (XR Rig)                (Le joueur VR, sa tête et ses deux mains)
│   ├── Camera Offset                 (Ajuste la hauteur du sol réel vers le jeu)
│   └── Locomotion                    (Le système qui permet de se téléporter sur le ground)
│
└── EventSystem                       (Écoute forcée des clics lasers sur les menus Canvas)
```

##  Les 8 Scripts Actifs (C#)

L'expérience est pilotée par seulement 8 scripts optimisés, sans aucun fichier obsolète :

###  1. AmbientSoundManager.cs
* **Attribué à :** [AmbientSoundManager](file:///C:/Users/kevin/Fil%20rouge/Assets/Scripts/AmbientSoundManager.cs#7-216)
* **Rôle :** Responsable du chargement et du contrôle du volume des sons dynamiques en arrière-plan.

###  2. PlantBehavior.cs
* **Attribué à :** `WeatherPlant`
* **Rôle :** Aligné sur le ciel, il modifie le matériau de la `PlantModel` pour qu'elle soit claire (Soleil) ou assombrie (Orage).
* **Particularité :** Implique une animation sinusoïdale de flexion au vent lors d'une tempête. Gère les musiques MP3 en temps réel (`Pluie.mp3`, `storm.mp3`, `sun & cloudy.mp3`).

###  3. VRPlantInteraction.cs
* **Attribué à :** `WeatherPlant`
* **Rôle :** Communique avec l'API XR (XR Interaction Toolkit) pour qu'on puisse saisir la plante avec les mains VR.

###  4. WeatherSystem.cs
* **Attribué à :** [WeatherSystem](file:///C:/Users/kevin/Fil%20rouge/Assets/Scripts/WeatherSystem.cs#8-173)
* **Rôle :** Le « cerveau logistique ». Retient l'état météorologique, diminue la puissance du soleil, et déclenche une boucle automatique asynchrone pour passer au climat suivant toutes les 30-60s.

###  5. WeatherEffectsController.cs
* **Attribué à :** [WeatherEffectsController](file:///C:/Users/kevin/Fil%20rouge/Assets/Scripts/WeatherEffectsController.cs#8-307)
* **Rôle :** Le « décorateur technique ». Allume les particules actives (ex: `RainParticles`), remplace la texture du GameObject `Ground` (Neige/Terre Mouillée), et gère les valeurs LERP pour la force du brouillard et l'épaisseur du FastSky. 

###  6. VRWeatherUI.cs
* **Attribué à :** `WeatherInfoUI` (Canvas)
* **Rôle :** Limite l'activation/désactivation de l'interface graphique du panel InfoPanel à la distance ou l'orientation du joueur (Casque VR).

###  7. WeatherSelectionUI.cs
* **Attribué à :** Aux Boutons du Canvas
* **Rôle :** Relie la main de l'utilisateur avec la Météo. Clic = interdiction temporaire du changement météo automatique et transition immédiate vers l'intempérie requise.

---

