﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;
using Terraria;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.GameContent.UI;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace JadeFables.Core
{
	public abstract class WaterAddon : ILoadable
	{
		/// <summary>
		/// call Main.SpriteBatch.Begin with the parameters you want for the front of water. Primarily used for applying shaders
		/// </summary>
		public abstract void SpritebatchChange();
		/// <summary>
		/// call Main.SpriteBatch.Begin with the parameters you want for the back of water. Primarily used for applying shaders
		/// </summary>
		public abstract void SpritebatchChangeBack();

		public abstract bool Visible { get; }

		public abstract Texture2D BlockTexture(Texture2D normal, int x, int y);

		public void Load(Mod mod)
		{
			WaterAddonHandler.addons.Add(this);
		}

		public void Unload()
		{
			
		}
	}

	class WaterAddonHandler : ModSystem
	{
		public static List<WaterAddon> addons = new List<WaterAddon>();

		public static WaterAddon activeAddon;

		public override void Load()
		{
			Terraria.IL_Main.DoDraw += AddWaterShader;
			//IL.Terraria.Main.DrawTiles += SwapBlockTexture;//TODO: Figure out where this logic moved in vanilla
		}

		public override void PostUpdateEverything()
		{
            activeAddon = addons.FirstOrDefault(n => n.Visible);
        }

		public override void Unload()
		{
			addons.Clear();
			addons = null;
			activeAddon = null;
		}

		private void SwapBlockTexture(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(n => n.MatchLdsfld(typeof(Main), "liquidTexture"));
			c.Index += 3;
			c.Emit(OpCodes.Ldloc, 16);
			c.Emit(OpCodes.Ldloc, 15);

			c.EmitDelegate<Func<Texture2D, int, int, Texture2D>>(LavaBlockBody);
		}

		private Texture2D LavaBlockBody(Texture2D arg, int x, int y)
		{
			var tile = Framing.GetTileSafely(x, y);

			if (tile.LiquidType != LiquidID.Water)
				return arg;

			if (activeAddon is null) //putting this in the same check as above dosent seem to short circuit properly? 
				return arg;

			return activeAddon.BlockTexture(arg, x, y);

		}

		private void AddWaterShader(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			//back target
			c.TryGotoNext(n => n.MatchLdfld<Main>("backWaterTarget"));

			c.TryGotoNext(n => n.MatchCallvirt<SpriteBatch>("Draw"));
			c.Index++;
			ILLabel label = il.DefineLabel(c.Next);

			c.TryGotoPrev(n => n.MatchLdfld<Main>("backWaterTarget"));
			c.Index -= 1;
			c.Emit(OpCodes.Pop);
			c.EmitDelegate<Action>(NewDrawBack);
			c.Emit(OpCodes.Br, label);

			//front target
			c.TryGotoNext(n => n.MatchLdsfld<Main>("waterTarget"));

			c.TryGotoNext(n => n.MatchCallvirt<SpriteBatch>("Draw"));
			c.Index++;
			ILLabel label2 = il.DefineLabel(c.Next);

			c.TryGotoPrev(n => n.MatchLdsfld<Main>("waterTarget"));
			c.Emit(OpCodes.Pop);
			c.EmitDelegate<Action>(NewDraw);
			c.Emit(OpCodes.Br, label2);
		}

		private void NewDrawBack()
		{
			var sb = Main.spriteBatch;

			if (activeAddon != null)
			{
				sb.End();
				activeAddon.SpritebatchChangeBack();
			}


			Main.spriteBatch.Draw(Main.instance.backWaterTarget, Main.sceneBackgroundPos - Main.screenPosition, Color.White);

			if (activeAddon != null)
			{
				sb.End();
				sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
			}
		}

		private void NewDraw()
		{
            var sb = Main.spriteBatch;

			if (activeAddon != null)
			{
				sb.End();
				activeAddon.SpritebatchChange();
			}


			Main.spriteBatch.Draw(Main.waterTarget, Main.sceneWaterPos - Main.screenPosition, Color.White);

			if (activeAddon != null)
			{
				sb.End();
				sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
			}
		}
	}
}
