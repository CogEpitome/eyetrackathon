Hello!
This is a temporary readme for how to set up eye tracking and the eye recording stuff I made.

Prerequisites:
From the SteamVR SDK, drag the [SteamVR] and [CameraRig] prefabs into the scene.
From the TobiiPro SDK, drag the [VREyeTacker] prefab into the scene.

Alternatively, use your own VR and eye tracking software. This will require a tweak in the HMDEyeRecording script to use your custom eye data object rather than the standard eye tracker's IVRGazeData.
The script is designed to be easily modified.

Next, drag the HMDGazeRecorder and/or HMDGazeReplayer prefab(s) into the scene.

Recorder:
	Drag the main camera (that representing the user's eyes) to the Main Camera field.
	Optionally enter a custom file name.

Replayer:
	Optionally enter a custom file name.

Done.


