namespace Voxel2Unity {

	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;

	public class Postprocessor : AssetPostprocessor {


		public static List<string> PathQueue = new List<string>();



		#region --- API ---



		public static void AddToQueue (string path) {
			PathQueue.Add(Util.RelativePath(path));
		}



		public static void ClearQueue () {
			PathQueue.Clear();
		}


		#endregion


		#region --- MSG ---




		public void OnPreprocessModel () {

			string path = Util.RelativePath(assetPath);

			if (PathQueue.Contains(path)) {
				PathQueue.Remove(path);

				if (assetImporter) {
					ModelImporter mi = assetImporter as ModelImporter;
					mi.importMaterials = true;
					mi.materialSearch = ModelImporterMaterialSearch.Local;
					mi.importAnimation = false;
					mi.importBlendShapes = false;
					mi.importNormals = ModelImporterNormals.Calculate;
					mi.normalSmoothingAngle = 0f;

					string fileName = Util.GetName(path);
					EditorApplication.delayCall += () => {
						string parentPath = Util.RelativePath(new FileInfo(path).Directory.FullName);
						FileInfo[] infos = new DirectoryInfo(parentPath + "/Materials").GetFiles("*.mat");
						for (int i = 0; i < infos.Length; i++) {
							Material mat = AssetDatabase.LoadAssetAtPath<Material>(Util.RelativePath(infos[i].FullName));
							if (mat) {
								Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(parentPath + "/" + fileName + ".png");
								if (texture) {
									mat.mainTexture = texture;
									Shader shader = VoxelToUnityWindow.TheShader;
									if (shader) {
										mat.shader = shader;
									}
								}
							}
						}
					};

				}


			}


		}




		void OnPostprocessTexture (Texture2D texture) {
			string path = Util.RelativePath(assetPath);
			if (PathQueue.Contains(path)) {
				PathQueue.Remove(path);
				TextureImporter ti = assetImporter as TextureImporter;
				ti.alphaIsTransparency = true;
				ti.filterMode = FilterMode.Point;
				ti.mipmapEnabled = false;
				ti.wrapMode = TextureWrapMode.Clamp;
				ti.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				ti.textureType = TextureImporterType.Default;
				ti.npotScale = TextureImporterNPOTScale.None;
				ti.maxTextureSize = Mathf.Max(texture.width, texture.height);
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
			}
		}



		#endregion


	}
}
