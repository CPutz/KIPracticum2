using System;
using System.Collections.Generic;
using System.IO;

namespace Ants {

    class QLearning {
        QValueSet store;
        Random random;


        public QLearning() {
            this.store = new QValueSet();
            this.random = new Random();
        }


        /// <summary>
        /// Gets an action that is possible in a particular state.
        /// </summary>
        /// <param name="state">The state to choose an action in.</param>
        /// <param name="rho">The probability of choosing a random action.</param>
        /// <returns>the best possible action in State <paramref name="state"/> with probability 
        /// <paramref name="rho"/> to return a random action.</returns>
        public Action GetAction(State state, float rho) {

            List<Action> actions = new List<Action> { Action.North, Action.East, Action.South, Action.West, Action.None };

            if (this.random.NextDouble() < rho)
                return PickRandomAction(actions);
            else
                return this.store.GetBestAction(state);
        }


        /// <summary>
        /// Porcesses a given reward.
        /// </summary>
        /// <param name="reward">The reward that was earned by taking the action.</param>
        /// <param name="oldState">The state before the <paramref name="action"/> was taken.</param>
        /// <param name="newState">The state we ended up in after taking the <paramref name="action"/>.</param>
        /// <param name="action">The action that was taken.</param>
        /// <param name="alpha">The Learning rate.</param>
        /// <param name="gamma">The Discount rate.</param>
        public void ProcessReward(float reward, State oldState, State newState, Action action, float alpha, float gamma) {

            float Q = store[oldState, action];
            float maxQ = store[newState, store.GetBestAction(newState)];

            Q = (1 - alpha) * Q + alpha * (reward + gamma * maxQ);

            store[oldState, action] = Q;
        }


        /// <summary>
        /// Picks a random action from a list of acitons.
        /// </summary>
        /// <param name="actions">The list to choose an actions from.</param>
        /// <returns>An action from the list <paramref name="actions"/>.</returns>
        private Action PickRandomAction(List<Action> actions) {
            if (actions.Count > 0)
                return actions[this.random.Next(actions.Count)];

            return Action.None;
        }


        //file format:
        //1 int (state)
        //5 floats (5 Q-values for the actions)

        /// <summary>
        /// Loads a Q-learning file containing Q-values.
        /// </summary>
        /// <param name="filename">The path of the Q-learning file.</param>
        public void LoadFile(string filename) {


            FileStream fs = File.Open(filename, FileMode.OpenOrCreate);
            BinaryReader br = new BinaryReader(fs);


            while (fs.Position < fs.Length) {
                State state = new State(br.ReadUInt64());

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


        //file format:
        //1 int (state)
        //5 floats (5 Q-values for the actions)

        /// <summary>
        /// Saves the Q-values stored in store in a file.
        /// </summary>
        /// <param name="filename">The path of the Q-learning file.</param>
        public void SaveFile(string filename) {
            FileStream fs = File.Open(filename, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            Dictionary<State, QSetItem> set = this.store.Set;

            foreach (State state in set.Keys) {
                QSetItem item = set[state];
                
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


    class StateAction {
        public State State { get; set; }
        public Action Action { get; set; }
    }


    /// <summary>
    /// A set that contains Q-values.
    /// It couples a State and QSetItem.
    /// The QSetItems contain the Q-values associated with each Action.
    /// </summary>
    class QValueSet {

        public Dictionary<State, QSetItem> Set { get; private set; }

        private Random random;


        public QValueSet() {
            this.Set = new Dictionary<State, QSetItem>();
            this.random = new Random();
        }

        public float this[State s, Action a] {
            get {
                if (this.Set.ContainsKey(s))
                    return this.Set[s][a];
                return 0;
            }
            set {
                if (this.Set.ContainsKey(s)) {
                    this.Set[s][a] = value;
                } else {
                    QSetItem newItem = new QSetItem();
                    newItem[a] = value;
                    this.Set.Add(s, newItem);
                }

            }
        }


        public void Add(State s, QSetItem item) {
            this.Set.Add(s, item);
        }


        /// <summary>
        /// Gets the best possible action in a certain state.
        /// </summary>
        /// <param name="s">The state to choose an action in.</param>
        /// <returns>Returns the action with the highest Q-value. If all values are zero then
        /// a random value will be returned.</returns>
        public Action GetBestAction(State s) {
            float maxQ = float.MinValue;
            Action result = default(Action);
            List<Action> bestActions = new List<Action>();

            if (this.Set.ContainsKey(s)) {
                foreach (Action a in Enum.GetValues(typeof(Action))) {
                    float Q = this.Set[s][a];

                    if (Q > maxQ) {
                        bestActions = new List<Action>();
                        maxQ = Q;
                        bestActions.Add(a);
                    } else if (Q == maxQ) {
                        bestActions.Add(a);
                    }
                }

                //pick random action from list of best actions
                result = bestActions[random.Next(bestActions.Count)];
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



    // The information stored about each tile in a state
    public enum QTile { Ant, Food, None };



    //The positions around an ant x that are stored in a state.
    //      9
    //    1 2 3
    // 12 4 x 5 10
    //    6 7 8
    //      11

    struct State {
        public ulong Value { get; private set; }

        public State(QTile[] tiles, ushort position)
            : this() {
            
            this.Value = 0;

            //The first 16 bits are used to store a position value (between 0 and 143)
            this.Value |= (ulong)position;

            //The next 24 bits are used to store 12 2-bit values representing
            //the QTile value in each of the 12 tiles around the position.
            for (int i = 0; i < tiles.Length; ++i) {
                this.Value |= ((ulong)tiles[i] << (2 * i + 16));
            }
        }

        public State(ulong key)
            : this() {
            
            this.Value = key;
        }

        public short GetPosition() {
            //pick first 16 bits from value (that cantains the position)
            return (short)(this.Value & 0x000000000000FFFF);
        }
    }
}
