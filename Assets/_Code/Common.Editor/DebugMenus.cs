#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Common.Editor
{
	public class DebugMenus
	{
		[MenuItem("Tools/Build Project")]
		static void BuildProject()
		{
			CustomUnityBuilderAction.Builder.BuildProject();
		}
	}
}
#endif
