<Query Kind="Program">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"

struct StateKey : IEquatable<StateKey> {
	public Point p0;
	public Point p1;
	public Point p2;
	public Point p3;
	public int keybits;

	public override int GetHashCode() => Hash(p0, p1, p2, p3, keybits);

	public bool Equals(StateKey other) => keybits == other.keybits
		&& p0.Equals(other.p0) && p1.Equals(other.p1) && p2.Equals(other.p2) && p3.Equals(other.p3);
}

struct State {
	public Point p0;
	public Point p1;
	public Point p2;
	public Point p3;
	public int active;
	
    public int lastkeybits;
	public int keybits;
	public int moves;
	
	public Point this[int i]{
		get => i switch { 0 => p0, 1 => p1, 2 => p2, 3 => p3 };
		set {
			switch (i) {
				case 0: p0 = value; break;
				case 1: p1 = value; break;
				case 2: p2 = value; break;
				case 3: p3 = value; break;
			}
		}
	}

    public bool HasKey(char c) => (this.keybits >> (char.ToLower(c) - 'a') & 1) == 1;
}

class NeptuneMultiVault : BreadthFirst<State, StateKey> {
	private Plane<char> Board;
	private int KeybitsTarget;
	
	public NeptuneMultiVault(Plane<char> board) {
		Board = board;
		KeybitsTarget = (1 << Board.Values.Count(c => c >='a' && c <= 'z')) - 1;
		SealDeadEnds();
		var start = Board.Find(c => c == '@').Single();
		Board[start] = '#';
		foreach (var n in start.Neighbors) Board[n] = '#';

		var startState = new State {
			p0 = start.Up.Left,
			p1 = start.Up.Right,
			p2 = start.Down.Left,
			p3 = start.Down.Right,
		};
		for (int i = 0; i < 4; i++) {
			startState.active = i;
			Frontier.Enqueue(startState);
		}
	}

	private void SealDeadEnds() {
		while (true) {
			bool didwork = false;
			for (int x = 1; x < Board.Width - 1; x++) {
				for (int y = 1; y < Board.Height - 1; y++) {
					var pt = new Point(x, y);

					bool dead = (Board[pt] == '.' || Board[pt] >= 'A' && Board[pt] <= 'Z')
						&& pt.Neighbors.Select(p => Board[p]).Count(c => c == '#') >= 3;
					if (dead) Board[pt] = '#';
					didwork |= dead;
				}
			}
			if (!didwork) break;
		}
	}

	protected override StateKey GetKey(State state) => new StateKey {
		keybits = state.keybits,
		p0 = state.p0,
		p1 = state.p1,
		p2 = state.p2,
		p3 = state.p3,
	};

	protected override bool IsGoal(State state) => state.keybits == KeybitsTarget;

	protected override IEnumerable<State> NextStates(State state) {
		var ch = Board[state[state.active]];
		for (int i = 0; i < 4; i++) {
			if (i != state.active && state.lastkeybits == state.keybits) continue;
			foreach (var n in state[i].Neighbors) {
				char tch = Board[n];
				if (tch == '#') continue;
				if (tch >= 'A' && tch <= 'Z' && !state.HasKey(tch)) continue;

				State next = state;
				next.lastkeybits = state.keybits;
				next.active = i;
				next[i] = n;
				next.moves += 1;
				if (tch >= 'a' && tch <= 'z' && !state.HasKey(tch))
					next.keybits |= 1 << tch - 'a';
				yield return next;
			}
		}
	}
}

void Main() {
	var maze = new NeptuneMultiVault(GetAocCharPlane())
		//.Dump()
		;
	maze.Search().Dump(1);
}