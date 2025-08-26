using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Numbers
{

    public class PawnColumnWorker_JobQueued : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            var jobs = GetJobs(pawn);
            if (jobs.Count == 0) return null;

            string text = jobs[0];
            GenText.SetTextSizeToFit(text, new Rect(0f, 0f, Mathf.CeilToInt(Text.CalcSize(def.LabelCap).x), GetMinCellHeight(pawn)));

            return text;
        }

        public override int Compare(Pawn a, Pawn b)
            => (a.jobs?.jobQueue?.Count ?? 0).CompareTo(b.jobs?.jobQueue?.Count ?? 0);

        protected override string GetTip(Pawn pawn)
        {
            var jobs = GetJobs(pawn);
            if (jobs.Count == 0) return null;

            var sb = new StringBuilder();
            int width = jobs.Count.ToString().Length;
            for (int i = 0; i < jobs.Count; i++) sb.AppendLine($"{(i + 1).ToString().PadLeft(width)}. {jobs[i]}");

            return sb.ToString().TrimEnd();
        }

        private List<string> GetJobs(Pawn pawn)
        {
            var result = new List<string>();
            if (pawn.jobs?.jobQueue.Any() ?? false)
            {
                // queued jobs
                foreach (var queued in pawn.jobs.jobQueue) result.Add(queued.job.GetReport(pawn).CapitalizeFirst());
            }
            return result;
        }

        public override int GetMinWidth(PawnTable table)
            => Mathf.Max(base.GetMinWidth(table), 200);

        public override int GetMinHeaderHeight(PawnTable table)
            => Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, GetMinWidth(table))).y);
    }
}
