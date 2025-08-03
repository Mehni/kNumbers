namespace Numbers
{
    using RimWorld;
    using System;
    using Unity.Burst.Intrinsics;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_SelfTend : PawnColumnWorker_Checkbox
    {
        public static float IconPositionVertical = 35f;
        public static float IconPositionHorizontal = 5f;

        protected override bool GetValue(Pawn pawn) => pawn.playerSettings.selfTend;

        protected override bool HasCheckbox(Pawn pawn) => pawn.IsColonist && !pawn.Dead && !(pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor));

        protected override void SetValue(Pawn pawn, bool value, PawnTable table)
        {
            if (pawn.workSettings != null)
            {
                if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
                {
                    if (value)
                    {
                        Messages.Message(
                            "IncapableOfWorkType".Translate(pawn.LabelShort, WorkTypeDefOf.Doctor.labelShort),
                            pawn,
                            MessageTypeDefOf.RejectInput
                        );
                    }
                    // Skip setting self-tend if incapable
                    return;
                }

                int priority;
                try
                {
                    priority = pawn.workSettings.GetPriority(WorkTypeDefOf.Doctor);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Work priorities may be uninitialized (e.g., newly joined pawn) or corrupt
                    Messages.Message(
                        "Cannot enable self tend because work priorities are uninitialized or corrupt for the pawn.",
                        pawn,
                        MessageTypeDefOf.CautionInput,
                        false
                    );
                    return;
                }

                if (value && priority == 0)
                {
                    Messages.Message(
                        "MessageSelfTendUnsatisfied".Translate(pawn.LabelShort, pawn),
                        MessageTypeDefOf.CautionInput,
                        false
                    );
                }
            }

            pawn.playerSettings.selfTend = value;
        }

        protected override string GetHeaderTip(PawnTable table) => "SelfTend".Translate() + "\n\n" + "Numbers_ColumnHeader_Tooltip".Translate();

        protected override string GetTip(Pawn pawn) => "SelfTendTip".Translate(Faction.OfPlayer.def.pawnsPlural, TendUtility.SelfTendQualityFactor.ToStringPercent()).CapitalizeFirst();

        public override void DoHeader(Rect rect, PawnTable table)
        {
            float scale = 0.3f;
            base.DoHeader(rect, table);
            Vector2 headerIconSize = new Vector2(StaticConstructorOnGameStart.Tame.width, StaticConstructorOnGameStart.Tame.height) * scale;
            int     num            = (int)((rect.width - headerIconSize.x) * 0.4);
            Rect    position       = new(rect.x + num + IconPositionHorizontal, rect.yMax - StaticConstructorOnGameStart.Tame.height + IconPositionVertical, headerIconSize.x, headerIconSize.y);
            GUI.DrawTexture(position, StaticConstructorOnGameStart.Tame);
        }
    }
}
