# 🚀 VR-Unity-HandRockets

An implementation of the Hand-Rockets locomotion technique.

## Summary

The hand-rocket locomotion technique, featured in [Iron Man VR](https://www.marvel.com/games/marvel-s-iron-man-vr), [Rocket Skates VR](https://store.steampowered.com/app/1535200/Rocket_Skates_VR/), and in a slightly different form in [Megaton Rainfall](https://store.steampowered.com/app/430210/Megaton_Rainfall/) is a steering metaphor. It stimulates the feeling of having a rocket strapped to your arm and flying through the air. Based on the orientation of the user's hands, the direction he/she is traveling changes. See it [here](https://www.youtube.com/watch?v=P0ghZf6oTco) in action.

## Test It on Your Own

This is a Unity 2020.3.1 project. Feel free to test it on your own machine. Should be HMD independent, as the input is polled via the XR Interaction Toolkit and the Input System. Simply download the repository and add it to Unity. The editor should download the dependencies and then you should be good to go.

You can test the 3 levels on your own and provide back feedback. Complete each checkpoint as fast as you can. Once you reach the last checkpoint the level should automatically end. You can recognize that a checkpoint is complete by the changing color of the checkpoint smoke.

You can share your experience by completing the following questionnaire: [https://forms.gle/rPZBhEv5ztcdYACS9](https://forms.gle/rPZBhEv5ztcdYACS9)

## Project Content

The project includes 3 scenes:

- large terrain - designed to test motion sickness for traversing large distances.
- platform level - designed to test the vertical navigation capabilities
- ring circuit - designed to test the maneuverability of the technique

Besides this, the project contains the script for the Hand-Rocket [Locomotion Provider](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@1.0/api/UnityEngine.XR.Interaction.Toolkit.LocomotionProvider.html).

Other assets include the hand models with propulsion effects, checkpoint system, and checkpoint VFX.

