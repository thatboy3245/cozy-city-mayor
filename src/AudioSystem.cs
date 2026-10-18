using System;
using System.Collections.Generic;
using System.IO;
using System.Media;

namespace CozyCityMayor
{
    public class AudioSystem
    {
        public bool Muted { get; set; }
        private readonly Dictionary<string, byte[]> _wavCache = new Dictionary<string, byte[]>();

        public AudioSystem()
        {
            Muted = false;
            PrebuildSounds();
        }

        private void PrebuildSounds()
        {
            _wavCache["click"] = GenerateSequenceWav(new SynthNote[]
            {
                new SynthNote(587.33, 0.04, 0.25),
                new SynthNote(880.0, 0.05, 0.20)
            });

            _wavCache["blip"] = GenerateSequenceWav(new SynthNote[]
            {
                new SynthNote(440.0, 0.025, 0.14),
                new SynthNote(523.25, 0.025, 0.14)
            });

            _wavCache["coin"] = GenerateSequenceWav(new SynthNote[]
            {
                new SynthNote(987.77, 0.07, 0.30),
                new SynthNote(1318.51, 0.18, 0.32)
            });

            _wavCache["build"] = GenerateSequenceWav(new SynthNote[]
            {
                new SynthNote(523.25, 0.08, 0.28),
                new SynthNote(659.25, 0.08, 0.28),
                new SynthNote(783.99, 0.08, 0.28),
                new SynthNote(1046.50, 0.22, 0.32)
            });

            _wavCache["fanfare"] = GenerateSequenceWav(new SynthNote[]
            {
                new SynthNote(523.25, 0.10, 0.32),
                new SynthNote(523.25, 0.10, 0.32),
                new SynthNote(523.25, 0.10, 0.32),
                new SynthNote(659.25, 0.22, 0.35),
                new SynthNote(587.33, 0.12, 0.32),
                new SynthNote(659.25, 0.12, 0.32),
                new SynthNote(783.99, 0.35, 0.38)
            });

            _wavCache["police"] = GenerateSequenceWav(new SynthNote[]
            {
                new SynthNote(740.0, 0.06, 0.26),
                new SynthNote(880.0, 0.06, 0.26),
                new SynthNote(740.0, 0.08, 0.26)
            });

            _wavCache["weather"] = GenerateSequenceWav(new SynthNote[]
            {
                new SynthNote(440.0, 0.07, 0.24),
                new SynthNote(554.37, 0.07, 0.24),
                new SynthNote(659.25, 0.07, 0.24),
                new SynthNote(880.0, 0.18, 0.26)
            });

            _wavCache["page"] = GenerateSequenceWav(new SynthNote[]
            {
                new SynthNote(330.0, 0.03, 0.18),
                new SynthNote(392.0, 0.04, 0.16)
            });
        }

        public void Play(string soundKey)
        {
            if (Muted) return;
            byte[] wavBytes;
            if (!_wavCache.TryGetValue(soundKey, out wavBytes)) return;

            try
            {
                var ms = new MemoryStream(wavBytes);
                var player = new SoundPlayer(ms);
                player.Play();
            }
            catch { }
        }

        private byte[] GenerateSequenceWav(SynthNote[] notes)
        {
            const int sampleRate = 22050;
            int totalSamples = 0;
            foreach (var n in notes)
            {
                totalSamples += (int)(sampleRate * n.DurationSec);
            }

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                int dataChunkSize = totalSamples * 2;
                bw.Write(new char[] { 'R', 'I', 'F', 'F' });
                bw.Write(36 + dataChunkSize);
                bw.Write(new char[] { 'W', 'A', 'V', 'E' });
                bw.Write(new char[] { 'f', 'm', 't', ' ' });
                bw.Write(16);
                bw.Write((short)1);
                bw.Write((short)1);
                bw.Write(sampleRate);
                bw.Write(sampleRate * 2);
                bw.Write((short)2);
                bw.Write((short)16);
                bw.Write(new char[] { 'd', 'a', 't', 'a' });
                bw.Write(dataChunkSize);

                double phase = 0.0;
                foreach (var n in notes)
                {
                    int noteSamples = (int)(sampleRate * n.DurationSec);
                    for (int i = 0; i < noteSamples; i++)
                    {
                        double env = 1.0;
                        if (i < noteSamples * 0.1) env = i / (noteSamples * 0.1);
                        else if (i > noteSamples * 0.7) env = (noteSamples - i) / (noteSamples * 0.3);

                        double s1 = Math.Sin(phase);
                        double s2 = Math.Sin(phase * 2.0) * 0.25;
                        double s3 = (Math.Sin(phase) >= 0 ? 0.2 : -0.2);
                        double sample = (s1 + s2 + s3) * 0.65 * n.Volume * env;

                        short pcm = (short)Math.Max(-32000, Math.Min(32000, sample * 32767));
                        bw.Write(pcm);
                        phase += 2.0 * Math.PI * n.Freq / sampleRate;
                    }
                }

                return ms.ToArray();
            }
        }
    }
}
