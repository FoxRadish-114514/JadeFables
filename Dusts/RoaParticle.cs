﻿using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace JadeFables.Dusts
{
	public class RoaParticle : ModDust
	{
		public override string Texture => "JadeFables/Dusts/RoaParticle";

		public override void OnSpawn(Dust dust)
		{
			dust.noLight = true;
			dust.customData = false;
			dust.noGravity = true;
			dust.fadeIn = 0f;
			dust.frame = new Rectangle(0, 0, 32, 144);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			dust.rotation = dust.velocity.ToRotation() + MathHelper.PiOver2;
			dust.position += dust.velocity;

			dust.color.A = 0;

			dust.fadeIn++;
			if (dust.fadeIn >= 4)
			{
				if (dust.alpha == 3)
					dust.active = false;

				dust.fadeIn = 0;
				dust.alpha = (dust.alpha + 1) % 4;
			}

            if (!dust.noLight)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.5f * dust.scale);

            return false;
		}


		public override bool PreDraw(Dust dust)
		{
			int frameHeight = Texture2D.Value.Height / 4;
			int startY = frameHeight * dust.alpha;

			Rectangle sourceRectangle = new Rectangle(0, startY, Texture2D.Value.Width, frameHeight);
			Vector2 origin = sourceRectangle.Size() / 2f;

			Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, sourceRectangle, dust.color with { A = 0 }, dust.rotation, origin, new Vector2(dust.scale * 0.5f, dust.scale) * 0.5f, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, sourceRectangle, dust.color with { A = 0 } * 0.5f, dust.rotation, origin, new Vector2(dust.scale * 0.5f, dust.scale) * 0.75f, SpriteEffects.None, 0f);

			return false;
		}
	}
}