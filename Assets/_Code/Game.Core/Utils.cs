using System.IO;
using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

namespace Game.Core
{
	public static class Utils
	{
		public static bool IsDevBuild()
		{
#if UNITY_EDITOR
			return true;
#endif

#pragma warning disable 162
			if (Debug.isDebugBuild)
			{
				return true;
			}

			return false;
#pragma warning restore 162
		}

		public static bool IsWebGL()
		{
			return Application.platform == RuntimePlatform.WebGLPlayer;
		}

		public static async UniTask<string> ReadStreamingAsset(string filename)
		{
			var path = Application.streamingAssetsPath + filename;

			if (IsWebGL())
				return (await UnityWebRequest.Get(path).SendWebRequest()).downloadHandler.text;

			return File.ReadAllText(path);
		}

		public static Vector3 SnapTo(Vector3 v3, float snapAngle)
		{
			float angle = Vector3.Angle(v3, Vector3.up);
			if (angle < snapAngle / 2.0f)          // Cannot do cross product
				return Vector3.up * v3.magnitude;  //   with angles 0 & 180
			if (angle > 180.0f - snapAngle / 2.0f)
				return Vector3.down * v3.magnitude;

			float t = Mathf.Round(angle / snapAngle);
			float deltaAngle = (t * snapAngle) - angle;

			Vector3 axis = Vector3.Cross(Vector3.up, v3);
			Quaternion q = Quaternion.AngleAxis(deltaAngle, axis);
			return q * v3;
		}

		public static async UniTask Shake(float gain, int duration)
		{
			if (Globals.State.PlayerSettings.Screenshake == false)
				return;

			var perlin = Globals.CameraRig.VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			perlin.m_AmplitudeGain = gain;
			// Gamepad.current?.SetMotorSpeeds(gain / 8f, gain / 4f);

			await UniTask.Delay(duration);

			perlin.m_AmplitudeGain = 0f;
			// Gamepad.current?.SetMotorSpeeds(0f, 0f);
		}

		public static string DiceRequestToString(DiceBag bag)
		{
			return DiceRequestToString(bag.Die, bag.Quantity, bag.Bonus);
		}

		public static string DiceRequestToString(DiceRequest req)
		{
			return DiceRequestToString(req.Die, req.Quantity, req.Bonus);
		}

		public static string DiceRequestToString(DieTypes die, int quantity, int bonus)
		{
			if (bonus > 0)
				return $"{quantity}D{(int)die} +{bonus}";
			else if (bonus < 0)
				return $"{quantity}D{(int)die} {bonus}";
			return $"{quantity}D{(int)die}";
		}
	}
}
