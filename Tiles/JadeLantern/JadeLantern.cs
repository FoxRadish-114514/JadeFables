﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using JadeFables.Tiles.JadeSand;
using JadeFables.Core;

namespace JadeFables.Tiles.JadeLantern
{
    public class JadeLantern : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0); // Todo: make less annoying.
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16};
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Spring Lantern");
            AddMapEntry(new Color(207, 160, 118), name);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            var existingGrass = Main.projectile.Where(n => n.active && n.Center == new Vector2(i, j) * 16 && n.type == ModContent.ProjectileType<JadeLanternProj>()).FirstOrDefault();
            if (existingGrass == default)
            {
                Projectile.NewProjectile(new EntitySource_Misc("Jade Lantern"), new Vector2(i, j) * 16, Vector2.Zero, ModContent.ProjectileType<JadeLanternProj>(), 0, 0);
            }
        }
    }

    public class JadeLanternItem : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spring Lantern");
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = TileType<JadeLantern>();
            Item.rare = ItemRarityID.White;
            Item.value = 5;
        }
    }

    public class JadeLanternProj : ModProjectile
    {

        public VerletChain chain;

        private Rectangle chainFrame;

        private Rectangle lanternFrame;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jade Lantern");
        }

        public override void SetDefaults()
        {
            Projectile.knockBack = 6f;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            chain = new VerletChain(Main.rand.Next(10, 15), true, Projectile.Center, 16, true);
            chain.Start();
            chain.forceGravity = new Vector2(0, 0.4f);
            chainFrame = new Rectangle(0, 16 * Main.rand.Next(4), 12, 16);
            lanternFrame = new Rectangle(0, 32 * Main.rand.Next(4), 32, 32);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (chain == null)
                return false;
            Texture2D chainTex = ModContent.Request<Texture2D>(Texture + "_Chain").Value;
            Texture2D lanternTex = ModContent.Request<Texture2D>(Texture).Value;
            for (int i = 0; i < chain.segmentCount - 1; i++)
            {
                RopeSegment segInner = chain.ropeSegments[i];
                RopeSegment nextSegInner = chain.ropeSegments[i + 1];
                Main.spriteBatch.Draw(chainTex, segInner.posNow - Main.screenPosition, chainFrame, Lighting.GetColor((int)(segInner.posNow.X / 16), (int)(segInner.posNow.Y / 16)), segInner.posNow.DirectionTo(nextSegInner.posNow).ToRotation() + 1.57f, chainFrame.Size() / 2, 1, SpriteEffects.None, 0f);
            }

            RopeSegment seg = chain.ropeSegments[chain.segmentCount - 2];
            RopeSegment nextSeg = chain.ropeSegments[chain.segmentCount - 1];
            Main.spriteBatch.Draw(lanternTex, nextSeg.posNow - Main.screenPosition, lanternFrame, Lighting.GetColor((int)(nextSeg.posNow.X / 16), (int)(nextSeg.posNow.Y / 16)), seg.posNow.DirectionTo(nextSeg.posNow).ToRotation() - 1.57f, lanternFrame.Size() / 2, 1, SpriteEffects.None, 0f);
            return false;
        }

        public override void AI()
        {
            Tile tile = Main.tile[(int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)];
            if (tile.HasTile && tile.TileType == ModContent.TileType<JadeLantern>())
                Projectile.timeLeft = 2;

            chain.UpdateChain();
        }
    }
}
