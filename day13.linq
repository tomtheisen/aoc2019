<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

var machine = new IntCodeMachine();
var board = new Dictionary<(int x, int y), int>();

while (true) {
	var x = machine.GetOutput();
	var y = machine.GetOutput();
	var tile = machine.GetOutput();
	if (tile == null) break;
	board[((int)x, (int)y)] = (int)tile;
}

board.Values.Count(t => t == 2).Dump("Total blocks");

board.Clear();
var score = new DumpContainer(0).Dump("Score");
int blocks = 0;
var blocksContainer = new DumpContainer(blocks).Dump("Blocks");

var boardContainer = new DumpContainer().Dump("Board");
void ShowBoard() {
	var height = board.Keys.Max(k => k.y) + 1;
	var width = board.Keys.Max(k => k.x) + 1;
	var result = Enumerable.Range(0, height).Select(_ => new string(' ',width).ToCharArray()).ToArray();
	foreach (var ((x,y),t) in board) result[y][x] = " #=_O"[t];
	boardContainer.Content = string.Join('\n', 
		result.Select(line => string.Concat(line)));
}

int ballx = 0, padx = 0;
machine.Reset();
machine.Poke(0, 2);
machine.NextInput = () => Sign(ballx - padx);

while (true) {
	var x = machine.GetOutput();
	var y = machine.GetOutput();
	var tile = machine.GetOutput();
	if (tile == null) break;

	var pos = ((int)x, (int)y);
	if (x == -1 && y == 0) {
		score.Content = tile;
		continue;
	}
	switch ((int)tile) {
		case 0: {
			if (board.TryGetValue(pos, out var exist) && exist == 2) {
				blocksContainer.Content = blocks -= 1;
			}
			break;
		}
		case 2: {
			if (!board.TryGetValue(pos, out var exist) || exist != 2) {
				blocksContainer.Content = blocks += 1;
			}
			break;
		}
		case 3:
			padx = (int)x;
			break;
		case 4:
			ballx = (int)x;
			break;
	}
	board[pos] = (int)tile;

	if (tile >= 3) {
		ShowBoard();
		Thread.Sleep(50);
	}
}
