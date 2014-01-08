using System;
using System.Collections.Generic;

namespace Ants {

	class MyBot : Bot {
        private string QLearnFile;
        private string gameLogFile;

        private QLearning learn;

        private List<StateAction>[] doneMoves;

        private float alpha;
        private float gamma;
        private float rho;


        public MyBot(string QLearnFile, string gameLogFile) {
            this.QLearnFile = QLearnFile;
            this.gameLogFile = gameLogFile;

            this.learn = new QLearning();

            this.doneMoves = new List<StateAction>[144];

            this.alpha = 0.3f;
            this.gamma = 0.1f;
            this.rho = 0.1f;
        }


		// DoTurn is run once per turn
		public override void DoTurn (IGameState state) {

            CheckRewards(state);

            this.doneMoves = new List<StateAction>[144];


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

                if (this.doneMoves[position] == null) {
                    this.doneMoves[position] = new List<StateAction>();
                }

                StateAction sa = new StateAction();
                sa.State = s;
                sa.Action = a;
                this.doneMoves[position].Add(sa);

				
				// check if we have time left to calculate more orders
				if (state.TimeRemaining < 10) break;
			}
			
		}


        private void CheckRewards(IGameState state) {

            foreach (Location location in state.MyDeads) {
                byte position = location.ToByte();

                List<StateAction> moves = this.doneMoves[position];
                State newState = GetState(state, location);

                foreach (StateAction sa in moves) {
                    this.learn.ProcessReward(-0.1f, sa.State, newState, sa.Action, this.alpha, this.gamma);
                }
            }
        }



        private Location[] statePositions = new Location[] { new Location(1, -1), new Location(1, 0),
                                                            new Location(1, 1), new Location(0, -1),
                                                            new Location(0, 1), new Location(-1, -1),
                                                            new Location(-1, 0), new Location(-1, 1) };

        public State GetState(IGameState state, Location location) {

            QTile[] tiles = new QTile[statePositions.Length];
            for (int i = 0; i < statePositions.Length; ++i) {

                Location loc = state.GetDestination(location, statePositions[i]);
                tiles[i] = state[loc].ToQTile();
            }

            byte position = location.ToByte();

            return new State(tiles, position);
        }


        public bool GameFinished() {
            throw new NotImplementedException();
        }

		
		public static void Main (string[] args) {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
            while (!System.Diagnostics.Debugger.IsAttached) { }
#endif

			new Ants().PlayGame(new MyBot(args[0], args[1]));
		}

	}
	
}