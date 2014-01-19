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

            this.alpha = 0.3f;
            this.gamma = 0.1f;
            this.rho = 0.1f;
        }


		// DoTurn is run once per turn
		public override void DoTurn (IGameState gameState) {

            ProcessRewards(gameState);

            // erase old state data since we start a new turn
            this.lastStates = new List<StateAction>[gameState.Width * gameState.Height];


			// loop through all my ants and try to give them orders
            foreach (Ant ant in gameState.MyAnts) {

                State s = GetState(gameState, ant);
                Action a = this.learn.GetAction(s, this.rho);

                Location newLoc;
                if (a != Action.None) {
                    Direction direction = (Direction)a;

                    this.IssueOrder(ant, direction);

                    newLoc = gameState.GetDestination(ant, direction);
                } else {
                    newLoc = ant;
                }


                //If the move will be blocked, the new location of the ant 
                //is just were the ant is right now.
                if (gameState[newLoc] == Tile.Water) {
                    newLoc = ant;
                }


                short newPosition = newLoc.ToShort(gameState.Width);


                if (lastStates[newPosition] == null)
                    lastStates[newPosition] = new List<StateAction>();

                StateAction sa = new StateAction();
                sa.State = s;
                sa.Action = a;
                this.lastStates[newPosition].Add(sa);


				// check if we have time left to calculate more orders
                if (gameState.TimeRemaining < 10) break;
			}


            this.SaveLastState(gameState);
            this.learn.SaveFile(this.learnFile);
		}


        private void SaveLastState(IGameState gameState) {
            FileStream fs = new FileStream(this.lastStateFile, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            for (int row = 0; row < gameState.Height; ++row) {
                for (int col = 0; col < gameState.Width; ++col) {
                    Location location = new Location(row, col);
                    short position = location.ToShort(gameState.Width);
                    
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
            if (this.lastStates != null) {

                Location enemyHill = new Location(2, 8);

                for (int row = 0; row < gameState.Height; ++row) {
                    for (int col = 0; col < gameState.Width; ++col) {
                        Location newLocation = new Location(row, col);
                        short newPosition = newLocation.ToShort(gameState.Width);

                        if (this.lastStates[newPosition] != null) {
                            State state = GetState(gameState, newLocation);

                            foreach (StateAction sa in this.lastStates[newPosition]) {
                                int oldPosition = sa.State.GetPosition();
                                Location oldLocation = new Location(oldPosition / gameState.Width, oldPosition % gameState.Width);


                                float reward = 0;

                                int d1 = gameState.GetDistance(enemyHill, oldLocation);
                                int d2 = gameState.GetDistance(enemyHill, newLocation);

                                //positive reward for going more towards the enemy hill and
                                //negative reward for going away from the enemy hill
                                //if (d2 != 0)
                                //    reward += 3.0f * (float)(d1 - d2) / (d2);

                                //give reward for getting food (more food => higher reward)
                                reward += 0.5f * NumOfFoodNextTo(gameState, newLocation);

                                //negative reward for having more than one ant walking to the same location,
                                if (this.lastStates[newPosition].Count >= 2) {

                                    //but do not give that penalty when the ant did not move that turn
                                    //(because then that ant did nothing wrong).
                                    if (sa.Action != Action.None) {
                                        reward += -3.0f;
                                    }
                                }

                                this.learn.ProcessReward(reward, sa.State, state, sa.Action, this.alpha, this.gamma);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets the number of food there is next to a certain location.
        /// </summary>
        /// <param name="gameState">The GameState.</param>
        /// <param name="location">The Location to check.</param>
        /// <returns>The number of food next to <paramref name="location"/>.</returns>
        private int NumOfFoodNextTo(IGameState gameState, Location location) {
            int res = 0;

            foreach (Direction direction in Enum.GetValues(typeof(Direction))) {
                if (gameState[gameState.GetDestination(location, direction)] == Tile.Food) {
                    res++;
                }
            }

            return res;
        }



        private Location[] statePositions = new Location[] { new Location(1, -1), new Location(1, 0),
                                                            new Location(1, 1), new Location(0, -1),
                                                            new Location(0, 1), new Location(-1, -1),
                                                            new Location(-1, 0), new Location(-1, 1),
                                                            new Location(2, 0), new Location(0, 2),
                                                            new Location(-2, 0), new Location(0, -2) };

        /// <summary>
        /// Gets a State object representing the current state.
        /// </summary>
        /// <param name="state">The GameState.</param>
        /// <param name="location">The Location from which to generate a state.</param>
        /// <returns>A State object that repressents the current state given the location.</returns>
        public State GetState(IGameState gameState, Location location) {

            QTile[] tiles = new QTile[statePositions.Length];
            for (int i = 0; i < statePositions.Length; ++i) {

                Location loc = gameState.GetDestination(location, statePositions[i]);
                tiles[i] = gameState[loc].ToQTile();
            }

            short position = location.ToShort(gameState.Width);

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