using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RWGallary
{
    public class Dialog_NodeTreeWithImage : Dialog_NodeTree
    {
        private float optTotalHeight;
        private Vector2 scrollPosition;
        private Vector2 optsScrollPosition;
        private Texture2D image;

        public Dialog_NodeTreeWithImage(DiaNode nodeRoot, bool pause = false, Texture2D image = null) : base(nodeRoot, false, false, null)
        {
            if (pause)
            {
                this.forcePause = true;
                this.absorbInputAroundWindow = true;
                this.preventCameraMotion = true;
                this.soundAmbient = SoundDefOf.RadioComms_Ambience;
            }
            else
            {
                this.forcePause = false;
                this.absorbInputAroundWindow = false;
                this.preventCameraMotion = false;
            }

            this.resizeable = true;
            this.draggable = true;
            this.image = image;
            this.onlyOneOfTypeAllowed = false;
            this.doCloseX = true;
            // this.closeAction
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = inRect.AtZero();
            if (this.title != null)
            {
                Text.Font = GameFont.Small;
                Rect rect2 = rect;
                rect2.height = 36f;
                rect.yMin += 53f;
                Widgets.DrawTitleBG(rect2);
                rect2.xMin += 9f;
                rect2.yMin += 5f;
                Widgets.Label(rect2, this.title);
            }
            this.DrawNodeWithImage(rect);
        }

        private void DrawNodeWithImage(Rect rect)
        {
            Widgets.BeginGroup(rect);
            Text.Font = GameFont.Small;
            float num = Mathf.Min(this.optTotalHeight, rect.height - 100f - this.Margin * 2f);
            Rect outRect = new Rect(0f, 0f, rect.width, rect.height - num);

            float width = rect.width - 16f;

            // 이미지를 여러개 표시하는 방법
            // text에 이미지를 넣는 위치 정보를 넣는다 => {image} 이런 식으로
            // 이미지를 여러 개 전달한다
            // 

            Rect rectTexture = new Rect(0.0f, 0.0f, (image?.width ?? 0f), (image?.height ?? 0f));
            if (image != null)
            {
                float multiplier = width / rectTexture.width;
                    //((double)rectTexture.width / rectTexture.height >= (double)width / (rect.height - num)
                    //    ? width / rectTexture.width
                    //    : width / (rectTexture.height - num));
                rectTexture.width *= multiplier;
                rectTexture.height *= multiplier;
                //rectTexture.x = (float)(rect.x + rect.width / 2.0 - rectTexture.width / 2.0);
                //rectTexture.y = (float)(rect.y + rect.height / 2.0 - rectTexture.height / 2.0);
            }

            float height = Text.CalcHeight(this.curNode.text, width) + (image != null ? rectTexture.height : 0f);
            Rect rect2 = new Rect(0f, 0f, width, height);
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect2);
            if (image != null)
            {
                GUI.DrawTexture(rectTexture, image);
                rect2.y += rectTexture.height;
            }
            Widgets.Label(rect2, this.curNode.text.Resolve());
            Widgets.EndScrollView();
            Widgets.BeginScrollView(new Rect(0f, rect.height - num, rect.width, num), ref this.optsScrollPosition, new Rect(0f, 0f, rect.width - 16f, this.optTotalHeight), true);
            float num2 = 0f;
            float num3 = 0f;
            for (int i = 0; i < this.curNode.options.Count; i++)
            {
                Rect rect3 = new Rect(15f, num2, rect.width - 30f, 999f);
                float num4 = this.curNode.options[i].OptOnGUI(rect3);
                num2 += num4 + 7f;
                num3 += num4 + 7f;
            }
            if (Event.current.type == EventType.Layout)
            {
                this.optTotalHeight = num3;
            }
            Widgets.EndScrollView();
            Widgets.EndGroup();
        }
    }
}
