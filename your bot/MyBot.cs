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

                State s = GetState(ant);
                Action a = this.learn.GetAction(s, 0.1f);

                if (a != Action.None) {
                    Direction direction = (Direction)a;

                    this.IssueOrder(ant, direction);
                }
				
				// check if we have time left to calculate more orders
				if (state.TimeRemaining < 10) break;
			}
			
		}


        public State GetState(Ant ant) {
            throw new NotImplementedException();
        }


        public bool GameFinished() {
            throw new NotImplementedException();
        }

		
		public static void Main (string[] args) {
			new Ants().PlayGame(new MyBot(args[0], args[1]));
		}

	}
	
}