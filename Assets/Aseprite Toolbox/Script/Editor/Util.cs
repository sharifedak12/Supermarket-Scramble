namespace AsepriteToolbox {
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;


	public struct Util {



		#region --- File ---



		public static string Read (string path) {
			path = FixPath(path, false);
			StreamReader sr = File.OpenText(path);
			string data = sr.ReadToEnd();
			sr.Close();
			return data;
		}



		public static void Write (string data, string path) {
			path = FixPath(path, false);
			CreateFolder(GetParentPath(path));
			FileStream fs = new FileStream(path, FileMode.Create);
			StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
			sw.Write(data);
			sw.Close();
			fs.Close();
		}




		public static void CreateFolder (string path) {
			if (!string.IsNullOrEmpty(path) && !DirectoryExists(path)) {
				string pPath = GetParentPath(path);
				if (!DirectoryExists(pPath)) {
					CreateFolder(pPath);
				}
				path = FixPath(path, false);
				Directory.CreateDirectory(path);
			}
		}



		public static byte[] FileToByte (string path) {
			byte[] bytes = null;
			if (FileExists(path)) {
				path = FixPath(path, false);
				bytes = File.ReadAllBytes(path);
			}
			return bytes;
		}



		public static void ByteToFile (byte[] bytes, string path) {
			CreateFolder(Directory.GetParent(path).FullName);
			path = FixPath(path, false);
			FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();
			fs.Dispose();
		}




		public static bool HasFileIn (string path, params string[] searchPattern) {
			if (PathIsDirectory(path)) {
				for (int i = 0; i < searchPattern.Length; i++) {
					path = FixPath(path, false);
					if (new DirectoryInfo(path).GetFiles(searchPattern[i], SearchOption.AllDirectories).Length > 0) {
						return true;
					}
				}
			}
			return false;
		}



		public static FileInfo[] GetFilesIn (string path, params string[] searchPattern) {
			List<FileInfo> allFiles = new List<FileInfo>();
			path = FixPath(path, false);
			if (PathIsDirectory(path)) {
				if (searchPattern.Length <= 0) {
					allFiles.AddRange(new DirectoryInfo(path).GetFiles("*.*", SearchOption.AllDirectories));
				} else {
					for (int i = 0; i < searchPattern.Length; i++) {
						allFiles.AddRange(new DirectoryInfo(path).GetFiles(searchPattern[i], SearchOption.AllDirectories));
					}
				}
			}
			return allFiles.ToArray();
		}



		public static void DeleteFile (string path) {
			if (FileExists(path)) {
				path = FixPath(path, false);
				File.Delete(path);
			}
		}



		public static void DeleteFolder (string path) {
			if (Directory.Exists(path)) {
				Directory.Delete(path, true);
			}
		}



		public static void CopyFile (string from, string to) {
			if (FileExists(from) && from != to) {
				CreateFolder(GetParentPath(to));
				File.Copy(from, to, true);
			}
		}


		#endregion



		#region --- Path ---



		private const string ROOT_NAME = "Aseprite Toolbox";



		public static string GetRootPath () {
			ScriptableObject scriptObj = ScriptableObject.CreateInstance<AsepriteToolbox_Window>();
			if (!scriptObj) { return ""; }
			var rootPath = "";
			var script = MonoScript.FromScriptableObject(scriptObj);
			Object.DestroyImmediate(scriptObj, true);
			if (script) {
				var path = AssetDatabase.GetAssetPath(script);
				string rootName = ROOT_NAME;
				if (!string.IsNullOrEmpty(path)) {
					int index = path.LastIndexOf(rootName);
					if (index >= 0) {
						rootPath = path.Substring(0, index + rootName.Length);
					}
				}
			}
			return rootPath;
		}



		public static string FixPath (string path, bool forUnity = true) {
			char dsChar = forUnity ? '/' : Path.DirectorySeparatorChar;
			char adsChar = forUnity ? '\\' : Path.AltDirectorySeparatorChar;
			path = path.Replace(adsChar, dsChar);
			path = path.Replace(new string(dsChar, 2), dsChar.ToString());
			while (path.Length > 0 && path[0] == dsChar) {
				path = path.Remove(0, 1);
			}
			while (path.Length > 0 && path[path.Length - 1] == dsChar) {
				path = path.Remove(path.Length - 1, 1);
			}
			return path;
		}



		public static string GetParentPath (string path) {
			path = FixPath(path, false);
			return FixedRelativePath(Directory.GetParent(path).FullName);
		}




		public static string FixedRelativePath (string path) {
			path = FixPath(path);
			if (path.StartsWith("Assets")) {
				return path;
			}
			var fixedDataPath = FixPath(Application.dataPath);
			if (path.StartsWith(fixedDataPath)) {
				return "Assets" + path.Substring(fixedDataPath.Length);
			} else {
				return "";
			}
		}




		public static string GetFullPath (string path) {
			path = FixPath(path, false);
			return new FileInfo(path).FullName;
		}



		public static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, paths[i]);
			}
			return path;
		}



		public static string GetExtension (string path) {
			return Path.GetExtension(path);//.txt
		}



		public static string GetNameWithoutExtension (string path) {
			return Path.GetFileNameWithoutExtension(path);
		}


		public static string GetNameWithExtension (string path) {
			return Path.GetFileName(path);
		}


		public static string ChangeExtension (string path, string newEx) {
			return Path.ChangeExtension(path, newEx);
		}



		public static bool DirectoryExists (string path) {
			path = FixPath(path, false);
			return Directory.Exists(path);
		}



		public static bool FileExists (string path) {
			path = FixPath(path, false);
			return File.Exists(path);
		}



		public static bool PathIsDirectory (string path) {
			if (!DirectoryExists(path)) { return false; }
			path = FixPath(path, false);
			FileAttributes attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;
		}



		public static bool IsChildPath (string pathA, string pathB) {
			if (pathA.Length == pathB.Length) {
				return pathA == pathB;
			} else if (pathA.Length > pathB.Length) {
				return IsChildPathCompar(pathA, pathB);
			} else {
				return IsChildPathCompar(pathB, pathA);
			}
		}



		public static bool IsChildPathCompar (string longPath, string path) {
			if (longPath.Length <= path.Length || !PathIsDirectory(path) || !longPath.StartsWith(path)) {
				return false;
			}
			char c = longPath[path.Length];
			if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar) {
				return false;
			}
			return true;
		}



		#endregion



		#region --- Message ---


		public static bool Dialog (string title, string msg, string ok, string cancel = "") {
			//EditorApplication.Beep();
			PauseWatch();
			if (string.IsNullOrEmpty(cancel)) {
				bool sure = EditorUtility.DisplayDialog(title, msg, ok);
				RestartWatch();
				return sure;
			} else {
				bool sure = EditorUtility.DisplayDialog(title, msg, ok, cancel);
				RestartWatch();
				return sure;
			}
		}


		public static int DialogComplex (string title, string msg, string ok, string cancel, string alt) {
			//EditorApplication.Beep();
			PauseWatch();
			int index = EditorUtility.DisplayDialogComplex(title, msg, ok, cancel, alt);
			RestartWatch();
			return index;
		}


		public static void ProgressBar (string title, string msg, float value) {
			value = Mathf.Clamp01(value);
			EditorUtility.DisplayProgressBar(title, msg, value);
		}


		public static void ClearProgressBar () {
			EditorUtility.ClearProgressBar();
		}


		#endregion



		#region --- Watch ---


		private static System.Diagnostics.Stopwatch TheWatch;


		public static void StartWatch () {
			TheWatch = new System.Diagnostics.Stopwatch();
			TheWatch.Start();
		}


		public static void PauseWatch () {
			if (TheWatch != null) {
				TheWatch.Stop();
			}
		}


		public static void RestartWatch () {
			if (TheWatch != null) {
				TheWatch.Start();
			}
		}


		public static double StopWatchAndGetTime () {
			if (TheWatch != null) {
				TheWatch.Stop();
				return TheWatch.Elapsed.TotalSeconds;
			}
			return 0f;
		}


		#endregion



		#region --- Misc ---


		public static bool IsTypingInGUI () {
			return GUIUtility.keyboardControl != 0;
		}



		public static bool NoFuncKeyPressing () {
			return !Event.current.alt && !Event.current.control && !Event.current.shift;
		}




		public static bool InRange (int x, int y, int z, int sizeX, int sizeY, int sizeZ) {
			return x >= 0 && x < sizeX && y >= 0 && y < sizeY && z >= 0 && z < sizeZ;
		}


		public static Vector2 VectorAbs (Vector2 v) {
			v.x = Mathf.Abs(v.x);
			v.y = Mathf.Abs(v.y);
			return v;
		}



		public static Vector3 VectorAbs (Vector3 v) {
			v.x = Mathf.Abs(v.x);
			v.y = Mathf.Abs(v.y);
			v.z = Mathf.Abs(v.z);
			return v;
		}


		public static float Remap (float l, float r, float newL, float newR, float t) {
			return l == r ? 0 : Mathf.LerpUnclamped(
				newL, newR,
				(t - l) / (r - l)
			);
		}


		public static int MaxAxis (Vector3 v) {
			if (Mathf.Abs(v.x) >= Mathf.Abs(v.y)) {
				return Mathf.Abs(v.x) >= Mathf.Abs(v.z) ? 0 : 2;
			} else {
				return Mathf.Abs(v.y) >= Mathf.Abs(v.z) ? 1 : 2;
			}
		}


		public static void CopyToClipboard (string containt) {
			GUIUtility.systemCopyBuffer = containt;
		}



		public static void TrimTexture (Texture2D texture, float alpha = 0.01f, int gap = 0) {
			int width = texture.width;
			int height = texture.height;
			var colors = texture.GetPixels();
			int minX = int.MaxValue;
			int minY = int.MaxValue;
			int maxX = int.MinValue;
			int maxY = int.MinValue;

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					var c = colors[y * width + x];
					if (c.a > alpha) {
						minX = Mathf.Min(minX, x);
						minY = Mathf.Min(minY, y);
						maxX = Mathf.Max(maxX, x);
						maxY = Mathf.Max(maxY, y);
					}
				}
			}

			// Gap
			minX = Mathf.Clamp(minX - gap, 0, width - 1);
			minY = Mathf.Clamp(minY - gap, 0, height - 1);
			maxX = Mathf.Clamp(maxX + gap, 0, width - 1);
			maxY = Mathf.Clamp(maxY + gap, 0, height - 1);

			int newWidth = maxX - minX + 1;
			int newHeight = maxY - minY + 1;
			if (newWidth != width || newHeight != height) {
				texture.Reinitialize(newWidth, newHeight);
				var newColors = new Color[newWidth * newHeight];
				for (int y = 0; y < newHeight; y++) {
					for (int x = 0; x < newWidth; x++) {
						newColors[y * newWidth + x] = colors[(y + minY) * width + (x + minX)];
					}
				}
				texture.SetPixels(newColors);
				texture.Apply();
			}
		}




		public static bool GetBit (int value, int index) {
			if (index < 0 || index > 31) { return false; }
			var val = 1 << index;
			return (value & val) == val;
		}



		public static int SetBitValue (int value, int index, bool bitValue) {
			if (index < 0 || index > 31) { return value; }
			var val = 1 << index;
			return bitValue ? (value | val) : (value & ~val);
		}




		public static AnimationClip CopyAnimation (AnimationClip source) {
			// Init
			var animation = new AnimationClip() {
				frameRate = source.frameRate,
				name = source.name,
				wrapMode = source.wrapMode,
				legacy = source.legacy,
				hideFlags = source.hideFlags,
				localBounds = source.localBounds,
			};
			// Data
			var bindings = AnimationUtility.GetCurveBindings(source);
			for (int i = 0; i < bindings.Length; i++) {
				var binding = bindings[i];
				var curve = AnimationUtility.GetEditorCurve(source, binding);
				var keys = new Keyframe[curve.length];
				for (int j = 0; j < keys.Length; j++) {
					keys[j] = curve.keys[j];
				}
				animation.SetCurve(binding.path, binding.type, binding.propertyName, new AnimationCurve(keys) {
					postWrapMode = curve.postWrapMode,
					preWrapMode = curve.preWrapMode,
				});
			}
			return animation;
		}



		public static void ClearChildrenImmediate (Transform tf) {
			if (!tf) { return; }
			int len = tf.childCount;
			for (int i = 0; i < len; i++) {
				Object.DestroyImmediate(tf.GetChild(0).gameObject, false);
			}
		}



		public static void CurveAllLiner (AnimationCurve curve) {
			for (int i = 0; i < curve.keys.Length; i++) {
				var key = curve.keys[i];
				key.inTangent = 0f;
				key.outTangent = 0f;
#if UNITY_2018_3_6
				key.inWeight = 0f;
				key.outWeight = 0f;
				key.weightedMode = WeightedMode.Both;
#endif
				curve.MoveKey(i, key);
			}
		}



		public static bool GetTrimOffset (Color[] pixels, int width, int height, out int bottom, out int top, out int left, out int right) {
			return GetTrimOffset(pixels, width, height, out bottom, out top, out left, out right, 0, 0, width - 1, height - 1);
		}


		public static bool GetTrimOffset (Color[] pixels, int width, int height, out int bottom, out int top, out int left, out int right, int minX, int minY, int maxX, int maxY) {
			bottom = -1;
			top = -1;
			left = -1;
			right = -1;
			minX = Mathf.Clamp(minX, 0, width - 1);
			minY = Mathf.Clamp(minY, 0, height - 1);
			maxX = Mathf.Clamp(maxX, minX, width - 1);
			maxY = Mathf.Clamp(maxY, minY, height - 1);
			for (int y = minY; y <= maxY && bottom < 0; y++) {
				for (int x = minX; x <= maxX; x++) {
					if (pixels[y * width + x].a > 0.001f) {
						bottom = y;
						break;
					}
				}
			}
			for (int y = Mathf.Min(height - 1, maxY); y >= minY && top < 0; y--) {
				for (int x = minX; x <= maxX; x++) {
					if (pixels[y * width + x].a > 0.001f) {
						top = y;
						break;
					}
				}
			}
			for (int x = minX; x <= maxX && left < minX; x++) {
				for (int y = minY; y <= maxY; y++) {
					if (pixels[y * width + x].a > 0.001f) {
						left = x;
						break;
					}
				}
			}
			for (int x = Mathf.Min(width - 1, maxX); x >= minX && right < 0; x--) {
				for (int y = minY; y <= maxY; y++) {
					if (pixels[y * width + x].a > 0.001f) {
						right = x;
						break;
					}
				}
			}
			if (bottom < 0) { bottom = minY; }
			if (top < 0) { top = maxY; }
			if (left < 0) { left = minX; }
			if (right < 0) { right = maxX; }
			return bottom < top && left < right && (bottom > minY || top < maxY || left > minX || right < maxX);
		}



		#endregion



	}


}
