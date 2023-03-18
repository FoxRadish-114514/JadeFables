﻿using JadeFables.Dusts;
using JadeFables.Helpers.FastNoise;
using JadeFables.Tiles.BlossomWall;
using JadeFables.Tiles.JadeSeaweed;
using JadeFables.Tiles.JadeGrassShort;
using JadeFables.Tiles.JadeLantern;
using JadeFables.Tiles.JadeOre;
using JadeFables.Tiles.JadeSand;
using JadeFables.Tiles.JadeSandstone;
using JadeFables.Tiles.JadeSandWall;
using JadeFables.Tiles.JadeWaterfall;
using JadeFables.Tiles.JasmineFlower;
using JadeFables.Tiles.OvergrownJadeSand;
using JadeFables.Tiles.SpringChest;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.IO;
using JadeFables.Tiles.Pearls;
using Terraria.WorldBuilding;
using static Terraria.ModLoader.PlayerDrawLayer;
using Terraria.Utilities;
using JadeFables.Tiles.HardenedJadeSand;

namespace JadeFables.Biomes.JadeLake
{
    internal static partial class JadeLakeWorldGen
    {

        public static void PolishPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Filling up the hot springs";


            Rectangle worldRect = new Rectangle(0, 0, Main.maxTilesX, Main.maxTilesY);

            //Places foreground waterfalls (has to be outside of polish pass because it only does the top half of the biome)
            foreach (Rectangle rect in UpperIslandRects)
                PlaceForegroundWaterfalls(rect, 700);

            //Places spring chests
            foreach (Rectangle rect in WholeBiomeRects)
                PlaceJadeChests(rect, WorldGen.genRand.Next(4, 7));

            //Places blossom walls
            foreach(Rectangle rect in UpperIslandRects)
                BlossomWallPopulation(rect, 0.03f, 5f, 3, 5, 10, 10f, 3);

            BlossomWallPopulationInPillars(worldRect, 50);

            //Places jade grass
            foreach (Rectangle rect in LowerIslandRects)
                JadeSeaweedPopulation(rect, 0.1f, 5f);

            //Places bamboo
            BambooPopulation(worldRect, 10, 10f);

            //Places sand piles
            PlaceJadeSandPiles(worldRect, 5);

            //Places jade ore
            PlaceJadeOre(worldRect, 0.5f, 0.25f, 15f);

            //Places jasmine flowers
            PlaceJasmineFlowers(worldRect, 10);

            //Places short jade grass
            JadeGrassShortPopulation(worldRect, 1);

            //Places hanging lanterns
            PlaceJadeLanterns(worldRect, 15, 20);

            //Places overgrown sand
            PlaceOvergrownSand(worldRect, 0.1f, 2f);

            //Places pearls
            PlacePearls(worldRect, 80);
        }

        public static void PlacePearls(Rectangle rect, int chance)
        {
            int[] validTiles = new int[] { ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>(), ModContent.TileType<Tiles.HardenedJadeSand.HardenedJadeSandTile>(), ModContent.TileType<JadeSandTile>() };
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top + 1; j < rect.Top + rect.Height; j++)
                {
                    Tile tile = Framing.GetTileSafely(i, j);
                    Tile leftTile = Framing.GetTileSafely(i - 1, j);
                    Tile rightTile = Framing.GetTileSafely(i + 1, j);
                    Tile topTile = Framing.GetTileSafely(i, j - 1);
                    Tile bottomTile = Framing.GetTileSafely(i, j + 1);

                    if (leftTile.HasTile && Main.tileSolid[leftTile.TileType] && rightTile.HasTile && Main.tileSolid[rightTile.TileType] && topTile.HasTile && Main.tileSolid[topTile.TileType] && bottomTile.HasTile && Main.tileSolid[bottomTile.TileType])
                        continue;
                    if (tile.HasTile && validTiles.Contains(tile.TileType) && WorldGen.genRand.NextBool(chance))
                    {
                        WeightedRandom<Pearl> pool = new(WorldGen.genRand);

                        foreach (Pearl pearl in JadeFables.Instance.GetContent<Pearl>()) 
                        {
                            pool.Add(pearl, pearl.SpawnChance);
                        }

                        pool.Get().Place(i, j);
                    }
                }
            }
        }

        public static void JadeSeaweedPopulation(Rectangle rect, float threshhold, float noiseFreq)
        {
            //Debug method to wipe jade grass
            Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<JadeSeaweedProj>()).ToList().ForEach(n => n.active = false);

            FastNoise fastnoise = new FastNoise(WorldGen.genRand.Next(0, 1000000));
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top + 1; j < rect.Top + rect.Height; j++)
                {
                    Tile tileAbove = Framing.GetTileSafely(i, j - 1);
                    Tile mainTile = Framing.GetTileSafely(i, j);

                    if (!tileAbove.HasTile && mainTile.HasTile && mainTile.TileType == ModContent.TileType<JadeSandTile>() && mainTile.BlockType == BlockType.Solid)
                    {
                        float noiseVal = fastnoise.GetPerlin(i * noiseFreq, j * noiseFreq);
                        if (noiseVal > threshhold)
                        {
                            tileAbove.HasTile = true;
                            tileAbove.TileType = (ushort)ModContent.TileType<JadeSeaweedTile>();
                        }
                    }
                }
            }
        }

        public static void PlaceOvergrownSand(Rectangle rect, float threshhold, float noiseFreq)
        {
            FastNoise fastnoise = new FastNoise(WorldGen.genRand.Next(0, 1000000));
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top + 1; j < rect.Top + rect.Height; j++)
                {
                    Tile tileAbove = Framing.GetTileSafely(i, j - 1);
                    Tile mainTile = Framing.GetTileSafely(i, j);

                    if ((!tileAbove.HasTile || !Main.tileSolid[tileAbove.TileType]) && mainTile.HasTile && mainTile.TileType == ModContent.TileType<JadeSandTile>())
                    {
                        float noiseVal = fastnoise.GetPerlin(i * noiseFreq, j * noiseFreq);
                        if (noiseVal > threshhold)
                        {
                            mainTile.TileType = (ushort)ModContent.TileType<OvergrownJadeSandTile>();
                        }
                    }
                }
            }
        }

        public static void BambooPopulation(Rectangle rect, int chance, float noiseFreq)
        {
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top + 1; j < rect.Top + rect.Height; j++)
                {
                    Tile tileAbove = Framing.GetTileSafely(i, j - 1);
                    Tile mainTile = Framing.GetTileSafely(i, j);
                    if (!tileAbove.HasTile && mainTile.HasTile && mainTile.TileType == ModContent.TileType<JadeSandTile>() && mainTile.BlockType == BlockType.Solid)
                    {
                        if (WorldGen.genRand.NextBool(chance))
                        {
                            int height = WorldGen.genRand.Next(10, 15);
                            byte liquidLevel = tileAbove.LiquidAmount;
                            tileAbove.LiquidAmount = 255;
                            tileAbove.HasTile = true;
                            tileAbove.TileType = TileID.Bamboo;
                            tileAbove.TileFrameY = 0;
                            tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(5) * 18);
                            for (int x = 2; x < height; x++)
                            {
                                Tile tileAbove2 = Framing.GetTileSafely(i, j - x);
                                if (tileAbove2.HasTile)
                                    break;
                                tileAbove2.HasTile = true;
                                tileAbove2.TileType = TileID.Bamboo;
                                tileAbove2.TileFrameY = 0;
                                tileAbove2.TileFrameX = (short)(WorldGen.genRand.Next(5, 15) * 18);
                                if (x == height - 1)
                                {
                                    tileAbove2.TileFrameX = (short)(WorldGen.genRand.Next(15, 20) * 18);
                                }
                            }

                            tileAbove.LiquidAmount = liquidLevel;
                        }
                    }
                }
            }
        }

        public static void BlossomWallPopulation(Rectangle rect, float threshhold, float noiseFreq, int chance, int heightMin, int heightMax, float noiseFreqCircle, float xShrink)
        {
            FastNoise fastnoise = new FastNoise(WorldGen.genRand.Next(0, 10000));
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top + 1; j < rect.Top + rect.Height; j++)
                {
                    Tile tileAbove = Framing.GetTileSafely(i, j - 1);
                    Tile mainTile = Framing.GetTileSafely(i, j);

                    if (!tileAbove.HasTile && mainTile.HasTile && mainTile.TileType == ModContent.TileType<JadeSandTile>())
                    {
                        float noiseVal = fastnoise.GetPerlin(i * noiseFreq, j * noiseFreq);
                        if (noiseVal > threshhold && WorldGen.genRand.NextBool(chance))
                        {
                            for (float rad = 0; rad < 6.28f; rad += 0.03f)
                            {
                                float x = i + (float)Math.Cos(rad);
                                float y = j + (float)Math.Sin(rad);
                                int height = (int)MathHelper.Lerp(heightMin, heightMax, fastnoise.GetPerlin(x * noiseFreqCircle, y * noiseFreqCircle));
                                for (int h = 0; h < height; h++)
                                {
                                    x = i + ((MathF.Cos(rad) * h) / xShrink);
                                    y = j + (MathF.Sin(rad) * h);
                                    Tile wallTile = Framing.GetTileSafely((int)x, (int)y);
                                    if (!wallTile.HasTile && wallTile.WallType == 0)
                                        wallTile.WallType = (ushort)ModContent.WallType<BlossomWall>();
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void BlossomWallPopulationInPillars(Rectangle rect, int chance)
        {
            FastNoise fastnoise = new FastNoise(WorldGen.genRand.Next(0, 10000));
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top + 1; j < rect.Top + rect.Height; j++)
                {
                    Tile mainTile = Framing.GetTileSafely(i, j);

                    if (mainTile.WallType == ModContent.WallType<JadeSandWall>() && Main.rand.NextBool(chance))
                    {
                        mainTile.WallType = (ushort)ModContent.WallType<JadeSandBlossomWall>();
                    }
                }
            }
        }

        public static void PlaceJadeChests(Rectangle rect, int number)
        {
            int tries = 0;
            for (int k = 0; k < number; k++)
            {
                tries++;
                if (tries > 99999)
                    break;
                int i = rect.Left + WorldGen.genRand.Next(rect.Width);
                int j = rect.Top + WorldGen.genRand.Next(rect.Height);
                if (CanPlaceChest(i, j))
                {
                    if (k == 0) //place waterfall above one chest
                    {
                        bool success = false;
                        for (int y = j; y > j - 100; y--)
                        {
                            Tile leftTile = Framing.GetTileSafely(i, y);
                            Tile rightTile = Framing.GetTileSafely(i + 1, y);
                            if (leftTile.LiquidAmount > 0 || rightTile.LiquidAmount > 0)
                                break;
                            if (leftTile.HasTile && leftTile.TileType == ModContent.TileType<HardenedJadeSandTile>() && rightTile.HasTile && rightTile.TileType == ModContent.TileType<HardenedJadeSandTile>())
                            {
                                success = true;
                                leftTile.TileType = (ushort)ModContent.TileType<JadeWaterfallTile>();
                                rightTile.HasTile = false;
                                break;
                            }
                        }

                        if (success)
                        {
                            WorldGen.PlaceChest(i, j, (ushort)ModContent.TileType<SpringChest>());
                        }
                        else
                            k--;
                    }
                    else
                        WorldGen.PlaceChest(i, j, (ushort)ModContent.TileType<SpringChest>());
                }
                else
                    k--;
            }
        }

        public static bool CanPlaceChest(int x, int y)
        {
            for (int i = x; i < x + 2; i++)
            {
                Tile tile1 = Framing.GetTileSafely(x, y - 1);
                if (tile1.HasTile)
                    return false;

                Tile tile2 = Framing.GetTileSafely(x, y);
                if (tile2.HasTile)
                    return false;

                Tile tile3 = Framing.GetTileSafely(x, y + 1);
                if (!tile3.HasTile || tile3.TileType != ModContent.TileType<JadeSandTile>() || tile3.BlockType != BlockType.Solid)
                    return false;
            }
            return true;
        }

        public static void PlaceJasmineFlowers(Rectangle rect, int chance)
        {
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top; j < rect.Top + rect.Height - 1; j++)
                {
                    Tile tileBelow = Framing.GetTileSafely(i, j + 1);
                    Tile mainTile = Framing.GetTileSafely(i, j);

                    if (!mainTile.HasTile && tileBelow.HasTile && tileBelow.TileType == ModContent.TileType<JadeSandTile>() && WorldGen.genRand.NextBool(chance))
                    {
                        WorldGen.PlaceTile(i, j, ModContent.TileType<JasmineFlowerTile>());
                        mainTile.TileFrameX = (short)(WorldGen.genRand.Next(3) * 18);
                    }
                }
            }
        }

        public static void JadeGrassShortPopulation(Rectangle rect, int chance)
        {
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top; j < rect.Top + rect.Height - 1; j++)
                {
                    Tile tileBelow = Framing.GetTileSafely(i, j + 1);
                    Tile mainTile = Framing.GetTileSafely(i, j);
                    Tile tileAbove = Framing.GetTileSafely(i, j - 1);

                    if (!mainTile.HasTile && tileBelow.HasTile && tileBelow.TileType == ModContent.TileType<JadeSandTile>() && WorldGen.genRand.NextBool(chance))
                    {
                        short tileFrame = (short)(WorldGen.genRand.Next(6) * 18);
                        if (WorldGen.genRand.NextBool(3))
                        {
                            WorldGen.PlaceTile(i, j, ModContent.TileType<JadeGrassTall>());
                            tileAbove.TileFrameX = tileFrame;
                        }
                        else
                        {
                            WorldGen.PlaceTile(i, j, ModContent.TileType<JadeGrassShort>());
                        }
                        mainTile.TileFrameX = tileFrame;
                    }
                }
            }
        }

        public static void PlaceJadeSandPiles(Rectangle rect, int chance)
        {
            int[] piles = new int[] { ModContent.TileType<JadeSandCastle1>(), ModContent.TileType<JadeSandCastle2>(), ModContent.TileType<JadeSandPile1>(), ModContent.TileType<JadeSandPile2>(), ModContent.TileType<JadeSandPile3>(), ModContent.TileType<JadeSandPile4>(), ModContent.TileType<JadeSandPile5>(), ModContent.TileType<JadeSandPile6>(), ModContent.TileType<JadeSandPile7>(), ModContent.TileType<JadeSandPile8>() };
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top; j < rect.Top + rect.Height - 1; j++)
                {
                    Tile tileBelow = Framing.GetTileSafely(i, j + 1);
                    Tile mainTile = Framing.GetTileSafely(i, j);

                    if (tileBelow.HasTile && tileBelow.TileType == ModContent.TileType<JadeSandTile>() && WorldGen.genRand.NextBool(chance))
                    {
                        WorldGen.PlaceObject(i, j, piles[WorldGen.genRand.Next(piles.Length)]);
                    }
                }
            }
        }

        public static void PlaceJadeOre(Rectangle rect, float threshhold, float exposedThreshhold, float noiseFreq)
        {
            FastNoise fastnoise = new FastNoise(WorldGen.genRand.Next(0, 10000));
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top; j < rect.Top + rect.Height; j++)
                {
                    Tile mainTile = Framing.GetTileSafely(i, j);
                    if (mainTile.HasTile && mainTile.TileType == ModContent.TileType<JadeSandTile>())
                    {
                        bool exposed = false;
                        for (int x = i - 3; x < i + 3; x++)
                        {
                            for (int y = j - 3; y < j + 3; y++)
                            {
                                Tile airTile = Framing.GetTileSafely(x, y);
                                if (!airTile.HasTile)
                                {
                                    exposed = true;
                                    break;
                                }
                            }
                            if (exposed)
                                break;
                        }
                        float noiseVal = fastnoise.GetPerlin(i * noiseFreq, j * noiseFreq);
                        if (noiseVal > (exposed ? exposedThreshhold : threshhold))
                        {
                            mainTile.TileType = (ushort)ModContent.TileType<JadeOre>();
                        }
                    }
                }
            }
        }

        public static void PlaceJadeLanterns(Rectangle rect, int chance, int spaceBelow)
        {
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top; j < rect.Top + rect.Height - spaceBelow; j++)
                {
                    Tile mainTile = Framing.GetTileSafely(i, j);
                    if (mainTile.HasTile && mainTile.TileType == ModContent.TileType<JadeSandstoneTile>())
                    {
                        bool safe = true;
                        for (int y = 1; y < spaceBelow; y++)
                        {
                            Tile tileBelow = Framing.GetTileSafely(i, j + y);
                            if (tileBelow.HasTile)
                            {
                                safe = false;
                                break;
                            }
                        }

                        if (safe && WorldGen.genRand.NextBool(chance))
                        {
                            JadeLantern.Spawn(i, j + 1);
                        }
                    }
                }
            }
        }

        public static void PlaceForegroundWaterfalls(Rectangle rect, int chance)
        {
            int[] validTiles = new int[] { ModContent.TileType<Tiles.JadeSandstone.JadeSandstoneTile>(), ModContent.TileType<Tiles.HardenedJadeSand.HardenedJadeSandTile>(), ModContent.TileType<JadeSandTile>() };
            for (int i = rect.Left; i < rect.Left + rect.Width; i++)
            {
                for (int j = rect.Top; j < rect.Top + rect.Height; j++)
                {
                    Tile leftTile = Framing.GetTileSafely(i, j);
                    Tile rightTile = Framing.GetTileSafely(i + 1, j);
                    if (!leftTile.HasTile || !rightTile.HasTile)
                        continue;
                    if (validTiles.Contains(leftTile.TileType) && validTiles.Contains(rightTile.TileType))
                    {
                        if (WorldGen.genRand.NextBool(chance))
                        {
                            leftTile.TileType = (ushort)ModContent.TileType<JadeWaterfallTile>();
                            rightTile.HasTile = false;
                            i += 15;
                        }
                    }
                }
            }
        }
    }
}