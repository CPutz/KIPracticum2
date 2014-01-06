using System;
using System.Collections.Generic;

namespace Ants {
    public enum Action { North, East, South, West, None };

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

            return Action.None;
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


    //1 2 3
    //4   5
    //6 7 8
    class State {
        public uint Value { get; private set; }

        public State(Tile[] tiles) {
            uint mask = 7; //00000000 000 000 000 000 000 000 000 111
                           //          8   7   6   5   4   3   2   1
            this.Value = 0;

            for (int i = 0; i < 8; ++i) {
                this.Value |= ((uint)tiles[i] << (3 * i));
            }
        }
    }
}
