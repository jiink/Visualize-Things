# Visualize Things

Mixed reality 3D model viewer for the Meta Quest 3 family.

Uses Quest 3 specific features to make 3D models blend in realistically with the environment.

You can use this with the [Visualize Things Companion app for PC](https://github.com/jiink/Visualize-Things-Companion) to drag-and-drop 3D models from your computer to see them in this app.

Made in Unity.

This uses the paid [TriLib 2 asset](https://assetstore.unity.com/packages/tools/modeling/trilib-2-model-loading-package-157548?srsltid=AfmBOoqjkptm8kh3QdliqEHWGOn1LThKZJmY-kS-YAhbv9NIVoi-nUiM). You'll need to purchase that and import it into the project yourself for this to work in the Unity Editor.

By the way the working title / codename was "Realivation"; that name hasn't been fully replaced in the code entirely yet.

## Known issues

- On initial use of the "Set Reflections" option in the radial menu, the app requests camera permissions. After granting permission, you must select "Set Reflections" a second time for the feature to work.
- If you connect to the Companion app with the QR code and then disconnect, you won't be able to connect to the same computer again until you restart the app. This has to do with QR code "trackables", not the networking itself.
- Error handling and communication is poor. Things will be printed to the debug log, but not shown to the user. So if something wrong happens when loading a 3D model, connecting to the Companion app, etc., nothing will happen and the user will have no information to go off of. A good way to show errors to the user needs to be figured out.
