using System;
using System.Collections.Generic;
using System.IO;

namespace Ants {

	class MyBot : Bot {
        private string learnFile;
        private string lastStateFile;

        private QLearning learn;

        private StateAction[] lastState;

        private float alpha;
        private float gamma;
        private float rho;


        public MyBot(string learnFile, string lastStateFile) {
            this.learnFile = learnFile;
            this.lastStateFile = lastStateFile;

            this.learn = new QLearning();
            this.learn.LoadFile(this.learnFile);

            this.lastState = new StateAction[144];

            this.alpha = 0.3f;
            this.gamma = 0.1f;
            this.rho = 0.1f;
        }


		// DoTurn is run once per turn
		public override void DoTurn (IGameState state) {

            ProcessRewards(state);

            // erase old state data since we start a new turn
            this.lastState = new StateAction[144];


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


                //If the move will be blocked, the new location of the ant 
                //is just were the ant is right now.
                if (state[newLoc] == Tile.Water) {
                    newLoc = ant;
                }


                byte position = newLoc.ToByte();

                this.lastState[position] = new StateAction();
                this.lastState[position].State = s;
                this.lastState[position].Action = a;


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
                        StateAction sa = this.lastState[position];

                        bw.Write(sa.State.Value);
                        bw.Write((byte)sa.Action);
                        bw.Write(state.Value);
                    }
                }
            }

            bw.Close();
            fs.Close();
        }


        private void ProcessRewards(IGameState gameState) {
            Location enemyHill = new Location(2, 8);

            for (int row = 0; row < gameState.Height; ++row) {
                for (int col = 0; col < gameState.Width; ++col) {
                    Location newLocation = new Location(row, col);
                    byte newPosition = newLocation.ToByte();

                    if (this.lastState[newPosition] != null) {
                        State state = GetState(gameState, newLocation);
                        StateAction sa = this.lastState[newPosition];

                        int oldPosition = sa.State.GetPosition();
                        Location oldLocation = new Location(oldPosition / 12, oldPosition % 12);

                        float reward = 0.1f * (gameState.GetDistance(enemyHill, oldLocation) -
                                                gameState.GetDistance(enemyHill, newLocation));

                        this.learn.ProcessReward(reward, sa.State, state, sa.Action, this.alpha, this.gamma);
                    }
                }
            }
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
#if DEBUG
            System.Diagnostics.Debugger.Launch();
            while (!System.Diagnostics.Debugger.IsAttached) { }
#endif

			new Ants().PlayGame(new MyBot(args[0], args[1]));
		}

	}
	
}