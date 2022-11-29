﻿using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace JadeFables.Core.Edits;

public sealed class ModifyTreeShakeResultsEdit : RuntimeDetourModSystem
{
    private static readonly int[] VanillaFruits =
    {
        // Forest
        ItemID.Apple,
        ItemID.Apricot,
        ItemID.Grapefruit,
        ItemID.Lemon,
        ItemID.Peach,

        // Boreal
        ItemID.Cherry,
        ItemID.Plum,

        // Ebonwood
        ItemID.BlackCurrant,
        ItemID.Elderberry,

        // Shadewood
        ItemID.BloodOrange,
        ItemID.Rambutan,

        // Mahogany
        ItemID.Mango,
        ItemID.Pineapple,

        // Palm
        ItemID.Banana,
        ItemID.Coconut,

        // Pearlwood
        ItemID.Dragonfruit,
        ItemID.Starfruit,

        // Ash (1.4.4)
        // ItemID.Pomegranate,
        // ItemID.SpicyPepper
    };

    public delegate void TreeShakeRateModifier(int rate, ILCursor c);

    public TreeShakeRateModifier MultiplicativeModifier(float multiplier) {
        return (_, c) =>
        {
            // Objective: multiply rate (already pushed to stack) by given multiplier

            c.Emit(OpCodes.Conv_R4); // convert rate to float
            c.Emit(OpCodes.Ldc_R4, multiplier); // push multiplier
            c.Emit(OpCodes.Mul); // multiply
            c.Emit(OpCodes.Conv_I4); // convert back to int
        };
    }

    public Dictionary<int, TreeShakeRateModifier> RateModifiers { get; } = new();

    public override void OnModLoad() {
        base.OnModLoad();

        foreach (int fruit in VanillaFruits) RateModifiers.Add(fruit, MultiplicativeModifier(1.3f));

        IL.Terraria.WorldGen.ShakeTree += RewriteShakeDropRates;
    }

    public override void OnModUnload() {
        base.OnModUnload();

        RateModifiers.Clear();
    }

    private void RewriteShakeDropRates(ILContext il) {
        /* Objective: Arbitrarily modify the drop rates of tree shake results.
         *  All instances of the pattern: ldc.i4(.s) x
         *  represent a drop rate.        UnifiedRandom::Next(int32)
         *  After finding the drop rate,
         *  we can search for item IDs based on Item::NewItem calls (item ID is last value on stack before method invocation).
         */
        ILCursor c = new(il);

        // TODO: Traverse IL and apply modifiers.
    }
}