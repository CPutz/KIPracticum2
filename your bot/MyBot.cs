using System;
using System.Collections.Generic;
using System.IO;

namespace Ants {

	class MyBot : Bot {
        private string learnFile;
        private string lastStateFile;

        private QLearning learn;

        private List<StateAction>[] lastState;

        private float alpha;
        private float gamma;
        private float rho;


        public MyBot(string learnFile, string lastStateFile) {
            this.learnFile = learnFile;
            this.lastStateFile = lastStateFile;

            this.learn = new QLearning();
            this.learn.LoadFile(this.learnFile);

            this.lastState = new List<StateAction>[144];

            this.alpha = 0.3f;
            this.gamma = 0.1f;
            this.rho = 0.1f;
        }


		// DoTurn is run once per turn
		public override void DoTurn (IGameState state) {

            //CheckRewards(state);

            this.lastState = new List<StateAction>[144];


			// loop through all my ants and try to give them orders
			foreach (Ant ant in state.MyAnts) {

                State s = GetState(state, ant);
                Action a = this.learn.GetAction(s, this.rho);

                Location newLoc;
                if (a != Action.None) {
                    Direction direction = (Direction)a;

                    this.IssueOrder(ant, direction);

                    newLoc = state.GetDestination(ant, direction);
                } else {
                    newLoc = ant;
                }


                byte position = newLoc.ToByte();

                if (this.lastState[position] == null) {
                    this.lastState[position] = new List<StateAction>();
                }

                StateAction sa = new StateAction();
                sa.State = s;
                sa.Action = a;
                this.lastState[position].Add(sa);

				
				// check if we have time left to calculate more orders
				if (state.TimeRemaining < 10) break;
			}


            this.SaveLastState(state);
		}


        private void SaveLastState(IGameState gameState) {
            FileStream fs = new FileStream(this.lastStateFile, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            for (int row = 0; row < gameState.Height; ++row) {
                for (int col = 0; col < gameState.Width; ++col) {
                    Location location = new Location(row, col);
                    byte position = location.ToByte();
                    
                    if (this.lastState[position] != null) {
                        State state = GetState(gameState, location);

                        foreach (StateAction sa in this.lastState[position]) {
                            bw.Write(sa.State.Value);
                            bw.Write((byte)sa.Action);
                            bw.Write(state.Value);
                        }
                    }
                }
            }

            bw.Close();
            fs.Close();
        }


        private void CheckRewards(IGameState state) {

            /*foreach (Location location in state.MyDeads) {
                byte position = location.ToByte();

                List<StateAction> moves = this.doneMoves[position];
                State newState = GetState(state, location);

                foreach (StateAction sa in moves) {
                    this.learn.ProcessReward(-0.1f, sa.State, newState, sa.Action, this.alpha, this.gamma);
                }
            }*/
        }



        private Location[] statePositions = new Location[] { new Location(1, -1), new Location(1, 0),
                                                            new Location(1, 1), new Location(0, -1),
                                                            new Location(0, 1), new Location(-1, -1),
                                                            new Location(-1, 0), new Location(-1, 1),
                                                            new Location(2, 0), new Location(0, 2),
                                                            new Location(-2, 0), new Location(0, -2) };


        public State GetState(IGameState state, Location location) {

            QTile[] tiles = new QTile[statePositions.Length];
            for (int i = 0; i < statePositions.Length; ++i) {

                Location loc = state.GetDestination(location, statePositions[i]);
                tiles[i] = state[loc].ToQTile();
            }

            byte position = location.ToByte();

            return new State(tiles, position);
        }



        public static void Main(string[] args) {
/*#if DEBUG
            System.Diagnostics.Debugger.Launch();
            while (!System.Diagnostics.Debugger.IsAttached) { }
#endif*/

			new Ants().PlayGame(new MyBot(args[0], args[1]));
		}

	}
	
}