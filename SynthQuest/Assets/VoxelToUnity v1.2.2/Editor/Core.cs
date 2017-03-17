namespace Voxel2Unity {
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public enum Direction {
		Up = 0,
		Down = 1,
		Front = 2,
		Back = 3,
		Left = 4,
		Right = 5,
	}




	public enum Axis {
		X = 0,
		Y = 1,
		Z = 2,
	}




	public struct Voxel {

		public int ColorIndex {
			get {
				return colorIndex;
			}
			set {
				colorIndex = value;
			}
		}
		public bool IsEmpty {
			get {
				return ColorIndex == 0;
			}
		}

		public bool IsVisible {
			get {
				return visible != null && visible.Length > 5 && (visible[0] || visible[1] || visible[2] || visible[3] || visible[4] || visible[5]);
			}
			set {
				visible[0] = value;
				visible[1] = value;
				visible[2] = value;
				visible[3] = value;
				visible[4] = value;
				visible[5] = value;
			}
		}
		public bool AllVisible {
			get {
				return visible != null && visible.Length > 5 && (visible[0] && visible[1] && visible[2] && visible[3] && visible[4] && visible[5]);
			}
			set {
				visible[0] = value;
				visible[1] = value;
				visible[2] = value;
				visible[3] = value;
				visible[4] = value;
				visible[5] = value;
			}
		}

		public bool VisibleLeft {
			get {
				return visible[(int)Direction.Left];
			}
			set {
				visible[(int)Direction.Left] = value;
			}
		}
		public bool VisibleRight {
			get {
				return visible[(int)Direction.Right];
			}
			set {
				visible[(int)Direction.Right] = value;
			}
		}
		public bool VisibleUp {
			get {
				return visible[(int)Direction.Up];
			}
			set {
				visible[(int)Direction.Up] = value;
			}
		}
		public bool VisibleDown {
			get {
				return visible[(int)Direction.Down];
			}
			set {
				visible[(int)Direction.Down] = value;
			}
		}
		public bool VisibleFront {
			get {
				return visible[(int)Direction.Front];
			}
			set {
				visible[(int)Direction.Front] = value;
			}
		}
		public bool VisibleBack {
			get {
				return visible[(int)Direction.Back];
			}
			set {
				visible[(int)Direction.Back] = value;
			}
		}
		public bool[] Visible {
			get {
				return visible;
			}
		}

		private bool[] visible;
		private int colorIndex;

		public void Init () {
			colorIndex = 0;
			visible = new bool[6] { false, false, false, false, false, false };
		}

	}


	public struct VoxelFace {

		public Vector3 MaxPoint {
			get {
				switch (direction) {
					case Direction.Left:
					case Direction.Right:
						return new Vector3(X, Y + Width, Z + Height);
					case Direction.Front:
					case Direction.Back:
						return new Vector3(X + Width, Y, Z + Height);
					case Direction.Up:
					case Direction.Down:
						return new Vector3(X + Width, Y + Height, Z);
					default:
						return Vector3.zero;
				}
			}
		}
		public Vector2 MaxPoints2D {
			get {
				switch (direction) {
					case Direction.Left:
					case Direction.Right:
						return new Vector2(Y + Width, Z + Height);
					case Direction.Front:
					case Direction.Back:
						return new Vector2(X + Width, Z + Height);
					case Direction.Up:
					case Direction.Down:
						return new Vector2(X + Width, Y + Height);
					default:
						return Vector2.zero;
				}
			}
		}
		public Vector3[] Points {
			get {
				return new Vector3[4] {
				new Vector3(X, Z, Y),
				direction == Direction.Left || direction == Direction.Right ?
				new Vector3(X, Z, Y + Width) : new Vector3(X + Width, Z, Y),
				direction == Direction.Up || direction == Direction.Down ?
				new Vector3(X, Z, Y + Height) : new Vector3(X, Z+ Height, Y),
				direction == Direction.Left || direction == Direction.Right ?
				new Vector3(X, Z + Height ,Y + Width ):
				direction == Direction.Up || direction == Direction.Down ?
				new Vector3(X + Width, Z, Y + Height):
				new Vector3(X + Width, Z + Height, Y)
			};
			}
		}
		public int VoxelX {
			get {
				return direction == Direction.Right ? X - 1 : X;
			}
		}
		public int VoxelY {
			get {
				return direction == Direction.Back ? Y - 1 : Y;
			}
		}
		public int VoxelZ {
			get {
				return direction == Direction.Up ? Z - 1 : Z;
			}
		}

		public int X, Y, Z;
		public int Width;
		public int Height;
		public Direction direction;

		public VoxelFace (int x, int y, int z, int w, int h, Direction d) {
			X = d == Direction.Right ? x + 1 : x;
			Y = d == Direction.Back ? y + 1 : y;
			Z = d == Direction.Up ? z + 1 : z;
			Width = w;
			Height = h;
			direction = d;
		}

	}


	public class VoxelMesh {

		private Voxel[,,] Voxels;
		private VoxData MainVoxelData;
		private List<VoxelFace> VoxelFaces = new List<VoxelFace>();
		private int SizeX = 0, SizeY = 0, SizeZ = 0;
		private float Scale = 0.01f;
		private Vector3 Pivot = new Vector3(0.5f, 0f, 0.5f);
		private int MaxFacesInOneMesh = 16200;

		public void CreateVoxelMesh (VoxData voxelData, float scale, Vector3 pivot, ref Mesh[] meshs, ref Texture2D texture) {

			// Init Voxels
			InitVoxels(voxelData, scale, pivot);

			// Visibility
			FixVisible();

			// Faces
			Getfaces();

			// Mesh
			CreateMesh(voxelData, ref meshs, ref texture);

		}

		public void CreateVoxelMesh (VoxData voxelData, float scale, Vector3 pivot, ref Texture2D texture, ref string objFile) {

			// Init Voxels
			InitVoxels(voxelData, scale, pivot);

			// Visibility
			FixVisible();

			// Faces
			Getfaces();

			// Mesh
			Mesh[] meshs = null;
			CreateMesh(voxelData, ref meshs, ref texture);

			// Obj
			objFile = CreateObj(meshs, objFile);

		}


		private void InitVoxels (VoxData voxelData, float scale, Vector3 pivot) {
			Scale = scale;
			Pivot = pivot;
			SizeX = voxelData.SizeX;
			SizeY = voxelData.SizeY;
			SizeZ = voxelData.SizeZ;
			MainVoxelData = voxelData;
			Voxels = new Voxel[SizeX, SizeY, SizeZ];
			for (int i = 0; i < SizeX; i++) {
				for (int j = 0; j < SizeY; j++) {
					for (int k = 0; k < SizeZ; k++) {
						Voxels[i, j, k].Init();
						Voxels[i, j, k].ColorIndex = MainVoxelData.Voxels[i, j, k];
					}
				}
			}
		}


		private void FixVisible () {
			for (int i = 0; i < SizeX; i++) {
				for (int j = 0; j < SizeY; j++) {
					for (int k = 0; k < SizeZ; k++) {
						if (Voxels[i, j, k].IsEmpty) {
							Voxels[i, j, k].IsVisible = true;
							continue;
						}
						Voxels[i, j, k].VisibleLeft = i > 0 ? Voxels[i - 1, j, k].IsEmpty : true;
						Voxels[i, j, k].VisibleRight = i < SizeX - 1 ? Voxels[i + 1, j, k].IsEmpty : true;
						Voxels[i, j, k].VisibleFront = j > 0 ? Voxels[i, j - 1, k].IsEmpty : true;
						Voxels[i, j, k].VisibleBack = j < SizeY - 1 ? Voxels[i, j + 1, k].IsEmpty : true;
						Voxels[i, j, k].VisibleDown = k > 0 ? Voxels[i, j, k - 1].IsEmpty : true;
						Voxels[i, j, k].VisibleUp = k < SizeZ - 1 ? Voxels[i, j, k + 1].IsEmpty : true;
					}
				}
			}

		}


		private void Getfaces () {

			VoxelFaces.Clear();

			bool[,,,] isFixed = new bool[SizeX, SizeY, SizeZ, 6];

			int unFixedNum = SizeX * SizeY * SizeZ * 6;

			while (unFixedNum > 0) {
				for (int x = 0; x < SizeX; x++) {
					for (int y = 0; y < SizeY; y++) {
						for (int z = 0; z < SizeZ; z++) {
							for (int facing = 0; facing < 6; facing++) {

								if (isFixed[x, y, z, facing]) {
									continue;
								}

								if (Voxels[x, y, z].IsEmpty || !Voxels[x, y, z].Visible[facing]) {
									isFixed[x, y, z, facing] = true;
									unFixedNum--;
									continue;
								}

								isFixed[x, y, z, facing] = true;
								unFixedNum--;

								int minX = x, minY = y, minZ = z;
								int maxX = x, maxY = y, maxZ = z;

								#region --- Moving ---

								for (int moving = 0; moving < 6; moving++) {
									if (moving / 2 != facing / 2) {
										bool done = false;
										int temp;
										switch ((Direction)moving) {
											default:
												break;
											case Direction.Front:
												temp = minY;
												while (!done && minY > 0) {
													temp--;
													for (int i = minX; !done && i <= maxX; i++) {
														for (int k = minZ; k <= maxZ; k++) {
															if (isFixed[i, temp, k, facing] || Voxels[i, temp, k].IsEmpty || !Voxels[i, temp, k].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														minY = temp;
														for (int i = minX; i <= maxX; i++) {
															for (int k = minZ; k <= maxZ; k++) {
																if (!isFixed[i, temp, k, facing]) {
																	isFixed[i, temp, k, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
											case Direction.Back:
												temp = maxY;
												while (!done && maxY < SizeY - 1) {
													temp++;
													for (int i = minX; !done && i <= maxX; i++) {
														for (int k = minZ; k <= maxZ; k++) {
															if (isFixed[i, temp, k, facing] || Voxels[i, temp, k].IsEmpty || !Voxels[i, temp, k].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														maxY = temp;
														for (int i = minX; i <= maxX; i++) {
															for (int k = minZ; k <= maxZ; k++) {
																if (!isFixed[i, temp, k, facing]) {
																	isFixed[i, temp, k, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
											case Direction.Left:
												temp = minX;
												while (!done && minX > 0) {
													temp--;
													for (int j = minY; !done && j <= maxY; j++) {
														for (int k = minZ; k <= maxZ; k++) {
															if (isFixed[temp, j, k, facing] || Voxels[temp, j, k].IsEmpty || !Voxels[temp, j, k].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														minX = temp;
														for (int j = minY; j <= maxY; j++) {
															for (int k = minZ; k <= maxZ; k++) {
																if (!isFixed[temp, j, k, facing]) {
																	isFixed[temp, j, k, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
											case Direction.Right:
												temp = maxX;
												while (!done && maxX < SizeX - 1) {
													temp++;
													for (int j = minY; !done && j <= maxY; j++) {
														for (int k = minZ; k <= maxZ; k++) {
															if (isFixed[temp, j, k, facing] || Voxels[temp, j, k].IsEmpty || !Voxels[temp, j, k].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														maxX = temp;
														for (int j = minY; j <= maxY; j++) {
															for (int k = minZ; k <= maxZ; k++) {
																if (!isFixed[temp, j, k, facing]) {
																	isFixed[temp, j, k, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
											case Direction.Up:
												temp = maxZ;
												while (!done && maxZ < SizeZ - 1) {
													temp++;
													for (int i = minX; !done && i <= maxX; i++) {
														for (int j = minY; j <= maxY; j++) {
															if (isFixed[i, j, temp, facing] || Voxels[i, j, temp].IsEmpty || !Voxels[i, j, temp].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														maxZ = temp;
														for (int i = minX; i <= maxX; i++) {
															for (int j = minY; j <= maxY; j++) {
																if (!isFixed[i, j, temp, facing]) {
																	isFixed[i, j, temp, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
											case Direction.Down:
												temp = minZ;
												while (!done && minZ > 0) {
													temp--;
													for (int i = minX; !done && i <= maxX; i++) {
														for (int j = minY; j <= maxY; j++) {
															if (isFixed[i, j, temp, facing] || Voxels[i, j, temp].IsEmpty || !Voxels[i, j, temp].Visible[facing]) {
																done = true;
																break;
															}
														}
													}
													if (!done) {
														minZ = temp;
														for (int i = minX; i <= maxX; i++) {
															for (int j = minY; j <= maxY; j++) {
																if (!isFixed[i, j, temp, facing]) {
																	isFixed[i, j, temp, facing] = true;
																	unFixedNum--;
																}
															}
														}
													}
												}
												break;
										}
									}
								}

								#endregion

								// Add The Face

								VoxelFaces.Add(new VoxelFace(
									minX, minY, minZ,
									(Direction)facing == Direction.Left || (Direction)facing == Direction.Right ? maxY - minY + 1 : maxX - minX + 1,
									(Direction)facing == Direction.Up || (Direction)facing == Direction.Down ? maxY - minY + 1 : maxZ - minZ + 1,
									(Direction)facing
								));

							}
						}
					}
				}
			}

		}


		private void CreateMesh (VoxData voxelData, ref Mesh[] meshs, ref Texture2D aimTexture) {

			int num = VoxelFaces.Count / MaxFacesInOneMesh + 1;
			if (VoxelFaces.Count % MaxFacesInOneMesh == 0) {
				num--;
			}

			meshs = new Mesh[num];
			List<Dictionary<int, int>> uvIdMaps = new List<Dictionary<int, int>>();
			List<Texture2D[]> temptxtList = new List<Texture2D[]>();

			for (int index = 0; index < num; index++) {

				List<VoxelFace> Faces = new List<VoxelFace>(VoxelFaces.GetRange(index * MaxFacesInOneMesh, Mathf.Min(MaxFacesInOneMesh, VoxelFaces.Count - index * MaxFacesInOneMesh)));


				#region --- Vertices ---

				List<Vector3> voxelVertices = new List<Vector3>();

				for (int i = 0; i < Faces.Count; i++) {
					Vector3[] points = Faces[i].Points;
					for (int j = 0; j < 4; j++) {
						points[j] -= new Vector3(SizeX * Pivot.x, SizeZ * Pivot.y, SizeY * Pivot.z);
						points[j] *= Scale;
					}
					voxelVertices.AddRange(points);
				}

				#endregion


				#region --- Triangles ---

				List<int> voxelTriangles = new List<int>();

				for (int i = 0; i < Faces.Count; i++) {
					if (Faces[i].direction == Direction.Front || Faces[i].direction == Direction.Up || Faces[i].direction == Direction.Right) {
						voxelTriangles.AddRange(new int[6] {
						i * 4 + 2,
						i * 4 + 1,
						i * 4 + 0,
						i * 4 + 3,
						i * 4 + 1,
						i * 4 + 2
					});
					} else {
						voxelTriangles.AddRange(new int[6] {
						i * 4 + 2,
						i * 4 + 0,
						i * 4 + 1,
						i * 4 + 3,
						i * 4 + 2,
						i * 4 + 1
					});
					}
				}

				#endregion


				meshs[index] = new Mesh();
				meshs[index].Clear();
				meshs[index].SetVertices(voxelVertices);
				meshs[index].SetTriangles(voxelTriangles, 0);


				#region --- Texture ---


				List<Texture2D> tempTextureList = new List<Texture2D>();
				List<int> tempColorIndexs = new List<int>();
				Dictionary<int, int> UVidMap = new Dictionary<int, int>();


				for (int i = 0; i < Faces.Count; i++) {
					int width = Faces[i].Width + 2;
					int height = Faces[i].Height + 2;

					Color[] _colors = new Color[width * height];

					bool sameIndex = true;
					int currentIndex = -1;

					for (int u = 0; u < Faces[i].Width; u++) {
						for (int v = 0; v < Faces[i].Height; v++) {
							int colorIndex = 0;
							switch (Faces[i].direction) {
								case Direction.Front:
								case Direction.Back:
									colorIndex = Voxels[u + Faces[i].VoxelX, Faces[i].VoxelY, v + Faces[i].VoxelZ].ColorIndex - 1;
									break;
								case Direction.Left:
								case Direction.Right:
									colorIndex = Voxels[Faces[i].VoxelX, u + Faces[i].VoxelY, v + Faces[i].VoxelZ].ColorIndex - 1;
									break;
								case Direction.Up:
								case Direction.Down:
									colorIndex = Voxels[u + Faces[i].VoxelX, v + Faces[i].VoxelY, Faces[i].VoxelZ].ColorIndex - 1;
									break;
							}
							Color _c = MainVoxelData.Palatte[colorIndex];
							_colors[(v + 1) * width + u + 1] = _c;


							if (currentIndex == -1) {
								currentIndex = colorIndex;
							}
							if (sameIndex && currentIndex != colorIndex) {
								sameIndex = false;
							}


							#region --- Side ---

							if (u == 0) {
								_colors[(v + 1) * width + u] = _c;
							}

							if (u == Faces[i].Width - 1) {
								_colors[(v + 1) * width + u + 2] = _c;
							}

							if (v == 0) {
								_colors[v * width + u + 1] = _c;
							}

							if (v == Faces[i].Height - 1) {
								_colors[(v + 2) * width + u + 1] = _c;
							}

							if (u == 0 && v == 0) {
								_colors[0] = _c;
							}

							if (u == 0 && v == Faces[i].Height - 1) {
								_colors[(v + 2) * width + u] = _c;
							}

							if (u == Faces[i].Width - 1 && v == 0) {
								_colors[v * width + u + 2] = _c;
							}

							if (u == Faces[i].Width - 1 && v == Faces[i].Height - 1) {
								_colors[(v + 2) * width + u + 2] = _c;
							}

							#endregion

						}
					}

					Texture2D _texture = null;
					int oldID = i;
					if (sameIndex && currentIndex > 0) {
						if (tempColorIndexs.Contains(currentIndex)) {
							oldID = tempColorIndexs.IndexOf(currentIndex);
						} else {
							Color c = MainVoxelData.Palatte[currentIndex];
							_texture = new Texture2D(3, 3, TextureFormat.ARGB32, false, false);
							_texture.SetPixels(new Color[9] { c, c, c, c, c, c, c, c, c });
							tempTextureList.Add(_texture);
							tempColorIndexs.Add(currentIndex);
							oldID = tempTextureList.Count - 1;
						}
					} else {
						_texture = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
						_texture.SetPixels(_colors);
						tempTextureList.Add(_texture);
						tempColorIndexs.Add(-1);
						oldID = tempTextureList.Count - 1;
					}
					if (_texture) {
						_texture.wrapMode = TextureWrapMode.Clamp;
						_texture.filterMode = FilterMode.Point;
						_texture.mipMapBias = 0f;
						_texture.Apply();
					}
					UVidMap.Add(i, oldID);

				}

				temptxtList.Add(tempTextureList.ToArray());
				tempTextureList.Clear();
				uvIdMaps.Add(UVidMap);

				#endregion


			}

			// Combine Textures && Reset UV

			aimTexture = new Texture2D(1, 1);
			List<Texture2D> txtList = new List<Texture2D>();
			for (int i = 0; i < temptxtList.Count; i++) {
				txtList.AddRange(temptxtList[i]);
			}


			Rect[] tempUVs = aimTexture.PackTextures(txtList.ToArray(), 0);

			CutTexture(ref aimTexture, ref tempUVs);

			List<Rect> aimUVs = new List<Rect>(tempUVs);

			aimTexture.wrapMode = TextureWrapMode.Clamp;
			aimTexture.filterMode = FilterMode.Point;
			aimTexture.mipMapBias = 0f;
			aimTexture.Apply();


			#region --- UV ---

			int rectIndexOffset = 0;
			for (int index = 0; index < num; index++) {

				List<VoxelFace> Faces = new List<VoxelFace>(VoxelFaces.GetRange(
					index * MaxFacesInOneMesh,
					Mathf.Min(MaxFacesInOneMesh, VoxelFaces.Count - index * MaxFacesInOneMesh)
				));

				Rect[] rects = aimUVs.GetRange(
					rectIndexOffset,
					temptxtList[index].Length
				).ToArray();

				float gapX = 1f / aimTexture.width;
				float gapY = 1f / aimTexture.height;

				Vector2[] voxelUV = new Vector2[Faces.Count * 4];

				for (int i = 0; i < Faces.Count; i++) {
					int rectID = uvIdMaps[index][i];
					if (rectID >= rects.Length) {
						continue;
					}
					voxelUV[i * 4 + 0] = new Vector2(rects[rectID].xMin + gapX, rects[rectID].yMin + gapY);
					voxelUV[i * 4 + 1] = new Vector2(rects[rectID].xMax - gapX, rects[rectID].yMin + gapY);
					voxelUV[i * 4 + 2] = new Vector2(rects[rectID].xMin + gapX, rects[rectID].yMax - gapY);
					voxelUV[i * 4 + 3] = new Vector2(rects[rectID].xMax - gapX, rects[rectID].yMax - gapY);
				}
				uvIdMaps[index].Clear();

				rectIndexOffset += temptxtList[index].Length;

				meshs[index].uv = voxelUV;
				meshs[index].RecalculateNormals();
				
			}

			uvIdMaps.Clear();
			aimUVs.Clear();

			#endregion


		}


		private void CutTexture (ref Texture2D texture, ref Rect[] rects) {

			if (texture == null) {
				return;
			}

			float scaleX = 0f;
			float scaleY = 0f;

			for (int i = 0; i < rects.Length; i++) {
				scaleX = Mathf.Max(scaleX, rects[i].xMax);
				scaleY = Mathf.Max(scaleY, rects[i].yMax);
			}

			for (int i = 0; i < rects.Length; i++) {
				rects[i] = new Rect(
					rects[i].x / scaleX,
					rects[i].y / scaleY,
					rects[i].width / scaleX,
					rects[i].height / scaleY
				);
			}

			int newWidth = Mathf.CeilToInt(texture.width * scaleX);
			int newHeight = Mathf.CeilToInt(texture.height * scaleY);

			Texture2D newTexture = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false, false);
			newTexture.SetPixels(texture.GetPixels(0, 0, newWidth, newHeight));
			newTexture.wrapMode = TextureWrapMode.Clamp;
			newTexture.filterMode = FilterMode.Point;
			newTexture.mipMapBias = 0f;
			newTexture.Apply();
			texture = newTexture;

		}


		private string CreateObj (Mesh[] meshs, string name) {

			StringBuilder sb = new StringBuilder();
			sb.Append("mtllib " + name + ".mtl\n");
			sb.Append("usemtl " + name + "\n");

			int offset = 0;

			for (int index = 0; index < meshs.Length; index++) {

				Vector3[] vs = meshs[index].vertices;
				Vector2[] uvs = meshs[index].uv;
				int[] trs = meshs[index].triangles;

				sb.Append("g " + name + "_" + index.ToString() + "_\n");

				foreach (Vector3 v in vs) {
					sb.Append(string.Format("v {0} {1} {2}\n", -v.x, v.y, v.z));
				}
				sb.Append("\n");

				foreach (Vector3 v in uvs) {
					sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
				}
				sb.Append("\n");

				for (int i = 0; i < trs.Length; i += 3) {
					sb.Append(string.Format("f {2}/{2} {1}/{1} {0}/{0}\n",
						trs[i] + 1 + offset, trs[i + 1] + 1 + offset, trs[i + 2] + 1 + offset));
				}
				sb.Append("\n");

				offset += vs.Length;

			}
			return sb.ToString();
		}





	}
}