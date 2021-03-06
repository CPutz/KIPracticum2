﻿using System;
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

            float alpha = float.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture);
            float gamma = float.Parse(args[4], System.Globalization.CultureInfo.InvariantCulture);

            //initialize Q-Learning
            QLearning learn = new QLearning();
            learn.LoadFile(learnFile);


            int reward = GetReward(gamelog);


            FileStream fs = new FileStream(lastStateFile, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            //read all last states from the laststate file.
            //
            //file format:
            //ulong (state before action was taken)
            //byte (action that was taken)
            //ulong (state after action was taken)
            while (fs.Position < fs.Length) {
                State oldState = new State((uint)br.ReadInt64());
                Action action = (Action)br.ReadByte();
                State newState = new State((uint)br.ReadInt64());

                //process reward for every action taken in last turn
                learn.ProcessReward(reward, oldState, newState, action, alpha, gamma);
            }

            br.Close();
            fs.Close();

            learn.SaveFile(learnFile);


            //check whether a log file is passed as a parameter to the program, if so:
            //write the reward and length of the game to the log file.
            if (args.Length > 5) {
                string log = args[5];

                using(StreamWriter sw = new StreamWriter(log, true)) {

                    sw.WriteLine(reward + "\t" + GetGameLength(gamelog));
                }
            }
        }


        /// <summary>
        /// Gets the reward: our score minus enemy score, from the gamelog.
        /// </summary>
        /// <param name="gamelog">The path of the gamelog.</param>
        /// <returns><c>3</c> for winning, <c>0</c> for a draw and <c>-3</c> for losing.</returns>
        static int GetReward(string gamelog) {

            FileStream fs = new FileStream(gamelog, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            string s = sr.ReadToEnd();

            sr.Close();
            fs.Close();


            //match for "score": [, and then two numbers, ended by ].
            string pattern = "\"score\": \\[([0-9]+), ([0-9]+)\\]";

            Match m = Regex.Match(s, pattern);
            string score1 = m.Groups[1].Value;
            string score2 = m.Groups[2].Value;

            return int.Parse(score1) - int.Parse(score2);
        }


        /// <summary>
        /// Gets the length of the last game from the gamelog.
        /// </summary>
        /// <param name="gamelog">The path of the gamelog.</param>
        /// <returns>The length of the last game.</returns>
        static int GetGameLength(string gamelog) {
            FileStream fs = new FileStream(gamelog, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            string s = sr.ReadToEnd();

            sr.Close();
            fs.Close();


            //match for "game_length": , and then a number
            string pattern = "\"game_length\": ([0-9]+),";

            Match m = Regex.Match(s, pattern);
            string length = m.Groups[1].Value;

            return int.Parse(length);
        }
    }
}
