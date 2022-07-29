namespace AsepriteToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;


	public class MoenenEditorWindow : EditorWindow {




		#region --- VAR ---


		private readonly static string[] COLORFUL_COLORS = new string[] {
			"#ff3333", // red
			"#ffcc00", // oran
			"#ffff33", // yell
			"#33ff33", // green
			"#33cccc", // cyan
			"#33ccff", // blue
			"#33ff33", // green
			"#ffff33", // yell
		};


		#endregion


		protected static Rect GUIRect (float width, float height) {
			return GUILayoutUtility.GetRect(
				width, height,
				GUILayout.ExpandWidth(width == 0), GUILayout.ExpandHeight(height == 0)
			);
		}



		protected static void Link (Rect buttonRect, string label, string link) {
			if (GUI.Button(buttonRect, label, new GUIStyle(GUI.skin.label) {
				wordWrap = true,
				normal = new GUIStyleState() {
					textColor = new Color(86f / 256f, 156f / 256f, 214f / 256f),
					background = null,
					scaledBackgrounds = null,
				}
			})) {
				Application.OpenURL(link);
			}
			EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);
		}



		protected static bool Button (Rect rect, string label, bool enabled = true, GUIStyle style = null) {
			return Button(rect, label, Color.clear, enabled, style);
		}



		protected static bool Button (Rect rect, string label, Color mark, bool enabled = true, GUIStyle style = null) {
			bool oldE = GUI.enabled;
			GUI.enabled = oldE && enabled;
			if (style == null) { style = GUI.skin.button; }
			bool pressed = GUI.Button(rect, label, style);
			if (enabled && mark.a > 0.01f) {
				ColorBlock(new Rect(rect.x + 2, rect.y + 3, 1f, rect.height - 4), mark);
			}
			GUI.enabled = oldE;
			return pressed;
		}


		protected static void Label (Rect rect, string label, Color tint, bool enable = true, GUIStyle style = null) {
			if (style == null) {
				style = GUI.skin.label;
			}
			bool oldE = GUI.enabled;
			var oldC = GUI.color;
			GUI.color = tint;
			GUI.enabled = enable;
			GUI.Label(rect, label, style);
			GUI.enabled = oldE;
			GUI.color = oldC;
		}


		protected static Color ColorField (int width, int height, string label, Color value, bool enabled = true) {
			bool oldE = GUI.enabled;
			GUI.enabled = enabled;
			LayoutH(() => {
				if (width > 0 && !string.IsNullOrEmpty(label)) {
					GUI.Label(GUIRect(width, height), label);
				}
				value = ColorField(GUIRect(height, height), value);
			}, false, new GUIStyle() {
				fixedWidth = width + height,
			});
			GUI.enabled = oldE;
			return value;
		}



		protected static Color ColorField (Rect rect, Color value) {
#if UNITY_2017 || UNITY_5 || UNITY_4 || UNITY_3
			return EditorGUI.ColorField(rect, GUIContent.none, value, false, false, false, null);
#else
			return EditorGUI.ColorField(rect, GUIContent.none, value, false, false, false);
#endif
		}



		protected static void LayoutV (System.Action action, bool box = false, GUIStyle style = null) {
			if (box) {
				style = new GUIStyle(GUI.skin.box) {
					padding = new RectOffset(6, 6, 2, 2)
				};
			}
			if (style != null) {
				GUILayout.BeginVertical(style);
			} else {
				GUILayout.BeginVertical();
			}
			action();
			GUILayout.EndVertical();
		}



		protected static void LayoutH (System.Action action, bool box = false, GUIStyle style = null) {
			if (box) {
				style = new GUIStyle(GUI.skin.box) {
					padding = new RectOffset(6, 6, 2, 2)
				};
			}
			if (style != null) {
				GUILayout.BeginHorizontal(style);
			} else {
				GUILayout.BeginHorizontal();
			}
			action();
			GUILayout.EndHorizontal();
		}


		static bool OldLogger = true;
		protected static void BeginLogger () {
			OldLogger = Debug.unityLogger.logEnabled;
			Debug.unityLogger.logEnabled = false;
		}


		protected static void EndLogger () {
			Debug.unityLogger.logEnabled = OldLogger;
		}


		protected static bool LayoutF (System.Action action, string label, bool open, bool box = false, GUIStyle style = null) {
			LayoutV(() => {
				open = GUILayout.Toggle(
					open,
					label,
					GUI.skin.GetStyle("foldout"),
					GUILayout.ExpandWidth(true),
					GUILayout.Height(18)
				);
				if (open) {
					action();
				}
			}, box, style);
			Space(4);
			return open;
		}



		protected static void Space (float space = 4f) {
			GUILayout.Space(space);
		}



		protected static string GetDisplayString (string str, int maxLength) {
			return str.Length > maxLength ? str.Substring(0, maxLength - 3) + "..." : str;
		}



		protected static bool ColorfulButton (Rect rect, string label, Color color, GUIStyle style = null) {
			Color oldColor = GUI.color;
			GUI.color = color;
			bool pressed = style == null ? GUI.Button(rect, label) : GUI.Button(rect, label, style);
			GUI.color = oldColor;
			return pressed;
		}



		protected static void ColorBlock (Rect rect, Color color) {
			var oldC = GUI.color;
			GUI.color = color;
			GUI.DrawTexture(rect, Texture2D.whiteTexture);
			GUI.color = oldC;
		}



		protected static string GetColorfulString (string source, int len = -1, string defaultColor = "#ffffff") {
			if (string.IsNullOrEmpty(source)) { return ""; }
			string result = "";
			for (int i = 0; i < source.Length; i++) {
				result += string.Format("<color={1}>{0}</color>", source[i], len < 0 || i < len ? COLORFUL_COLORS[i % COLORFUL_COLORS.Length] : defaultColor);
			}
			return result;
		}


		protected static Texture2D GetImage (string name) {
			try {
				return AssetDatabase.LoadAssetAtPath<Texture2D>(Util.CombinePaths(Util.GetRootPath(), "Image", name));
			} catch { }
			return null;
		}



	}
}
