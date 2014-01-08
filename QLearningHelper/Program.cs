using System;
using System.Collections.Generic;
using System.IO;

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

            int reward = GetReward(gamelog);


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

            int index = s.IndexOf("\"score\": ") + 10;
            int end = s.IndexOf(']', index);

            string scoreString = s.Substring(index, end - index);

            string[] scoreStrings = scoreString.Split();
            scoreStrings[0] = scoreStrings[0].Replace(",", String.Empty);

            sr.Close();
            fs.Close();

            return int.Parse(scoreStrings[0]) - int.Parse(scoreStrings[1]);
        }
    }
}
