using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RayTracer.Composition
{
    public sealed class AnimationPreprocessor
    {
        private static readonly NumberFormatInfo nfi = CultureInfo.InvariantCulture.NumberFormat;

        private readonly List<IChunk> elements;

        public int Frames { get; private set; }

        private interface IChunk
        {
            string ToStr(int frame);
        }

        private class TextBlock : IChunk
        {
            private readonly string text;

            public TextBlock(string text)
            {
                this.text = text;
            }

            public string ToStr(int frame)
            {
                return text;
            }
        }

        private class Animation : IChunk
        {
            private readonly float start;
            private readonly float end;
            private readonly int frames;

            public Animation(float start, int frames, float end)
            {
                this.start = start;
                this.frames = frames;
                this.end = end;
            }

            public string ToStr(int frame)
            {
                float f = end;
                if (frame < frames) f = start + (end - start) * (frame / (float)frames);
                return f.ToString(nfi);
            }
        }

        public AnimationPreprocessor(string scene)
        {
            elements = new List<IChunk>();
            Frames = 1;
            while (true)
            {
                int pos = scene.IndexOf('{');
                if (pos == -1) break;
                int end = scene.IndexOf('}');
                if (end == -1) throw new Exception("Missing closing bracket }");
                string[] animTxt = scene.Substring(pos + 1, end - pos - 1).Split(';');
                float animStart = Convert.ToSingle(animTxt[0]);
                int frames = Convert.ToInt32(animTxt[1]);
                float animEnd = Convert.ToSingle(animTxt[2]);
                Frames = Math.Max(frames, frames);
                elements.Add(new TextBlock(scene[..pos]));
                elements.Add(new Animation(animStart, frames, animEnd));
                scene = scene[(end + 1)..];
            }
            if (scene != "") elements.Add(new TextBlock(scene));
        }


        public string GetFrame(int frame)
        {
            return String.Join("", elements.Select(e => e.ToStr(frame)));
        }
    }
}
