using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;
using System.IO;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.Audio;
using JadeFables.Core;
using JadeFables.Helpers;
using JadeFables.Helpers.FastNoise;
using JadeFables.Items.BullfrogTree.BullfrogLegs;
using JadeFables.Items.BullfrogTree.FrogInFroggle;

namespace JadeFables.Items.BullfrogTree.Bulfrauble
{
    [AutoloadEquip(EquipType.Neck)]
    public class Bulfrauble : ModItem
    {
        public bool jumping = false;

        public int cooldown = 0;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.accessory = true;
            Item.hasVanityEffects = true;

            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            Lighting.AddLight(player.Center, new Color(142, 196, 251).ToVector3() * 0.95f);
            player.hasMagiluminescence = true;

            if (cooldown-- < 0 && player.controlJump && player.velocity.Y == 0 && !jumping)
            {
                cooldown = 40;
                if (!player.controlLeft && !player.controlRight)
                    return;

                SoundEngine.PlaySound(SoundID.Run, player.Center);
                DoJump(player, 10);

                Projectile.NewProjectileDirect(player.GetSource_Accessory(Item), player.Bottom, Vector2.Zero, ModContent.ProjectileType<BullfrogLegRingAlt>(), 0, 0, player.whoAmI).rotation = 1.57f - (0.78f * Math.Sign(player.velocity.X));
            }

            if (player.controlJump)
            {
                jumping = true;
            }
            else
            {
                jumping = false;
            }
        }

        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            int[] incompadible = new int[] { ModContent.ItemType<BullfrogLegs.BullfrogLegs>(), ModContent.ItemType<FrogInFroggle.FrogInFroggle>(), ItemID.Magiluminescence };
            if (equippedItem.type == ModContent.ItemType<Bulfrauble>() && incompadible.Contains(incomingItem.type))
            {
                return false;
            }

            if (incomingItem.type == ModContent.ItemType<Bulfrauble>() && incompadible.Contains(equippedItem.type))
            {
                return false;
            }

            return base.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<BullfrogLegs.BullfrogLegs>());
            recipe.AddIngredient(ItemID.Magiluminescence, 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }

        private static void DoJump(Player player, float speed)
        {
            if (!player.controlLeft && !player.controlRight)
                return;
            if (player.controlLeft)
                player.velocity.X = -speed;
            else if (player.controlRight)
                player.velocity.X = speed;
        }

    }
}
