<Query Kind="Statements">
  <Namespace>static System.Console</Namespace>
  <Namespace>static System.Math</Namespace>
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

string input = GetAocLines()[0];

int[] coefficients = {0,1,0,-1};

var signal = input.Select(c => c - '0').ToArray();

for (int phase = 0; phase < 100; phase++) {
	int[] newSignal = new int[signal.Length];
	for (int i = 0; i < signal.Length; i++) {
		int e = 0;
		for (int j = 0; j < signal.Length; j++) {
			int coefficient = coefficients[-~j / -~i % 4];
			e += coefficient * signal[j];
		}
		newSignal[i] = Abs(e % 10);
	}
	signal = newSignal;
}

string.Concat(signal.Take(8)).Dump();