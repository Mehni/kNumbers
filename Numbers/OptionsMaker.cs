namespace Numbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class OptionsMaker
    {
        private readonly MainTabWindow_Numbers numbers;
        private readonly Numbers_Settings settings;

        private PawnTableDef PawnTable
        {
            get => numbers.pawnTableDef;
            set => numbers.pawnTableDef = value;
        }

        //these should be Defs, probably
        private static IEnumerable<PawnColumnDef> EquipmentBearers
            => new[] { DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Equipment") };

        private static IEnumerable<PawnColumnDef> LivingThings
            => new[] { DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Age"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_MentalState"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_JobCurrent"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_JobQueued"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_HediffList") };

        private static IEnumerable<PawnColumnDef> Prisoners
            => new[] { DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_PrisonerInteraction"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_PrisonerRecruitmentDifficulty"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_PrisonerResistance"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("FoodRestriction"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Inventory") };

        private static IEnumerable<PawnColumnDef> Animals
            => new[] { DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Milkfullness"),
                        DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_AnimalWoolGrowth"),
                        DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_AnimalEggProgress"),
                        DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Wildness"),
                        DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_TameChance"),
                        DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Inventory"),
                        DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_AnimalFilthRate"), }
               .Concat(DefDatabase<PawnTableDef>.GetNamed("Animals").columns.Where(x => pcdValidator(x)));

        private static IEnumerable<PawnColumnDef> MainTable
            => new[] { DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Inspiration"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Inventory"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_SelfTend"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Meditation"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Psyfocus"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Entropy"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_PsylinkLevel") }
               .Concat(DefDatabase<PawnTableDef>.GetNamed("Assign").columns)
               .Concat(DefDatabase<PawnTableDef>.GetNamed("Restrict").columns).Where(x => pcdValidator(x) && filterRoyalty(x));

        private static readonly Func<PawnColumnDef, bool> filterRoyalty = pcd => ModLister.RoyaltyInstalled || !pcd.HasModExtension<DefModExtension_NeedsRoyalty>();

        private static IEnumerable<PawnColumnDef> WildAnimals
            => new[] { DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Wildness"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_TameChance"),
                         DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_ManhunterOnTameFailChance") }
               .Concat(DefDatabase<PawnTableDef>.GetNamed("Wildlife").columns.Where(x => pcdValidator(x)));

        private static IEnumerable<PawnColumnDef> DeadThings
            => new[] { DefDatabase<PawnColumnDef>.GetNamedSilentFail(defName: "Numbers_Forbidden") };

        public OptionsMaker(MainTabWindow_Numbers mainTabWindow)
        {
            numbers = mainTabWindow;
            settings = LoadedModManager.GetMod<Numbers>().GetSettings<Numbers_Settings>();
        }

        public List<FloatMenuOption> PresetOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>
            {
                new FloatMenuOption("Numbers_SaveCurrentLayout".Translate(), Save),
                new FloatMenuOption("Numbers_LoadSavedLayout".Translate(), Load),
                new FloatMenuOption("Numbers_Presets.Load".Translate("Numbers_Presets.Medical".Translate()), () => ChangeMainTableTo(StaticConstructorOnGameStart.medicalPreset)),
                new FloatMenuOption("Numbers_Presets.Load".Translate("Numbers_Presets.Combat".Translate()), () => ChangeMainTableTo(StaticConstructorOnGameStart.combatPreset)),
                new FloatMenuOption("Numbers_Presets.Load".Translate("Numbers_Presets.WorkTabPlus".Translate()), () => ChangeMainTableTo(StaticConstructorOnGameStart.workTabPlusPreset)),
                new FloatMenuOption("Numbers_Presets.Load".Translate("Numbers_Presets.ColonistNeeds".Translate()), () => ChangeMainTableTo(StaticConstructorOnGameStart.colonistNeedsPreset)),
                //maybe Psycasting here, index 6, referenced below
                new FloatMenuOption("Numbers_SetAsDefault".Translate(), SetAsDefault,
                        extraPartWidth: 29f,
                        extraPartOnGUI: rect => Numbers_Utility.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2, "Numbers_SetAsDefaultExplanation".Translate(PawnTable.LabelCap))),
                new FloatMenuOption("Numbers_LoadDefault".Translate(), LoadDefault)
            };
            if (ModLister.RoyaltyInstalled)
            {
                list.Insert(6, new FloatMenuOption("Numbers_Presets.Load".Translate("Numbers_Presets.Psycasting".Translate()), () => ChangeMainTableTo(StaticConstructorOnGameStart.psycastingPreset)));
            }

            return list;
        }

        private IEnumerable<FloatMenuOption> General()
        {
            yield return new FloatMenuOption("Race".Translate(), () => AddPawnColumnAtBestPositionAndRefresh(DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Race")));
            yield return new FloatMenuOption("Faction".Translate(), () => AddPawnColumnAtBestPositionAndRefresh(DefDatabase<PawnColumnDef>.GetNamedSilentFail("Numbers_Faction")));
            yield return new FloatMenuOption("Gender", () => AddPawnColumnAtBestPositionAndRefresh(DefDatabase<PawnColumnDef>.GetNamedSilentFail("Gender")));
        }

        public List<FloatMenuOption> FloatMenuOptionsFor(IEnumerable<PawnColumnDef> pcdList)
            => pcdList.Select(pcd => new FloatMenuOption(GetBestLabelForPawnColumn(pcd), () => AddPawnColumnAtBestPositionAndRefresh(pcd))).ToList();

        public List<FloatMenuOption> OtherOptionsMaker()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            list.AddRange(General());

            //equipment bearers
            if (new[] { NumbersDefOf.Numbers_MainTable,
                        NumbersDefOf.Numbers_Prisoners,
                        NumbersDefOf.Numbers_Enemies,
                        NumbersDefOf.Numbers_Corpses
                      }.Contains(PawnTable))
            {
                list.AddRange(FloatMenuOptionsFor(EquipmentBearers));
            }

            //all living things
            if (!new[] { NumbersDefOf.Numbers_AnimalCorpses, NumbersDefOf.Numbers_Corpses }.Contains(PawnTable))
            {
                list.AddRange(FloatMenuOptionsFor(LivingThings));
            }

            if (PawnTable == NumbersDefOf.Numbers_Prisoners)
            {
                list.AddRange(FloatMenuOptionsFor(Prisoners));
            }

            if (PawnTable == NumbersDefOf.Numbers_Animals)
            {
                list.AddRange(FloatMenuOptionsFor(Animals));
            }

            if (PawnTable == NumbersDefOf.Numbers_MainTable)
            {
                list.AddRange(FloatMenuOptionsFor(MainTable));
            }

            if (PawnTable == NumbersDefOf.Numbers_WildAnimals)
            {
                list.AddRange(FloatMenuOptionsFor(WildAnimals));
            }

            //all dead things
            if (new[] { NumbersDefOf.Numbers_AnimalCorpses, NumbersDefOf.Numbers_Corpses }.Contains(PawnTable))
            {
                list.AddRange(FloatMenuOptionsFor(DeadThings));
            }

            return list;
        }

        public List<FloatMenuOption> OptionsMakerForGenericDef<T>(IEnumerable<T> listOfDefs) where T : Def
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            foreach (var defCurrent in listOfDefs)
            {
                void Action()
                {
                    PawnColumnDef pcd = DefDatabase<PawnColumnDef>.GetNamedSilentFail(HorribleStringParsersForSaving.CreateDefNameFromType(defCurrent));
                    AddPawnColumnAtBestPositionAndRefresh(pcd);
                }
                string label = defCurrent.LabelCap;
                list.Add(new FloatMenuOption(label, Action));
            }

            return list;
        }

        public List<FloatMenuOption> PawnSelector()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (KeyValuePair<PawnTableDef, Func<Pawn, bool>> filter in WorldComponent_Numbers.PrimaryFilter)
            {
                void Action()
                {
                    if (filter.Value != MainTabWindow_Numbers.filterValidator.First())
                    {
                        PawnTable = filter.Key;

                        if (Find.World.GetComponent<WorldComponent_Numbers>().sessionTable.TryGetValue(filter.Key, out List<PawnColumnDef> listPawnColumDef))
                            PawnTable.columns = listPawnColumDef;

                        numbers.UpdateFilter();
                        numbers.Notify_ResolutionChanged();
                    }
                }
                list.Add(new FloatMenuOption(filter.Key.label, Action));
            }
            return list;
        }

        private void Save()
        {
            //not actually saved like this, just the easiest way to pass it around
            PawnTableDef ptdPawnTableDef = new PawnTableDef
            {
                columns = PawnTable.columns,
                modContentPack = PawnTable.modContentPack,
                workerClass = PawnTable.workerClass,
                defName = PawnTable.defName,
                label = "NumbersTable" + Rand.Range(0, 10000)
            };
            Find.WindowStack.Add(new Dialog_IHaveToCreateAnEntireFuckingDialogForAGODDAMNOKAYBUTTONFFS(ref ptdPawnTableDef));
        }

        private void Load()
        {
            List<FloatMenuOption> loadOptions = new List<FloatMenuOption>();
            foreach (string tableDefToBe in settings.storedPawnTableDefs)
            {
                void ApplySetting()
                {
                    PawnTableDef ptD = HorribleStringParsersForSaving.TurnCommaDelimitedStringIntoPawnTableDef(tableDefToBe);

                    PawnTable = DefDatabase<PawnTableDef>.GetNamed(ptD.defName);
                    PawnTable.columns = ptD.columns;

                    numbers.UpdateFilter();
                    numbers.RefreshAndStoreSessionInWorldComp();
                }
                string label = tableDefToBe.Split(',')[1] == "Default" ? tableDefToBe.Split(',')[0].Split('_')[1] + " (" + tableDefToBe.Split(',')[1] + ")" : tableDefToBe.Split(',')[1];
                loadOptions.Add(new FloatMenuOption(label, ApplySetting));
            }

            if (loadOptions.NullOrEmpty())
                loadOptions.Add(new FloatMenuOption("Numbers_NothingSaved".Translate(), null));

            Find.WindowStack.Add(new FloatMenu(loadOptions));
        }

        private void ChangeMainTableTo(List<PawnColumnDef> list)
        {
            PawnTable = NumbersDefOf.Numbers_MainTable;
            PawnTable.columns = new List<PawnColumnDef>(list);
            numbers.UpdateFilter();
            numbers.Notify_ResolutionChanged();
        }

        private void SetAsDefault()
        {
            string pawnTableDeftoSave = HorribleStringParsersForSaving.TurnPawnTableDefIntoCommaDelimitedString(PawnTable, true);
            settings.StoreNewPawnTableDef(pawnTableDeftoSave);
        }

        private void LoadDefault()
        {
            bool foundSomething = false;
            foreach (string tableDefToBe in settings.storedPawnTableDefs)
            {
                string[] ptdToBe = tableDefToBe.Split(',');
                if (ptdToBe[1] == "Default" && PawnTable.defName == ptdToBe[0])
                {
                    foundSomething = true;
                    PawnTableDef ptD = HorribleStringParsersForSaving.TurnCommaDelimitedStringIntoPawnTableDef(tableDefToBe);

                    PawnTable = DefDatabase<PawnTableDef>.GetNamed(ptD.defName);
                    PawnTable.columns = ptD.columns;
                    numbers.UpdateFilter();
                    numbers.RefreshAndStoreSessionInWorldComp();
                    break;
                }
            }
            if (!foundSomething)
                Messages.Message("Numbers_NoDefaultStoredForThisView".Translate(), MessageTypeDefOf.RejectInput);
        }

        private static string GetBestLabelForPawnColumn(PawnColumnDef pcd)
        {
            if (pcd == null)
                return string.Empty;

            if (pcd.workType != null)
                return pcd.workType.labelShort;

            if (!pcd.LabelCap.NullOrEmpty())
                return pcd.LabelCap;

            if (!pcd.headerTip.NullOrEmpty())
                return pcd.headerTip;

            return pcd.defName;
        }

        private void AddPawnColumnAtBestPositionAndRefresh(PawnColumnDef pcd)
        {
            if (pcd == null)
                return;
            int lastIndex = PawnTable.columns.FindLastIndex(x => x.Worker is PawnColumnWorker_RemainingSpace);
            PawnTable.columns.Insert(Mathf.Max(1, lastIndex), pcd);

            numbers.RefreshAndStoreSessionInWorldComp();
        }

        private static readonly Func<PawnColumnDef, bool> pcdValidator = pcd => !(pcd.Worker is PawnColumnWorker_Gap)
                                && !(pcd.Worker is PawnColumnWorker_Label) && !(pcd.Worker is PawnColumnWorker_RemainingSpace)
                                && !(pcd.Worker is PawnColumnWorker_CopyPaste) && !(pcd.Worker is PawnColumnWorker_MedicalCare)
                                && !(pcd.Worker is PawnColumnWorker_Timetable) || (!(pcd.label.NullOrEmpty() && pcd.HeaderIcon == null)
                                && !pcd.HeaderInteractable);
        //basically all that are already present, don't have an interactable header, and uh
    }
}
