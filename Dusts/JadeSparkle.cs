﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using System;

namespace JadeFables.Dusts
{
    public class JadeSparkle : ModDust
    {
        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(18 * Main.rand.Next(2), 0, 18, 18);
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(dust.frame.Width / 2, dust.frame.Height / 2) * dust.scale;
                dust.customData = 1;
            }

            if (dust.frame.X == 18)
            {
                if (dust.alpha % 50 == 45)
                    dust.frame.Y += 18;
                dust.alpha += 5;
            }
            else
            {
                if (dust.alpha % 64 == 56)
                    dust.frame.Y += 18;
                dust.alpha += 8;
            }

            if (dust.alpha > 255)
                dust.active = false;

            dust.position += dust.velocity;

            Lighting.AddLight(dust.position, new Color(174, 235, 30).ToVector3() * 0.35f);
            return false;
        }
    }
}
