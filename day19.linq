<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

int affected = 0, tx = 0, ty = 0;
var machine = new IntCodeMachine();
var plane = new Plane<char>('.');

bool InTractor(int x, int y) {
	if (x < 0 || y < 0) return false;
	machine.Reset();
	machine.TakeInput(x, y);
	return machine.GetOutput() == 1;
}

for (int y = 0; y < 50; y++) {
	for (int x = 0; x < 50; x++) {
		if (InTractor(x, y)) {
			(tx, ty) = (x, y);
			plane[x, y] = '#';
			affected += 1;
		}
	}
}
plane.Dump();
WriteLine(affected);

for (; !InTractor(tx + 99, ty - 99); )
for (tx++; InTractor(tx, ty + 1); ty++);
WriteLine(tx * 1e4 + (ty - 99));
