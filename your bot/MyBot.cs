using System;
using System.Collections.Generic;

namespace Ants {

	class MyBot : Bot {
        private string QLearnFile;
        private string gameLogFile;

        private QLearning learn; 


        public MyBot(string QLearnFile, string gameLogFile) {
            this.QLearnFile = QLearnFile;
            this.gameLogFile = gameLogFile;

            this.learn = new QLearning();
        }


		// DoTurn is run once per turn
		public override void DoTurn (IGameState state) {

			// loop through all my ants and try to give them orders
			foreach (Ant ant in state.MyAnts) {

                State s = GetState(state, ant);
                Action a = this.learn.GetAction(s, 0.1f);

                if (a != Action.None) {
                    Direction direction = (Direction)a;

                    this.IssueOrder(ant, direction);
                }
				
				// check if we have time left to calculate more orders
				if (state.TimeRemaining < 10) break;
			}
			
		}


        private Location[] statePositions = new Location[] { new Location(1, -1), new Location(1, 0),
                                                            new Location(1, 1), new Location(0, -1),
                                                            new Location(0, 1), new Location(-1, -1),
                                                            new Location(-1, 0), new Location(-1, 1) };

        public State GetState(IGameState state, Ant ant) {

            QTile[] tiles = new QTile[statePositions.Length];
            for (int i = 0; i < statePositions.Length; ++i) {
                
                Location loc = state.GetDestination(ant, statePositions[i]);
                tiles[i] = state[loc].ToQTile();
            }

            byte position = (byte)(ant.Row * 12 + ant.Col);

            return new State(tiles, position);
        }


        public bool GameFinished() {
            throw new NotImplementedException();
        }

		
		public static void Main (string[] args) {
/*#if DEBUG
            System.Diagnostics.Debugger.Launch();
            while (!System.Diagnostics.Debugger.IsAttached) { }
#endif*/

			new Ants().PlayGame(new MyBot(args[0], args[1]));
		}

	}
	
}