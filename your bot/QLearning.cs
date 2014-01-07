using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace Ants {
    public enum Action { North = 0, East = 1, South = 2, West = 3, None = 4};

    class QLearning {
        QValueSet store;
        Random random;

        public QLearning() {
            this.store = new QValueSet();
            this.random = new Random();
        }

        public Action GetAction(State state, Problem problem, float alpha, float gamma, float rho) {

            //List<Action> actions = problem.GetAvalaibleActions(state);
            List<Action> actions = new List<Action> { Action.North, Action.East, Action.South, Action.West, Action.None };

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


        public void LoadFile(string s) {
            FileStream fs = File.Open(s, FileMode.Open);
            StreamReader sr = new StreamReader(fs);


            State state = new State((uint)sr.Read());

            QSetItem newItem = new QSetItem();
            foreach (Action a in Enum.GetValues(typeof(Action))) {
                newItem[a] = (float)sr.Read();
            }

            this.store.Add(state, newItem);
        }


        public void SaveFile(string s) {
            FileStream fs = File.Open(s, FileMode.CreateNew);
            StreamWriter sw = new StreamWriter(fs);

            //foreach (QSetItem q
        }
    }

    class Problem {
        public List<Action> GetAvalaibleActions(State s) {
            throw new NotImplementedException();
        }
    }

    class QValueSet {

        private Hashtable set;

        public QValueSet() {
            //this.set = new HashSet<int>();
            this.set = new Hashtable();
        }

        public float this[State s, Action a] {
            get {
                if (this.set.ContainsKey(s))
                    return ((QSetItem)this.set[s])[a];
                return 0;
            }
            set {
                if (this.set.ContainsKey(s)) {
                    ((QSetItem)this.set[s])[a] = value;
                } else {
                    QSetItem newItem = new QSetItem();
                    newItem[a] = value;
                    this.set.Add(s, newItem);
                }

            }
        }


        public void Add(State s, QSetItem item) {
            this.set.Add(s, item);
        }


        public Action GetBestAction(State s) {
            float maxQ = float.MinValue;
            Action result = default(Action);

            foreach (Action a in Enum.GetValues(typeof(Action))) {
                float Q = ((QSetItem)this.set[s])[a];
                if (Q > maxQ) {
                    maxQ = Q;
                    result = a;
                }
            }

            return result;
        }
    }

    class QSetItem {
        private float[] values;

        public QSetItem() {
            this.values = new float[5]; //5 actions
        }

        public float this[Action a] {
            get {
                return this.values[(int)a];
            }
            set {
                this.values[(int)a] = value;
            }
        }
    }


    //1 2 3
    //4   5
    //6 7 8
    struct State {
        public uint Value { get; private set; }

        public State(Tile[] tiles) : this() {
            this.Value = 0;

            for (int i = 0; i < 8; ++i) {
                this.Value |= ((uint)tiles[i] << (3 * i));
            }
        }

        public State(uint u) : this() {
            this.Value = u;
        }
    }
}
