using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace Ants {
    public enum Action { North = 0, South = 1, East = 2, West = 3, None = 4 };

    class QLearning {
        QValueSet store;
        Random random;



        public QLearning() {
            this.store = new QValueSet();
            this.random = new Random();
        }

        public Action GetAction(State state, float rho) {

            //List<Action> actions = problem.GetAvalaibleActions(state);
            List<Action> actions = new List<Action> { Action.North, Action.East, Action.South, Action.West, Action.None };

            if (this.random.NextDouble() < rho)
                return PickRandomAction(actions);
            else
                return this.store.GetBestAction(state);
        }


        public void ProcessReward(float reward, State oldState, State newState, Action action, float alpha, float gamma) {

            float Q = store[oldState, action];
            float maxQ = store[newState, store.GetBestAction(newState)];

            Q = (1 - alpha) * Q + alpha * (reward + gamma * maxQ);

            store[oldState, action] = Q;
        }


        private Action PickRandomAction(List<Action> actions) {
            if (actions.Count > 0)
                return actions[this.random.Next(actions.Count)];

            return Action.None;
        }


        public void LoadFile(string s) {
            FileStream fs = File.Open(s, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);


            while (fs.Position < fs.Length) {
                State state = new State(br.ReadInt32());

                QSetItem newItem = new QSetItem();
                newItem[Action.North] = br.ReadSingle();
                newItem[Action.South] = br.ReadSingle();
                newItem[Action.East] = br.ReadSingle();
                newItem[Action.West] = br.ReadSingle();
                newItem[Action.None] = br.ReadSingle();

                this.store.Add(state, newItem);
            }

            br.Close();
            fs.Close();
        }


        public void SaveFile(string s) {
            FileStream fs = File.Open(s, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            Hashtable set = this.store.set;

            foreach (State state in set.Keys) {
                QSetItem item = (QSetItem)set[state];
                
                bw.Write(state.Value);

                bw.Write(item[Action.North]);
                bw.Write(item[Action.South]);
                bw.Write(item[Action.East]);
                bw.Write(item[Action.West]);
                bw.Write(item[Action.None]);
            }

            bw.Close();
            fs.Close();
        }
    }


    struct StateAction {
        public State State { get; set; }
        public Action Action { get; set; }
    }


    class Problem {
        public List<Action> GetAvalaibleActions(State s) {
            throw new NotImplementedException();
        }
    }

    class QValueSet {

        //dont want to make this public but is better for saving the file...
        public Hashtable set;

        Random random;

        public QValueSet() {
            this.set = new Hashtable();
            this.random = new Random();
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

            if (this.set.ContainsKey(s)) {
                foreach (Action a in Enum.GetValues(typeof(Action))) {
                    float Q = ((QSetItem)this.set[s])[a];
                    if (Q > maxQ) {
                        maxQ = Q;
                        result = a;
                    }
                }
            } else {
                result = (Action)random.Next(5);
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


    public enum QTile { Ant, Enemy, Food, None };

    //     9
    //   1 2 3
    //12 4   5 10
    //   6 7 8
    //     11
    struct State {
        public int Value { get; private set; }

        public State(QTile[] tiles, byte position)
            : this() {
            
            this.Value = 0;

            for (int i = 0; i < tiles.Length; ++i) {
                this.Value |= ((int)tiles[i] << (2 * i));
            }

            this.Value |= position << 24;
        }

        public State(int key)
            : this() {
            
            this.Value = key;
        }

        public int GetPosition() {
            return (int)(this.Value & 4278190080) >> 24;
        }
    }
}
