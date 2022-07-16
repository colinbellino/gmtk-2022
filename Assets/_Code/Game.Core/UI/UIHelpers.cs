using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Core
{
	public class UIHelpers : MonoBehaviour
	{
		public static void PlayButtonClip()
		{
			AudioHelpers.PlayOneShot(Globals.Config.SoundMenuConfirm);
		}

		public void SetSelectedGameObject()
		{
			EventSystem.current.SetSelectedGameObject(gameObject);
		}

		public void Log()
		{
			UnityEngine.Debug.Log("Hello");
		}
	}
}
