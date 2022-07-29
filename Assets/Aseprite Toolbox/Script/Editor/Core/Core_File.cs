namespace AsepriteToolbox.Core {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using System.Linq;

	public class FileCore {


		// Data
		private readonly TaskResult[] Results = null;


		// Config
		public AnimationExportMode AnimationExportMode = AnimationExportMode.OneAnimationPerTag;
		public TextureExportMode TextureExportMode = TextureExportMode.OneTexturePerAse;
		public PaddingMode PaddingMode = PaddingMode.ClearColor;
		public bool CreateAnimation = false;
		public bool CreateAnimator = false;
		public bool IgnoreSprites = false;
		public bool FolderForTask = true;
		public bool TrimTexture = false;
		public bool TrimSprite = false;
		public bool SeparateEachLayer = false;
		public bool FolderForLayer = false;
		public bool CreatePrefab = false;
		public int PixelPerUnit = 16;
		public string AseName = "";
		public string ExportingFolder = "";
		public string[] NamingStrategy_Texture = new string[] { "", "", };
		public string[] NamingStrategy_Sprite = new string[] { "", "", };
		public string[] NamingStrategy_Animation = new string[] { "", "", };
		public string[] NamingStrategy_Animator = new string[] { "", "", };


		// API
		public FileCore (TaskResult[] results) {
			Results = results;
		}


		public string MakeFiles () {
			if (Results == null || Results.Length == 0) { return ""; }
			string path = "";
			for (int i = 0; i < Results.Length; i++) {
				var result = Results[i];
				if (result == null || string.IsNullOrEmpty(ExportingFolder) || result.Frames == null || result.Frames.Length == 0) { continue; }
				path = MakeFile(result);
			}
			return path;
		}


		// LGC
		private string MakeFile (TaskResult result) {
			result = CombineColors(result);
			var textureSpriteMetaList = GetTextureList(result);
			var exportFolder = CreateExportFolder(result);
			RenameSameNameSprites(result);
			var aniList = GetAnimationList(result, textureSpriteMetaList, exportFolder);
			MakePNGFiles(textureSpriteMetaList, exportFolder);
			var aniPaths = MakeAnimationProcess(aniList, exportFolder);
			MakeAnimatorProcess(aniPaths, exportFolder);
			CreatePrefabs(textureSpriteMetaList, exportFolder);
			return exportFolder;
		}


		// Pipe
		private TaskResult CombineColors (TaskResult result) {
			// Combine Colors
			if (TextureExportMode == TextureExportMode.OneTexturePerAse && result.Frames.Length > 1) {

				// Get Packing Data
				var packingList = new List<PackingData>();
				for (int i = 0; i < result.Frames.Length; i++) {
					var frame = result.Frames[i];
					packingList.Add(new PackingData(frame.Width, frame.Height, frame.Pixels));
				}

				// Pack
				Color[] colors;
				int width, height;
				var textureRects = RectPacking.PackTextures(out colors, out width, out height, packingList, !TrimSprite && !TrimTexture);

				// Trim Color
				if (TrimTexture) {
					int bottom, top, left, right;
					if (Util.GetTrimOffset(colors, width, height, out bottom, out top, out left, out right)) {
						int newWidth = Mathf.Min(width, right + 2);
						int newHeight = Mathf.Min(height, top + 2);
						Color[] newColors = new Color[newWidth * newHeight];
						for (int x = 0; x < newWidth; x++) {
							for (int y = 0; y < newHeight; y++) {
								newColors[y * newWidth + x] = colors[y * width + x];
							}
						}
						colors = newColors;
						width = newWidth;
						height = newHeight;
					}
				}

				if (colors.Length > 0) {
					// Fix Sprite Meta
					var spriteList = new List<SpriteMetaData>();
					var sFrameList = new List<int>();
					for (int i = 0; i < textureRects.Length && i < result.Frames.Length; i++) {
						var rect = textureRects[i];
						var frame = result.Frames[i];
						for (int j = 0; j < frame.Sprites.Length; j++) {
							var sprite = frame.Sprites[j];
							sprite.rect.x += rect.x;
							sprite.rect.y += rect.y;
							spriteList.Add(sprite);
							sFrameList.Add(i);
						}
					}

					// Final
					var fData = new TaskResult.FrameResult() {
						Width = width,
						Height = height,
						Duration = 0f,
						FrameIndex = 0,
						Tag = "",
						Pixels = colors,
						Sprites = spriteList.ToArray(),
						SpriteFrames = sFrameList.ToArray(),
					};
					return new TaskResult() {
						Frames = new TaskResult.FrameResult[1] { fData },
						Tags = result.Tags,
						Durations = result.Durations,
					};
				} else {
					Debug.LogWarning("[Aseprite Toolbox] Failed to combine texture.");
				}
			}
			return result;
		}


		private List<Pair2<Texture2D, SpriteMetaData[]>> GetTextureList (TaskResult result) {
			var textureList = new List<Pair2<Texture2D, SpriteMetaData[]>>();
			var nameStrategy_Texture = NamingStrategy_Texture[result.Frames.Length > 1 ? 1 : 0];
			var renameMap = new Dictionary<string, byte>();
			for (int i = 0; i < result.Frames.Length; i++) {
				var frame = result.Frames[i];
				if (frame == null) { continue; }
				// Name
				var textureBasicName = string.Format(
					nameStrategy_Texture,
					AseName, frame.FrameIndex, "", frame.Tag, i
				);
				if (SeparateEachLayer && Results.Length > 1) {
					if (string.IsNullOrEmpty(textureBasicName)) {
						textureBasicName = result.LayerName;
					} else {
						textureBasicName += "_" + result.LayerName;
					}
				}
				var textureName = textureBasicName;
				int nameIndex = 0;
				while (renameMap.ContainsKey(textureName)) {
					textureName = textureBasicName + "_" + nameIndex.ToString();
					nameIndex++;
				}
				renameMap.Add(textureName, 0);
				// Texture
				var texture = new Texture2D(frame.Width, frame.Height) {
					name = textureName,
					filterMode = FilterMode.Point,
					alphaIsTransparency = true,
					wrapMode = TextureWrapMode.Clamp,
				};
				texture.SetPixels(frame.Pixels);
				texture.Apply();

				// Set Sprite Names
				if (!IgnoreSprites) {
					var nameStrategy_Sprite = NamingStrategy_Sprite[frame.Sprites.Length > 1 ? 1 : 0];
					bool useTag = nameStrategy_Sprite.IndexOf("{3}") >= 0;
					for (int j = 0; j < frame.Sprites.Length; j++) {
						// Tag
						int namingCount = j;
						string tag = frame.Tag;
						if (useTag) {
							if (frame.SpriteFrames != null && j < frame.SpriteFrames.Length) {
								(tag, namingCount) = GetTagInTags(result.Tags, frame.SpriteFrames[j]);
							} else {
								namingCount = i;
							}
						}
						// Final
						frame.Sprites[j].name = string.Format(
							nameStrategy_Sprite,
							AseName, frame.FrameIndex, frame.Sprites[j].name, tag, namingCount
						);
					}
				}

				// Final
				textureList.Add(new Pair2<Texture2D, SpriteMetaData[]>(texture, IgnoreSprites ? new SpriteMetaData[0] : frame.Sprites));
			}
			return textureList;
		}


		private void RenameSameNameSprites (TaskResult result) {
			for (int f = 0; f < result.Frames.Length; f++) {
				var frame = result.Frames[f];
				if (frame == null) { continue; }
				var renameMap = new Dictionary<string, byte>();
				var sprites = frame.Sprites;
				for (int i = 0; i < sprites.Length; i++) {
					var sprite = sprites[i];
					var name = sprites[i].name;
					int index = 1;
					while (renameMap.ContainsKey(name)) {
						name = sprite.name + "_" + index.ToString();
						index++;
					}
					renameMap.Add(name, 0);
					if (name != sprite.name) {
						sprite.name = name;
						sprites[i] = sprite;
					}
				}
			}
		}


		private string CreateExportFolder (TaskResult result) {
			string exportFolder = FolderForTask ? Util.CombinePaths(ExportingFolder, AseName) : ExportingFolder;
			if (SeparateEachLayer && FolderForLayer) {
				exportFolder = Util.CombinePaths(exportFolder, result.LayerName);
			}
			Util.CreateFolder(exportFolder);
			return exportFolder;
		}


		private void MakePNGFiles (List<Pair2<Texture2D, SpriteMetaData[]>> textureSpriteMetaList, string exportFolder) {
			foreach (var pair in textureSpriteMetaList) {
				var texture = pair.A;
				var metas = pair.B;
				var path = Util.CombinePaths(exportFolder, texture.name + ".png");

				// Delete Meta
				Util.DeleteFile(path + ".meta");

				// Create
				Util.ByteToFile(texture.EncodeToPNG(), path);
				AsePostprocessor.Add(path, new AsePostprocessor.TextureImportData() {
					PixelPerUnit = PixelPerUnit,
					SpriteMetas = metas,
				});
			}
		}


		private List<Pair4<string, string, float, string>[]> GetAnimationList (TaskResult result, List<Pair2<Texture2D, SpriteMetaData[]>> textureSpriteMetaList, string exportFolder) {
			var animationList = new List<Pair4<string, string, float, string>[]>(); // spriteName, duration, tag
			if (!CreateAnimation || textureSpriteMetaList == null || textureSpriteMetaList.Count == 0) { return animationList; }

			// Get Fixed Texture SpriteMeta List
			int frameCount = Mathf.Max(textureSpriteMetaList.Count, textureSpriteMetaList[0].B.Length);
			var FixedTextureSpriteMetas = new Pair2<Texture2D, SpriteMetaData>[frameCount];
			for (int i = 0; i < frameCount; i++) {
				var pair = new Pair2<Texture2D, SpriteMetaData>();
				var sourcePair = textureSpriteMetaList[Mathf.Clamp(i, 0, textureSpriteMetaList.Count - 1)];
				pair.A = sourcePair.A;
				pair.B = sourcePair.B[Mathf.Clamp(i, 0, sourcePair.B.Length - 1)];
				FixedTextureSpriteMetas[i] = pair;
			}

			// Get Ani List
			var pair4List = new List<Pair4<string, string, float, string>>();
			var realAnimationExportMode = AnimationExportMode;
			if (realAnimationExportMode == AnimationExportMode.OneAnimationPerTag && result.Tags.Length == 0) {
				realAnimationExportMode = AnimationExportMode.OneAnimationPerAse;
			}
			switch (realAnimationExportMode) {
				default:
				case AnimationExportMode.OneAnimationPerTag: {
					foreach (var tag in result.Tags) {
						pair4List.Clear();
						for (int i = tag.From; i <= tag.To && i >= 0 && i < frameCount && i < result.Durations.Length; i++) {
							var sourcePair = FixedTextureSpriteMetas[i];
							pair4List.Add(new Pair4<string, string, float, string>( // texturePath, spriteName, duration, tag
								Util.CombinePaths(exportFolder, sourcePair.A.name + ".png"),
								sourcePair.B.name,
								result.Durations[i],
								tag.Name
							));
						}
						animationList.Add(pair4List.ToArray());
					}
					break;
				}
				case AnimationExportMode.OneAnimationPerAse: {
					pair4List.Clear();
					for (int i = 0; i < frameCount && i < result.Durations.Length; i++) {
						var sourcePair = FixedTextureSpriteMetas[i];
						pair4List.Add(new Pair4<string, string, float, string>( // texturePath, spriteName, duration, tag
							Util.CombinePaths(exportFolder, sourcePair.A.name + ".png"),
							sourcePair.B.name,
							result.Durations[i],
							""
						));
					}
					animationList.Add(pair4List.ToArray());
					break;
				}
			}
			return animationList;
		}


		private List<string> MakeAnimationProcess (List<Pair4<string, string, float, string>[]> aniList, string exportFolder) {
			var resultList = new List<string>();
			if (!CreateAnimation) { return resultList; }
			// Make Animation Clip Files
			var nameStrategy_Animation = NamingStrategy_Animation[aniList.Count > 1 ? 1 : 0];
			var renameMap = new Dictionary<string, byte>();
			for (int aniIndex = 0; aniIndex < aniList.Count; aniIndex++) {
				var pair4s = aniList[aniIndex];
				if (pair4s == null || pair4s.Length == 0) { continue; }
				// One Animation
				var importPairList = new List<AsePostprocessor.AnimationFrameImportData>();
				for (int i = 0; i < pair4s.Length; i++) {
					var pair4 = pair4s[i]; // texturePath, spriteName, duration, tag
					importPairList.Add(new AsePostprocessor.AnimationFrameImportData() {
						TexturePath = pair4.A,
						SpriteName = pair4.B,
						Duration = pair4.C,
					});
				}
				// Ani Name
				var animationBasicName = string.Format(
					nameStrategy_Animation,
					AseName, aniIndex, "", pair4s[0].D, aniIndex
				);
				var animationName = animationBasicName;
				int aniNameIndex = 0;
				while (renameMap.ContainsKey(animationName)) {
					animationName = animationBasicName + "_" + aniNameIndex.ToString();
					aniNameIndex++;
				}
				renameMap.Add(animationName, 0);
				string path = Util.CombinePaths(exportFolder, animationName + ".anim");

				// Add to Postprocessor
				AsePostprocessor.Add(path, importPairList.ToArray());
				resultList.Add(path);
			}
			return resultList;
		}


		private void MakeAnimatorProcess (List<string> animationPaths, string exportFolder) {
			if (!CreateAnimation || !CreateAnimator) { return; }
			AsePostprocessor.Add(Util.CombinePaths(exportFolder, string.Format(
				NamingStrategy_Animator[0],
				AseName, "", "", "", "0"
			) + ".controller"), animationPaths.ToArray());
		}


		private void CreatePrefabs (List<Pair2<Texture2D, SpriteMetaData[]>> textureSpriteMetaList, string exportFolder) {
			if (!CreatePrefab) { return; }
			foreach (var pair in textureSpriteMetaList) {
				var texture = pair.A;
				var metas = pair.B;
				var rootPath = Util.CombinePaths(exportFolder, texture.name + " Prefab");
				if (!Util.DirectoryExists(rootPath)) {
					Util.CreateFolder(rootPath);
				}
				// Add to Postprocessor
				foreach (var meta in metas) {
					AsePostprocessor.Add(Util.CombinePaths(rootPath, meta.name + ".prefab"), new AsePostprocessor.PrefabImportData() {
						SpriteName = meta.name,
						TexturePath = Util.CombinePaths(exportFolder, texture.name + ".png"),
					});
				}
			}
		}


		private (string tag, int tagCount) GetTagInTags (TaskResult.TagData[] tags, int frameIndex) {
			string tag = "";
			int tagCount = 0;
			foreach (var t in tags) {
				if (frameIndex >= t.From && frameIndex <= t.To) {
					tag = t.Name;
					tagCount = frameIndex - t.From;
					break;
				}
			}
			return (tag, tagCount);
		}



	}
}