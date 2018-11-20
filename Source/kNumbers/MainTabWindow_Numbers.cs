using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;

namespace kNumbers
{
    public abstract class MainTabWindow_ThingWithComp : MainTabWindow
    {
        public const int cFreeSpaceAtTheEnd = 50;

        public const float buttonWidth = 160f;

        public const float PawnRowHeight = 35f;

        protected const float NameColumnWidth = 175f;

        protected const float NameLeftMargin = 15f;

        protected Vector2 scrollPosition = Vector2.zero;

        protected List<ThingWithComps> things = new List<ThingWithComps>();

        public float kListDesiredWidth = 0f;
        /*
                protected List<Pawn> pawns
                {
                    set
                    {
                        this.things = value.Select(p=>p as ThingWithComps).ToList();
                    }
                }
        */
        protected int ThingsCount
        {
            get
            {
                return this.things.Count;
            }
        }

        protected abstract void DrawPawnRow(Rect r, ThingWithComps p);

        public override void PreOpen()
        {
            base.PreOpen();
            this.BuildPawnList();
        }

        public override void PostOpen()
        {
            base.PostOpen();
            this.windowRect.size = this.InitialSize;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            this.windowRect.size = this.InitialSize;
        }

        protected virtual void BuildPawnList()
        {
            this.things.Clear();
        }

        public void Notify_PawnsChanged()
        {
            this.BuildPawnList();
        }

        protected void DrawRows(Rect outRect)
        {
            float winWidth = outRect.width - 16f;
            Rect viewRect = new Rect(0f, 0f, winWidth, (float)this.things.Count * PawnRowHeight);

            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);
            float num = 0f;
            for (int i = 0; i < this.things.Count; i++)
            {
                ThingWithComps p = this.things[i];
                Rect rect = new Rect(0f, num, viewRect.width, PawnRowHeight);
                if (num - this.scrollPosition.y + PawnRowHeight >= 0f && num - this.scrollPosition.y <= outRect.height)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.2f);
                    Widgets.DrawLineHorizontal(0f, num, viewRect.width);
                    GUI.color = Color.white;
                    this.PreDrawPawnRow(rect, p);
                    this.DrawPawnRow(rect, p);
                    this.PostDrawPawnRow(rect, p);
                }
                num += PawnRowHeight;
            }
            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void PreDrawPawnRow(Rect rect, ThingWithComps p)
        {
            Rect rect2 = new Rect(0f, rect.y, rect.width, PawnRowHeight);
            if (Mouse.IsOver(rect2))
            {
                GUI.DrawTexture(rect2, TexUI.HighlightTex);
            }
            Rect rect3 = new Rect(0f, rect.y, 175f, PawnRowHeight);
            Rect position = rect3.ContractedBy(3f);
            if (p is Pawn)
            {
                if ((p as Pawn).health.summaryHealth.SummaryHealthPercent < 0.999f)
                {
                    Rect rect4 = new Rect(rect3);
                    rect4.xMin -= 4f;
                    rect4.yMin += 4f;
                    rect4.yMax -= 6f;
                    Widgets.FillableBar(rect4, (p as Pawn).health.summaryHealth.SummaryHealthPercent, GenMapUI.OverlayHealthTex, BaseContent.ClearTex, false);
                }
            }
            if (Mouse.IsOver(rect3))
            {
                GUI.DrawTexture(position, TexUI.HighlightTex);
            }
            string label;
            Pawn p1 = (p is Corpse) ? (p as Corpse).InnerPawn : p as Pawn;
            if (!p1.RaceProps.Humanlike && p1.Name != null && !p1.Name.Numerical)
            {
                label = p1.Name.ToStringShort.CapitalizeFirst() + ", " + p1.KindLabel;
            }
            else
            {
                label = p1.LabelCap;
            }
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            Rect rect5 = new Rect(rect3);
            rect5.xMin += 15f;
            Widgets.Label(rect5, label);
            Text.WordWrap = true;
            if (Widgets.ButtonInvisible(rect3))
            {
                //shift-selection: keep tab, don't deselect, don't move camera
                if (Event.current.shift)
                {
                    //do nothing
                }
                //alt-selection: deselect, remove tab
                else if (Event.current.alt)
                {
                    Find.MainTabsRoot.EscapeCurrentTab(true);
                    Find.Selector.ClearSelection();
                }
                //normal selection: remove tab, deselect, move camera
                else
                {
                    Find.MainTabsRoot.EscapeCurrentTab(true);
                    Find.Selector.ClearSelection();
                    Find.CameraDriver.JumpToCurrentMapLoc(p.PositionHeld);
                }

                //finally select if pawn is present
                if (p.Spawned)
                {
                    Find.Selector.Select(p, true, true);
                }
                return;
            }
            TipSignal tooltip = p.GetTooltip();
            tooltip.text = "ClickToJumpTo".Translate() + "\n\n" + tooltip.text;
            TooltipHandler.TipRegion(rect3, tooltip);
        }

        private void PostDrawPawnRow(Rect rect, ThingWithComps p)
        {
            if (p is Pawn)
            {
                if ((p as Pawn).Downed)
                {
                    GUI.color = new Color(1f, 0f, 0f, 0.5f);
                    Widgets.DrawLineHorizontal(rect.x, rect.center.y, rect.width);
                    GUI.color = Color.white;
                }
            }
        }
    }


    public class MainTabWindow_Numbers : MainTabWindow_ThingWithComp
    {

        public enum PawnType
        {
            Colonists,
            Prisoners,
            Guests,
            Enemies,    //assuming humanlike enemies, animals somewhat worked, but mechanoids will crash the tab
            Animals,
            WildAnimals,
            Corpses,
            AnimalCorpses,
        }

        public enum OrderBy
        {
            Name,
            Column
        }

        // List<ThingWithComps> things;
        public static bool pawnListDescending = false;
        public static bool isDirty = true;
        int pawnListUpdateNext = 0;

        //global lists
        readonly List<StatDef> pawnHumanlikeStatDef = new List<StatDef>();
        readonly List<StatDef> pawnAnimalStatDef = new List<StatDef>();
        readonly List<StatDef> corpseStatDef = new List<StatDef>();
        List<NeedDef> pawnHumanlikeNeedDef = new List<NeedDef>();
        readonly List<NeedDef> pawnAnimalNeedDef = new List<NeedDef>();
        readonly List<SkillDef> pawnSkillDef = new List<SkillDef>();
        

        //local lists - content depends on pawn type
        List<StatDef> pStatDef;
        List<NeedDef> pNeedDef;

        List<KListObject> kList = new List<KListObject>();
        int listScrollPos = 0;

        OrderBy chosenOrderBy = OrderBy.Name;
        KListObject sortObject;

        float maxWindowWidth = 1060f;

        public override Vector2 RequestedTabSize
        {
            get
            {
                float maxWidth = (maxWindowWidth > kListDesiredWidth + 70) ? maxWindowWidth : kListDesiredWidth + 70;
                return new Vector2(maxWidth, 90f + (float)base.ThingsCount * PawnRowHeight + 65f + 16f);
            }
        }

        public MainTabWindow_Numbers()
        {
            Pawn tmpPawn;

            MethodInfo statsToDraw = typeof(StatsReportUtility).GetMethod("StatsToDraw", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, new Type[] { typeof(Thing) }, null);
            MethodInfo corpseStatsToDraw = typeof(StatsReportUtility).GetMethod("StatsToDraw", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, new Type[] { typeof(Def), typeof(ThingDef) }, null);

            tmpPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.AncientSoldier, Faction.OfPlayer);

            pawnHumanlikeStatDef = (from s in ((IEnumerable<StatDrawEntry>)statsToDraw.Invoke(null, new[] { tmpPawn })) where s.ShouldDisplay && s.stat != null select s.stat).OrderBy(stat => stat.LabelCap).ToList();
            pawnHumanlikeNeedDef.AddRange(DefDatabase<NeedDef>.AllDefsListForReading);

            tmpPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Thrumbo, null);
            pawnAnimalStatDef = (from s in ((IEnumerable<StatDrawEntry>)statsToDraw.Invoke(null, new[] { tmpPawn })) where s.ShouldDisplay && s.stat != null select s.stat).ToList();
            pawnAnimalNeedDef = tmpPawn.needs.AllNeeds.Where(x => x.def.showOnNeedList).Select(x => x.def).ToList();

            corpseStatDef = (from s in ((IEnumerable<StatDrawEntry>)corpseStatsToDraw.Invoke(null, new[] { tmpPawn.RaceProps.corpseDef, null })) where s.ShouldDisplay && s.stat != null select s.stat).ToList();
            corpseStatDef.Add(StatDefOf.MeatAmount);
            corpseStatDef.Add(StatDefOf.LeatherAmount);
        }

        String NumbersXMLPath
        {
            get
            {
                //TODO: FIX!!!
                return Path.Combine(GenFilePaths.ModsConfigFilePath, "kNumbers.config");
            }
        }

        public void WritePresets()
        {

        }

        public void ReadPresets()
        {

        }

        public override void PreOpen()
        {
            var component = Find.World.GetComponent<WorldComponent_Numbers>();
            component.savedKLists.TryGetValue(component.chosenPawnType, out kList);
            if (kList == null)
            {
                kList = new List<KListObject>();
                component.savedKLists[component.chosenPawnType] = kList;
            }
            base.PreOpen();
            isDirty = true;
        }

        bool Fits(float desiredSize)
        {
            return (kListDesiredWidth + desiredSize + 70 < maxWindowWidth);
        }


        bool IsEnemy(Pawn p)
        {
            return
                !p.IsPrisoner &&
                (
                    ((p.Faction != null) && p.Faction.HostileTo(Faction.OfPlayer)) ||
                    (!p.RaceProps.Animal && (!p.RaceProps.Humanlike || p.RaceProps.IsMechanoid))
                ) &&
                !p.Position.Fogged(Find.CurrentMap) && (p.Position != IntVec3.Invalid);
        }

        bool IsWildAnimal(Pawn p)
        {
            return p.RaceProps.Animal && (p.Faction != Faction.OfPlayer) && !p.Position.Fogged(Find.CurrentMap) && (p.Position != IntVec3.Invalid);
        }

        bool IsGuest(Pawn p)
        {
            return
                (p.guest != null) && !p.guest.IsPrisoner &&
                (p.Faction != null) && !p.Faction.HostileTo(Faction.OfPlayer) && p.Faction != Faction.OfPlayer &&
                !p.Position.Fogged(Find.CurrentMap) && (p.Position != IntVec3.Invalid);
        }

        void UpdatePawnList()
        {
            var component = Find.World.GetComponent<WorldComponent_Numbers>();

            this.things.Clear();
            IEnumerable<ThingWithComps> tempPawns = new List<ThingWithComps>();
            switch (component.chosenPawnType)
            {
                default:
                case PawnType.Colonists:
                    tempPawns = Find.CurrentMap.mapPawns.FreeColonists.Cast<ThingWithComps>().ToList();
                    pStatDef = pawnHumanlikeStatDef;
                    pNeedDef = pawnHumanlikeNeedDef;
                    break;

                case PawnType.Prisoners:
                    tempPawns = Find.CurrentMap.mapPawns.PrisonersOfColony.Cast<ThingWithComps>().ToList();
                    pStatDef = pawnHumanlikeStatDef;
                    pNeedDef = pawnHumanlikeNeedDef;
                    break;

                case PawnType.Guests:
                    tempPawns = Find.CurrentMap.mapPawns.AllPawns.Where(IsGuest).Cast<ThingWithComps>().ToList();
                    pStatDef = pawnHumanlikeStatDef;
                    pNeedDef = pawnHumanlikeNeedDef;
                    break;

                case PawnType.Enemies:
                    // tempPawns = Find.MapPawns.PawnsHostileToColony.Cast<ThingWithComps>().ToList();
                    tempPawns = (from p in Find.CurrentMap.mapPawns.AllPawns where IsEnemy(p) select p).Cast<ThingWithComps>().ToList();
                    pStatDef = pawnHumanlikeStatDef;
                    pNeedDef = pawnHumanlikeNeedDef;
                    break;

                case PawnType.Animals:
                    tempPawns = (from p in Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer) where p.RaceProps.Animal select p).Cast<ThingWithComps>().ToList();
                    pStatDef = pawnAnimalStatDef;
                    pNeedDef = pawnAnimalNeedDef;
                    break;

                case PawnType.WildAnimals:
                    tempPawns = (from p in Find.CurrentMap.mapPawns.AllPawns where IsWildAnimal(p) select p).Cast<ThingWithComps>().ToList();
                    pStatDef = pawnAnimalStatDef;
                    pNeedDef = pawnAnimalNeedDef;
                    break;

                case PawnType.Corpses:
                    tempPawns = Find.CurrentMap.listerThings.AllThings.Where(p => (p is Corpse) && (!(p as Corpse).InnerPawn.RaceProps.Animal)).Cast<ThingWithComps>().ToList();
                    pStatDef = corpseStatDef;
                    break;

                case PawnType.AnimalCorpses:
                    tempPawns = Find.CurrentMap.listerThings.AllThings.Where(p => (p is Corpse) && (p as Corpse).InnerPawn.RaceProps.Animal && !p.Position.Fogged(Find.CurrentMap)).Cast<ThingWithComps>().ToList();
                    pStatDef = corpseStatDef;
                    break;
            }

            switch (chosenOrderBy)
            {
                default:
                case OrderBy.Name:
                    this.things = (from p in tempPawns
                        orderby p.LabelCap ascending
                        select p).ToList();
                    break;

                case OrderBy.Column:
                    switch (sortObject.oType)
                    {
                        case KListObject.ObjectType.Stat:
                            this.things = (from p in tempPawns
                                orderby p.GetStatValue((StatDef)sortObject.displayObject, true) ascending
                                select p).ToList();
                            break;

                        case KListObject.ObjectType.Need:

                            this.things = (from p in tempPawns
                                where (p is Pawn) && !(p as Pawn).RaceProps.IsMechanoid && ((p as Pawn).needs != null)
                                orderby ((p as Pawn).needs.TryGetNeed((NeedDef)sortObject.displayObject) != null ? (p as Pawn).needs.TryGetNeed((NeedDef)sortObject.displayObject).CurLevel : 0) ascending
                                select p).ToList();
                            break;

                        case KListObject.ObjectType.Capacity:

                            this.things = (from p in tempPawns
                                where (p is Pawn) && ((p as Pawn).health != null)
                                orderby ((p as Pawn).health.capacities.GetLevel((PawnCapacityDef)sortObject.displayObject)) ascending
                                select p).ToList();
                            break;

                        case KListObject.ObjectType.Skill:
                            this.things = (from p in tempPawns
                                where (p is Pawn) && (p as Pawn).RaceProps.Humanlike && ((p as Pawn).skills != null)
                                orderby (p as Pawn).skills.GetSkill((SkillDef)sortObject.displayObject).XpTotalEarned ascending
                                select p).ToList();
                            break;

                        case KListObject.ObjectType.Gear:
                            this.things = tempPawns.Where(p => (p is Pawn) || ((p is Corpse) && (!(p as Corpse).InnerPawn.RaceProps.Animal))).OrderBy(p => {
                                Pawn p1 = (p is Pawn) ? (p as Pawn) : (p as Corpse).InnerPawn;
                                return (p1.equipment != null) ? ((p1.equipment.AllEquipmentListForReading.Any()) ? p1.equipment.AllEquipmentListForReading.First().LabelCap : "") : "";
                            }).ToList();
                            break;

                        case KListObject.ObjectType.Record:
                            this.things = (from p in tempPawns
                                           where (p is Pawn) && ((p as Pawn).records != null)
                                           orderby ((p as Pawn)).records.GetValue((RecordDef)sortObject.displayObject) ascending
                                           select p).ToList();
                            break;

                        case KListObject.ObjectType.MentalState:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(p => (p as Pawn).MentalState != null ? (p as Pawn).MentalState.ToString() : "").ToList();
                            break;

                        case KListObject.ObjectType.ControlPrisonerInteraction:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(p => (p as Pawn).guest.interactionMode.index).ToList();
                            break;

                        case KListObject.ObjectType.PrisonerRecruitmentDifficulty:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(p => (p as Pawn).RecruitDifficulty(Faction.OfPlayer)).ToList();
                            break;

                        case KListObject.ObjectType.Age:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(p => (p as Pawn).ageTracker.AgeBiologicalYearsFloat).ToList();
                            break;

                        case KListObject.ObjectType.ControlMedicalCare:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(p => (p as Pawn).playerSettings.medCare).ToList();
                            break;

                        case KListObject.ObjectType.CurrentJob:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(p => (p as Pawn).jobs?.curDriver.GetReport()).ToList();
                            break;

                        case KListObject.ObjectType.QueuedJob:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(
                                p => {
                                    string j = "";
                                    if ((p as Pawn).jobs?.curJob != null && (p as Pawn).jobs?.jobQueue.Count > 0)
                                    {
                                        j = (p as Pawn).jobs.jobQueue[0].job.GetReport(p as Pawn);
                                    }
                                    return j;
                                }
                            ).ToList();
                            break;

                        case KListObject.ObjectType.AnimalMilkFullness:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(
                                p => {
                                    float f = -1;
                                    if ((p as Pawn).ageTracker.CurLifeStage.milkable)
                                    {
                                        var comp = p.AllComps.OfType<CompMilkable>().FirstOrDefault();
                                        if (comp != null)
                                            f = comp.Fullness;
                                    }
                                    return f;
                                }
                            ).ToList();
                            break;

                        case KListObject.ObjectType.AnimalWoolGrowth:
                            this.things = tempPawns.Where(p => p is Pawn).OrderBy(
                                p => {
                                    float f = -1;
                                    if ((p as Pawn).ageTracker.CurLifeStage.milkable)
                                    {
                                        var comp = p.AllComps.OfType<CompShearable>().FirstOrDefault();
                                        if (comp != null)
                                            f = comp.Fullness;
                                    }
                                    return f;
                                }
                            ).ToList();
                            break;

                        default:
                            //no way to sort
                            this.things = tempPawns.ToList();
                            break;
                    }

                    break;
            }

            if (pawnListDescending)
            {
                this.things.Reverse();
            }

            isDirty = false;
            pawnListUpdateNext = Find.TickManager.TicksGame + Verse.GenTicks.TickRareInterval;

        }

        public void PawnSelectOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (PawnType pawn in Enum.GetValues(typeof(PawnType)))
            {
                Action action = delegate
                {
                    var component = Find.World.GetComponent<WorldComponent_Numbers>();
                    if (pawn != component.chosenPawnType)
                    {
                        component.savedKLists.TryGetValue(pawn, out kList);
                        if (kList == null)
                        {
                            kList = new List<KListObject>();
                            component.savedKLists[pawn] = kList;
                        }
                        component.chosenPawnType = pawn;
                        isDirty = true;
                    }
                };

                list.Add(new FloatMenuOption(("koisama.pawntype." + pawn.ToString()).Translate(), action, MenuOptionPriority.Default, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public void StatsOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (StatDef stat in pStatDef)
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.Stat, stat.LabelCap, stat);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption(stat.LabelCap, action, MenuOptionPriority.Default, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public void SkillsOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (SkillDef skill in DefDatabase<SkillDef>.AllDefsListForReading)
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.Skill, skill.LabelCap, skill);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption(skill.LabelCap, action, MenuOptionPriority.Default, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public void NeedsOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (NeedDef need in pNeedDef)
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.Need, need.LabelCap, need);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption(need.LabelCap, action, MenuOptionPriority.Default, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public void CapacityOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (PawnCapacityDef pcd in DefDatabase<PawnCapacityDef>.AllDefsListForReading)
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.Capacity, pcd.LabelCap, pcd);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption(pcd.LabelCap, action, MenuOptionPriority.Default, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public void RecordsOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (RecordDef rcd in DefDatabase<RecordDef>.AllDefsListForReading)
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.Record, rcd.LabelCap, rcd);
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption(rcd.LabelCap, action, MenuOptionPriority.Default, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        //presets
        public void PresetOptionsMaker()
        {
            //TODO: Implement ;-)
        }

        //other hardcoded options
        public void OtherOptionsMaker()
        {
            var component = Find.World.GetComponent<WorldComponent_Numbers>();
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            //equipment bearers            
            if (new[] { PawnType.Colonists, PawnType.Prisoners, PawnType.Enemies, PawnType.Corpses }.Contains(component.chosenPawnType))
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.Gear, "koisama.Equipment".Translate(), null);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.Equipment".Translate(), action, MenuOptionPriority.Default, null, null));
            }

            //all living things
            if (new[] { PawnType.Colonists, PawnType.Prisoners, PawnType.Enemies, PawnType.Animals, PawnType.WildAnimals, PawnType.Guests }.Contains(component.chosenPawnType))
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.Age, "koisama.Age".Translate(), null);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.Age".Translate(), action, MenuOptionPriority.Default, null, null));

                action = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.MentalState, "koisama.MentalState".Translate(), null);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.MentalState".Translate(), action, MenuOptionPriority.Default, null, null));

                Action action2 = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.Race, "Race".Translate(), null);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("Race".Translate(), action2, MenuOptionPriority.Default, null, null));
            }

            if (component.chosenPawnType == PawnType.Prisoners)
            {

                Action action2 = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.ControlPrisonerInteraction, "koisama.Interaction".Translate(), null);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.Interaction".Translate(), action2, MenuOptionPriority.Default, null, null));

                Action action3 = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.PrisonerRecruitmentDifficulty, "RecruitmentDifficulty".Translate(), null);
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("RecruitmentDifficulty".Translate(), action3, MenuOptionPriority.Default, null, null));

                Action action4 = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.PrisonerResistance, "RecruitmentResistance".Translate(), null);
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("RecruitmentResistance".Translate(), action4, MenuOptionPriority.Default, null, null));
            }

            if (component.chosenPawnType == PawnType.Animals)
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.AnimalMilkFullness, "MilkFullness".Translate(), null);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("MilkFullness".Translate(), action, MenuOptionPriority.Default, null, null));

                Action action2 = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.AnimalWoolGrowth, "WoolGrowth".Translate(), null);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("WoolGrowth".Translate(), action2, MenuOptionPriority.Default, null, null));

                Action action3 = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.AnimalEggProgress, "EggProgress".Translate(), null);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("EggProgress".Translate(), action3, MenuOptionPriority.Default, null, null));
            }

            //healable
            if (new[] { PawnType.Colonists, PawnType.Prisoners, PawnType.Animals }.Contains(component.chosenPawnType))
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.ControlMedicalCare, "koisama.MedicalCare".Translate(), null);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.MedicalCare".Translate(), action, MenuOptionPriority.Default, null, null));
            }

            if (!new[] { PawnType.Corpses, PawnType.AnimalCorpses }.Contains(component.chosenPawnType))
            {
                Action action = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.CurrentJob, "koisama.CurrentJob".Translate(), null);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.CurrentJob".Translate(), action, MenuOptionPriority.Default, null, null));

                Action action2 = delegate
                {
                    KListObject kl = new KListObject(KListObject.ObjectType.QueuedJob, "koisama.QueuedJob".Translate(), null);
                    //if (fits(kl.minWidthDesired))
                    kList.Add(kl);
                };
                list.Add(new FloatMenuOption("koisama.QueuedJob".Translate(), action2, MenuOptionPriority.Default, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public override void DoWindowContents(Rect r)
        {
            var component = Find.World.GetComponent<WorldComponent_Numbers>();
            maxWindowWidth = Screen.width;
            base.DoWindowContents(r);

            if (pawnListUpdateNext < Find.TickManager.TicksGame)
                isDirty = true;

            if (isDirty)
            {
                UpdatePawnList();
            }

            Rect position = new Rect(0f, 0f, r.width, 115f);
            GUI.BeginGroup(position);

            float x = 0f;
            Text.Font = GameFont.Small;

            //pawn/prisoner list switch
            Rect sourceButton = new Rect(x, 0f, buttonWidth, PawnRowHeight);
            if (Widgets.ButtonText(sourceButton, ("koisama.pawntype." + component.chosenPawnType.ToString()).Translate()))
            {
                PawnSelectOptionsMaker();
            }
            x += buttonWidth + 10;
            TooltipHandler.TipRegion(sourceButton, new TipSignal("koisama.Numbers.ClickToToggle".Translate(), sourceButton.GetHashCode()));

            //stats btn
            Rect addColumnButton = new Rect(x, 0f, buttonWidth, PawnRowHeight);
            if (Widgets.ButtonText(addColumnButton, "koisama.Numbers.AddColumnLabel".Translate()))
            {
                StatsOptionsMaker();
            }
            x += buttonWidth + 10;
            

            //skills btn
            if (new[] { PawnType.Colonists, PawnType.Prisoners, PawnType.Enemies }.Contains(component.chosenPawnType))
            {
                Rect skillColumnButton = new Rect(x, 0f, buttonWidth, PawnRowHeight);
                if (Widgets.ButtonText(skillColumnButton, "koisama.Numbers.AddSkillColumnLabel".Translate()))
                {
                    SkillsOptionsMaker();
                }
                x += buttonWidth + 10;
            }

            if (!new[] { PawnType.AnimalCorpses, PawnType.Corpses }.Contains(component.chosenPawnType))
            {
                //needs btn
                Rect needsColumnButton = new Rect(x, 0f, buttonWidth, PawnRowHeight);
                if (Widgets.ButtonText(needsColumnButton, "koisama.Numbers.AddNeedsColumnLabel".Translate()))
                {
                    NeedsOptionsMaker();
                }
                x += buttonWidth + 10;
            }

            //cap btn
            Rect capacityColumnButton = new Rect(x, 0f, buttonWidth, PawnRowHeight);
            if (Widgets.ButtonText(capacityColumnButton, "koisama.Numbers.AddCapacityColumnLabel".Translate()))
            {
                CapacityOptionsMaker();
            }
            x += buttonWidth + 10;

            if (!new[] { PawnType.AnimalCorpses }.Contains(component.chosenPawnType))
            {
                Rect otherColumnBtn = new Rect(x, 0f, buttonWidth, PawnRowHeight);
                if (Widgets.ButtonText(otherColumnBtn, "koisama.Numbers.AddOtherColumnLabel".Translate()))
                {
                    OtherOptionsMaker();
                }
                x += buttonWidth + 10;
            }

            Rect recordsColumnBtn = new Rect(x, 0f, buttonWidth, PawnRowHeight);
            if (Widgets.ButtonText(recordsColumnBtn, "TabRecords".Translate()))
            {
                RecordsOptionsMaker();
            }
            x += buttonWidth + 10;

            // Scroll stats
            {
                Rect pageDownBtn = new Rect(x, 0f, PawnRowHeight, PawnRowHeight);
                if (Widgets.ButtonText(pageDownBtn, "<<"))
                {
                    listScrollPos -= 8;
                }
                x += PawnRowHeight + 10;

                Rect pageUpBtn = new Rect(x, 0f, PawnRowHeight, PawnRowHeight);
                if (Widgets.ButtonText(pageUpBtn, ">>"))
                {
                    listScrollPos += 8;
                }
                x += PawnRowHeight + 10;

                if (listScrollPos >= kList.Count) listScrollPos = kList.Count - 1;
                if (listScrollPos < 0) listScrollPos = 0;
            }

            //TODO: implement
            /*
            Rect addPresetBtn = new Rect(x, 0f, buttonWidth, PawnRowHeight);
            if (Widgets.ButtonText(addPresetBtn, "koisama.Numbers.SetPresetLabel".Translate()))
            {
                PresetOptionsMaker();
            }
            x += buttonWidth + 10;
            */

            Rect thingCount = new Rect(10f, 45f, 200f, 30f);
            Widgets.Label(thingCount, "koisama.Numbers.Count".Translate() + ": " + this.things.Count().ToString());

            x = 0;
            //names
            Rect nameLabel = new Rect(x, 75f, NameColumnWidth, PawnRowHeight);
            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(nameLabel, "koisama.Numbers.Name".Translate());
            if (Widgets.ButtonInvisible(nameLabel))
            {
                if (chosenOrderBy == OrderBy.Name)
                {
                    pawnListDescending = !pawnListDescending;
                }
                else
                {
                    chosenOrderBy = OrderBy.Name;
                    pawnListDescending = false;
                }
                isDirty = true;
            }

            TooltipHandler.TipRegion(nameLabel, "koisama.Numbers.SortByTooltip".Translate("koisama.Numbers.Name".Translate()));
            Widgets.DrawHighlightIfMouseover(nameLabel);
            x += NameColumnWidth;

            //header
            //TODO: better interface - auto width calculation

            int reorderableGroup = ReorderableWidget.NewGroup(delegate (int from, int to)
            {
                KListObject oldKlistObject = kList[from];
                kList.Insert(to, oldKlistObject);
                kList.RemoveAt((from >= to) ? (from + 1) : from);

            }, ReorderableDirection.Horizontal);

            bool offset = true;
            kListDesiredWidth = 175f;
            Text.Anchor = TextAnchor.MiddleCenter;

            for (int i = listScrollPos; i < kList.Count; i++)
            {
                float colWidth = kList[i].minWidthDesired;

                if (colWidth + kListDesiredWidth + cFreeSpaceAtTheEnd > maxWindowWidth)
                {
                    break;
                }



                kListDesiredWidth += colWidth;

                Rect defLabel = new Rect(x - 35, 25f + (offset ? 10f : 50f), colWidth + 70, 40f);
                Widgets.DrawLine(new Vector2(x + colWidth / 2, 55f + (offset ? 15f : 55f)), new Vector2(x + colWidth / 2, 113f), Color.gray, 1);
                Widgets.Label(defLabel, kList[i].label);

                ReorderableWidget.Reorderable(reorderableGroup, defLabel);

                StringBuilder labelSB = new StringBuilder();
                labelSB.AppendLine("koisama.Numbers.SortByTooltip".Translate(kList[i].label));
                labelSB.AppendLine("koisama.Numbers.RemoveTooltip".Translate());
                labelSB.AppendLine("DragToReorder".Translate());
                TooltipHandler.TipRegion(defLabel, labelSB.ToString());
                Widgets.DrawHighlightIfMouseover(defLabel);

                if (Widgets.ButtonInvisible(defLabel))
                {
                    if (Event.current.button == 1)
                    {
                        kList.RemoveAt(i);
                    }
                    else
                    {

                        if (chosenOrderBy == OrderBy.Column && kList[i].Equals(sortObject))
                        {
                            pawnListDescending = !pawnListDescending;
                        }
                        else
                        {
                            sortObject = kList[i];
                            chosenOrderBy = OrderBy.Column;
                            pawnListDescending = false;
                        }
                    }
                    isDirty = true;
                }
                offset = !offset;
                x += colWidth;
            }
            GUI.EndGroup();

            //content
            Rect content = new Rect(0f, position.yMax, r.width, r.height - position.yMax);
            GUI.BeginGroup(content);
            base.DrawRows(new Rect(0f, 0f, content.width, content.height));
            GUI.EndGroup();
        }

        protected override void DrawPawnRow(Rect r, ThingWithComps p)
        {
            float x = 175f;
            float y = r.yMin;

            Text.Anchor = TextAnchor.MiddleCenter;

            //TODO: better interface - auto width calculation, make sure columns won't overlap
            for (int i = listScrollPos; i < kList.Count; i++)
            {
                float colWidth = kList[i].minWidthDesired;
                if (colWidth + x + cFreeSpaceAtTheEnd > maxWindowWidth)
                {
                    //soft break
                    break;
                }
                Rect capCell = new Rect(x, y, colWidth, PawnRowHeight);
                kList[i].Draw(capCell, p);
                x += colWidth;
            }

            /*
            if (p.health.Downed) {
                Widgets.DrawLine(new Vector2(5f, y + PawnRowHeight / 2), new Vector2(r.xMax - 5f, y + PawnRowHeight / 2), Color.red, 1);
            }*/

        }

    }
}