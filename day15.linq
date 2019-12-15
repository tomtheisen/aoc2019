<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

var steps = new Dictionary<(int x, int y), int> {[(0, 0)] = 0};
var machine = new IntCodeMachine();
var plane = new Plane<char>(' ') { [0, 0] = 'D' };

var boardContainer = new DumpContainer(plane).Dump();
int x = 0, y = 0;
int dir = 1; // north (1), south (2), west (3), and east (4)

for (int stepsSinceDiscovery = 0; stepsSinceDiscovery < 5_000; stepsSinceDiscovery++) {
	int input = dir;
	machine.TakeInput(input);
	int lastSteps = steps[(x, y)];
	switch (machine.GetOutput()!) {
		case 0:
			switch (input) {
				case 1: plane[x, y - 1] = '#'; break;
				case 2: plane[x, y + 1] = '#'; break;
				case 3: plane[x - 1, y] = '#'; break;
				case 4: plane[x + 1, y] = '#'; break;
			}
			dir = dir switch { 1 => 4, 4 => 2, 2 => 3, 3 => 1 };
			break;
		case 1:
			if (plane[x, y] == ' ') stepsSinceDiscovery = 0;
			if (plane[x, y] == 'D') plane[x, y] = '.';
			switch (input) {
				case 1: plane[x, --y] = 'D'; break;
				case 2: plane[x, ++y] = 'D'; break;
				case 3: plane[--x, y] = 'D'; break;
				case 4: plane[++x, y] = 'D'; break;
			}
			if (!steps.ContainsKey((x, y))) steps[(x, y)] = lastSteps + 1;
			dir = dir switch { 1 => 3, 3 => 2, 2 => 4, 4 => 1 };
			break;
		case 2:
			plane[x, y] = '.';
			switch (input) {
				case 1: plane[x, --y] = 'O'; break;
				case 2: plane[x, ++y] = 'O'; break;
				case 3: plane[--x, y] = 'O'; break;
				case 4: plane[++x, y] = 'O'; break;
			}
			if (!steps.ContainsKey((x, y))) {
				steps[(x, y)] = lastSteps + 1;
				WriteLine($"Oxygen {(x, y)} steps {lastSteps + 1}");
			}
			break;
	}
	boardContainer.Refresh();
	Thread.Sleep(1);
}

plane[x, y] = '.';
boardContainer.Refresh();

// part 2 is done in a stax program lol
