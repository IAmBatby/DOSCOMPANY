using System;
using System.Collections.Generic;
using System.Text;

namespace DOSCOMPANY
{
    public struct TerrainPixel
    {
        public int PositionX { get; private set; }
        public int PositionY { get; private set; }
        public int TerrainLayerIndex { get; private set; }
        public float BlendValue { get; private set; }

        public TerrainPixel(int x, int y, int z, float blend)
        {
            PositionX = x;
            PositionY = y;
            TerrainLayerIndex = z;
            BlendValue = blend;
        }
    }
}
