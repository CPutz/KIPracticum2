using System;
using System.Collections.Generic;
using System.IO;

namespace Ants {

	class MyBot : Bot {
        private string learnFile;
        private string lastStateFile;

        private QLearning learn;

        private List<StateAction>[] lastStates;

        private float alpha;
        private float gamma;
        private float rho;


        public MyBot(string learnFile, string lastStateFile) {
            this.learnFile = learnFile;
            this.lastStateFile = lastStateFile;

            this.learn = new QLearning();
            this.learn.LoadFile(this.learnFile);

            this.lastStates = new List<StateAction>[144];

            this.alpha = 0.3f;
            this.gamma = 0.1f;
            this.rho = 0.1f;
        }


		// DoTurn is run once per turn
		public override void DoTurn (IGameState state) {

            ProcessRewards(state);

            // erase old state data since we start a new turn
            this.lastStates = new List<StateAction>[144];


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


                byte newPosition = newLoc.ToByte();


                if (lastStates[newPosition] == null)
                    lastStates[newPosition] = new List<StateAction>();

                StateAction sa = new StateAction();
                sa.State = s;
                sa.Action = a;
                this.lastStates[newPosition].Add(sa);


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
                    
                    if (this.lastStates[position] != null) {
                        State state = GetState(gameState, location);

                        foreach (StateAction sa in this.lastStates[position]) {
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


        private void ProcessRewards(IGameState gameState) {
            Location enemyHill = new Location(2, 8);

            for (int row = 0; row < gameState.Height; ++row) {
                for (int col = 0; col < gameState.Width; ++col) {
                    Location newLocation = new Location(row, col);
                    byte newPosition = newLocation.ToByte();

                    if (this.lastStates[newPosition] != null) {
                        State state = GetState(gameState, newLocation);

                        foreach (StateAction sa in this.lastStates[newPosition]) {
                            int oldPosition = sa.State.GetPosition();
                            Location oldLocation = new Location(oldPosition / 12, oldPosition % 12);


                            if (newPosition == oldPosition && sa.Action != Action.None) {
                                int test = 2;
                                test *= 5000;
                            }


                            float reward = 0;

                            //positive reward for going more towards the enemy hill and
                            //negative reward for going away from the enemy hill
                            reward += 0.1f * (gameState.GetDistance(enemyHill, oldLocation) -
                                                    gameState.GetDistance(enemyHill, newLocation));

                            //negative reward for having more than one ant walking to the same location,
                            if (this.lastStates[newPosition].Count >= 2) {

                                //but do not give that penalty when the ant did not move that turn
                                //(because then that ant did nothing wrong).
                                if (sa.Action != Action.None) {
                                    reward += -0.5f;
                                }
                            }

                            this.learn.ProcessReward(reward, sa.State, state, sa.Action, this.alpha, this.gamma);
                        }
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