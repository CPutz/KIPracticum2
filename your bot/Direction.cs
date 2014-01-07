using System;

namespace Ants {

	public enum Direction {
		North = 0,
		South = 1,
		East = 2,
		West = 3
	}

	public static class DirectionExtensions {

		public static char ToChar (this Direction self) {
			switch (self)
			{
				case Direction.East:
					return 'e';

				case Direction.North:
					return 'n';

				case Direction.South:
					return 's';

				case Direction.West:
					return 'w';

				default:
					throw new ArgumentException ("Unknown direction", "self");
			}
		}
	}
}