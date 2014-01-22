using System;
using System.Collections.Generic;
using System.IO;

namespace Ants {

	class MyBot : Bot {
        private string learnFile;
        private string lastStateFile;

        private QLearning learn;

        //for every position on the map, it holds a List that
        //contains from what state that ant came by doing which action
        private List<StateAction>[] lastStates;

        private float alpha;
        private float gamma;
        private float rho;

        
        public MyBot(string learnFile, string lastStateFile, float alpha, float gamma, float rho) {
            this.learnFile = learnFile;
            this.lastStateFile = lastStateFile;
            
            this.learn = new QLearning();
            this.learn.LoadFile(this.learnFile);

            this.alpha = alpha;
            this.gamma = gamma;
            this.rho = rho;
        }


		// DoTurn is run once per turn
		public override void DoTurn (IGameState gameState) {

            ProcessRewards(gameState);

            // erase old state data since we start a new turn
            this.lastStates = new List<StateAction>[gameState.Width * gameState.Height];


			// loop through all my ants and give them orders
            foreach (Ant ant in gameState.MyAnts) {

                State s = GetState(gameState, ant);
                Action a = this.learn.GetAction(s, this.rho);

                Location newLoc;
                if (a != Action.None) {
                    Direction direction = a.ToDirection();

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


                ushort newPosition = newLoc.ToUShort(gameState.Width);

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


        /// <summary>
        /// Saves the last state to a file.
        /// </summary>
        /// <param name="gameState">The GameState.</param>
        private void SaveLastState(IGameState gameState) {
            FileStream fs = new FileStream(this.lastStateFile, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            for (int row = 0; row < gameState.Height; ++row) {
                for (int col = 0; col < gameState.Width; ++col) {
                    Location location = new Location(row, col);
                    ushort position = location.ToUShort(gameState.Width);
                    
                    if (this.lastStates[position] != null) {
                        State state = GetState(gameState, location);

                        //file format:
                        //ulong (old state)
                        //byte (action)
                        //ulong (new state)

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


        /// <summary>
        /// Uses the lastStates to process the rewards from moves taken in the last turn.
        /// </summary>
        /// <param name="gameState">The GameState.</param>
        private void ProcessRewards(IGameState gameState) {

            //make sure the lastStates array is initialized, because it is not in the first turn.
            //(we cannot initialize it at the start of the turn because we need it then, and we cannot
            //initialize it before the first turn, because then we do not know the Width and Height of the map)
            if (this.lastStates != null) {

                for (int row = 0; row < gameState.Height; ++row) {
                    for (int col = 0; col < gameState.Width; ++col) {
                        
                        Location newLocation = new Location(row, col);
                        ushort newPosition = newLocation.ToUShort(gameState.Width);

                        if (this.lastStates[newPosition] != null) {
                            State state = GetState(gameState, newLocation);

                            foreach (StateAction sa in this.lastStates[newPosition]) {
                                int oldPosition = sa.State.GetPosition();
                                Location oldLocation = new Location(oldPosition / gameState.Width, oldPosition % gameState.Width);


                                float reward = GetReward(gameState, oldLocation, sa.Action, newLocation);

                                this.learn.ProcessReward(reward, sa.State, state, sa.Action, this.alpha, this.gamma);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets the reward by going from <paramref name="oldLocation"/> to <paramref name="newLocation"/>
        /// by doing some action <paramref name="a"/>.
        /// </summary>
        /// <param name="gameState">The GameState.</param>
        /// <param name="oldLocation">The location before taking the action.</param>
        /// <param name="a">The action that was taken.</param>
        /// <param name="newLocation">The location after taking the action.</param>
        /// <returns>The reward by doing action <paramref name="a"/> from Location 
        /// <paramref name="oldLocation"/> to get to Location <paramref name="newLocation"/>.</returns>
        private float GetReward(IGameState gameState, Location oldLocation, Action a, Location newLocation) {
            
            float reward = 0;
            
            //positive reward for going more towards the enemy hill and
            //negative reward for going away from the enemy hill
            /*Location enemyHill = new Location(2, 8);
             int d1 = gameState.GetDistance(enemyHill, oldLocation);
             int d2 = gameState.GetDistance(enemyHill, newLocation);
             reward += 1.0f * (d1 - d2);
             */


            //positive reward for going away from our hill and
            //negative reward for going more towards our own hill
            Location ownHill = new Location(3, 9);
            int d1 = gameState.GetDistance(ownHill, oldLocation);
            int d2 = gameState.GetDistance(ownHill, newLocation);
            reward += 1.0f * (d2 - d1);
            

            //give reward for getting food (more food => higher reward)
            /*reward += 0.5f * NumOfFoodNextTo(gameState, newLocation);
             */


            //negative reward for having more than one ant walking to the same location,
            /*if (this.lastStates[newLocation.ToUShort(gameState.Width)].Count >= 2) {
            
                //but do not give that penalty when the ant did not move that turn
                //(because then that ant did nothing wrong).
                if (a != Action.None) {
                    reward += -3.0f;
                }
            }
             */


            return reward;
        }


        /// <summary>
        /// Gets the number of food there was next to a certain location in the previous turn.
        /// </summary>
        /// <param name="gameState">The GameState.</param>
        /// <param name="location">The Location to check.</param>
        /// <returns>The number of food next to <paramref name="location"/> in the previous turn.</returns>
        private int NumOfFoodNextTo(IGameState gameState, Location location) {
            int res = 0;

            //in every direction, check whether there was food in the previous turn
            foreach (Direction direction in Enum.GetValues(typeof(Direction))) {
                Location checkLocation = gameState.GetDestination(location, direction);
                
                if (gameState.OldMap[checkLocation.Row, checkLocation.Col] == Tile.Food) {
                    res++;
                }
            }

            return res;
        }


        //Positions that are saved around an ant x:
        //      9
        //    1 2 3
        // 12 4 x 5 10
        //    6 7 8
        //      11
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

            ushort position = location.ToUShort(gameState.Width);

            return new State(tiles, position);
        }




        public static void Main(string[] args) {
/*#if DEBUG
            System.Diagnostics.Debugger.Launch();
            while (!System.Diagnostics.Debugger.IsAttached) { }
#endif*/

			new Ants().PlayGame(new MyBot(args[0], args[1], 
                float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4])));
		}

	}
	
}