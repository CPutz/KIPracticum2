using System;
using System.Collections.Generic;

namespace YourBot {
    class QLearning {
        QValueSet store;
        Random random;

        public QLearning() {
            this.store = new QValueSet();
            this.random = new Random();
        }

        public Action GetAction(State state, Problem problem, float alpha, float gamma, float rho) {

            List<Action> actions = problem.GetAvalaibleActions(state);

            if (this.random.NextDouble() < rho)
                return PickRandomAction(actions);
            else
                return this.store.GetBestAction(state);
        }

        public Action PickRandomAction(List<Action> actions) {
            if (actions.Count > 0)
                return actions[this.random.Next(actions.Count)];

            return null;
        }
    }

    class Problem {

        public List<Action> GetAvalaibleActions(State s) {
            throw new NotImplementedException();
        }
    }

    class QValueSet {

        public float this[State s, Action a] {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }


        public Action GetBestAction(State s) {
            throw new NotImplementedException();
        }
    }

    class Action {

    }

    class State {

    }
}
