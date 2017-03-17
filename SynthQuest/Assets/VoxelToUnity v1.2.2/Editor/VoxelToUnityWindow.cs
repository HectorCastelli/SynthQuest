namespace Voxel2Unity {

	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.IO;
	using System.Collections.Generic;

	public class VoxelToUnityWindow : EditorWindow {



		#region --- SUB ---



		public enum TaskType {
			CreatePrefab = 0,
			CreateLOD = 1,
			CreateOBJ = 2,
			Flip = 3,
			Rotate = 4,
		}


		public enum ErrorType {
			None = 0,
			FileReading = 1,
			VoxelData = 2,
			MeshGeneration = 3,
		}


		public struct PathData {
			public string ObjPath;
			public string ExportRoot;
			public PathData (string path, string expRoot) {
				ObjPath = path;
				ExportRoot = expRoot;
			}
		}



		#endregion



		#region --- VAR ---

		// Const
		private const string MAIN_TITLE = "Voxel to Unity";
		private const string MAIN_TITLE_RICH = "<color=#ff3333>V</color><color=#ffcc00>o</color><color=#ffff33>x</color><color=#33ff33>e</color><color=#33ffff>l</color><color=#eeeeee> to Unity</color>";

		// Shot Cut
		public static Shader TheShader {
			get {
				return Shader.Find(ShaderPath);
			}
			set {
				ShaderPath = value.name;
			}
		}

		private static bool AnyVoxelFileSelected {
			get {
				return SelectingVoxNum > 0 || SelectingQbNum > 0;
			}
		}

		private static int SelectingVoxelFileNum {
			get {
				return SelectingVoxNum + SelectingQbNum;
			}
		}

		private static Vector3 ModelPivot {
			get {
				return new Vector3(ModelPivotX, ModelPivotY, ModelPivotZ);
			}
			set {
				ModelPivotX = value.x;
				ModelPivotY = value.y;
				ModelPivotZ = value.z;
			}
		}


		// Data
		private static Dictionary<Object, PathData> SeletingObjAndPaths = new Dictionary<Object, PathData>();
		private static Texture2D VoxFileIcon = null;
		private static Texture2D QbFileIcon = null;
		private static Texture2D DirFileIcon = null;
		private static Vector2 MasterScrollPosition = Vector2.zero;
		private static int SelectingVoxNum = 0;
		private static int SelectingQbNum = 0;
		private static int SelectingDirNum = 0;
		private static int SelectingObjNum = 0;


		//Saving Data
		private static string ShaderPath = "Mobile/Diffuse";
		private static string ExportPath = "Assets";
		private static float ModelScale = 0.1f;
		private static float ModelPivotX = 0.5f;
		private static float ModelPivotY = 0f;
		private static float ModelPivotZ = 0.5f;
		private static bool SelectingFilePannelOpen = true;
		private static bool CreatePannelOpen = true;
		private static bool ToolsPannelOpen = false;
		private static bool SettingPannelOpen = false;
		private static bool ModelGenerationSettingPannelOpen = false;
		private static bool SystemSettingPannelOpen = false;
		private static bool ColorfulUI = false;
		private static bool ColorfulTitle = true;
		private static bool LogMessage = true;
		private static bool ShowDialog = true;
		private static int LodNum = 3;

		#endregion



		#region --- MSG ---




		#region --- GUI ---


		[MenuItem("Tools/Voxel to Unity")]
		public static void OpenWindow () {
			VoxelToUnityWindow window = GetWindow<VoxelToUnityWindow>("Voxel To Unity", true, typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
			window.minSize = new Vector2(275, 400);
			window.maxSize = new Vector2(600, 420);

		}



		public void OnEnable () {
			EditorLoad();
			FixSelectingFile();
			Repaint();
		}


		public void OnGUI () {


			MasterScrollPosition = GUILayout.BeginScrollView(MasterScrollPosition, GUI.skin.scrollView);


			TitleGUI();


			SelectingFileGUI();


			CreateGUI();


			ToolsGUI();


			SettingGUI();


			// System
			if (GUI.changed) {
				EditorSave();
			}

			GUILayout.EndScrollView();

		}



		void OnFocus () {
			FixSelectingFile();
			Repaint();
		}


		void OnSelectionChange () {
			FixSelectingFile();
			Repaint();
		}


		#endregion




		#region --- SUB ---




		void TitleGUI () {
			Space(6);
			LayoutV(() => {
				GUIStyle style = new GUIStyle();
				style.alignment = TextAnchor.LowerCenter;
				style.fontSize = 12;
				style.fontStyle = FontStyle.Bold;
				style.normal.textColor = Color.white;
				style.richText = true;
				Rect rect = GUIRect(0, 18, true);

				GUIStyle shadowStyle = new GUIStyle(style);
				shadowStyle.richText = false;

				EditorGUI.DropShadowLabel(rect, MAIN_TITLE, shadowStyle);
				GUI.Label(rect, ColorfulTitle ? MAIN_TITLE_RICH : MAIN_TITLE, style);

			});
			Space(6);
		}




		void SelectingFileGUI () {

			LayoutF(() => {

				bool addSpaceFlag = true;
				Space(2);
				int iconSize = 26;

				LayoutH(() => {

					// Init
					GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
					labelStyle.alignment = TextAnchor.MiddleLeft;
					labelStyle.fontSize = 10;

					// Icons
					if (SelectingDirNum > 0) {
						if (SelectingVoxNum + SelectingQbNum > 0) {
							// Dir
							LayoutH(() => {
								GUI.DrawTexture(GUIRect(iconSize, iconSize), DirFileIcon);
							}, true);
							GUI.Label(GUIRect(0, iconSize, true), "folder\n× " + SelectingDirNum.ToString(), labelStyle);
						} else {
							// None With Folder
							EditorGUI.HelpBox(GUIRect(0, iconSize + 14, true), "There are NO .vox or .qb file in selecting folder.", MessageType.Warning);
							addSpaceFlag = false;
						}
					} else if (SelectingVoxNum + SelectingQbNum <= 0) {
						if (SelectingObjNum > 0) {
							// Selecting Not Voxel File
							EditorGUI.HelpBox(GUIRect(0, iconSize + 14, true), "The file selecting is NOT .vox or .qb file.", MessageType.Warning);
							addSpaceFlag = false;
						} else {
							// None
							EditorGUI.HelpBox(GUIRect(0, iconSize + 14, true), "Select *.vox, *.qb or folder in Project View.", MessageType.Info);
							addSpaceFlag = false;
						}
					}

					Space(4);

					if (SelectingVoxNum > 0) {
						// Vox
						LayoutH(() => {
							if (VoxFileIcon) {
								GUI.DrawTexture(GUIRect(iconSize, iconSize), VoxFileIcon);
							}
						}, true);
						GUI.Label(GUIRect(0, iconSize, true), ".vox\n× " + SelectingVoxNum.ToString(), labelStyle);
						Space(4);
					}

					if (SelectingQbNum > 0) {
						// Qb
						LayoutH(() => {
							if (QbFileIcon) {
								GUI.DrawTexture(GUIRect(iconSize, iconSize), QbFileIcon);
							}
						}, true);
						GUI.Label(GUIRect(0, iconSize, true), ".qb\n× " + SelectingQbNum.ToString(), labelStyle);
					}

				});
				Space(4);

				// Scale Too Small Warning
				if (ModelScale == 0) {
					EditorGUI.HelpBox(GUIRect(0, iconSize + 14, true), "Model scale is 0. Your model will be invisible.", MessageType.Error);
				} else if (ModelScale <= 0.0001f) {
					EditorGUI.HelpBox(GUIRect(0, iconSize + 14, true), "Model scale is too small. Your may not able to see them.", MessageType.Warning);
				}

				Space(addSpaceFlag ? 14 : 6);
			}, "Selecting Files", ref SelectingFilePannelOpen, true, 2);
			Space(4);
		}




		void CreateGUI () {
			LayoutF(() => {
				bool oldEnable = GUI.enabled;
				GUI.enabled = AnyVoxelFileSelected;
				int buttonHeight = 34;
				Space(6);


				if (GUI.Button(
					GUIRect(0, buttonHeight, true),
					"Create Prefab" + TheS(SelectingVoxelFileNum)
				)) {
					// Create Prefab
					TaskForAll(TaskType.CreatePrefab);
				}
				Space(6);
				if (GUI.Button(
					GUIRect(0, buttonHeight, true),
					"Create LOD Prefab" + TheS(SelectingVoxelFileNum)
				)) {
					// Create LOD Prefab
					TaskForAll(TaskType.CreateLOD);
				}
				Space(6);
				if (GUI.Button(
					GUIRect(0, buttonHeight, true),
					"Create Obj File" + TheS(SelectingVoxelFileNum)
				)) {
					// Create Obj File
					TaskForAll(TaskType.CreateOBJ);
				}
				Space(6);

				// Export To

				LayoutV(() => {
					GUI.enabled = true;
					GUI.Label(GUIRect(0, 18, true), "Export To:");
					GUI.enabled = oldEnable;
					Space(4);
					LayoutH(() => {
						Space(6);
						EditorGUI.SelectableLabel(GUIRect(0, 18, true), ExportPath, GUI.skin.textField);
						if (GUI.Button(GUIRect(60, 18), "Browse", EditorStyles.miniButtonMid)) {
							string newPath = Util.FixPath(EditorUtility.OpenFolderPanel("Select Export Path", ExportPath, ""));
							if (!string.IsNullOrEmpty(newPath)) {
								newPath = Util.RelativePath(newPath);
								if (!string.IsNullOrEmpty(newPath)) {
									ExportPath = newPath;
								} else {
									Util.Dialog("Warning", "Export path must in Assets folder.", "OK");
								}
							}
						}
					});
					GUI.enabled = oldEnable;
					Space(4);
				}, true, 3);

			}, "Create", ref CreatePannelOpen, true, 3);
			Space(4);
		}




		void ToolsGUI () {
			LayoutF(() => {
				bool oldEnable = GUI.enabled;
				GUI.enabled = AnyVoxelFileSelected;
				Space(2);
				// Init
				GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
				labelStyle.alignment = TextAnchor.MiddleCenter;
				labelStyle.fontSize = 12;
				// Logic
				LayoutH(() => {
					int buttonHeight = 24;
					// Flip
					LayoutV(() => {
						GUI.Label(GUIRect(0, buttonHeight, true), "Flip", labelStyle);
						Space(2);
						LayoutH(() => {
							if (GUI.Button(GUIRect(0, buttonHeight, true), "X")) {
								TaskForAll(TaskType.Flip, Axis.X);
							}
							if (GUI.Button(GUIRect(0, buttonHeight, true), "Y")) {
								TaskForAll(TaskType.Flip, Axis.Z);
							}
							if (GUI.Button(GUIRect(0, buttonHeight, true), "Z")) {
								TaskForAll(TaskType.Flip, Axis.Y);
							}
						});
					});
					Space(8);
					// Rot
					LayoutV(() => {
						GUI.Label(GUIRect(0, buttonHeight, true), "Rotate", labelStyle);
						Space(2);
						LayoutH(() => {
							if (GUI.Button(GUIRect(0, buttonHeight, true), "X")) {
								TaskForAll(TaskType.Rotate, Axis.X);
							}
							if (GUI.Button(GUIRect(0, buttonHeight, true), "Y")) {
								TaskForAll(TaskType.Rotate, Axis.Z);
							}
							if (GUI.Button(GUIRect(0, buttonHeight, true), "Z")) {
								TaskForAll(TaskType.Rotate, Axis.Y);
							}
						});
					});
				});
				Space(4);
				GUI.enabled = oldEnable;
			}, "Tools", ref ToolsPannelOpen, true, 4);
			Space(4);
		}




		void SettingGUI () {
			LayoutF(() => {

				int fieldWidth = 65;
				int itemHeight = 16;

				Space(2);

				LayoutF(() => {

					// Model Generation

					Space(4);

					// Pivot
					LayoutH(() => {
						GUI.Label(GUIRect(fieldWidth, itemHeight), "Pivot");
						Space(2);
						ModelPivot = EditorGUI.Vector3Field(GUIRect(0, itemHeight, true), "", ModelPivot);
					});

					Space(4);

					// Scale
					ModelScale = Mathf.Max(EditorGUI.FloatField(GUIRect(0, itemHeight, true), "Scale", ModelScale), 0f);


					Space(4);

					// LOD
					LodNum = Mathf.Clamp(EditorGUI.IntField(GUIRect(0, itemHeight, true), "LOD Num", LodNum), 2, 9);

					Space(4);

					// Shader
					TheShader = (Shader)EditorGUI.ObjectField(GUIRect(0, 16, true), "Shader", TheShader, typeof(Shader), false);

					Space(4);

				}, "Model Generation", ref ModelGenerationSettingPannelOpen, true);
				Space(4);
				LayoutF(() => {
					// System
					Space(2);
					LayoutH(() => {
						LogMessage = EditorGUI.Toggle(GUIRect(itemHeight, itemHeight), LogMessage);
						GUI.Label(GUIRect(0, 18, true), "Log To Console");
						Space(2);
						ShowDialog = EditorGUI.Toggle(GUIRect(itemHeight, itemHeight), ShowDialog);
						GUI.Label(GUIRect(0, 18, true), "Dialog Window");
					});
					Space(2);
					LayoutH(() => {
						ColorfulTitle = EditorGUI.Toggle(GUIRect(itemHeight, itemHeight), ColorfulTitle);
						GUI.Label(GUIRect(0, 18, true), "Colorful Title");
						Space(2);
						ColorfulUI = EditorGUI.Toggle(GUIRect(itemHeight, itemHeight), ColorfulUI);
						GUI.Label(GUIRect(0, 18, true), "Colorful UI");
					});
					Space(2);
				}, "System", ref SystemSettingPannelOpen, true);
				Space(4);
			}, "Setting", ref SettingPannelOpen, true, 5);
			Space(4);
		}


		

		#endregion




		#endregion



		#region --- API ---



		// Main
		public void TaskForAll (TaskType task, Axis axis = Axis.X) {

			int fileErrorNum = 0;
			int voxDataErrorNum = 0;
			int meshCreateErrorNum = 0;
			int doneNum = 0;

			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();

			int currentIndex = 0;
			ForEachSelecting((pathData) => {
				ErrorType error = ErrorType.None;
				if (task == TaskType.CreatePrefab || task == TaskType.CreateLOD || task == TaskType.CreateOBJ) {
					switch (task) {
						case TaskType.CreatePrefab:
							error = CreatePrefab(pathData, currentIndex);
							break;
						case TaskType.CreateLOD:
							error = CreateLODPrefab(pathData, currentIndex);
							break;
						case TaskType.CreateOBJ:
							error = CreateObj(pathData, currentIndex);
							break;
					}
				} else if (task == TaskType.Flip || task == TaskType.Rotate) {
					error = TryFlipOrRotate(pathData, task, axis);
				}

				switch (error) {
					default:
					case ErrorType.None:
						doneNum++;
						break;
					case ErrorType.FileReading:
						fileErrorNum++;
						break;
					case ErrorType.VoxelData:
						voxDataErrorNum++;
						break;
					case ErrorType.MeshGeneration:
						meshCreateErrorNum++;
						break;
				}

				currentIndex++;

			});

			watch.Stop();
			string secondStr = watch.Elapsed.TotalSeconds.ToString("0.00");
			Util.ClearProgressBar();

			// Error Handling
			string msg = "";
			if (fileErrorNum > 0) {
				msg += "[File Reading Error] " + fileErrorNum.ToString() + " file" + TheS(fileErrorNum) + " can NOT read.\n";
			}
			if (voxDataErrorNum > 0) {
				msg += "[Vox Data Error] " + voxDataErrorNum.ToString() + " file" + TheS(voxDataErrorNum) + " is NOT voxel format file.\n";
			}
			if (meshCreateErrorNum > 0) {
				msg += "[Mesh Generation Error] Failed to create mesh for " + meshCreateErrorNum.ToString() + " file" + TheS(meshCreateErrorNum) + ".\n";
			}
			if (!string.IsNullOrEmpty(msg)) {
				Util.Dialog("Warning", msg, "OK");
			}

			// Done Msg
			if (doneNum > 0) {
				string successMsg = string.Format(
					" Success. {0} {1}{2} {3} in {4}s.",
					doneNum,
					task == TaskType.CreateOBJ ? "obj file" : task == TaskType.CreatePrefab || task == TaskType.CreateLOD ? "prefab" : "voxel file",
					TheS(doneNum),
					task == TaskType.CreatePrefab || task == TaskType.CreateLOD || task == TaskType.CreateOBJ ? "created" : task == TaskType.Flip ? "fliped" : "rotated",
					secondStr
				);
				if (LogMessage) {
					Debug.Log("[" + MAIN_TITLE + "] " + successMsg);
				}
				if (ShowDialog) {
					Util.Dialog("Success", successMsg, "OK");
				}

			}

			// Final
			
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			Postprocessor.ClearQueue();
			AssetDatabase.SaveAssets();
			
			FixSelectingFile();
			Repaint();

		}




		public ErrorType TryFlipOrRotate (PathData pathData, TaskType task, Axis axis) {

			if (task != TaskType.Rotate && task != TaskType.Flip) {
				return ErrorType.None;
			}

			string objPath = pathData.ObjPath;
			string ex = Util.GetExtension(objPath);
			
			// Read File 
			byte[] bytes = Util.FileToByte(objPath);
			if (bytes == null) {
				return ErrorType.FileReading;
			}


			// File --> Voxel Data
			if (ex == ".vox") {
				VoxData data = null;
				data = VoxFile.LoadVoxel(bytes);
				if (data == null) {
					return ErrorType.VoxelData;
				}
				// Voxel Data --> Fliped Voxel Data
				if (task == TaskType.Flip) {
					data.Flip(axis);
				} else {
					data.Rotate(axis);
				}
				// Fliped Voxel Data --> File
				byte[] newBytes = VoxFile.GetMainByte(data);
				if (newBytes == null) {
					return ErrorType.FileReading;
				}
				Util.ByteToFile(newBytes, objPath);
			} else {
				QbData qbData = null;
				qbData = QbFile.LoadQb(bytes);
				if (qbData == null) {
					return ErrorType.VoxelData;
				}
				// Qb Data --> Fliped Voxel Data
				if (task == TaskType.Flip) {
					qbData.Flip(axis);
				} else {
					qbData.Rotate(axis);
				}
				// Fliped Voxel Data --> File
				byte[] newBytes = QbFile.GetQbByte(qbData);
				if (newBytes == null) {
					return ErrorType.FileReading;
				}
				Util.ByteToFile(newBytes, objPath);
			}

			return ErrorType.None;
		}





		public ErrorType CreatePrefab (PathData pathData, int currentIndex) {

			string objPath = pathData.ObjPath;
			string expPath = pathData.ExportRoot;
			string _ext = Util.GetExtension(objPath);
			string name = Util.GetName(objPath);

			if (_ext == ".vox" || _ext == ".qb") {

				VoxData _chunk;

				if (_ext == ".vox") {
					_chunk = VoxFile.LoadVoxel(objPath);
				} else {
					_chunk = QbFile.LoadQb(objPath).GetVoxData();
				}

				if (_chunk != null) {

					Mesh[] _meshs = null;
					Texture2D _texture = null;
					VoxelMesh vMesh = new VoxelMesh();

					EditorUtility.DisplayProgressBar(
						"Hold On...",
						string.Format(
							"Creating voxel model: {0} ({1} / {2})",
							Path.GetFileNameWithoutExtension(objPath),
							currentIndex,
							SelectingVoxelFileNum
						),
						(float)(currentIndex) / (float)(SelectingVoxelFileNum)
					);

					try {
						// Doing the magic stuff
						vMesh.CreateVoxelMesh(_chunk, ModelScale, ModelPivot, ref _meshs, ref _texture);
						// magic stuff done
					} catch {
						return ErrorType.MeshGeneration;
					}

					string fileName = name;

					#region --- Check ---

					bool failed = false;

					if (_meshs == null || _texture == null || vMesh == null) {
						failed = true;
					} else {
						foreach (Mesh m in _meshs) {
							if (!m) {
								failed = true;
								break;
							}
						}
					}

					if (failed) {
						return ErrorType.MeshGeneration;
					}

					#endregion

					int num = _meshs.Length;

					GameObject _prefab = new GameObject();
					GameObject[] _models = new GameObject[num];
					Material _mat = new Material(TheShader);
					_texture.name = "Texture";
					_mat.mainTexture = _texture;
					_mat.name = "Material";
					_prefab.name = fileName;

					for (int i = 0; i < num; i++) {
						_models[i] = new GameObject("Model_" + i);
						_models[i].transform.SetParent(_prefab.transform);
						MeshFilter _meshFilter = _models[i].AddComponent<MeshFilter>();
						MeshRenderer _meshRenderer = _models[i].AddComponent<MeshRenderer>();
						_meshs[i].name = "Mesh_" + i;
						_meshFilter.mesh = _meshs[i];
						_meshRenderer.material = _mat;
					}

					string _newPath = Util.CombinePaths(expPath, name + ".prefab");
					string _parentPath = Util.RelativePath(new FileInfo(_newPath).Directory.FullName);

					Util.CreateFolder(_parentPath);
					Object _assetPrefab;
					if (File.Exists(_newPath)) {
						_assetPrefab = AssetDatabase.LoadAssetAtPath<Object>(_newPath);
						Object[] things = AssetDatabase.LoadAllAssetRepresentationsAtPath(_newPath);
						foreach (Object o in things) {
							DestroyImmediate(o, true);
						}
					} else {
						_assetPrefab = PrefabUtility.CreateEmptyPrefab(_newPath);
					}

					AssetDatabase.AddObjectToAsset(_mat, _newPath);
					AssetDatabase.AddObjectToAsset(_texture, _newPath);
					for (int i = 0; i < num; i++) {
						AssetDatabase.AddObjectToAsset(_meshs[i], _newPath);
					}
					PrefabUtility.ReplacePrefab(_prefab, _assetPrefab, ReplacePrefabOptions.ReplaceNameBased);

					DestroyImmediate(_prefab, false);
					for (int i = 0; i < num; i++) {
						DestroyImmediate(_models[i], false);
					}

					vMesh = null;

				} else {
					return ErrorType.VoxelData;
				}

			} else {
				return ErrorType.FileReading;
			}


			return ErrorType.None;

		}






		public ErrorType CreateLODPrefab (PathData pathData, int currentIndex) {

			string objPath = pathData.ObjPath;
			string expPath = pathData.ExportRoot;
			string _ext = Util.GetExtension(objPath);
			string name = Util.GetName(objPath);

			if (_ext == ".vox" || _ext == ".qb") {

				VoxData _chunk;

				if (_ext == ".vox") {
					_chunk = VoxFile.LoadVoxel(objPath);
				} else {
					_chunk = QbFile.LoadQb(objPath).GetVoxData();
				}

				if (_chunk != null) {

					EditorUtility.DisplayProgressBar(
						"Hold On...",
						string.Format(
							"Creating voxel model: {0} ({1} / {2})",
							Path.GetFileNameWithoutExtension(objPath),
							currentIndex,
							SelectingVoxelFileNum
						),
						(float)(currentIndex) / (float)SelectingVoxelFileNum
					);

					List<Mesh[]> aimMeshs = new List<Mesh[]>();
					List<Texture2D> aimTexture = new List<Texture2D>();
					int[] lodModelSize = new int[LodNum];

					// Mesh

					for (int i = 0; i < LodNum; i++) {
						Mesh[] _meshs = null;
						Texture2D _texture = null;
						VoxelMesh vMesh = new VoxelMesh();
						VoxData _currentChunk = i == 0 ? _chunk : _chunk.GetLODVoxelData(i + 1);
						if (_currentChunk == null) {
							break;
						}
						lodModelSize[i] = Mathf.Max(_chunk.SizeX, _chunk.SizeY, _chunk.SizeZ);
						float scaleOffset = (float)(lodModelSize[i]) / (float)Mathf.Max(_currentChunk.SizeX, _currentChunk.SizeY, _currentChunk.SizeZ);
						try {
							vMesh.CreateVoxelMesh(_currentChunk, ModelScale * scaleOffset, ModelPivot, ref _meshs, ref _texture);
						} catch {
							return ErrorType.VoxelData;
						}
						aimMeshs.Add(_meshs);
						aimTexture.Add(_texture);
						vMesh = null;
					}

					string fileName = Path.GetFileNameWithoutExtension(objPath);


					#region --- Check ---

					bool failed = false;
					if (aimMeshs.Count != aimTexture.Count) {
						failed = true;
					} else {
						for (int i = 0; i < aimMeshs.Count; i++) {
							Mesh[] _meshs = aimMeshs[i];
							Texture2D _texture = aimTexture[i];
							if (_meshs == null || _texture == null) {
								failed = true;
							} else {
								foreach (Mesh m in _meshs) {
									if (!m) {
										failed = true;
										break;
									}
								}
							}
						}
					}

					if (failed) {
						return ErrorType.MeshGeneration;
					}

					#endregion


					int currentLodNum = aimMeshs.Count;
					GameObject aimPrefab = new GameObject();
					List<Material> aimMats = new List<Material>();
					List<Renderer[]> meshRenderers = new List<Renderer[]>();

					for (int lod = 0; lod < currentLodNum; lod++) {
						Mesh[] _meshs = aimMeshs[lod];
						Texture2D _texture = aimTexture[lod];
						int num = _meshs.Length;

						GameObject _lodPrefab = new GameObject();
						GameObject[] _models = new GameObject[num];
						Material _mat = new Material(TheShader);
						_texture.name = "Texture" + "_LOD_" + lod;
						_mat.mainTexture = _texture;
						_mat.name = "Material" + "_LOD_" + lod;
						Renderer[] _renderers = new Renderer[num];

						for (int i = 0; i < num; i++) {
							_models[i] = new GameObject("Model_" + i);
							_models[i].transform.SetParent(_lodPrefab.transform);
							MeshFilter _meshFilter = _models[i].AddComponent<MeshFilter>();
							_renderers[i] = _models[i].AddComponent<MeshRenderer>();
							_meshs[i].name = "Mesh_" + i + "_LOD_" + lod;
							_meshFilter.mesh = _meshs[i];
							_renderers[i].material = _mat;
						}

						meshRenderers.Add(_renderers);

						_lodPrefab.name = fileName + "_LOD_" + lod;
						_lodPrefab.transform.SetParent(aimPrefab.transform);
						aimMats.Add(_mat);

					}

					// add lod

					LODGroup group = aimPrefab.AddComponent<LODGroup>();
					LOD[] lods = new LOD[currentLodNum];
					for (int i = 0; i < currentLodNum; i++) {
						lods[i] = new LOD(i == currentLodNum - 1 ? 0f : GetLodRant(lodModelSize[i], i), meshRenderers[i]);
					}
					group.SetLODs(lods);
					group.RecalculateBounds();


					string _newPath = Util.CombinePaths(expPath, name + ".prefab");
					string _parentPath = Util.RelativePath(new FileInfo(_newPath).Directory.FullName);

					Util.CreateFolder(_parentPath);
					Object _assetPrefab;
					if (File.Exists(_newPath)) {
						_assetPrefab = AssetDatabase.LoadAssetAtPath<Object>(_newPath);
						DestroyImmediate((_assetPrefab as GameObject).GetComponent<LODGroup>(), true);
						Object[] things = AssetDatabase.LoadAllAssetRepresentationsAtPath(_newPath);
						foreach (Object o in things) {
							DestroyImmediate(o, true);
						}
					} else {
						_assetPrefab = PrefabUtility.CreateEmptyPrefab(_newPath);
					}

					for (int lod = 0; lod < currentLodNum; lod++) {
						Mesh[] _meshs = aimMeshs[lod];
						AssetDatabase.AddObjectToAsset(aimMats[lod], _newPath);
						AssetDatabase.AddObjectToAsset(aimMats[lod].mainTexture, _newPath);
						int num = _meshs.Length;

						for (int i = 0; i < num; i++) {
							AssetDatabase.AddObjectToAsset(_meshs[i], _newPath);
						}
					}

					PrefabUtility.ReplacePrefab(aimPrefab, _assetPrefab, ReplacePrefabOptions.ReplaceNameBased);

					DestroyImmediate(aimPrefab, false);

				} else {
					return ErrorType.VoxelData;
				}

			} else {
				return ErrorType.FileReading;
			}

			return ErrorType.None;

		}





		public ErrorType CreateObj (PathData pathData, int currentIndex) {

			string objPath = pathData.ObjPath;
			string expPath = pathData.ExportRoot;
			string _ext = Util.GetExtension(objPath);
			string name = Util.GetName(objPath);

			if (_ext == ".vox" || _ext == ".qb") {


				VoxData _chunk;

				if (_ext == ".vox") {
					_chunk = VoxFile.LoadVoxel(objPath);
				} else {
					_chunk = QbFile.LoadQb(objPath).GetVoxData();
				}

				if (_chunk != null) {

					VoxelMesh vMesh = new VoxelMesh();
					Texture2D texture = null;
					string fileName = Path.GetFileNameWithoutExtension(objPath);
					string objFile = fileName;

					EditorUtility.DisplayProgressBar(
						"Hold On...",
						string.Format(
							"Creating obj file: {0} ({1} / {2})",
							Path.GetFileNameWithoutExtension(objPath),
							currentIndex,
							SelectingVoxelFileNum
						),
						(float)(currentIndex) / (float)SelectingVoxelFileNum
					);

					try {
						// Doing the magic stuff
						vMesh.CreateVoxelMesh(_chunk, ModelScale, ModelPivot, ref texture, ref objFile);
						// magic stuff done
					} catch {
						return ErrorType.VoxelData;
					}

					if (!string.IsNullOrEmpty(objFile)) {
						string _newPath = Util.CombinePaths(expPath, name, name + ".obj");
						string _parentPath = Util.RelativePath(new FileInfo(_newPath).Directory.FullName);
						Util.CreateFolder(_parentPath);
						Util.Save(objFile, _newPath);
						Postprocessor.AddToQueue(_newPath);
						if (texture != null) {
							Util.CreateFolder(_parentPath);
							string _texturePath = _parentPath + "/" + fileName + ".png";
							Util.ByteToFile(texture.EncodeToPNG(), _texturePath);
							Postprocessor.AddToQueue(_texturePath);
						}
					} else {
						return ErrorType.MeshGeneration;
					}
				} else {
					return ErrorType.VoxelData;
				}
			}

			return ErrorType.None;

		}




		#endregion



		#region --- UTL ---



		private Rect GUIRect (float width, float height, bool exWidth = false, bool exHeight = false) {
			return GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(exWidth), GUILayout.ExpandHeight(exHeight));
		}



		private void LayoutV (System.Action action, bool box = false, int colorID = 0) {
			if (box) {
				GUIStyle style = new GUIStyle(GUI.skin.box);
				style.padding = new RectOffset(6, 6, 2, 2);
				Color old = GUI.color;
				GUI.color = GetLayoutColor(colorID);
				GUILayout.BeginVertical(style);
				GUI.color = old;
			} else {
				GUILayout.BeginVertical();
			}
			Color _old = GUI.color;
			GUI.color = Color.Lerp(GetLayoutColor(colorID), Color.white, 0.94f);
			action();
			GUI.color = _old;
			GUILayout.EndVertical();
		}



		private void LayoutH (System.Action action, bool box = false, int colorID = 0) {
			if (box) {
				GUIStyle style = new GUIStyle(GUI.skin.box);
				Color old = GUI.color;
				GUI.color = GetLayoutColor(colorID);
				GUILayout.BeginHorizontal(style);
				GUI.color = old;
			} else {
				GUILayout.BeginHorizontal();
			}
			action();
			GUILayout.EndHorizontal();
		}



		private void LayoutF (System.Action action, string label, ref bool open, bool box = false, int colorID = 0) {
			bool _open = open;
			LayoutV(() => {
				_open = GUILayout.Toggle(
					_open,
					label,
					GUI.skin.GetStyle("foldout"),
					GUILayout.ExpandWidth(true),
					GUILayout.Height(18)
				);
				if (_open) {
					action();
				}
			}, box, colorID);
			open = _open;
		}



		private void Space (float space = 4f) {
			GUILayout.Space(space);
		}



		private void ForEachSelecting (System.Action<PathData> action) {
			foreach (var f_p in SeletingObjAndPaths) {
				action(f_p.Value);
			}
		}



		private Color GetLayoutColor (int id) {
			if (!ColorfulUI) {
				return Color.white;
			}
			switch (id) {
				default:
				case 0:
					return Color.white;
				case 1:
					return Color.black;
				case 2:
					return new Color(1, 0.24f, 0.14f, 1);
				case 3:
					return new Color(1, 1, 0.14f, 1);
				case 4:
					return new Color(0.14f, 1, 0.14f, 1);
				case 5:
					return new Color(0.14f, 1, 1, 1);
				case 6:
					return new Color(0.14f, 0.14f, 1, 1);
			}
		}



		private string TheS (int num) {
			return num > 1 ? "s" : "";
		}



		private string GetFixedExportRoot (string objPath, string selectingPath, string exportPath) {
			if (objPath.StartsWith(selectingPath)) {
				string subPath = objPath.Substring(selectingPath.Length);
				int index = subPath.LastIndexOf('/');
				if (index > 0) {
					subPath = subPath.Substring(0, index);
				} else {
					subPath = "";
				}
				return Util.CombinePaths(exportPath, Util.GetName(selectingPath), subPath);
			} else {
				return exportPath;
			}

		}



		#endregion



		#region --- SYS ---



		private void EditorSave () {

			EditorPrefs.SetString(MAIN_TITLE + ".ShaderPath", ShaderPath);
			EditorPrefs.SetString(MAIN_TITLE + ".ExportPath", ExportPath);

			EditorPrefs.SetBool(MAIN_TITLE + ".SelectingFilePannelOpen", SelectingFilePannelOpen);
			EditorPrefs.SetBool(MAIN_TITLE + ".CreateFuncsOpen", CreatePannelOpen);
			EditorPrefs.SetBool(MAIN_TITLE + ".ToolsPannelOpen", ToolsPannelOpen);
			EditorPrefs.SetBool(MAIN_TITLE + ".SettingPannelOpen", SettingPannelOpen);
			EditorPrefs.SetBool(MAIN_TITLE + ".ModelGenerationSettingPannelOpen", ModelGenerationSettingPannelOpen);
			EditorPrefs.SetBool(MAIN_TITLE + ".SystemSettingPannelOpen", SystemSettingPannelOpen);
			EditorPrefs.SetBool(MAIN_TITLE + ".LogMessage", LogMessage);
			EditorPrefs.SetBool(MAIN_TITLE + ".ShowDialog", ShowDialog);
			EditorPrefs.SetBool(MAIN_TITLE + ".ColorfulUI", ColorfulUI);
			EditorPrefs.SetBool(MAIN_TITLE + ".ColorfulTitle", ColorfulTitle);

			EditorPrefs.SetFloat(MAIN_TITLE + ".ModelScale", ModelScale);
			EditorPrefs.SetFloat(MAIN_TITLE + ".ModelPivotX", ModelPivotX);
			EditorPrefs.SetFloat(MAIN_TITLE + ".ModelPivotY", ModelPivotY);
			EditorPrefs.SetFloat(MAIN_TITLE + ".ModelPivotZ", ModelPivotZ);

			EditorPrefs.SetInt(MAIN_TITLE + ".LodNum", LodNum);

		}



		private void EditorLoad () {

			ShaderPath = EditorPrefs.GetString(MAIN_TITLE + ".ShaderPath", "Mobile/Diffuse");
			ExportPath = EditorPrefs.GetString(MAIN_TITLE + ".ExportPath", "Assets");

			SelectingFilePannelOpen = EditorPrefs.GetBool(MAIN_TITLE + ".SelectingFilePannelOpen", true);
			CreatePannelOpen = EditorPrefs.GetBool(MAIN_TITLE + ".CreateFuncsOpen", true);
			ToolsPannelOpen = EditorPrefs.GetBool(MAIN_TITLE + ".ToolsPannelOpen", false);
			SettingPannelOpen = EditorPrefs.GetBool(MAIN_TITLE + ".SettingPannelOpen", false);
			ModelGenerationSettingPannelOpen = EditorPrefs.GetBool(MAIN_TITLE + ".ModelGenerationSettingPannelOpen", false);
			SystemSettingPannelOpen = EditorPrefs.GetBool(MAIN_TITLE + ".SystemSettingPannelOpen", false);
			LogMessage = EditorPrefs.GetBool(MAIN_TITLE + ".LogMessage", true);
			ShowDialog = EditorPrefs.GetBool(MAIN_TITLE + ".ShowDialog", true);
			ColorfulUI = EditorPrefs.GetBool(MAIN_TITLE + ".ColorfulUI", false);
			ColorfulTitle = EditorPrefs.GetBool(MAIN_TITLE + ".ColorfulTitle", true);

			ModelScale = EditorPrefs.GetFloat(MAIN_TITLE + ".ModelScale", 0.1f);
			ModelPivotX = EditorPrefs.GetFloat(MAIN_TITLE + ".ModelPivotX", 0.5f);
			ModelPivotY = EditorPrefs.GetFloat(MAIN_TITLE + ".ModelPivotY", 0f);
			ModelPivotZ = EditorPrefs.GetFloat(MAIN_TITLE + ".ModelPivotZ", 0.5f);

			LodNum = EditorPrefs.GetInt(MAIN_TITLE + ".LodNum", 3);

		}



		private void FixSelectingFile () {

			// Init
			SelectingVoxNum = 0;
			SelectingQbNum = 0;
			SelectingDirNum = 0;
			SelectingObjNum = 0;
			SeletingObjAndPaths = new Dictionary<Object, PathData>();
			List<Object> seletingObjs = new List<Object>(Selection.GetFiltered(typeof(Object), SelectionMode.Assets));

			// Get Paths
			for (int i = 0; i < seletingObjs.Count; i++) {
				// Check
				if (SeletingObjAndPaths.ContainsKey(seletingObjs[i])) {
					continue;
				}
				// Init
				string path = Util.FixPath(AssetDatabase.GetAssetPath(seletingObjs[i]));
				// Logic
				if (AssetDatabase.IsValidFolder(path)) {
					// Selecting Dir
					SelectingDirNum++;
					SelectingObjNum++;
					FixDirIcon(seletingObjs[i]);
					// Vox
					string[] tempVox = Directory.GetFiles(path, "*.vox", SearchOption.AllDirectories);
					for (int j = 0; j < tempVox.Length; j++) {
						tempVox[j] = Util.RelativePath(tempVox[j]);
						Object obj = AssetDatabase.LoadAssetAtPath<Object>(tempVox[j]);
						if (!SeletingObjAndPaths.ContainsKey(obj)) {
							SeletingObjAndPaths.Add(obj, new PathData(
								tempVox[j],
								GetFixedExportRoot(tempVox[j], path, ExportPath)
							));
							FixVoxIcon(obj);
							SelectingVoxNum++;
							SelectingObjNum++;
						}
					}
					// Qb
					string[] tempQb = Directory.GetFiles(path, "*.qb", SearchOption.AllDirectories);
					for (int j = 0; j < tempQb.Length; j++) {
						tempQb[j] = Util.RelativePath(tempQb[j]);
						Object obj = AssetDatabase.LoadAssetAtPath<Object>(tempQb[j]);
						if (!SeletingObjAndPaths.ContainsKey(obj)) {
							SeletingObjAndPaths.Add(obj, new PathData(
								tempQb[j],
								GetFixedExportRoot(tempQb[j], path, ExportPath)
							));
							FixQbIcon(obj);
							SelectingQbNum++;
							SelectingObjNum++;
						}
					}
				} else {
					// Selecting File
					string ex = Util.GetExtension(path);
					if (!SeletingObjAndPaths.ContainsKey(seletingObjs[i])) {
						if (ex == ".vox" || ex == ".qb") {
							if (ex == ".vox") {
								SelectingVoxNum++;
								FixVoxIcon(seletingObjs[i]);
							} else {
								SelectingQbNum++;
								FixQbIcon(seletingObjs[i]);
							}
							SeletingObjAndPaths.Add(seletingObjs[i], new PathData(path, ExportPath));
							SelectingObjNum++;
						} else {
							SelectingObjNum++;
						}
					}
				}
			}
		}



		void FixVoxIcon (Object vox) {
			VoxFileIcon = AssetPreview.GetMiniThumbnail(vox);
		}


		void FixQbIcon (Object qb) {
			QbFileIcon = AssetPreview.GetMiniThumbnail(qb);
		}


		void FixDirIcon (Object dir) {
			DirFileIcon = AssetPreview.GetMiniThumbnail(dir);
		}


		float GetLodRant (int modelSize, int lodLevel) {
			float[] LodRant = new float[9]{
				0.004f, 0.002f, 0.001f,
				0.0004f, 0.0002f, 0.0001f,
				0.00004f, 0.00002f, 0.00001f
			};
			return LodRant[lodLevel] * modelSize;
		}



		#endregion



	}
}
