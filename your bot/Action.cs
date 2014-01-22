using System;

namespace Ants {

    public enum Action { 
        North, 
        South, 
        East, 
        West, 
        None 
    }

    public static class ActionExtensions {

        public static Direction ToDirection(this Action self) {
            switch (self) {
                case Action.East:
                    return Direction.East;

                case Action.North:
                    return Direction.North;

                case Action.South:
                    return Direction.South;

                case Action.West:
                    return Direction.West;

                case Action.None:
                    throw new ArgumentException("Action.None cannot be converted to a Direction");

                default:
                    throw new ArgumentException("Unknown Action", "self");
            }
        }
    }
}
