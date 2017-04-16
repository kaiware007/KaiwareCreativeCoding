#ifndef morton_order_h 
#define morton_order_h 

uint BitSeparate32(uint n) {
	n = (n | (n << 8)) & 0x00ff00ff;
	n = (n | (n << 4)) & 0x0f0f0f0f;
	n = (n | (n << 2)) & 0x33333333;
	return (n | (n << 1)) & 0x55555555;
}

// モートン番号取得
// x,y:テクスチャ中の位置
float2 GetMotonNumber(uint x, uint y) {
	return (BitSeparate32(x) | (BitSeparate32(y) << 1));
}
#endif