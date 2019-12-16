<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

string input = string.Concat(
	Enumerable.Repeat(GetAocLines()[0], 10_000));

int[] coefficients = {0,1,0,-1};

var signal = input.Select(c => (int)(c - '0')).ToArray();
int[] newSignal = new int[signal.Length];

for (int phase = 0; phase < 100; phase++) {
	Console.WriteLine(phase);
	for (int i = 0; i < signal.Length; i++) {
		int e = 0;
		for (int j = 0; j < signal.Length; j++) {
			e += coefficients[-~j / -~i & 3] * signal[j];
		}
		newSignal[i] = Abs(e % 10);
	}
	(signal, newSignal) = (newSignal, signal);
}

var message = signal
	.Skip(int.Parse(input.Substring(0,7)))
	.Take(8);
string.Concat(message).Dump();