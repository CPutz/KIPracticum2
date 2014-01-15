using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace QLearningHelper {
    class Program {
        static void Main(string[] args) {

/*#if DEBUG
            System.Diagnostics.Debugger.Launch();
            while (!System.Diagnostics.Debugger.IsAttached) { }
#endif*/

            string learnFile = args[0];
            string lastStateFile = args[1];
            string gamelog = args[2];
            

            float alpha = 0.3f;
            float gamma = 0.1f;

            float reward = GetReward(gamelog);

            QLearning learn = new QLearning();
            learn.LoadFile(learnFile);


            FileStream fs = new FileStream(lastStateFile, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            while (fs.Position < fs.Length) {
                State oldState = new State((uint)br.ReadInt32());
                Action action = (Action)br.ReadByte();
                State newState = new State((uint)br.ReadInt32());

                learn.ProcessReward(reward, oldState, newState, action, alpha, gamma);
            }

            br.Close();
            fs.Close();

            learn.SaveFile(learnFile);
        }

        static int GetReward(string gamelog) {

            FileStream fs = new FileStream(gamelog, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            string s = sr.ReadToEnd();

            sr.Close();
            fs.Close();

            string pattern = "\"score\": \\[([0-9]+), ([0-9]+)\\]";

            Match m = Regex.Match(s, pattern);
            string score1 = m.Groups[1].Value;
            string score2 = m.Groups[2].Value;

            return int.Parse(score1) - int.Parse(score2);
        }
    }
}
