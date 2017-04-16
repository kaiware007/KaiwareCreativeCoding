#ifndef __FLOWGRID_CGINC__
#define __FLOWGRID_CGINC__

int _GridNumX;
int _GridNumY;
int _GridNumZ;

// Indexから座標を返す　Range: -0.5～0.5
float3 GetIndexToPosition(uint id) {
	float x = (float)(id % _GridNumX) / _GridNumX - 0.5;
	float y = (float)(id / _GridNumX % _GridNumY) / _GridNumY - 0.5;
	float z = (float)(id / _GridNumX / _GridNumY % _GridNumZ) / _GridNumZ - 0.5;

	return float3(x, y, z);
}

// 座標からIndexを返す 範囲外は-1
uint GetPositionToIndex(float3 pos, float3 wallSize, float3 wallCenter) {
	int3 gridSize = (int3)wallSize / int3(_GridNumX, _GridNumY, _GridNumZ);
	float3 wallSizeHalf = wallSize * 0.5;

	int3 idx = (int3)(pos - wallCenter + wallSizeHalf) / gridSize;

	if (idx.x < 0 || idx.x >= _GridNumX ||
		idx.y < 0 || idx.y >= _GridNumY ||
		idx.z < 0 || idx.z >= _GridNumZ) 
	{
		return -1;
	}
	return idx.x + idx.y * _GridNumX + idx.z * _GridNumX * _GridNumY;
}

#endif // __FLOWGRID_CGINC__