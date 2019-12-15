<Query Kind="Statements">
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

var machine = new IntCodeMachine();
BigInteger Run(long noun, long verb) {
	machine.Poke(1, noun);
	machine.Poke(2, verb);
	machine.Run();
	return machine.Peek(0);
}

Console.WriteLine(Run(12, 2));

for (int noun = 0; noun < 100; noun++) {
	for (int verb = 0; verb < 100; verb++) {
		machine.Reset();
		var mem = Run(noun, verb);
		if (mem == 19690720) Console.WriteLine(100 * noun + verb);
	}
}