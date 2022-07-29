namespace AsepriteToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using UnityEditor.Animations;


	public class AsePostprocessor : AssetPostprocessor {



		// SUB
		public class TextureImportData {
			public int PixelPerUnit;
			public SpriteMetaData[] SpriteMetas;
		}




		public class AnimationFrameImportData {
			public string TexturePath;
			public string SpriteName;
			public float Duration;
		}



		public class PrefabImportData {
			public string TexturePath;
			public string SpriteName;
		}



		// VAR
		private readonly static Dictionary<string, TextureImportData> ImportMapTexture = new Dictionary<string, TextureImportData>();
		private readonly static Dictionary<string, PrefabImportData> ImportMapPrefab = new Dictionary<string, PrefabImportData>();
		private readonly static Dictionary<string, AnimationFrameImportData[]> ImportMapAnimation = new Dictionary<string, AnimationFrameImportData[]>();
		private readonly static Dictionary<string, string[]> ImportMapAnimator = new Dictionary<string, string[]>();
		private readonly static EditorCurveBinding SPRITE_BINDING = new EditorCurveBinding() {
			path = "",
			propertyName = "m_Sprite",
			type = typeof(SpriteRenderer),
		};



		// API
		public static void Clear () {
			ImportMapTexture.Clear();
			ImportMapAnimation.Clear();
			ImportMapAnimator.Clear();
			ImportMapPrefab.Clear();
		}


		public static void Add (string assetPath, TextureImportData data) {
			assetPath = Util.FixedRelativePath(assetPath);
			if (!ImportMapTexture.ContainsKey(assetPath)) {
				ImportMapTexture.Add(assetPath, data);
			}
		}


		public static void Add (string assetPath, AnimationFrameImportData[] datas) {
			assetPath = Util.FixedRelativePath(assetPath);
			if (!ImportMapAnimation.ContainsKey(assetPath)) {
				ImportMapAnimation.Add(assetPath, datas);
			} else {
				ImportMapAnimation[assetPath] = datas;
			}
		}


		public static void Add (string assetPath, string[] data) {
			assetPath = Util.FixedRelativePath(assetPath);
			if (!ImportMapAnimator.ContainsKey(assetPath)) {
				ImportMapAnimator.Add(assetPath, data);
			}
		}



		public static void Add (string assetPath, PrefabImportData data) {
			assetPath = Util.FixedRelativePath(assetPath);
			if (!ImportMapPrefab.ContainsKey(assetPath)) {
				ImportMapPrefab.Add(assetPath, data);
			}
		}



		// MSG
		private void OnPreprocessTexture () {

			TextureImporter ti = assetImporter as TextureImporter;
			if (ti == null) { return; }

			var path = Util.FixedRelativePath(ti.assetPath);
			if (!ImportMapTexture.ContainsKey(path)) { return; }
			var data = ImportMapTexture[path];
			ImportMapTexture.Remove(path);

			// Texture
			ti.textureType = TextureImporterType.Sprite;
			ti.spriteImportMode = SpriteImportMode.Multiple;
			ti.filterMode = FilterMode.Point;
			ti.textureCompression = TextureImporterCompression.Uncompressed;
			ti.mipmapEnabled = false;
			ti.alphaIsTransparency = true;
			ti.spritesheet = data.SpriteMetas;
			ti.spritePixelsPerUnit = data.PixelPerUnit;
			var textureSettings = new TextureImporterSettings();
			ti.ReadTextureSettings(textureSettings);
			textureSettings.spriteMeshType = SpriteMeshType.FullRect;
			textureSettings.spriteExtrude = 0;
			textureSettings.spriteGenerateFallbackPhysicsShape = true;
			ti.SetTextureSettings(textureSettings);

		}


		private static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if (ImportMapAnimation.Count == 0 && ImportMapAnimator.Count == 0 && ImportMapPrefab.Count == 0) { return; }

			var animationMap = new Dictionary<string, AnimationFrameImportData[]>(ImportMapAnimation);
			var animatorMap = new Dictionary<string, string[]>(ImportMapAnimator);
			var importMapPrefab = new Dictionary<string, PrefabImportData>(ImportMapPrefab);
			ImportMapAnimation.Clear();
			ImportMapAnimator.Clear();
			ImportMapPrefab.Clear();

			EditorApplication.delayCall += () => {

				bool needRefresh = false;

				// Create Animations
				foreach (var pair in animationMap) {
					if (pair.Value == null || pair.Value.Length == 0) { continue; }

					var importData = pair.Value;
					var path = pair.Key;

					// Old or New
					AnimationClip aniClip = null;
					if (Util.FileExists(path)) {
						aniClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
					}
					if (aniClip == null) {
						aniClip = new AnimationClip() {
							name = Util.GetNameWithoutExtension(path),
							wrapMode = WrapMode.Once,
						};
					}
					aniClip.frameRate = 120f;

					// Set Sprites
					float currentTime = 0f;
					var frameList = new List<ObjectReferenceKeyframe>();
					Texture2D texture = null;
					string texturePath = "";
					Object[] spritesInTexture = null;
					for (int i = 0; i < importData.Length; i++) {
						var frame = importData[i];
						float duration = Mathf.Max(frame.Duration, 0.001f);
						// Get Texture and Sprites Obj
						if (!texture || frame.TexturePath != texturePath) {
							texturePath = "";
							spritesInTexture = null;
							texture = AssetDatabase.LoadAssetAtPath<Texture2D>(frame.TexturePath);
							if (texture) {
								texturePath = frame.TexturePath;
								spritesInTexture = AssetDatabase.LoadAllAssetsAtPath(frame.TexturePath);
							}
						}
						// Get Sprite
						Sprite sprite = null;
						if (spritesInTexture != null) {
							for (int j = 0; j < spritesInTexture.Length; j++) {
								var sp = spritesInTexture[j] as Sprite;
								if (sp && sp.name == frame.SpriteName) {
									sprite = sp;
									break;
								}
							}
						}

						// Final
						frameList.Add(new ObjectReferenceKeyframe() { time = currentTime, value = sprite, });
						currentTime += duration;
					}
					frameList.Add(new ObjectReferenceKeyframe() { time = currentTime, value = frameList.Count == 0 ? null : frameList[frameList.Count - 1].value, });
					AnimationUtility.SetObjectReferenceCurve(aniClip, SPRITE_BINDING, frameList.ToArray());

					// Dirty Old or Create New
					if (Util.FileExists(path)) {
						EditorUtility.SetDirty(aniClip);
						needRefresh = true;
					} else {
						Util.CreateFolder(Util.GetParentPath(path));
						AssetDatabase.CreateAsset(aniClip, path);
					}
				}

				// Create Animators
				foreach (var pair in animatorMap) {
					var path = pair.Key;
					var aniPaths = pair.Value;
					// Old or New
					AnimatorController ani = null;
					if (!Util.FileExists(path)) {
						AnimatorController.CreateAnimatorControllerAtPath(path);
					}
					ani = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
					if (ani == null) { continue; }

					// Try Setup Layer
					if (ani.layers.Length == 0) {
						ani.AddLayer("Layer");
					}

					// Add Animations
					var oldClipList = new List<AnimationClip>(ani.animationClips);
					foreach (var aniPath in aniPaths) {
						var aClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(aniPath);
						if (aClip && !oldClipList.Contains(aClip)) {
							ani.AddMotion(aClip, 0);
						}
					}

					// Dirty
					EditorUtility.SetDirty(ani);
					needRefresh = true;
				}

				// Prefabs
				Texture2D pTexture = null;
				string pTexturePath = "";
				Object[] spriteObjs = null;
				foreach (var pair in importMapPrefab) {
					var path = pair.Key;
					var importData = pair.Value;
					if (string.IsNullOrEmpty(path) || importData == null) { continue; }
					// Get Texture
					if (pTexturePath != importData.TexturePath) {
						try {
							pTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(importData.TexturePath);
							spriteObjs = AssetDatabase.LoadAllAssetsAtPath(importData.TexturePath);
							pTexturePath = importData.TexturePath;
						} catch {
							pTexture = null;
						}
					}
					if (!pTexture) {
						pTexturePath = "";
						spriteObjs = null;
						continue;
					}
					// Get Sprite
					Sprite sp = null;
					foreach (var obj in spriteObjs) {
						if (obj is Sprite && (obj as Sprite).name == importData.SpriteName) {
							sp = obj as Sprite;
							break;
						}
					}
					if (!sp) { continue; }
					// Prefab
					Transform tf = null;
					try {
						// Create Game Object
						tf = new GameObject(importData.SpriteName, typeof(SpriteRenderer)).transform;
						var sr = tf.GetComponent<SpriteRenderer>();
						sr.sprite = sp;
						// To Prefab
						PrefabUtility.SaveAsPrefabAsset(tf.gameObject, path);
					} catch { }
					// Delete Instance
					if (tf) {
						Object.DestroyImmediate(tf.gameObject, false);
					}
					needRefresh = true;
				}

				// Final
				if (needRefresh) {
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			};

		}



	}
}