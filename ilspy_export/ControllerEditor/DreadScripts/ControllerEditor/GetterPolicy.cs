using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal static class GetterPolicy
{
	internal static GetterPolicy AssetDecorator;

	internal static Color ConnectHelper(string value)
	{
		int hashCode = value.GetHashCode();
		hashCode = (hashCode >> 16) ^ (hashCode & 0xFFFF);
		hashCode *= 73244475;
		float h = (float)(hashCode & 0xFF) / 255f;
		float s = 0.7f + (float)((hashCode >> 8) & 0x7F) / 255f * 0.3f;
		float v = 0.8f + (float)((hashCode >> 16) & 0x7F) / 255f * 0.2f;
		return Color.HSVToRGB(h, s, v);
	}

	internal static bool SelectDecorator()
	{
		return AssetDecorator == null;
	}
}
