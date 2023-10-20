using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IPA;
using IPALogger = IPA.Logging.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlassPad3CameraBehavior
{
	[Plugin(RuntimeOptions.SingleStartInit)]
	internal class Plugin
    {
		private class FrontCameraBehavior : MonoBehaviour
		{
			public CustomAvatar.Tracking.IAvatarInput input { get; set; }

			private Queue<float> queue = new Queue<float>(128);

			private const int updateInterval = 4;

			private int updateCounter;

			private void Start()
			{
				logger.Debug("behavior start");
			}

			private void Update()
			{
				if (updateCounter++ < updateInterval) return;
				updateCounter = 0;

				Pose headPose;
				if (input.TryGetPose(CustomAvatar.Tracking.DeviceUse.Head, out headPose) == false) return;
				queue.Enqueue(headPose.position.y);
				while (queue.Count > 100) { queue.Dequeue(); }
				Vector3 position = transform.position;
				position.y = queue.Average() * 0.92f;
				transform.position = position;
			}
		}

		public static IPALogger logger { get; private set; }

		[Init]
		public Plugin(IPALogger logger)
		{
			Plugin.logger = logger;
		}

		[OnStart]
		public void OnStart()
		{
			SceneManager.activeSceneChanged += OnActiveSceneChanged;
		}

		public void OnActiveSceneChanged(Scene from, Scene to)
		{
			SharedCoroutineStarter.instance.StartCoroutine(InjectBehavior());
		}

		private IEnumerator InjectBehavior()
		{
			yield return new WaitForSecondsRealtime(1.0f);

			Camera camera = Camera.allCameras.FirstOrDefault((Camera c) => c.name == "CPCameraFront" && c.gameObject.layer == 5);
			if (camera == null) yield break;

			logger.Debug("camera found");

			GameObject avatarContainer = GameObject.Find("Avatar Container");
			if (avatarContainer == null) yield break;

			logger.Debug("avatar container found");

			CustomAvatar.Avatar.SpawnedAvatar avatar = avatarContainer.GetComponentInChildren<CustomAvatar.Avatar.SpawnedAvatar>();
			if (avatar == null) yield break;

			logger.Debug("spawned avatar found");

			System.Object tracking = avatar.GetComponent<CustomAvatar.Avatar.AvatarTracking>();
			if (tracking == null) yield break;

			logger.Debug("tracking found");

			CustomAvatar.Tracking.IAvatarInput input = ReflectionUtil.GetPrivateField<CustomAvatar.Tracking.IAvatarInput>(tracking, "_input");
			if (input == null) yield break;

			logger.Debug("input found");
			logger.Debug("setting up behavior");

			FrontCameraBehavior behavior = camera.GetComponent<FrontCameraBehavior>();
			if (behavior == null)
			{
				behavior = camera.gameObject.AddComponent<FrontCameraBehavior>();
			}
			behavior.input = input;

			logger.Info("ready to go");
		}
	}
}
