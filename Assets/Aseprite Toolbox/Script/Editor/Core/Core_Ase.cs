namespace AsepriteToolbox.Core {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;


	public class AseCore {




		#region --- VAR ---


		// Data
		private readonly AseData Data = null;


		// Config
		public UserPivotMode UserPivotMode = UserPivotMode.SlicePivotFirst;
		public PaddingMode PaddingMode = PaddingMode.ClearColor;
		public PaddingTarget PaddingTarget = PaddingTarget.FrameEdge;
		public Vector2 UserPivot = Vector2.one * 0.5f;
		public bool IgnoreBackgroundLayer = true;
		public bool VisibleLayerOnly = true;
		public bool TrimTexture = true;
		public bool TrimSprite = true;
		public bool CreateAnimation = false;
		public bool SeparateEachLayer = false;
		public bool IgnoreEmptySlice = true;
		public string AseName = "";


		#endregion




		#region --- API ---


		public AseCore (AseData data) {
			Data = data;
		}


		public TaskResult[] CreateResults () {
			if (Data == null) { return null; }
			if (SeparateEachLayer) {
				// Get Results
				var results = new List<TaskResult>();
				int layerCount = Data.GetLayerCount(false);
				for (int i = 0; i < layerCount; i++) {
					var result = CreateResultForTargetLayer(i);
					if (result == null) { continue; }
					results.Add(result);
				}
				// Set TaskLayerCount
				for (int i = 0; i < results.Count; i++) {
					results[i].TaskLayerIndex = i;
					results[i].TaskLayerCount = results.Count;
				}
				return results.ToArray();
			} else {
				var result = CreateResultForTargetLayer(-1);
				if (result == null) { return null; }
				result.TaskLayerIndex = 0;
				result.TaskLayerCount = 1;
				return new TaskResult[1] { result };
			}
		}


		#endregion




		#region --- LGC ---


		private TaskResult CreateResultForTargetLayer (int targetLayerIndex) {

			// Check
			if (Data == null) { return null; }

			var result = new TaskResult();

			// Layer Check
			int layerCount = Data.GetLayerCount(false);
			bool taskForAllLayers = targetLayerIndex < 0;
			List<AseData.LayerChunk> layers = null;
			if (targetLayerIndex >= 0 && targetLayerIndex < layerCount) {
				bool nullFlag = false;
				layers = Data.GetLayerChunks();
				var layerChunk = layers[targetLayerIndex];
				result.LayerName = layers != null ? layerChunk.Name : "";
				if (layerChunk.Type == 1) { return null; }
				if (IgnoreBackgroundLayer && layerChunk.CheckFlag(AseData.LayerChunk.LayerFlag.Background)) { nullFlag = true; }
				if (VisibleLayerOnly && !layerChunk.CheckFlag(AseData.LayerChunk.LayerFlag.Visible)) { nullFlag = true; }
				if (nullFlag) {
					return null;
				}
			}

			// Get Cels
			var cels = new AseData.CelChunk[layerCount, Data.FrameDatas.Count];
			for (int i = 0; i < Data.FrameDatas.Count; i++) {
				for (int l = 0; l < layerCount; l++) {
					cels[l, i] = null;
				}
				Data.ForAllChunks<AseData.CelChunk>(i, (chunk, chunkIndex) => {
					if (!taskForAllLayers && chunk.LayerIndex != targetLayerIndex) { return; }
					if (chunk.LayerIndex < 0 || chunk.LayerIndex >= layerCount) { return; }
					cels[chunk.LayerIndex, i] = chunk;
				});
			}

			// Get Frame Results
			result.Frames = new TaskResult.FrameResult[Data.FrameDatas.Count];
			float duration = 0f;
			for (int i = 0; i < Data.FrameDatas.Count; i++) {
				var fData = Data.FrameDatas[i];
				duration += fData.Header.FrameDuration / 1000f;
				if (!fData.AllCelsLinked()) {
					result.Frames[i] = GetFrameResult(cels, i, duration);
					duration = 0f;
				} else {
					result.Frames[i] = TaskResult.FrameResult.GetEmpty(i);
				}
			}

			// Get Tags
			var tagList = new List<TaskResult.TagData>();
			Data.ForAllChunks<AseData.FrameTagsChunk>((chunk, fIndex, cIndex) => {
				foreach (var tag in chunk.Tags) {
					tagList.Add(new TaskResult.TagData() {
						Name = tag.Name,
						From = tag.FromFrame,
						To = tag.ToFrame,
					});
				}
			});
			result.Tags = tagList.ToArray();

			// Get Durations
			var durations = new float[result.Frames.Length];
			for (int i = 0; i < result.Frames.Length; i++) {
				var frame = result.Frames[i];
				durations[i] = frame != null ? frame.Duration : 0f;
			}
			result.Durations = durations;

			return result;
		}


		private TaskResult.FrameResult GetFrameResult (AseData.CelChunk[,] cels, int frameIndex, float duration) {

			int width = Data.Header.Width;
			int height = Data.Header.Height;
			if (width <= 0 || height <= 0) {
				return TaskResult.FrameResult.GetEmpty(frameIndex);
			}
			ushort colorDepth = Data.Header.ColorDepth;
			var palette = colorDepth == 8 ? Data.GetPalette() : null;
			var layerChunks = Data.GetLayerChunks();

			// New Pixels
			var pixels = new Color[width * height];
			Color CLEAR = Color.clear;
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = CLEAR;
			}

			// Cels
			int layerCount = cels.GetLength(0);
			int frameCount = cels.GetLength(1);
			for (int i = 0; i < layerCount; i++) {
				var chunk = cels[i, frameIndex];
				if (chunk == null) { continue; }
				int chunkWidth = chunk.Width;
				int chunkHeight = chunk.Height;
				Color[] colors = null;
				if (chunk.Type == (ushort)AseData.CelChunk.CelType.Linked) {
					if (chunk.FramePosition >= 0 && chunk.FramePosition < frameCount) {
						var linkedChunk = cels[i, chunk.FramePosition];
						if (linkedChunk != null) {
							chunkWidth = linkedChunk.Width;
							chunkHeight = linkedChunk.Height;
							colors = linkedChunk.GetColors(colorDepth, palette);
						}
					}
				} else {
					colors = chunk.GetColors(colorDepth, palette);
				}
				if (colors == null) { continue; }
				var layerChunk = layerChunks[i];
				if (IgnoreBackgroundLayer && layerChunk.CheckFlag(AseData.LayerChunk.LayerFlag.Background)) { continue; }
				if (VisibleLayerOnly && !layerChunk.CheckFlag(AseData.LayerChunk.LayerFlag.Visible)) { continue; }
				if (colors.Length != chunkWidth * chunkHeight || pixels.Length != width * height) { continue; }
				// Overlap Color
				int offsetY = height - chunkHeight - chunk.Y;
				int minFromX = Mathf.Clamp(-chunk.X, 0, chunkWidth);
				int minFromY = Mathf.Clamp(-offsetY, 0, chunkHeight);
				int maxFromX = Mathf.Clamp(width - chunk.X, 0, chunkWidth);
				int maxFromY = Mathf.Clamp(height - offsetY, 0, chunkHeight);
				for (int y = minFromY; y < maxFromY; y++) {
					for (int x = minFromX; x < maxFromX; x++) {
						var fromC = colors[y * chunkWidth + x];
						if (fromC.a < 0.01f) { continue; }
						int toIndex = (y + offsetY) * width + x + chunk.X;
						pixels[toIndex] = layerChunk.MergeColor(fromC, pixels[toIndex]);
					}
				}
			}

			// Sprites
			var sprites = new List<SpriteMetaData>();
			Data.ForAllChunks<AseData.SliceChunk>((chunk, fIndex, cIndex) => {
				AseData.SliceChunk.SliceData sData = null;
				for (int i = 0; i < chunk.Slices.Length; i++) {
					var d = chunk.Slices[i];
					if (sData == null || frameIndex >= d.FrameIndex) {
						sData = d;
					} else if (frameIndex < d.FrameIndex) {
						break;
					}
				}
				if (sData != null) {
					// Rect
					var rect = new Rect(
						sData.X,
						height - sData.Y - sData.Height,
						sData.Width,
						sData.Height
					);
					// Empty Check
					if (IgnoreEmptySlice && EmptyCheck(pixels, width, height, rect)) { return; }
					// Add into Sprites
					sprites.Add(new SpriteMetaData() {
						name = CreateAnimation ? AseName : chunk.Name,
						rect = rect,
						border = chunk.CheckFlag(AseData.SliceChunk.SliceFlag.NinePatches) ? new Vector4(
							sData.CenterX,
							sData.Height - sData.CenterY - sData.CenterHeight,
							sData.Width - sData.CenterX - sData.CenterWidth,
							sData.CenterY
						) : Vector4.zero,
						pivot = UserPivotMode == UserPivotMode.SlicePivotFirst && chunk.CheckFlag(AseData.SliceChunk.SliceFlag.HasPivot) ? new Vector2(
							(sData.PivotX + UserPivot.x) / sData.Width,
							1f - (sData.PivotY + (1f - UserPivot.y)) / sData.Height
						) : UserPivot,
						alignment = 9,
					});
				}
			});

			// Trim Texture
			if (TrimTexture || TrimSprite) {
				int bottom, top, left, right;
				if (TrimSprite) {
					// Trim All Sprites
					for (int i = 0; i < sprites.Count; i++) {
						var sp = sprites[i];
						if (Util.GetTrimOffset(pixels, width, height, out bottom, out top, out left, out right, (int)sp.rect.xMin, (int)sp.rect.yMin, (int)sp.rect.xMax - 1, (int)sp.rect.yMax - 1)) {
							var oldRect = sp.rect;
							sp.rect = Rect.MinMaxRect(left, bottom, right + 1, top + 1);
							if (sp.rect.width > 0f && sp.rect.height > 0f) {
								sp.pivot = new Vector2(
									(oldRect.x + oldRect.width * sp.pivot.x - sp.rect.x) / sp.rect.width,
									(oldRect.y + oldRect.height * sp.pivot.y - sp.rect.y) / sp.rect.height
								);
							}
							sprites[i] = sp;
						}
					}
				}
				// Trim Texture
				if (TrimTexture && Util.GetTrimOffset(pixels, width, height, out bottom, out top, out left, out right)) {
					// Expend for Sprite
					foreach (var sprite in sprites) {
						top = Mathf.Max(top, (int)sprite.rect.yMax - 1);
						left = Mathf.Min(left, (int)sprite.rect.xMin);
						right = Mathf.Max(right, (int)sprite.rect.xMax - 1);
						bottom = Mathf.Min(bottom, (int)sprite.rect.yMin);
					}
					// Trim Texture
					int newWidth = right - left + 1;
					int newHeight = top - bottom + 1;
					var newPixels = new Color[newWidth * newHeight];
					// Pixels
					for (int x = 0; x < newWidth; x++) {
						for (int y = 0; y < newHeight; y++) {
							newPixels[y * newWidth + x] = pixels[(y + bottom) * width + x + left];
						}
					}
					// Sprites
					for (int i = 0; i < sprites.Count; i++) {
						var sprite = sprites[i];
						sprite.rect.x -= left;
						sprite.rect.y -= bottom;
						sprites[i] = sprite;
					}

					// Final
					pixels = newPixels;
					width = newWidth;
					height = newHeight;
				}
			}

			// Root Failback Sprite
			if (sprites.Count == 0) {
				int bottom = 0, top = height - 1, left = 0, right = width - 1;
				if (TrimSprite) {
					Util.GetTrimOffset(pixels, width, height, out bottom, out top, out left, out right);
				}
				sprites.Add(new SpriteMetaData() {
					name = AseName,
					rect = new Rect(left, bottom, right - left + 1, top - bottom + 1),
					alignment = 9,
					pivot = UserPivot,
					border = Vector4.zero,
				});
			}

			// Padding
			if (PaddingMode != PaddingMode.NoPadding) {

				// Frame
				if (PaddingTarget == PaddingTarget.FrameEdge || PaddingTarget == PaddingTarget.FrameAndSliceEdge) {
					int newWidth = width + 2;
					int newHeight = height + 2;
					var newPixels = new Color[newWidth * newHeight];
					// Pixels
					for (int x = 1; x < newWidth - 1; x++) {
						for (int y = 1; y < newHeight - 1; y++) {
							newPixels[y * newWidth + x] = pixels[(y - 1) * width + x - 1];
						}
					}
					// Padding Colors
					switch (PaddingMode) {
						case PaddingMode.ClearColor:
							// Clear
							var clearColor = Color.clear;
							for (int x = 0; x < newWidth; x++) {
								newPixels[x] = clearColor;
								newPixels[(newHeight - 1) * newWidth + x] = clearColor;
							}
							for (int y = 1; y < newHeight - 1; y++) {
								newPixels[y * newWidth] = clearColor;
								newPixels[y * newWidth + newWidth - 1] = clearColor;
							}
							break;
						case PaddingMode.NearbyColor:
							// Nearby
							for (int x = 0; x < newWidth; x++) {
								newPixels[x] = pixels[Mathf.Clamp(x - 1, 0, width - 1)];
								newPixels[(newHeight - 1) * newWidth + x] = pixels[(height - 1) * width + Mathf.Clamp(x - 1, 0, width - 1)];
							}
							for (int y = 1; y < newHeight - 1; y++) {
								newPixels[y * newWidth] = pixels[Mathf.Clamp(y - 1, 0, height - 1) * width];
								newPixels[y * newWidth + newWidth - 1] = pixels[Mathf.Clamp(y - 1, 0, height - 1) * width + width - 1];
							}
							break;
					}
					// Sprites
					for (int i = 0; i < sprites.Count; i++) {
						var sprite = sprites[i];
						sprite.rect.x++;
						sprite.rect.y++;
						sprites[i] = sprite;
					}
					// Final
					pixels = newPixels;
					width = newWidth;
					height = newHeight;
				}

				// Slice
				if (PaddingTarget == PaddingTarget.SliceEdge || PaddingTarget == PaddingTarget.FrameAndSliceEdge) {
					// Init InsideSlice
					var insideSlice = new bool[width, height];
					for (int x = 0; x < width; x++) {
						for (int y = 0; y < height; y++) {
							insideSlice[x, y] = false;
						}
					}
					int xMin, xMax, yMin, yMax;
					for (int i = 0; i < sprites.Count; i++) {
						var rect = sprites[i].rect;
						xMin = Mathf.Max(Mathf.RoundToInt(rect.xMin), 0);
						yMin = Mathf.Max(Mathf.RoundToInt(rect.yMin), 0);
						xMax = Mathf.Min(Mathf.RoundToInt(rect.xMax - 1), width - 1);
						yMax = Mathf.Min(Mathf.RoundToInt(rect.yMax - 1), height - 1);
						for (int x = xMin; x <= xMax; x++) {
							for (int y = yMin; y <= yMax; y++) {
								insideSlice[x, y] = true;
							}
						}
					}
					// Set Pixels
					int _x, _y;
					Color _c;
					var clearColor = Color.clear;
					bool usingClearColor = PaddingMode == PaddingMode.ClearColor;
					for (int i = 0; i < sprites.Count; i++) {
						var rect = sprites[i].rect;
						xMin = Mathf.Max(Mathf.RoundToInt(rect.xMin - 1), 0);
						yMin = Mathf.Max(Mathf.RoundToInt(rect.yMin - 1), 0);
						xMax = Mathf.Min(Mathf.RoundToInt(rect.xMax), width - 1);
						yMax = Mathf.Min(Mathf.RoundToInt(rect.yMax), height - 1);
						// Top Bottom
						for (int x = xMin; x <= xMax; x++) {
							_x = Mathf.Clamp(x, xMin + 1, xMax - 1);
							if (_x < 0 || _x >= width) { continue; }
							if (!insideSlice[x, yMin]) {
								_y = Mathf.Clamp(yMin + 1, 0, height - 1);
								if (_y >= 0 && _y < height) {
									_c = usingClearColor ? clearColor : pixels[_y * width + _x];
									if (_c.a > 0.01f) {
										pixels[yMin * width + x] = _c;
									}
								}
							}
							if (!insideSlice[x, yMax]) {
								_y = Mathf.Clamp(yMax - 1, 0, height - 1);
								if (_y >= 0 && _y < height) {
									_c = usingClearColor ? clearColor : pixels[_y * width + _x];
									if (_c.a > 0.01f) {
										pixels[yMax * width + x] = _c;
									}
								}
							}
						}
						// Left Right
						for (int y = yMin; y <= yMax; y++) {
							_y = Mathf.Clamp(y, yMin + 1, yMax - 1);
							if (_y < 0 || _y >= height) { continue; }
							if (!insideSlice[xMin, y]) {
								_x = Mathf.Clamp(xMin + 1, 0, width - 1);
								if (_x >= 0 && _x < width) {
									_c = usingClearColor ? clearColor : pixels[_y * width + _x];
									if (_c.a > 0.01f) {
										pixels[y * width + xMin] = _c;
									}
								}
							}
							if (!insideSlice[xMax, y]) {
								_x = Mathf.Clamp(xMax - 1, 0, width - 1);
								if (_x >= 0 && _x < width) {
									_c = usingClearColor ? clearColor : pixels[_y * width + _x];
									if (_c.a > 0.01f) {
										pixels[y * width + xMax] = _c;
									}
								}
							}
						}
					}


				}
			}

			// Final
			return new TaskResult.FrameResult() {
				Tag = Data.GetTagIn(frameIndex),
				FrameIndex = frameIndex,
				Pixels = pixels,
				Sprites = sprites.ToArray(),
				Duration = duration,
				Width = width,
				Height = height,
			};
		}


		private bool EmptyCheck (Color[] pixels, int width, int height, Rect rect) {
			for (int x = (int)rect.xMin; x < rect.xMax; x++) {
				if (x < 0 || x >= width) { continue; }
				for (int y = (int)rect.y; y < rect.yMax; y++) {
					if (y < 0 || y >= height) { continue; }
					if (pixels[y * width + x].a > 0.001f) {
						return false;
					}
				}
			}
			return true;
		}


		#endregion




	}
}