using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tankkkos
{

    struct UintPair { 
        public uint n1;
        public uint n2;

        public UintPair(uint n1, uint n2) {
            this.n1 = n1;
            this.n2 = n2;
        }

        public override bool Equals(object obj)
        {
            if (obj is UintPair other)
                return n1 == other.n1 && n2 == other.n2;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(n1, n2);
        }
    }

    class UintPairTree {
        public UintPair thisPair;
        public List<UintPairTree> nodes;

        public UintPairTree(UintPair pair)
        {
            this.thisPair = pair;
            nodes = new List<UintPairTree>();
        }

    }

    internal class DLAMountain
    {

        float[,] HeightMap;

        

        public DLAMountain(int seed, uint res)
        {

            generate(seed, res);

        }

        void generate( int seed, uint iterations )
        {

            Random random = new Random((int)seed);

            bool[,] occupied = new bool[4, 4];
            float[,] hm = new float[4, 4];
            for ( uint i=0; i < 4; i++) { for (uint j = 0; j < 4; j++) { occupied[i, j] = false; hm[i, j] = 0.0f;  }}
            occupied[1, 2] = true;


            for (uint i = 0; i < iterations; i++) {
                bool[,] tooccupied;
                float[,] thm;
                addLayer(ref random, occupied, hm, out tooccupied, out thm);
                occupied = tooccupied;
                hm = thm;
            }

            float maxPoint = 0;
            for( uint i=0; i < hm.GetLength(0); i++)
            {
                for( uint j=0; j < hm.GetLength(1); j++)
                {
                    if (hm[i, j] > maxPoint) { maxPoint = hm[i, j]; }

                }
            }

            for (uint i = 0; i < hm.GetLength(0); i++)
            {
                for (uint j = 0; j < hm.GetLength(1); j++)
                {
                    hm[i, j] /= maxPoint;
                }
            }

            HeightMap = hm;


        }

        void addLayer( ref Random random, bool[,] ioccupied, float[,] ihm, out bool[,] ooccupied, out float[,] ohm ) {
            // should be a nxn matrix
            uint width = (uint)ioccupied.GetLength(0)*2;

            ooccupied = addParticles(ref random, ioccupied, width * width / 10);

            ohm = BlurHeightMap(upscaleHeightMap(ihm));

            for( uint i=0; i<ohm.GetLength(0); i++)
            {
                for( uint j=0;  j<ohm.GetLength(1); j++)
                {
                    if(ooccupied[i,j])
                    {
                        ohm[i, j] += 1f; 
                    }
                }
            }

            ohm = BlurHeightMap(ohm);

        }

        
        bool[,] addParticles( ref Random random, bool[,] occupied, uint particleCount ) {

            bool[,] o = crispUpscaling(occupied);

            uint addedCount = 0;
            while (addedCount < particleCount) {
                int posx = (int)random.Next(o.GetLength(0));
                int posz = (int)random.Next(o.GetLength(1));

                int diffx = (int)random.Next(3) - 1;
                int diffz = (int)random.Next(3) - 1;

                if (o[posx, posz]) continue;
                if ( (diffx == 0 && diffz == 0)
                    || (diffx==-1&&posx<1)
                    || (diffx == 1&&posx>o.GetLength(0)-2)
                    || (diffz==-1&&posz < 1)
                    || (diffz==1&&posz>o.GetLength(1)-2) ) continue;

                while ( !o[posx+diffx, posz+diffz])
                {
                    posx += diffx;
                    posz += diffz;

                    diffx = 0;
                    diffz = 0;

                    int dx = (int)random.Next(3) - 1;
                    int dz = (int)random.Next(3) - 1;

                    if (o[posx, posz]) continue;
                    if ((dx == 0 && dz == 0)
                        || (dx == -1 && posx < 1)
                        || (dx == 1 && posx > o.GetLength(0) - 2)
                        || (dz == -1 && posz < 1)
                        || (dz == 1 && posz > o.GetLength(1) - 2)) continue;

                    diffx += dx;
                    diffz += dz;
                }

                o[posx, posz] = true;

                addedCount++;

            }

            return o;

        }

        bool[,] crispUpscaling(bool[,] origin)
        {

            uint startx = 0;
            uint startz = 0;

            // finding the first occupied cell, to know where to start the upscaling from
            for (uint i = 0; i < origin.GetLength(0); i++) { 
                for( uint j= 0; j < origin.GetLength(0); j++) {
                    if (origin[i, j]) {
                        startx = i;
                        startz = j;
                        break;
                    }
                }
            }

            // dfs graph
            HashSet<UintPair> used = new HashSet<UintPair>();
            UintPairTree tree = DfsOnGrid(origin, ref used, startx, startz);

            bool[,] o = new bool[origin.GetLength(0) * 2, origin.GetLength(0) * 2];
            for( uint i = 0; i<o.GetLength(0); i++ ) { for( uint j = 0; j < o.GetLength(1); j++) {o[i, j] = false;}}

            fillOnGrid(ref o, tree);

            return o;

        }

        UintPairTree DfsOnGrid(bool[,] grid, ref HashSet<UintPair> used, uint x, uint z)
        {
            used.Add(new UintPair(x, z));

            UintPairTree o = new UintPairTree(new UintPair(x, z));

            if (x > 0 && grid[x - 1, z] && !used.Contains(new UintPair(x - 1, z))) {
                o.nodes.Add( DfsOnGrid(grid, ref used, x - 1, z) );
            }
            if( x < grid.GetLength(0) - 1 && grid[x + 1, z] && !used.Contains(new UintPair(x + 1, z))) {
                o.nodes.Add( DfsOnGrid(grid, ref used, x + 1, z) ); 
            }
            if(z > 0 && grid[x, z - 1] && !used.Contains(new UintPair(x, z - 1))) {
                o.nodes.Add( DfsOnGrid(grid, ref used, x, z - 1));
            }
            if(z < grid.GetLength(1) - 1 && grid[x, z + 1] && !used.Contains(new UintPair(x, z + 1))) {
                o.nodes.Add( DfsOnGrid(grid, ref used, x, z + 1));
            }

            return o;

        }

        void fillOnGrid( ref bool[,] grid, UintPairTree tree)
        {

            uint x = tree.thisPair.n1;
            uint z = tree.thisPair.n2;

            grid[x*2, z*2] = true;
            foreach (var node in tree.nodes) {
                // we hope the best.... if passed with good args than it should be safe :D
                int diffx = (int)node.thisPair.n1 - (int)x;
                int diffz = (int)node.thisPair.n2 - (int)z;

                grid[x * 2 + diffx, z * 2 + diffz] = true;

                fillOnGrid(ref grid, node);

            }
        }

        float[,] upscaleHeightMap(float[,] heightMap)
        {
            int n = heightMap.GetLength(0);
            int newSize = n * 2;

            float[,] result = new float[newSize, newSize];

            // --- STEP 1: Upscale using bilinear interpolation ---
            for (int y = 0; y < newSize; y++)
            {
                for (int x = 0; x < newSize; x++)
                {
                    // Map back to original space
                    float gx = (float)x / (newSize - 1) * (n - 1);
                    float gy = (float)y / (newSize - 1) * (n - 1);

                    int x0 = (int)Math.Floor(gx);
                    int y0 = (int)Math.Floor(gy);
                    int x1 = Math.Min(x0 + 1, n - 1);
                    int y1 = Math.Min(y0 + 1, n - 1);

                    float tx = gx - x0;
                    float ty = gy - y0;

                    float v00 = heightMap[x0, y0];
                    float v10 = heightMap[x1, y0];
                    float v01 = heightMap[x0, y1];
                    float v11 = heightMap[x1, y1];

                    float vx0 = MathHelper.Lerp(v00, v10, tx);
                    float vx1 = MathHelper.Lerp(v01, v11, tx);

                    result[x, y] = MathHelper.Lerp(vx0, vx1, ty);
                }
            }

            return result;

        }

        float[,] BlurHeightMap(float[,] heightMap)
        {

            int newSize = heightMap.GetLength(0);

            // --- STEP 2: Blur (3x3 box blur) ---
            float[,] blurred = new float[newSize, newSize];

            for (int y = 0; y < newSize; y++)
            {
                for (int x = 0; x < newSize; x++)
                {
                    float sum = 0f;
                    int count = 0;

                    for (int oy = -1; oy <= 1; oy++)
                    {
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            int sx = Math.Clamp(x + ox, 0, newSize - 1);
                            int sy = Math.Clamp(y + oy, 0, newSize - 1);

                            sum += heightMap[sx, sy];
                            count++;
                        }
                    }

                    blurred[x, y] = sum / count;
                }
            }

            return blurred;
        }

        float getLerpHeight(float x, float z)
        {

            z = Math.Clamp(z, 0, HeightMap.GetLength(1) - 1);
            x = Math.Clamp(x, 0, HeightMap.GetLength(0) - 1);

            uint x0 = (uint)Math.Floor(x);
            uint x1 = (uint)Math.Ceiling(x);
            uint z0 = (uint)Math.Floor(z);
            uint z1 = (uint)Math.Ceiling(z);

            float hx0z0 = HeightMap[x0, z0];
            float hx1z0 = HeightMap[x1, z0];
            float hx0z1 = HeightMap[x0, z1];
            float hx1z1 = HeightMap[x1, z1];

            float xLerp = x - x0;
            float zLerp = z - z0;

            float hx0 = MathHelper.Lerp(hx0z0, hx1z0, xLerp);
            float hx1 = MathHelper.Lerp(hx0z1, hx1z1, xLerp);

            return MathHelper.Lerp(hx0, hx1, zLerp);
        }


        public float getHeightAtPoint(float x, float z){

            x = x * HeightMap.GetLength(0) / 2 + HeightMap.GetLength(0) / 2;
            z = z * HeightMap.GetLength(1) / 2 + HeightMap.GetLength(1) / 2;

            return getLerpHeight(x, z);

        }

    }
}
