using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RWGallary
{
    public class CustomLetter : StandardLetter
    {
        public string sourceUrl;
        public Texture2D image;
        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                yield return base.Option_Close;
                DiaOption openUrl = new DiaOption("원문 보기");
                openUrl.action = () =>
                {
                    Application.OpenURL(sourceUrl);
                    if (!Find.TickManager.Paused)
                        Find.TickManager.Pause();
                };
                yield return openUrl;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref sourceUrl, nameof(sourceUrl));
        }

        public override void OpenLetter()
        {
            DiaNode node = new DiaNode(this.Text);
            node.options.AddRange(Choices);
            Dialog_NodeTreeWithImage dialog = new Dialog_NodeTreeWithImage(node, Settings.PauseWhenOpenMessage, image);
            Find.WindowStack.Add(dialog);
        }


        public static CustomLetter MakeLetter(TaggedString label, TaggedString text, string sourceUrl, LetterDef def, Texture2D image = null)
        {
            var letter = new CustomLetter();
            letter.def = def;
            letter.ID = Find.UniqueIDsManager.GetNextLetterID();
            letter.Label = label;
            letter.Text = text;
            letter.sourceUrl = sourceUrl;
            letter.image = image;
            return letter;
        }
    }
}
