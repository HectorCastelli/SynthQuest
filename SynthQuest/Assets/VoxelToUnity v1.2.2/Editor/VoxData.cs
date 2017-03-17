namespace Voxel2Unity {
	
	using UnityEngine;
	

	public enum VoxelFileFormat {
		Vox = 0,
		Qb = 1,
	}



	public class VoxData {

		public string Name;

		public Color[] Palatte;

		public int SizeX, SizeY, SizeZ;

		public byte[] Version;

		public int[,,] Voxels;

		public int VoxelNum {
			get {
				int num = 0;
				for (int i = 0; i < SizeX; i++) {
					for (int j = 0; j < SizeY; j++) {
						for (int k = 0; k < SizeZ; k++) {
							if (Voxels[i, j, k] != 0) {
								num++;
							}
						}
					}
				}
				return num;
			}
		}

		public VoxData GetLODVoxelData (int lodLevel) {
			if (SizeX <= 1 || SizeY <= 1 || SizeZ <= 1) {
				return null;
			}
			lodLevel = Mathf.Clamp(lodLevel, 0, 8);
			if (lodLevel <= 1) {
				return this;
			}
			if (SizeX <= lodLevel && SizeY <= lodLevel && SizeZ <= lodLevel) {
				return null;
			}
			VoxData data = new VoxData();
			data.SizeX = Mathf.Max(Mathf.CeilToInt((float)SizeX / lodLevel), 1);
			data.SizeY = Mathf.Max(Mathf.CeilToInt((float)SizeY / lodLevel));
			data.SizeZ = Mathf.Max(Mathf.CeilToInt((float)SizeZ / lodLevel));
			data.Version = Version;
			data.Palatte = Palatte;
			data.Voxels = new int[data.SizeX, data.SizeY, data.SizeZ];
			for (int x = 0; x < data.SizeX; x++) {
				for (int y = 0; y < data.SizeY; y++) {
					for (int z = 0; z < data.SizeZ; z++) {
						data.Voxels[x, y, z] = this.GetMajorityColorIndex(x * lodLevel, y * lodLevel, z * lodLevel, lodLevel);
					}
				}
			}
			return data;
		}




		public void Flip (Axis _axis) {

			for (int i = 0; i < (_axis == Axis.X ? SizeX * 0.5f : SizeX); i++) {
				for (int j = 0; j < (_axis == Axis.Y ? SizeY * 0.5f : SizeY); j++) {
					for (int k = 0; k < (_axis == Axis.Z ? SizeZ * 0.5f : SizeZ); k++) {
						int ii = _axis == Axis.X ? SizeX - i - 1 : i;
						int jj = _axis == Axis.Y ? SizeY - j - 1 : j;
						int kk = _axis == Axis.Z ? SizeZ - k - 1 : k;
						int _b = Voxels[i, j, k];
						Voxels[i, j, k] = Voxels[ii, jj, kk];
						Voxels[ii, jj, kk] = _b;
					}
				}
			}
		}

		public void Rotate (Axis _axis, bool reverse = false) {

			int _newSizeX = SizeX;
			int _newSizeY = SizeY;
			int _newSizeZ = SizeZ;
			int[,,] _newByte = null;

			switch (_axis) {
				case Axis.X:
					_newSizeY = SizeZ;
					_newSizeZ = SizeY;
					_newByte = new int[_newSizeX, _newSizeY, _newSizeZ];
					for (int i = 0; i < SizeX; i++) {
						for (int j = 0; j < SizeY; j++) {
							for (int k = 0; k < SizeZ; k++) {
								_newByte[i, k, j] = Voxels[i, j, k];
							}
						}
					}
					SizeY = _newSizeY;
					SizeZ = _newSizeZ;
					Voxels = _newByte;

					if (reverse) {
						Flip(Axis.Z);
					} else {
						Flip(Axis.Y);
					}


					break;
				case Axis.Y:
					_newSizeX = SizeZ;
					_newSizeZ = SizeX;
					_newByte = new int[_newSizeX, _newSizeY, _newSizeZ];
					for (int i = 0; i < SizeX; i++) {
						for (int j = 0; j < SizeY; j++) {
							for (int k = 0; k < SizeZ; k++) {
								_newByte[k, j, i] = Voxels[i, j, k];
							}
						}
					}
					SizeX = _newSizeX;
					SizeZ = _newSizeZ;
					Voxels = _newByte;


					if (reverse) {
						Flip(Axis.X);
					} else {
						Flip(Axis.Z);
					}

					break;
				case Axis.Z:
					_newSizeX = SizeY;
					_newSizeY = SizeX;
					_newByte = new int[_newSizeX, _newSizeY, _newSizeZ];
					for (int i = 0; i < SizeX; i++) {
						for (int j = 0; j < SizeY; j++) {
							for (int k = 0; k < SizeZ; k++) {
								_newByte[j, i, k] = Voxels[i, j, k];
							}
						}
					}
					SizeX = _newSizeX;
					SizeY = _newSizeY;
					Voxels = _newByte;


					if (reverse) {
						Flip(Axis.Y);
					} else {
						Flip(Axis.X);
					}

					break;
			}







		}


		public int GetMajorityColorIndex (int x, int y, int z, int lodLevel) {
			x = Mathf.Min(x, SizeX - 2);
			y = Mathf.Min(y, SizeY - 2);
			z = Mathf.Min(z, SizeZ - 2);
			int cubeNum = (int)Mathf.Pow(lodLevel, 3);
			int[] index = new int[cubeNum];
			for (int i = 0; i < lodLevel; i++) {
				for (int j = 0; j < lodLevel; j++) {
					for (int k = 0; k < lodLevel; k++) {
						if (x + i > SizeX - 1 || y + j > SizeY - 1 || z + k > SizeZ - 1) {
							index[i * lodLevel * lodLevel + j * lodLevel + k] = 0;
						} else {
							index[i * lodLevel * lodLevel + j * lodLevel + k] = this.Voxels[x + i, y + j, z + k];
						}
					}
				}
			}

			int[] numIndex = new int[cubeNum];
			int maxNum = 1;
			int maxNumIndex = 0;
			for (int i = 0; i < cubeNum; i++) {
				numIndex[i] = index[i] == 0 ? 0 : 1;
			}
			for (int i = 0; i < cubeNum; i++) {
				for (int j = 0; j < cubeNum; j++) {
					if (i != j && index[i] != 0 && index[i] == index[j]) {
						numIndex[i]++;
						if (numIndex[i] > maxNum) {
							maxNum = numIndex[i];
							maxNumIndex = i;
						}
					}
				}
			}
			return index[maxNumIndex];
		}


		public VoxData GetLod(int level) {



			return this;
		}

	}

	

}
